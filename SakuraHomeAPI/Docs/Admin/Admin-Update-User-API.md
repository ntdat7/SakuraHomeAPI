# 📝 Admin Update User API - Tài liệu cho Frontend Developer

## 📋 Mục lục
1. [Tổng quan](#tổng-quan)
2. [Endpoint Details](#endpoint-details)
3. [Request Body Schema](#request-body-schema)
4. [Response Schema](#response-schema)
5. [Validation Rules](#validation-rules)
6. [Code Examples](#code-examples)
7. [Error Handling](#error-handling)
8. [React Component Example](#react-component-example)
9. [Testing Guide](#testing-guide)
10. [Best Practices](#best-practices)

---

## 🎯 Tổng quan

API này cho phép Admin/Staff **cập nhật thông tin người dùng** trong hệ thống SakuraHome. API hỗ trợ cập nhật thông tin cho cả Customer, Staff và Admin.

### Đặc điểm chính
- ✅ **Partial Update**: Chỉ cần gửi các field muốn thay đổi
- ✅ **Role-based Update**: Hỗ trợ cập nhật thông tin theo vai trò
- ✅ **Automatic Validation**: Backend tự động validate dữ liệu
- ✅ **Audit Trail**: Tự động ghi lại người và thời gian cập nhật
- ✅ **Safe Updates**: Không cho phép sửa email (dùng làm identifier)

### Quyền hạn
| Role | Quyền hạn |
|------|-----------|
| **SuperAdmin** | Có thể cập nhật tất cả users |
| **Admin** | Có thể cập nhật tất cả users |
| **Staff** | Có thể cập nhật Customer, không thể sửa Admin/Staff khác |
| **Customer** | ❌ Không có quyền truy cập |

---

## 🔌 Endpoint Details

### HTTP Method & URL
```http
PUT /api/admin/users/{userId}/update
```

### Headers
```javascript
{
  "Content-Type": "application/json",
  "Authorization": "Bearer {JWT_TOKEN}"
}
```

### URL Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userId` | GUID | ✅ Yes | ID của user cần cập nhật |

### Example URLs
```
PUT https://localhost:8080/api/admin/users/123e4567-e89b-12d3-a456-426614174000/update
PUT https://api.sakurahome.com/api/admin/users/a1b2c3d4-e5f6-7890-abcd-ef1234567890/update
```

---

## 📦 Request Body Schema

### UpdateUserRequest DTO

```typescript
interface UpdateUserRequest {
  // ========== BASIC INFORMATION ==========
  firstName?: string;          // Họ (max: 100 ký tự)
  lastName?: string;           // Tên (max: 100 ký tự)
  phoneNumber?: string;        // Số điện thoại (format chuẩn)
  dateOfBirth?: string;        // Ngày sinh (ISO 8601 format: "YYYY-MM-DD")
  gender?: Gender;             // Giới tính
  avatar?: string;             // URL ảnh đại diện (max: 500 ký tự)
  
  // ========== EMPLOYEE FIELDS (Staff/Admin only) ==========
  nationalIdCard?: string;     // CCCD/CMND (max: 20 ký tự)
  hireDate?: string;           // Ngày vào làm (ISO 8601)
  baseSalary?: number;         // Lương cơ bản (0 - 100,000,000 VND)
  
  // ========== ROLE & STATUS ==========
  role?: UserRole;             // Vai trò: Customer, Staff, Admin, SuperAdmin
  status?: AccountStatus;      // Trạng thái tài khoản
  isActive?: boolean;          // Trạng thái hoạt động
  
  // ========== CUSTOMER FIELDS ==========
  tier?: UserTier;             // Hạng thành viên (Customer only)
}

// ========== ENUMS ==========

enum Gender {
  Unknown = 0,           // Không xác định
  Male = 1,             // Nam
  Female = 2,           // Nữ
  Other = 3,            // Khác
  PreferNotToSay = 4    // Không muốn nói
}

enum UserRole {
  Customer = 1,         // Khách hàng
  Staff = 2,           // Nhân viên
  Admin = 3,           // Quản lý
  SuperAdmin = 4       // Super Admin
}

enum AccountStatus {
  Pending = 1,         // Chờ xác thực email
  Active = 2,          // Hoạt động bình thường
  Suspended = 3,       // Bị tạm khóa
  Banned = 4,          // Bị cấm vĩnh viễn
  Inactive = 5         // Không hoạt động
}

enum UserTier {
  Bronze = 1,          // 0 - 1M VND
  Silver = 2,          // 1M - 5M VND
  Gold = 3,            // 5M - 10M VND
  Platinum = 4,        // 10M - 20M VND
  Diamond = 5          // > 20M VND
}
```

### Request Body Examples

#### 1. Cập nhật thông tin cơ bản
```json
{
  "firstName": "Văn A",
  "lastName": "Nguyễn",
  "phoneNumber": "0901234567",
  "dateOfBirth": "1990-05-15",
  "gender": 1
}
```

#### 2. Thay đổi role user
```json
{
  "role": 2
}
```

#### 3. Cập nhật thông tin nhân viên
```json
{
  "nationalIdCard": "001234567890",
  "hireDate": "2024-01-01",
  "baseSalary": 15000000
}
```

#### 4. Thay đổi status & tier
```json
{
  "status": 2,
  "tier": 3,
  "isActive": true
}
```

#### 5. Cập nhật nhiều field cùng lúc
```json
{
  "firstName": "Thị B",
  "lastName": "Trần",
  "phoneNumber": "0987654321",
  "dateOfBirth": "1995-08-20",
  "gender": 2,
  "role": 1,
  "tier": 2,
  "status": 2,
  "isActive": true,
  "avatar": "https://example.com/avatars/user123.jpg"
}
```

---

## 📤 Response Schema

### Success Response (200 OK)

```typescript
interface ApiResponse<UserSummaryDto> {
  success: true;
  message: string;
  data: UserSummaryDto;
}

interface UserSummaryDto {
  id: string;                    // GUID
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;              // Computed: firstName + lastName
  phoneNumber?: string;
  role: string;                  // "Customer", "Staff", "Admin", "SuperAdmin"
  status: string;                // "Active", "Pending", "Suspended", "Banned", "Inactive"
  isActive: boolean;
  emailVerified: boolean;
  phoneVerified: boolean;
  tier?: string;                 // "Bronze", "Silver", "Gold", "Platinum", "Diamond"
  loyaltyPoints: number;
  totalSpent: number;
  orderCount: number;
  avatar?: string;
  dateOfBirth?: string;
  gender?: string;               // "Male", "Female", "Other", "Unknown"
  createdAt: string;             // ISO 8601
  updatedAt: string;             // ISO 8601
  lastLoginAt?: string;          // ISO 8601
}
```

### Success Response Example

```json
{
  "success": true,
  "message": "Cập nhật user thành công",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "email": "nguyenvana@example.com",
    "firstName": "Văn A",
    "lastName": "Nguyễn",
    "fullName": "Nguyễn Văn A",
    "phoneNumber": "0901234567",
    "role": "Customer",
    "status": "Active",
    "isActive": true,
    "emailVerified": true,
    "phoneVerified": true,
    "tier": "Gold",
    "loyaltyPoints": 1500,
    "totalSpent": 15000000,
    "orderCount": 25,
    "avatar": "https://example.com/avatar.jpg",
    "dateOfBirth": "1990-05-15",
    "gender": "Male",
    "createdAt": "2023-01-15T10:30:00Z",
    "updatedAt": "2024-06-15T14:20:00Z",
    "lastLoginAt": "2024-06-15T10:00:00Z"
  }
}
```

### Error Response Examples

#### Validation Error (400 Bad Request)
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": {
    "PhoneNumber": ["Số điện thoại không hợp lệ"],
    "BaseSalary": ["Lương cơ bản phải từ 0 đến 100,000,000"]
  }
}
```

#### Not Found (404 Not Found)
```json
{
  "success": false,
  "message": "Không tìm thấy user"
}
```

#### Unauthorized (401 Unauthorized)
```json
{
  "success": false,
  "message": "Chưa đăng nhập hoặc token hết hạn"
}
```

#### Forbidden (403 Forbidden)
```json
{
  "success": false,
  "message": "Bạn không có quyền cập nhật user này"
}
```

#### Server Error (500 Internal Server Error)
```json
{
  "success": false,
  "message": "Đã xảy ra lỗi hệ thống"
}
```

---

## ✅ Validation Rules

### Basic Information

| Field | Rules | Error Message |
|-------|-------|---------------|
| `firstName` | MaxLength(100) | "Họ không được quá 100 ký tự" |
| `lastName` | MaxLength(100) | "Tên không được quá 100 ký tự" |
| `phoneNumber` | Phone Format | "Số điện thoại không hợp lệ" |
| `dateOfBirth` | Valid Date, MinAge: 16, MaxAge: 100 | "Ngày sinh không hợp lệ" |
| `gender` | Enum (0-4) | "Giới tính không hợp lệ" |
| `avatar` | MaxLength(500), Valid URL | "URL avatar không hợp lệ" |

### Employee Fields

| Field | Rules | Required When | Error Message |
|-------|-------|---------------|---------------|
| `nationalIdCard` | MaxLength(20), Numbers only | Role = Staff/Admin | "CCCD không hợp lệ" |
| `hireDate` | Valid Date, Not future | Role = Staff/Admin | "Ngày vào làm không hợp lệ" |
| `baseSalary` | Range(0, 100000000) | Role = Staff/Admin | "Lương phải từ 0 đến 100,000,000" |

### Role & Status

| Field | Rules | Error Message |
|-------|-------|---------------|
| `role` | Enum (1-4) | "Role không hợp lệ" |
| `status` | Enum (1-5) | "Status không hợp lệ" |
| `isActive` | Boolean | "isActive phải là true hoặc false" |
| `tier` | Enum (1-5), Only for Customer | "Tier không hợp lệ" |

### Business Rules

1. **Email không thể thay đổi**: Email là unique identifier
2. **Role restrictions**: Staff không thể nâng cấp lên Admin
3. **Tier auto-calculation**: Tier được tính dựa trên totalSpent, nhưng Admin có thể override
4. **Employee fields**: Chỉ áp dụng khi Role = Staff/Admin
5. **Points & Spent**: Chỉ hệ thống mới có thể thay đổi (không qua API này)

---

## 💻 Code Examples

### JavaScript/Fetch API

```javascript
/**
 * Update user information
 * @param {string} userId - User ID (GUID)
 * @param {UpdateUserRequest} userData - Update data
 * @returns {Promise<ApiResponse>}
 */
const updateUser = async (userId, userData) => {
  const token = localStorage.getItem('adminToken');
  
  const response = await fetch(
    `https://localhost:7240/api/admin/users/${userId}/update`,
    {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(userData)
    }
  );

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Cập nhật thất bại');
  }

  return await response.json();
};

// Usage Examples

// 1. Update basic info
try {
  const result = await updateUser('123e4567-e89b-12d3-a456-426614174000', {
    firstName: 'Văn A',
    lastName: 'Nguyễn',
    phoneNumber: '0901234567'
  });
  console.log('Updated:', result.data);
} catch (error) {
  console.error('Error:', error.message);
}

// 2. Change user role
try {
  const result = await updateUser('123e4567-e89b-12d3-a456-426614174000', {
    role: 2 // Staff
  });
  console.log('Role changed:', result.data);
} catch (error) {
  console.error('Error:', error.message);
}

// 3. Update employee information
try {
  const result = await updateUser('123e4567-e89b-12d3-a456-426614174000', {
    nationalIdCard: '001234567890',
    hireDate: '2024-01-01',
    baseSalary: 15000000
  });
  console.log('Employee updated:', result.data);
} catch (error) {
  console.error('Error:', error.message);
}
```

### Axios

```javascript
import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'https://localhost:7240/api',
  headers: {
    'Content-Type': 'application/json'
  }
});

// Add token to all requests
apiClient.interceptors.request.use(config => {
  const token = localStorage.getItem('adminToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

/**
 * Update user
 */
const updateUser = async (userId, userData) => {
  try {
    const response = await apiClient.put(
      `/admin/users/${userId}/update`,
      userData
    );
    return response.data;
  } catch (error) {
    if (error.response) {
      // Server responded with error
      throw new Error(error.response.data.message);
    } else if (error.request) {
      // No response from server
      throw new Error('Không thể kết nối đến server');
    } else {
      throw error;
    }
  }
};

// Usage
updateUser('123e4567-e89b-12d3-a456-426614174000', {
  firstName: 'Văn A',
  lastName: 'Nguyễn'
})
  .then(result => console.log('Success:', result))
  .catch(error => console.error('Error:', error.message));
```

### TypeScript with Type Safety

```typescript
import axios, { AxiosInstance } from 'axios';

// Types
interface UpdateUserRequest {
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  gender?: Gender;
  avatar?: string;
  nationalIdCard?: string;
  hireDate?: string;
  baseSalary?: number;
  role?: UserRole;
  status?: AccountStatus;
  isActive?: boolean;
  tier?: UserTier;
}

interface UserSummaryDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  phoneNumber?: string;
  role: string;
  status: string;
  isActive: boolean;
  emailVerified: boolean;
  phoneVerified: boolean;
  tier?: string;
  loyaltyPoints: number;
  totalSpent: number;
  orderCount: number;
  avatar?: string;
  dateOfBirth?: string;
  gender?: string;
  createdAt: string;
  updatedAt: string;
  lastLoginAt?: string;
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: Record<string, string[]>;
}

enum Gender {
  Unknown = 0,
  Male = 1,
  Female = 2,
  Other = 3,
  PreferNotToSay = 4
}

enum UserRole {
  Customer = 1,
  Staff = 2,
  Admin = 3,
  SuperAdmin = 4
}

enum AccountStatus {
  Pending = 1,
  Active = 2,
  Suspended = 3,
  Banned = 4,
  Inactive = 5
}

enum UserTier {
  Bronze = 1,
  Silver = 2,
  Gold = 3,
  Platinum = 4,
  Diamond = 5
}

// API Service
class AdminUserService {
  private api: AxiosInstance;

  constructor(baseURL: string) {
    this.api = axios.create({
      baseURL,
      headers: {
        'Content-Type': 'application/json'
      }
    });

    // Add token interceptor
    this.api.interceptors.request.use(config => {
      const token = localStorage.getItem('adminToken');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });
  }

  async updateUser(
    userId: string,
    userData: UpdateUserRequest
  ): Promise<ApiResponse<UserSummaryDto>> {
    try {
      const response = await this.api.put<ApiResponse<UserSummaryDto>>(
        `/admin/users/${userId}/update`,
        userData
      );
      return response.data;
    } catch (error: any) {
      if (error.response) {
        return error.response.data;
      }
      throw error;
    }
  }
}

// Usage
const adminService = new AdminUserService('https://localhost:7240/api');

const updateUserExample = async () => {
  const result = await adminService.updateUser(
    '123e4567-e89b-12d3-a456-426614174000',
    {
      firstName: 'Văn A',
      lastName: 'Nguyễn',
      role: UserRole.Customer,
      tier: UserTier.Gold
    }
  );

  if (result.success) {
    console.log('User updated:', result.data);
  } else {
    console.error('Update failed:', result.message);
  }
};
```

---

## 🔥 Error Handling

### Comprehensive Error Handler

```javascript
/**
 * Handle API errors with user-friendly messages
 */
const handleUpdateError = (error, response) => {
  // Validation errors (400)
  if (response?.status === 400 && response.data?.errors) {
    const errors = response.data.errors;
    const errorMessages = Object.entries(errors)
      .map(([field, messages]) => `${field}: ${messages.join(', ')}`)
      .join('\n');
    
    return {
      title: 'Dữ liệu không hợp lệ',
      message: errorMessages,
      type: 'validation'
    };
  }

  // Not found (404)
  if (response?.status === 404) {
    return {
      title: 'Không tìm thấy',
      message: 'Không tìm thấy user này trong hệ thống',
      type: 'not_found'
    };
  }

  // Forbidden (403)
  if (response?.status === 403) {
    return {
      title: 'Không có quyền',
      message: 'Bạn không có quyền cập nhật user này',
      type: 'forbidden'
    };
  }

  // Unauthorized (401)
  if (response?.status === 401) {
    return {
      title: 'Chưa đăng nhập',
      message: 'Vui lòng đăng nhập lại',
      type: 'unauthorized'
    };
  }

  // Server error (500)
  if (response?.status === 500) {
    return {
      title: 'Lỗi hệ thống',
      message: 'Đã xảy ra lỗi, vui lòng thử lại sau',
      type: 'server_error'
    };
  }

  // Network error
  return {
    title: 'Lỗi kết nối',
    message: 'Không thể kết nối đến server',
    type: 'network_error'
  };
};

// Usage with try-catch
const updateUserWithErrorHandling = async (userId, userData) => {
  try {
    const result = await updateUser(userId, userData);
    
    if (result.success) {
      showSuccessNotification('Cập nhật thành công!');
      return result.data;
    } else {
      const errorInfo = handleUpdateError(null, { 
        status: 400, 
        data: result 
      });
      showErrorNotification(errorInfo.title, errorInfo.message);
      return null;
    }
  } catch (error) {
    const errorInfo = handleUpdateError(error, error.response);
    showErrorNotification(errorInfo.title, errorInfo.message);
    
    // Handle specific error types
    if (errorInfo.type === 'unauthorized') {
      redirectToLogin();
    }
    
    return null;
  }
};
```

### Client-side Validation

```javascript
/**
 * Validate update data before sending to server
 */
const validateUpdateData = (data) => {
  const errors = {};

  // First name
  if (data.firstName !== undefined) {
    if (data.firstName.trim().length === 0) {
      errors.firstName = 'Họ không được để trống';
    } else if (data.firstName.length > 100) {
      errors.firstName = 'Họ không được quá 100 ký tự';
    }
  }

  // Last name
  if (data.lastName !== undefined) {
    if (data.lastName.trim().length === 0) {
      errors.lastName = 'Tên không được để trống';
    } else if (data.lastName.length > 100) {
      errors.lastName = 'Tên không được quá 100 ký tự';
    }
  }

  // Phone number
  if (data.phoneNumber !== undefined) {
    const phoneRegex = /^[0-9]{10,11}$/;
    if (!phoneRegex.test(data.phoneNumber.replace(/[\s-]/g, ''))) {
      errors.phoneNumber = 'Số điện thoại phải có 10-11 chữ số';
    }
  }

  // Date of birth
  if (data.dateOfBirth !== undefined) {
    const dob = new Date(data.dateOfBirth);
    const age = (new Date() - dob) / (365.25 * 24 * 60 * 60 * 1000);
    
    if (age < 16) {
      errors.dateOfBirth = 'Phải ít nhất 16 tuổi';
    } else if (age > 100) {
      errors.dateOfBirth = 'Tuổi không hợp lệ';
    }
  }

  // Base salary
  if (data.baseSalary !== undefined) {
    if (data.baseSalary < 0 || data.baseSalary > 100000000) {
      errors.baseSalary = 'Lương phải từ 0 đến 100,000,000';
    }
  }

  // National ID Card
  if (data.nationalIdCard !== undefined) {
    const cccdRegex = /^[0-9]{9,12}$/;
    if (!cccdRegex.test(data.nationalIdCard)) {
      errors.nationalIdCard = 'CCCD phải có 9-12 chữ số';
    }
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors
  };
};

// Usage
const safeUpdateUser = async (userId, userData) => {
  // Validate first
  const validation = validateUpdateData(userData);
  
  if (!validation.isValid) {
    showValidationErrors(validation.errors);
    return null;
  }

  // Then call API
  return await updateUser(userId, userData);
};
```

---

## ⚛️ React Component Example

### Complete Edit User Form

```jsx
import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';

const EditUserPage = () => {
  const { userId } = useParams();
  const navigate = useNavigate();
  
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [user, setUser] = useState(null);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    phoneNumber: '',
    dateOfBirth: '',
    gender: 1,
    role: 1,
    status: 2,
    tier: 1,
    nationalIdCard: '',
    hireDate: '',
    baseSalary: 0,
    isActive: true
  });
  const [errors, setErrors] = useState({});

  // Load user data
  useEffect(() => {
    loadUserData();
  }, [userId]);

  const loadUserData = async () => {
    try {
      setLoading(true);
      const response = await fetch(
        `https://localhost:7240/api/admin/users/${userId}/detail`,
        {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('adminToken')}`
          }
        }
      );

      if (!response.ok) throw new Error('Failed to load user');

      const result = await response.json();
      if (result.success) {
        setUser(result.data);
        setFormData({
          firstName: result.data.firstName || '',
          lastName: result.data.lastName || '',
          phoneNumber: result.data.phoneNumber || '',
          dateOfBirth: result.data.dateOfBirth?.split('T')[0] || '',
          gender: getGenderValue(result.data.gender),
          role: getRoleValue(result.data.role),
          status: getStatusValue(result.data.status),
          tier: getTierValue(result.data.tier),
          nationalIdCard: result.data.nationalIdCard || '',
          hireDate: result.data.hireDate?.split('T')[0] || '',
          baseSalary: result.data.baseSalary || 0,
          isActive: result.data.isActive
        });
      }
    } catch (error) {
      alert('Không thể tải thông tin user: ' + error.message);
      navigate('/admin/users');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
    
    // Clear error when user types
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: null }));
    }
  };

  const validate = () => {
    const newErrors = {};

    if (!formData.firstName.trim()) {
      newErrors.firstName = 'Họ là bắt buộc';
    } else if (formData.firstName.length > 100) {
      newErrors.firstName = 'Họ không được quá 100 ký tự';
    }

    if (!formData.lastName.trim()) {
      newErrors.lastName = 'Tên là bắt buộc';
    } else if (formData.lastName.length > 100) {
      newErrors.lastName = 'Tên không được quá 100 ký tự';
    }

    if (formData.phoneNumber) {
      const phoneRegex = /^[0-9]{10,11}$/;
      if (!phoneRegex.test(formData.phoneNumber.replace(/[\s-]/g, ''))) {
        newErrors.phoneNumber = 'Số điện thoại không hợp lệ';
      }
    }

    if ((formData.role === 2 || formData.role === 3) && !formData.nationalIdCard) {
      newErrors.nationalIdCard = 'CCCD là bắt buộc cho nhân viên';
    }

    if (formData.baseSalary < 0 || formData.baseSalary > 100000000) {
      newErrors.baseSalary = 'Lương phải từ 0 đến 100,000,000';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validate()) {
      alert('Vui lòng kiểm tra lại dữ liệu');
      return;
    }

    try {
      setSaving(true);

      // Build update data (only changed fields)
      const updateData = {};
      Object.keys(formData).forEach(key => {
        const newValue = formData[key];
        const oldValue = user[key];
        
        if (newValue !== oldValue) {
          updateData[key] = newValue;
        }
      });

      // Convert enum values to integers
      if (updateData.gender !== undefined) updateData.gender = parseInt(updateData.gender);
      if (updateData.role !== undefined) updateData.role = parseInt(updateData.role);
      if (updateData.status !== undefined) updateData.status = parseInt(updateData.status);
      if (updateData.tier !== undefined) updateData.tier = parseInt(updateData.tier);
      if (updateData.baseSalary !== undefined) updateData.baseSalary = parseFloat(updateData.baseSalary);

      const response = await fetch(
        `https://localhost:7240/api/admin/users/${userId}/update`,
        {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${localStorage.getItem('adminToken')}`
          },
          body: JSON.stringify(updateData)
        }
      );

      const result = await response.json();

      if (result.success) {
        alert('Cập nhật user thành công!');
        navigate('/admin/users');
      } else {
        if (result.errors) {
          setErrors(result.errors);
        }
        alert(result.message || 'Cập nhật thất bại');
      }
    } catch (error) {
      alert('Lỗi: ' + error.message);
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <div className="text-xl">Đang tải...</div>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto p-6">
      <div className="bg-white rounded-lg shadow-md p-6">
        <h1 className="text-2xl font-bold mb-6">Chỉnh sửa User</h1>

        <form onSubmit={handleSubmit}>
          {/* Basic Information */}
          <div className="mb-6">
            <h2 className="text-lg font-semibold mb-4">Thông tin cơ bản</h2>
            
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block mb-2 font-medium">
                  Họ <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  name="firstName"
                  value={formData.firstName}
                  onChange={handleChange}
                  className={`w-full px-3 py-2 border rounded-md ${
                    errors.firstName ? 'border-red-500' : 'border-gray-300'
                  }`}
                  maxLength={100}
                />
                {errors.firstName && (
                  <p className="text-red-500 text-sm mt-1">{errors.firstName}</p>
                )}
              </div>

              <div>
                <label className="block mb-2 font-medium">
                  Tên <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  name="lastName"
                  value={formData.lastName}
                  onChange={handleChange}
                  className={`w-full px-3 py-2 border rounded-md ${
                    errors.lastName ? 'border-red-500' : 'border-gray-300'
                  }`}
                  maxLength={100}
                />
                {errors.lastName && (
                  <p className="text-red-500 text-sm mt-1">{errors.lastName}</p>
                )}
              </div>

              <div>
                <label className="block mb-2 font-medium">Email (Không thể sửa)</label>
                <input
                  type="email"
                  value={user?.email || ''}
                  disabled
                  className="w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-100"
                />
              </div>

              <div>
                <label className="block mb-2 font-medium">Số điện thoại</label>
                <input
                  type="tel"
                  name="phoneNumber"
                  value={formData.phoneNumber}
                  onChange={handleChange}
                  className={`w-full px-3 py-2 border rounded-md ${
                    errors.phoneNumber ? 'border-red-500' : 'border-gray-300'
                  }`}
                />
                {errors.phoneNumber && (
                  <p className="text-red-500 text-sm mt-1">{errors.phoneNumber}</p>
                )}
              </div>

              <div>
                <label className="block mb-2 font-medium">Ngày sinh</label>
                <input
                  type="date"
                  name="dateOfBirth"
                  value={formData.dateOfBirth}
                  onChange={handleChange}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                />
              </div>

              <div>
                <label className="block mb-2 font-medium">Giới tính</label>
                <select
                  name="gender"
                  value={formData.gender}
                  onChange={handleChange}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                >
                  <option value={0}>Không xác định</option>
                  <option value={1}>Nam</option>
                  <option value={2}>Nữ</option>
                  <option value={3}>Khác</option>
                  <option value={4}>Không muốn nói</option>
                </select>
              </div>
            </div>
          </div>

          {/* Role & Status */}
          <div className="mb-6">
            <h2 className="text-lg font-semibold mb-4">Vai trò & Trạng thái</h2>
            
            <div className="grid grid-cols-3 gap-4">
              <div>
                <label className="block mb-2 font-medium">Vai trò</label>
                <select
                  name="role"
                  value={formData.role}
                  onChange={handleChange}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                >
                  <option value={1}>Customer</option>
                  <option value={2}>Staff</option>
                  <option value={3}>Admin</option>
                  <option value={4}>SuperAdmin</option>
                </select>
              </div>

              <div>
                <label className="block mb-2 font-medium">Trạng thái</label>
                <select
                  name="status"
                  value={formData.status}
                  onChange={handleChange}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                >
                  <option value={1}>Pending</option>
                  <option value={2}>Active</option>
                  <option value={3}>Suspended</option>
                  <option value={4}>Banned</option>
                  <option value={5}>Inactive</option>
                </select>
              </div>

              <div>
                <label className="block mb-2 font-medium">Hoạt động</label>
                <div className="flex items-center h-10">
                  <input
                    type="checkbox"
                    name="isActive"
                    checked={formData.isActive}
                    onChange={handleChange}
                    className="w-5 h-5"
                  />
                  <span className="ml-2">Đang hoạt động</span>
                </div>
              </div>
            </div>

            {formData.role === 1 && (
              <div className="mt-4">
                <label className="block mb-2 font-medium">Hạng thành viên</label>
                <select
                  name="tier"
                  value={formData.tier}
                  onChange={handleChange}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                >
                  <option value={1}>Bronze</option>
                  <option value={2}>Silver</option>
                  <option value={3}>Gold</option>
                  <option value={4}>Platinum</option>
                  <option value={5}>Diamond</option>
                </select>
              </div>
            )}
          </div>

          {/* Employee Information */}
          {(formData.role === 2 || formData.role === 3 || formData.role === 4) && (
            <div className="mb-6">
              <h2 className="text-lg font-semibold mb-4">Thông tin nhân viên</h2>
              
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block mb-2 font-medium">
                    CCCD <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    name="nationalIdCard"
                    value={formData.nationalIdCard}
                    onChange={handleChange}
                    className={`w-full px-3 py-2 border rounded-md ${
                      errors.nationalIdCard ? 'border-red-500' : 'border-gray-300'
                    }`}
                    maxLength={20}
                  />
                  {errors.nationalIdCard && (
                    <p className="text-red-500 text-sm mt-1">{errors.nationalIdCard}</p>
                  )}
                </div>

                <div>
                  <label className="block mb-2 font-medium">Ngày vào làm</label>
                  <input
                    type="date"
                    name="hireDate"
                    value={formData.hireDate}
                    onChange={handleChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  />
                </div>

                <div className="col-span-2">
                  <label className="block mb-2 font-medium">Lương cơ bản (VND)</label>
                  <input
                    type="number"
                    name="baseSalary"
                    value={formData.baseSalary}
                    onChange={handleChange}
                    className={`w-full px-3 py-2 border rounded-md ${
                      errors.baseSalary ? 'border-red-500' : 'border-gray-300'
                    }`}
                    min={0}
                    max={100000000}
                    step={100000}
                  />
                  {errors.baseSalary && (
                    <p className="text-red-500 text-sm mt-1">{errors.baseSalary}</p>
                  )}
                </div>
              </div>
            </div>
          )}

          {/* Buttons */}
          <div className="flex justify-end gap-4 mt-6 pt-6 border-t">
            <button
              type="button"
              onClick={() => navigate('/admin/users')}
              disabled={saving}
              className="px-6 py-2 border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50"
            >
              Hủy
            </button>
            <button
              type="submit"
              disabled={saving}
              className="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50"
            >
              {saving ? 'Đang lưu...' : 'Lưu thay đổi'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

// Helper functions
const getGenderValue = (gender) => {
  const map = { 'Unknown': 0, 'Male': 1, 'Female': 2, 'Other': 3, 'PreferNotToSay': 4 };
  return map[gender] || 0;
};

const getRoleValue = (role) => {
  const map = { 'Customer': 1, 'Staff': 2, 'Admin': 3, 'SuperAdmin': 4 };
  return map[role] || 1;
};

const getStatusValue = (status) => {
  const map = { 'Pending': 1, 'Active': 2, 'Suspended': 3, 'Banned': 4, 'Inactive': 5 };
  return map[status] || 2;
};

const getTierValue = (tier) => {
  const map = { 'Bronze': 1, 'Silver': 2, 'Gold': 3, 'Platinum': 4, 'Diamond': 5 };
  return map[tier] || 1;
};

export default EditUserPage;
```

---

## 🧪 Testing Guide

### Manual Testing with HTTP File

```http
### Update User - Basic Info
PUT https://localhost:7240/api/admin/users/123e4567-e89b-12d3-a456-426614174000/update
Content-Type: application/json
Authorization: Bearer {{adminToken}}

{
  "firstName": "Văn A",
  "lastName": "Nguyễn",
  "phoneNumber": "0901234567",
  "dateOfBirth": "1990-05-15",
  "gender": 1
}

### Update User - Change Role
PUT https://localhost:7240/api/admin/users/123e4567-e89b-12d3-a456-426614174000/update
Content-Type: application/json
Authorization: Bearer {{adminToken}}

{
  "role": 2
}

### Update User - Employee Info
PUT https://localhost:7240/api/admin/users/123e4567-e89b-12d3-a456-426614174000/update
Content-Type: application/json
Authorization: Bearer {{adminToken}}

{
  "nationalIdCard": "001234567890",
  "hireDate": "2024-01-01",
  "baseSalary": 15000000
}

### Update User - Status & Tier
PUT https://localhost:7240/api/admin/users/123e4567-e89b-12d3-a456-426614174000/update
Content-Type: application/json
Authorization: Bearer {{adminToken}}

{
  "status": 2,
  "tier": 3,
  "isActive": true
}

### Update User - Multiple Fields
PUT https://localhost:7240/api/admin/users/123e4567-e89b-12d3-a456-426614174000/update
Content-Type: application/json
Authorization: Bearer {{adminToken}}

{
  "firstName": "Thị B",
  "lastName": "Trần",
  "phoneNumber": "0987654321",
  "dateOfBirth": "1995-08-20",
  "gender": 2,
  "role": 1,
  "tier": 2,
  "status": 2,
  "isActive": true
}

### Validation Test - Invalid Phone
PUT https://localhost:7240/api/admin/users/123e4567-e89b-12d3-a456-426614174000/update
Content-Type: application/json
Authorization: Bearer {{adminToken}}

{
  "phoneNumber": "123"
}

### Validation Test - Invalid Salary
PUT https://localhost:7240/api/admin/users/123e4567-e89b-12d3-a456-426614174000/update
Content-Type: application/json
Authorization: Bearer {{adminToken}}

{
  "baseSalary": 200000000
}

### Not Found Test
PUT https://localhost:7240/api/admin/users/00000000-0000-0000-0000-000000000000/update
Content-Type: application/json
Authorization: Bearer {{adminToken}}

{
  "firstName": "Test"
}
```

### Automated Tests (Jest)

```javascript
describe('Update User API', () => {
  const API_URL = 'https://localhost:7240/api/admin/users';
  let token;
  let testUserId;

  beforeAll(async () => {
    // Login to get token
    const loginResponse = await fetch('https://localhost:7240/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        email: 'admin@sakura.com',
        password: 'Admin@123'
      })
    });
    const loginResult = await loginResponse.json();
    token = loginResult.data.token;
    
    // Create test user
    const createResponse = await fetch(`${API_URL}/create`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        firstName: 'Test',
        lastName: 'User',
        email: 'testuser@example.com',
        password: 'Test@123',
        role: 1
      })
    });
    const createResult = await createResponse.json();
    testUserId = createResult.data.id;
  });

  afterAll(async () => {
    // Delete test user
    await fetch(`${API_URL}/${testUserId}/delete`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${token}` }
    });
  });

  test('Should update basic information', async () => {
    const response = await fetch(`${API_URL}/${testUserId}/update`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        firstName: 'Updated',
        lastName: 'Name'
      })
    });

    expect(response.status).toBe(200);
    const result = await response.json();
    expect(result.success).toBe(true);
    expect(result.data.firstName).toBe('Updated');
    expect(result.data.lastName).toBe('Name');
  });

  test('Should validate phone number', async () => {
    const response = await fetch(`${API_URL}/${testUserId}/update`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        phoneNumber: 'invalid'
      })
    });

    expect(response.status).toBe(400);
    const result = await response.json();
    expect(result.success).toBe(false);
    expect(result.errors).toHaveProperty('PhoneNumber');
  });

  test('Should reject excessive salary', async () => {
    const response = await fetch(`${API_URL}/${testUserId}/update`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        baseSalary: 200000000
      })
    });

    expect(response.status).toBe(400);
    const result = await response.json();
    expect(result.success).toBe(false);
  });

  test('Should return 404 for non-existent user', async () => {
    const response = await fetch(`${API_URL}/00000000-0000-0000-0000-000000000000/update`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        firstName: 'Test'
      })
    });

    expect(response.status).toBe(404);
  });

  test('Should require authentication', async () => {
    const response = await fetch(`${API_URL}/${testUserId}/update`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        firstName: 'Test'
      })
    });

    expect(response.status).toBe(401);
  });
});
```

---

## 💡 Best Practices

### 1. **Partial Updates Only**
```javascript
// ✅ Good: Only send changed fields
const updateData = {
  firstName: 'New Name'
};

