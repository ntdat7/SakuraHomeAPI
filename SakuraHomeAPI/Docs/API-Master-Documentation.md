# 🏠 SakuraHome API - Comprehensive Endpoints Documentation

## 📖 Tổng quan

SakuraHome API là một hệ thống e-commerce hoàn chỉnh được xây dựng trên .NET 9, cung cấp đầy đủ các chức năng cho một nền tảng thương mại điện tử hiện đại với tập trung vào sản phẩm Nhật Bản.

### 🌐 Base URLhttps://localhost:8080
### 🔐 Authentication
Hầu hết các endpoint yêu cầu JWT Bearer token:Authorization: Bearer your-jwt-token
---

## 📋 Danh sách Controllers & Endpoints

### 🔑 1. AuthController - Xác thực người dùng (13 endpoints)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/login` | Đăng nhập | ❌ |
| POST | `/api/auth/register` | Đăng ký tài khoản | ❌ |
| POST | `/api/auth/logout` | Đăng xuất | ✅ |
| POST | `/api/auth/refresh-token` | Làm mới token | ❌ |
| POST | `/api/auth/forgot-password` | Quên mật khẩu | ❌ |
| POST | `/api/auth/reset-password` | Đặt lại mật khẩu | ❌ |
| POST | `/api/auth/change-password` | Đổi mật khẩu | ✅ |
| POST | `/api/auth/verify-email` | Xác thực email | ❌ |
| POST | `/api/auth/resend-email-verification` | Gửi lại email xác thực | ❌ |
| GET | `/api/auth/me` | Thông tin người dùng hiện tại | ✅ |
| POST | `/api/auth/revoke-token` | Thu hồi refresh token | ✅ |
| POST | `/api/auth/revoke-all-tokens` | Thu hồi tất cả token | ✅ |
| GET | `/api/auth/check` | Kiểm tra trạng thái xác thực | ✅ |

### 👤 2. UserController - Quản lý người dùng (9 endpoints)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/user/profile` | Lấy thông tin hồ sơ | ✅ |
| PUT | `/api/user/profile` | Cập nhật hồ sơ | ✅ |
| DELETE | `/api/user/profile` | Xóa tài khoản | ✅ |
| GET | `/api/user/stats` | Thống kê người dùng | ✅ |
| GET | `/api/user/addresses` | Danh sách địa chỉ | ✅ |
| POST | `/api/user/addresses` | Thêm địa chỉ mới | ✅ |
| PUT | `/api/user/addresses/{id}` | Cập nhật địa chỉ | ✅ |
| DELETE | `/api/user/addresses/{id}` | Xóa địa chỉ | ✅ |
| PATCH | `/api/user/addresses/{id}/set-default` | Đặt địa chỉ mặc định | ✅ |

### 🛒 3. CartController - Giỏ hàng (12 endpoints)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/cart` | Lấy giỏ hàng | 🔄 |
| GET | `/api/cart/summary` | Tóm tắt giỏ hàng | 🔄 |
| POST | `/api/cart/items` | Thêm sản phẩm vào giỏ | 🔄 |
| PUT | `/api/cart/items` | Cập nhật số lượng | 🔄 |
| DELETE | `/api/cart/items` | Xóa sản phẩm khỏi giỏ | 🔄 |
| DELETE | `/api/cart/clear` | Xóa toàn bộ giỏ hàng | 🔄 |
| PUT | `/api/cart/bulk` | Cập nhật hàng loạt | 🔄 |
| POST | `/api/cart/validate` | Xác thực giỏ hàng | 🔄 |
| POST | `/api/cart/merge` | Gộp giỏ hàng (guest + user) | ✅ |
| POST | `/api/cart/coupon/{couponCode}` | Áp dụng mã giảm giá | 🔄 |
| DELETE | `/api/cart/coupon` | Xóa mã giảm giá | 🔄 |

