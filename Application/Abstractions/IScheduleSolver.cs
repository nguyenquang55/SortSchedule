using SortSchedule.Domain.Entities;

namespace SortSchedule.Application.Abstractions;

public interface IScheduleSolver
{
    Schedule Solve(Schedule schedule, CancellationToken cancellationToken = default);
}