// ❌ Bad: Sending all fields
const updateData = {
  firstName: 'New Name',
  lastName: user.lastName,
  email: user.email,
  // ... all other fields
};
```

### 2. **Client-side Validation First**
```javascript
// Always validate before API call
const validation = validateUpdateData(formData);
if (!validation.isValid) {
  showErrors(validation.errors);
  return;
}

// Then call API
await updateUser(userId, formData);
```

### 3. **Handle All Error Cases**
```javascript
try {
  const result = await updateUser(userId, data);
  if (result.success) {
    showSuccess('Updated successfully');
  } else {
    handleApiError(result);
  }
} catch (error) {
  if (error.response?.status === 401) {
    redirectToLogin();
  } else if (error.response?.status === 403) {
    showError('Permission denied');
  } else {
    showError('Network error');
  }
}
```

### 4. **Show Loading States**
```javascript
const [saving, setSaving] = useState(false);

const handleUpdate = async () => {
  setSaving(true);
  try {
    await updateUser(userId, data);
  } finally {
    setSaving(false);
  }
};

// In UI
<button disabled={saving}>
  {saving ? 'Saving...' : 'Save Changes'}
</button>
```

### 5. **Optimistic Updates (Optional)**
```javascript
// Update UI immediately
setUser(prevUser => ({ ...prevUser, ...newData }));

