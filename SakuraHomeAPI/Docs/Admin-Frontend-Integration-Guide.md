# 🎯 SakuraHome API - Hướng dẫn tích hợp Frontend cho trang Admin

## 📋 Tổng quan

Tài liệu này cung cấp hướng dẫn đầy đủ cho front-end developers để xây dựng **trang quản trị (Admin Panel)** cho hệ thống SakuraHome API. Bao gồm tất cả endpoints, authentication, data models và best practices.

## 🔧 Cấu hình cơ bản

### Base URL & Environment
```javascript
// Development
const API_BASE_URL = 'https://localhost:7240/api';

// Production (cập nhật khi deploy)
const API_BASE_URL = 'https://your-domain.com/api';
```

### Authentication Headers
```javascript
// Mỗi request cần include JWT token
const headers = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${localStorage.getItem('adminToken')}`
};
```

## 🔐 1. AUTHENTICATION SYSTEM

### 🚀 Login Flow cho Admin

```javascript
// 1. Admin Login
const adminLogin = async (email, password) => {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            email: email,
            password: password,
            rememberMe: true
        })
    });
    
    if (response.ok) {
        const result = await response.json();
        // Lưu token và thông tin admin
        localStorage.setItem('adminToken', result.data.token);
        localStorage.setItem('refreshToken', result.data.refreshToken);
        localStorage.setItem('adminInfo', JSON.stringify(result.data.user));
        return result.data;
    }
    throw new Error('Login failed');
};

// 2. Check Admin Role
const checkAdminRole = () => {
    const adminInfo = JSON.parse(localStorage.getItem('adminInfo') || '{}');
    return adminInfo.role === 'Admin' || adminInfo.role === 'Staff';
};

// 3. Refresh Token khi hết hạn
const refreshToken = async () => {
    const refresh = localStorage.getItem('refreshToken');
    const response = await fetch(`${API_BASE_URL}/auth/refresh-token`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: refresh })
    });
    
    if (response.ok) {
        const result = await response.json();
        localStorage.setItem('adminToken', result.data.token);
        return result.data.token;
    }
    // Redirect to login if refresh fails
    window.location.href = '/admin/login';
};
```

### Core Auth Endpoints
```http
POST /api/auth/login                 # Admin login
POST /api/auth/refresh-token         # Refresh access token  
POST /api/auth/logout               # Logout
GET /api/auth/me                    # Get current admin info
POST /api/auth/change-password      # Change password
```

## 👥 2. USER MANAGEMENT

### 📊 Dashboard API

```javascript
// Lấy thống kê users cho dashboard
const getUserStats = async () => {
    const response = await fetch(`${API_BASE_URL}/user/stats`, {
        headers: authHeaders
    });
    return await response.json();
};

// Kết quả trả về:
{
    "success": true,
    "data": {
        "totalUsers": 1250,
        "activeUsers": 980,
        "newUsersThisMonth": 145,
        "averageOrderValue": 875000,
        "topSpendingUsers": [...],
        "userGrowthChart": [...]
    }
}
```

### 🔍 User Management APIs

```javascript
// 1. Lấy danh sách users (có phân trang & filter)
const getUsers = async (page = 1, pageSize = 20, filters = {}) => {
    const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
        ...filters
    });
    
    const response = await fetch(`${API_BASE_URL}/user?${params}`, {
        headers: authHeaders
    });
    return await response.json();
};

// Filters có thể include:
const filters = {
    role: 'Customer',           // Customer, Staff, Admin
    status: 'Active',           // Active, Inactive, Locked
    tier: 'Gold',              // Bronze, Silver, Gold, Platinum
    emailVerified: 'true',     // true, false
    search: 'john@email.com'   // Tìm theo email/tên
};

// 2. Lấy chi tiết một user
const getUserById = async (userId) => {
    const response = await fetch(`${API_BASE_URL}/user/${userId}`, {
        headers: authHeaders
    });
    return await response.json();
};

// 3. Cập nhật user (Admin only)
const updateUser = async (userId, userData) => {
    const response = await fetch(`${API_BASE_URL}/user/${userId}`, {
        method: 'PUT',
        headers: authHeaders,
        body: JSON.stringify(userData)
    });
    return await response.json();
};

// 4. Khóa/Mở khóa user account
const toggleUserStatus = async (userId, status) => {
    const response = await fetch(`${API_BASE_URL}/user/${userId}/status`, {
        method: 'PATCH',
        headers: authHeaders,
        body: JSON.stringify({ status: status }) // 'Active', 'Locked'
    });
    return await response.json();
};
```

### User Management Endpoints
```http
GET /api/user                       # Danh sách users + filters
GET /api/user/{id}                  # Chi tiết user
PUT /api/user/{id}                  # Cập nhật user [Admin]
PATCH /api/user/{id}/status         # Khóa/mở user [Admin]
GET /api/user/stats                 # Thống kê users
GET /api/user/{id}/orders           # Lịch sử đơn hàng của user
GET /api/user/{id}/addresses        # Địa chỉ của user
```

## 🛍️ 3. PRODUCT MANAGEMENT

### 📦 Product CRUD Operations

```javascript
// 1. Lấy danh sách products (có filter & pagination)
const getProducts = async (filters = {}) => {
    const params = new URLSearchParams({
        page: filters.page || 1,
        pageSize: filters.pageSize || 20,
        ...filters
    });
    
    const response = await fetch(`${API_BASE_URL}/product?${params}`, {
        headers: authHeaders
    });
    return await response.json();
};

