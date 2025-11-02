# 👥 SakuraHome API - Tài liệu Quản lý User cho Admin

## 📋 Tổng quan

Tài liệu này cung cấp **hướng dẫn đầy đủ** cho Frontend Developers về các chức năng **quản lý người dùng** trong trang Admin, bao gồm:
- ✅ Xem danh sách users
- ✅ Tìm kiếm & lọc users
- ✅ Xem chi tiết user
- ✅ **Chỉnh sửa thông tin user**
- ✅ Tạo user mới
- ✅ Khóa/Mở khóa tài khoản
- ✅ Xóa user

---

## 🔧 Cấu hình cơ bản

### Base URL
```javascript
const API_BASE_URL = 'https://localhost:7240/api';
```

### Authentication
Tất cả endpoints yêu cầu **JWT token** của admin/staff:
```javascript
const authHeaders = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${localStorage.getItem('adminToken')}`
};
```

### Kiểm tra quyền Admin
```javascript
const checkAdminRole = () => {
    const adminInfo = JSON.parse(localStorage.getItem('adminInfo') || '{}');
    return ['Admin', 'Staff'].includes(adminInfo.role);
};
```

---

## 📊 1. XEM DANH SÁCH USERS

### Endpoint
```http
GET /api/admin/users/list
```

### Parameters
| Tên | Type | Bắt buộc | Mặc định | Mô tả |
|-----|------|----------|----------|-------|
| `pageNumber` | int | Không | 1 | Trang hiện tại |
| `pageSize` | int | Không | 20 | Số items/trang (max: 100) |
| `searchTerm` | string | Không | null | Tìm theo email, tên, số điện thoại |
| `role` | string | Không | null | Lọc theo role: "Customer", "Staff", "Admin" |
| `status` | string | Không | null | Lọc theo status: "Active", "Inactive", "Locked", "Suspended" |
| `isActive` | bool | Không | null | Lọc active: true/false |

### Code mẫu
```javascript
// Hàm lấy danh sách users
const getUsersList = async (filters = {}) => {
    const params = new URLSearchParams({
        pageNumber: filters.page || 1,
        pageSize: filters.pageSize || 20,
        ...(filters.searchTerm && { searchTerm: filters.searchTerm }),
        ...(filters.role && { role: filters.role }),
        ...(filters.status && { status: filters.status }),
        ...(filters.isActive !== undefined && { isActive: filters.isActive })
    });

    const response = await fetch(`${API_BASE_URL}/admin/users/list?${params}`, {
        method: 'GET',
        headers: authHeaders
    });

    if (!response.ok) {
        throw new Error('Không thể lấy danh sách users');
    }

    return await response.json();
};

// Sử dụng
const result = await getUsersList({
    page: 1,
    pageSize: 20,
    searchTerm: 'nguyen',
    role: 'Customer',
    status: 'Active'
});
```

### Response mẫu
```json
{
    "success": true,
    "message": "Lấy danh sách users thành công",
    "data": {
        "items": [
            {
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
                "lastLoginAt": "2024-06-01T14:20:00Z"
            }
        ],
        "pagination": {
            "currentPage": 1,
            "pageSize": 20,
            "totalPages": 10,
            "totalItems": 195,
            "hasNextPage": true,
            "hasPreviousPage": false
        }
    }
}
```

---

## 🔍 2. XEM CHI TIẾT USER

### Endpoint
```http
GET /api/admin/users/{userId}/detail
```

### Parameters
| Tên | Type | Bắt buộc | Mô tả |
|-----|------|----------|-------|
| `userId` | guid | Có | ID của user cần xem |

### Code mẫu
```javascript
const getUserDetail = async (userId) => {
    const response = await fetch(`${API_BASE_URL}/admin/users/${userId}/detail`, {
        method: 'GET',
        headers: authHeaders
    });

    if (!response.ok) {
        if (response.status === 404) {
            throw new Error('Không tìm thấy user');
        }
        throw new Error('Không thể lấy thông tin user');
    }

    return await response.json();
};

