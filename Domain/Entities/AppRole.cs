namespace SortSchedule.Domain.Entities;

public sealed class AppRole
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; init; } = [];
}
