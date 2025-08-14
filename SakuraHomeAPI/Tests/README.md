# 🧪 SakuraHome API Test Suite

Thư mục này chứa tất cả các file test cho SakuraHome API system, bao gồm các endpoint chính và complete workflows.

## 📁 Cấu trúc Files

### 🎯 **Quick Start Files**
- **`Complete-API-Suite.http`** - Test suite tổng hợp cho complete e-commerce flow
- **`quick-start.bat/.sh`** - Scripts tự động chạy app và open test files

### 🔑 **Core API Tests**
- **`Auth.http`** - Authentication & Authorization tests
- **`User.http`** - User management & profile tests

### 🛍️ **E-commerce API Tests**
- **`Cart.http`** - Shopping cart operations
- **`Order.http`** - Order management & processing
- **`Payment.http`** - Payment processing & transactions
- **`Wishlist.http`** - Wishlist management

### 🆕 **New API Tests (Recently Added)**
- **`Shipping.http`** - Shipping system tests (rates, tracking, orders)
- **`Review.http`** - Product review system tests
- **`Coupon.http`** - Coupon & discount system tests

### 📦 **Product & Catalog Tests**
- **`Brand-Category.http`** - Brand & Category management
- **`Notification.http`** - Notification system tests

### 📚 **Documentation**
- **`Testing-Guide.md`** - Comprehensive guide cho API testing

## 🚀 Quick Start

### 1. **Prepare Environment**
```bash
# Start the API
dotnet run

# Trust HTTPS certificate (if needed)
dotnet dev-certs https --trust
```

### 2. **Choose Your Testing Method**

#### **Method 1: VS Code + REST Client (Recommended)**
1. Install "REST Client" extension
2. Open any `.http` file
3. Click "Send Request" above each test

#### **Method 2: Visual Studio**
1. Open any `.http` file
2. Click the play button next to requests
3. View results in "Web API" tab

#### **Method 3: Quick Scripts**
```bash
# Windows
quick-start.bat

# Linux/Mac  
./quick-start.sh
```

### 3. **Start with Complete Flow**
1. Open `Complete-API-Suite.http`
2. Run the "QUICK START" section từ trên xuống
3. Test complete e-commerce workflow

## 📋 Test Execution Order

### **🎯 For New Users:**
```
1. Complete-API-Suite.http (QUICK START section)
2. Auth.http (detailed authentication tests)
3. User.http (profile management)
4. Cart.http (shopping cart)
5. Order.http (order processing)
```

### **🔧 For Developers:**
```
1. Auth.http (authentication foundation)
2. Specific API tests based on feature development
3. Complete-API-Suite.http (integration testing)
```

### **👨‍💼 For Testing Team:**
```
1. All individual API test files
2. Error scenarios in each file
3. Performance tests in Complete-API-Suite.http
4. Security tests section
```

## 🎯 API Coverage

### ✅ **Fully Tested APIs**
- **Authentication** - Login, register, token management
- **User Management** - Profile, addresses, statistics
- **Shopping Cart** - Add/remove items, apply coupons
- **Wishlist** - Manage favorite products
- **Orders** - Create, track, manage orders
- **Payments** - Process payments, transaction history
- **Notifications** - Send and manage notifications
- **Shipping** - Calculate rates, track packages
- **Reviews** - Product reviews and ratings
- **Coupons** - Discount codes and promotions

### 🚧 **Partially Tested APIs**
- **Products** - Basic operations (full tests in Brand-Category.http)
- **Analytics** - Basic reporting (more tests in Complete-API-Suite.http)

### ❌ **Pending Test Coverage**
- **File Upload** - Image and document handling
- **Email System** - Email templates and sending
- **Admin Dashboard** - Advanced administrative features

## 🔧 Configuration

### **Base URLs**
```http
# Development
@baseUrl = http://localhost:5273

# Staging  
@baseUrl = https://staging-api.sakurahome.com

# Production
@baseUrl = https://api.sakurahome.com
```

### **Authentication Tokens**
```http
# Quick test token (update after login)
@quickAuthToken = your-token-here

# Dynamic token (auto-populated from login response)
@authToken = {{login.response.body.data.token}}
```

## 🧪 Test Categories

### **1. 🎯 Functional Tests**
- Happy path scenarios
- Required field validation
- Business logic verification
- Data consistency checks

### **2. 🔒 Security Tests**
- Authentication enforcement
- Authorization validation
- Input sanitization
- Rate limiting

### **3. ⚡ Performance Tests**
- Response time validation
- Concurrent request handling
- Large data set processing
- Memory usage monitoring

