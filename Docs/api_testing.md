# Hướng dẫn Test API và Validation

## 1. Chạy Unit Test

```powershell
dotnet test SortSchedule.slnx
```

Kỳ vọng: toàn bộ test pass (scheduling + auth + enum parser).

## 2. Khởi chạy API

```powershell
dotnet run --project SortSchedule/SortSchedule.csproj
```

API mặc định lắng nghe tại `http://localhost:5243`. Swagger UI: `http://localhost:5243/swagger`.

## 3. Test Scheduling API

### Happy path — Solve

```powershell
$body = Get-Content SortSchedule/payload3.json -Raw
Invoke-RestMethod -Method Post `
  -Uri http://localhost:5243/api/schedule/solve `
  -ContentType "application/json" -Body $body
```

Kỳ vọng: HTTP 200, response có `hardScore`, `softScore`, `schedule`.

### Fail-fast enum validation

Sửa tạm `roomType` hoặc `deliveryMode` thành giá trị sai trong payload rồi gọi lại.

Kỳ vọng: HTTP 400 (hoặc 500 nếu chưa bật middleware chuẩn hóa đầy đủ).

## 4. Test Auth API

### Register

```powershell
$register = @{
  email = "test@example.com"
  userName = "TestUser"
  password = "Password123!"
  confirmPassword = "Password123!"
} | ConvertTo-Json

Invoke-RestMethod -Method Post `
  -Uri http://localhost:5243/api/auth/register `
  -ContentType "application/json" -Body $register
```

Kỳ vọng: HTTP 200, response có `accessToken`, `refreshToken`, `userName`, `email`, `roles`.

### Login

```powershell
$login = @{
  email = "test@example.com"
  password = "Password123!"
} | ConvertTo-Json

Invoke-RestMethod -Method Post `
  -Uri http://localhost:5243/api/auth/login `
  -ContentType "application/json" -Body $login
```

### Refresh Token

```powershell
$refresh = @{
  accessToken = "<expired-access-token>"
  refreshToken = "<raw-refresh-token>"
} | ConvertTo-Json

Invoke-RestMethod -Method Post `
  -Uri http://localhost:5243/api/auth/refresh-token `
  -ContentType "application/json" -Body $refresh
```

## 5. Chuẩn lỗi API — ProblemDetails (RFC 7807)

ExceptionHandlingMiddleware chuyển exception thành JSON chuẩn:

```json
{
  "type": "https://httpstatuses.io/400",
  "title": "Validation failed",
  "status": 400,
  "traceId": "00-...",
  "errors": {
    "Email": ["'Email' is not a valid email address."]
  }
}
```

### Mapping exception → HTTP status

| Exception | Status | Khi nào |
|-----------|--------|---------|
| ValidationException (FluentValidation) | 400 | Request DTO không hợp lệ |
| UnauthorizedAccessException | 401 | Sai password, token không hợp lệ |
| InvalidOperationException chứa "already" | 409 | Email đã tồn tại |
| Exception khác | 500 | Lỗi server, không expose stack trace |

## 6. Regression checklist

- [ ] RoomType mismatch → hard penalty.
- [ ] Online có RoomId → hard penalty.
- [ ] Offline không có RoomId → hard penalty.
- [ ] Không phát sinh string comparison trong scorer cho enum constraints.
- [ ] Register email trùng → 409.
- [ ] Login sai password → 401.
- [ ] Refresh token đã revoke → 401.