### ❤️ 4. WishlistController - Danh sách yêu thích (9 endpoints)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/wishlist` | Lấy danh sách yêu thích | ✅ |
| GET | `/api/wishlist/summary` | Tóm tắt wishlist | ✅ |
| POST | `/api/wishlist` | Thêm vào wishlist | ✅ |
| DELETE | `/api/wishlist/{productId}` | Xóa khỏi wishlist | ✅ |
| DELETE | `/api/wishlist/clear` | Xóa toàn bộ wishlist | ✅ |
| POST | `/api/wishlist/{productId}/move-to-cart` | Chuyển sang giỏ hàng | ✅ |
| POST | `/api/wishlist/move-all-to-cart` | Chuyển tất cả sang giỏ | ✅ |
| GET | `/api/wishlist/check/{productId}` | Kiểm tra sản phẩm trong wishlist | ✅ |
| POST | `/api/wishlist/share` | Chia sẻ wishlist | ✅ |

### 📦 5. OrderController - Quản lý đơn hàng (20 endpoints)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/order` | Danh sách đơn hàng | ✅ |
| GET | `/api/order/{id}` | Chi tiết đơn hàng | ✅ |
| POST | `/api/order` | Tạo đơn hàng | ✅ |
| PUT | `/api/order/{id}/cancel` | Hủy đơn hàng | ✅ |
| GET | `/api/order/{id}/status` | Trạng thái đơn hàng | ✅ |
| PUT | `/api/order/{id}/status` | Cập nhật trạng thái (Staff) | ✅ |
| GET | `/api/order/{id}/tracking` | Theo dõi đơn hàng | ✅ |
| POST | `/api/order/{id}/confirm-delivery` | Xác nhận giao hàng | ✅ |
| POST | `/api/order/{id}/return` | Yêu cầu trả hàng | ✅ |
| GET | `/api/order/{id}/return-status` | Trạng thái trả hàng | ✅ |
| PUT | `/api/order/{id}/return` | Xử lý trả hàng (Staff) | ✅ |
| POST | `/api/order/{id}/refund` | Hoàn tiền (Staff) | ✅ |
| GET | `/api/order/stats` | Thống kê đơn hàng (User) | ✅ |
| GET | `/api/order/admin/stats` | Thống kê tổng quan (Admin) | ✅ |
| GET | `/api/order/admin/analytics` | Phân tích đơn hàng (Admin) | ✅ |
| POST | `/api/order/bulk-update` | Cập nhật hàng loạt (Staff) | ✅ |
| GET | `/api/order/export` | Xuất dữ liệu đơn hàng (Staff) | ✅ |
| POST | `/api/order/{id}/add-note` | Thêm ghi chú (Staff) | ✅ |
| GET | `/api/order/pending-confirmation` | Đơn hàng chờ xác nhận (Staff) | ✅ |
| GET | `/api/order/ready-to-ship` | Đơn hàng sẵn sàng giao (Staff) | ✅ |

---

## 🛍️ 6. ProductController - HỆ THỐNG TÌM KIẾM SAN PHẨM NÂNG CAO

### 🎯 **Enhanced Product Search & Filter System (25+ endpoints)**

SakuraHome API được trang bị **hệ thống tìm kiếm và lọc sản phẩm nâng cao nhất** với khả năng xử lý 45+ biến thể endpoint khác nhau.

---

