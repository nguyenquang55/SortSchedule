namespace SortSchedule.Domain.Entities;

public sealed class UserRole
{
    public Guid UserId { get; init; }

    public AppUser User { get; init; } = null!;

    public Guid RoleId { get; init; }

    public AppRole Role { get; init; } = null!;
}
