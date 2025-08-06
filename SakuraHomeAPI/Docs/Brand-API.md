# SakuraHome API - Brand Management

## Tổng quan

Hệ thống quản lý thương hiệu SakuraHome cung cấp các chức năng quản lý thương hiệu sản phẩm, bao gồm tạo, xem, cập nhật và xóa thương hiệu.

## Base URL
```
https://localhost:7240/api/brand
```

## Authentication
- Endpoints công khai: Không cần authentication
- Endpoints quản lý (Staff/Admin): Yêu cầu JWT Bearer token
```
Authorization: Bearer your-jwt-token
```

## Endpoints

### 📖 Public Endpoints

#### 1. Lấy danh sách thương hiệu
```http
GET /api/brand
```

**Query Parameters:**
- `page` (int, optional): Trang hiện tại (default: 1)
- `pageSize` (int, optional): Số item mỗi trang (default: 20)
- `search` (string, optional): Tìm kiếm theo tên thương hiệu
- `isActive` (bool, optional): Lọc theo trạng thái hoạt động
- `featured` (bool, optional): Lọc thương hiệu nổi bật

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy danh sách thương hiệu thành công",
  "data": {
    "items": [
      {
        "id": 1,
        "name": "Samsung",
        "slug": "samsung",
        "description": "Thương hiệu điện tử hàng đầu Hàn Quốc",
        "logoUrl": "https://example.com/samsung-logo.jpg",
        "website": "https://samsung.com",
        "isActive": true,
        "isFeatured": true,
        "productCount": 156,
        "displayOrder": 1,
        "createdAt": "2023-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      },
      {
        "id": 2,
        "name": "Apple",
        "slug": "apple",
        "description": "Thương hiệu công nghệ cao cấp từ Mỹ",
        "logoUrl": "https://example.com/apple-logo.jpg",
        "website": "https://apple.com",
        "isActive": true,
        "isFeatured": true,
        "productCount": 89,
        "displayOrder": 2,
        "createdAt": "2023-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "pagination": {
      "currentPage": 1,
      "totalPages": 5,
      "totalCount": 100,
      "pageSize": 20,
      "hasNext": true,
      "hasPrevious": false
    }
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 2. Lấy thương hiệu theo ID
```http
GET /api/brand/{id}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy thông tin thương hiệu thành công",
  "data": {
    "id": 1,
    "name": "Samsung",
    "slug": "samsung",
    "description": "Samsung là tập đoàn đa quốc gia hàng đầu Hàn Quốc, chuyên về điện tử, viễn thông và công nghệ thông tin. Được thành lập năm 1938, Samsung đã trở thành một trong những thương hiệu công nghệ uy tín nhất thế giới.",
    "logoUrl": "https://example.com/samsung-logo.jpg",
    "bannerUrl": "https://example.com/samsung-banner.jpg",
    "website": "https://samsung.com",
    "email": "contact@samsung.com",
    "phone": "+82-2-2255-0114",
    "address": "Samsung Digital City, Suwon, South Korea",
    "isActive": true,
    "isFeatured": true,
    "productCount": 156,
    "displayOrder": 1,
    "metaTitle": "Samsung - Thương hiệu điện tử hàng đầu",
    "metaDescription": "Khám phá các sản phẩm Samsung chính hãng với giá tốt nhất",
    "categories": [
      {
        "id": 1,
        "name": "Điện thoại",
        "productCount": 45
      },
      {
        "id": 2,
        "name": "Laptop",
        "productCount": 23
      },
      {
        "id": 3,
        "name": "TV",
        "productCount": 34
      }
    ],
    "topProducts": [
      {
        "id": 101,
        "name": "Samsung Galaxy S24 Ultra",
        "sku": "SM-S928B",
        "price": 30990000,
        "salePrice": 27990000,
        "imageUrl": "https://example.com/galaxy-s24-ultra.jpg",
        "rating": 4.8,
        "reviewCount": 1250
      }
    ],
    "createdAt": "2023-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 3. Lấy thương hiệu nổi bật
```http
GET /api/brand/featured
```

**Query Parameters:**
- `limit` (int, optional): Số lượng thương hiệu (default: 10, max: 50)

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy thương hiệu nổi bật thành công",
  "data": [
    {
      "id": 1,
      "name": "Samsung",
      "slug": "samsung",
      "logoUrl": "https://example.com/samsung-logo.jpg",
      "productCount": 156,
      "displayOrder": 1
    },
    {
      "id": 2,
      "name": "Apple",
      "slug": "apple", 
      "logoUrl": "https://example.com/apple-logo.jpg",
      "productCount": 89,
      "displayOrder": 2
    }
  ],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 4. Lấy sản phẩm theo thương hiệu
```http
GET /api/brand/{id}/products
```

**Query Parameters:**
- `page` (int, optional): Trang hiện tại (default: 1)
- `pageSize` (int, optional): Số item mỗi trang (default: 20)
- `categoryId` (int, optional): Lọc theo danh mục
- `priceMin` (decimal, optional): Giá tối thiểu
- `priceMax` (decimal, optional): Giá tối đa
- `sortBy` (string, optional): Sắp xếp (name, price, newest, popular)
- `sortOrder` (string, optional): Thứ tự (asc, desc)

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy sản phẩm theo thương hiệu thành công",
  "data": {
    "brand": {
      "id": 1,
      "name": "Samsung",
      "slug": "samsung"
    },
    "products": {
      "items": [
        {
          "id": 101,
          "name": "Samsung Galaxy S24 Ultra",
          "sku": "SM-S928B",
          "slug": "samsung-galaxy-s24-ultra",
          "price": 30990000,
          "salePrice": 27990000,
          "discountPercent": 10,
          "imageUrl": "https://example.com/galaxy-s24-ultra.jpg",
          "rating": 4.8,
          "reviewCount": 1250,
          "isInStock": true,
          "stockQuantity": 45,
          "category": {
            "id": 1,
            "name": "Điện thoại"
          },
          "tags": ["flagship", "camera", "5g"]
        }
      ],
      "pagination": {
        "currentPage": 1,
        "totalPages": 8,
        "totalCount": 156,
        "pageSize": 20,
        "hasNext": true,
        "hasPrevious": false
      }
    }
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### 👨‍💼 Staff & Admin Endpoints

#### 5. Tạo thương hiệu mới (Staff Only)
```http
POST /api/brand
Authorization: Bearer staff-token
```

**Request Body:**
```json
{
  "name": "Xiaomi",
  "description": "Thương hiệu công nghệ thông minh từ Trung Quốc",
  "logoUrl": "https://example.com/xiaomi-logo.jpg",
  "bannerUrl": "https://example.com/xiaomi-banner.jpg",
  "website": "https://mi.com",
  "email": "contact@mi.com",
  "phone": "+86-400-100-5678",
  "address": "Xiaomi Campus, Beijing, China",
  "isActive": true,
  "isFeatured": false,
  "displayOrder": 10,
  "metaTitle": "Xiaomi - Thương hiệu công nghệ thông minh",
  "metaDescription": "Sản phẩm Xiaomi chính hãng với giá cạnh tranh nhất"
}
```

**Response Success (201):**
```json
{
  "success": true,
  "message": "Tạo thương hiệu thành công",
  "data": {
    "id": 15,
    "name": "Xiaomi",
    "slug": "xiaomi",
    "description": "Thương hiệu công nghệ thông minh từ Trung Quốc",
    "logoUrl": "https://example.com/xiaomi-logo.jpg",
    "bannerUrl": "https://example.com/xiaomi-banner.jpg",
    "website": "https://mi.com",
    "email": "contact@mi.com",
    "phone": "+86-400-100-5678",
    "address": "Xiaomi Campus, Beijing, China",
    "isActive": true,
    "isFeatured": false,
    "productCount": 0,
    "displayOrder": 10,
    "metaTitle": "Xiaomi - Thương hiệu công nghệ thông minh",
    "metaDescription": "Sản phẩm Xiaomi chính hãng với giá cạnh tranh nhất",
    "createdAt": "2024-01-01T12:00:00Z",
    "updatedAt": "2024-01-01T12:00:00Z"
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 6. Cập nhật thương hiệu (Staff Only)
```http
PUT /api/brand/{id}
Authorization: Bearer staff-token
```

**Request Body:**
```json
{
  "name": "Xiaomi (Updated)",
  "description": "Thương hiệu công nghệ thông minh từ Trung Quốc - cập nhật",
  "logoUrl": "https://example.com/xiaomi-logo-new.jpg",
  "bannerUrl": "https://example.com/xiaomi-banner-new.jpg",
  "website": "https://mi.com",
  "email": "support@mi.com",
  "phone": "+86-400-100-5678",
  "address": "Xiaomi Campus, Beijing, China",
  "isActive": true,
  "isFeatured": true,
  "displayOrder": 5,
  "metaTitle": "Xiaomi - Công nghệ thông minh giá tốt",
  "metaDescription": "Khám phá sản phẩm Xiaomi chính hãng với công nghệ tiên tiến"
}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Cập nhật thương hiệu thành công",
  "data": {
    "id": 15,
    "name": "Xiaomi (Updated)",
    "slug": "xiaomi-updated",
    "description": "Thương hiệu công nghệ thông minh từ Trung Quốc - cập nhật",
    "logoUrl": "https://example.com/xiaomi-logo-new.jpg",
    "bannerUrl": "https://example.com/xiaomi-banner-new.jpg",
    "website": "https://mi.com",
    "email": "support@mi.com",
    "phone": "+86-400-100-5678",
    "address": "Xiaomi Campus, Beijing, China",
    "isActive": true,
    "isFeatured": true,
    "productCount": 0,
    "displayOrder": 5,
    "metaTitle": "Xiaomi - Công nghệ thông minh giá tốt",
    "metaDescription": "Khám phá sản phẩm Xiaomi chính hãng với công nghệ tiên tiến",
    "createdAt": "2024-01-01T12:00:00Z",
    "updatedAt": "2024-01-01T12:30:00Z"
  },
  "timestamp": "2024-01-01T12:30:00Z"
}
```

#### 7. Xóa thương hiệu (Staff Only)
```http
DELETE /api/brand/{id}
Authorization: Bearer staff-token
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Xóa thương hiệu thành công",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

**Response Error (409) - Có sản phẩm liên kết:**
```json
{
  "success": false,
  "message": "Không thể xóa thương hiệu đang có sản phẩm",
  "errors": ["Thương hiệu này có 25 sản phẩm đang hoạt động"],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 8. Cập nhật trạng thái thương hiệu (Staff Only)
```http
PATCH /api/brand/{id}/status
Authorization: Bearer staff-token
```

**Request Body:**
```json
{
  "isActive": false,
  "reason": "Tạm ngừng hợp tác"
}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Cập nhật trạng thái thương hiệu thành công",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 9. Cập nhật thứ tự hiển thị (Staff Only)
```http
PATCH /api/brand/reorder
Authorization: Bearer staff-token
```

**Request Body:**
```json
{
  "brandOrders": [
    {
      "brandId": 1,
      "displayOrder": 1
    },
    {
      "brandId": 2,
      "displayOrder": 2
    },
    {
      "brandId": 3,
      "displayOrder": 3
    }
  ]
}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Cập nhật thứ tự hiển thị thành công",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## Data Definitions

### 🏢 Brand Fields

| Field | Type | Description | Required |
|-------|------|-------------|----------|
| `id` | int | ID duy nhất của thương hiệu | Auto |
| `name` | string | Tên thương hiệu | ✅ |
| `slug` | string | URL-friendly name | Auto |
| `description` | string | Mô tả thương hiệu | ❌ |
| `logoUrl` | string | Link logo thương hiệu | ❌ |
| `bannerUrl` | string | Link banner thương hiệu | ❌ |
| `website` | string | Website chính thức | ❌ |
| `email` | string | Email liên hệ | ❌ |
| `phone` | string | Số điện thoại | ❌ |
| `address` | string | Địa chỉ trụ sở | ❌ |
| `isActive` | boolean | Trạng thái hoạt động | ✅ |
| `isFeatured` | boolean | Thương hiệu nổi bật | ❌ |
| `productCount` | int | Số lượng sản phẩm | Auto |
| `displayOrder` | int | Thứ tự hiển thị | ❌ |
| `metaTitle` | string | SEO title | ❌ |
| `metaDescription` | string | SEO description | ❌ |

## Validation Rules

### Brand Creation/Update
- `name`: Bắt buộc, 2-100 ký tự, duy nhất
- `description`: Tối đa 1000 ký tự
- `logoUrl`: URL hợp lệ (nếu có)
- `bannerUrl`: URL hợp lệ (nếu có)
- `website`: URL hợp lệ (nếu có)
- `email`: Email hợp lệ (nếu có)
- `phone`: Số điện thoại hợp lệ (nếu có)
- `displayOrder`: Số nguyên dương
- `metaTitle`: Tối đa 60 ký tự
- `metaDescription`: Tối đa 160 ký tự

## Error Responses

### 400 Bad Request
```json
{
  "success": false,
  "message": "Dữ liệu không hợp lệ",
  "errors": [
    "Tên thương hiệu là bắt buộc",
    "URL logo không hợp lệ"
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

### 403 Forbidden
```json
{
  "success": false,
  "message": "Chỉ staff mới có quyền thực hiện thao tác này",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### 404 Not Found
```json
{
  "success": false,
  "message": "Không tìm thấy thương hiệu",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### 409 Conflict
```json
{
  "success": false,
  "message": "Tên thương hiệu đã tồn tại",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## Frontend Integration Examples

### Hiển thị danh sách thương hiệu
```javascript
const getBrands = async (page = 1, featured = false) => {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: '20'
  });
  
  if (featured) params.append('featured', 'true');
  
  const response = await fetch(`/api/brand?${params}`);
  const result = await response.json();
  return result.data;
};

// Get featured brands for homepage
const getFeaturedBrands = async () => {
  const response = await fetch('/api/brand/featured?limit=8');
  const result = await response.json();
  return result.data;
};
```

### Tìm kiếm thương hiệu
```javascript
const searchBrands = async (searchTerm) => {
  const params = new URLSearchParams({
    search: searchTerm,
    pageSize: '50'
  });
  
  const response = await fetch(`/api/brand?${params}`);
  const result = await response.json();
  return result.data.items;
};
```

### Quản lý thương hiệu (Staff)
```javascript
// Create brand
const createBrand = async (brandData) => {
  const response = await fetch('/api/brand', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${staffToken}`
    },
    body: JSON.stringify(brandData)
  });
  return await response.json();
};

// Update brand
const updateBrand = async (brandId, brandData) => {
  const response = await fetch(`/api/brand/${brandId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${staffToken}`
    },
    body: JSON.stringify(brandData)
  });
  return await response.json();
};

// Delete brand
const deleteBrand = async (brandId) => {
  const response = await fetch(`/api/brand/${brandId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${staffToken}`
    }
  });
  return await response.json();
};
```

## Testing

Test các endpoints với file HTTP:
```http
### Get all brands
GET {{baseUrl}}/api/brand

