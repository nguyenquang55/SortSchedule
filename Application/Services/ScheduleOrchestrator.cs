using SortSchedule.Application.Abstractions;
using SortSchedule.Domain.Entities;

namespace SortSchedule.Application.Services;

public sealed class ScheduleOrchestrator(
    FirstFitSolver firstFitSolver,
    TabuSearchSolver tabuSearchSolver) : IScheduleOrchestrator
{
    private readonly FirstFitSolver _firstFitSolver = firstFitSolver;
    private readonly TabuSearchSolver _tabuSearchSolver = tabuSearchSolver;

    public Schedule Solve(Schedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        var firstFitSolution = _firstFitSolver.Solve(schedule, cancellationToken);
        return _tabuSearchSolver.Solve(firstFitSolution, cancellationToken);
    }
}