// Sử dụng
const userDetail = await getUserDetail('123e4567-e89b-12d3-a456-426614174000');
```

### Response mẫu
```json
{
    "success": true,
    "message": "Lấy thông tin user thành công",
    "data": {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "email": "nguyenvana@example.com",
        "firstName": "Văn A",
        "lastName": "Nguyễn",
        "fullName": "Nguyễn Văn A",
        "phoneNumber": "0901234567",
        "phoneVerified": true,
        "emailVerified": true,
        "role": "Customer",
        "status": "Active",
        "isActive": true,
        "avatar": "https://example.com/avatar.jpg",
        "dateOfBirth": "1990-05-15",
        "gender": "Male",
        "nationalIdCard": null,
        "hireDate": null,
        "baseSalary": null,
        "tier": "Gold",
        "loyaltyPoints": 1500,
        "totalSpent": 15000000,
        "orderCount": 25,
        "reviewCount": 12,
        "preferredLanguage": "vi",
        "preferredCurrency": "VND",
        "emailNotifications": true,
        "smsNotifications": false,
        "pushNotifications": true,
        "createdAt": "2023-01-15T10:30:00Z",
        "updatedAt": "2024-05-20T16:45:00Z",
        "lastLoginAt": "2024-06-01T14:20:00Z",
        "addresses": [
            {
                "id": "addr-001",
                "fullName": "Nguyễn Văn A",
                "phoneNumber": "0901234567",
                "addressLine": "123 Đường ABC",
                "ward": "Phường 1",
                "district": "Quận 1",
                "city": "TP. Hồ Chí Minh",
                "isDefault": true
            }
        ]
    }
}
```

---

## ✏️ 3. CHỈNH SỬA USER (QUAN TRỌNG)

### Endpoint
```http
PUT /api/admin/users/{userId}/update
```

### Parameters
| Tên | Type | Bắt buộc | Mô tả |
|-----|------|----------|-------|
| `userId` | guid | Có | ID của user cần update |

### Request Body
```typescript
interface UpdateUserRequest {
    // Thông tin cơ bản
    firstName?: string;           // Họ (max 100 ký tự)
    lastName?: string;            // Tên (max 100 ký tự)
    phoneNumber?: string;         // Số điện thoại (format hợp lệ)
    dateOfBirth?: string;         // Ngày sinh (ISO format)
    gender?: "Male" | "Female" | "Other";
    avatar?: string;              // URL avatar (max 500 ký tự)
    
    // Thông tin nhân viên (nếu role = Staff/Admin)
    nationalIdCard?: string;      // CCCD (max 20 ký tự)
    hireDate?: string;            // Ngày vào làm (ISO format)
    baseSalary?: number;          // Lương cơ bản (0-100,000,000)
    
    // Role & Status
    role?: "Customer" | "Staff" | "Admin";
    status?: "Active" | "Inactive" | "Locked" | "Suspended";
    isActive?: boolean;
    
    // Customer tier (chỉ cho Customer)
    tier?: "Bronze" | "Silver" | "Gold" | "Platinum";
}
```

### Validation Rules
| Field | Quy tắc |
|-------|---------|
| `firstName` | Max 100 ký tự |
| `lastName` | Max 100 ký tự |
| `phoneNumber` | Phải đúng format số điện thoại |
| `dateOfBirth` | Phải là ngày hợp lệ (ISO 8601) |
| `gender` | Male, Female, hoặc Other |
| `avatar` | Max 500 ký tự, phải là URL hợp lệ |
| `nationalIdCard` | Max 20 ký tự (chỉ cho Staff/Admin) |
| `hireDate` | Phải là ngày hợp lệ |
| `baseSalary` | 0 - 100,000,000 |
| `role` | Customer, Staff, hoặc Admin |
| `status` | Active, Inactive, Locked, Suspended |
| `tier` | Bronze, Silver, Gold, Platinum (chỉ Customer) |

### Code mẫu đầy đủ
```javascript
/**
 * Cập nhật thông tin user
 * @param {string} userId - ID của user
 * @param {UpdateUserRequest} userData - Dữ liệu cập nhật
 * @returns {Promise<ApiResponse>}
 */
