namespace SortSchedule.Application.DTOs.Auth;

public sealed class TokenValidationResult
{
    private TokenValidationResult(bool isValid, Guid? userId, string? error)
    {
        IsValid = isValid;
        UserId = userId;
        Error = error;
    }

    public bool IsValid { get; }

    public Guid? UserId { get; }

    public string? Error { get; }

    public static TokenValidationResult Success(Guid userId) => new(true, userId, null);

    public static TokenValidationResult Failure(string error) => new(false, null, error);
}
