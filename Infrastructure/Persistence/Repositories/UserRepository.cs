using Microsoft.EntityFrameworkCore;
using SortSchedule.Application.Abstractions.Auth;
using SortSchedule.Domain.Entities;

namespace SortSchedule.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == email, ct);
    }

    public async Task<AppUser?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<bool> ExistsAsync(string email, CancellationToken ct = default)
    {
        return await _dbContext.Users.AnyAsync(x => x.Email == email, ct);
    }

    public Task AddAsync(AppUser user, CancellationToken ct = default)
    {
        return _dbContext.Users.AddAsync(user, ct).AsTask();
    }

    public async Task<AppRole?> GetRoleByNameAsync(string roleName, CancellationToken ct = default)
    {
        return await _dbContext.Roles.FirstOrDefaultAsync(x => x.Name == roleName, ct);
    }

    public async Task<RefreshToken?> GetActiveRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        return await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash
                    && x.RevokedAtUtc == null
                    && x.ExpiresAtUtc > now,
                ct);
    }

    public async Task RevokeAllRefreshTokensAsync(Guid userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        await _dbContext.RefreshTokens
            .Where(x => x.UserId == userId
                     && x.RevokedAtUtc == null
                     && x.ExpiresAtUtc > now)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.RevokedAtUtc, now), ct);

        _dbContext.ChangeTracker.Clear();
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return _dbContext.SaveChangesAsync(ct);
    }
}