### 🔍 **CORE SEARCH ENDPOINTS**

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/product` | **🌟 ENDPOINT CHÍNH - Tìm kiếm nâng cao** | ❌ |
| GET | `/api/product/search` | Tìm kiếm chuyên dụng với relevance scoring | ❌ |
| GET | `/api/product/{id}` | Chi tiết sản phẩm theo ID | ❌ |
| GET | `/api/product/slug/{slug}` | Chi tiết sản phẩm theo slug | ❌ |
| GET | `/api/product/{id}/related` | Sản phẩm liên quan thông minh | ❌ |
| GET | `/api/product/{id}/stock-availability` | Kiểm tra tồn kho real-time | ❌ |

---

### 🏷️ **TAG-BASED SEARCH SYSTEM (MỚI)**

#### **1. Tìm kiếm theo Tag Names:**# Single tag
GET /api/product?tagNames=Premium

# Multiple tags với ANY logic (OR)
GET /api/product?tagNames=Premium,Japanese,Authentic&tagMatchMode=Any

# Multiple tags với ALL logic (AND)  
GET /api/product?tagNames=Premium,Japanese&tagMatchMode=All
#### **2. Tìm kiếm trong Tags Field:**# String search in tags field
GET /api/product?tagsSearch=traditional

# Combined text and tag search
GET /api/product?search=sake&tagsSearch=premium&tagNames=Japanese
#### **3. Ví dụ Tag Search nâng cao:**# Tìm sản phẩm có tag "Premium" HOẶC "Luxury"
GET /api/product?tagNames=Premium,Luxury&tagMatchMode=Any&page=1&pageSize=20

# Tìm sản phẩm có CÙNG LÚC tag "Japanese" VÀ "Authentic"
GET /api/product?tagNames=Japanese,Authentic&tagMatchMode=All&sortBy=rating&sortOrder=desc

# Tìm sản phẩm có chứa từ "traditional" trong tags
GET /api/product?tagsSearch=traditional&minRating=4.0&inStockOnly=true
---

### 📊 **ADVANCED SORTING ALGORITHMS**

#### **1. Relevance Scoring (Thông minh):**# Sắp xếp theo độ liên quan với từ khóa tìm kiếm
GET /api/product?search=japanese sake&sortBy=relevance&sortOrder=desc

# Relevance + multiple filters
GET /api/product?search=premium matcha&categoryId=1&sortBy=relevance&minRating=4.5
#### **2. Popularity Scoring (Views + Sales + Rating):**# Sắp xếp theo độ phổ biến tổng hợp
GET /api/product?sortBy=popularity&page=1&pageSize=20

# Popularity trong category cụ thể
GET /api/product?categoryId=2&sortBy=popularity&inStockOnly=true
#### **3. Discount Percentage Sorting:**# Sắp xếp theo % giảm giá cao nhất
GET /api/product?hasDiscount=true&sortBy=discount&sortOrder=desc

# Sale products với filter bổ sung
GET /api/product?onSaleOnly=true&sortBy=discount&minPrice=100000&maxPrice=1000000
#### **4. Multi-field Sorting Examples:**# Sắp xếp theo rating cao nhất
GET /api/product?sortBy=rating&sortOrder=desc&minRating=4.0

# Sắp xếp theo bán chạy nhất
GET /api/product?sortBy=sold&sortOrder=desc&page=1&pageSize=15

# Sắp xếp theo lượt xem nhiều nhất
GET /api/product?sortBy=views&sortOrder=desc&isFeatured=true

# Sắp xếp theo tồn kho
GET /api/product?sortBy=stock&sortOrder=asc&minStock=1
---

### 🎛️ **COMPREHENSIVE FILTERING SYSTEM**

#### **1. Price & Rating Filters:**# Khoảng giá cụ thể
GET /api/product?minPrice=500000&maxPrice=2000000&page=1&pageSize=20

# Rating tối thiểu với price range
GET /api/product?minRating=4.5&minPrice=100000&maxPrice=500000&sortBy=rating

# High-end products
GET /api/product?minPrice=5000000&minRating=4.8&sortBy=price&sortOrder=desc
#### **2. Stock & Inventory Filters:**# Chỉ sản phẩm còn hàng
GET /api/product?inStockOnly=true&page=1&pageSize=50

# Sản phẩm sắp hết hàng (< 10 items)
GET /api/product?minStock=1&maxStock=10&sortBy=stock&sortOrder=asc

# Khoảng tồn kho cụ thể
GET /api/product?minStock=50&maxStock=200&categoryId=1
#### **3. Date Range Filters:**# Sản phẩm được tạo trong năm 2024
GET /api/product?createdFrom=2024-01-01&createdTo=2024-12-31&sortBy=created&sortOrder=desc

# Sản phẩm mới trong 30 ngày qua
GET /api/product?createdFrom=2024-07-01&isNew=true&sortBy=created&sortOrder=desc

# Sản phẩm có sẵn trong khoảng thời gian
GET /api/product?availableFrom=2024-01-01&availableUntil=2024-12-31
#### **4. Weight & Dimension Filters:**# Khoảng trọng lượng
GET /api/product?minWeight=100&maxWeight=1000&page=1&pageSize=20

# Sản phẩm nhẹ (dưới 500g)
GET /api/product?maxWeight=500&categoryId=1&sortBy=weight&sortOrder=asc

# Sản phẩm nặng (trên 2kg)
GET /api/product?minWeight=2000&sortBy=weight&sortOrder=desc
---

### 🌍 **JAPANESE-SPECIFIC FILTERS**

#### **1. Origin & Region Filters:**# Sản phẩm từ Tokyo
GET /api/product?origin=Tokyo&page=1&pageSize=20

# Theo vùng miền Nhật Bản
GET /api/product?japaneseRegion=Tokyo&sortBy=rating&sortOrder=desc

# Multiple Japanese regions
GET /api/product?japaneseRegion=Osaka&authenticityLevel=Verified
#### **2. Authenticity Filters:**# Sản phẩm đã xác thực
GET /api/product?authenticityLevel=Verified&minRating=4.0

# Sản phẩm authentic Japanese
GET /api/product?isJapaneseProduct=true&authenticityLevel=Verified&tagNames=Authentic

# Traditional Japanese products
GET /api/product?japaneseRegion=Kyoto&tagNames=Traditional,Authentic&sortBy=rating
---

### ✅ **BOOLEAN FILTERS (Advanced Combinations)**

#### **1. Product Status Filters:**# Sản phẩm nổi bật
GET /api/product?isFeatured=true&sortBy=popularity&page=1&pageSize=12

# Sản phẩm mới
GET /api/product?isNew=true&sortBy=created&sortOrder=desc&page=1&pageSize=15

# Sản phẩm bán chạy
GET /api/product?isBestseller=true&categoryId=1&sortBy=sold&sortOrder=desc

# Sản phẩm phiên bản giới hạn
GET /api/product?isLimitedEdition=true&sortBy=price&sortOrder=desc
#### **2. Sales & Promotion Filters:**# Sản phẩm đang sale
GET /api/product?onSaleOnly=true&sortBy=discount&page=1&pageSize=25

# Sản phẩm có giảm giá
GET /api/product?hasDiscount=true&minPrice=100000&sortBy=discount&sortOrder=desc

# Combined sales filters
GET /api/product?hasDiscount=true&isFeatured=true&inStockOnly=true
#### **3. Service Filters:**# Sản phẩm có thể gói quà
GET /api/product?isGiftWrappingAvailable=true&minPrice=200000&sortBy=popularity

# Cho phép đặt hàng trước
GET /api/product?allowBackorder=true&sortBy=rating&sortOrder=desc

# Multiple service options
GET /api/product?isGiftWrappingAvailable=true&allowPreorder=true&isFeatured=true
---

### 🔍 **DISCOVERY ENDPOINTS**

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/product/featured` | Sản phẩm nổi bật | ❌ |
| GET | `/api/product/newest` | Sản phẩm mới nhất | ❌ |
| GET | `/api/product/bestsellers` | Sản phẩm bán chạy | ❌ |
| GET | `/api/product/on-sale` | Sản phẩm đang sale | ❌ |
| GET | `/api/product/trending` | Sản phẩm trending | ❌ |
| GET | `/api/product/category/{categoryId}` | Sản phẩm theo danh mục | ❌ |
| GET | `/api/product/brand/{brandId}` | Sản phẩm theo thương hiệu | ❌ |

