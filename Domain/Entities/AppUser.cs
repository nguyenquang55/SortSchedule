namespace SortSchedule.Domain.Entities;

public sealed class AppUser
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public DateTime? LastLoginAtUtc { get; set; }

    public ICollection<UserRole> UserRoles { get; init; } = [];

    public ICollection<RefreshToken> RefreshTokens { get; init; } = [];
}