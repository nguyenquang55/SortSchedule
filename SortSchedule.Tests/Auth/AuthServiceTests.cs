using FluentValidation;
using NSubstitute;
using SortSchedule.Application.Abstractions.Auth;
using SortSchedule.Application.DTOs.Auth;
using SortSchedule.Application.Services;
using SortSchedule.Application.Validators;
using SortSchedule.Domain.Entities;

namespace SortSchedule.Tests.Auth;

public sealed class AuthServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IValidator<RegisterRequest> _registerValidator = new RegisterRequestValidator();
    private readonly IValidator<LoginRequest> _loginValidator = new LoginRequestValidator();
    private readonly IValidator<RefreshTokenRequest> _refreshValidator = new RefreshTokenRequestValidator();

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldReturnTokensAndUserInfo()
    {
        var service = CreateService();
        var request = new RegisterRequest
        {
            Email = "student@example.com",
            UserName = "student01",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        var studentRole = new AppRole { Id = Guid.NewGuid(), Name = "Student" };

        _userRepository.ExistsAsync("student@example.com", Arg.Any<CancellationToken>()).Returns(false);
        _userRepository.GetRoleByNameAsync("Student", Arg.Any<CancellationToken>()).Returns(studentRole);
        _passwordHasher.Hash("password123").Returns("hashed-password");
        _tokenService.GenerateAccessToken(Arg.Any<AppUser>(), Arg.Any<IEnumerable<string>>()).Returns("access-token");
        _tokenService.GenerateRefreshToken().Returns(("raw-refresh", "hash-refresh"));

        var response = await service.RegisterAsync(request);

        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("raw-refresh", response.RefreshToken);
        Assert.Equal("student01", response.UserName);
        Assert.Equal("student@example.com", response.Email);
        Assert.Contains("Student", response.Roles);

        await _userRepository.Received(1).AddAsync(Arg.Any<AppUser>(), Arg.Any<CancellationToken>());
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
    {
        var service = CreateService();
        var request = new RegisterRequest
        {
            Email = "student@example.com",
            UserName = "student01",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        _userRepository.ExistsAsync("student@example.com", Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldRevokeOldTokensAndReturnNewPair()
    {
        var service = CreateService();
        var request = new LoginRequest
        {
            Email = "student@example.com",
            Password = "password123"
        };

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "student@example.com",
            UserName = "student01",
            PasswordHash = "hashed-password"
        };
        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            User = user,
            RoleId = Guid.NewGuid(),
            Role = new AppRole { Name = "Student" }
        });

        _userRepository.GetByEmailAsync("student@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("password123", "hashed-password").Returns(true);
        _tokenService.GenerateAccessToken(user, Arg.Any<IEnumerable<string>>()).Returns("access-token");
        _tokenService.GenerateRefreshToken().Returns(("raw-refresh", "hash-refresh"));

        var response = await service.LoginAsync(request);

        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("raw-refresh", response.RefreshToken);
        await _userRepository.Received(1).RevokeAllRefreshTokensAsync(user.Id, Arg.Any<CancellationToken>());
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldThrowUnauthorizedAccessException()
    {
        var service = CreateService();
        var request = new LoginRequest
        {
            Email = "student@example.com",
            Password = "password123"
        };

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "student@example.com",
            UserName = "student01",
            PasswordHash = "hashed-password"
        };

        _userRepository.GetByEmailAsync("student@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("password123", "hashed-password").Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(request));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldRotateTokens()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
        var request = new RefreshTokenRequest
        {
            AccessToken = "expired-access",
            RefreshToken = "raw-refresh"
        };

        var user = new AppUser
        {
            Id = userId,
            Email = "student@example.com",
            UserName = "student01",
            PasswordHash = "hashed-password"
        };
        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            User = user,
            RoleId = Guid.NewGuid(),
            Role = new AppRole { Name = "Student" }
        });

        var existingToken = new RefreshToken
        {
            UserId = userId,
            TokenHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("raw-refresh"))),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1)
        };

        _tokenService.Validate("expired-access").Returns(TokenValidationResult.Success(userId));
        _userRepository.GetActiveRefreshTokenByHashAsync(existingToken.TokenHash, Arg.Any<CancellationToken>()).Returns(existingToken);
        _userRepository.GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _tokenService.GenerateAccessToken(user, Arg.Any<IEnumerable<string>>()).Returns("new-access");
        _tokenService.GenerateRefreshToken().Returns(("new-raw-refresh", "new-hash-refresh"));

        var response = await service.RefreshTokenAsync(request);

        Assert.Equal("new-access", response.AccessToken);
        Assert.Equal("new-raw-refresh", response.RefreshToken);
        Assert.NotNull(existingToken.RevokedAtUtc);
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedOrMissingToken_ShouldThrowUnauthorizedAccessException()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
        var request = new RefreshTokenRequest
        {
            AccessToken = "expired-access",
            RefreshToken = "raw-refresh"
        };

        var tokenHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("raw-refresh")));

        _tokenService.Validate("expired-access").Returns(TokenValidationResult.Success(userId));
        _userRepository.GetActiveRefreshTokenByHashAsync(tokenHash, Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RefreshTokenAsync(request));
    }

    private AuthService CreateService()
    {
        return new AuthService(
            _userRepository,
            _tokenService,
            _passwordHasher,
            _registerValidator,
            _loginValidator,
            _refreshValidator);
    }
}
