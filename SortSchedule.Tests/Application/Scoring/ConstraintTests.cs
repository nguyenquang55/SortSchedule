using SortSchedule.Application.Services;
using SortSchedule.Domain.Entities;
using SortSchedule.Domain.Enums;

namespace SortSchedule.Tests.Application.Scoring;

public sealed class ConstraintTests
{
    [Fact]
    public void CalculateScore_WhenOfflineLessonUsesMatchingRoomType_ShouldNotPenalizeHardScore()
    {
        var schedule = CreateSchedule(DeliveryMode.Offline, RoomType.Theory, roomId: 1, roomType: RoomType.Theory);
        var scorer = new ScheduleScorer();

        var score = scorer.CalculateScore(schedule);

        Assert.Equal(0, score.HardScore);
    }

    [Fact]
    public void CalculateScore_WhenOfflineLessonUsesMismatchedRoomType_ShouldPenalizeHardScore()
    {
        var schedule = CreateSchedule(DeliveryMode.Offline, RoomType.Theory, roomId: 1, roomType: RoomType.Practice);
        var scorer = new ScheduleScorer();

        var score = scorer.CalculateScore(schedule);

        Assert.Equal(-1, score.HardScore);
    }

    [Fact]
    public void CalculateScore_WhenOnlineLessonHasRoom_ShouldPenalizeHardScore()
    {
        var schedule = CreateSchedule(DeliveryMode.Online, RoomType.Theory, roomId: 1, roomType: RoomType.Theory);
        var scorer = new ScheduleScorer();

        var score = scorer.CalculateScore(schedule);

        Assert.Equal(-1, score.HardScore);
    }

    [Fact]
    public void CalculateScore_WhenOnlineLessonHasNoRoom_ShouldNotPenalizeHardScore()
    {
        var schedule = CreateSchedule(DeliveryMode.Online, RoomType.Theory, roomId: null, roomType: RoomType.Theory);
        var scorer = new ScheduleScorer();

        var score = scorer.CalculateScore(schedule);

        Assert.Equal(0, score.HardScore);
    }

    [Fact]
    public void CalculateScore_WhenOfflineLessonHasNoRoom_ShouldPenalizeHardScore()
    {
        var schedule = CreateSchedule(DeliveryMode.Offline, RoomType.Theory, roomId: null, roomType: RoomType.Theory);
        var scorer = new ScheduleScorer();

        var score = scorer.CalculateScore(schedule);

        Assert.Equal(-1, score.HardScore);
    }

    private static Schedule CreateSchedule(
        DeliveryMode deliveryMode,
        RoomType requiredRoomType,
        int? roomId,
        RoomType roomType)
    {
        return new Schedule
        {
            Teachers = [new Teacher { Id = 1, Name = "T1" }],
            Rooms = [new Room { Id = 1, Name = "R1", Capacity = 40, RoomType = roomType }],
            StudentGroups = [new StudentGroup { Id = 1, Name = "G1", Size = 30 }],
            Subjects = [new Subject { Id = 1, Name = "Math" }],
            Lessons =
            [
                new Lesson
                {
                    Id = 1,
                    TeacherId = 1,
                    StudentGroupId = 1,
                    SubjectId = 1,
                    RequiredRoomType = requiredRoomType,
                    DeliveryMode = deliveryMode,
                    RoomId = roomId
                }
            ]
        };
    }
}
