using SortSchedule.Application.Services;

namespace SortSchedule.Tests.Application;

public sealed class FirstFitSolverTests
{
    [Fact]
    public void Solve_ShouldAssignAllLessons_AndAvoidHardConflictsWhenPossible()
    {
        var schedule = TestScheduleFactory.CreateBaseSchedule();
        var scorer = new ScheduleScorer();
        var solver = new FirstFitSolver(scorer);

        var solved = solver.Solve(schedule);

        Assert.All(solved.Lessons, static lesson =>
        {
            Assert.NotNull(lesson.RoomId);
            Assert.NotNull(lesson.TimeSlotId);
        });
        Assert.Equal(0, solved.Score.HardScore);
    }
}
