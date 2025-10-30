# 🏪 SakuraHome API - Tài liệu hoàn chỉnh cho Admin Panel

## 📋 Tổng quan

Tài liệu này cung cấp **toàn bộ thông tin cần thiết** để xây dựng một trang admin hoàn chỉnh cho hệ thống SakuraHome API, bao gồm tất cả các endpoints, models, và examples.

## 🔧 Cấu hình cơ bản

### Environment Setup
```javascript
// Configuration
const API_CONFIG = {
    baseURL: 'https://localhost:8080/api',
    timeout: 30000,
    headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
    }
};

// Auth headers với JWT token
const getAuthHeaders = () => ({
    ...API_CONFIG.headers,
    'Authorization': `Bearer ${localStorage.getItem('adminToken')}`
});
```

---

## 🔐 1. AUTHENTICATION & AUTHORIZATION

### 🚀 Admin Login & Token Management

```javascript
// 1. Admin Login
const adminLogin = async (credentials) => {
    const response = await fetch(`${API_CONFIG.baseURL}/auth/login`, {
        method: 'POST',
        headers: API_CONFIG.headers,
        body: JSON.stringify({
            email: credentials.email,
            password: credentials.password,
            rememberMe: true
        })
    });
    
    const result = await response.json();
    if (result.success) {
        localStorage.setItem('adminToken', result.data.token);
        localStorage.setItem('refreshToken', result.data.refreshToken);
        localStorage.setItem('adminInfo', JSON.stringify(result.data.user));
        return result.data;
    }
    throw new Error(result.message);
};

// 2. Check Admin Permissions
const checkAdminRole = () => {
    const adminInfo = JSON.parse(localStorage.getItem('adminInfo') || '{}');
    return ['Admin', 'Staff'].includes(adminInfo.role);
};

// 3. Refresh Token
const refreshToken = async () => {
    const refresh = localStorage.getItem('refreshToken');
    const response = await fetch(`${API_CONFIG.baseURL}/auth/refresh-token`, {
        method: 'POST',
        headers: API_CONFIG.headers,
        body: JSON.stringify({ refreshToken: refresh })
    });
    
    if (response.ok) {
        const result = await response.json();
        localStorage.setItem('adminToken', result.data.token);
        return result.data.token;
    }
    window.location.href = '/admin/login';
};
```

### Auth Endpoints
```http
POST /api/auth/login                 # Admin login
POST /api/auth/refresh-token         # Refresh access token
POST /api/auth/logout               # Logout
GET /api/auth/me                    # Get current admin info
POST /api/auth/change-password      # Change password
```

---

## 👥 2. USER MANAGEMENT

### 📊 User List & Filter

```javascript
// 1. Get Users with Filtering
const getUsers = async (filters = {}) => {
    const params = new URLSearchParams({
        page: filters.page || 1,
        pageSize: filters.pageSize || 20,
        keyword: filters.keyword || '',
        role: filters.role || '',
        isActive: filters.isActive !== undefined ? filters.isActive : ''
    });
    
    const response = await fetch(`${API_CONFIG.baseURL}/admin/users?${params}`, {
        headers: getAuthHeaders()
    });
    return await response.json();
};

// Response Model
const userListResponse = {
    success: true,
    data: {
        users: [
            {
                id: "uuid",
                userName: "string",
                email: "string",
                firstName: "string",
                lastName: "string",
                fullName: "string",
                avatar: "string",
                role: "Customer|Staff|Admin",
                status: "Active|Inactive|Locked",
                tier: "Bronze|Silver|Gold|Platinum",
                isActive: true,
                emailConfirmed: true,
                phoneNumberConfirmed: false,
                createdAt: "2024-01-01T00:00:00Z",
                lastLoginAt: "2024-01-01T00:00:00Z"
            }
        ],
        totalCount: 100,
        page: 1,
        pageSize: 20
    }
};
```

### 👤 User Detail & Management

```javascript
// 2. Get User Details
const getUserDetails = async (userId) => {
    const response = await fetch(`${API_CONFIG.baseURL}/admin/users/${userId}`, {
        headers: getAuthHeaders()
    });
    return await response.json();
};

// 3. Create New User
const createUser = async (userData) => {
    const response = await fetch(`${API_CONFIG.baseURL}/admin/users`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({
            email: userData.email,
            userName: userData.userName,
            password: userData.password,
            phoneNumber: userData.phoneNumber,
            role: userData.role || 'Customer',
            isActive: userData.isActive !== false
        })
    });
    return await response.json();
};

// 4. Update User
const updateUser = async (userId, userData) => {
    const response = await fetch(`${API_CONFIG.baseURL}/admin/users/${userId}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify(userData)
    });
    return await response.json();
};

