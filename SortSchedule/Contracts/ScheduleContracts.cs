using SortSchedule.Domain.Enums;

namespace SortSchedule.Contracts;

public sealed class TeacherDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? SpecificRequirements { get; init; }
}

public sealed class RoomDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Capacity { get; init; }

    public RoomType RoomType { get; init; }
}

public sealed class StudentGroupDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Size { get; init; }
}

public sealed class SubjectDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public RoomType? RequiredRoomType { get; init; }
}

public sealed class TimeSlotDto
{
    public int Id { get; init; }

    public DayOfWeek DayOfWeek { get; init; }

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }
}

public sealed class LessonDto
{
    public int Id { get; init; }

    public int TeacherId { get; init; }

    public int StudentGroupId { get; init; }

    public int SubjectId { get; init; }

    public RoomType RequiredRoomType { get; init; }

    public DeliveryMode DeliveryMode { get; init; }

    public int? RoomId { get; init; }

    public int? TimeSlotId { get; init; }
}

public sealed class ScheduleDto
{
    public List<TeacherDto> Teachers { get; init; } = [];

    public List<RoomDto> Rooms { get; init; } = [];

    public List<StudentGroupDto> StudentGroups { get; init; } = [];

    public List<SubjectDto> Subjects { get; init; } = [];

    public List<TimeSlotDto> TimeSlots { get; init; } = [];

    public List<LessonDto> Lessons { get; init; } = [];

    public int HardScore { get; init; }

    public int SoftScore { get; init; }
}

public sealed class SolveScheduleRequest
{
    public string? ScenarioId { get; init; }

    public ScheduleDto? Schedule { get; init; }

    public bool SaveScenario { get; init; }

    public bool SaveResult { get; init; } = true;
}

public sealed class SolveScheduleResponse
{
    public string? ScenarioId { get; init; }

    public string Source { get; init; } = string.Empty;

    public int HardScore { get; init; }

    public int SoftScore { get; init; }

    public ScheduleDto Schedule { get; init; } = new();
}