// Then sync with server
try {
  await updateUser(userId, newData);
} catch (error) {
  // Rollback on error
  setUser(originalUser);
  showError('Update failed');
}
```

### 6. **Debounce API Calls**
```javascript
import { debounce } from 'lodash';

const debouncedUpdate = debounce(async (userId, data) => {
  await updateUser(userId, data);
}, 500);

// Use in onChange handlers
const handleFieldChange = (field, value) => {
  debouncedUpdate(userId, { [field]: value });
};
```

### 7. **Use Enums Consistently**
```javascript
// Define enums
const Gender = {
  Unknown: 0,
  Male: 1,
  Female: 2,
  Other: 3,
  PreferNotToSay: 4
};

// Use in code
const updateData = {
  gender: Gender.Male
};
```

### 8. **Type Safety with TypeScript**
```typescript
// Always use proper types
interface UpdateUserRequest {
  firstName?: string;
  // ... other fields
}

const updateUser = async (
  userId: string,
  data: UpdateUserRequest
): Promise<ApiResponse<UserSummaryDto>> => {
  // Implementation
};
```

### 9. **Audit Trail**
```javascript
// Backend automatically tracks:
// - Who updated (from JWT token)
// - When updated (timestamp)
// - What changed (audit log)

// Frontend should show:
console.log('Updated by:', result.data.updatedBy);
console.log('Updated at:', result.data.updatedAt);
```

### 10. **Security Considerations**
```javascript
// Never trust client-side data
// Backend validates everything