// Product filters:
const productFilters = {
    categoryId: '123',
    brandId: '456', 
    minPrice: '100000',
    maxPrice: '5000000',
    status: 'Active',              // Active, Inactive, OutOfStock
    featured: 'true',
    search: 'iPhone',
    sortBy: 'price',               // name, price, createdAt, sales
    sortOrder: 'asc'               // asc, desc
};

// 2. Tạo product mới (Staff/Admin)
const createProduct = async (productData) => {
    const response = await fetch(`${API_BASE_URL}/product`, {
        method: 'POST',
        headers: authHeaders,
        body: JSON.stringify(productData)
    });
    return await response.json();
};

// Product data structure:
const newProduct = {
    name: "iPhone 15 Pro Max",
    description: "Latest iPhone with advanced features",
    shortDescription: "iPhone 15 Pro Max 256GB",
    sku: "IP15PM256",
    price: 29990000,
    originalPrice: 32990000,
    categoryId: "smartphone-category-id",
    brandId: "apple-brand-id",
    weight: 221,
    dimensions: "159.9 x 76.7 x 8.25 mm",
    stockQuantity: 50,
    minStockLevel: 5,
    maxStockLevel: 200,
    isActive: true,
    isFeatured: true,
    tags: ["smartphone", "apple", "5g"],
    images: [
        {
            url: "https://example.com/image1.jpg",
            altText: "iPhone 15 Pro Max front",
            isPrimary: true,
            sortOrder: 1
        }
    ],
    attributes: [
        {
            name: "Color",
            value: "Space Black",
            sortOrder: 1
        },
        {
            name: "Storage",
            value: "256GB", 
            sortOrder: 2
        }
    ],
    variants: [
        {
            name: "iPhone 15 Pro Max 512GB",
            sku: "IP15PM512",
            price: 35990000,
            stockQuantity: 30,
            attributes: [
                { name: "Storage", value: "512GB" }
            ]
        }
    ]
};

// 3. Cập nhật product
const updateProduct = async (productId, productData) => {
    const response = await fetch(`${API_BASE_URL}/product/${productId}`, {
        method: 'PUT',
        headers: authHeaders,
        body: JSON.stringify(productData)
    });
    return await response.json();
};

// 4. Cập nhật stock
const updateStock = async (productId, stockData) => {
    const response = await fetch(`${API_BASE_URL}/product/${productId}/stock`, {
        method: 'PATCH',
        headers: authHeaders,
        body: JSON.stringify(stockData)
    });
    return await response.json();
};

// Stock update structure:
const stockUpdate = {
    stockQuantity: 100,
    minStockLevel: 10,
    maxStockLevel: 500,
    reason: "Inventory restock",
    notes: "New shipment received"
};

// 5. Xóa product (soft delete)
const deleteProduct = async (productId) => {
    const response = await fetch(`${API_BASE_URL}/product/${productId}`, {
        method: 'DELETE',
        headers: authHeaders
    });
    return await response.json();
};
```

### Product Management Endpoints
```http
GET /api/product                    # Danh sách products + filters
GET /api/product/{id}               # Chi tiết product
GET /api/product/sku/{sku}          # Product theo SKU
POST /api/product                   # Tạo product mới [Staff]
PUT /api/product/{id}               # Cập nhật product [Staff]
PATCH /api/product/{id}/stock       # Cập nhật stock [Staff]  
DELETE /api/product/{id}            # Xóa product [Staff]
GET /api/product/debug              # Debug info
```

## 🏷️ 4. BRAND MANAGEMENT

```javascript
// 1. Lấy danh sách brands
const getBrands = async (page = 1, featured = false) => {
    const params = new URLSearchParams({
        page: page.toString(),
        pageSize: '20'
    });
    if (featured) params.append('featured', 'true');
    
    const response = await fetch(`${API_BASE_URL}/brand?${params}`, {
        headers: authHeaders
    });
    return await response.json();
};

// 2. Tạo brand mới
const createBrand = async (brandData) => {
    const response = await fetch(`${API_BASE_URL}/brand`, {
        method: 'POST',
        headers: authHeaders,
        body: JSON.stringify(brandData)
    });
    return await response.json();
};

// Brand data structure:
const newBrand = {
    name: "Apple",
    description: "Technology company known for innovative products",
    logoUrl: "https://example.com/apple-logo.png",
    websiteUrl: "https://apple.com",
    isActive: true,
    isFeatured: true,
    seoTitle: "Apple Products - Official Store",
    seoDescription: "Shop original Apple products with warranty",
    seoKeywords: "apple, iphone, ipad, macbook"
};

