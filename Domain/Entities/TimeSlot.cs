namespace SortSchedule.Domain.Entities;

public sealed class TimeSlot
{
    public int Id { get; init; }

    public DayOfWeek DayOfWeek { get; init; }

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }
}
