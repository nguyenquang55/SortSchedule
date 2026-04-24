using SortSchedule.Application.Options;
using SortSchedule.Application.Services;

namespace SortSchedule.Tests.Application;

public sealed class TabuSearchSolverTests
{
    [Fact]
    public void Solve_ShouldNotReturnWorseScoreThanInitialSolution()
    {
        var schedule = TestScheduleFactory.CreateBaseSchedule();

        foreach (var lesson in schedule.Lessons)
        {
            lesson.RoomId = 1;
            lesson.TimeSlotId = 1;
        }

        var scorer = new ScheduleScorer();
        schedule.Score = scorer.CalculateScore(schedule);

        var options = new TabuSearchOptions
        {
            TabuTenure = 10,
            MaxIterations = 300,
            NeighborhoodSize = 80,
            RandomSeed = 42
        };

        var solver = new TabuSearchSolver(scorer, options);

        var solved = solver.Solve(schedule);

        Assert.True(solved.Score >= schedule.Score);
    }
}
