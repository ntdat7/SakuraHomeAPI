# ?? H??ng d?n Test Authentication API

## T?ng quan

Tài li?u này h??ng d?n chi ti?t cách test h? th?ng authentication c?a SakuraHome API s? d?ng nhi?u ph??ng pháp khác nhau.

## ?? Chu?n b?

### 1. Kh?i ch?y ?ng d?ng

```bash
cd SakuraHomeAPI
dotnet run
```

?ng d?ng s? ch?y trên:
- HTTPS: `https://localhost:7240`
- HTTP: `http://localhost:5273`

### 2. T?o và update database

```bash
# T?o migration cho RefreshToken (n?u ch?a có)
dotnet ef migrations add AddRefreshTokenTable

# Update database
dotnet ef database update
```

### 3. Ki?m tra c?u hình JWT

??m b?o `appsettings.json` có c?u hình JWT ?úng:

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

## ?? Ph??ng pháp Test

### Ph??ng pháp 1: S? d?ng HTTP Files (Khuyên dùng)

#### A. VS Code v?i REST Client Extension

1. Cài ??t extension "REST Client" trong VS Code
2. M? file `Tests/Auth.http`
3. Click vào "Send Request" cho t?ng endpoint

#### B. Visual Studio

1. M? file `Tests/Auth.http`
2. Click vào icon ?? bên c?nh m?i request
3. Xem k?t qu? trong tab "Web API"

#### C. JetBrains IDEs (Rider, IntelliJ)

1. M? file `Tests/Auth.http`
2. Click vào icon ?? ?? run request
3. K?t qu? hi?n trong panel d??i

### Ph??ng pháp 2: PowerShell Script

```powershell
# Ch?y script t? ??ng test t?t c? endpoints
cd SakuraHomeAPI/Tests
.\Auth-Test.ps1

# Ho?c v?i custom parameters
.\Auth-Test.ps1 -BaseUrl "https://localhost:7240" -TestEmail "custom@test.com" -TestPassword "CustomPass123!"
```

### Ph??ng pháp 3: Swagger UI

1. Truy c?p `https://localhost:7240`
2. Swagger UI s? hi?n th? t?t c? endpoints
3. Test tr?c ti?p trên web interface

### Ph??ng pháp 4: Postman

1. Import file `Tests/Auth.http` vào Postman
2. Ho?c t?o collection m?i v?i các endpoints
3. S? d?ng environment variables cho BaseURL và tokens

### Ph??ng pháp 5: cURL

```bash
# Registration
curl -X POST "https://localhost:7240/api/auth/register" \
-H "Content-Type: application/json" \
-d '{
  "email": "test@example.com",
  "password": "Test123!",
  "confirmPassword": "Test123!",
  "firstName": "John",
  "lastName": "Doe",
  "acceptTerms": true
}'

# Login
curl -X POST "https://localhost:7240/api/auth/login" \
-H "Content-Type: application/json" \
-d '{
  "email": "test@example.com",
  "password": "Test123!",
  "rememberMe": false
}'
```

## ?? Test Scenarios

### 1. Happy Path Testing

**Th? t? th?c hi?n:**

1. **Register** ? T?o user m?i
2. **Login** ? L?y access token
3. **Get Me** ? Verify token ho?t ??ng
4. **Check Auth** ? Verify authentication status
5. **Refresh Token** ? Test token refresh
6. **Change Password** ? Test password change
7. **Logout** ? Test logout

### 2. Error Testing

**Test các tr??ng h?p l?i:**

1. **Invalid Email Format**
   ```json
   {
     "email": "invalid-email",
     "password": "Test123!"
   }
   ```

2. **Weak Password**
   ```json
   {
     "email": "test@example.com",
     "password": "123"
   }
   ```

3. **Password Mismatch**
   ```json
   {
     "password": "Test123!",
     "confirmPassword": "Different123!"
   }
   ```

4. **Duplicate Email Registration**
5. **Wrong Password Login**
6. **Non-existent User Login**
7. **Invalid Token Access**
8. **Expired Token Access**

### 3. Security Testing

1. **Brute Force Protection**
   - Th? login sai nhi?u l?n
   - Verify account b? lock

2. **Token Security**
   - Verify token expiration
   - Test token revocation
   - Test refresh token rotation

3. **Input Validation**
   - SQL injection attempts
   - XSS attempts
   - Long input strings

## ?? K?t qu? Expected