const updateUser = async (userId, userData) => {
    // Validate dữ liệu trước khi gửi
    const errors = validateUserData(userData);
    if (errors.length > 0) {
        throw new Error(errors.join(', '));
    }

    const response = await fetch(`${API_BASE_URL}/admin/users/${userId}/update`, {
        method: 'PUT',
        headers: authHeaders,
        body: JSON.stringify(userData)
    });

    const result = await response.json();

    if (!response.ok) {
        throw new Error(result.message || 'Không thể cập nhật user');
    }

    return result;
};

// Hàm validate phía client
const validateUserData = (data) => {
    const errors = [];

    if (data.firstName && data.firstName.length > 100) {
        errors.push('Họ không được quá 100 ký tự');
    }

    if (data.lastName && data.lastName.length > 100) {
        errors.push('Tên không được quá 100 ký tự');
    }

    if (data.phoneNumber && !isValidPhoneNumber(data.phoneNumber)) {
        errors.push('Số điện thoại không hợp lệ');
    }

    if (data.baseSalary !== undefined) {
        if (data.baseSalary < 0 || data.baseSalary > 100000000) {
            errors.push('Lương cơ bản phải từ 0 đến 100,000,000');
        }
    }

    if (data.nationalIdCard && data.nationalIdCard.length > 20) {
        errors.push('CCCD không được quá 20 ký tự');
    }

    return errors;
};

// Helper function
const isValidPhoneNumber = (phone) => {
    const phoneRegex = /^[0-9]{10,11}$/;
    return phoneRegex.test(phone.replace(/[\s-]/g, ''));
};

// ============ VÍ DỤ SỬ DỤNG ============

// 1. Cập nhật thông tin cơ bản
const updateBasicInfo = async (userId) => {
    try {
        const result = await updateUser(userId, {
            firstName: 'Văn B',
            lastName: 'Nguyễn',
            phoneNumber: '0987654321',
            dateOfBirth: '1995-03-20',
            gender: 'Male'
        });
        
        console.log('Cập nhật thành công:', result);
        alert('Cập nhật thông tin thành công!');
    } catch (error) {
        console.error('Lỗi:', error);
        alert(error.message);
    }
};

// 2. Thay đổi role user
const changeUserRole = async (userId, newRole) => {
    try {
        const result = await updateUser(userId, {
            role: newRole
        });
        
        console.log('Đổi role thành công:', result);
    } catch (error) {
        console.error('Lỗi:', error);
    }
};

// 3. Cập nhật thông tin nhân viên
const updateStaffInfo = async (userId) => {
    try {
        const result = await updateUser(userId, {
            nationalIdCard: '001234567890',
            hireDate: '2024-01-01',
            baseSalary: 15000000
        });
        
        console.log('Cập nhật nhân viên thành công:', result);
    } catch (error) {
        console.error('Lỗi:', error);
    }
};

// 4. Thay đổi tier của customer
const updateCustomerTier = async (userId, newTier) => {
    try {
        const result = await updateUser(userId, {
            tier: newTier  // 'Bronze', 'Silver', 'Gold', 'Platinum'
        });
        
        console.log('Cập nhật tier thành công:', result);
    } catch (error) {
        console.error('Lỗi:', error);
    }
};

// 5. Cập nhật nhiều trường cùng lúc
const updateMultipleFields = async (userId, formData) => {
    try {
        const result = await updateUser(userId, {
            firstName: formData.firstName,
            lastName: formData.lastName,
            phoneNumber: formData.phoneNumber,
            dateOfBirth: formData.dateOfBirth,
            gender: formData.gender,
            status: formData.status,
            tier: formData.tier
        });
        
        return result;
    } catch (error) {
        throw error;
    }
};
```

### Response thành công
```json
{
    "success": true,
    "message": "Cập nhật user thành công",
    "data": {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "email": "nguyenvana@example.com",
        "firstName": "Văn B",
        "lastName": "Nguyễn",
        "fullName": "Nguyễn Văn B",
        "phoneNumber": "0987654321",
        "role": "Customer",
        "status": "Active",
        "isActive": true,
        "tier": "Gold",
        "updatedAt": "2024-06-15T10:30:00Z"
    }
}
```

### Response lỗi
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

---

## 🎨 4. COMPONENT MẪU - FORM CHỈNH SỬA USER

### React Component Example
```jsx
import React, { useState, useEffect } from 'react';

