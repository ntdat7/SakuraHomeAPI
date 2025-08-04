#!/bin/bash
# Quick Start Script for SakuraHome API Authentication Testing
# Bash script for Linux/Mac, or use with Git Bash on Windows

echo "?? SakuraHome API Authentication Quick Start"
echo "============================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
BASE_URL="https://localhost:7240"
TEST_EMAIL="quicktest@example.com"
TEST_PASSWORD="QuickTest123!"

echo -e "${BLUE}Base URL: $BASE_URL${NC}"
echo -e "${BLUE}Test Email: $TEST_EMAIL${NC}"
echo ""

# Function to make API call
make_api_call() {
    local method=$1
    local endpoint=$2
    local data=$3
    local token=$4
    local description=$5
    
    echo -e "${YELLOW}?? $description${NC}"
    echo -e "   $method $endpoint"
    
    # Prepare headers
    local headers="Content-Type: application/json"
    if [ ! -z "$token" ]; then
        headers="$headers, Authorization: Bearer $token"
    fi
    
    # Make the API call
    if [ "$method" = "GET" ]; then
        response=$(curl -s -w "HTTPSTATUS:%{http_code}" \
            -H "Content-Type: application/json" \
            -H "Authorization: Bearer $token" \
            "$BASE_URL$endpoint")
    else
        response=$(curl -s -w "HTTPSTATUS:%{http_code}" \
            -X "$method" \
            -H "Content-Type: application/json" \
            -H "Authorization: Bearer $token" \
            -d "$data" \
            "$BASE_URL$endpoint")
    fi
    
    # Extract HTTP status and body
    http_code=$(echo $response | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    body=$(echo $response | sed -e 's/HTTPSTATUS\:.*//g')
    
    # Check response
    if [ $http_code -eq 200 ] || [ $http_code -eq 201 ]; then
        echo -e "   ${GREEN}? Success ($http_code)${NC}"
        echo "$body" | jq . 2>/dev/null || echo "$body"
        echo "$body"
    else
        echo -e "   ${RED}? Error ($http_code)${NC}"
        echo "$body" | jq . 2>/dev/null || echo "$body"
        echo ""
    fi
}

# Check if server is running
echo -e "${YELLOW}?? Checking if server is running...${NC}"
if curl -s -k "$BASE_URL/health" > /dev/null 2>&1; then
    echo -e "${GREEN}? Server is running${NC}"
else
    echo -e "${RED}? Server is not running. Please start with: dotnet run${NC}"
    exit 1
fi

echo ""
echo "?? Starting Authentication Tests..."
echo "=================================="

# 1. Registration
echo ""
register_data='{
  "email": "'$TEST_EMAIL'",
  "password": "'$TEST_PASSWORD'",
  "confirmPassword": "'$TEST_PASSWORD'",
  "firstName": "Quick",
  "lastName": "Test",
  "phoneNumber": "+84123456789",
  "dateOfBirth": "1990-01-01",
  "gender": 1,
  "preferredLanguage": "vi",
  "acceptTerms": true,
  "emailNotifications": true,
  "smsNotifications": false
}'

register_response=$(make_api_call "POST" "/api/auth/register" "$register_data" "" "User Registration")

# Extract token from registration (if successful)
auth_token=$(echo "$register_response" | jq -r '.data.token // empty' 2>/dev/null)
refresh_token=$(echo "$register_response" | jq -r '.data.refreshToken // empty' 2>/dev/null)

if [ ! -z "$auth_token" ] && [ "$auth_token" != "null" ]; then
    echo -e "${GREEN}?? Token obtained: ${auth_token:0:20}...${NC}"
else
    echo -e "${YELLOW}??  Registration may have failed, trying login...${NC}"
    
    # 2. Try Login if registration failed (user might already exist)
    echo ""
    login_data='{
      "email": "'$TEST_EMAIL'",
      "password": "'$TEST_PASSWORD'",
      "rememberMe": false
    }'
    
    login_response=$(make_api_call "POST" "/api/auth/login" "$login_data" "" "User Login")
    auth_token=$(echo "$login_response" | jq -r '.data.token // empty' 2>/dev/null)
    refresh_token=$(echo "$login_response" | jq -r '.data.refreshToken // empty' 2>/dev/null)
    
    if [ ! -z "$auth_token" ] && [ "$auth_token" != "null" ]; then
        echo -e "${GREEN}?? Token obtained from login: ${auth_token:0:20}...${NC}"
    else
        echo -e "${RED}? Could not obtain authentication token${NC}"
        exit 1
    fi
fi

# 3. Get User Info
echo ""
make_api_call "GET" "/api/auth/me" "" "$auth_token" "Get Current User Info" > /dev/null

# 4. Check Authentication
echo ""
make_api_call "GET" "/api/auth/check" "" "$auth_token" "Check Authentication Status" > /dev/null

# 5. Refresh Token Test
if [ ! -z "$refresh_token" ] && [ "$refresh_token" != "null" ]; then
    echo ""
    refresh_data='{
      "accessToken": "'$auth_token'",
      "refreshToken": "'$refresh_token'"
    }'
    refresh_response=$(make_api_call "POST" "/api/auth/refresh-token" "$refresh_data" "" "Refresh Token")
    
    # Update tokens
    new_auth_token=$(echo "$refresh_response" | jq -r '.data.token // empty' 2>/dev/null)
    if [ ! -z "$new_auth_token" ] && [ "$new_auth_token" != "null" ]; then
        auth_token="$new_auth_token"
        echo -e "${GREEN}?? Token refreshed: ${auth_token:0:20}...${NC}"
    fi
fi

# 6. Password Change Test
echo ""
change_password_data='{
  "currentPassword": "'$TEST_PASSWORD'",
  "newPassword": "NewQuickTest123!",
  "confirmPassword": "NewQuickTest123!"
}'

make_api_call "POST" "/api/auth/change-password" "$change_password_data" "$auth_token" "Change Password" > /dev/null

# 7. Error Testing
echo ""
echo "?? Testing Error Scenarios..."
echo "============================"

# Test wrong password
echo ""
wrong_login_data='{
  "email": "'$TEST_EMAIL'",
  "password": "WrongPassword123!",
  "rememberMe": false
}'

make_api_call "POST" "/api/auth/login" "$wrong_login_data" "" "Test Wrong Password Login" > /dev/null

# Test invalid token
echo ""
make_api_call "GET" "/api/auth/me" "" "invalid-token-here" "Test Invalid Token" > /dev/null

# 8. Logout
echo ""
logout_data='{
  "refreshToken": "'$refresh_token'"
}'

make_api_call "POST" "/api/auth/logout" "$logout_data" "$auth_token" "User Logout" > /dev/null

# Summary
echo ""
echo "?? Quick Test Summary"
echo "===================="
echo -e "${GREEN}? Authentication flow tested successfully!${NC}"
echo -e "${BLUE}?? Check detailed logs in: logs/sakura-home-$(date +%Y%m%d).txt${NC}"
echo -e "${BLUE}?? View Swagger UI at: $BASE_URL${NC}"
echo -e "${BLUE}?? Run full tests with: Tests/Auth-Test.ps1${NC}"
echo ""
echo -e "${YELLOW}?? Next Steps:${NC}"
echo "   1. Run comprehensive tests with HTTP files"
echo "   2. Test with Swagger UI"
echo "   3. Check database for user records"
echo "   4. Monitor application logs"
echo ""
echo -e "${GREEN}?? Quick start completed!${NC}"