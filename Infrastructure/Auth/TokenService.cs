using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SortSchedule.Application.Abstractions.Auth;
using SortSchedule.Domain.Entities;

namespace SortSchedule.Infrastructure.Auth;

public sealed class TokenService(IOptions<JwtSettings> jwtOptions) : ITokenService
{
    private readonly JwtSettings _settings = jwtOptions.Value;

    public string GenerateAccessToken(AppUser user, IEnumerable<string> roles)
    {
        ArgumentNullException.ThrowIfNull(user);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("email", user.Email),
            new("unique_name", user.UserName)
        };

        claims.AddRange(roles.Select(static role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string RawToken, string TokenHash) GenerateRefreshToken()
    {
        var rawBytes = RandomNumberGenerator.GetBytes(64);
        var rawToken = Convert.ToBase64String(rawBytes);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
        return (rawToken, hash);
    }

    public SortSchedule.Application.DTOs.Auth.TokenValidationResult Validate(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return SortSchedule.Application.DTOs.Auth.TokenValidationResult.Failure("Token is required.");
        }

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
            ValidateLifetime = false
        };

        var result = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler()
            .ValidateTokenAsync(token, validationParameters)
            .GetAwaiter()
            .GetResult();
        if (!result.IsValid)
        {
            return SortSchedule.Application.DTOs.Auth.TokenValidationResult.Failure("Invalid token.");
        }

        if (!result.Claims.TryGetValue("sub", out var subject) || subject is null)
        {
            return SortSchedule.Application.DTOs.Auth.TokenValidationResult.Failure("Token subject is missing.");
        }

        if (!Guid.TryParse(subject.ToString(), out var userId))
        {
            return SortSchedule.Application.DTOs.Auth.TokenValidationResult.Failure("Token subject is invalid.");
        }

        return SortSchedule.Application.DTOs.Auth.TokenValidationResult.Success(userId);
    }
}
