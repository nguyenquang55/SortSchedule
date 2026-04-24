namespace SortSchedule.Domain.Entities;

public sealed class Subject
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? RequiredRoomType { get; init; }
}
