using SortSchedule.Domain.Entities;

namespace SortSchedule.Application.Abstractions.Auth;

public interface IUserRepository
{
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<AppUser?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsAsync(string email, CancellationToken ct = default);

    Task AddAsync(AppUser user, CancellationToken ct = default);

    Task<AppRole?> GetRoleByNameAsync(string roleName, CancellationToken ct = default);

    Task<RefreshToken?> GetActiveRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default);

    Task RevokeAllRefreshTokensAsync(Guid userId, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