### Get featured brands
GET {{baseUrl}}/api/brand/featured?limit=5

### Get brand by ID
GET {{baseUrl}}/api/brand/1

### Get products by brand
GET {{baseUrl}}/api/brand/1/products?page=1&pageSize=10

### Create brand (Staff)
POST {{baseUrl}}/api/brand
Authorization: Bearer {{staffToken}}
Content-Type: application/json

{
  "name": "Test Brand",
  "description": "Test brand description",
  "isActive": true,
  "isFeatured": false
}

### Update brand (Staff)
PUT {{baseUrl}}/api/brand/1
Authorization: Bearer {{staffToken}}
Content-Type: application/json

{
  "name": "Updated Brand Name",
  "description": "Updated description",
  "isActive": true,
  "isFeatured": true
}

### Delete brand (Staff)
DELETE {{baseUrl}}/api/brand/1
Authorization: Bearer {{staffToken}}
```

## SEO & Performance

### 🔍 SEO Features
- **Friendly URLs**: Sử dụng slug thay vì ID
- **Meta Tags**: metaTitle và metaDescription
- **Structured Data**: Schema.org markup cho thương hiệu
- **Sitemap**: Auto-generate XML sitemap

### ⚡ Performance Optimization
- **Caching**: Redis cache cho thương hiệu phổ biến
- **CDN**: Deliver logo/banner qua CDN
- **Lazy Loading**: Load images on demand
- **Compression**: Gzip/Brotli compression

## Business Logic

### 📊 Analytics & Tracking
- Track số lượng view cho mỗi thương hiệu
- Monitor conversion rate theo thương hiệu
- A/B testing cho featured brands
- Heatmap cho brand pages

### 🎯 Marketing Integration
- **Brand Partnerships**: Theo dõi performance từng đối tác
- **Seasonal Promotions**: Highlight brands theo mùa
- **Cross-selling**: Suggest related brands
- **Email Campaigns**: Personalized brand recommendations

## Summary

### ✅ Hoàn thành:
- **CRUD Operations**: Tạo, đọc, cập nhật, xóa thương hiệu
- **Public APIs**: Danh sách thương hiệu, featured brands
- **Product Integration**: Lấy sản phẩm theo thương hiệu
- **Search & Filter**: Tìm kiếm và lọc thương hiệu
- **Staff Management**: Quản lý thương hiệu cho nhân viên
- **Data Validation**: Validate đầy đủ input data
- **Error Handling**: Xử lý lỗi toàn diện
- **SEO Ready**: Meta tags và friendly URLs

### 🔄 Có thể mở rộng:
- **Brand Analytics Dashboard**: Thống kê chi tiết
- **Brand Comparison**: So sánh thương hiệu
- **Brand Reviews**: Đánh giá thương hiệu
- **Brand News**: Tin tức về thương hiệu
- **Brand Social Media**: Tích hợp social feeds

Hệ thống Brand Management đã hoàn thiện và ready để phục vụ một platform e-commerce chuyên nghiệp!