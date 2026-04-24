# Kiến trúc hệ thống SortSchedule (Clean Architecture)

Tài liệu này phản ánh trạng thái hiện tại của hệ thống sau khi tích hợp đầy đủ:
- Hard constraints theo enum cho RoomType và DeliveryMode.
- Persistence Layer (EF Core + SQL Server).
- JWT Authentication (Register, Login, Refresh Token).
- Exception Handling Middleware.

## Cấu trúc thư mục hiện tại

```text
SortSchedule/
|- Application/
|  |- Abstractions/
|  |  |- Auth/
|  |  |  |- IAuthService.cs
|  |  |  |- IPasswordHasher.cs
|  |  |  |- ITokenService.cs
|  |  |  `- IUserRepository.cs
|  |  |- IScheduleOrchestrator.cs
|  |  |- IScheduleScenarioRepository.cs
|  |  |- IScheduleScorer.cs
|  |  `- IScheduleSolver.cs
|  |- DTOs/
|  |  `- Auth/
|  |     |- AuthResponse.cs
|  |     |- LoginRequest.cs
|  |     |- RefreshTokenRequest.cs
|  |     |- RegisterRequest.cs
|  |     `- TokenValidationResult.cs
|  |- Entities/
|  |  `- ScheduleMove.cs
|  |- Options/
|  |  `- TabuSearchOptions.cs
|  |- Services/
|  |  |- AuthService.cs
|  |  |- FirstFitSolver.cs
|  |  |- ScheduleOrchestrator.cs
|  |  |- ScheduleScorer.cs
|  |  `- TabuSearchSolver.cs
|  |- Validators/
|  |  |- LoginRequestValidator.cs
|  |  |- RefreshTokenRequestValidator.cs
|  |  `- RegisterRequestValidator.cs
|  `- SortSchedule.Application.csproj
|- Domain/
|  |- Common/
|  |  `- HardSoftScore.cs
|  |- Entities/
|  |  |- AppRole.cs
|  |  |- AppUser.cs
|  |  |- Lesson.cs
|  |  |- RefreshToken.cs
|  |  |- Room.cs
|  |  |- Schedule.cs
|  |  |- StudentGroup.cs
|  |  |- Subject.cs
|  |  |- Teacher.cs
|  |  |- TimeSlot.cs
|  |  `- UserRole.cs
|  |- Enums/
|  |  |- DeliveryMode.cs
|  |  `- RoomType.cs
|  `- SortSchedule.Domain.csproj
|- Infrastructure/
|  |- Auth/
|  |  |- JwtSettings.cs
|  |  |- PasswordHasher.cs
|  |  `- TokenService.cs
|  |- DI/
|  |  `- DependencyInjection.cs
|  |- Persistence/
|  |  |- AppDbContext.cs
|  |  |- Configurations/
|  |  |  |- AppRoleConfiguration.cs
|  |  |  |- AppUserConfiguration.cs
|  |  |  |- RefreshTokenConfiguration.cs
|  |  |  `- UserRoleConfiguration.cs
|  |  |- Migrations/
|  |  |  |- 20260423155702_InitialAuth.cs
|  |  |  |- 20260423155702_InitialAuth.Designer.cs
|  |  |  `- AppDbContextModelSnapshot.cs
|  |  `- Repositories/
|  |     `- UserRepository.cs
|  |- Repositories/
|  |  `- InMemoryScheduleScenarioRepository.cs
|  `- SortSchedule.Infrastructure.csproj
|- SortSchedule/                              (Presentation / API Layer)
|  |- Controllers/
|  |  |- AuthController.cs
|  |  `- ScheduleController.cs
|  |- Contracts/
|  |  |- ScheduleContracts.cs
|  |  `- ScheduleMappings.cs
|  |- Extensions/
|  |  `- EnumParser.cs
|  |- Middleware/
|  |  `- ExceptionHandlingMiddleware.cs
|  |- Properties/
|  |  `- launchSettings.json
|  |- Program.cs
|  |- appsettings.json
|  |- appsettings.Development.json
|  |- SortSchedule.csproj
|  |- payload.json
|  |- payload2.json
|  `- payload3.json
|- SortSchedule.Tests/
|  |- Application/
|  |  |- FirstFitSolverTests.cs
|  |  |- ScheduleScorerTests.cs
|  |  |- Scoring/
|  |  |  `- ConstraintTests.cs
|  |  |- TabuSearchSolverTests.cs
|  |  `- TestScheduleFactory.cs
|  |- Auth/
|  |  |- AuthServiceTests.cs
|  |  `- TokenServiceTests.cs
|  |- Contracts/
|  |  `- EnumParserTests.cs
|  |- GlobalUsings.cs
|  `- SortSchedule.Tests.csproj
|- Docs/
|  |- README.md               (← tài liệu tổng hợp — file này)
|  |- architecture.md         (cấu trúc và kiến trúc hệ thống)
|  |- algorithms.md            (thuật toán và cơ chế chấm điểm)
|  |- persistence_auth.md      (persistence + JWT authentication)
|  `- api_testing.md           (hướng dẫn test API và validation)
`- SortSchedule.slnx
```

