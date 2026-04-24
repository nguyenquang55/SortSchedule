namespace SortSchedule.Infrastructure.Auth;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Key { get; init; } = string.Empty;

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public int AccessTokenExpiryMinutes { get; init; } = 15;

    public int RefreshTokenExpiryDays { get; init; } = 7;
}
