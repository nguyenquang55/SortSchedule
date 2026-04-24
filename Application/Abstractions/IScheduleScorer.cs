using SortSchedule.Domain.Common;
using SortSchedule.Domain.Entities;

namespace SortSchedule.Application.Abstractions;

public interface IScheduleScorer
{
    HardSoftScore CalculateScore(Schedule schedule);
}
