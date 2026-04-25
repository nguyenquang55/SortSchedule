using SortSchedule.Application.Abstractions;
using SortSchedule.Application.Options;
using SortSchedule.Domain.Common;
using SortSchedule.Domain.Entities;

namespace SortSchedule.Application.Services;

public sealed class TabuSearchSolver : IScheduleSolver
{
    private readonly IScheduleScorer _scorer;
    private readonly TabuSearchOptions _options;
    private readonly Random _random;

    public TabuSearchSolver(IScheduleScorer scorer, TabuSearchOptions options)
    {
        _scorer = scorer;
        _options = options;
        _options.TabuTenure = Math.Max(1, _options.TabuTenure);
        _options.MaxIterations = Math.Max(1, _options.MaxIterations);
        _options.NeighborhoodSize = Math.Max(1, _options.NeighborhoodSize);
        _random = _options.RandomSeed.HasValue ? new Random(_options.RandomSeed.Value) : new Random();
    }

    public Schedule Solve(Schedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        var current = schedule.DeepClone();
        current.Score = _scorer.CalculateScore(current);

        var best = current.DeepClone();

        if (current.Lessons.Count == 0 || current.Rooms.Count == 0 || current.TimeSlots.Count == 0)
        {
            return best;
        }

        var tabuQueue = new Queue<string>();
        var tabuSet = new HashSet<string>(StringComparer.Ordinal);

        for (var iteration = 0; iteration < _options.MaxIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var neighbor = FindBestNeighbor(current, best.Score, tabuSet, out var bestMoveKey);
            if (neighbor is null)
            {
                break;
            }

            current = neighbor;

            if (current.Score > best.Score)
            {
                best = current.DeepClone();
            }

            if (!string.IsNullOrWhiteSpace(bestMoveKey))
            {
                tabuQueue.Enqueue(bestMoveKey);
                tabuSet.Add(bestMoveKey);

                if (tabuQueue.Count > _options.TabuTenure)
                {
                    var expired = tabuQueue.Dequeue();
                    tabuSet.Remove(expired);
                }
            }
        }

        return best;
    }

    private Schedule? FindBestNeighbor(
        Schedule current,
        HardSoftScore globalBest,
        HashSet<string> tabuSet,
        out string? selectedMoveKey)
    {
        selectedMoveKey = null;

        Schedule? bestNeighbor = null;
        var bestNeighborScore = new HardSoftScore(int.MinValue, int.MinValue);

        foreach (var move in GenerateMoves(current))
        {
            var moveKey = move.ToTabuKey();

            var candidate = current.DeepClone();
            ApplyMove(candidate, move);
            candidate.Score = _scorer.CalculateScore(candidate);

            var isTabu = tabuSet.Contains(moveKey);
            var meetsAspiration = candidate.Score > globalBest;
            if (isTabu && !meetsAspiration)
            {
                continue;
            }

            if (bestNeighbor is null || candidate.Score > bestNeighborScore)
            {
                bestNeighbor = candidate;
                bestNeighborScore = candidate.Score;
                selectedMoveKey = moveKey;
            }
        }

        return bestNeighbor;
    }

    private IEnumerable<ScheduleMove> GenerateMoves(Schedule schedule)
    {
        var lessons = schedule.Lessons.ToArray();
        var rooms = schedule.Rooms.ToArray();
        var timeSlots = schedule.TimeSlots.ToArray();

        if (lessons.Length == 0 || rooms.Length == 0 || timeSlots.Length == 0)
        {
            yield break;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var attempts = 0;
        var generated = 0;
        var maxAttempts = _options.NeighborhoodSize * 5;

        while (generated < _options.NeighborhoodSize && attempts < maxAttempts)
        {
            attempts++;

            ScheduleMove move;
            var canSwap = lessons.Length > 1;
            var generateSwap = canSwap && _random.NextDouble() < 0.3;

            if (generateSwap)
            {
                var firstIndex = _random.Next(lessons.Length);
                var secondIndex = _random.Next(lessons.Length - 1);
                if (secondIndex >= firstIndex)
                {
                    secondIndex++;
                }

                move = ScheduleMove.CreateSwap(lessons[firstIndex].Id, lessons[secondIndex].Id);
            }
            else
            {
                var lesson = lessons[_random.Next(lessons.Length)];
                var room = rooms[_random.Next(rooms.Length)];
                var slot = timeSlots[_random.Next(timeSlots.Length)];
                move = ScheduleMove.CreateChange(lesson.Id, room.Id, slot.Id);
            }

            var key = move.ToTabuKey();
            if (!seen.Add(key))
            {
                continue;
            }

            generated++;
            yield return move;
        }
    }

    private static void ApplyMove(Schedule schedule, ScheduleMove move)
    {
        if (move.Kind == MoveKind.Change)
        {
            var lesson = schedule.Lessons.FirstOrDefault(item => item.Id == move.LessonId);
            if (lesson is null)
            {
                return;
            }

            lesson.RoomId = move.RoomId;
            lesson.TimeSlotId = move.TimeSlotId;
            return;
        }

        if (!move.OtherLessonId.HasValue)
        {
            return;
        }

        var first = schedule.Lessons.FirstOrDefault(item => item.Id == move.LessonId);
        var second = schedule.Lessons.FirstOrDefault(item => item.Id == move.OtherLessonId.Value);

        if (first is null || second is null)
        {
            return;
        }

        var firstRoom = first.RoomId;
        var firstSlot = first.TimeSlotId;

        first.RoomId = second.RoomId;
        first.TimeSlotId = second.TimeSlotId;

        second.RoomId = firstRoom;
        second.TimeSlotId = firstSlot;
    }
}