// 5. Change User Status
const changeUserStatus = async (userId, isActive) => {
    const response = await fetch(`${API_CONFIG.baseURL}/admin/users/${userId}/status`, {
        method: 'PATCH',
        headers: getAuthHeaders(),
        body: JSON.stringify({ isActive })
    });
    return await response.json();
};
```

### 📈 User Statistics

```javascript
// 6. Get User Statistics
const getUserStats = async () => {
    const response = await fetch(`${API_CONFIG.baseURL}/admin/users/stats`, {
        headers: getAuthHeaders()
    });
    return await response.json();
};

// Statistics Response Model
const userStatsResponse = {
    success: true,
    data: {
        totalUsers: 1500,
        activeUsers: 1200,
        inactiveUsers: 300,
        admins: 5,
        staffs: 20,
        customers: 1475
    }
};
```

### Admin User Management Endpoints
```http
GET /api/admin/users                    # Get users list with filters
GET /api/admin/users/{id}               # Get user details
POST /api/admin/users                   # Create new user
PUT /api/admin/users/{id}               # Update user
DELETE /api/admin/users/{id}            # Delete user (soft delete)
PATCH /api/admin/users/{id}/status      # Change user status
GET /api/admin/users/stats              # Get user statistics
POST /api/admin/users/{id}/reset-password  # Reset user password
POST /api/admin/users/{id}/unlock       # Unlock user account
POST /api/admin/users/{id}/verify-email # Verify user email
POST /api/admin/users/{id}/verify-phone # Verify user phone
```

---

## 🛍️ 3. PRODUCT MANAGEMENT

### 📦 Product List & Search

```javascript
// 1. Get Products with Advanced Filtering
const getProducts = async (filters = {}) => {
    const params = new URLSearchParams({
        page: filters.page || 1,
        pageSize: filters.pageSize || 20,
        search: filters.search || '',
        categoryId: filters.categoryId || '',
        brandId: filters.brandId || '',
        minPrice: filters.minPrice || '',
        maxPrice: filters.maxPrice || '',
        isActive: filters.isActive !== undefined ? filters.isActive : '',
        inStock: filters.inStock !== undefined ? filters.inStock : '',
        sortBy: filters.sortBy || 'CreatedAt',
        sortDirection: filters.sortDirection || 'Desc'
    });
    
    const response = await fetch(`${API_CONFIG.baseURL}/product?${params}`, {
        headers: getAuthHeaders()
    });
    return await response.json();
};

// Product List Response Model
const productListResponse = {
    success: true,
    data: {
        products: [
            {
                id: "uuid",
                name: "Product Name",
                sku: "SKU123",
                slug: "product-slug",
                description: "Product description",
                shortDescription: "Short description",
                price: 100000,
                originalPrice: 120000,
                stock: 50,
                isActive: true,
                isDeleted: false,
                mainImage: "image-url",
                images: ["image1", "image2"],
                categoryId: "uuid",
                categoryName: "Category Name",
                brandId: "uuid", 
                brandName: "Brand Name",
                averageRating: 4.5,
                reviewCount: 25,
                soldCount: 100,
                viewCount: 500,
                createdAt: "2024-01-01T00:00:00Z",
                updatedAt: "2024-01-01T00:00:00Z"
            }
        ],
        totalCount: 200,
        page: 1,
        pageSize: 20
    }
};
```

### 🛠️ Product CRUD Operations

```javascript
// 2. Get Product Details
const getProductDetails = async (productId) => {
    const response = await fetch(`${API_CONFIG.baseURL}/product/${productId}`, {
        headers: getAuthHeaders()
    });
    return await response.json();
};

// 3. Create New Product
const createProduct = async (productData) => {
    const response = await fetch(`${API_CONFIG.baseURL}/product`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({
            name: productData.name,
            sku: productData.sku,
            description: productData.description,
            shortDescription: productData.shortDescription,
            price: productData.price,
            originalPrice: productData.originalPrice,
            stock: productData.stock,
            categoryId: productData.categoryId,
            brandId: productData.brandId,
            mainImage: productData.mainImage,
            images: productData.images || [],
            metaTitle: productData.metaTitle,
            metaDescription: productData.metaDescription,
            metaKeywords: productData.metaKeywords,
            tags: productData.tags || [],
            attributes: productData.attributes || {},
            variants: productData.variants || [],
            isActive: productData.isActive !== false,
            allowBackorder: productData.allowBackorder || false,
            trackQuantity: productData.trackQuantity !== false,
            weight: productData.weight || 0,
            dimensions: productData.dimensions
        })
    });
    return await response.json();
};

