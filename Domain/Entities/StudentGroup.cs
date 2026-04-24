namespace SortSchedule.Domain.Entities;

public sealed class StudentGroup
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Size { get; init; }
}