const EditUserForm = ({ userId, onSuccess, onCancel }) => {
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [userData, setUserData] = useState(null);
    const [formData, setFormData] = useState({
        firstName: '',
        lastName: '',
        phoneNumber: '',
        dateOfBirth: '',
        gender: 'Male',
        role: 'Customer',
        status: 'Active',
        tier: 'Bronze',
        nationalIdCard: '',
        hireDate: '',
        baseSalary: 0
    });
    const [errors, setErrors] = useState({});

    // Load user data
    useEffect(() => {
        loadUserData();
    }, [userId]);

    const loadUserData = async () => {
        try {
            setLoading(true);
            const result = await getUserDetail(userId);
            
            if (result.success) {
                setUserData(result.data);
                setFormData({
                    firstName: result.data.firstName || '',
                    lastName: result.data.lastName || '',
                    phoneNumber: result.data.phoneNumber || '',
                    dateOfBirth: result.data.dateOfBirth?.split('T')[0] || '',
                    gender: result.data.gender || 'Male',
                    role: result.data.role || 'Customer',
                    status: result.data.status || 'Active',
                    tier: result.data.tier || 'Bronze',
                    nationalIdCard: result.data.nationalIdCard || '',
                    hireDate: result.data.hireDate?.split('T')[0] || '',
                    baseSalary: result.data.baseSalary || 0
                });
            }
        } catch (error) {
            alert('Không thể tải thông tin user: ' + error.message);
        } finally {
            setLoading(false);
        }
    };

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
        
        // Clear error khi user nhập
        if (errors[name]) {
            setErrors(prev => ({ ...prev, [name]: null }));
        }
    };

    const validateForm = () => {
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

        if (formData.phoneNumber && !isValidPhoneNumber(formData.phoneNumber)) {
            newErrors.phoneNumber = 'Số điện thoại không hợp lệ';
        }

        if ((formData.role === 'Staff' || formData.role === 'Admin') && !formData.nationalIdCard) {
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

        if (!validateForm()) {
            return;
        }

        try {
            setSaving(true);

            // Chỉ gửi các field đã thay đổi
            const changedData = {};
            Object.keys(formData).forEach(key => {
                if (formData[key] !== userData[key]) {
                    changedData[key] = formData[key];
                }
            });

            const result = await updateUser(userId, changedData);

            if (result.success) {
                alert('Cập nhật user thành công!');
                if (onSuccess) onSuccess(result.data);
            }
        } catch (error) {
            alert('Lỗi: ' + error.message);
        } finally {
            setSaving(false);
        }
    };

    if (loading) {
        return <div className="loading">Đang tải...</div>;
    }

    return (
        <form onSubmit={handleSubmit} className="edit-user-form">
            <h2>Chỉnh sửa thông tin User</h2>

            {/* Thông tin cơ bản */}
            <div className="form-section">
                <h3>Thông tin cơ bản</h3>
                
                <div className="form-group">
                    <label>Họ *</label>
                    <input
                        type="text"
                        name="firstName"
                        value={formData.firstName}
                        onChange={handleInputChange}
                        maxLength={100}
                        className={errors.firstName ? 'error' : ''}
                    />
                    {errors.firstName && <span className="error-message">{errors.firstName}</span>}
                </div>

                <div className="form-group">
                    <label>Tên *</label>
                    <input
                        type="text"
                        name="lastName"
                        value={formData.lastName}
                        onChange={handleInputChange}
                        maxLength={100}
                        className={errors.lastName ? 'error' : ''}
                    />
                    {errors.lastName && <span className="error-message">{errors.lastName}</span>}
                </div>

                <div className="form-group">
                    <label>Email (Không thể sửa)</label>
                    <input
                        type="email"
                        value={userData?.email || ''}
                        disabled
                        className="disabled"
                    />
                </div>

                <div className="form-group">
                    <label>Số điện thoại</label>
                    <input
                        type="tel"
                        name="phoneNumber"
                        value={formData.phoneNumber}
                        onChange={handleInputChange}
                        className={errors.phoneNumber ? 'error' : ''}
                    />
                    {errors.phoneNumber && <span className="error-message">{errors.phoneNumber}</span>}
                </div>

                <div className="form-group">
                    <label>Ngày sinh</label>
                    <input
                        type="date"
                        name="dateOfBirth"
                        value={formData.dateOfBirth}
                        onChange={handleInputChange}
                    />
                </div>

                <div className="form-group">
                    <label>Giới tính</label>
                    <select name="gender" value={formData.gender} onChange={handleInputChange}>
                        <option value="Male">Nam</option>
                        <option value="Female">Nữ</option>
                        <option value="Other">Khác</option>
                    </select>
                </div>
            </div>

            {/* Role & Status */}
            <div className="form-section">
                <h3>Role & Trạng thái</h3>
                
                <div className="form-group">
                    <label>Role</label>
                    <select name="role" value={formData.role} onChange={handleInputChange}>
                        <option value="Customer">Customer</option>
                        <option value="Staff">Staff</option>
                        <option value="Admin">Admin</option>
                    </select>
                </div>

                <div className="form-group">
                    <label>Trạng thái</label>
                    <select name="status" value={formData.status} onChange={handleInputChange}>
                        <option value="Active">Active</option>
                        <option value="Inactive">Inactive</option>
                        <option value="Locked">Locked</option>
                        <option value="Suspended">Suspended</option>
                    </select>
                </div>

                {formData.role === 'Customer' && (
                    <div className="form-group">
                        <label>Tier</label>
                        <select name="tier" value={formData.tier} onChange={handleInputChange}>
                            <option value="Bronze">Bronze</option>
                            <option value="Silver">Silver</option>
                            <option value="Gold">Gold</option>
                            <option value="Platinum">Platinum</option>
                        </select>
                    </div>
                )}
            </div>

            {/* Thông tin nhân viên */}
            {(formData.role === 'Staff' || formData.role === 'Admin') && (
                <div className="form-section">
                    <h3>Thông tin nhân viên</h3>
                    
                    <div className="form-group">
                        <label>CCCD *</label>
                        <input
                            type="text"
                            name="nationalIdCard"
                            value={formData.nationalIdCard}
                            onChange={handleInputChange}
                            maxLength={20}
                            className={errors.nationalIdCard ? 'error' : ''}
                        />
                        {errors.nationalIdCard && <span className="error-message">{errors.nationalIdCard}</span>}
                    </div>

                    <div className="form-group">
                        <label>Ngày vào làm</label>
                        <input
                            type="date"
                            name="hireDate"
                            value={formData.hireDate}
                            onChange={handleInputChange}
                        />
                    </div>

                    <div className="form-group">
                        <label>Lương cơ bản</label>
                        <input
                            type="number"
                            name="baseSalary"
                            value={formData.baseSalary}
                            onChange={handleInputChange}
                            min={0}
                            max={100000000}
                            step={100000}
                            className={errors.baseSalary ? 'error' : ''}
                        />
                        {errors.baseSalary && <span className="error-message">{errors.baseSalary}</span>}
                    </div>
                </div>
            )}

            {/* Buttons */}
            <div className="form-actions">
                <button type="button" onClick={onCancel} disabled={saving}>
                    Hủy
                </button>
                <button type="submit" disabled={saving} className="primary">
                    {saving ? 'Đang lưu...' : 'Lưu thay đổi'}
                </button>
            </div>
        </form>
    );
};