// 4. Update Product
const updateProduct = async (productId, productData) => {
    const response = await fetch(`${API_CONFIG.baseURL}/product/${productId}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify(productData)
    });
    return await response.json();
};

// 5. Update Stock
const updateStock = async (productId, stockData) => {
    const response = await fetch(`${API_CONFIG.baseURL}/product/${productId}/stock`, {
        method: 'PATCH',
        headers: getAuthHeaders(),
        body: JSON.stringify({
            quantity: stockData.quantity,
            reason: stockData.reason || 'Manual Adjustment'
        })
    });
    return await response.json();
};

// 6. Delete Product
const deleteProduct = async (productId) => {
    const response = await fetch(`${API_CONFIG.baseURL}/product/${productId}`, {
        method: 'DELETE',
        headers: getAuthHeaders()
    });
    return await response.json();
};
```

### Product Management Endpoints
```http
GET /api/product                        # Get products with filters & search
GET /api/product/{id}                   # Get product details
GET /api/product/sku/{sku}              # Get product by SKU
POST /api/product                       # Create product [Staff]
PUT /api/product/{id}                   # Update product [Staff]
DELETE /api/product/{id}                # Delete product [Staff]
PATCH /api/product/{id}/stock           # Update stock [Staff]
```

---

## 🏷️ 4. BRAND & CATEGORY MANAGEMENT

### 🎨 Brand Management

```javascript
// 1. Get Brands
const getBrands = async () => {
    const response = await fetch(`${API_CONFIG.baseURL}/brand`, {
        headers: getAuthHeaders()
    });
    return await response.json();
};

// 2. Create Brand
const createBrand = async (brandData) => {
    const response = await fetch(`${API_CONFIG.baseURL}/brand`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({
            name: brandData.name,
            description: brandData.description,
            logo: brandData.logo,
            website: brandData.website,
            isActive: brandData.isActive !== false,
            isFeatured: brandData.isFeatured || false,
            metaTitle: brandData.metaTitle,
            metaDescription: brandData.metaDescription
        })
    });
    return await response.json();
};

// 3. Delete Brand
const deleteBrand = async (brandId) => {
    const response = await fetch(`${API_CONFIG.baseURL}/brand/${brandId}`, {
        method: 'DELETE',
        headers: getAuthHeaders()
    });
    return await response.json();
};
```

### 📂 Category Management

```javascript
// 1. Get Categories (Tree Structure)
const getCategories = async () => {
    const response = await fetch(`${API_CONFIG.baseURL}/category`, {
        headers: getAuthHeaders()
    });
    return await response.json();
};

// 2. Create Category
const createCategory = async (categoryData) => {
    const response = await fetch(`${API_CONFIG.baseURL}/category`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({
            name: categoryData.name,
            description: categoryData.description,
            parentId: categoryData.parentId || null,
            slug: categoryData.slug,
            image: categoryData.image,
            icon: categoryData.icon,
            isActive: categoryData.isActive !== false,
            isFeatured: categoryData.isFeatured || false,
            sortOrder: categoryData.sortOrder || 0,
            metaTitle: categoryData.metaTitle,
            metaDescription: categoryData.metaDescription
        })
    });
    return await response.json();
};
```

### Brand & Category Endpoints
```http
GET /api/brand                          # Get all brands
GET /api/brand/{id}                     # Get brand details
GET /api/brand/featured                 # Get featured brands
POST /api/brand                         # Create brand [Staff]
DELETE /api/brand/{id}                  # Delete brand [Staff]

GET /api/category                       # Get category tree
GET /api/category/{id}                  # Get category details
GET /api/category/root                  # Get root categories
POST /api/category                      # Create category [Staff]
DELETE /api/category/{id}               # Delete category [Staff]
```

---

## 📦 5. ORDER MANAGEMENT

### 📋 Order List & Filtering

```javascript
// 1. Get Orders with Filters (Admin/Staff view)
const getOrders = async (filters = {}) => {
    const params = new URLSearchParams({
        page: filters.page || 1,
        pageSize: filters.pageSize || 20,
        status: filters.status || '',
        userId: filters.userId || '',
        paymentStatus: filters.paymentStatus || '',
        startDate: filters.startDate || '',
        endDate: filters.endDate || '',
        minAmount: filters.minAmount || '',
        maxAmount: filters.maxAmount || '',
        sortBy: filters.sortBy || 'CreatedAt',
        sortDirection: filters.sortDirection || 'Desc'
    });
    
    const response = await fetch(`${API_CONFIG.baseURL}/order?${params}`, {
        headers: getAuthHeaders()
    });
    return await response.json();
};

// Order List Response Model
const orderListResponse = {
    success: true,
    data: {
        orders: [
            {
                id: "uuid",
                orderNumber: "ORD-2024-001",
                userId: "uuid",
                userEmail: "user@email.com",
                userFullName: "User Name",
                status: "Pending",
                statusDisplay: "Chờ xác nhận",
                paymentStatus: "Pending",
                paymentStatusDisplay: "Chờ thanh toán",
                paymentMethod: "BankTransfer",
                paymentMethodDisplay: "Chuyển khoản",
                subtotal: 500000,
                shippingFee: 30000,
                tax: 0,
                totalAmount: 530000,
                itemCount: 3,
                shippingAddress: {
                    fullName: "Recipient Name",
                    phoneNumber: "0123456789",
                    addressLine: "123 Street",
                    wardName: "Ward",
                    districtName: "District",
                    provinceName: "Province"
                },
                createdAt: "2024-01-01T00:00:00Z",
                updatedAt: "2024-01-01T00:00:00Z"
            }
        ],
        totalCount: 500,
        page: 1,
        pageSize: 20
    }
};
```

### 🔧 Order Management Actions

```javascript
// 2. Get Order Details
const getOrderDetails = async (orderId) => {
    const response = await fetch(`${API_CONFIG.baseURL}/order/${orderId}`, {
        headers: getAuthHeaders()
    });
    return await response.json();
};

// 3. Update Order Status (Staff only)
const updateOrderStatus = async (orderId, status, note = '') => {
    const response = await fetch(`${API_CONFIG.baseURL}/order/${orderId}/status`, {
        method: 'PATCH',
        headers: getAuthHeaders(),
        body: JSON.stringify({
            status: status,
            note: note
        })
    });
    return await response.json();
};

// 4. Add Staff Note
const addStaffNote = async (orderId, note) => {
    const response = await fetch(`${API_CONFIG.baseURL}/order/${orderId}/staff-notes`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({
            note: note
        })
    });
    return await response.json();
};

// 5. Process Return Request
const processReturn = async (orderId, returnData) => {
    const response = await fetch(`${API_CONFIG.baseURL}/order/${orderId}/return`, {
        method: 'PATCH',
        headers: getAuthHeaders(),
        body: JSON.stringify({
            approved: returnData.approved,
            reason: returnData.reason,
            refundAmount: returnData.refundAmount,
            note: returnData.note
        })
    });
    return await response.json();
};
```

### 📊 Order Analytics

```javascript
// 6. Get Order Statistics
const getOrderStats = async (period = 'month') => {
    const response = await fetch(`${API_CONFIG.baseURL}/order/stats?period=${period}`, {
        headers: getAuthHeaders()
    });
    return await response.json();
};

// Order Statistics Response Model
const orderStatsResponse = {
    success: true,
    data: {
        totalOrders: 1000,
        pendingOrders: 50,
        processingOrders: 30,
        shippedOrders: 20,
        deliveredOrders: 800,
        cancelledOrders: 100,
        totalRevenue: 50000000,
        averageOrderValue: 500000,
        topProducts: [
            {
                productId: "uuid",
                productName: "Product Name",
                quantitySold: 100,
                revenue: 5000000
            }
        ]
    }
};
```

### Order Management Endpoints
```http
GET /api/order                          # Get orders with filters
GET /api/order/{id}                     # Get order details
GET /api/order/stats                    # Get order statistics [Staff]
GET /api/order/recent                   # Get recent orders [Staff]
PATCH /api/order/{id}/status            # Update order status [Staff]
PATCH /api/order/{id}/confirm           # Confirm order [Staff]
PATCH /api/order/{id}/process           # Process order [Staff]
PATCH /api/order/{id}/ship              # Ship order [Staff]
PATCH /api/order/{id}/deliver           # Mark as delivered [Staff]
POST /api/order/{id}/staff-notes        # Add staff note [Staff]
PATCH /api/order/{id}/return            # Process return [Staff]
```

---

## 🎟️ 6. COUPON MANAGEMENT

### 🏷️ Coupon List & Management

```javascript
// 1. Get Coupons (Admin/Staff only)
const getCoupons = async (filters = {}) => {
    const params = new URLSearchParams({
        page: filters.page || 1,
        pageSize: filters.pageSize || 20,
        search: filters.search || '',
        type: filters.type || '',
        isActive: filters.isActive !== undefined ? filters.isActive : ''
    });
    
    const response = await fetch(`${API_CONFIG.baseURL}/coupon?${params}`, {
        headers: getAuthHeaders()
    });
    return await response.json();
};

// 2. Create Coupon
const createCoupon = async (couponData) => {
    const response = await fetch(`${API_CONFIG.baseURL}/coupon`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({
            code: couponData.code,
            name: couponData.name,
            description: couponData.description,
            type: couponData.type, // 'Percentage' | 'FixedAmount' | 'FreeShipping'
            value: couponData.value,
            minOrderAmount: couponData.minOrderAmount || 0,
            maxDiscountAmount: couponData.maxDiscountAmount,
            usageLimit: couponData.usageLimit,
            usagePerUser: couponData.usagePerUser || 1,
            startDate: couponData.startDate,
            endDate: couponData.endDate,
            isActive: couponData.isActive !== false
        })
    });
    return await response.json();
};

// 3. Update Coupon
const updateCoupon = async (couponId, couponData) => {
    const response = await fetch(`${API_CONFIG.baseURL}/coupon/${couponId}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify(couponData)
    });
    return await response.json();
};

