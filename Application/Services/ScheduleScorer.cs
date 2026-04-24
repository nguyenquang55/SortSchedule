using SortSchedule.Application.Abstractions;
using SortSchedule.Domain.Common;
using SortSchedule.Domain.Entities;
using SortSchedule.Domain.Enums;

namespace SortSchedule.Application.Services;

public sealed class ScheduleScorer : IScheduleScorer
{
    public HardSoftScore CalculateScore(Schedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        var hardScore = 0;
        var softScore = 0;

        var roomById = schedule.Rooms.ToDictionary(static room => room.Id);

        foreach (var lesson in schedule.Lessons)
        {
            if (lesson.DeliveryMode == DeliveryMode.Online)
            {
                if (lesson.RoomId.HasValue)
                {
                    hardScore--;
                }

                continue;
            }

            if (!lesson.RoomId.HasValue)
            {
                hardScore--;
                continue;
            }

            if (!roomById.TryGetValue(lesson.RoomId.Value, out var room) ||
                room.RoomType != lesson.RequiredRoomType)
            {
                hardScore--;
            }
        }

        var lessonsWithTime = schedule.Lessons
            .Where(static lesson => lesson.TimeSlotId.HasValue)
            .ToArray();

        hardScore -= CountPairViolations(
            lessonsWithTime
                .Where(static lesson => lesson.RoomId.HasValue)
                .GroupBy(static lesson => new
                {
                    RoomId = lesson.RoomId!.Value,
                    TimeSlotId = lesson.TimeSlotId!.Value
                }));

        hardScore -= CountPairViolations(
            lessonsWithTime.GroupBy(static lesson => new
            {
                lesson.TeacherId,
                TimeSlotId = lesson.TimeSlotId!.Value
            }));

        hardScore -= CountPairViolations(
            lessonsWithTime.GroupBy(static lesson => new
            {
                lesson.StudentGroupId,
                TimeSlotId = lesson.TimeSlotId!.Value
            }));

        var timeSlotById = schedule.TimeSlots.ToDictionary(static slot => slot.Id);

        softScore -= CalculateTeacherRoomStability(schedule.Lessons);
        softScore += CalculateTeacherTimeEfficiency(schedule.Lessons, timeSlotById);
        softScore -= CalculateStudentGroupSubjectVariety(schedule.Lessons, timeSlotById);

        return new HardSoftScore(hardScore, softScore);
    }

    private static int CountPairViolations<TGroupKey>(IEnumerable<IGrouping<TGroupKey, Lesson>> groups)
        where TGroupKey : notnull
    {
        var penalties = 0;

        foreach (var group in groups)
        {
            var count = group.Count();
            if (count > 1)
            {
                penalties += count * (count - 1) / 2;
            }
        }

        return penalties;
    }

    private static int CalculateTeacherRoomStability(IEnumerable<Lesson> lessons)
    {
        var penalties = 0;

        foreach (var teacherGroup in lessons
                     .Where(static lesson => lesson.RoomId.HasValue)
                     .GroupBy(static lesson => lesson.TeacherId))
        {
            var distinctRoomCount = teacherGroup
                .Select(static lesson => lesson.RoomId!.Value)
                .Distinct()
                .Count();

            if (distinctRoomCount > 1)
            {
                penalties += distinctRoomCount - 1;
            }
        }

        return penalties;
    }

    private static int CalculateTeacherTimeEfficiency(
        IEnumerable<Lesson> lessons,
        IReadOnlyDictionary<int, TimeSlot> timeSlotById)
    {
        var rewards = 0;

        foreach (var teacherGroup in lessons
                     .Where(static lesson => lesson.TimeSlotId.HasValue)
                     .GroupBy(static lesson => lesson.TeacherId))
        {
            var lessonsByDay = teacherGroup
                .Select(lesson => new { Lesson = lesson, TimeSlot = timeSlotById.GetValueOrDefault(lesson.TimeSlotId!.Value) })
                .Where(static item => item.TimeSlot is not null)
                .GroupBy(item => item.TimeSlot!.DayOfWeek);

            foreach (var dayGroup in lessonsByDay)
            {
                var ordered = dayGroup
                    .Select(static item => item.TimeSlot!)
                    .OrderBy(static slot => slot.StartTime)
                    .ToArray();

                for (var i = 0; i < ordered.Length - 1; i++)
                {
                    if (IsConsecutive(ordered[i], ordered[i + 1]))
                    {
                        rewards++;
                    }
                }
            }
        }

        return rewards;
    }

    private static int CalculateStudentGroupSubjectVariety(
        IEnumerable<Lesson> lessons,
        IReadOnlyDictionary<int, TimeSlot> timeSlotById)
    {
        var penalties = 0;

        foreach (var group in lessons
                     .Where(static lesson => lesson.TimeSlotId.HasValue)
                     .Select(lesson => new { Lesson = lesson, TimeSlot = timeSlotById.GetValueOrDefault(lesson.TimeSlotId!.Value) })
                     .Where(static item => item.TimeSlot is not null)
                     .GroupBy(item => new
                     {
                         item.Lesson.StudentGroupId,
                         item.TimeSlot!.DayOfWeek
                     }))
        {
            var ordered = group
                .OrderBy(static item => item.TimeSlot!.StartTime)
                .ToArray();

            for (var i = 0; i < ordered.Length - 1; i++)
            {
                if (ordered[i].Lesson.SubjectId == ordered[i + 1].Lesson.SubjectId &&
                    IsConsecutive(ordered[i].TimeSlot!, ordered[i + 1].TimeSlot!))
                {
                    penalties++;
                }
            }
        }

        return penalties;
    }

    private static bool IsConsecutive(TimeSlot first, TimeSlot second) =>
        first.DayOfWeek == second.DayOfWeek && first.EndTime == second.StartTime;
}
