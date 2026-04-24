using System.Collections.Concurrent;
using SortSchedule.Application.Abstractions;
using SortSchedule.Domain.Entities;

namespace SortSchedule.Infrastructure.Repositories;

public sealed class InMemoryScheduleScenarioRepository : IScheduleScenarioRepository
{
    private readonly ConcurrentDictionary<string, Schedule> _scenarios = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Schedule> _results = new(StringComparer.OrdinalIgnoreCase);

    public Task<Schedule?> GetScenarioAsync(string scenarioId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_scenarios.TryGetValue(scenarioId, out var schedule) ? schedule.DeepClone() : null);
    }

    public Task SaveScenarioAsync(string scenarioId, Schedule schedule, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _scenarios[scenarioId] = schedule.DeepClone();
        return Task.CompletedTask;
    }

    public Task<Schedule?> GetResultAsync(string scenarioId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_results.TryGetValue(scenarioId, out var schedule) ? schedule.DeepClone() : null);
    }

    public Task SaveResultAsync(string scenarioId, Schedule schedule, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _results[scenarioId] = schedule.DeepClone();
        return Task.CompletedTask;
    }
}
