using SortSchedule.Domain.Enums;

namespace SortSchedule.Domain.Entities;

public sealed class Room
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Capacity { get; init; }

    public RoomType RoomType { get; init; }
}
