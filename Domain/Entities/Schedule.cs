using SortSchedule.Domain.Common;

namespace SortSchedule.Domain.Entities;

public sealed class Schedule
{
    public List<Teacher> Teachers { get; init; } = [];

    public List<Room> Rooms { get; init; } = [];

    public List<StudentGroup> StudentGroups { get; init; } = [];

    public List<Subject> Subjects { get; init; } = [];

    public List<TimeSlot> TimeSlots { get; init; } = [];

    public List<Lesson> Lessons { get; init; } = [];

    public HardSoftScore Score { get; set; } = HardSoftScore.Zero;

    public Schedule DeepClone() => new()
    {
        Teachers = Teachers.ToList(),
        Rooms = Rooms.ToList(),
        StudentGroups = StudentGroups.ToList(),
        Subjects = Subjects.ToList(),
        TimeSlots = TimeSlots.ToList(),
        Lessons = Lessons.Select(static lesson => lesson.Clone()).ToList(),
        Score = Score
    };
}
