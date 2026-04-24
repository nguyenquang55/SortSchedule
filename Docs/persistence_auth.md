# Persistence Layer (EF Core + SQL Server) và JWT Authentication

Tài liệu này mô tả hệ thống persistence và xác thực JWT **đã triển khai** trong SortSchedule.

## 1. Domain Entities

Tất cả entity là POCO thuần trong `Domain/Entities/`, không phụ thuộc thư viện ngoài.

### AppUser
- `Id`: Guid (NewGuid mặc định).
- `UserName`, `Email`, `PasswordHash`: string.
- `CreatedAtUtc`: DateTime. `LastLoginAtUtc`: DateTime?.
- Navigation: `ICollection<UserRole>`, `ICollection<RefreshToken>`.

### AppRole
- `Id`: Guid. `Name`: string (Admin, Lecturer, Student).
- Navigation: `ICollection<UserRole>`.

### UserRole (join entity User ↔ Role)
- Composite key: `UserId` + `RoleId`.

### RefreshToken
- `Id`: Guid. `TokenHash`: string (SHA256 hash, không lưu raw).
- `ExpiresAtUtc`, `CreatedAtUtc`: DateTime. `RevokedAtUtc`: DateTime?.
- Computed (không map DB): `IsExpired`, `IsRevoked`, `IsActive`.
- FK: `UserId` → AppUser.

## 2. Application Layer

### Interfaces (`Abstractions/Auth/`)

| Interface | Phương thức chính |
|-----------|-------------------|
| IPasswordHasher | Hash, Verify |
| ITokenService | GenerateAccessToken, GenerateRefreshToken, Validate → TokenValidationResult |
| IUserRepository | GetByEmailAsync, ExistsAsync, AddAsync, GetRoleByNameAsync, GetActiveRefreshTokenByHashAsync, RevokeAllRefreshTokensAsync, SaveChangesAsync |
| IAuthService | RegisterAsync, LoginAsync, RefreshTokenAsync |

### DTOs (`DTOs/Auth/`)
- RegisterRequest: Email, UserName, Password, ConfirmPassword.
- LoginRequest: Email, Password.
- RefreshTokenRequest: AccessToken (expired), RefreshToken (raw).
- AuthResponse: AccessToken, RefreshToken, ExpiresAtUtc, UserName, Email, Roles.
- TokenValidationResult: record với IsValid, UserId?, Error?, static Success/Failure.

### AuthService (`Services/AuthService.cs`)

**Register:** validate → check email trùng → hash password → gán role Student → tạo token pair → lưu.
**Login:** validate → verify email/password → revoke old tokens → update LastLoginAtUtc → tạo token pair mới.
**Refresh:** validate expired access token → SHA256(input refresh) → find active token → revoke → rotate.

### Validators (`Validators/`)
- RegisterRequestValidator: Email (NotEmpty, EmailAddress, Max 256), UserName (Min 3, Max 100), Password (Min 8, Max 128), ConfirmPassword (Equal).
- LoginRequestValidator: Email (NotEmpty, EmailAddress), Password (NotEmpty).
- RefreshTokenRequestValidator: AccessToken (NotEmpty), RefreshToken (NotEmpty).

## 3. Infrastructure Layer

### EF Core (`Persistence/`)

**AppDbContext:** DbSet Users, Roles, UserRoles, RefreshTokens. ApplyConfigurationsFromAssembly.

**Configurations:** Fluent API cho 4 bảng — HasKey, HasIndex, HasData (3 roles seed), Ignore computed props.

**SQL Schema:**

```sql
CREATE TABLE [AppUsers] (
    [Id] UNIQUEIDENTIFIER PK, [UserName] NVARCHAR(100),
    [Email] NVARCHAR(256) UNIQUE, [PasswordHash] NVARCHAR(512),
    [CreatedAtUtc] DATETIME2, [LastLoginAtUtc] DATETIME2 NULL
);
CREATE TABLE [AppRoles] (
    [Id] UNIQUEIDENTIFIER PK, [Name] NVARCHAR(50) UNIQUE
);
CREATE TABLE [UserRoles] (
    [UserId] + [RoleId] composite PK, FK cascade
);
CREATE TABLE [RefreshTokens] (
    [Id] UNIQUEIDENTIFIER PK, [TokenHash] NVARCHAR(128) indexed,
    [ExpiresAtUtc] DATETIME2, [CreatedAtUtc] DATETIME2,
    [RevokedAtUtc] DATETIME2 NULL, [UserId] FK cascade
);
```

**UserRepository:** Include UserRoles→Role, RefreshTokens. Query active tokens by hash.

**Migration:** `20260423155702_InitialAuth` — tạo 4 bảng + seed 3 roles.

### Auth (`Auth/`)

- **JwtSettings:** Key, Issuer, Audience, AccessTokenExpiryMinutes, RefreshTokenExpiryDays.
- **TokenService:** JWT HMACSHA512, refresh = 64 random bytes → Base64 raw + SHA256 hash.
- **PasswordHasher:** BCrypt.EnhancedHashPassword (workFactor: 12).

### DI (`DI/DependencyInjection.cs`)

AddAuthServices đăng ký: DbContext, JwtSettings, UserRepository, PasswordHasher, TokenService, AuthService, 3 Validators, JWT Bearer Authentication, Authorization.

## 4. API Layer

### AuthController (`Controllers/AuthController.cs`)
- `POST /api/auth/register` → RegisterAsync → Ok(AuthResponse).
- `POST /api/auth/login` → LoginAsync → Ok(AuthResponse).
- `POST /api/auth/refresh-token` → RefreshTokenAsync → Ok(AuthResponse).

### ExceptionHandlingMiddleware (`Middleware/ExceptionHandlingMiddleware.cs`)
- RFC 7807 ProblemDetails với traceId.
- ValidationException → 400, UnauthorizedAccessException → 401, "already" → 409, khác → 500.

## 5. Bảo mật Refresh Token

- DB chỉ lưu SHA256(rawToken).
- Client nhận raw token một lần duy nhất khi login/refresh.
- Mỗi login mới revoke toàn bộ refresh token cũ (single session).
- Refresh flow: validate expired access → SHA256(input) → find active → revoke → rotate.

## 6. Migration commands

```powershell
# Tạo migration
dotnet ef migrations add <Name> `
    --project Infrastructure/SortSchedule.Infrastructure.csproj `
    --startup-project SortSchedule/SortSchedule.csproj `
    --output-dir Persistence/Migrations

# Áp dụng
dotnet ef database update `
    --project Infrastructure/SortSchedule.Infrastructure.csproj `
    --startup-project SortSchedule/SortSchedule.csproj
```

## 7. Packages

| Project | Package | Version |
|---------|---------|---------|
| Application | FluentValidation | 12.0.0 |
| Infrastructure | Microsoft.EntityFrameworkCore.SqlServer | 10.0.0 |
| Infrastructure | Microsoft.EntityFrameworkCore.Tools | 10.0.0 |
| Infrastructure | BCrypt.Net-Next | 4.0.3 |
| Infrastructure | Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.0 |
| SortSchedule | Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.0 |
| SortSchedule | Microsoft.EntityFrameworkCore.Design | 10.0.7 |
| SortSchedule | Swashbuckle.AspNetCore | 9.0.6 |
| Tests | xUnit | 2.9.0 |
| Tests | NSubstitute | 5.3.0 |
| Tests | coverlet.collector | 6.0.2 |
