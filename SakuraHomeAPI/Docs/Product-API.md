# ??? Product API Documentation

## T?ng quan

API Product c?a SakuraHome cung c?p ??y ?? các ch?c n?ng qu?n lý s?n ph?m, bao g?m tìm ki?m, l?c, phân trang, và qu?n lý chi ti?t s?n ph?m.

**Base URL:** `https://localhost:7240/api/product`

## ?? M?c l?c

- [L?y danh sách s?n ph?m](#l?y-danh-sách-s?n-ph?m)
- [L?y chi ti?t s?n ph?m](#l?y-chi-ti?t-s?n-ph?m)
- [Tìm s?n ph?m theo SKU](#tìm-s?n-ph?m-theo-sku)
- [T?o s?n ph?m m?i](#t?o-s?n-ph?m-m?i)
- [C?p nh?t s?n ph?m](#c?p-nh?t-s?n-ph?m)
- [Xóa s?n ph?m](#xóa-s?n-ph?m)
- [C?p nh?t t?n kho](#c?p-nh?t-t?n-kho)
- [Debug endpoint](#debug-endpoint)

---

## ?? L?y danh sách s?n ph?m

### GET `/`

L?y danh sách s?n ph?m v?i tính n?ng l?c, tìm ki?m và phân trang.

#### Query Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | integer | 1 | Trang hi?n t?i |
| `pageSize` | integer | 20 | S? s?n ph?m trên m?i trang (max: 100) |
| `search` | string | - | T? khóa tìm ki?m |
| `categoryId` | integer | - | ID danh m?c |
| `brandId` | integer | - | ID th??ng hi?u |
| `minPrice` | decimal | - | Giá t?i thi?u |
| `maxPrice` | decimal | - | Giá t?i ?a |
| `inStockOnly` | boolean | false | Ch? s?n ph?m còn hàng |
| `onSaleOnly` | boolean | false | Ch? s?n ph?m gi?m giá |
| `featuredOnly` | boolean | false | Ch? s?n ph?m n?i b?t |
| `newOnly` | boolean | false | Ch? s?n ph?m m?i |
| `sortBy` | string | "created" | S?p x?p theo (name, price, rating, sold, views, stock, created) |
| `sortOrder` | string | "desc" | Th? t? s?p x?p (asc, desc) |

#### Example Request

```
GET /api/product?page=1&pageSize=12&search=sake&categoryId=1&minPrice=100000&maxPrice=500000&inStockOnly=true&sortBy=price&sortOrder=asc
```

#### Response Success (200)

```json
{
  "data": [
    {
      "id": 1,
      "name": "Sake Dassai 39 Junmai Daiginjo",
      "slug": "sake-dassai-39-junmai-daiginjo",
      "shortDescription": "Sake cao c?p t? Nh?t B?n v?i h??ng v? tinh t?",
      "mainImage": "https://example.com/images/dassai-39.jpg",
      "price": 350000.00,
      "originalPrice": 420000.00,
      "stock": 15,
      "status": "Available",
      "condition": "New",
      "rating": 4.8,
      "reviewCount": 24,
      "viewCount": 1250,
      "soldCount": 89,
      "isInStock": true,
      "isOnSale": true,
      "isFeatured": true,
      "isNew": false,
      "isBestseller": true,
      "isLimitedEdition": false,
      "isGiftWrappingAvailable": true,
      "allowBackorder": false,
      "allowPreorder": false,
      "createdAt": "2024-01-01T10:00:00Z",
      "updatedAt": "2024-01-15T14:30:00Z",
      "brand": {
        "id": 1,
        "name": "Dassai",
        "slug": "dassai",
        "logoUrl": "https://example.com/brands/dassai-logo.jpg",
        "isActive": true
      },
      "category": {
        "id": 1,
        "name": "Sake",
        "slug": "sake",
        "imageUrl": "https://example.com/categories/sake.jpg",
        "isActive": true,
        "parentId": null
      }
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 12,
    "totalItems": 156,
    "totalPages": 13,
    "hasNext": true,
    "hasPrevious": false
  },
  "success": true,
  "message": "Products retrieved successfully"
}
```

#### Frontend Integration

```javascript
// React/JavaScript example
const fetchProducts = async (filters = {}) => {
  try {
    const queryParams = new URLSearchParams();
    
    // Add filters to query params
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        queryParams.append(key, value);
      }
    });
    
    const response = await fetch(`${API_BASE_URL}/api/product?${queryParams}`);
    const result = await response.json();
    
    if (result.success) {
      return {
        products: result.data,
        pagination: result.pagination
      };
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Fetch products error:', error);
    throw error;
  }
};

// Usage example
const ProductList = () => {
  const [products, setProducts] = useState([]);
  const [pagination, setPagination] = useState({});
  const [filters, setFilters] = useState({
    page: 1,
    pageSize: 12,
    search: '',
    categoryId: null,
    sortBy: 'created',
    sortOrder: 'desc'
  });

  useEffect(() => {
    const loadProducts = async () => {
      try {
        const { products, pagination } = await fetchProducts(filters);
        setProducts(products);
        setPagination(pagination);
      } catch (error) {
        showError('Không th? t?i danh sách s?n ph?m');
      }
    };

    loadProducts();
  }, [filters]);

  return (
    <div>
      {/* Search and filters */}
      <ProductFilters 
        filters={filters} 
        onFiltersChange={setFilters} 
      />
      
      {/* Product grid */}
      <div className="product-grid">
        {products.map(product => (
          <ProductCard key={product.id} product={product} />
        ))}
      </div>
      
      {/* Pagination */}
      <Pagination 
        pagination={pagination}
        onPageChange={(page) => setFilters(prev => ({ ...prev, page }))}
      />
    </div>
  );
};
```

---

## ?? L?y chi ti?t s?n ph?m

### GET `/{id}`

L?y thông tin chi ti?t c?a m?t s?n ph?m c? th?.

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | integer | ID c?a s?n ph?m |

#### Response Success (200)

```json
{
  "success": true,
  "message": "Product retrieved successfully",
  "data": {
    "id": 1,
    "name": "Sake Dassai 39 Junmai Daiginjo",
    "slug": "sake-dassai-39-junmai-daiginjo",
    "shortDescription": "Sake cao c?p t? Nh?t B?n v?i h??ng v? tinh t?",
    "description": "Dassai 39 Junmai Daiginjo là m?t lo?i sake cao c?p ???c s?n xu?t t? g?o Yamada Nishiki ???c ?ánh bóng ??n 39% lõi g?o. V?i h??ng v? m?m m?i, thanh thoát và ?? tinh khi?t cao, ?ây là l?a ch?n hoàn h?o cho nh?ng ng??i yêu thích sake ch?t l??ng.",
    "mainImage": "https://example.com/images/dassai-39-main.jpg",
    "price": 350000.00,
    "originalPrice": 420000.00,
    "stock": 15,
    "status": "Available",
    "condition": "New",
    "rating": 4.8,
    "reviewCount": 24,
    "viewCount": 1251,
    "soldCount": 89,
    "isInStock": true,
    "isOnSale": true,
    "isFeatured": true,
    "isNew": false,
    "isBestseller": true,
    "isLimitedEdition": false,
    "isGiftWrappingAvailable": true,
    "allowBackorder": false,
    "allowPreorder": false,
    "createdAt": "2024-01-01T10:00:00Z",
    "updatedAt": "2024-01-15T14:30:00Z",
    "origin": "Japan",
    "japaneseRegion": "Yamaguchi",
    "authenticityLevel": "Official",
    "weight": 720.0,
    "weightUnit": "ml",
    "length": 6.5,
    "width": 6.5,
    "height": 30.0,
    "dimensionUnit": "cm",
    "trackInventory": true,
    "brand": {
      "id": 1,
      "name": "Dassai",
      "slug": "dassai",
      "logoUrl": "https://example.com/brands/dassai-logo.jpg",
      "isActive": true
    },
    "category": {
      "id": 1,
      "name": "Sake",
      "slug": "sake",
      "imageUrl": "https://example.com/categories/sake.jpg",
      "isActive": true,
      "parentId": null
    }
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
```

#### Frontend Integration

```javascript
const fetchProductDetail = async (productId) => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/product/${productId}`);
    const result = await response.json();
    
    if (result.success) {
      return result.data;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Fetch product detail error:', error);
    throw error;
  }
};

// React component example
const ProductDetail = ({ productId }) => {
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadProduct = async () => {
      try {
        setLoading(true);
        const productData = await fetchProductDetail(productId);
        setProduct(productData);
      } catch (error) {
        showError('Không th? t?i thông tin s?n ph?m');
      } finally {
        setLoading(false);
      }
    };

    loadProduct();
  }, [productId]);

  if (loading) return <ProductSkeleton />;
  if (!product) return <NotFound />;

  return (
    <div className="product-detail">
      <div className="product-images">
        <img src={product.mainImage} alt={product.name} />
      </div>
      
      <div className="product-info">
        <h1>{product.name}</h1>
        <p className="brand">{product.brand?.name}</p>
        
        <div className="price">
          <span className="current-price">
            {product.price.toLocaleString('vi-VN')}?
          </span>
          {product.originalPrice && (
            <span className="original-price">
              {product.originalPrice.toLocaleString('vi-VN')}?
            </span>
          )}
        </div>
        
        <div className="stock-status">
          {product.isInStock ? 'Còn hàng' : 'H?t hàng'}
          <span className="stock-count">({product.stock} s?n ph?m)</span>
        </div>
        
        <div className="rating">
          ? {product.rating} ({product.reviewCount} ?ánh giá)
        </div>
        
        <div className="description">
          <p>{product.description}</p>
        </div>
        
        <AddToCartButton product={product} />
      </div>
    </div>
  );
};
```

---

## ?? Tìm s?n ph?m theo SKU

### GET `/sku/{sku}`

Tìm s?n ph?m b?ng mã SKU.

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `sku` | string | Mã SKU c?a s?n ph?m |

#### Response Success (200)

```json
{
  "success": true,
  "message": "Product retrieved successfully",
  "data": {
    // Same structure as product detail
  }
}
```

#### Response Error (404)

```json
{
  "success": false,
  "message": "Product not found",
  "timestamp": "2024-01-15T15:00:00Z"
}
```

---

## ? T?o s?n ph?m m?i

### POST `/`

T?o s?n ph?m m?i (ch? dành cho Staff).

**Yêu c?u:** Bearer Token v?i role Staff

#### Request Body

```json
{
  "name": "Sake Dassai 23 Junmai Daiginjo",
  "shortDescription": "Sake siêu cao c?p v?i 23% lõi g?o",
  "description": "Dassai 23 là ??nh cao c?a ngh? thu?t làm sake...",
  "mainImage": "https://example.com/images/dassai-23.jpg",
  "price": 850000.00,
  "originalPrice": 950000.00,
  "stock": 5,
  "status": "Available",
  "condition": "New",
  "brandId": 1,
  "categoryId": 1,
  "sku": "DASSAI-23-720ML",
  "tags": "sake, premium, dassai, limited"
}
```

#### Response Success (201)

```json
{
  "success": true,
  "message": "Product created successfully",
  "data": {
    // Full product details
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
```

#### Frontend Integration (Admin Panel)

```javascript
const createProduct = async (productData) => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/product`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      },
      body: JSON.stringify(productData)
    });
    
    const result = await response.json();
    
    if (result.success) {
      showSuccess('T?o s?n ph?m thành công');
      return result.data;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Create product error:', error);
    showError('Có l?i x?y ra khi t?o s?n ph?m');
    throw error;
  }
};

// React form component
const CreateProductForm = () => {
  const [formData, setFormData] = useState({
    name: '',
    shortDescription: '',
    description: '',
    price: '',
    stock: '',
    brandId: '',
    categoryId: ''
  });

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    try {
      await createProduct(formData);
      navigate('/admin/products');
    } catch (error) {
      // Error already handled in createProduct
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <input
        type="text"
        placeholder="Tên s?n ph?m"
        value={formData.name}
        onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
        required
      />
      
      <textarea
        placeholder="Mô t? ng?n"
        value={formData.shortDescription}
        onChange={(e) => setFormData(prev => ({ ...prev, shortDescription: e.target.value }))}
        required
      />
      
      <input
        type="number"
        placeholder="Giá"
        value={formData.price}
        onChange={(e) => setFormData(prev => ({ ...prev, price: e.target.value }))}
        required
      />
      
      <button type="submit">T?o s?n ph?m</button>
    </form>
  );
};
```

---

## ?? C?p nh?t s?n ph?m

### PUT `/{id}`

C?p nh?t thông tin s?n ph?m (ch? dành cho Staff).

**Yêu c?u:** Bearer Token v?i role Staff

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | integer | ID c?a s?n ph?m |

#### Request Body

```json
{
  "name": "Sake Dassai 39 Junmai Daiginjo - Updated",
  "shortDescription": "Sake cao c?p t? Nh?t B?n v?i h??ng v? tinh t?",
  "description": "Mô t? chi ti?t ?ã ???c c?p nh?t...",
  "mainImage": "https://example.com/images/dassai-39-new.jpg",
  "price": 360000.00,
  "originalPrice": 420000.00,
  "stock": 20,
  "status": "Available",
  "condition": "New",
  "isFeatured": true,
  "isNew": false,
  "isBestseller": true,
  "isLimitedEdition": false,
  "isGiftWrappingAvailable": true,
  "allowBackorder": false,
  "allowPreorder": false,
  "brandId": 1,
  "categoryId": 1,
  "sku": "DASSAI-39-720ML",
  "tags": "sake, premium, dassai, bestseller"
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "Product updated successfully", 
  "data": {
    // Updated product details
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
```

---

## ??? Xóa s?n ph?m

### DELETE `/{id}`

Xóa s?n ph?m (soft delete - ch? dành cho Staff).

**Yêu c?u:** Bearer Token v?i role Staff

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | integer | ID c?a s?n ph?m |

#### Response Success (200)

```json
{
  "success": true,
  "message": "Product deleted successfully",
  "timestamp": "2024-01-15T15:00:00Z"
}
```

---

## ?? C?p nh?t t?n kho

### PATCH `/{id}/stock`

C?p nh?t s? l??ng t?n kho (ch? dành cho Staff).

**Yêu c?u:** Bearer Token v?i role Staff

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | integer | ID c?a s?n ph?m |

#### Request Body

```json
{
  "newStock": 25
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "Stock updated successfully",
  "data": {
    "message": "Stock updated successfully",
    "oldStock": 15,
    "newStock": 25,
    "isLowStock": false,
    "stockChange": "15 ? 25",
    "inventoryLogId": 123
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
```

#### Frontend Integration

```javascript
const updateStock = async (productId, newStock) => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/product/${productId}/stock`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      },
      body: JSON.stringify({ newStock })
    });
    
    const result = await response.json();
    
    if (result.success) {
      showSuccess(`C?p nh?t t?n kho thành công: ${result.data.stockChange}`);
      return result.data;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Update stock error:', error);
    showError('Có l?i x?y ra khi c?p nh?t t?n kho');
    throw error;
  }
};
```

---

## ?? Debug endpoint

### GET `/debug`

Endpoint debug ?? ki?m tra database và d? li?u s?n ph?m.

#### Response Success (200)

```json
{
  "TotalProducts": 156,
  "ActiveProducts": 142,
  "TotalCategories": 15,
  "TotalBrands": 23,
  "TotalSettings": 45,
  "AllProductIds": [
    {
      "id": 1,
      "name": "Sake Dassai 39",
      "isActive": true,
      "isDeleted": false
    }
  ],
  "DatabaseProviderName": "Microsoft.EntityFrameworkCore.SqlServer",
  "CanConnect": true,
  "ConnectionString": "Server=...",
  "AppliedMigrations": ["20240101_Initial", "20240115_AddProducts"],
  "PendingMigrations": []
}
```

---

## ??? Frontend Utilities

### Product Service Class

```javascript
class ProductService {
  constructor() {
    this.baseURL = process.env.REACT_APP_API_URL;
  }

  // Get products with filters
  async getProducts(filters = {}) {
    const queryParams = new URLSearchParams();
    
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        queryParams.append(key, value);
      }
    });
    
    const response = await fetch(`${this.baseURL}/api/product?${queryParams}`);
    return this.handleResponse(response);
  }

  // Get product by ID
  async getProductById(id) {
    const response = await fetch(`${this.baseURL}/api/product/${id}`);
    return this.handleResponse(response);
  }

  // Search products
  async searchProducts(query, filters = {}) {
    return this.getProducts({ ...filters, search: query });
  }

  // Get products by category
  async getProductsByCategory(categoryId, filters = {}) {
    return this.getProducts({ ...filters, categoryId });
  }

  // Get products by brand
  async getProductsByBrand(brandId, filters = {}) {
    return this.getProducts({ ...filters, brandId });
  }

  // Get featured products
  async getFeaturedProducts(limit = 12) {
    return this.getProducts({ 
      featuredOnly: true, 
      pageSize: limit,
      sortBy: 'created',
      sortOrder: 'desc'
    });
  }

  // Get bestsellers
  async getBestsellers(limit = 12) {
    return this.getProducts({ 
      pageSize: limit,
      sortBy: 'sold',
      sortOrder: 'desc'
    });
  }

  // Get new products
  async getNewProducts(limit = 12) {
    return this.getProducts({ 
      newOnly: true,
      pageSize: limit,
      sortBy: 'created',
      sortOrder: 'desc'
    });
  }

  // Get sale products
  async getSaleProducts(limit = 12) {
    return this.getProducts({ 
      onSaleOnly: true,
      pageSize: limit,
      sortBy: 'price',
      sortOrder: 'asc'
    });
  }

  // Handle API response
  async handleResponse(response) {
    const result = await response.json();
    
    if (!response.ok) {
      throw new Error(result.message || 'API request failed');
    }
    
    return result;
  }
}

