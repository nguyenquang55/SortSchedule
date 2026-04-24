using SortSchedule.Domain.Entities;

namespace SortSchedule.Application.Abstractions;

public interface IScheduleOrchestrator
{
    Schedule Solve(Schedule schedule, CancellationToken cancellationToken = default);
}