// 4. Toggle Coupon Status
const toggleCouponStatus = async (couponId, isActive) => {
    const response = await fetch(`${API_CONFIG.baseURL}/coupon/${couponId}/toggle-status`, {
        method: 'PATCH',
        headers: getAuthHeaders(),
        body: JSON.stringify(isActive)
    });
    return await response.json();
};

// 5. Get Coupon Statistics
const getCouponStats = async () => {
    const response = await fetch(`${API_CONFIG.baseURL}/coupon/stats`, {
        headers: getAuthHeaders()
    });
    return await response.json();
};
```

### Coupon Management Endpoints
```http
GET /api/coupon                         # Get coupons [Staff]
GET /api/coupon/{id}                    # Get coupon details [Staff]
POST /api/coupon                        # Create coupon [Staff]
PUT /api/coupon/{id}                    # Update coupon [Staff]
DELETE /api/coupon/{id}                 # Delete coupon [Staff]
PATCH /api/coupon/{id}/toggle-status    # Toggle coupon status [Staff]
GET /api/coupon/stats                   # Get coupon statistics [Staff]
POST /api/coupon/validate               # Validate coupon (Public)
GET /api/coupon/code/{code}             # Get coupon by code (Public)
```

---

## 📊 7. ANALYTICS & DASHBOARD

### 📈 Dashboard Statistics

```javascript
// 1. Get Dashboard Overview
const getDashboardStats = async () => {
    const [userStats, orderStats, productStats, revenueStats] = await Promise.all([
        getUserStats(),
        getOrderStats(),
        getProductStats(),
        getRevenueStats()
    ]);
    
    return {
        users: userStats.data,
        orders: orderStats.data,
        products: productStats.data,
        revenue: revenueStats.data
    };
};

