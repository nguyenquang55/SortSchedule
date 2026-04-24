using SortSchedule.Domain.Entities;
using SortSchedule.Domain.Enums;

namespace SortSchedule.Tests.Application;

internal static class TestScheduleFactory
{
    public static Schedule CreateBaseSchedule()
    {
        return new Schedule
        {
            Teachers =
            [
                new Teacher { Id = 1, Name = "T1" },
                new Teacher { Id = 2, Name = "T2" }
            ],
            Rooms =
            [
                new Room { Id = 1, Name = "R1", Capacity = 40, RoomType = RoomType.Theory },
                new Room { Id = 2, Name = "R2", Capacity = 40, RoomType = RoomType.Theory }
            ],
            StudentGroups =
            [
                new StudentGroup { Id = 1, Name = "G1", Size = 30 },
                new StudentGroup { Id = 2, Name = "G2", Size = 30 }
            ],
            Subjects =
            [
                new Subject { Id = 1, Name = "Math" },
                new Subject { Id = 2, Name = "Physics" }
            ],
            TimeSlots =
            [
                new TimeSlot { Id = 1, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(9, 0) },
                new TimeSlot { Id = 2, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0) },
                new TimeSlot { Id = 3, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0) }
            ],
            Lessons =
            [
                new Lesson { Id = 1, TeacherId = 1, StudentGroupId = 1, SubjectId = 1, RequiredRoomType = RoomType.Theory, DeliveryMode = DeliveryMode.Offline },
                new Lesson { Id = 2, TeacherId = 1, StudentGroupId = 2, SubjectId = 2, RequiredRoomType = RoomType.Theory, DeliveryMode = DeliveryMode.Offline },
                new Lesson { Id = 3, TeacherId = 2, StudentGroupId = 1, SubjectId = 2, RequiredRoomType = RoomType.Theory, DeliveryMode = DeliveryMode.Offline }
            ]
        };
    }
}
