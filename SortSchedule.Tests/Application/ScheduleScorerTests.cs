using SortSchedule.Application.Services;
using SortSchedule.Domain.Entities;
using SortSchedule.Domain.Enums;

namespace SortSchedule.Tests.Application;

public sealed class ScheduleScorerTests
{
    [Fact]
    public void CalculateScore_WhenRoomConflictExists_ShouldPenalizeHardScore()
    {
        var schedule = new Schedule
        {
            Teachers = [new Teacher { Id = 1, Name = "T1" }, new Teacher { Id = 2, Name = "T2" }],
            Rooms = [new Room { Id = 1, Name = "R1", Capacity = 30, RoomType = RoomType.Theory }],
            StudentGroups = [new StudentGroup { Id = 1, Name = "G1", Size = 20 }, new StudentGroup { Id = 2, Name = "G2", Size = 20 }],
            Subjects = [new Subject { Id = 1, Name = "Math" }],
            TimeSlots = [new TimeSlot { Id = 1, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(9, 0) }],
            Lessons =
            [
                new Lesson { Id = 1, TeacherId = 1, StudentGroupId = 1, SubjectId = 1, RequiredRoomType = RoomType.Theory, DeliveryMode = DeliveryMode.Offline, RoomId = 1, TimeSlotId = 1 },
                new Lesson { Id = 2, TeacherId = 2, StudentGroupId = 2, SubjectId = 1, RequiredRoomType = RoomType.Theory, DeliveryMode = DeliveryMode.Offline, RoomId = 1, TimeSlotId = 1 }
            ]
        };

        var scorer = new ScheduleScorer();

        var score = scorer.CalculateScore(schedule);

        Assert.Equal(-1, score.HardScore);
    }

    [Fact]
    public void CalculateScore_WhenTeacherHasConsecutiveLessons_ShouldRewardSoftScore()
    {
        var schedule = TestScheduleFactory.CreateBaseSchedule();
        schedule.Lessons[0].RoomId = 1;
        schedule.Lessons[0].TimeSlotId = 1;
        schedule.Lessons[1].RoomId = 1;
        schedule.Lessons[1].TimeSlotId = 2;
        schedule.Lessons[2].RoomId = 2;
        schedule.Lessons[2].TimeSlotId = 3;

        var scorer = new ScheduleScorer();

        var score = scorer.CalculateScore(schedule);

        Assert.True(score.SoftScore >= 1);
    }
}