export default EditUserForm;
```

### CSS mẫu
```css
.edit-user-form {
    max-width: 800px;
    margin: 0 auto;
    padding: 20px;
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.form-section {
    margin-bottom: 30px;
    padding-bottom: 20px;
    border-bottom: 1px solid #eee;
}

.form-section:last-of-type {
    border-bottom: none;
}

.form-section h3 {
    margin-bottom: 15px;
    color: #333;
    font-size: 18px;
}

.form-group {
    margin-bottom: 15px;
}

.form-group label {
    display: block;
    margin-bottom: 5px;
    font-weight: 500;
    color: #555;
}

.form-group input,
.form-group select {
    width: 100%;
    padding: 10px;
    border: 1px solid #ddd;
    border-radius: 4px;
    font-size: 14px;
}

.form-group input.error,
.form-group select.error {
    border-color: #dc3545;
}

.form-group input.disabled {
    background-color: #f5f5f5;
    cursor: not-allowed;
}

.error-message {
    display: block;
    margin-top: 5px;
    color: #dc3545;
    font-size: 12px;
}

.form-actions {
    display: flex;
    gap: 10px;
    justify-content: flex-end;
    margin-top: 20px;
    padding-top: 20px;
    border-top: 1px solid #eee;
}

.form-actions button {
    padding: 10px 20px;
    border: 1px solid #ddd;
    border-radius: 4px;
    cursor: pointer;
    font-size: 14px;
    transition: all 0.3s;
}

.form-actions button:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}

