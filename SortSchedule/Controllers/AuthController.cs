using Microsoft.AspNetCore.Mvc;
using SortSchedule.Application.Abstractions.Auth;
using SortSchedule.Application.DTOs.Auth;
using SortSchedule.Controllers.Common;

namespace SortSchedule.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : BaseController
{
    private readonly IAuthService _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
       return await HandleActionAsync(_authService.RegisterAsync(request, cancellationToken));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        return await HandleActionAsync(_authService.LoginAsync(request, cancellationToken));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        return await HandleActionAsync(_authService.RefreshTokenAsync(request, cancellationToken));
    }
}
