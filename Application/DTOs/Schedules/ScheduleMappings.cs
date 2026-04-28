using SortSchedule.Domain.Common;
using SortSchedule.Domain.Entities;
using SortSchedule.Domain.Enums;

namespace SortSchedule.Application.DTOs.Schedules;

public static class ScheduleMappings
{
    public static Schedule ToDomain(this ScheduleDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Schedule
        {
            Teachers = dto.Teachers.Select(static item => new Teacher
            {
                Id = item.Id,
                Name = item.Name,
                SpecificRequirements = item.SpecificRequirements
            }).ToList(),
            Rooms = dto.Rooms.Select(static item => new Room
            {
                Id = item.Id,
                Name = item.Name,
                Capacity = item.Capacity,
                RoomType = item.RoomType
            }).ToList(),
            StudentGroups = dto.StudentGroups.Select(static item => new StudentGroup
            {
                Id = item.Id,
                Name = item.Name,
                Size = item.Size
            }).ToList(),
            Subjects = dto.Subjects.Select(static item => new Subject
            {
                Id = item.Id,
                Name = item.Name,
                RequiredRoomType = item.RequiredRoomType
            }).ToList(),
            TimeSlots = dto.TimeSlots.Select(static item => new TimeSlot
            {
                Id = item.Id,
                DayOfWeek = item.DayOfWeek,
                StartTime = item.StartTime,
                EndTime = item.EndTime
            }).ToList(),
            Lessons = dto.Lessons.Select(static item => new Lesson
            {
                Id = item.Id,
                TeacherId = item.TeacherId,
                StudentGroupId = item.StudentGroupId,
                SubjectId = item.SubjectId,
                RequiredRoomType = item.RequiredRoomType,
                DeliveryMode = item.DeliveryMode,
                RoomId = item.RoomId,
                TimeSlotId = item.TimeSlotId
            }).ToList(),
            Score = new HardSoftScore(dto.HardScore, dto.SoftScore)
        };
    }

    public static ScheduleDto ToDto(this Schedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        return new ScheduleDto
        {
            Teachers = schedule.Teachers.Select(static item => new TeacherDto
            {
                Id = item.Id,
                Name = item.Name,
                SpecificRequirements = item.SpecificRequirements
            }).ToList(),
            Rooms = schedule.Rooms.Select(static item => new RoomDto
            {
                Id = item.Id,
                Name = item.Name,
                Capacity = item.Capacity,
                RoomType = item.RoomType
            }).ToList(),
            StudentGroups = schedule.StudentGroups.Select(static item => new StudentGroupDto
            {
                Id = item.Id,
                Name = item.Name,
                Size = item.Size
            }).ToList(),
            Subjects = schedule.Subjects.Select(static item => new SubjectDto
            {
                Id = item.Id,
                Name = item.Name,
                RequiredRoomType = item.RequiredRoomType
            }).ToList(),
            TimeSlots = schedule.TimeSlots.Select(static item => new TimeSlotDto
            {
                Id = item.Id,
                DayOfWeek = item.DayOfWeek,
                StartTime = item.StartTime,
                EndTime = item.EndTime
            }).ToList(),
            Lessons = schedule.Lessons.Select(static item => new LessonDto
            {
                Id = item.Id,
                TeacherId = item.TeacherId,
                StudentGroupId = item.StudentGroupId,
                SubjectId = item.SubjectId,
                RequiredRoomType = item.RequiredRoomType,
                DeliveryMode = item.DeliveryMode,
                RoomId = item.RoomId,
                TimeSlotId = item.TimeSlotId
            }).ToList(),
            HardScore = schedule.Score.HardScore,
            SoftScore = schedule.Score.SoftScore
        };
    }
}
