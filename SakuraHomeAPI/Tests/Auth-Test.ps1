# SakuraHome API Authentication Testing Script
# PowerShell script ?? test t? ??ng các endpoint authentication

param(
    [string]$BaseUrl = "https://localhost:7240",
    [string]$TestEmail = "test@example.com",
    [string]$TestPassword = "Test123!"
)

Write-Host "?? SakuraHome API Authentication Testing" -ForegroundColor Green
Write-Host "Base URL: $BaseUrl" -ForegroundColor Cyan
Write-Host "Test Email: $TestEmail" -ForegroundColor Cyan

# Function to make HTTP requests
function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [string]$Token = $null,
        [string]$Description
    )
    
    $headers = @{
        "Content-Type" = "application/json"
    }
    
    if ($Token) {
        $headers["Authorization"] = "Bearer $Token"
    }
    
    $uri = "$BaseUrl$Endpoint"
    
    Write-Host "`n?? $Description" -ForegroundColor Yellow
    Write-Host "   $Method $Endpoint" -ForegroundColor Gray
    
    try {
        $params = @{
            Uri = $uri
            Method = $Method
            Headers = $headers
            UseBasicParsing = $true
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
            Write-Host "   Body: $($params.Body)" -ForegroundColor Gray
        }
        
        $response = Invoke-RestMethod @params
        
        Write-Host "   ? Success: $($response.message)" -ForegroundColor Green
        return $response
    }
    catch {
        $errorResponse = $_.Exception.Response
        if ($errorResponse) {
            $reader = [System.IO.StreamReader]::new($errorResponse.GetResponseStream())
            $errorContent = $reader.ReadToEnd()
            $reader.Close()
            
            try {
                $errorJson = $errorContent | ConvertFrom-Json
                Write-Host "   ? Error: $($errorJson.message)" -ForegroundColor Red
                if ($errorJson.errors) {
                    $errorJson.errors | ForEach-Object { Write-Host "      - $_" -ForegroundColor Red }
                }
            }
            catch {
                Write-Host "   ? Error: $errorContent" -ForegroundColor Red
            }
        }
        else {
            Write-Host "   ? Error: $($_.Exception.Message)" -ForegroundColor Red
        }
        return $null
    }
}

# Test variables
$authToken = $null
$refreshToken = $null
$userId = $null

Write-Host "`n" + "="*60
Write-Host "?? AUTHENTICATION FLOW TESTING" -ForegroundColor Magenta
Write-Host "="*60

# 1. Test Registration
$registerData = @{
    email = $TestEmail
    password = $TestPassword
    confirmPassword = $TestPassword
    firstName = "John"
    lastName = "Doe"
    phoneNumber = "+84123456789"
    dateOfBirth = "1990-01-01"
    gender = 1
    preferredLanguage = "vi"
    acceptTerms = $true
    emailNotifications = $true
    smsNotifications = $false
}

$registerResponse = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/register" -Body $registerData -Description "User Registration"

if ($registerResponse -and $registerResponse.success) {
    $authToken = $registerResponse.data.token
    $refreshToken = $registerResponse.data.refreshToken
    $userId = $registerResponse.data.user.id
    
    Write-Host "   Token: $($authToken.Substring(0, 20))..." -ForegroundColor Cyan
    Write-Host "   User ID: $userId" -ForegroundColor Cyan
}

# 2. Test Login (with different user to avoid duplicate email)
$loginData = @{
    email = $TestEmail
    password = $TestPassword
    rememberMe = $false
}

$loginResponse = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/login" -Body $loginData -Description "User Login"

if ($loginResponse -and $loginResponse.success) {
    $authToken = $loginResponse.data.token
    $refreshToken = $loginResponse.data.refreshToken
    
    Write-Host "   New Token: $($authToken.Substring(0, 20))..." -ForegroundColor Cyan
}

# 3. Test Get Current User
$userResponse = Invoke-ApiRequest -Method "GET" -Endpoint "/api/auth/me" -Token $authToken -Description "Get Current User Info"

# 4. Test Check Authentication
$checkResponse = Invoke-ApiRequest -Method "GET" -Endpoint "/api/auth/check" -Token $authToken -Description "Check Authentication Status"

Write-Host "`n" + "="*60
Write-Host "?? TOKEN MANAGEMENT TESTING" -ForegroundColor Magenta
Write-Host "="*60