export default new ProductService();
```

### React Hooks

```javascript
// useProducts hook
export const useProducts = (filters = {}) => {
  const [products, setProducts] = useState([]);
  const [pagination, setPagination] = useState({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const fetchProducts = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const result = await ProductService.getProducts(filters);
      
      if (result.success) {
        setProducts(result.data);
        setPagination(result.pagination);
      } else {
        setError(result.message);
      }
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  return {
    products,
    pagination,
    loading,
    error,
    refetch: fetchProducts
  };
};

// useProduct hook
export const useProduct = (productId) => {
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!productId) return;

    const fetchProduct = async () => {
      try {
        setLoading(true);
        setError(null);
        
        const result = await ProductService.getProductById(productId);
        
        if (result.success) {
          setProduct(result.data);
        } else {
          setError(result.message);
        }
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchProduct();
  }, [productId]);

  return { product, loading, error };
};
```

### Product Card Component

```javascript
const ProductCard = ({ product }) => {
  const { addToCart } = useCart();
  const { addToWishlist } = useWishlist();

  const handleAddToCart = () => {
    addToCart({
      productId: product.id,
      quantity: 1
    });
  };

  const handleAddToWishlist = () => {
    addToWishlist({
      productId: product.id
    });
  };

  return (
    <div className="product-card">
      <div className="product-image">
        <img src={product.mainImage} alt={product.name} />
        
        {product.isOnSale && (
          <div className="sale-badge">
            -{Math.round((1 - product.price / product.originalPrice) * 100)}%
          </div>
        )}
        
        {product.isNew && <div className="new-badge">M?i</div>}
        {product.isBestseller && <div className="bestseller-badge">Bán ch?y</div>}
      </div>
      
      <div className="product-info">
        <h3 className="product-name">{product.name}</h3>
        <p className="product-brand">{product.brand?.name}</p>
        
        <div className="product-rating">
          ? {product.rating} ({product.reviewCount})
        </div>
        
        <div className="product-price">
          <span className="current-price">
            {product.price.toLocaleString('vi-VN')}?
          </span>
          {product.originalPrice && (
            <span className="original-price">
              {product.originalPrice.toLocaleString('vi-VN')}?
            </span>
          )}
        </div>
        
        <div className="product-stock">
          {product.isInStock ? (
            <span className="in-stock">Còn hàng</span>
          ) : (
            <span className="out-of-stock">H?t hàng</span>
          )}
        </div>
      </div>
      
      <div className="product-actions">
        <button 
          onClick={handleAddToCart}
          disabled={!product.isInStock}
          className="add-to-cart-btn"
        >
          Thêm vào gi?
        </button>
        
        <button 
          onClick={handleAddToWishlist}
          className="add-to-wishlist-btn"
        >
          ?
        </button>
      </div>
    </div>
  );
};
```

---

## ?? Product Enums

### Product Status

```javascript
export const ProductStatus = {
  AVAILABLE: 'Available',
  OUT_OF_STOCK: 'OutOfStock',
  DISCONTINUED: 'Discontinued',
  PRE_ORDER: 'PreOrder',
  BACK_ORDER: 'BackOrder'
};
```

### Product Condition

```javascript
export const ProductCondition = {
  NEW: 'New',
  USED: 'Used',
  REFURBISHED: 'Refurbished',
  DAMAGED: 'Damaged'
};
```

### Sort Options

```javascript
export const SortOptions = [
  { value: 'created', label: 'M?i nh?t' },
  { value: 'name', label: 'Tên A-Z' },
  { value: 'price', label: 'Giá' },
  { value: 'rating', label: '?ánh giá' },
  { value: 'sold', label: 'Bán ch?y' },
  { value: 'views', label: 'L??t xem' }
];
```

---

## ?? Advanced Search Example

```javascript
const AdvancedProductSearch = () => {
  const [filters, setFilters] = useState({
    search: '',
    categoryId: null,
    brandId: null,
    minPrice: '',
    maxPrice: '',
    inStockOnly: false,
    onSaleOnly: false,
    featuredOnly: false,
    sortBy: 'created',
    sortOrder: 'desc',
    page: 1,
    pageSize: 12
  });

  const { products, pagination, loading, error } = useProducts(filters);

  const updateFilter = (key, value) => {
    setFilters(prev => ({
      ...prev,
      [key]: value,
      page: 1 // Reset to first page when filters change
    }));
  };

  return (
    <div className="product-search">
      <div className="search-filters">
        <input
          type="text"
          placeholder="Tìm ki?m s?n ph?m..."
          value={filters.search}
          onChange={(e) => updateFilter('search', e.target.value)}
        />
        
        <select
          value={filters.categoryId || ''}
          onChange={(e) => updateFilter('categoryId', e.target.value || null)}
        >
          <option value="">T?t c? danh m?c</option>
          {/* Category options */}
        </select>
        
        <input
          type="number"
          placeholder="Giá t?"
          value={filters.minPrice}
          onChange={(e) => updateFilter('minPrice', e.target.value)}
        />
        
        <input
          type="number"
          placeholder="Giá ??n"
          value={filters.maxPrice}
          onChange={(e) => updateFilter('maxPrice', e.target.value)}
        />
        
        <label>
          <input
            type="checkbox"
            checked={filters.inStockOnly}
            onChange={(e) => updateFilter('inStockOnly', e.target.checked)}
          />
          Ch? s?n ph?m còn hàng
        </label>
        
        <select
          value={`${filters.sortBy}-${filters.sortOrder}`}
          onChange={(e) => {
            const [sortBy, sortOrder] = e.target.value.split('-');
            updateFilter('sortBy', sortBy);
            updateFilter('sortOrder', sortOrder);
          }}
        >
          <option value="created-desc">M?i nh?t</option>
          <option value="price-asc">Giá t?ng d?n</option>
          <option value="price-desc">Giá gi?m d?n</option>
          <option value="rating-desc">?ánh giá cao nh?t</option>
          <option value="sold-desc">Bán ch?y nh?t</option>
        </select>
      </div>

      {loading && <div>?ang t?i...</div>}
      {error && <div className="error">L?i: {error}</div>}
      
      <div className="products-grid">
        {products.map(product => (
          <ProductCard key={product.id} product={product} />
        ))}
      </div>

      <Pagination
        currentPage={pagination.currentPage}
        totalPages={pagination.totalPages}
        onPageChange={(page) => updateFilter('page', page)}
      />
    </div>
  );
};
```

---

Tài li?u này cung c?p ??y ?? thông tin ?? frontend team tích h?p v?i Product API c?a SakuraHome. API h? tr? tìm ki?m, l?c, phân trang và qu?n lý s?n ph?m m?t cách hi?u qu?.