// 3. Cập nhật brand (TODO: chưa implement)
const updateBrand = async (brandId, brandData) => {
    const response = await fetch(`${API_BASE_URL}/brand/${brandId}`, {
        method: 'PUT',
        headers: authHeaders,
        body: JSON.stringify(brandData)
    });
    return await response.json();
};

// 4. Xóa brand
const deleteBrand = async (brandId) => {
    const response = await fetch(`${API_BASE_URL}/brand/${brandId}`, {
        method: 'DELETE',
        headers: authHeaders
    });
    return await response.json();
};

// 5. Lấy products theo brand
const getProductsByBrand = async (brandId, page = 1) => {
    const response = await fetch(`${API_BASE_URL}/brand/${brandId}/products?page=${page}`, {
        headers: authHeaders
    });
    return await response.json();
};
```

### Brand Management Endpoints
```http
GET /api/brand                      # Danh sách brands
GET /api/brand/{id}                 # Chi tiết brand
GET /api/brand/featured             # Featured brands
GET /api/brand/{id}/products        # Products của brand
POST /api/brand                     # Tạo brand [Staff]
PUT /api/brand/{id}                 # Cập nhật brand [Staff] - TODO
DELETE /api/brand/{id}              # Xóa brand [Staff]
```

## 📂 5. CATEGORY MANAGEMENT

```javascript
// 1. Lấy category tree (hierachical)
const getCategoryTree = async () => {
    const response = await fetch(`${API_BASE_URL}/category`, {
        headers: authHeaders
    });
    return await response.json();
};

// 2. Lấy root categories
const getRootCategories = async () => {
    const response = await fetch(`${API_BASE_URL}/category/root`, {
        headers: authHeaders
    });
    return await response.json();
};

// 3. Tạo category mới
const createCategory = async (categoryData) => {
    const response = await fetch(`${API_BASE_URL}/category`, {
        method: 'POST',
        headers: authHeaders,
        body: JSON.stringify(categoryData)
    });
    return await response.json();
};

// Category data structure:
const newCategory = {
    name: "Smartphones",
    description: "Mobile phones and accessories",
    parentId: null,                     // null for root category
    slug: "smartphones",
    imageUrl: "https://example.com/smartphones.jpg",
    isActive: true,
    isFeatured: true,
    sortOrder: 1,
    seoTitle: "Smartphones - Best Prices",
    seoDescription: "Shop latest smartphones with best prices",
    seoKeywords: "smartphone, mobile, phone",
    attributes: [                       // Custom attributes for this category
        {
            name: "Brand",
            type: "select",
            required: true,
            options: ["Apple", "Samsung", "Xiaomi"]
        },
        {
            name: "Storage",
            type: "select", 
            required: true,
            options: ["64GB", "128GB", "256GB", "512GB"]
        }
    ]
};

// 4. Cập nhật category (TODO: chưa implement)
const updateCategory = async (categoryId, categoryData) => {
    const response = await fetch(`${API_BASE_URL}/category/${categoryId}`, {
        method: 'PUT',
        headers: authHeaders,
        body: JSON.stringify(categoryData)
    });
    return await response.json();
};

// 5. Xóa category
const deleteCategory = async (categoryId) => {
    const response = await fetch(`${API_BASE_URL}/category/${categoryId}`, {
        method: 'DELETE',
        headers: authHeaders
    });
    return await response.json();
};

// 6. Lấy products theo category
const getProductsByCategory = async (categoryId, page = 1) => {
    const response = await fetch(`${API_BASE_URL}/category/${categoryId}/products?page=${page}`, {
        headers: authHeaders
    });
    return await response.json();
};
```

### Category Management Endpoints
```http
GET /api/category                   # Category tree
GET /api/category/{id}              # Chi tiết category  
GET /api/category/root              # Root categories
GET /api/category/{id}/products     # Products trong category
POST /api/category                  # Tạo category [Staff]
PUT /api/category/{id}              # Cập nhật category [Staff] - TODO
DELETE /api/category/{id}           # Xóa category [Staff]
```

## 📦 6. ORDER MANAGEMENT

### 📊 Order Dashboard & Statistics

```javascript
// 1. Lấy thống kê orders cho dashboard
const getOrderStats = async () => {
    const response = await fetch(`${API_BASE_URL}/order/stats`, {
        headers: authHeaders
    });
    return await response.json();
};

// Kết quả trả về:
{
    "success": true,
    "data": {
        "totalOrders": 5247,
        "todayOrders": 23,
        "pendingOrders": 156,
        "processingOrders": 89,
        "shippedOrders": 234,
        "deliveredOrders": 4523,
        "cancelledOrders": 245,
        "totalRevenue": 1875432000,
        "todayRevenue": 12500000,
        "averageOrderValue": 357000,
        "monthlyGrowth": 15.5,
        "topSellingProducts": [...],
        "recentOrders": [...]
    }
}