#### **Discovery Examples:**# Top 10 sản phẩm nổi bật
GET /api/product/featured?count=10

# 15 sản phẩm mới nhất
GET /api/product/newest?count=15

# 20 sản phẩm bán chạy nhất
GET /api/product/bestsellers?count=20

# Sản phẩm trending trong 7 ngày qua
GET /api/product/trending?count=12&daysPeriod=7

# Sản phẩm theo danh mục với subcategories
GET /api/product/category/1?includeSubcategories=true&page=1&pageSize=20
---

### 🎯 **REAL-WORLD SCENARIOS (Use Cases)**

#### **1. 🛍️ Khách hàng tìm snack Nhật Bản:**GET /api/product?search=japanese snack&categoryId=1&japaneseRegion=Tokyo&authenticityLevel=Verified&inStockOnly=true&sortBy=rating&sortOrder=desc&page=1&pageSize=12
#### **2. 🛍️ Tìm sản phẩm làm đẹp cao cấp:**GET /api/product?categoryId=2&tagNames=Premium,Luxury&minPrice=1000000&minRating=4.5&isGiftWrappingAvailable=true&sortBy=popularity&page=1&pageSize=10
#### **3. 🛍️ Mua sắm sale cuối năm:**GET /api/product?hasDiscount=true&onSaleOnly=true&minPrice=100000&maxPrice=5000000&sortBy=discount&sortOrder=desc&page=1&pageSize=25
#### **4. 🛍️ Tìm quà tặng traditional:**GET /api/product?tagNames=Traditional,Gift,Authentic&isGiftWrappingAvailable=true&japaneseRegion=Kyoto&minRating=4.0&sortBy=popularity&page=1&pageSize=15
#### **5. 🛍️ Mua sắm tiết kiệm chất lượng:**GET /api/product?maxPrice=500000&minRating=4.0&inStockOnly=true&hasDiscount=true&sortBy=price&sortOrder=asc&page=1&pageSize=20
#### **6. 🛍️ Săn sản phẩm limited edition:**GET /api/product?isLimitedEdition=true&isFeatured=true&authenticityLevel=Verified&sortBy=created&sortOrder=desc&page=1&pageSize=8
---

