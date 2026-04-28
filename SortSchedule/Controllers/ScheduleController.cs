using Microsoft.AspNetCore.Mvc;
using SortSchedule.Application.Abstractions;
using SortSchedule.Application.DTOs.Schedules;
using SortSchedule.Domain.Entities;
using SortSchedule.Controllers.Common;

namespace SortSchedule.Controllers;

[ApiController]
[Route("api/schedule")]
public sealed class ScheduleController(
    IScheduleOrchestrator orchestrator,
    IScheduleScenarioRepository scenarioRepository) : BaseController
{
    private readonly IScheduleOrchestrator _orchestrator = orchestrator;
    private readonly IScheduleScenarioRepository _scenarioRepository = scenarioRepository;

    [HttpPost("scenarios/{scenarioId}")]
    public async Task<IActionResult> SaveScenario(string scenarioId, [FromBody] ScheduleDto schedule, CancellationToken cancellationToken)
    {
        return await HandleActionAsync(_orchestrator.SaveScenarioAsync(scenarioId, schedule.ToDomain(), cancellationToken));
    }

    [HttpPost("solve")]
    public async Task<IActionResult> Solve([FromBody] SolveScheduleRequest request, CancellationToken cancellationToken)
    {
        return await HandleActionAsync(_orchestrator.SolveAsync(request, cancellationToken));
    }

    [HttpGet("result/{scenarioId}")]
    public async Task<IActionResult> GetResult(string scenarioId, CancellationToken cancellationToken)
    {
        return await HandleActionAsync(_orchestrator.GetResultAsync(scenarioId, cancellationToken));
    }
}
