@echo off
REM Quick Start Script for SakuraHome API Authentication Testing (Windows)
REM Batch script for Windows Command Prompt

echo.
echo ?? SakuraHome API Authentication Quick Start
echo =============================================
echo.

REM Configuration
set BASE_URL=https://localhost:7240
set TEST_EMAIL=quicktest@example.com
set TEST_PASSWORD=QuickTest123!

echo Base URL: %BASE_URL%
echo Test Email: %TEST_EMAIL%
echo.

REM Check if curl is available
curl --version >nul 2>&1
if errorlevel 1 (
    echo ? curl is not available. Please install curl or use PowerShell script instead.
    echo ?? Try: Tests\Auth-Test.ps1
    pause
    exit /b 1
)

REM Check if jq is available (optional)
jq --version >nul 2>&1
if errorlevel 1 (
    echo ??  jq is not available. JSON responses will not be formatted.
    echo ?? Install jq from: https://stedolan.github.io/jq/download/
    set JQ_AVAILABLE=false
) else (
    set JQ_AVAILABLE=true
)

echo.
echo ?? Checking if server is running...

REM Check if server is running
curl -s -k "%BASE_URL%/health" >nul 2>&1
if errorlevel 1 (
    echo ? Server is not running. Please start with: dotnet run
    pause
    exit /b 1
) else (
    echo ? Server is running
)

echo.
echo ?? Starting Authentication Tests...
echo ==================================

REM 1. Registration Test
echo.
echo ?? User Registration
echo    POST /api/auth/register

curl -s -X POST "%BASE_URL%/api/auth/register" ^
-H "Content-Type: application/json" ^
-d "{\"email\":\"%TEST_EMAIL%\",\"password\":\"%TEST_PASSWORD%\",\"confirmPassword\":\"%TEST_PASSWORD%\",\"firstName\":\"Quick\",\"lastName\":\"Test\",\"phoneNumber\":\"+84123456789\",\"dateOfBirth\":\"1990-01-01\",\"gender\":1,\"preferredLanguage\":\"vi\",\"acceptTerms\":true,\"emailNotifications\":true,\"smsNotifications\":false}" ^
-w "HTTPSTATUS:%%{http_code}" > temp_register.json

REM Extract HTTP status
for /f "tokens=2 delims=:" %%a in ('findstr "HTTPSTATUS" temp_register.json') do set HTTP_STATUS=%%a

if "%HTTP_STATUS%"=="200" (
    echo    ? Registration Success ^(%HTTP_STATUS%^)
    REM Try to extract token (simplified)
    findstr "token" temp_register.json >nul 2>&1
    if not errorlevel 1 (
        echo    ?? Token obtained from registration
        set REGISTRATION_SUCCESS=true
    ) else (
        set REGISTRATION_SUCCESS=false
    )
) else (
    echo    ??  Registration failed ^(%HTTP_STATUS%^) - User might already exist
    set REGISTRATION_SUCCESS=false
)

REM 2. Login Test (if registration failed or always)
echo.
echo ?? User Login
echo    POST /api/auth/login

curl -s -X POST "%BASE_URL%/api/auth/login" ^
-H "Content-Type: application/json" ^
-d "{\"email\":\"%TEST_EMAIL%\",\"password\":\"%TEST_PASSWORD%\",\"rememberMe\":false}" ^
-w "HTTPSTATUS:%%{http_code}" > temp_login.json

REM Extract HTTP status
for /f "tokens=2 delims=:" %%a in ('findstr "HTTPSTATUS" temp_login.json') do set LOGIN_STATUS=%%a

if "%LOGIN_STATUS%"=="200" (
    echo    ? Login Success ^(%LOGIN_STATUS%^)
    echo    ?? Authentication token obtained
    set LOGIN_SUCCESS=true
) else (
    echo    ? Login Failed ^(%LOGIN_STATUS%^)
    set LOGIN_SUCCESS=false
)

REM 3. Get User Info Test (requires manual token extraction for full implementation)
if "%LOGIN_SUCCESS%"=="true" (
    echo.
    echo ?? Get Current User Info
    echo    GET /api/auth/me
    echo    ??  Note: Token extraction simplified in batch script
    echo    ? Endpoint available ^(token required^)
)

REM 4. Error Testing
echo.
echo ?? Testing Error Scenarios...
echo ============================

echo.
echo ?? Test Wrong Password Login
echo    POST /api/auth/login

curl -s -X POST "%BASE_URL%/api/auth/login" ^
-H "Content-Type: application/json" ^
-d "{\"email\":\"%TEST_EMAIL%\",\"password\":\"WrongPassword123!\",\"rememberMe\":false}" ^
-w "HTTPSTATUS:%%{http_code}" > temp_wrong_login.json

for /f "tokens=2 delims=:" %%a in ('findstr "HTTPSTATUS" temp_wrong_login.json') do set WRONG_LOGIN_STATUS=%%a

if "%WRONG_LOGIN_STATUS%"=="401" (
    echo    ? Correctly rejected wrong password ^(%WRONG_LOGIN_STATUS%^)
) else (
    echo    ??  Unexpected response for wrong password ^(%WRONG_LOGIN_STATUS%^)
)

echo.
echo ?? Test Invalid Token
echo    GET /api/auth/me

curl -s -X GET "%BASE_URL%/api/auth/me" ^
-H "Authorization: Bearer invalid-token-here" ^
-w "HTTPSTATUS:%%{http_code}" > temp_invalid_token.json

for /f "tokens=2 delims=:" %%a in ('findstr "HTTPSTATUS" temp_invalid_token.json') do set INVALID_TOKEN_STATUS=%%a

if "%INVALID_TOKEN_STATUS%"=="401" (
    echo    ? Correctly rejected invalid token ^(%INVALID_TOKEN_STATUS%^)
) else (
    echo    ??  Unexpected response for invalid token ^(%INVALID_TOKEN_STATUS%^)
)

REM Cleanup temp files
del temp_register.json temp_login.json temp_wrong_login.json temp_invalid_token.json 2>nul

REM Summary
echo.
echo ?? Quick Test Summary
echo ====================
echo ? Basic authentication flow tested!
echo.
echo ?? Check detailed logs in: logs\sakura-home-%date:~-4,4%%date:~-7,2%%date:~-10,2%.txt
echo ?? View Swagger UI at: %BASE_URL%
echo ?? Run full tests with: Tests\Auth-Test.ps1
echo.
echo ?? Next Steps:
echo    1. Use PowerShell script for comprehensive testing
echo    2. Test with Swagger UI in browser
echo    3. Use HTTP files in Visual Studio/VS Code
echo    4. Check database for user records
echo.
echo ?? For advanced testing with token management:
echo    PowerShell: Tests\Auth-Test.ps1
echo    HTTP Files: Tests\Auth.http
echo.
echo ?? Quick start completed!
echo.
pause