### 🔧 **MANAGEMENT ENDPOINTS (Staff/Admin)**

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/product` | Tạo sản phẩm mới | ✅ (Staff) |
| PUT | `/api/product/{id}` | Cập nhật sản phẩm | ✅ (Staff) |
| DELETE | `/api/product/{id}` | Xóa sản phẩm | ✅ (Staff) |
| PATCH | `/api/product/{id}/stock` | Cập nhật tồn kho | ✅ (Staff) |
| PATCH | `/api/product/{id}/status` | Cập nhật trạng thái | ✅ (Staff) |
| GET | `/api/product/low-stock` | Sản phẩm sắp hết hàng | ✅ (Staff) |
| GET | `/api/product/out-of-stock` | Sản phẩm hết hàng | ✅ (Staff) |
| GET | `/api/product/sku-available` | Kiểm tra SKU khả dụng | ✅ (Staff) |
| GET | `/api/product/slug-available` | Kiểm tra slug khả dụng | ✅ (Staff) |

---

### 🔥 **COMPLEX FILTER COMBINATIONS**

#### **1. Maximum Realistic Filters:**GET /api/product?search=premium japanese&categoryId=1&brandId=3&minPrice=200000&maxPrice=2000000&minRating=4.5&tagNames=Premium,Authentic&tagMatchMode=All&japaneseRegion=Tokyo&authenticityLevel=Verified&inStockOnly=true&isFeatured=true&isGiftWrappingAvailable=true&sortBy=relevance&page=1&pageSize=15
#### **2. Regional + Quality + Service Combo:**GET /api/product?japaneseRegion=Osaka&authenticityLevel=Verified&minRating=4.8&isLimitedEdition=true&allowPreorder=true&tagNames=Traditional,Premium&tagMatchMode=All&sortBy=popularity&page=1&pageSize=8
#### **3. Sale Hunting Pro:**GET /api/product?hasDiscount=true&onSaleOnly=true&minPrice=500000&maxPrice=3000000&isFeatured=true&inStockOnly=true&minRating=4.5&sortBy=discount&sortOrder=desc&page=1&pageSize=20
---

### 📊 **PERFORMANCE METRICS**

| Search Type | Target Response Time | Cache Duration | Complexity Level |
|-------------|---------------------|----------------|------------------|
| Simple text search | < 150ms | 5 minutes | ⭐ |
| Tag-based search | < 200ms | 5 minutes | ⭐⭐ |
| Complex filters (5+ params) | < 300ms | 3 minutes | ⭐⭐⭐ |
| Combined search + filters | < 400ms | 2 minutes | ⭐⭐⭐⭐ |
| Maximum filters (10+ params) | < 500ms | 1 minute | ⭐⭐⭐⭐⭐ |

### 🎯 **SEARCH INTELLIGENCE FEATURES**

#### **1. Auto-suggestions & Completions:**# Search suggestions
GET /api/product/search/suggestions?q=jap&maxSuggestions=10

# Popular search terms
GET /api/product/search/popular?count=15

# Trending searches
GET /api/product/search/trending?days=7&count=10
#### **2. Search Analytics:**# Search facets for filter refinement
GET /api/product/search/facets?search=japanese&categoryId=1

# Search