// 2. Lấy orders gần đây cho dashboard
const getRecentOrders = async (limit = 10) => {
    const response = await fetch(`${API_BASE_URL}/order/recent?limit=${limit}`, {
        headers: authHeaders
    });
    return await response.json();
};
```

### 🔍 Order Management Operations

```javascript
// 1. Lấy danh sách orders (có filter & pagination)
const getOrders = async (filters = {}) => {
    const params = new URLSearchParams({
        page: filters.page || 1,
        pageSize: filters.pageSize || 20,
        ...filters
    });
    
    const response = await fetch(`${API_BASE_URL}/order?${params}`, {
        headers: authHeaders
    });
    return await response.json();
};

// Order filters:
const orderFilters = {
    status: 'Pending',                 // Pending, Confirmed, Processing, Shipped, Delivered, Cancelled, Returned, Refunded, Completed
    paymentStatus: 'Paid',             // Pending, Paid, Failed, Refunded
    shippingStatus: 'Shipped',         // Pending, Shipped, Delivered
    dateFrom: '2024-01-01',
    dateTo: '2024-12-31',
    minAmount: '100000',
    maxAmount: '5000000',
    customerId: 'user-id',
    search: 'ORD-20240101-001'         // Tìm theo order number/customer
};

// 2. Lấy chi tiết order
const getOrderById = async (orderId) => {
    const response = await fetch(`${API_BASE_URL}/order/${orderId}`, {
        headers: authHeaders
    });
    return await response.json();
};

// 3. Cập nhật order status (Staff operations)
const updateOrderStatus = async (orderId, status, notes = '') => {
    const response = await fetch(`${API_BASE_URL}/order/${orderId}/status`, {
        method: 'PATCH',
        headers: authHeaders,
        body: JSON.stringify({ 
            status: status,
            notes: notes 
        })
    });
    return await response.json();
};

// Available status transitions:
// Pending → Confirmed → Processing → Shipped → Delivered → Completed
// Any status → Cancelled (with reason)
// Delivered → Returned → Refunded

// 4. Staff actions cho orders
const confirmOrder = async (orderId, notes = '') => {
    const response = await fetch(`${API_BASE_URL}/order/${orderId}/confirm`, {
        method: 'PATCH',
        headers: authHeaders,
        body: JSON.stringify({ notes })
    });
    return await response.json();
};

const processOrder = async (orderId, notes = '') => {
    const response = await fetch(`${API_BASE_URL}/order/${orderId}/process`, {
        method: 'PATCH',
        headers: authHeaders,
        body: JSON.stringify({ notes })
    });
    return await response.json();
};

const shipOrder = async (orderId, trackingInfo) => {
    const response = await fetch(`${API_BASE_URL}/order/${orderId}/ship`, {
        method: 'PATCH',
        headers: authHeaders,
        body: JSON.stringify({
            trackingNumber: trackingInfo.trackingNumber,
            shippingProvider: trackingInfo.provider,
            estimatedDelivery: trackingInfo.estimatedDelivery,
            notes: trackingInfo.notes
        })
    });
    return await response.json();
};

const deliverOrder = async (orderId, deliveryInfo) => {
    const response = await fetch(`${API_BASE_URL}/order/${orderId}/deliver`, {
        method: 'PATCH',
        headers: authHeaders,
        body: JSON.stringify({
            deliveredAt: deliveryInfo.deliveredAt,
            receiverName: deliveryInfo.receiverName,
            notes: deliveryInfo.notes
        })
    });
    return await response.json();
};

// 5. Order notes management
const addOrderNote = async (orderId, note) => {
    const response = await fetch(`${API_BASE_URL}/order/${orderId}/staff-notes`, {
        method: 'POST',
        headers: authHeaders,
        body: JSON.stringify({
            content: note.content,
            isInternal: note.isInternal || false,  // Internal notes chỉ staff thấy
            priority: note.priority || 'Normal'    // Low, Normal, High
        })
    });
    return await response.json();
};

const getOrderNotes = async (orderId) => {
    const response = await fetch(`${API_BASE_URL}/order/${orderId}/notes`, {
        headers: authHeaders
    });
    return await response.json();
};

// 6. Return/Refund management
const processReturn = async (orderId, returnData) => {
    const response = await fetch(`${API_BASE_URL}/order/${orderId}/return`, {
        method: 'PATCH',
        headers: authHeaders,
        body: JSON.stringify({
            reason: returnData.reason,
            refundAmount: returnData.refundAmount,
            refundMethod: returnData.refundMethod,
            processingNotes: returnData.notes
        })
    });
    return await response.json();
};

// 7. Lấy order status history
const getOrderStatusHistory = async (orderId) => {
    const response = await fetch(`${API_BASE_URL}/order/${orderId}/status-history`, {
        headers: authHeaders
    });
    return await response.json();
};
```

### Order Management Endpoints
```http
# Customer Operations
GET /api/order                      # Danh sách orders + filters
GET /api/order/{id}                 # Chi tiết order
GET /api/order/{id}/status-history  # Lịch sử status
GET /api/order/{id}/notes           # Order notes