# 5. Test Refresh Token
if ($authToken -and $refreshToken) {
    $refreshData = @{
        accessToken = $authToken
        refreshToken = $refreshToken
    }
    
    $refreshResponse = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/refresh-token" -Body $refreshData -Description "Refresh Token"
    
    if ($refreshResponse -and $refreshResponse.success) {
        $newAuthToken = $refreshResponse.data.token
        $newRefreshToken = $refreshResponse.data.refreshToken
        Write-Host "   New Token Generated: $($newAuthToken.Substring(0, 20))..." -ForegroundColor Cyan
        
        # Update tokens for subsequent tests
        $authToken = $newAuthToken
        $refreshToken = $newRefreshToken
    }
}

Write-Host "`n" + "="*60
Write-Host "?? PASSWORD MANAGEMENT TESTING" -ForegroundColor Magenta
Write-Host "="*60

# 6. Test Forgot Password
$forgotPasswordData = @{
    email = $TestEmail
}

$forgotResponse = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/forgot-password" -Body $forgotPasswordData -Description "Forgot Password"

# 7. Test Change Password
if ($authToken) {
    $changePasswordData = @{
        currentPassword = $TestPassword
        newPassword = "NewTest123!"
        confirmPassword = "NewTest123!"
    }
    
    $changeResponse = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/change-password" -Body $changePasswordData -Token $authToken -Description "Change Password"
    
    if ($changeResponse -and $changeResponse.success) {
        # Update password for subsequent tests
        $TestPassword = "NewTest123!"
    }
}

Write-Host "`n" + "="*60
Write-Host "?? EMAIL VERIFICATION TESTING" -ForegroundColor Magenta
Write-Host "="*60

# 8. Test Resend Email Verification
$resendResponse = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/resend-email-verification" -Body "`"$TestEmail`"" -Description "Resend Email Verification"

Write-Host "`n" + "="*60
Write-Host "? ERROR SCENARIO TESTING" -ForegroundColor Magenta
Write-Host "="*60

# 9. Test Invalid Token
$invalidResponse = Invoke-ApiRequest -Method "GET" -Endpoint "/api/auth/me" -Token "invalid-token-here" -Description "Test Invalid Token"

# 10. Test Login with Wrong Password
$wrongLoginData = @{
    email = $TestEmail
    password = "WrongPassword!"
    rememberMe = $false
}

$wrongLoginResponse = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/login" -Body $wrongLoginData -Description "Test Wrong Password"

# 11. Test Registration with Existing Email
$duplicateRegisterData = @{
    email = $TestEmail
    password = "AnotherPass123!"
    confirmPassword = "AnotherPass123!"
    firstName = "Jane"
    lastName = "Smith"
    acceptTerms = $true
}

$duplicateResponse = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/register" -Body $duplicateRegisterData -Description "Test Duplicate Email Registration"

Write-Host "`n" + "="*60
Write-Host "?? LOGOUT TESTING" -ForegroundColor Magenta
Write-Host "="*60

# 12. Test Revoke Token
if ($authToken -and $refreshToken) {
    $revokeData = @{
        refreshToken = $refreshToken
    }
    
    $revokeResponse = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/revoke-token" -Body $revokeData -Token $authToken -Description "Revoke Specific Token"
}

# 13. Test Logout
if ($authToken) {
    $logoutData = @{
        refreshToken = $refreshToken
    }
    
    $logoutResponse = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/logout" -Body $logoutData -Token $authToken -Description "User Logout"
}

Write-Host "`n" + "="*60
Write-Host "?? TESTING SUMMARY" -ForegroundColor Green
Write-Host "="*60

Write-Host "? Authentication testing completed!" -ForegroundColor Green
Write-Host "?? Check the responses above for any failures" -ForegroundColor Yellow
Write-Host "?? Review logs at: logs/sakura-home-$(Get-Date -Format 'yyyyMMdd').txt" -ForegroundColor Cyan
Write-Host "?? Check database for user activities and refresh tokens" -ForegroundColor Cyan

Write-Host "`n?? Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Implement email service for actual email sending" -ForegroundColor Gray
Write-Host "   2. Add rate limiting middleware" -ForegroundColor Gray
Write-Host "   3. Implement background job for token cleanup" -ForegroundColor Gray
Write-Host "   4. Add more comprehensive validation" -ForegroundColor Gray
Write-Host "   5. Setup monitoring and alerts" -ForegroundColor Gray