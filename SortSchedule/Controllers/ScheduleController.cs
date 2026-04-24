using Microsoft.AspNetCore.Mvc;
using SortSchedule.Application.Abstractions;
using SortSchedule.Contracts;
using SortSchedule.Domain.Entities;

namespace SortSchedule.Controllers;

[ApiController]
[Route("api/schedule")]
public sealed class ScheduleController(
    IScheduleOrchestrator orchestrator,
    IScheduleScenarioRepository scenarioRepository) : ControllerBase
{
    private readonly IScheduleOrchestrator _orchestrator = orchestrator;
    private readonly IScheduleScenarioRepository _scenarioRepository = scenarioRepository;

    [HttpPost("scenarios/{scenarioId}")]
    public async Task<IActionResult> SaveScenario(string scenarioId, [FromBody] ScheduleDto schedule, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(scenarioId))
        {
            return BadRequest("scenarioId is required.");
        }

        await _scenarioRepository.SaveScenarioAsync(scenarioId, schedule.ToDomain(), cancellationToken);
        return Ok(new { ScenarioId = scenarioId, Message = "Scenario saved." });
    }

    [HttpPost("solve")]
    public async Task<ActionResult<SolveScheduleResponse>> Solve([FromBody] SolveScheduleRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("Request payload is required.");
        }

        var scenarioId = request.ScenarioId?.Trim();
        var hasScenarioId = !string.IsNullOrWhiteSpace(scenarioId);

        Schedule? input = null;
        var source = string.Empty;

        if (request.Schedule is not null)
        {
            input = request.Schedule.ToDomain();
            source = "request";

            if (request.SaveScenario && hasScenarioId)
            {
                await _scenarioRepository.SaveScenarioAsync(scenarioId!, input, cancellationToken);
            }
        }
        else if (hasScenarioId)
        {
            input = await _scenarioRepository.GetScenarioAsync(scenarioId!, cancellationToken);
            source = "scenario";

            if (input is null)
            {
                return NotFound($"Scenario '{scenarioId}' was not found.");
            }
        }

        if (input is null)
        {
            return BadRequest("Provide either schedule data in request.Schedule or an existing scenarioId.");
        }

        var solved = _orchestrator.Solve(input, cancellationToken);

        if (hasScenarioId && request.SaveResult)
        {
            await _scenarioRepository.SaveResultAsync(scenarioId!, solved, cancellationToken);
        }

        return Ok(new SolveScheduleResponse
        {
            ScenarioId = scenarioId,
            Source = source,
            HardScore = solved.Score.HardScore,
            SoftScore = solved.Score.SoftScore,
            Schedule = solved.ToDto()
        });
    }

    [HttpGet("result/{scenarioId}")]
    public async Task<ActionResult<SolveScheduleResponse>> GetResult(string scenarioId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(scenarioId))
        {
            return BadRequest("scenarioId is required.");
        }

        var result = await _scenarioRepository.GetResultAsync(scenarioId, cancellationToken);
        if (result is null)
        {
            return NotFound($"Result for scenario '{scenarioId}' was not found.");
        }

        return Ok(new SolveScheduleResponse
        {
            ScenarioId = scenarioId,
            Source = "result-store",
            HardScore = result.Score.HardScore,
            SoftScore = result.Score.SoftScore,
            Schedule = result.ToDto()
        });
    }
}