.form-actions button.primary {
    background-color: #007bff;
    color: white;
    border-color: #007bff;
}

.form-actions button.primary:hover:not(:disabled) {
    background-color: #0056b3;
}

.loading {
    text-align: center;
    padding: 40px;
    color: #666;
}
```

---

## 🔄 5. CÁC CHỨC NĂNG BỔ SUNG

### 5.1. Khóa/Mở khóa tài khoản
```javascript
const toggleUserStatus = async (userId) => {
    const response = await fetch(`${API_BASE_URL}/admin/users/${userId}/toggle`, {
        method: 'PATCH',
        headers: authHeaders
    });

    const result = await response.json();
    
    if (result.success) {
        alert('Thay đổi trạng thái thành công');
        return result.data;
    }
    
    throw new Error(result.message);
};
```

### 5.2. Kiểm tra email đã tồn tại
```javascript
const checkEmailAvailable = async (email) => {
    const params = new URLSearchParams({ email });
    
    const response = await fetch(`${API_BASE_URL}/admin/users/check-email?${params}`, {
        method: 'GET',
        headers: authHeaders
    });

    const result = await response.json();
    return result.data.available; // true/false
};

// Sử dụng trong form validation
const validateEmailUnique = async (email, currentUserId) => {
    const available = await checkEmailAvailable(email);
    if (!available) {
        return 'Email này đã được sử dụng';
    }
    return null;
};
```

### 5.3. Kiểm tra CCCD đã tồn tại
```javascript
const checkNationalIdAvailable = async (nationalId) => {
    const params = new URLSearchParams({ nationalIdCard: nationalId });
    
    const response = await fetch(`${API_BASE_URL}/admin/users/check-cccd?${params}`, {
        method: 'GET',
        headers: authHeaders
    });

    const result = await response.json();
    return result.data.available;
};
```

---

## ⚠️ 6. LƯU Ý QUAN TRỌNG

### 6.1. Quyền hạn
- ✅ **Admin**: Có thể chỉnh sửa tất cả users
- ✅ **Staff**: Có thể chỉnh sửa Customer, không thể sửa Admin/Staff khác
- ❌ **Customer**: Không có quyền truy cập

### 6.2. Các trường không thể sửa
- ❌ `email` - Email là unique identifier, không được phép thay đổi
- ❌ `id` - System generated
- ❌ `createdAt` - Timestamp tự động
- ❌ `loyaltyPoints` - Chỉ thay đổi qua hệ thống tích điểm
- ❌ `totalSpent` - Chỉ cập nhật khi có đơn hàng
- ❌ `orderCount` - Tự động tính

### 6.3. Validation quan trọng
```javascript
// Các validation cần check ở frontend
const frontendValidations = {
    firstName: {
        required: true,
        maxLength: 100,
        pattern: /^[\p{L}\s]+$/u  // Chỉ chữ cái và khoảng trắng
    },
    lastName: {
        required: true,
        maxLength: 100,
        pattern: /^[\p{L}\s]+$/u
    },
    phoneNumber: {
        pattern: /^[0-9]{10,11}$/,  // 10-11 số
        optional: true
    },
    nationalIdCard: {
        pattern: /^[0-9]{9,12}$/,  // 9-12 số
        requiredIf: (data) => ['Staff', 'Admin'].includes(data.role)
    },
    baseSalary: {
        min: 0,
        max: 100000000,
        requiredIf: (data) => ['Staff', 'Admin'].includes(data.role)
    },
    dateOfBirth: {
        minAge: 16,  // Ít nhất 16 tuổi
        maxAge: 100
    }
};
```

### 6.4. Error handling
```javascript
const handleApiError = (error, response) => {
    if (response?.status === 400) {
        // Validation errors
        const errors = response.data?.errors || {};
        return formatValidationErrors(errors);
    }
    
    if (response?.status === 404) {
        return 'Không tìm thấy user';
    }
    
    if (response?.status === 403) {
        return 'Bạn không có quyền thực hiện thao tác này';
    }
    
    return error.message || 'Đã xảy ra lỗi không xác định';
};
```

---

## 📱 7. RESPONSIVE DESIGN

### Mobile-first approach
```css
/* Mobile (< 768px) */
.edit-user-form {
    padding: 15px;
}

