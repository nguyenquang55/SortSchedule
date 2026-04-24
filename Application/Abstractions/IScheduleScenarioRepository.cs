using SortSchedule.Domain.Entities;

namespace SortSchedule.Application.Abstractions;

public interface IScheduleScenarioRepository
{
    Task<Schedule?> GetScenarioAsync(string scenarioId, CancellationToken cancellationToken = default);

    Task SaveScenarioAsync(string scenarioId, Schedule schedule, CancellationToken cancellationToken = default);

    Task<Schedule?> GetResultAsync(string scenarioId, CancellationToken cancellationToken = default);

    Task SaveResultAsync(string scenarioId, Schedule schedule, CancellationToken cancellationToken = default);
}
