using Microsoft.Extensions.Options;
using SortSchedule.Application.DTOs.Auth;
using SortSchedule.Domain.Entities;
using SortSchedule.Infrastructure.Auth;

namespace SortSchedule.Tests.Auth;

public sealed class TokenServiceTests
{
    [Fact]
    public void GenerateAccessToken_ShouldContainExpectedClaims()
    {
        var service = CreateService();
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "student@example.com",
            UserName = "student01"
        };

        var token = service.GenerateAccessToken(user, ["Student", "Lecturer"]);

        var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Contains(jwt.Claims, c => c.Type == "sub" && c.Value == user.Id.ToString());
        Assert.Contains(jwt.Claims, c => c.Type == "email" && c.Value == user.Email);
        Assert.Contains(jwt.Claims, c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName && c.Value == user.UserName);
        Assert.Contains(jwt.Claims, c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Student");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueRawAndMatchingHash()
    {
        var service = CreateService();

        var first = service.GenerateRefreshToken();
        var second = service.GenerateRefreshToken();

        Assert.NotEqual(first.RawToken, second.RawToken);
        Assert.NotEqual(first.TokenHash, second.TokenHash);

        var expectedHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(first.RawToken)));
        Assert.Equal(expectedHash, first.TokenHash);
    }

    [Fact]
    public void Validate_WithValidToken_ShouldReturnUserId()
    {
        var service = CreateService();
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "student@example.com",
            UserName = "student01"
        };

        var token = service.GenerateAccessToken(user, ["Student"]);

        var result = service.Validate(token);

        Assert.True(result.IsValid);
        Assert.Equal(user.Id, result.UserId);
    }

    [Fact]
    public void Validate_WithTamperedToken_ShouldReturnFailure()
    {
        var service = CreateService();

        var result = service.Validate("invalid.token.value");

        Assert.False(result.IsValid);
        Assert.Null(result.UserId);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    private static TokenService CreateService()
    {
        var settings = Options.Create(new JwtSettings
        {
            Key = "DEV-ONLY-KEY-CHANGE-THIS-TO-A-LONG-RANDOM-VALUE-FOR-LOCAL-TESTING-1234567890",
            Issuer = "SortSchedule",
            Audience = "SortSchedule",
            AccessTokenExpiryMinutes = 15,
            RefreshTokenExpiryDays = 7
        });

        return new TokenService(settings);
    }
}