### ? Success Response Format

```json
{
  "success": true,
  "message": "??ng nh?p thành công",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-refresh-token",
    "expiresAt": "2024-01-01T13:00:00Z",
    "user": {
      "id": "guid-here",
      "email": "test@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "fullName": "John Doe",
      "role": "Customer",
      "tier": "Bronze",
      "status": "Active",
      "emailVerified": true,
      "phoneVerified": false
    }
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### ? Error Response Format

```json
{
  "success": false,
  "message": "D? li?u không h?p l?",
  "errors": [
    "Email là b?t bu?c",
    "M?t kh?u ph?i có ít nh?t 6 ký t?"
  ],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## ?? Monitoring & Debugging

### 1. Log Files

Ki?m tra logs t?i: `logs/sakura-home-{date}.txt`

```
[12:00:00 INF] Attempting login for email: test@example.com
[12:00:01 INF] Successful login for user: {UserId}
[12:00:02 WRN] Failed login attempt for user: {UserId}
```

### 2. Database Inspection

**User Activities Table:**
```sql
SELECT * FROM UserActivities 
WHERE UserId = 'your-user-id' 
ORDER BY CreatedAt DESC;
```

**Refresh Tokens Table:**
```sql
SELECT * FROM RefreshTokens 
WHERE UserId = 'your-user-id' 
AND IsActive = 1;
```

**User Table:**
```sql
SELECT Id, Email, FirstName, LastName, Status, Role, 
       LastLoginAt, FailedLoginAttempts, LockoutEnd
FROM Users 
WHERE Email = 'test@example.com';
```

### 3. Performance Monitoring

**Response Times:**
- Registration: < 500ms
- Login: < 200ms
- Token Refresh: < 100ms
- Get User Info: < 100ms

**Concurrent Users:**
- Test v?i nhi?u user ??ng th?i
- Monitor memory usage
- Check database connections

## ?? Common Issues & Solutions

### Issue 1: Certificate Error (HTTPS)

**Error:** `certificate not trusted`

**Solution:**
```bash
# Trust development certificate
dotnet dev-certs https --trust

# Or use HTTP endpoint
@baseUrl = http://localhost:5273
```

### Issue 2: Database Connection Error

**Error:** `Cannot connect to database`

**Solutions:**
1. Check connection string in `appsettings.json`
2. Ensure SQL Server is running
3. Run `dotnet ef database update`

### Issue 3: JWT Key Error

**Error:** `JWT Key not found`

**Solution:**
Ensure `appsettings.json` has valid JWT configuration

### Issue 4: Port Already in Use

**Error:** `Port 7240 is already in use`

**Solutions:**
1. Change port in `launchSettings.json`
2. Kill process using port: `netstat -ano | findstr :7240`

## ?? Test Checklist

### Basic Functionality
- [ ] User can register with valid data
- [ ] User can login with correct credentials
- [ ] JWT token is generated and valid
- [ ] User info can be retrieved with token
- [ ] Token can be refreshed
- [ ] User can change password
- [ ] User can logout

### Error Handling
- [ ] Invalid email format rejected
- [ ] Weak password rejected
- [ ] Duplicate email registration rejected
- [ ] Wrong password login rejected
- [ ] Invalid token rejected
- [ ] Expired token rejected

### Security
- [ ] Password is hashed (not stored as plain text)
- [ ] Account locks after failed attempts
- [ ] Token expires after configured time
- [ ] Refresh token can be revoked
- [ ] IP address is logged
- [ ] User activities are tracked

### Performance
- [ ] Registration completes in < 500ms
- [ ] Login completes in < 200ms
- [ ] Token operations complete in < 100ms
- [ ] No memory leaks during testing

## ?? Next Steps

1. **Production Testing**
   - Test v?i HTTPS certificate
   - Test v?i production database
   - Load testing v?i nhi?u users

2. **Integration Testing**
   - Test v?i email service th?t
   - Test v?i notification system
   - Test v?i audit logging

3. **Automation**
   - CI/CD pipeline testing
   - Automated regression testing
   - Performance benchmarking

4. **Security Hardening**
   - Penetration testing
   - Vulnerability scanning
   - Security audit

## ?? Support

N?u g?p v?n ?? trong quá trình test:

1. Check logs t?i `logs/` folder
2. Verify database connection
3. Check Swagger UI t?i `https://localhost:7240`
4. Review error responses ?? debug

Happy Testing! ??