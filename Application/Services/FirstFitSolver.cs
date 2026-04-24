using SortSchedule.Application.Abstractions;
using SortSchedule.Domain.Entities;

namespace SortSchedule.Application.Services;

public sealed class FirstFitSolver(IScheduleScorer scorer) : IScheduleSolver
{
    private readonly IScheduleScorer _scorer = scorer;

    public Schedule Solve(Schedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        var solution = schedule.DeepClone();
        var rooms = solution.Rooms.OrderBy(static room => room.Id).ToArray();
        var timeSlots = solution.TimeSlots
            .OrderBy(static slot => slot.DayOfWeek)
            .ThenBy(static slot => slot.StartTime)
            .ToArray();

        if (rooms.Length == 0 || timeSlots.Length == 0)
        {
            solution.Score = _scorer.CalculateScore(solution);
            return solution;
        }

        foreach (var lesson in solution.Lessons.OrderBy(static item => item.Id))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (lesson.RoomId.HasValue && lesson.TimeSlotId.HasValue && !HasHardConflict(solution, lesson))
            {
                continue;
            }

            var assigned = false;
            foreach (var slot in timeSlots)
            {
                foreach (var room in rooms)
                {
                    lesson.RoomId = room.Id;
                    lesson.TimeSlotId = slot.Id;

                    if (!HasHardConflict(solution, lesson))
                    {
                        assigned = true;
                        break;
                    }
                }

                if (assigned)
                {
                    break;
                }
            }

            if (!assigned)
            {
                lesson.RoomId = rooms[0].Id;
                lesson.TimeSlotId = timeSlots[0].Id;
            }
        }

        solution.Score = _scorer.CalculateScore(solution);
        return solution;
    }

    private static bool HasHardConflict(Schedule schedule, Lesson target)
    {
        if (!target.TimeSlotId.HasValue || !target.RoomId.HasValue)
        {
            return false;
        }

        foreach (var lesson in schedule.Lessons)
        {
            if (lesson.Id == target.Id || !lesson.TimeSlotId.HasValue)
            {
                continue;
            }

            if (lesson.TimeSlotId != target.TimeSlotId)
            {
                continue;
            }

            if (lesson.RoomId == target.RoomId ||
                lesson.TeacherId == target.TeacherId ||
                lesson.StudentGroupId == target.StudentGroupId)
            {
                return true;
            }
        }

        return false;
    }
}
