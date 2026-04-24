using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SortSchedule.Application.Abstractions;
using SortSchedule.Application.Abstractions.Auth;
using SortSchedule.Application.DTOs.Auth;
using SortSchedule.Application.Options;
using SortSchedule.Application.Services;
using SortSchedule.Application.Validators;
using SortSchedule.Infrastructure.Auth;
using SortSchedule.Infrastructure.Persistence;
using SortSchedule.Infrastructure.Persistence.Repositories;
using SortSchedule.Infrastructure.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SortSchedule.Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddScheduleServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TabuSearchOptions>(configuration.GetSection("TabuSearch"));

        services.AddSingleton<IScheduleScenarioRepository, InMemoryScheduleScenarioRepository>();

        services.AddSingleton<IScheduleScorer, ScheduleScorer>();
        services.AddTransient<FirstFitSolver>();
        services.AddTransient<TabuSearchSolver>(serviceProvider =>
            new TabuSearchSolver(
                serviceProvider.GetRequiredService<IScheduleScorer>(),
                serviceProvider.GetRequiredService<IOptions<TabuSearchOptions>>().Value));
        services.AddTransient<IScheduleOrchestrator, ScheduleOrchestrator>();

        return services;
    }

    public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RefreshTokenRequest>, RefreshTokenRequestValidator>();

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }
}
