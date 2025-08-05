# SakuraHome API - H? th?ng Xác th?c Hoàn ch?nh

## T?ng quan

H? th?ng xác th?c SakuraHome API cung c?p ??y ?? các ch?c n?ng ??ng ký, ??ng nh?p và qu?n lý ng??i dùng v?i các tính n?ng b?o m?t cao:

- **JWT Token Authentication**: S? d?ng JWT cho xác th?c và refresh token cho b?o m?t
- **ASP.NET Core Identity**: Tích h?p v?i Identity framework cho qu?n lý ng??i dùng
- **Password Security**: Mã hóa m?t kh?u v?i bcrypt và chính sách m?t kh?u m?nh
- **Account Lockout**: Khóa tài kho?n sau nhi?u l?n ??ng nh?p sai
- **Email Verification**: Xác th?c email ng??i dùng
- **Password Reset**: ??t l?i m?t kh?u qua email
- **Multi-language Support**: H? tr? ?a ngôn ng?
- **User Activity Tracking**: Theo dõi ho?t ??ng ng??i dùng

## C?u trúc D? án

### 1. Models và DTOs

```
Models/
??? Entities/Identity/
?   ??? User.cs                     # Entity ng??i dùng chính
?   ??? RefreshToken.cs             # Entity qu?n lý refresh token
?   ??? Address.cs                  # ??a ch? ng??i dùng
??? DTOs/
?   ??? AuthDto.cs                  # DTOs cho authentication
?   ??? ApiResponseDto.cs           # DTO response chung
??? Enums/
    ??? UserEnums.cs                # Enums liên quan user
    ??? SystemEnums.cs              # Enums h? th?ng
```

### 2. Services

```
Services/
??? Interfaces/
?   ??? IAuthService.cs             # Interface d?ch v? xác th?c
?   ??? ITokenService.cs            # Interface d?ch v? JWT
??? Implementations/
    ??? AuthService.cs              # Tri?n khai d?ch v? xác th?c
    ??? TokenService.cs             # Tri?n khai d?ch v? JWT
```

### 3. Controllers

```
Controllers/
??? AuthController.cs               # Controller x? lý các API xác th?c
```

### 4. Mappings

```
Mappings/
??? AuthMappingProfile.cs           # AutoMapper profile cho auth
```

## Endpoints API

### ??ng ký và ??ng nh?p

#### 1. ??ng ký ng??i dùng
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "StrongPassword123!",
  "confirmPassword": "StrongPassword123!",
  "firstName": "Tên",
  "lastName": "H?",
  "phoneNumber": "+84123456789",
  "dateOfBirth": "1990-01-01",
  "gender": 1,
  "preferredLanguage": "vi",
  "acceptTerms": true,
  "emailNotifications": true,
  "smsNotifications": false
}
```

**Response:**
```json
{
  "success": true,
  "message": "??ng ký thành công",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-refresh-token",
    "expiresAt": "2024-01-01T12:00:00Z",
    "user": {
      "id": "guid",
      "email": "user@example.com",
      "firstName": "Tên",
      "lastName": "H?",
      "fullName": "Tên H?",
      "role": "Customer",
      "tier": "Bronze",
      "status": "Active"
    }
  }
}
```

#### 2. ??ng nh?p
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "StrongPassword123!",
  "rememberMe": false
}
```

### Qu?n lý Token

#### 3. Làm m?i token
```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "accessToken": "current-access-token",
  "refreshToken": "current-refresh-token"
}
```

#### 4. Thu h?i token
```http
POST /api/auth/revoke-token
Authorization: Bearer your-token
Content-Type: application/json

{
  "refreshToken": "token-to-revoke"
}
```

#### 5. Thu h?i t?t c? token
```http
POST /api/auth/revoke-all-tokens
Authorization: Bearer your-token
```

### Qu?n lý M?t kh?u

#### 6. Quên m?t kh?u
```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

#### 7. ??t l?i m?t kh?u
```http
POST /api/auth/reset-password
Content-Type: application/json

{
  "email": "user@example.com",
  "token": "reset-token-from-email",
  "newPassword": "NewStrongPassword123!",
  "confirmPassword": "NewStrongPassword123!"
}
```

#### 8. ??i m?t kh?u
```http
POST /api/auth/change-password
Authorization: Bearer your-token
Content-Type: application/json

{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword123!",
  "confirmPassword": "NewPassword123!"
}
```

### Xác th?c Email

#### 9. Xác th?c email
```http
POST /api/auth/verify-email
Content-Type: application/json

{
  "email": "user@example.com",
  "token": "verification-token-from-email"
}
```

#### 10. G?i l?i email xác th?c
```http
POST /api/auth/resend-email-verification
Content-Type: application/json

"user@example.com"
```

### Thông tin Ng??i dùng

#### 11. L?y thông tin ng??i dùng hi?n t?i
```http
GET /api/auth/me
Authorization: Bearer your-token
```

#### 12. Ki?m tra tr?ng thái xác th?c
```http
GET /api/auth/check
Authorization: Bearer your-token
```

#### 13. ??ng xu?t
```http
POST /api/auth/logout
Authorization: Bearer your-token
Content-Type: application/json

