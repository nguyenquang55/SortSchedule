namespace SortSchedule.Application.DTOs.Auth;

public sealed class RegisterRequest
{
    public string Email { get; init; } = string.Empty;

    public string UserName { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string ConfirmPassword { get; init; } = string.Empty;
}
