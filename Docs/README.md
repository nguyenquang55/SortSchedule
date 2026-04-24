# SortSchedule — Tài liệu dự án

Hệ thống sắp lịch học tự động sử dụng Clean Architecture, .NET 10, EF Core + SQL Server, JWT Authentication.

## Mục lục tài liệu

| File | Nội dung |
|------|----------|
| [architecture.md](architecture.md) | Kiến trúc hệ thống, cấu trúc thư mục, phân tách layer, DI, dependency rule |
| [algorithms.md](algorithms.md) | Thuật toán (FirstFit + TabuSearch), ScheduleScorer, hard/soft constraints |
| [persistence_auth.md](persistence_auth.md) | Persistence Layer (EF Core), JWT Authentication, entity, migration |
| [api_testing.md](api_testing.md) | Hướng dẫn chạy test, test API scheduling + auth, chuẩn lỗi ProblemDetails |

## Quick Start

```powershell
# 1. Áp dụng migration database
dotnet ef database update `
    --project Infrastructure/SortSchedule.Infrastructure.csproj `
    --startup-project SortSchedule/SortSchedule.csproj

# 2. Chạy API
dotnet run --project SortSchedule/SortSchedule.csproj

# 3. Chạy unit test
dotnet test SortSchedule.slnx
```

API: `http://localhost:5243` — Swagger UI: `http://localhost:5243/swagger`

## Tech Stack

- **.NET 10** (net10.0)
- **EF Core 10** + SQL Server
- **JWT** (HMACSHA512) + BCrypt password hashing
- **FluentValidation** 12.x
- **xUnit** + NSubstitute
- **Swashbuckle** (Swagger)