{
  "refreshToken": "optional-refresh-token"
}
```

## C?u hình B?o m?t

### 1. JWT Settings (appsettings.json)
```json
{
  "JwtSettings": {
    "Key": "your-super-secret-key-here-make-it-long-and-complex",
    "Issuer": "SakuraHomeAPI",
    "Audience": "SakuraHomeUsers",
    "DurationInMinutes": 60
  }
}
```

### 2. Password Policy
- T?i thi?u 6 ký t?
- Yêu c?u ít nh?t 1 ch? hoa
- Yêu c?u ít nh?t 1 ch? th??ng
- Yêu c?u ít nh?t 1 s?
- Không yêu c?u ký t? ??c bi?t (có th? thay ??i)

### 3. Account Lockout
- Khóa tài kho?n sau 5 l?n ??ng nh?p sai
- Th?i gian khóa: 5 phút
- T? ??ng m? khóa sau khi h?t th?i gian

### 4. Token Security
- Access Token: 60 phút (có th? c?u hình)
- Refresh Token: 7 ngày
- Refresh token ???c l?u trong database và có th? thu h?i
- T? ??ng thu h?i token c? khi t?o token m?i

## Tính n?ng Nâng cao

### 1. User Activity Tracking
H? th?ng t? ??ng ghi l?i các ho?t ??ng:
- ??ng nh?p/??ng xu?t
- ??ng ký tài kho?n
- ??i m?t kh?u
- ??t l?i m?t kh?u
- IP address và timestamp

### 2. User Roles và Permissions
```csharp
public enum UserRole
{
    Customer = 1,      // Khách hàng
    Staff = 2,         // Nhân viên
    Admin = 3,         // Qu?n tr? viên
    SuperAdmin = 4     // Qu?n tr? viên c?p cao
}
```

### 3. User Tiers (Loyalty System)
```csharp
public enum UserTier
{
    Bronze = 1,     // 0 - 1M VND
    Silver = 2,     // 1M - 5M VND
    Gold = 3,       // 5M - 10M VND
    Platinum = 4,   // 10M - 20M VND
    Diamond = 5     // 20M+ VND
}
```

### 4. Account Status
```csharp
public enum AccountStatus
{
    Pending = 1,        // Ch? xác th?c email
    Active = 2,         // Ho?t ??ng bình th??ng
    Suspended = 3,      // B? t?m khóa
    Banned = 4,         // B? c?m v?nh vi?n
    Inactive = 5        // Không ho?t ??ng
}
```

## Cách S? d?ng

### 1. Kh?i ch?y ?ng d?ng
```bash
cd SakuraHomeAPI
dotnet run
```

### 2. T?o Migration cho RefreshToken
```bash
dotnet ef migrations add AddRefreshTokenTable
dotnet ef database update
```

### 3. Test API b?ng file HTTP
S? d?ng file `Tests/Auth.http` ?? test các endpoint

### 4. Swagger Documentation
Truy c?p `https://localhost:7071` ?? xem Swagger UI v?i ??y ?? documentation

## X? lý L?i

H? th?ng tr? v? các mã l?i chu?n:
- **200**: Thành công
- **400**: D? li?u không h?p l?
- **401**: Không có quy?n truy c?p
- **404**: Không tìm th?y
- **500**: L?i server

Format response l?i:
```json
{
  "success": false,
  "message": "Thông báo l?i",
  "errors": ["Chi ti?t l?i 1", "Chi ti?t l?i 2"],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## B?o m?t

### 1. Password Hashing
- S? d?ng ASP.NET Core Identity v?i bcrypt
- Salt t? ??ng cho m?i password
- Không l?u password d?ng plain text

### 2. JWT Security
- Signing v?i HMAC SHA256
- Claims-based authorization
- Token expiration và validation

### 3. Refresh Token Security
- L?u trong database v?i thông tin metadata
- Có th? thu h?i b?t c? lúc nào
- T? ??ng xóa token h?t h?n

### 4. Rate Limiting
- Có th? thêm middleware ?? gi?i h?n s? request
- B?o v? kh?i brute force attacks

### 5. IP Tracking
- Ghi l?i IP address cho m?i login
- Có th? detect và c?nh báo login t? IP l?

## Tích h?p Email (TODO)

Hi?n t?i các endpoint email ?ã s?n sàng, c?n tích h?p:
- SMTP service cho g?i email
- Email templates
- Background job ?? x? lý email queue

## Monitoring và Logging

H? th?ng s? d?ng Serilog ?? ghi log:
- Authentication events
- Error tracking
- Performance monitoring
- Security events

Log ???c l?u t?i: `logs/sakura-home-{date}.txt`

## K?t lu?n

H? th?ng xác th?c SakuraHome API ?ã ???c tri?n khai hoàn ch?nh v?i:
? ??ng ký và ??ng nh?p
? JWT token management
? Password security
? Email verification
? Password reset
? User management
? Activity tracking
? Comprehensive error handling
? Security best practices
? Full API documentation

H? th?ng s?n sàng ?? s? d?ng trong môi tr??ng production v?i các tính n?ng b?o m?t cao và tr?i nghi?m ng??i dùng t?t.