.form-group {
    margin-bottom: 12px;
}

.form-actions {
    flex-direction: column;
}

.form-actions button {
    width: 100%;
}

/* Tablet (>= 768px) */
@media (min-width: 768px) {
    .edit-user-form {
        padding: 20px;
    }
    
    .form-row {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 15px;
    }
}

/* Desktop (>= 1024px) */
@media (min-width: 1024px) {
    .edit-user-form {
        max-width: 900px;
    }
    
    .form-row {
        grid-template-columns: 1fr 1fr 1fr;
    }
}
```

---

## 🎯 8. CHECKLIST IMPLEMENTATION

### Frontend Developer Checklist
- [ ] Setup API client với authentication
- [ ] Tạo service layer cho user management
- [ ] Implement danh sách users với filter/search
- [ ] Tạo form xem chi tiết user
- [ ] **Implement form chỉnh sửa user** ⭐
- [ ] Thêm client-side validation
- [ ] Implement error handling
- [ ] Thêm loading states
- [ ] Test tất cả edge cases
- [ ] Responsive design cho mobile/tablet
- [ ] Accessibility (ARIA labels, keyboard nav)
- [ ] Unit tests cho validation logic
- [ ] Integration tests

---

## 📞 9. HỖ TRỢ & RESOURCES

### API Endpoints Summary
| Endpoint | Method | Mục đích |
|----------|--------|----------|
| `/api/admin/users/list` | GET | Danh sách users |
| `/api/admin/users/{id}/detail` | GET | Chi tiết user |
| `/api/admin/users/{id}/update` | PUT | **Chỉnh sửa user** |
| `/api/admin/users/create` | POST | Tạo user mới |
| `/api/admin/users/{id}/delete` | DELETE | Xóa user |
| `/api/admin/users/{id}/toggle` | PATCH | Khóa/mở user |
| `/api/admin/users/check-email` | GET | Kiểm tra email |
| `/api/admin/users/check-cccd` | GET | Kiểm tra CCCD |

### Related Documentation
- [Admin Dashboard API](./Admin-Dashboard-Endpoints-Summary.md)
- [Frontend Integration Guide](./Admin-Frontend-Integration-Guide.md)
- [Authentication System](./Authentication-System.md)

### Test Files
- `SakuraHomeAPI\Tests\Admin-User-Management.http`
- `SakuraHomeAPI\Tests\Admin.http`

---

## ✅ KẾT LUẬN

Tài liệu này cung cấp **đầy đủ thông tin** để Frontend Developer có thể:
1. ✅ Hiểu rõ cấu trúc API user management
2. ✅ **Implement form chỉnh sửa user hoàn chỉnh**
3. ✅ Validate dữ liệu đúng cách
4. ✅ Handle errors và edge cases
5. ✅ Tạo UI/UX tốt cho admin

**Tất cả endpoints đã được test và sẵn sàng sử dụng! 🚀**