## Phân tách layer và trách nhiệm

### Domain (SortSchedule.Domain)

- **Entities lịch học:** Lesson, Teacher, Room, Schedule, StudentGroup, Subject, TimeSlot.
- **Entities xác thực:** AppUser, AppRole, UserRole, RefreshToken.
- **Common:** HardSoftScore (kiểu điểm hard/soft dùng cho scorer).
- **Enums:** RoomType (Theory, Practice), DeliveryMode (Offline, Online).
- Không phụ thuộc vào Application, Infrastructure hay API.

### Application (SortSchedule.Application)

- **Abstractions:** Interfaces nghiệp vụ — IScheduleOrchestrator, IScheduleSolver, IScheduleScorer, IScheduleScenarioRepository.
- **Abstractions/Auth:** Interfaces xác thực — IAuthService, ITokenService, IPasswordHasher, IUserRepository.
- **DTOs/Auth:** Request/Response DTO cho auth — RegisterRequest, LoginRequest, RefreshTokenRequest, AuthResponse, TokenValidationResult.
- **Entities:** ScheduleMove (model di chuyển dùng trong Tabu Search).
- **Options:** TabuSearchOptions (cấu hình Tabu Search từ appsettings).
- **Services:** Các use case — AuthService, FirstFitSolver, TabuSearchSolver, ScheduleScorer, ScheduleOrchestrator.
- **Validators:** FluentValidation — RegisterRequestValidator, LoginRequestValidator, RefreshTokenRequestValidator.
- **Package:** FluentValidation 12.x.
- Chỉ phụ thuộc vào Domain.

### Infrastructure (SortSchedule.Infrastructure)

- **Auth:** JwtSettings (options), PasswordHasher (BCrypt), TokenService (JWT + SHA256 refresh hash).
- **DI:** DependencyInjection — AddScheduleServices + AddAuthServices.
- **Persistence:** AppDbContext (EF Core), Configurations (Fluent API), Migrations, Repositories/UserRepository.
- **Repositories:** InMemoryScheduleScenarioRepository (lưu trữ scenario trong bộ nhớ).
- **Packages:** EF Core SqlServer 10.x, EF Core Tools 10.x, BCrypt.Net-Next 4.x, JwtBearer 10.x.
- Phụ thuộc vào Application và Domain.

### Presentation/API (SortSchedule)

- **Controllers:** AuthController (register/login/refresh), ScheduleController (solve/scenario CRUD).
- **Contracts:** ScheduleContracts (DTO), ScheduleMappings (DTO ↔ Domain).
- **Extensions:** EnumParser (parse fail-fast cho enum).
- **Middleware:** ExceptionHandlingMiddleware (RFC 7807 ProblemDetails).
- **Program.cs:** Composition root — DI, Swagger + JWT security, Authentication/Authorization middleware.
- **Packages:** JwtBearer 10.x, EF Core Design 10.x, Swashbuckle 9.x.
- Phụ thuộc vào Application, Domain, Infrastructure.

### Tests (SortSchedule.Tests)

- **Application/:** Unit test cho solver và scorer — FirstFitSolverTests, ScheduleScorerTests, TabuSearchSolverTests, TestScheduleFactory.
- **Application/Scoring/:** ConstraintTests (RoomType/DeliveryMode hard constraints).
- **Auth/:** AuthServiceTests, TokenServiceTests.
- **Contracts/:** EnumParserTests (trim, ignore-case, normalize, fail-fast).
- **Packages:** xUnit 2.9, NSubstitute 5.x, coverlet 6.x.
- Tham chiếu Application, Domain, SortSchedule.

## Luồng xử lý dữ liệu

### Luồng Solve (sắp lịch)

1. Client gửi `ScheduleDto` vào endpoint `POST /api/schedule/solve`.
2. `ScheduleMappings.ToDomain` chuyển DTO sang Domain model.
3. Các trường enum trong `RoomDto` và `LessonDto` được parse strict bằng `EnumParser`:
   - `Normalize(FormC)` → `Trim()` → `Enum.Parse(ignoreCase: true)`.
   - Giá trị không hợp lệ ném exception ngay (fail-fast).