# Staff Operations  
GET /api/order/stats                # Thống kê orders [Staff]
GET /api/order/recent               # Orders gần đây [Staff]
PATCH /api/order/{id}/confirm       # Xác nhận order [Staff]
PATCH /api/order/{id}/process       # Xử lý order [Staff]
PATCH /api/order/{id}/ship          # Giao ship [Staff]
PATCH /api/order/{id}/deliver       # Xác nhận giao hàng [Staff]
PATCH /api/order/{id}/status        # Cập nhật status [Staff]
POST /api/order/{id}/staff-notes    # Thêm staff note [Staff]
PATCH /api/order/{id}/return        # Xử lý return [Staff]
```

## 🛒 7. CART & WISHLIST MONITORING

### Cart Analytics for Admin

```javascript
// 1. Lấy thống kê abandoned carts
const getAbandonedCarts = async () => {
    const response = await fetch(`${API_BASE_URL}/cart/abandoned`, {
        headers: authHeaders
    });
    return await response.json();
};

// 2. Lấy cart conversion statistics  
const getCartStats = async () => {
    const response = await fetch(`${API_BASE_URL}/cart/stats`, {
        headers: authHeaders
    });
    return await response.json();
};
```

### Wishlist Analytics

```javascript
// 1. Lấy most wishlisted products
const getMostWishlistedProducts = async () => {
    const response = await fetch(`${API_BASE_URL}/wishlist/popular-products`, {
        headers: authHeaders
    });
    return await response.json();
};

// 2. Wishlist to cart conversion rates
const getWishlistStats = async () => {
    const response = await fetch(`${API_BASE_URL}/wishlist/stats`, {
        headers: authHeaders
    });
    return await response.json();
};
```

## 💳 8. PAYMENT MANAGEMENT

### Payment Analytics & Management

```javascript
// 1. Lấy payment statistics
const getPaymentStats = async () => {
    const response = await fetch(`${API_BASE_URL}/payment/stats`, {
        headers: authHeaders
    });
    return await response.json();
};

// 2. Lấy payment transactions
const getPaymentTransactions = async (filters = {}) => {
    const params = new URLSearchParams(filters);
    const response = await fetch(`${API_BASE_URL}/payment/transactions?${params}`, {
        headers: authHeaders
    });
    return await response.json();
};

// 3. Process refunds
const processRefund = async (paymentId, refundData) => {
    const response = await fetch(`${API_BASE_URL}/payment/${paymentId}/refund`, {
        method: 'POST',
        headers: authHeaders,
        body: JSON.stringify(refundData)
    });
    return await response.json();
};
```

### Payment Endpoints
```http
GET /api/payment/stats              # Payment statistics [Staff]
GET /api/payment/transactions       # Payment transactions [Staff]
POST /api/payment/{id}/refund       # Process refund [Staff]
GET /api/payment/methods            # Available payment methods
```

## 🔔 9. NOTIFICATION MANAGEMENT

### Send Notifications to Users

```javascript
// 1. Gửi notification đến user cụ thể
const sendPersonalNotification = async (userId, notificationData) => {
    const response = await fetch(`${API_BASE_URL}/notification/send`, {
        method: 'POST',
        headers: authHeaders,
        body: JSON.stringify({
            userId: userId,
            title: notificationData.title,
            message: notificationData.message,
            type: notificationData.type,        // Info, Warning, Success, Error
            priority: notificationData.priority, // Low, Normal, High
            actionUrl: notificationData.actionUrl
        })
    });
    return await response.json();
};

// 2. Gửi bulk notifications
const sendBulkNotification = async (notificationData) => {
    const response = await fetch(`${API_BASE_URL}/notification/broadcast`, {
        method: 'POST',
        headers: authHeaders,
        body: JSON.stringify({
            title: notificationData.title,
            message: notificationData.message,
            type: notificationData.type,
            criteria: {
                userRoles: ['Customer'],           // Filter by roles
                userTiers: ['Gold', 'Platinum'],   // Filter by tiers
                location: 'Ho Chi Minh City',      // Filter by location
                minOrderValue: 1000000             // Filter by spending
            }
        })
    });
    return await response.json();
};

// 3. Lấy notification statistics
const getNotificationStats = async () => {
    const response = await fetch(`${API_BASE_URL}/notification/stats`, {
        headers: authHeaders
    });
    return await response.json();
};
```

### Notification Endpoints
```http
POST /api/notification/send         # Gửi notification cá nhân [Staff]
POST /api/notification/broadcast    # Gửi bulk notification [Admin]
GET /api/notification/stats         # Notification statistics [Staff]
GET /api/notification/templates     # Notification templates [Staff]
```

## 📊 10. DASHBOARD DATA & ANALYTICS

### Main Dashboard APIs

```javascript
// 1. Tổng quan dashboard chính
const getDashboardOverview = async () => {
    const response = await fetch(`${API_BASE_URL}/dashboard/overview`, {
        headers: authHeaders
    });
    return await response.json();
};

// 2. Sales analytics
const getSalesAnalytics = async (period = '30d') => {
    const response = await fetch(`${API_BASE_URL}/analytics/sales?period=${period}`, {
        headers: authHeaders
    });
    return await response.json();
};