### **4. 🚨 Error Handling Tests**
- Invalid input handling
- Network error simulation
- Database connection issues
- Third-party service failures

## 📊 Expected Response Times

| API Category | Target Time | Description |
|-------------|-------------|-------------|
| Authentication | < 200ms | Login, token refresh |
| User Operations | < 150ms | Profile, preferences |
| Product Queries | < 300ms | Search, filtering |
| Cart Operations | < 100ms | Add/remove items |
| Order Processing | < 500ms | Create orders |
| Payment Processing | < 1000ms | External payment APIs |
| Shipping Calculations | < 250ms | Rate calculations |
| Review Operations | < 200ms | CRUD operations |

## 🔍 Debugging & Monitoring

### **Log Files**
```
logs/sakura-home-{date}.txt
```

### **Database Monitoring**
```sql
-- Check user activities
SELECT * FROM UserActivities ORDER BY CreatedAt DESC;

-- Monitor API performance
SELECT * FROM SystemLogs WHERE Level = 'Error';

-- Check order processing
SELECT * FROM Orders WHERE Status = 'Processing';
```

### **Health Checks**
```http
GET {{baseUrl}}/api/health
GET {{baseUrl}}/api/system/info
```

## 🎨 Test Data Setup

### **Required Test Users**
```json
{
  "customer": "testuser@sakurahome.com / Test123!",
  "admin": "admin@sakurahome.com / Admin123!",
  "staff": "staff@sakurahome.com / Staff123!"
}
```

### **Sample Products**
- Ensure database có ít nhất 5 products với different categories
- Products should have variants và proper inventory
- Include products với different price ranges

### **Test Addresses**
```json
{
  "domesticAddress": {
    "province": "HCM",
    "district": "Q1", 
    "ward": "P1"
  },
  "internationalAddress": {
    "country": "Vietnam",
    "province": "HN"
  }
}
```

## 🔄 Continuous Integration

### **CI/CD Integration**
```yaml
# Example GitHub Actions workflow
name: API Tests
on: [push, pull_request]
jobs:
  api-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
      - name: Run API Tests
        run: |
          dotnet run &
          # Wait for API to start
          sleep 30
          # Run tests using newman or similar tool
```

### **Test Automation Tools**
- **Newman** - Command line collection runner cho Postman
- **REST Client** - VS Code extension
- **Insomnia** - API testing tool
- **Postman** - Full-featured API platform

## 📈 Success Metrics

### **Quality Gates**
- ✅ All functional tests pass
- ✅ Response times within targets  
- ✅ Error rate < 1%
- ✅ Security scans pass
- ✅ No memory leaks detected

### **Performance Benchmarks**
- **Throughput**: > 1000 requests/second
- **Concurrency**: Support 100+ concurrent users
- **Availability**: 99.9% uptime
- **Scalability**: Linear performance scaling

## 🆘 Troubleshooting

### **Common Issues**

#### **"No project found" Error**
```bash
# Solution: Navigate to project directory
cd SakuraHomeAPI
dotnet ef database update
```

#### **JWT Token Expired**
```http
# Solution: Re-run login request
POST {{baseUrl}}/api/auth/login
```

#### **Database Connection Error**
```bash
# Solution: Check connection string và start SQL Server
dotnet ef database update
```

#### **HTTPS Certificate Issues**
```bash
# Solution: Trust development certificate
dotnet dev-certs https --trust
```

### **Getting Help**
1. Check logs in `logs/` directory
2. Verify database connectivity
3. Test với Swagger UI: `https://localhost:7240`
4. Review error responses for detailed messages

## 🎯 Best Practices

### **Test Organization**
- ✅ Group related tests trong same file
- ✅ Use descriptive test names
- ✅ Include expected outcomes trong comments
- ✅ Test both success và error scenarios

### **Data Management**
- ✅ Use realistic test data
- ✅ Clean up test data after runs
- ✅ Avoid hard-coded IDs
- ✅ Use variables cho reusable values

### **Security Considerations**
- ✅ Never commit real passwords
- ✅ Use test accounts only
- ✅ Rotate test credentials regularly
- ✅ Test authorization boundaries

---

## 🎉 Happy Testing!

Với comprehensive test suite này, bạn có thể:
- ✅ Verify API functionality
- ✅ Ensure data integrity  
- ✅ Validate business logic
- ✅ Monitor performance
- ✅ Catch regressions early

**Remember**: Good tests = Reliable software = Happy users! 🚀