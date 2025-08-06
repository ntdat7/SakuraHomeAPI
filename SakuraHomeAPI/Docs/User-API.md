# SakuraHome API - User Profile Management

## Tổng quan

Hệ thống quản lý hồ sơ người dùng SakuraHome cung cấp đầy đủ các chức năng quản lý thông tin cá nhân, địa chỉ, và thống kê người dùng.

## Base URL
```
https://localhost:7240/api/user
```

## Authentication
Tất cả endpoints yêu cầu JWT Bearer token:
```
Authorization: Bearer your-jwt-token
```

## Endpoints

### 👤 Profile Management

#### 1. Lấy thông tin hồ sơ người dùng
```http
GET /api/user/profile
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy thông tin hồ sơ thành công",
  "data": {
    "id": "user-guid-here",
    "email": "user@example.com",
    "firstName": "Nguyễn",
    "lastName": "Văn A",
    "fullName": "Nguyễn Văn A",
    "phoneNumber": "+84123456789",
    "dateOfBirth": "1990-01-01T00:00:00Z",
    "gender": 1,
    "preferredLanguage": "vi",
    "preferredCurrency": "VND",
    "role": "Customer",
    "tier": "Silver",
    "status": "Active",
    "emailVerified": true,
    "phoneVerified": false,
    "lastLoginAt": "2024-01-01T12:00:00Z",
    "createdAt": "2023-01-01T00:00:00Z",
    "avatar": "https://example.com/avatar.jpg",
    "notificationPreferences": {
      "emailNotifications": true,
      "smsNotifications": false,
      "pushNotifications": true
    }
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 2. Cập nhật thông tin hồ sơ
```http
PUT /api/user/profile
```

**Request Body:**
```json
{
  "firstName": "Nguyễn",
  "lastName": "Văn B",
  "phoneNumber": "+84987654321",
  "dateOfBirth": "1990-01-01T00:00:00Z",
  "gender": 1,
  "preferredLanguage": "vi",
  "preferredCurrency": "VND",
  "avatar": "https://example.com/new-avatar.jpg",
  "notificationPreferences": {
    "emailNotifications": true,
    "smsNotifications": true,
    "pushNotifications": true
  }
}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Cập nhật hồ sơ thành công",
  "data": {
    "id": "user-guid-here",
    "email": "user@example.com",
    "firstName": "Nguyễn",
    "lastName": "Văn B",
    "fullName": "Nguyễn Văn B",
    "phoneNumber": "+84987654321",
    "dateOfBirth": "1990-01-01T00:00:00Z",
    "gender": 1,
    "preferredLanguage": "vi",
    "preferredCurrency": "VND",
    "role": "Customer",
    "tier": "Silver",
    "status": "Active",
    "emailVerified": true,
    "phoneVerified": false,
    "lastLoginAt": "2024-01-01T12:00:00Z",
    "createdAt": "2023-01-01T00:00:00Z",
    "avatar": "https://example.com/new-avatar.jpg"
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 3. Xóa tài khoản (Soft Delete)
```http
DELETE /api/user/profile
```

**Request Body:**
```json
{
  "reason": "Không muốn sử dụng dịch vụ nữa",
  "password": "CurrentPassword123!"
}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Tài khoản đã được xóa thành công",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 4. Lấy thống kê người dùng
```http
GET /api/user/stats
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy thống kê người dùng thành công",
  "data": {
    "totalOrders": 25,
    "totalSpent": 15750000,
    "totalSaved": 2500000,
    "currentTier": "Silver",
    "nextTierRequirement": 5000000,
    "loyaltyPoints": 1575,
    "favoriteBrands": [
      {
        "brandId": 1,
        "brandName": "Samsung",
        "orderCount": 8
      },
      {
        "brandId": 2,
        "brandName": "Apple",
        "orderCount": 5
      }
    ],
    "favoriteCategories": [
      {
        "categoryId": 1,
        "categoryName": "Điện thoại",
        "orderCount": 10
      },
      {
        "categoryId": 2,
        "categoryName": "Laptop",
        "orderCount": 3
      }
    ],
    "monthlySpending": [
      {
        "month": "2024-01",
        "amount": 3500000
      },
      {
        "month": "2024-02",
        "amount": 2750000
      }
    ],
    "recentActivity": {
      "lastOrderDate": "2024-01-15T10:30:00Z",
      "lastLoginDate": "2024-01-01T12:00:00Z",
      "wishlistCount": 12,
      "cartItemCount": 3
    }
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### 📍 Address Management

#### 5. Lấy danh sách địa chỉ
```http
GET /api/user/addresses
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy danh sách địa chỉ thành công",
  "data": [
    {
      "id": 1,
      "title": "Nhà riêng",
      "recipientName": "Nguyễn Văn A",
      "phoneNumber": "+84123456789",
      "streetAddress": "123 Nguyễn Trãi",
      "ward": "Phường Bến Thành",
      "district": "Quận 1",
      "province": "TP. Hồ Chí Minh",
      "postalCode": "70000",
      "country": "Việt Nam",
      "isDefault": true,
      "addressType": "Home",
      "notes": "Gọi trước khi giao hàng",
      "createdAt": "2023-01-01T00:00:00Z",
      "updatedAt": "2024-01-01T10:00:00Z"
    },
    {
      "id": 2,
      "title": "Văn phòng",
      "recipientName": "Nguyễn Văn A",
      "phoneNumber": "+84123456789",
      "streetAddress": "456 Lê Lợi",
      "ward": "Phường Nguyễn Thái Bình",
      "district": "Quận 1",
      "province": "TP. Hồ Chí Minh",
      "postalCode": "70000",
      "country": "Việt Nam",
      "isDefault": false,
      "addressType": "Work",
      "notes": "Nhận hàng từ 9h-17h",
      "createdAt": "2023-02-01T00:00:00Z",
      "updatedAt": "2023-02-01T00:00:00Z"
    }
  ],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 6. Tạo địa chỉ mới
```http
POST /api/user/addresses
```

**Request Body:**
```json
{
  "title": "Nhà bố mẹ",
  "recipientName": "Nguyễn Văn A",
  "phoneNumber": "+84123456789",
  "streetAddress": "789 Hai Bà Trưng",
  "ward": "Phường Đa Kao",
  "district": "Quận 1",
  "province": "TP. Hồ Chí Minh",
  "postalCode": "70000",
  "country": "Việt Nam",
  "addressType": "Home",
  "notes": "Nhà màu xanh, cửa số 10",
  "isDefault": false
}
```

**Response Success (201):**
```json
{
  "success": true,
  "message": "Tạo địa chỉ thành công",
  "data": {
    "id": 3,
    "title": "Nhà bố mẹ",
    "recipientName": "Nguyễn Văn A",
    "phoneNumber": "+84123456789",
    "streetAddress": "789 Hai Bà Trưng",
    "ward": "Phường Đa Kao",
    "district": "Quận 1",
    "province": "TP. Hồ Chí Minh",
    "postalCode": "70000",
    "country": "Việt Nam",
    "isDefault": false,
    "addressType": "Home",
    "notes": "Nhà màu xanh, cửa số 10",
    "createdAt": "2024-01-01T12:00:00Z",
    "updatedAt": "2024-01-01T12:00:00Z"
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 7. Cập nhật địa chỉ
```http
PUT /api/user/addresses/{id}
```

**Request Body:**
```json
{
  "title": "Nhà bố mẹ (cập nhật)",
  "recipientName": "Nguyễn Văn A",
  "phoneNumber": "+84987654321",
  "streetAddress": "789 Hai Bà Trưng (cập nhật)",
  "ward": "Phường Đa Kao",
  "district": "Quận 1",
  "province": "TP. Hồ Chí Minh",
  "postalCode": "70000",
  "country": "Việt Nam",
  "addressType": "Home",
  "notes": "Nhà màu xanh, cửa số 10 (cập nhật)",
  "isDefault": false
}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Cập nhật địa chỉ thành công",
  "data": {
    "id": 3,
    "title": "Nhà bố mẹ (cập nhật)",
    "recipientName": "Nguyễn Văn A",
    "phoneNumber": "+84987654321",
    "streetAddress": "789 Hai Bà Trưng (cập nhật)",
    "ward": "Phường Đa Kao",
    "district": "Quận 1",
    "province": "TP. Hồ Chí Minh",
    "postalCode": "70000",
    "country": "Việt Nam",
    "isDefault": false,
    "addressType": "Home",
    "notes": "Nhà màu xanh, cửa số 10 (cập nhật)",
    "createdAt": "2024-01-01T12:00:00Z",
    "updatedAt": "2024-01-01T12:30:00Z"
  },
  "timestamp": "2024-01-01T12:30:00Z"
}
```

#### 8. Xóa địa chỉ
```http
DELETE /api/user/addresses/{id}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Xóa địa chỉ thành công",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 9. Đặt địa chỉ mặc định
```http
PATCH /api/user/addresses/{id}/set-default
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Đã đặt địa chỉ làm mặc định",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## Data Definitions

### 👤 User Profile Fields

| Field | Type | Description |
|-------|------|-------------|
| `id` | GUID | ID duy nhất của người dùng |
| `email` | string | Email đăng nhập |
| `firstName` | string | Tên |
| `lastName` | string | Họ |
| `fullName` | string | Họ và tên đầy đủ |
| `phoneNumber` | string | Số điện thoại |
| `dateOfBirth` | datetime | Ngày sinh |
| `gender` | int | Giới tính (1: Nam, 2: Nữ, 3: Khác) |
| `preferredLanguage` | string | Ngôn ngữ ưa thích |
| `preferredCurrency` | string | Tiền tệ ưa thích |
| `role` | string | Vai trò (Customer, Staff, Admin) |
| `tier` | string | Hạng khách hàng (Bronze, Silver, Gold, Platinum, Diamond) |
| `status` | string | Trạng thái tài khoản |
| `emailVerified` | boolean | Email đã xác thực |
| `phoneVerified` | boolean | Số điện thoại đã xác thực |

### 📍 Address Fields

| Field | Type | Description |
|-------|------|-------------|
| `id` | int | ID duy nhất của địa chỉ |
| `title` | string | Tên địa chỉ (Nhà riêng, Văn phòng...) |
| `recipientName` | string | Tên người nhận |
| `phoneNumber` | string | Số điện thoại người nhận |
| `streetAddress` | string | Địa chỉ đường phố |
| `ward` | string | Phường/Xã |
| `district` | string | Quận/Huyện |
| `province` | string | Tỉnh/Thành phố |
| `postalCode` | string | Mã bưu điện |
| `country` | string | Quốc gia |
| `isDefault` | boolean | Có phải địa chỉ mặc định |
| `addressType` | string | Loại địa chỉ (Home, Work, Other) |
| `notes` | string | Ghi chú cho shipper |

### 📊 User Tier System

| Tier | Requirement | Benefits |
|------|-------------|----------|
| Bronze | 0 - 1M VND | 1% cashback |
| Silver | 1M - 5M VND | 2% cashback, free shipping |
| Gold | 5M - 10M VND | 3% cashback, priority support |
| Platinum | 10M - 20M VND | 5% cashback, exclusive products |
| Diamond | 20M+ VND | 7% cashback, personal shopper |

## Validation Rules

### Profile Update Validation
- `firstName`: Bắt buộc, 1-50 ký tự
- `lastName`: Bắt buộc, 1-50 ký tự
- `phoneNumber`: Định dạng số điện thoại hợp lệ
- `dateOfBirth`: Phải trên 13 tuổi
- `gender`: 1 (Nam), 2 (Nữ), 3 (Khác)
- `preferredLanguage`: vi, en
- `preferredCurrency`: VND, USD

### Address Validation
- `title`: Bắt buộc, 1-100 ký tự
- `recipientName`: Bắt buộc, 1-100 ký tự
- `phoneNumber`: Bắt buộc, định dạng hợp lệ
- `streetAddress`: Bắt buộc, 1-200 ký tự
- `ward`: Bắt buộc, 1-100 ký tự
- `district`: Bắt buộc, 1-100 ký tự
- `province`: Bắt buộc, 1-100 ký tự
- `country`: Bắt buộc, 1-100 ký tự
- `addressType`: Home, Work, Other
- `notes`: Tối đa 500 ký tự

## Error Responses

### 400 Bad Request
```json
{
  "success": false,
  "message": "Dữ liệu không hợp lệ",
  "errors": [
    "Tên là bắt buộc",
    "Số điện thoại không đúng định dạng"
  ],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### 401 Unauthorized
```json
{
  "success": false,
  "message": "Không có quyền truy cập",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### 404 Not Found
```json
{
  "success": false,
  "message": "Không tìm thấy địa chỉ",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### 409 Conflict
```json
{
  "success": false,
  "message": "Không thể xóa địa chỉ mặc định",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## Frontend Integration Examples

### Lấy và hiển thị profile
```javascript
const getUserProfile = async () => {
  const response = await fetch('/api/user/profile', {
    headers: {
      'Authorization': `Bearer ${userToken}`
    }
  });
  const result = await response.json();
  return result.data;
};

// Update profile
const updateProfile = async (profileData) => {
  const response = await fetch('/api/user/profile', {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${userToken}`
    },
    body: JSON.stringify(profileData)
  });
  return await response.json();
};
```

### Quản lý địa chỉ
```javascript
// Get addresses
const getAddresses = async () => {
  const response = await fetch('/api/user/addresses', {
    headers: {
      'Authorization': `Bearer ${userToken}`
    }
  });
  const result = await response.json();
  return result.data;
};

// Add new address
const addAddress = async (addressData) => {
  const response = await fetch('/api/user/addresses', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${userToken}`
    },
    body: JSON.stringify(addressData)
  });
  return await response.json();
};

// Set default address
const setDefaultAddress = async (addressId) => {
  const response = await fetch(`/api/user/addresses/${addressId}/set-default`, {
    method: 'PATCH',
    headers: {
      'Authorization': `Bearer ${userToken}`
    }
  });
  return await response.json();
};
```

### Hiển thị thống kê người dùng
```javascript
const getUserStats = async () => {
  const response = await fetch('/api/user/stats', {
    headers: {
      'Authorization': `Bearer ${userToken}`
    }
  });
  const result = await response.json();
  return result.data;
};

// Display tier progress
const displayTierProgress = (stats) => {
  const { totalSpent, currentTier, nextTierRequirement } = stats;
  const progress = (totalSpent / nextTierRequirement) * 100;
  
  return {
    currentTier,
    progress: Math.min(progress, 100),
    remaining: Math.max(nextTierRequirement - totalSpent, 0)
  };
};
```

## Testing

Test endpoints với file HTTP:
```http
### Get user profile
GET {{baseUrl}}/api/user/profile
Authorization: Bearer {{userToken}}

### Update profile
PUT {{baseUrl}}/api/user/profile
Authorization: Bearer {{userToken}}
Content-Type: application/json

{
  "firstName": "Updated",
  "lastName": "Name",
  "phoneNumber": "+84987654321"
}

### Get addresses
GET {{baseUrl}}/api/user/addresses
Authorization: Bearer {{userToken}}

### Add new address
POST {{baseUrl}}/api/user/addresses
Authorization: Bearer {{userToken}}
Content-Type: application/json

{
  "title": "Test Address",
  "recipientName": "Test User",
  "phoneNumber": "+84123456789",
  "streetAddress": "123 Test Street",
  "ward": "Test Ward",
  "district": "Test District",
  "province": "Test Province",
  "postalCode": "70000",
  "country": "Việt Nam",
  "addressType": "Home"
}
```

## Features Summary

### ✅ Đã hoàn thành:
- **Profile Management**: CRUD hoàn chỉnh cho hồ sơ người dùng
- **Address Management**: Quản lý nhiều địa chỉ với địa chỉ mặc định
- **User Statistics**: Thống kê chi tiêu, đơn hàng, tier system
- **Notification Preferences**: Cài đặt thông báo cá nhân
- **Tier System**: Hệ thống hạng khách hàng với lợi ích
- **Data Validation**: Validation đầy đủ cho tất cả fields
- **Error Handling**: Xử lý lỗi toàn diện
- **Security**: Authentication & authorization đầy đủ

### 🔄 Có thể mở rộng:
- **Avatar Upload**: Tích hợp upload ảnh đại diện
- **Social Login**: Đăng nhập qua Facebook, Google
- **Two-Factor Authentication**: Bảo mật 2 lớp
- **Activity Log**: Lịch sử hoạt động chi tiết
- **Personalization**: Cá nhân hóa giao diện và nội dung

Hệ thống User Management đã hoàn thiện và sẵn sàng phục vụ các tính năng e-commerce cao cấp.