// 3. Top performing products
const getTopProducts = async (limit = 10, period = '30d') => {
    const response = await fetch(`${API_BASE_URL}/analytics/top-products?limit=${limit}&period=${period}`, {
        headers: authHeaders
    });
    return await response.json();
};

// 4. User behavior analytics
const getUserAnalytics = async (period = '30d') => {
    const response = await fetch(`${API_BASE_URL}/analytics/users?period=${period}`, {
        headers: authHeaders
    });
    return await response.json();
};

// 5. Inventory alerts
const getInventoryAlerts = async () => {
    const response = await fetch(`${API_BASE_URL}/inventory/alerts`, {
        headers: authHeaders
    });
    return await response.json();
};
```

## 🔧 11. UTILITY FUNCTIONS

### Error Handling

```javascript
// Global error handler cho API calls
const handleApiError = (error, response) => {
    if (response?.status === 401) {
        // Token expired, try refresh
        return refreshToken().then(() => {
            // Retry original request
            return fetch(originalRequest);
        });
    }
    
    if (response?.status === 403) {
        // Insufficient permissions
        showNotification('Bạn không có quyền thực hiện thao tác này', 'error');
        return;
    }
    
    if (response?.status >= 500) {
        // Server error
        showNotification('Lỗi server, vui lòng thử lại sau', 'error');
        return;
    }
    
    // Handle validation errors
    if (error.errors) {
        const errorMessages = Object.values(error.errors).flat().join(', ');
        showNotification(errorMessages, 'error');
    }
};

// Retry mechanism cho network failures
const fetchWithRetry = async (url, options, retries = 3) => {
    for (let i = 0; i < retries; i++) {
        try {
            const response = await fetch(url, options);
            if (response.ok) return response;
            
            if (response.status < 500 || i === retries - 1) {
                throw new Error(`Request failed: ${response.status}`);
            }
        } catch (error) {
            if (i === retries - 1) throw error;
            await new Promise(resolve => setTimeout(resolve, 1000 * (i + 1)));
        }
    }
};
```

### Data Formatting Utilities

```javascript
// Format tiền tệ
const formatCurrency = (amount) => {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
};