// Don't expose sensitive data
// Backend filters sensitive fields in response

// Use HTTPS only
const API_URL = 'https://api.sakurahome.com';

// Validate JWT token
if (isTokenExpired(token)) {
  await refreshToken();
}
```

---

## 📞 Support & Resources

### API Documentation
- **Postman Collection**: [Download](../Tests/Admin-User-Management.http)
- **Swagger UI**: `https://localhost:7240/swagger`
- **API Reference**: [Admin-User-Management-API.md](./Admin-User-Management-API.md)

### Related APIs
- **Get User Detail**: `GET /api/admin/users/{userId}/detail`
- **Create User**: `POST /api/admin/users/create`
- **Delete User**: `DELETE /api/admin/users/{userId}/delete`
- **Toggle Status**: `PATCH /api/admin/users/{userId}/toggle`

### Contact
- **Backend Team**: backend@sakurahome.com
- **API Support**: api-support@sakurahome.com
- **Documentation**: docs@sakurahome.com

---

## 📋 Changelog

### Version 1.0.0 (2024-06-15)
- ✅ Initial release
- ✅ Full CRUD operations for users
- ✅ Role-based updates
- ✅ Comprehensive validation
- ✅ Audit trail support

---

**Tài liệu này cung cấp đầy đủ thông tin để Frontend Developer có thể implement tính năng Update User một cách hoàn chỉnh và chuyên nghiệp! 🚀**
