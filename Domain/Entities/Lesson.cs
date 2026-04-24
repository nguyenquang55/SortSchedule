using SortSchedule.Domain.Enums;

namespace SortSchedule.Domain.Entities;

public sealed class Lesson
{
    public int Id { get; init; }

    public int TeacherId { get; init; }

    public int StudentGroupId { get; init; }

    public int SubjectId { get; init; }

    public RoomType RequiredRoomType { get; init; }

    public DeliveryMode DeliveryMode { get; init; }

    public int? RoomId { get; set; }

    public int? TimeSlotId { get; set; }

    public Lesson Clone() => new()
    {
        Id = Id,
        TeacherId = TeacherId,
        StudentGroupId = StudentGroupId,
        SubjectId = SubjectId,
        RequiredRoomType = RequiredRoomType,
        DeliveryMode = DeliveryMode,
        RoomId = RoomId,
        TimeSlotId = TimeSlotId
    };
}