// Format date
const formatDate = (dateString) => {
    return new Intl.DateTimeFormat('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    }).format(new Date(dateString));
};

// Status badge colors
const getStatusColor = (status) => {
    const colors = {
        'Pending': 'orange',
        'Confirmed': 'blue',
        'Processing': 'cyan',
        'Shipped': 'purple',
        'Delivered': 'green',
        'Cancelled': 'red',
        'Returned': 'gray',
        'Refunded': 'yellow'
    };
    return colors[status] || 'gray';
};
```

## 📋 12. COMPLETE API ENDPOINTS REFERENCE

### Authentication & Authorization
```http
POST /api/auth/login                 # Admin login
POST /api/auth/logout               # Logout  
POST /api/auth/refresh-token        # Refresh access token
GET /api/auth/me                    # Current admin info
POST /api/auth/change-password      # Change password
```

### User Management
```http
GET /api/user                       # Users list + filters
GET /api/user/{id}                  # User details
PUT /api/user/{id}                  # Update user [Admin]
PATCH /api/user/{id}/status         # Lock/unlock user [Admin]
GET /api/user/stats                 # User statistics
GET /api/user/{id}/orders           # User's order history
GET /api/user/{id}/addresses        # User's addresses
```

### Product Management
```http
GET /api/product                    # Products list + filters
GET /api/product/{id}               # Product details
GET /api/product/sku/{sku}          # Product by SKU
POST /api/product                   # Create product [Staff]
PUT /api/product/{id}               # Update product [Staff]
PATCH /api/product/{id}/stock       # Update stock [Staff]
DELETE /api/product/{id}            # Delete product [Staff]
```

### Brand Management
```http
GET /api/brand                      # Brands list
GET /api/brand/{id}                 # Brand details
GET /api/brand/featured             # Featured brands
GET /api/brand/{id}/products        # Brand's products
POST /api/brand                     # Create brand [Staff]
PUT /api/brand/{id}                 # Update brand [Staff] - TODO
DELETE /api/brand/{id}              # Delete brand [Staff]
```

### Category Management
```http
GET /api/category                   # Category tree
GET /api/category/{id}              # Category details
GET /api/category/root              # Root categories
GET /api/category/{id}/products     # Category's products
POST /api/category                  # Create category [Staff]
PUT /api/category/{id}              # Update category [Staff] - TODO
DELETE /api/category/{id}           # Delete category [Staff]
```

### Order Management
```http
# Customer Operations
GET /api/order                      # Orders list + filters
GET /api/order/{id}                 # Order details
GET /api/order/{id}/status-history  # Status history
GET /api/order/{id}/notes           # Order notes

# Staff Operations
GET /api/order/stats                # Order statistics [Staff]
GET /api/order/recent               # Recent orders [Staff]
PATCH /api/order/{id}/confirm       # Confirm order [Staff]
PATCH /api/order/{id}/process       # Process order [Staff]
PATCH /api/order/{id}/ship          # Ship order [Staff]
PATCH /api/order/{id}/deliver       # Deliver order [Staff]
PATCH /api/order/{id}/status        # Update status [Staff]
POST /api/order/{id}/staff-notes    # Add staff note [Staff]
PATCH /api/order/{id}/return        # Process return [Staff]
```

### Cart & Wishlist Analytics
```http
GET /api/cart/stats                 # Cart statistics [Staff]
GET /api/cart/abandoned             # Abandoned carts [Staff]
GET /api/wishlist/stats             # Wishlist statistics [Staff]
GET /api/wishlist/popular-products  # Popular wishlisted products [Staff]
```

### Payment Management
```http
GET /api/payment/stats              # Payment statistics [Staff]
GET /api/payment/transactions       # Payment transactions [Staff]
POST /api/payment/{id}/refund       # Process refund [Staff]
GET /api/payment/methods            # Payment methods
```

### Notification System
```http
POST /api/notification/send         # Send personal notification [Staff]
POST /api/notification/broadcast    # Send bulk notification [Admin]
GET /api/notification/stats         # Notification statistics [Staff]
GET /api/notification/templates     # Notification templates [Staff]
```

### Analytics & Dashboard
```http
GET /api/dashboard/overview         # Dashboard overview [Staff]
GET /api/analytics/sales            # Sales analytics [Staff]
GET /api/analytics/users            # User analytics [Staff]
GET /api/analytics/top-products     # Top products [Staff]
GET /api/inventory/alerts           # Inventory alerts [Staff]
```

## 🎨 13. SAMPLE FRONTEND COMPONENTS

### Admin Dashboard Layout

```javascript
// Dashboard.jsx
import React, { useState, useEffect } from 'react';

const AdminDashboard = () => {
    const [stats, setStats] = useState({});
    const [recentOrders, setRecentOrders] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        loadDashboardData();
    }, []);

    const loadDashboardData = async () => {
        try {
            const [statsRes, ordersRes] = await Promise.all([
                getDashboardOverview(),
                getRecentOrders(10)
            ]);
            
            setStats(statsRes.data);
            setRecentOrders(ordersRes.data);
        } catch (error) {
            handleApiError(error);
        } finally {
            setLoading(false);
        }
    };

    if (loading) return <div>Loading...</div>;

    return (
        <div className="admin-dashboard">
            {/* Stats Cards */}
            <div className="stats-grid">
                <StatCard 
                    title="Total Orders" 
                    value={stats.totalOrders} 
                    trend={stats.orderGrowth}
                    color="blue"
                />
                <StatCard 
                    title="Revenue Today" 
                    value={formatCurrency(stats.todayRevenue)} 
                    trend={stats.revenueGrowth}
                    color="green"
                />
                <StatCard 
                    title="Active Users" 
                    value={stats.activeUsers} 
                    trend={stats.userGrowth}
                    color="purple"
                />
                <StatCard 
                    title="Pending Orders" 
                    value={stats.pendingOrders} 
                    alert={stats.pendingOrders > 50}
                    color="orange"
                />
            </div>

            {/* Recent Orders Table */}
            <div className="recent-orders">
                <h3>Recent Orders</h3>
                <OrdersTable orders={recentOrders} />
            </div>
        </div>
    );
};
```

### Product Management Component

```javascript
// ProductManagement.jsx
const ProductManagement = () => {
    const [products, setProducts] = useState([]);
    const [filters, setFilters] = useState({});
    const [pagination, setPagination] = useState({ page: 1, pageSize: 20 });

    const loadProducts = async () => {
        try {
            const response = await getProducts({ ...filters, ...pagination });
            setProducts(response.data.items);
            setPagination(prev => ({ ...prev, total: response.data.totalItems }));
        } catch (error) {
            handleApiError(error);
        }
    };

    const handleStockUpdate = async (productId, newStock) => {
        try {
            await updateStock(productId, { stockQuantity: newStock });
            showNotification('Stock updated successfully', 'success');
            loadProducts(); // Reload data
        } catch (error) {
            handleApiError(error);
        }
    };

    return (
        <div className="product-management">
            <div className="header">
                <h2>Product Management</h2>
                <button onClick={() => setShowCreateModal(true)}>
                    Add New Product
                </button>
            </div>

            {/* Filters */}
            <ProductFilters filters={filters} onChange={setFilters} />

            {/* Products Table */}
            <ProductsTable 
                products={products}
                onStockUpdate={handleStockUpdate}
                onEdit={handleEdit}
                onDelete={handleDelete}
            />

            {/* Pagination */}
            <Pagination 
                current={pagination.page}
                total={pagination.total}
                pageSize={pagination.pageSize}
                onChange={(page) => setPagination(prev => ({ ...prev, page }))}
            />
        </div>
    );
};
```

## 🚀 14. BEST PRACTICES

### Performance Optimization

```javascript
// 1. Implement caching cho static data
const cache = new Map();
const getCachedData = async (key, fetchFn, ttl = 300000) => { // 5 minutes
    if (cache.has(key)) {
        const { data, timestamp } = cache.get(key);
        if (Date.now() - timestamp < ttl) {
            return data;
        }
    }
    
    const data = await fetchFn();
    cache.set(key, { data, timestamp: Date.now() });
    return data;
};

