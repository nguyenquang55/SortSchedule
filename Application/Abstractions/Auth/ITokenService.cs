using SortSchedule.Application.DTOs.Auth;
using SortSchedule.Domain.Entities;

namespace SortSchedule.Application.Abstractions.Auth;

public interface ITokenService
{
    string GenerateAccessToken(AppUser user, IEnumerable<string> roles);

    (string RawToken, string TokenHash) GenerateRefreshToken();

    TokenValidationResult Validate(string token);
}
