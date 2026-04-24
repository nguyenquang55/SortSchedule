namespace SortSchedule.Domain.Entities;

public sealed class Teacher
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? SpecificRequirements { get; init; }
}