// 2. Get Revenue Analytics
const getRevenueStats = async (period = 'month') => {
    const response = await fetch(`${API_CONFIG.baseURL}/analytics/revenue?period=${period}`, {
        headers: getAuthHeaders()
    });
    return await response.json();
};

// 3. Get Product Analytics
const getProductStats = async () => {
    const response = await fetch(`${API_CONFIG.baseURL}/analytics/products`, {
        headers: getAuthHeaders()
    });
    return await response.json();
};
```

### Dashboard Data Models

```javascript
// Dashboard Overview Response
const dashboardData = {
    users: {
        totalUsers: 1500,
        activeUsers: 1200,
        newUsersToday: 25,
        newUsersThisMonth: 300
    },
    orders: {
        totalOrders: 5000,
        pendingOrders: 45,
        todayOrders: 15,
        monthlyOrders: 400,
        totalRevenue: 250000000,
        monthlyRevenue: 25000000
    },
    products: {
        totalProducts: 200,
        activeProducts: 180,
        outOfStockProducts: 15,
        lowStockProducts: 8
    },
    analytics: {
        topSellingProducts: [
            {
                id: "uuid",
                name: "Product Name",
                soldCount: 150,
                revenue: 7500000
            }
        ],
        recentOrders: [
            {
                id: "uuid",
                orderNumber: "ORD-2024-001",
                customerName: "Customer Name",
                total: 500000,
                status: "Pending",
                createdAt: "2024-01-01T00:00:00Z"
            }
        ]
    }
};
```

---

## 🔧 8. SYSTEM UTILITIES

### 🛠️ System Management

```javascript
// 1. Get System Health
const getSystemHealth = async () => {
    const response = await fetch(`${API_CONFIG.baseURL}/system/health`, {
        headers: getAuthHeaders()
    });
    return await response.json();
};

// 2. Clear Cache
const clearCache = async () => {
    const response = await fetch(`${API_CONFIG.baseURL}/system/clear-cache`, {
        method: 'POST',
        headers: getAuthHeaders()
    });
    return await response.json();
};

