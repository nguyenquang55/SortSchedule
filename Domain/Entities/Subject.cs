using SortSchedule.Domain.Enums;

namespace SortSchedule.Domain.Entities;

public sealed class Subject
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public RoomType? RequiredRoomType { get; init; }
}