4. `ScheduleOrchestrator` chạy `FirstFitSolver` rồi `TabuSearchSolver`.
5. `ScheduleScorer` tính hard/soft score và trả về kết quả.
6. Kết quả map ngược sang DTO để trả response.

### Luồng Auth (xác thực)

1. **Register:** `POST /api/auth/register` → validate → hash password → gán role Student → tạo token pair → trả AuthResponse.
2. **Login:** `POST /api/auth/login` → verify email/password → revoke old tokens → tạo token pair mới → trả AuthResponse.
3. **Refresh:** `POST /api/auth/refresh-token` → validate expired access token → verify refresh token hash → rotate token pair → trả AuthResponse.

## Quy tắc phụ thuộc (Dependency Rule)

- Hướng phụ thuộc hợp lệ:
  - SortSchedule → Infrastructure → Application → Domain
  - SortSchedule → Application
  - SortSchedule → Domain
  - SortSchedule.Tests → Application, Domain, SortSchedule
- Hướng phụ thuộc không hợp lệ:
  - Domain phụ thuộc Application hoặc Infrastructure
  - Application phụ thuộc Infrastructure

## Project references hiện tại

- **Application/SortSchedule.Application.csproj**
  - Reference: Domain/SortSchedule.Domain.csproj
  - Package: FluentValidation 12.0.0

- **Infrastructure/SortSchedule.Infrastructure.csproj**
  - Reference: Application/SortSchedule.Application.csproj, Domain/SortSchedule.Domain.csproj
  - Package: BCrypt.Net-Next 4.0.3, JwtBearer 10.0.0, EF Core SqlServer 10.0.0, EF Core Tools 10.0.0
  - Framework: Microsoft.AspNetCore.App

- **SortSchedule/SortSchedule.csproj**
  - Reference: Application, Domain, Infrastructure
  - Package: JwtBearer 10.0.0, EF Core Design 10.0.7, Swashbuckle 9.0.6

- **SortSchedule.Tests/SortSchedule.Tests.csproj**
  - Reference: Application, Domain, SortSchedule
  - Package: xUnit 2.9.0, NSubstitute 5.3.0, coverlet 6.0.2, NET.Test.Sdk 17.14.1

## DI và cấu hình

### AddScheduleServices (scheduling)
- `TabuSearchOptions` bind từ section `TabuSearch` trong appsettings.json.
- `IScheduleScenarioRepository` → `InMemoryScheduleScenarioRepository` (Singleton).
- `IScheduleScorer` → `ScheduleScorer` (Singleton).
- `FirstFitSolver` (Transient), `TabuSearchSolver` (Transient).
- `IScheduleOrchestrator` → `ScheduleOrchestrator` (Transient).

### AddAuthServices (authentication)
- `AppDbContext` → SQL Server (connection string `DefaultConnection`).
- `JwtSettings` bind từ section `JwtSettings`.
- `IUserRepository` → `UserRepository` (Scoped).
- `IPasswordHasher` → `PasswordHasher` (Scoped).
- `ITokenService` → `TokenService` (Scoped).
- `IAuthService` → `AuthService` (Scoped).
- `IValidator<RegisterRequest>` → `RegisterRequestValidator` (Scoped).
- `IValidator<LoginRequest>` → `LoginRequestValidator` (Scoped).
- `IValidator<RefreshTokenRequest>` → `RefreshTokenRequestValidator` (Scoped).
- JWT Bearer Authentication + Authorization.

### Middleware pipeline (Program.cs)
1. `UseSwagger` + `UseSwaggerUI` (Development only).
2. `UseMiddleware<ExceptionHandlingMiddleware>()`.
3. `UseAuthentication()`.
4. `UseAuthorization()`.
5. `MapControllers()`.

## Chuẩn dữ liệu enum qua API

- RoomType hợp lệ: `Theory`, `Practice`.
- DeliveryMode hợp lệ: `Offline`, `Online`.
- Payload mẫu đã đồng bộ theo chuẩn này (payload.json, payload2.json, payload3.json).

## Cấu hình appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=...;Initial Catalog=SortSchedule;..."
  },
  "TabuSearch": {
    "TabuTenure": 50,
    "MaxIterations": 5000,
    "NeighborhoodSize": 300
  },
  "JwtSettings": {
    "Key": "REPLACE-IN-ENVIRONMENT-WITH-AT-LEAST-64-BYTES-OF-RANDOM-DATA",
    "Issuer": "SortSchedule",
    "Audience": "SortSchedule",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  }
}
```

> **Lưu ý:** `JwtSettings.Key` trong appsettings.json chỉ dùng cho development. Production phải dùng environment variable hoặc secret manager.