// Usage:
const brands = await getCachedData('brands', () => getBrands());

// 2. Debounce search inputs
const useDebounce = (value, delay) => {
    const [debouncedValue, setDebouncedValue] = useState(value);
    
    useEffect(() => {
        const handler = setTimeout(() => {
            setDebouncedValue(value);
        }, delay);
        
        return () => clearTimeout(handler);
    }, [value, delay]);
    
    return debouncedValue;
};

// 3. Pagination với virtual scrolling cho large datasets
const VirtualizedTable = ({ data, renderRow }) => {
    // Implementation with react-window or similar
};
```

### Error Handling Strategy

```javascript
// 1. Global error boundary
class ErrorBoundary extends React.Component {
    constructor(props) {
        super(props);
        this.state = { hasError: false };
    }
    
    static getDerivedStateFromError(error) {
        return { hasError: true };
    }
    
    componentDidCatch(error, errorInfo) {
        console.error('Admin panel error:', error, errorInfo);
        // Log to monitoring service
    }
    
    render() {
        if (this.state.hasError) {
            return <ErrorFallback />;
        }
        return this.props.children;
    }
}

// 2. API error handling
const apiErrorHandler = {
    400: (error) => showNotification('Invalid request data', 'error'),
    401: () => redirectToLogin(),
    403: () => showNotification('Access denied', 'error'),
    404: () => showNotification('Resource not found', 'error'),
    500: () => showNotification('Server error, please try again', 'error')
};
```

### Security Checklist

```javascript
// 1. Token management
const tokenManager = {
    getToken: () => localStorage.getItem('adminToken'),
    setToken: (token) => localStorage.setItem('adminToken', token),
    removeToken: () => localStorage.removeItem('adminToken'),
    isExpired: (token) => {
        const payload = JSON.parse(atob(token.split('.')[1]));
        return payload.exp * 1000 < Date.now();
    }
};

// 2. Role-based UI rendering
const ProtectedComponent = ({ requiredRole, children }) => {
    const userRole = getCurrentUserRole();
    
    if (!hasPermission(userRole, requiredRole)) {
        return <AccessDenied />;
    }
    
    return children;
};

// 3. Input sanitization
const sanitizeInput = (input) => {
    return DOMPurify.sanitize(input);
};
```

## 🎯 15. QUICK START GUIDE

### 1. Setup Authentication

```javascript
// auth.js
export const initializeAuth = () => {
    const token = localStorage.getItem('adminToken');
    if (token && !tokenManager.isExpired(token)) {
        setAuthHeaders(token);
        return true;
    }
    return false;
};

// App.js
useEffect(() => {
    if (!initializeAuth()) {
        router.push('/admin/login');
    }
}, []);
```

### 2. Create Base API Client

```javascript
// api.js
const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:7240/api';

export const apiClient = {
    async request(endpoint, options = {}) {
        const url = `${API_BASE_URL}${endpoint}`;
        const config = {
            headers: {
                'Content-Type': 'application/json',
                ...authHeaders
            },
            ...options
        };
        
        const response = await fetchWithRetry(url, config);
        
        if (!response.ok) {
            const error = await response.json();
            throw new ApiError(error, response.status);
        }
        
        return response.json();
    }
};
```

### 3. Setup Main Modules

```javascript
// admin/
├── pages/
│   ├── Dashboard/
│   ├── Products/
│   ├── Orders/
│   ├── Users/
│   └── Settings/
├── components/
│   ├── Layout/
│   ├── Tables/
│   ├── Forms/
│   └── Charts/
├── services/
│   ├── api.js
│   ├── auth.js
│   └── utils.js
└── App.js
```

## 📞 16. SUPPORT & RESOURCES

### Development Resources
- **Swagger UI**: `https://localhost:7240/swagger`
- **API Documentation**: `SakuraHomeAPI/Docs/`
- **Test Files**: `SakuraHomeAPI/Tests/*.http`

### Common Issues & Solutions
1. **CORS errors**: Configure CORS in backend
2. **Token expiration**: Implement refresh token flow
3. **Rate limiting**: Add retry mechanism
4. **Large datasets**: Use pagination and virtual scrolling

### Next Steps
1. **Complete missing endpoints**: Brand/Category updates
2. **Add real-time features**: WebSocket connections
3. **Implement file uploads**: Product images, documents
4. **Add advanced analytics**: Charts and reports

---

## 🎉 Conclusion

Bạn đã có tất cả thông tin cần thiết để xây dựng một trang admin hoàn chỉnh cho SakuraHome API! 

**Các tính năng chính đã sẵn sàng:**
- ✅ Authentication & Authorization
- ✅ User Management 
- ✅ Product Management
- ✅ Order Management
- ✅ Brand & Category Management
- ✅ Analytics & Dashboard

**Tips để bắt đầu:**
1. Bắt đầu với authentication và dashboard
2. Implement product management trước
3. Thêm order management
4. Cuối cùng là analytics và reports

Chúc bạn thành công trong việc xây dựng admin panel! 🚀