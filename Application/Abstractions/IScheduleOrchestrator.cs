using SortSchedule.Domain.Entities;
using SortSchedule.Application.DTOs.Schedules;
using Shared.Common;

namespace SortSchedule.Application.Abstractions;

public interface IScheduleOrchestrator
{
    Result<Schedule> Solve(Schedule schedule, CancellationToken cancellationToken = default);

    Task<Result<SolveScheduleResponse>> SolveAsync(SolveScheduleRequest request, CancellationToken cancellationToken = default);

    Task<Result<bool>> SaveScenarioAsync(string scenarioId, Schedule schedule, CancellationToken cancellationToken = default);

    Task<Result<SolveScheduleResponse>> GetResultAsync(string scenarioId, CancellationToken cancellationToken = default);
}
