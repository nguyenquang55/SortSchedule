using SortSchedule.Infrastructure.DI;
using SortSchedule.Middleware;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5243");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//
// CORS FIX (rất quan trọng)
//
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            // Dev nhanh nhất: cho phép tất cả
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();

        /*
        Production nên dùng kiểu này thay vì AllowAnyOrigin():

        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://membership-retain-buses-plaza.trycloudflare.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        */
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Input a valid JWT access token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScheduleServices(builder.Configuration);
builder.Services.AddAuthServices(builder.Configuration);

var app = builder.Build();

//
// Swagger
//
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//
// Middleware order cực quan trọng
//

app.UseMiddleware<ExceptionHandlingMiddleware>();

// CORS phải đứng trước Auth
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Optional: mở root domain khỏi bị 404
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();