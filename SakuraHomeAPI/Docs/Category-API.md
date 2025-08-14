# SakuraHome API - Category Management

## Tổng quan

Hệ thống quản lý danh mục SakuraHome cung cấp các chức năng quản lý danh mục sản phẩm có cấu trúc phân cấp, bao gồm tạo, xem, cập nhật và xóa danh mục.

## Base URL
```
https://localhost:7240/api/category
```

## Authentication
- Endpoints công khai: Không cần authentication
- Endpoints quản lý (Staff/Admin): Yêu cầu JWT Bearer token
```
Authorization: Bearer your-jwt-token
```

## Endpoints

### 📖 Public Endpoints

#### 1. Lấy danh sách danh mục
```http
GET /api/category
```

**Query Parameters:**
- `page` (int, optional): Trang hiện tại (default: 1)
- `pageSize` (int, optional): Số item mỗi trang (default: 20)
- `search` (string, optional): Tìm kiếm theo tên danh mục
- `parentId` (int, optional): Lọc theo danh mục cha
- `level` (int, optional): Lọc theo cấp độ (0: root, 1: level 1, etc.)
- `isActive` (bool, optional): Lọc theo trạng thái hoạt động
- `featured` (bool, optional): Lọc danh mục nổi bật
- `includeProducts` (bool, optional): Bao gồm thông tin sản phẩm

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy danh sách danh mục thành công",
  "data": {
    "items": [
      {
        "id": 1,
        "name": "Điện thoại & Phụ kiện",
        "slug": "dien-thoai-phu-kien",
        "description": "Điện thoại thông minh và phụ kiện",
        "imageUrl": "https://example.com/category/phones.jpg",
        "iconUrl": "https://example.com/icons/phone.svg",
        "parentId": null,
        "level": 0,
        "path": "1",
        "isActive": true,
        "isFeatured": true,
        "productCount": 245,
        "directProductCount": 0,
        "displayOrder": 1,
        "metaTitle": "Điện thoại thông minh chính hãng",
        "metaDescription": "Mua điện thoại và phụ kiện chính hãng với giá tốt nhất",
        "children": [
          {
            "id": 11,
            "name": "iPhone",
            "slug": "iphone",
            "description": "Điện thoại iPhone chính hãng",
            "imageUrl": "https://example.com/category/iphone.jpg",
            "parentId": 1,
            "level": 1,
            "path": "1.11",
            "isActive": true,
            "isFeatured": true,
            "productCount": 45,
            "directProductCount": 45,
            "displayOrder": 1
          },
          {
            "id": 12,
            "name": "Samsung Galaxy",
            "slug": "samsung-galaxy",
            "description": "Điện thoại Samsung Galaxy",
            "imageUrl": "https://example.com/category/samsung.jpg",
            "parentId": 1,
            "level": 1,
            "path": "1.12",
            "isActive": true,
            "isFeatured": true,
            "productCount": 67,
            "directProductCount": 67,
            "displayOrder": 2
          }
        ],
        "attributes": [
          {
            "id": 1,
            "name": "Thương hiệu",
            "type": "Select",
            "isRequired": true,
            "options": ["Apple", "Samsung", "Xiaomi"]
          },
          {
            "id": 2,
            "name": "Dung lượng",
            "type": "Select",
            "isRequired": false,
            "options": ["64GB", "128GB", "256GB", "512GB"]
          }
        ],
        "createdAt": "2023-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "pagination": {
      "currentPage": 1,
      "totalPages": 3,
      "totalCount": 50,
      "pageSize": 20,
      "hasNext": true,
      "hasPrevious": false
    }
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 2. Lấy danh mục theo ID
```http
GET /api/category/{id}
```

**Query Parameters:**
- `includeChildren` (bool, optional): Bao gồm danh mục con (default: true)
- `includeProducts` (bool, optional): Bao gồm sản phẩm (default: false)
- `childrenLevels` (int, optional): Số cấp con cần lấy (default: 1)

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy thông tin danh mục thành công",
  "data": {
    "id": 1,
    "name": "Điện thoại & Phụ kiện",
    "slug": "dien-thoai-phu-kien",
    "description": "Danh mục chuyên về điện thoại thông minh và các phụ kiện đi kèm. Bao gồm điện thoại từ các thương hiệu hàng đầu như Apple, Samsung, Xiaomi, Oppo và nhiều thương hiệu khác.",
    "imageUrl": "https://example.com/category/phones.jpg",
    "bannerUrl": "https://example.com/category/phones-banner.jpg",
    "iconUrl": "https://example.com/icons/phone.svg",
    "parentId": null,
    "level": 0,
    "path": "1",
    "fullPath": "Điện thoại & Phụ kiện",
    "isActive": true,
    "isFeatured": true,
    "productCount": 245,
    "directProductCount": 0,
    "displayOrder": 1,
    "metaTitle": "Điện thoại thông minh chính hãng giá tốt",
    "metaDescription": "Mua điện thoại và phụ kiện chính hãng với giá tốt nhất. Bảo hành chính hãng, giao hàng nhanh toàn quốc.",
    "parent": null,
    "children": [
      {
        "id": 11,
        "name": "iPhone",
        "slug": "iphone",
        "description": "Điện thoại iPhone chính hãng từ Apple",
        "imageUrl": "https://example.com/category/iphone.jpg",
        "parentId": 1,
        "level": 1,
        "path": "1.11",
        "fullPath": "Điện thoại & Phụ kiện > iPhone",
        "isActive": true,
        "isFeatured": true,
        "productCount": 45,
        "directProductCount": 45,
        "displayOrder": 1,
        "children": [
          {
            "id": 111,
            "name": "iPhone 15 Series",
            "slug": "iphone-15-series",
            "parentId": 11,
            "level": 2,
            "productCount": 12,
            "displayOrder": 1
          }
        ]
      }
    ],
    "breadcrumb": [
      {
        "id": 1,
        "name": "Điện thoại & Phụ kiện",
        "slug": "dien-thoai-phu-kien",
        "level": 0
      }
    ],
    "attributes": [
      {
        "id": 1,
        "name": "Thương hiệu",
        "type": "Select",
        "isRequired": true,
        "displayOrder": 1,
        "options": [
          {
            "value": "apple",
            "label": "Apple",
            "productCount": 45
          },
          {
            "value": "samsung",
            "label": "Samsung",
            "productCount": 67
          }
        ]
      }
    ],
    "topProducts": [
      {
        "id": 101,
        "name": "iPhone 15 Pro Max",
        "sku": "IP15PM-256",
        "price": 34990000,
        "salePrice": 32990000,
        "imageUrl": "https://example.com/products/iphone-15-pro-max.jpg",
        "rating": 4.9,
        "reviewCount": 850,
        "isInStock": true
      }
    ],
    "relatedCategories": [
      {
        "id": 2,
        "name": "Laptop",
        "slug": "laptop",
        "imageUrl": "https://example.com/category/laptop.jpg"
      }
    ],
    "createdAt": "2023-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 3. Lấy danh mục gốc
```http
GET /api/category/root
```

**Query Parameters:**
- `includeChildren` (bool, optional): Bao gồm danh mục con (default: true)
- `childrenLevels` (int, optional): Số cấp con cần lấy (default: 2)

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy danh mục gốc thành công",
  "data": [
    {
      "id": 1,
      "name": "Điện thoại & Phụ kiện",
      "slug": "dien-thoai-phu-kien",
      "imageUrl": "https://example.com/category/phones.jpg",
      "iconUrl": "https://example.com/icons/phone.svg",
      "productCount": 245,
      "displayOrder": 1,
      "children": [
        {
          "id": 11,
          "name": "iPhone",
          "slug": "iphone",
          "productCount": 45,
          "children": [
            {
              "id": 111,
              "name": "iPhone 15 Series",
              "slug": "iphone-15-series",
              "productCount": 12
            }
          ]
        }
      ]
    },
    {
      "id": 2,
      "name": "Laptop & Máy tính",
      "slug": "laptop-may-tinh",
      "imageUrl": "https://example.com/category/laptop.jpg",
      "iconUrl": "https://example.com/icons/laptop.svg",
      "productCount": 156,
      "displayOrder": 2,
      "children": [
        {
          "id": 21,
          "name": "Laptop Gaming",
          "slug": "laptop-gaming",
          "productCount": 45
        },
        {
          "id": 22,
          "name": "Laptop Văn phòng",
          "slug": "laptop-van-phong",
          "productCount": 78
        }
      ]
    }
  ],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 4. Lấy tree danh mục hoàn chỉnh
```http
GET /api/category/tree
```

**Query Parameters:**
- `maxLevels` (int, optional): Số cấp tối đa (default: 3)
- `activeOnly` (bool, optional): Chỉ lấy danh mục hoạt động (default: true)

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy cây danh mục thành công",
  "data": [
    {
      "id": 1,
      "name": "Điện thoại & Phụ kiện",
      "slug": "dien-thoai-phu-kien",
      "level": 0,
      "productCount": 245,
      "children": [
        {
          "id": 11,
          "name": "iPhone",
          "slug": "iphone",
          "level": 1,
          "productCount": 45,
          "children": [
            {
              "id": 111,
              "name": "iPhone 15 Series",
              "slug": "iphone-15-series",
              "level": 2,
              "productCount": 12,
              "children": []
            },
            {
              "id": 112,
              "name": "iPhone 14 Series",
              "slug": "iphone-14-series",
              "level": 2,
              "productCount": 20,
              "children": []
            }
          ]
        },
        {
          "id": 12,
          "name": "Samsung Galaxy",
          "slug": "samsung-galaxy",
          "level": 1,
          "productCount": 67,
          "children": []
        }
      ]
    }
  ],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 5. Lấy sản phẩm theo danh mục
```http
GET /api/category/{id}/products
```

**Query Parameters:**
- `page` (int, optional): Trang hiện tại (default: 1)
- `pageSize` (int, optional): Số item mỗi trang (default: 20)
- `brandId` (int, optional): Lọc theo thương hiệu
- `priceMin` (decimal, optional): Giá tối thiểu
- `priceMax` (decimal, optional): Giá tối đa
- `rating` (int, optional): Điểm đánh giá tối thiểu
- `sortBy` (string, optional): Sắp xếp (name, price, newest, popular, rating)
- `sortOrder` (string, optional): Thứ tự (asc, desc)
- `includeSubcategories` (bool, optional): Bao gồm sản phẩm từ danh mục con

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy sản phẩm theo danh mục thành công",
  "data": {
    "category": {
      "id": 1,
      "name": "Điện thoại & Phụ kiện",
      "slug": "dien-thoai-phu-kien",
      "breadcrumb": [
        {
          "id": 1,
          "name": "Điện thoại & Phụ kiện",
          "slug": "dien-thoai-phu-kien"
        }
      ]
    },
    "filters": {
      "brands": [
        {
          "id": 1,
          "name": "Apple",
          "productCount": 45
        },
        {
          "id": 2,
          "name": "Samsung",
          "productCount": 67
        }
      ],
      "priceRanges": [
        {
          "label": "Dưới 5 triệu",
          "min": 0,
          "max": 5000000,
          "productCount": 89
        },
        {
          "label": "5-10 triệu",
          "min": 5000000,
          "max": 10000000,
          "productCount": 95
        }
      ],
      "attributes": [
        {
          "name": "Dung lượng",
          "options": [
            {
              "value": "128gb",
              "label": "128GB",
              "productCount": 67
            }
          ]
        }
      ]
    },
    "products": {
      "items": [
        {
          "id": 101,
          "name": "iPhone 15 Pro Max",
          "sku": "IP15PM-256",
          "slug": "iphone-15-pro-max",
          "price": 34990000,
          "salePrice": 32990000,
          "discountPercent": 6,
          "imageUrl": "https://example.com/products/iphone-15-pro-max.jpg",
          "rating": 4.9,
          "reviewCount": 850,
          "isInStock": true,
          "stockQuantity": 25,
          "brand": {
            "id": 1,
            "name": "Apple"
          },
          "category": {
            "id": 11,
            "name": "iPhone"
          },
          "tags": ["flagship", "camera", "5g"]
        }
      ],
      "pagination": {
        "currentPage": 1,
        "totalPages": 12,
        "totalCount": 245,
        "pageSize": 20,
        "hasNext": true,
        "hasPrevious": false
      }
    }
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 6. Tìm kiếm danh mục
```http
GET /api/category/search
```

**Query Parameters:**
- `q` (string, required): Từ khóa tìm kiếm
- `limit` (int, optional): Số kết quả tối đa (default: 10)

**Response Success (200):**
```json
{
  "success": true,
  "message": "Tìm kiếm danh mục thành công",
  "data": [
    {
      "id": 11,
      "name": "iPhone",
      "slug": "iphone",
      "fullPath": "Điện thoại & Phụ kiện > iPhone",
      "productCount": 45,
      "imageUrl": "https://example.com/category/iphone.jpg"
    },
    {
      "id": 111,
      "name": "iPhone 15 Series",
      "slug": "iphone-15-series",
      "fullPath": "Điện thoại & Phụ kiện > iPhone > iPhone 15 Series",
      "productCount": 12,
      "imageUrl": "https://example.com/category/iphone-15.jpg"
    }
  ],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### 👨‍💼 Staff & Admin Endpoints

#### 7. Tạo danh mục mới (Staff Only)
```http
POST /api/category
Authorization: Bearer staff-token
```

**Request Body:**
```json
{
  "name": "Tai nghe & Audio",
  "description": "Tai nghe, loa và thiết bị âm thanh chất lượng cao",
  "imageUrl": "https://example.com/category/audio.jpg",
  "bannerUrl": "https://example.com/category/audio-banner.jpg",
  "iconUrl": "https://example.com/icons/headphones.svg",
  "parentId": 1,
  "isActive": true,
  "isFeatured": true,
  "displayOrder": 10,
  "metaTitle": "Tai nghe và thiết bị âm thanh chính hãng",
  "metaDescription": "Mua tai nghe, loa và thiết bị âm thanh chính hãng với giá tốt nhất",
  "attributes": [
    {
      "name": "Loại kết nối",
      "type": "Select",
      "isRequired": true,
      "options": ["Có dây", "Bluetooth", "USB-C"]
    },
    {
      "name": "Tần số đáp ứng",
      "type": "Text",
      "isRequired": false
    }
  ]
}
```

**Response Success (201):**
```json
{
  "success": true,
  "message": "Tạo danh mục thành công",
  "data": {
    "id": 15,
    "name": "Tai nghe & Audio",
    "slug": "tai-nghe-audio",
    "description": "Tai nghe, loa và thiết bị âm thanh chất lượng cao",
    "imageUrl": "https://example.com/category/audio.jpg",
    "bannerUrl": "https://example.com/category/audio-banner.jpg",
    "iconUrl": "https://example.com/icons/headphones.svg",
    "parentId": 1,
    "level": 1,
    "path": "1.15",
    "fullPath": "Điện thoại & Phụ kiện > Tai nghe & Audio",
    "isActive": true,
    "isFeatured": true,
    "productCount": 0,
    "directProductCount": 0,
    "displayOrder": 10,
    "metaTitle": "Tai nghe và thiết bị âm thanh chính hãng",
    "metaDescription": "Mua tai nghe, loa và thiết bị âm thanh chính hãng với giá tốt nhất",
    "createdAt": "2024-01-01T12:00:00Z",
    "updatedAt": "2024-01-01T12:00:00Z"
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 8. Cập nhật danh mục (Staff Only)
```http
PUT /api/category/{id}
Authorization: Bearer staff-token
```

**Request Body:**
```json
{
  "name": "Tai nghe & Audio (Updated)",
  "description": "Tai nghe, loa và thiết bị âm thanh cao cấp - cập nhật",
  "imageUrl": "https://example.com/category/audio-new.jpg",
  "bannerUrl": "https://example.com/category/audio-banner-new.jpg",
  "iconUrl": "https://example.com/icons/headphones-new.svg",
  "parentId": 1,
  "isActive": true,
  "isFeatured": false,
  "displayOrder": 5,
  "metaTitle": "Tai nghe cao cấp chính hãng",
  "metaDescription": "Khám phá bộ sưu tập tai nghe và thiết bị âm thanh cao cấp"
}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Cập nhật danh mục thành công",
  "data": {
    "id": 15,
    "name": "Tai nghe & Audio (Updated)",
    "slug": "tai-nghe-audio-updated",
    "description": "Tai nghe, loa và thiết bị âm thanh cao cấp - cập nhật",
    "imageUrl": "https://example.com/category/audio-new.jpg",
    "bannerUrl": "https://example.com/category/audio-banner-new.jpg",
    "iconUrl": "https://example.com/icons/headphones-new.svg",
    "parentId": 1,
    "level": 1,
    "path": "1.15",
    "fullPath": "Điện thoại & Phụ kiện > Tai nghe & Audio (Updated)",
    "isActive": true,
    "isFeatured": false,
    "productCount": 0,
    "directProductCount": 0,
    "displayOrder": 5,
    "metaTitle": "Tai nghe cao cấp chính hãng",
    "metaDescription": "Khám phá bộ sưu tập tai nghe và thiết bị âm thanh cao cấp",
    "createdAt": "2024-01-01T12:00:00Z",
    "updatedAt": "2024-01-01T12:30:00Z"
  },
  "timestamp": "2024-01-01T12:30:00Z"
}
```

#### 9. Xóa danh mục (Staff Only)
```http
DELETE /api/category/{id}
Authorization: Bearer staff-token
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Xóa danh mục thành công",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

**Response Error (409) - Có sản phẩm hoặc danh mục con:**
```json
{
  "success": false,
  "message": "Không thể xóa danh mục",
  "errors": [
    "Danh mục này có 3 danh mục con",
    "Danh mục này có 25 sản phẩm"
  ],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 10. Di chuyển danh mục (Staff Only)
```http
PATCH /api/category/{id}/move
Authorization: Bearer staff-token
```

**Request Body:**
```json
{
  "newParentId": 2,
  "newDisplayOrder": 5
}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Di chuyển danh mục thành công",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 11. Cập nhật thứ tự hiển thị (Staff Only)
```http
PATCH /api/category/reorder
Authorization: Bearer staff-token
```

**Request Body:**
```json
{
  "categoryOrders": [
    {
      "categoryId": 1,
      "displayOrder": 1
    },
    {
      "categoryId": 2,
      "displayOrder": 2
    },
    {
      "categoryId": 3,
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

### 📁 Category Fields

| Field | Type | Description | Required |
|-------|------|-------------|----------|
| `id` | int | ID duy nhất của danh mục | Auto |
| `name` | string | Tên danh mục | ✅ |
| `slug` | string | URL-friendly name | Auto |
| `description` | string | Mô tả danh mục | ❌ |
| `imageUrl` | string | Hình ảnh danh mục | ❌ |
| `bannerUrl` | string | Banner danh mục | ❌ |
| `iconUrl` | string | Icon danh mục | ❌ |
| `parentId` | int | ID danh mục cha | ❌ |
| `level` | int | Cấp độ trong cây danh mục | Auto |
| `path` | string | Đường dẫn trong cây (1.2.3) | Auto |
| `fullPath` | string | Đường dẫn đầy đủ | Auto |
| `isActive` | boolean | Trạng thái hoạt động | ✅ |
| `isFeatured` | boolean | Danh mục nổi bật | ❌ |
| `productCount` | int | Tổng số sản phẩm (bao gồm con) | Auto |
| `directProductCount` | int | Số sản phẩm trực tiếp | Auto |
| `displayOrder` | int | Thứ tự hiển thị | ❌ |
| `metaTitle` | string | SEO title | ❌ |
| `metaDescription` | string | SEO description | ❌ |

### 🔧 Category Attribute Fields

| Field | Type | Description |
|-------|------|-------------|
| `id` | int | ID thuộc tính |
| `name` | string | Tên thuộc tính |
| `type` | string | Loại (Text, Select, Number, Boolean) |
| `isRequired` | boolean | Bắt buộc hay không |
| `displayOrder` | int | Thứ tự hiển thị |
| `options` | array | Các tùy chọn (cho type Select) |

## Validation Rules

### Category Creation/Update
- `name`: Bắt buộc, 2-100 ký tự, duy nhất trong cùng parent
- `description`: Tối đa 1000 ký tự
- `imageUrl`: URL hợp lệ (nếu có)
- `bannerUrl`: URL hợp lệ (nếu có)
- `iconUrl`: URL hợp lệ (nếu có)
- `parentId`: Phải tồn tại (nếu có)
- `displayOrder`: Số nguyên dương
- `metaTitle`: Tối đa 60 ký tự
- `metaDescription`: Tối đa 160 ký tự

### Business Rules
- Không được tạo vòng lặp trong cây danh mục
- Cấp độ tối đa: 5 levels
- Không được xóa danh mục có con hoặc có sản phẩm
- Khi di chuyển danh mục, kiểm tra ràng buộc cây

## Error Responses

### 400 Bad Request
```json
{
  "success": false,
  "message": "Dữ liệu không hợp lệ",
  "errors": [
    "Tên danh mục là bắt buộc",
    "Danh mục cha không tồn tại"
  ],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### 409 Conflict
```json
{
  "success": false,
  "message": "Không thể thực hiện thao tác",
  "errors": [
    "Tên danh mục đã tồn tại",
    "Thao tác này sẽ tạo vòng lặp trong cây danh mục"
  ],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## Frontend Integration Examples

### Hiển thị menu danh mục
```javascript
const getCategoryTree = async (maxLevels = 3) => {
  const response = await fetch(`/api/category/tree?maxLevels=${maxLevels}`);
  const result = await response.json();
  return result.data;
};

// Render category menu
const renderCategoryMenu = (categories) => {
  return categories.map(category => `
    <li class="category-item">
      <a href="/category/${category.slug}">${category.name}</a>
      ${category.children.length > 0 ? `
        <ul class="subcategory-list">
          ${renderCategoryMenu(category.children)}
        </ul>
      ` : ''}
    </li>
  `).join('');
};
```

### Breadcrumb navigation
```javascript
const getCategoryWithBreadcrumb = async (categoryId) => {
  const response = await fetch(`/api/category/${categoryId}`);
  const result = await response.json();
  return result.data;
};

// Render breadcrumb
const renderBreadcrumb = (breadcrumb) => {
  return breadcrumb.map(item => `
    <span class="breadcrumb-item">
      <a href="/category/${item.slug}">${item.name}</a>
    </span>
  `).join(' > ');
};
```

### Category filter
```javascript
const getCategoryProducts = async (categoryId, filters = {}) => {
  const params = new URLSearchParams({
    page: filters.page || 1,
    pageSize: filters.pageSize || 20,
    ...filters
  });
  
  const response = await fetch(`/api/category/${categoryId}/products?${params}`);
  const result = await response.json();
  return result.data;
};

// Apply filters
const applyFilters = async (categoryId, filters) => {
  const data = await getCategoryProducts(categoryId, filters);
  renderProducts(data.products.items);
  renderFilters(data.filters);
  renderPagination(data.products.pagination);
};
```

### Category management (Staff)
```javascript
// Create category
const createCategory = async (categoryData) => {
  const response = await fetch('/api/category', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${staffToken}`
    },
    body: JSON.stringify(categoryData)
  });
  return await response.json();
};

// Move category
const moveCategory = async (categoryId, newParentId, newDisplayOrder) => {
  const response = await fetch(`/api/category/${categoryId}/move`, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${staffToken}`
    },
    body: JSON.stringify({
      newParentId,
      newDisplayOrder
    })
  });
  return await response.json();
};
```

## Testing

Test các endpoints với file HTTP:
```http
### Get category tree
GET {{baseUrl}}/api/category/tree?maxLevels=3

### Get root categories
GET {{baseUrl}}/api/category/root

### Get category by ID
GET {{baseUrl}}/api/category/1?includeChildren=true

### Get category products
GET {{baseUrl}}/api/category/1/products?page=1&sortBy=price&sortOrder=asc

### Search categories
GET {{baseUrl}}/api/category/search?q=phone

### Create category (Staff)
POST {{baseUrl}}/api/category
Authorization: Bearer {{staffToken}}
Content-Type: application/json

{
  "name": "Test Category",
  "description": "Test category description",
  "parentId": 1,
  "isActive": true,
  "isFeatured": false
}

### Update category (Staff)
PUT {{baseUrl}}/api/category/15
Authorization: Bearer {{staffToken}}
Content-Type: application/json

{
  "name": "Updated Category",
  "description": "Updated description",
  "isActive": true
}

### Move category (Staff)
PATCH {{baseUrl}}/api/category/15/move
Authorization: Bearer {{staffToken}}
Content-Type: application/json

{
  "newParentId": 2,
  "newDisplayOrder": 10
}

### Delete category (Staff)
DELETE {{baseUrl}}/api/category/15
Authorization: Bearer {{staffToken}}
```

## Advanced Features

### 🔍 SEO & Performance
- **Friendly URLs**: Sử dụng slug thay vì ID
- **Meta Tags**: SEO optimization cho từng danh mục
- **Structured Data**: Schema.org markup
- **Sitemap**: Auto-generate XML sitemap
- **Caching**: Redis cache cho category tree
- **CDN**: Optimize images và static assets

### 📊 Analytics & Insights
- **Category Performance**: Track views, conversions
- **Product Distribution**: Phân tích sản phẩm theo danh mục
- **User Behavior**: Heatmap, scroll tracking
- **A/B Testing**: Test different category layouts

### 🎯 Personalization
- **Recommended Categories**: Gợi ý dựa trên lịch sử
- **Trending Categories**: Danh mục hot theo thời gian thực
- **Seasonal Promotions**: Highlight categories theo mùa
- **User Preferences**: Cá nhân hóa thứ tự hiển thị

## Business Logic

### 📈 Category Intelligence
- **Auto-tagging**: Tự động gán tags cho sản phẩm
- **Smart Categorization**: AI-powered category suggestions
- **Inventory Optimization**: Phân bổ sản phẩm tối ưu
- **Price Strategy**: Giá cả theo từng danh mục

### 🔄 Category Lifecycle
- **Creation Workflow**: Quy trình tạo danh mục
- **Approval Process**: Duyệt danh mục mới
- **Performance Monitoring**: Theo dõi hiệu suất
- **Retirement Strategy**: Thu hồi danh mục không hiệu quả

## Summary

### ✅ Hoàn thành:
- **Hierarchical Structure**: Cấu trúc cây danh mục phân cấp
- **CRUD Operations**: Tạo, đọc, cập nhật, xóa danh mục
- **Tree Operations**: Lấy tree, root, di chuyển danh mục
- **Product Integration**: Lấy sản phẩm theo danh mục với filter
- **Search Functionality**: Tìm kiếm danh mục
- **Category Attributes**: Thuộc tính động cho danh mục
- **SEO Ready**: Meta tags và friendly URLs
- **Staff Management**: Quản lý danh mục cho nhân viên
- **Data Validation**: Validate đầy đủ cấu trúc cây
- **Error Handling**: Xử lý lỗi toàn diện

### 🔄 Có thể mở rộng:
- **Category Analytics Dashboard**: Thống kê chi tiết
- **AI-powered Categorization**: Tự động phân loại
- **Multi-language Categories**: Đa ngôn ngữ
- **Category Recommendations**: Gợi ý danh mục thông minh
- **Advanced Filtering**: Bộ lọc nâng cao với AI

Hệ thống Category Management đã hoàn thiện với cấu trúc phân cấp mạnh mẽ và ready cho một platform e-commerce chuyên nghiệp!