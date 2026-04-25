namespace SortSchedule.Application.Options;

public enum MoveKind
{
    Change,
    Swap
}

public readonly record struct ScheduleMove(
    MoveKind Kind,
    int LessonId,
    int? RoomId,
    int? TimeSlotId,
    int? OtherLessonId)
{
    public static ScheduleMove CreateChange(int lessonId, int roomId, int timeSlotId) =>
        new(MoveKind.Change, lessonId, roomId, timeSlotId, null);

    public static ScheduleMove CreateSwap(int firstLessonId, int secondLessonId)
    {
        var min = Math.Min(firstLessonId, secondLessonId);
        var max = Math.Max(firstLessonId, secondLessonId);
        return new ScheduleMove(MoveKind.Swap, min, null, null, max);
    }

    public string ToTabuKey() =>
        Kind == MoveKind.Change
            ? $"C:{LessonId}:{RoomId}:{TimeSlotId}"
            : $"S:{LessonId}:{OtherLessonId}";
}