// 3. Export Data
const exportData = async (type, filters = {}) => {
    const params = new URLSearchParams(filters);
    const response = await fetch(`${API_CONFIG.baseURL}/export/${type}?${params}`, {
        headers: getAuthHeaders()
    });
    
    if (response.ok) {
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${type}-export-${new Date().toISOString().split('T')[0]}.xlsx`;
        a.click();
        window.URL.revokeObjectURL(url);
    }
    return response.ok;
};
```

---

## 💡 9. FRONTEND INTEGRATION EXAMPLES

### 🎨 React Component Examples

```jsx
// User Management Component
import React, { useState, useEffect } from 'react';

const UserManagement = () => {
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(false);
    const [filters, setFilters] = useState({
        page: 1,
        pageSize: 20,
        keyword: '',
        role: '',
        isActive: ''
    });

    useEffect(() => {
        loadUsers();
    }, [filters]);

    const loadUsers = async () => {
        setLoading(true);
        try {
            const result = await getUsers(filters);
            if (result.success) {
                setUsers(result.data.users);
            }
        } catch (error) {
            console.error('Failed to load users:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleStatusChange = async (userId, isActive) => {
        try {
            const result = await changeUserStatus(userId, isActive);
            if (result.success) {
                loadUsers(); // Reload data
            }
        } catch (error) {
            console.error('Failed to change user status:', error);
        }
    };

    return (
        <div className="user-management">
            <div className="filters">
                <input
                    type="text"
                    placeholder="Search users..."
                    value={filters.keyword}
                    onChange={(e) => setFilters({...filters, keyword: e.target.value})}
                />
                <select
                    value={filters.role}
                    onChange={(e) => setFilters({...filters, role: e.target.value})}
                >
                    <option value="">All Roles</option>
                    <option value="Customer">Customer</option>
                    <option value="Staff">Staff</option>
                    <option value="Admin">Admin</option>
                </select>
            </div>

            {loading ? (
                <div>Loading...</div>
            ) : (
                <table className="users-table">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Email</th>
                            <th>Role</th>
                            <th>Status</th>
                            <th>Created</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.map(user => (
                            <tr key={user.id}>
                                <td>{user.fullName}</td>
                                <td>{user.email}</td>
                                <td>{user.role}</td>
                                <td>
                                    <span className={`status ${user.isActive ? 'active' : 'inactive'}`}>
                                        {user.isActive ? 'Active' : 'Inactive'}
                                    </span>
                                </td>
                                <td>{new Date(user.createdAt).toLocaleDateString()}</td>
                                <td>
                                    <button
                                        onClick={() => handleStatusChange(user.id, !user.isActive)}
                                        className={`btn ${user.isActive ? 'btn-danger' : 'btn-success'}`}
                                    >
                                        {user.isActive ? 'Deactivate' : 'Activate'}
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
};

export default UserManagement;
```

### 📊 Dashboard Component

```jsx
// Dashboard Component
import React, { useState, useEffect } from 'react';

const Dashboard = () => {
    const [stats, setStats] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        loadDashboardData();
    }, []);

    const loadDashboardData = async () => {
        try {
            const data = await getDashboardStats();
            setStats(data);
        } catch (error) {
            console.error('Failed to load dashboard data:', error);
        } finally {
            setLoading(false);
        }
    };

    if (loading) return <div>Loading dashboard...</div>;
    if (!stats) return <div>Failed to load dashboard data</div>;

    return (
        <div className="dashboard">
            <div className="stats-grid">
                <div className="stat-card">
                    <h3>Total Users</h3>
                    <div className="stat-value">{stats.users.totalUsers.toLocaleString()}</div>
                    <div className="stat-change">+{stats.users.newUsersToday} today</div>
                </div>

                <div className="stat-card">
                    <h3>Total Orders</h3>
                    <div className="stat-value">{stats.orders.totalOrders.toLocaleString()}</div>
                    <div className="stat-change">{stats.orders.pendingOrders} pending</div>
                </div>

                <div className="stat-card">
                    <h3>Revenue</h3>
                    <div className="stat-value">
                        {(stats.orders.totalRevenue / 1000000).toFixed(1)}M VND
                    </div>
                    <div className="stat-change">
                        +{(stats.orders.monthlyRevenue / 1000000).toFixed(1)}M this month
                    </div>
                </div>

                <div className="stat-card">
                    <h3>Products</h3>
                    <div className="stat-value">{stats.products.totalProducts}</div>
                    <div className="stat-change warning">
                        {stats.products.outOfStockProducts} out of stock
                    </div>
                </div>
            </div>

            <div className="dashboard-content">
                <div className="recent-orders">
                    <h3>Recent Orders</h3>
                    <table>
                        <thead>
                            <tr>
                                <th>Order</th>
                                <th>Customer</th>
                                <th>Amount</th>
                                <th>Status</th>
                                <th>Date</th>
                            </tr>
                        </thead>
                        <tbody>
                            {stats.analytics.recentOrders.map(order => (
                                <tr key={order.id}>
                                    <td>{order.orderNumber}</td>
                                    <td>{order.customerName}</td>
                                    <td>{order.total.toLocaleString()} VND</td>
                                    <td>
                                        <span className={`status ${order.status.toLowerCase()}`}>
                                            {order.status}
                                        </span>
                                    </td>
                                    <td>{new Date(order.createdAt).toLocaleDateString()}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>

                <div className="top-products">
                    <h3>Top Selling Products</h3>
                    <div className="product-list">
                        {stats.analytics.topSellingProducts.map(product => (
                            <div key={product.id} className="product-item">
                                <div className="product-name">{product.name}</div>
                                <div className="product-stats">
                                    <span>Sold: {product.soldCount}</span>
                                    <span>Revenue: {(product.revenue / 1000000).toFixed(1)}M VND</span>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Dashboard;
```

---

## 🔒 10. ERROR HANDLING & BEST PRACTICES

### ⚡ Error Handling

```javascript
// Global Error Handler
const handleApiError = (error, operation = 'API call') => {
    console.error(`${operation} failed:`, error);
    
    if (error.status === 401) {
        // Token expired, try refresh
        return refreshToken().then(() => {
            // Retry the original request
            return true;
        }).catch(() => {
            // Redirect to login
            window.location.href = '/admin/login';
            return false;
        });
    }
    
    if (error.status === 403) {
        // Insufficient permissions
        alert('You do not have permission to perform this action');
        return false;
    }
    
    if (error.status >= 500) {
        // Server error
        alert('Server error. Please try again later');
        return false;
    }
    
    return false;
};

// API Call Wrapper with Error Handling
const apiCall = async (url, options = {}) => {
    try {
        const response = await fetch(url, {
            ...options,
            headers: {
                ...getAuthHeaders(),
                ...options.headers
            }
        });
        
        if (!response.ok) {
            throw { status: response.status, message: response.statusText };
        }
        
        return await response.json();
    } catch (error) {
        const canRetry = await handleApiError(error, `${options.method || 'GET'} ${url}`);
        if (canRetry) {
            // Retry once with new token
            return await fetch(url, {
                ...options,
                headers: {
                    ...getAuthHeaders(),
                    ...options.headers
                }
            }).then(res => res.json());
        }
        throw error;
    }
};
```

### 📱 Responsive Design Considerations

```css
/* Admin Panel Styles */
.dashboard {
    padding: 20px;
    background: #f5f5f5;
    min-height: 100vh;
}

.stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 20px;
    margin-bottom: 30px;
}

.stat-card {
    background: white;
    padding: 20px;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.stat-value {
    font-size: 2rem;
    font-weight: bold;
    color: #333;
    margin: 10px 0;
}

.stat-change {
    font-size: 0.9rem;
    color: #28a745;
}

.stat-change.warning {
    color: #dc3545;
}

.status {
    padding: 4px 8px;
    border-radius: 4px;
    font-size: 0.8rem;
    font-weight: bold;
}

.status.active {
    background: #d4edda;
    color: #155724;
}

.status.inactive {
    background: #f8d7da;
    color: #721c24;
}

.status.pending {
    background: #fff3cd;
    color: #856404;
}

@media (max-width: 768px) {
    .stats-grid {
        grid-template-columns: 1fr;
    }
    
    .dashboard-content {
        flex-direction: column;
    }
    
    table {
        font-size: 0.8rem;
    }
}
```

---

## 🎯 11. DEVELOPMENT ROADMAP

### ✅ Currently Available (Ready to Use)
- **Authentication System** - Complete with JWT and role-based access
- **User Management** - Full CRUD with filtering and statistics
- **Product Management** - Advanced product catalog with search
- **Order Management** - Complete order workflow with status tracking
- **Brand & Category** - Basic management functionality
- **Cart System** - Advanced shopping cart with validation
- **Wishlist System** - Multiple wishlists with sharing

### 🚧 In Development
- **Coupon System** - Basic structure exists, needs full implementation
- **Payment Integration** - VNPay, MoMo, bank transfer gateways
- **Shipping System** - Real-time rate calculation and tracking
- **Analytics Dashboard** - Advanced reporting and insights

### 📋 Planned Features
- **Review & Rating System** - Product reviews with moderation
- **Notification System** - Real-time notifications and email alerts
- **File Upload System** - Image and document management
- **Advanced Search** - Elasticsearch integration
- **Multi-language Support** - Content management in multiple languages

---

## 🚀 12. GETTING STARTED

### Quick Setup Checklist

1. **Setup Authentication**
   ```javascript
   // Implement login flow
   await adminLogin({ email, password });
   ```

2. **Build Dashboard**
   ```javascript
   // Get dashboard statistics
   const stats = await getDashboardStats();
   ```

3. **User Management**
   ```javascript
   // Get and manage users
   const users = await getUsers(filters);
   ```

4. **Product Catalog**
   ```javascript
   // Manage products
   const products = await getProducts(filters);
   ```

5. **Order Processing**
   ```javascript
   // Handle orders
   const orders = await getOrders(filters);
   ```

### 🔧 Environment Configuration

```javascript
// .env file
REACT_APP_API_URL=https://localhost:8080/api
REACT_APP_APP_NAME=SakuraHome Admin
REACT_APP_VERSION=1.0.0

// config.js
export const CONFIG = {
    API_URL: process.env.REACT_APP_API_URL,
    APP_NAME: process.env.REACT_APP_APP_NAME,
    VERSION: process.env.REACT_APP_VERSION,
    PAGINATION: {
        DEFAULT_PAGE_SIZE: 20,
        MAX_PAGE_SIZE: 100
    },
    UPLOAD: {
        MAX_FILE_SIZE: 5 * 1024 * 1024, // 5MB
        ALLOWED_TYPES: ['image/jpeg', 'image/png', 'image/webp']
    }
};
```

---

## 📞 13. SUPPORT & RESOURCES

### API Documentation
- **Swagger UI**: `https://localhost:8080/swagger`
- **Test Files**: `SakuraHomeAPI/Tests/*.http`
- **Documentation**: `SakuraHomeAPI/Docs/`

### Common Patterns

```javascript
// Pagination Helper
const usePagination = (fetchFunction) => {
    const [data, setData] = useState([]);
    const [loading, setLoading] = useState(false);
    const [pagination, setPagination] = useState({
        page: 1,
        pageSize: 20,
        totalCount: 0
    });

    const loadData = async (filters = {}) => {
        setLoading(true);
        try {
            const result = await fetchFunction({
                ...filters,
                page: pagination.page,
                pageSize: pagination.pageSize
            });
            
            if (result.success) {
                setData(result.data.items || result.data);
                setPagination(prev => ({
                    ...prev,
                    totalCount: result.data.totalCount
                }));
            }
        } catch (error) {
            console.error('Failed to load data:', error);
        } finally {
            setLoading(false);
        }
    };

    return { data, loading, pagination, loadData, setPagination };
};

// Form Helper
const useForm = (initialValues, onSubmit) => {
    const [values, setValues] = useState(initialValues);
    const [errors, setErrors] = useState({});
    const [loading, setLoading] = useState(false);

    const handleChange = (name, value) => {
        setValues(prev => ({ ...prev, [name]: value }));
        if (errors[name]) {
            setErrors(prev => ({ ...prev, [name]: null }));
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        try {
            await onSubmit(values);
        } catch (error) {
            setErrors(error.errors || {});
        } finally {
            setLoading(false);
        }
    };

    return { values, errors, loading, handleChange, handleSubmit, setValues };
};
```

### Performance Tips

```javascript
// Debounced Search
import { useMemo, useState, useEffect } from 'react';
import { debounce } from 'lodash';

const useSearch = (searchFunction, delay = 300) => {
    const [query, setQuery] = useState('');
    const [results, setResults] = useState([]);
    const [loading, setLoading] = useState(false);

    const debouncedSearch = useMemo(
        () => debounce(async (searchQuery) => {
            if (searchQuery.trim()) {
                setLoading(true);
                try {
                    const results = await searchFunction(searchQuery);
                    setResults(results);
                } catch (error) {
                    console.error('Search failed:', error);
                } finally {
                    setLoading(false);
                }
            } else {
                setResults([]);
            }
        }, delay),
        [searchFunction, delay]
    );

    useEffect(() => {
        debouncedSearch(query);
    }, [query, debouncedSearch]);

    return { query, setQuery, results, loading };
};
```

---

## 🎉 CONCLUSION

Bạn đã có **toàn bộ thông tin cần thiết** để xây dựng một trang admin hoàn chỉnh cho SakuraHome API! 🚀

### ✅ Các tính năng đã sẵn sàng:
- **Authentication & Authorization** - JWT với role-based access
- **User Management** - Complete CRUD với filtering
- **Product Management** - Advanced catalog với search
- **Order Management** - Full workflow với status tracking
- **Brand & Category** - Basic management
- **Analytics Dashboard** - Statistics và insights
- **Error Handling** - Professional error management

### 🎯 Recommended Implementation Order:
1. **Authentication & Dashboard** - Setup login và overview
2. **User Management** - Build user administration
3. **Product Catalog** - Create product management
4. **Order Processing** - Handle order workflow
5. **Analytics & Reports** - Add insights và statistics

### 💡 Pro Tips:
- Sử dụng **pagination** cho tất cả danh sách
- Implement **real-time updates** với WebSocket
- Add **caching** cho dữ liệu static
- Use **optimistic updates** cho better UX
- Implement **progressive loading** cho large datasets

**Happy coding! 🎊**