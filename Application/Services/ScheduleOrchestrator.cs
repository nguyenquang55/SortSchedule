using SortSchedule.Application.Abstractions;
using SortSchedule.Domain.Entities;
using SortSchedule.Application.DTOs.Schedules;
using Shared.Common;
using System.Net;

namespace SortSchedule.Application.Services;

public sealed class ScheduleOrchestrator(
    FirstFitSolver firstFitSolver,
    TabuSearchSolver tabuSearchSolver,
    IScheduleScenarioRepository scenarioRepository) : IScheduleOrchestrator
{
    private readonly FirstFitSolver _firstFitSolver = firstFitSolver;
    private readonly TabuSearchSolver _tabuSearchSolver = tabuSearchSolver;
    private readonly IScheduleScenarioRepository _scenarioRepository = scenarioRepository;

    public Result<Schedule> Solve(Schedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        var firstFitSolution = _firstFitSolver.Solve(schedule, cancellationToken);
        var solved = _tabuSearchSolver.Solve(firstFitSolution, cancellationToken);
        return Result<Schedule>.SuccessResult(solved);
    }

    public async Task<Result<SolveScheduleResponse>> SolveAsync(SolveScheduleRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Result<SolveScheduleResponse>.FailureResult("Request payload is required.");
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
                return Result<SolveScheduleResponse>.FailureResult($"Scenario '{scenarioId}' was not found.", statusCode: HttpStatusCode.NotFound);
            }
        }

        if (input is null)
        {
            return Result<SolveScheduleResponse>.FailureResult("Provide either schedule data in request.Schedule or an existing scenarioId.");
        }

        var solvedRes = Solve(input, cancellationToken);
        var solved = solvedRes.Data;

        if (hasScenarioId && request.SaveResult)
        {
            await _scenarioRepository.SaveResultAsync(scenarioId!, solved, cancellationToken);
        }

        return Result<SolveScheduleResponse>.SuccessResult(new SolveScheduleResponse
        {
            ScenarioId = scenarioId,
            Source = source,
            HardScore = solved.Score.HardScore,
            SoftScore = solved.Score.SoftScore,
            Schedule = solved.ToDto()
        });
    }

    public async Task<Result<bool>> SaveScenarioAsync(string scenarioId, Schedule schedule, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scenarioId))
        {
            return Result<bool>.FailureResult("scenarioId is required.");
        }

        await _scenarioRepository.SaveScenarioAsync(scenarioId, schedule, cancellationToken);
        return Result<bool>.SuccessResult(true, "Scenario saved.");
    }

    public async Task<Result<SolveScheduleResponse>> GetResultAsync(string scenarioId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scenarioId))
        {
            return Result<SolveScheduleResponse>.FailureResult("scenarioId is required.");
        }

        var result = await _scenarioRepository.GetResultAsync(scenarioId, cancellationToken);
        
        if (result is null)
        {
            return Result<SolveScheduleResponse>.FailureResult($"Result for scenario '{scenarioId}' was not found.", statusCode: HttpStatusCode.NotFound);
        }

        return Result<SolveScheduleResponse>.SuccessResult(new SolveScheduleResponse
        {
            ScenarioId = scenarioId,
            Source = "result-store",
            HardScore = result.Score.HardScore,
            SoftScore = result.Score.SoftScore,
            Schedule = result.ToDto()
        });
    }
}
