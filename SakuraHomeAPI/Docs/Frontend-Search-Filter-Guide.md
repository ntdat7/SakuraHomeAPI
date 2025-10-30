# 🔍 SakuraHome API - Search & Filter Documentation for Frontend Team
**Version:** 2.1.0 | **Last Updated:** January 2025

---

## 📖 Overview

This documentation provides comprehensive guidance for the **Frontend Development Team** on implementing SakuraHome's advanced search and filtering capabilities. All endpoints are production-ready with the latest MetaKeywords multi-word search enhancements.

### 🌐 Base Information
- **Base URL:** `https://localhost:8080` (development) | `https://api.sakurahome.com` (production)
- **API Version:** v1
- **Authentication:** Optional for search endpoints (Bearer token for user-specific features)

---

## 🎯 Core Search Endpoints

### 1. **Primary Search Endpoint** ⭐
```http
GET /api/product?search={query}&page={page}&pageSize={size}&...filters
```
**Purpose:** Main search endpoint with full filtering capabilities  
**Authentication:** ❌ Not required  
**Caching:** 5 minutes for public searches

### 2. **Dedicated Search Endpoint** 
```http
GET /api/product/search?search={query}&sortBy=relevance&...params
```
**Purpose:** Search-focused with enhanced relevance scoring  
**Authentication:** ❌ Not required  
**Caching:** 5 minutes

### 3. **Product Discovery Endpoints**
```http
GET /api/product/featured?count=12
GET /api/product/newest?count=15  
GET /api/product/bestsellers?count=20
GET /api/product/on-sale?count=25
GET /api/product/trending?count=10&daysPeriod=7
```

---

## 🔍 Search Functionality (Enhanced 2025)

### **🆕 Multi-Word Search Support**
Our search now supports both **exact phrase matching** and **individual term matching**:

#### ✅ **Working Search Examples:**
```javascript
// Exact phrase searches (NEW - Fixed in v2.1.0)
"bim bim"           → Finds products with exact phrase in MetaKeywords
"japanese snack"    → Finds products with exact phrase match
"biscuit sticks"    → Exact phrase matching
"green tea"         → Finds Matcha products
"marine collagen"   → Finds supplement products
"noise canceling"   → Finds Sony headphones
"skin health"       → Finds health/beauty products

// Single word searches (Always worked)
"glico"            → Finds Pocky products  
"chocolate"        → Finds chocolate products
"pocky"            → Name matches (highest priority)
"sony"             → Brand matches
```

### **🎯 Search Fields (Priority Order):**
1. **Product Name** (Highest priority)
2. **MetaKeywords** (High priority - Enhanced!)
3. **Description** & **ShortDescription**
4. **Tags** & **SKU**
5. **Brand Name** & **Category Name**

### **📊 Relevance Scoring (New Algorithm):**
```javascript
// Priority levels (higher = better ranking)
Priority 6: Exact phrase match in Name
Priority 5: Name starts with search phrase  
Priority 4: Exact phrase match in MetaKeywords ⭐ NEW
Priority 3: Name contains search phrase
Priority 2: Description fields contain phrase
Priority 1: Other fields contain phrase or individual terms
```

---

## 🎛️ Filter Parameters Reference

### **📝 Basic Parameters**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | integer | 1 | Current page number |
| `pageSize` | integer | 12 | Items per page (max: 100) |
| `search` | string | - | Search query (supports multi-word) |

### **🏷️ Category & Brand Filters**
| Parameter | Type | Description |
|-----------|------|-------------|
| `categoryId` | integer | Filter by category ID |
| `brandId` | integer | Filter by brand ID |
| `includeSubcategories` | boolean | Include subcategory products |

### **💰 Price & Rating Filters**
| Parameter | Type | Description |
|-----------|------|-------------|
| `minPrice` | decimal | Minimum price (VND) |
| `maxPrice` | decimal | Maximum price (VND) |
| `minRating` | decimal | Minimum rating (0-5) |

### **📦 Stock & Availability Filters**
| Parameter | Type | Description |
|-----------|------|-------------|
| `inStockOnly` | boolean | Only products in stock |
| `minStock` | integer | Minimum stock quantity |
| `maxStock` | integer | Maximum stock quantity |
| `allowBackorder` | boolean | Allow backorder products |

### **🎯 Product Status Filters**
| Parameter | Type | Description |
|-----------|------|-------------|
| `isFeatured` | boolean | Featured products |
| `isNew` | boolean | New products |
| `isBestseller` | boolean | Bestselling products |
| `isLimitedEdition` | boolean | Limited edition products |
| `onSaleOnly` | boolean | Products on sale |
| `hasDiscount` | boolean | Products with discounts |

### **🌍 Japanese-Specific Filters**
| Parameter | Type | Values | Description |
|-----------|------|--------|-------------|
| `japaneseRegion` | string | Tokyo, Osaka, Kyoto, etc. | Japanese region |
| `authenticityLevel` | string | Verified, Certified, Unverified | Authenticity level |
| `origin` | string | - | Product origin (text search) |

### **🏷️ Tag-Based Search (Advanced)**
| Parameter | Type | Description |
|-----------|------|-------------|
| `tagNames` | array | Tag names to search (comma-separated) |
| `tagMatchMode` | string | `Any` or `All` - how to match tags |
| `tagsSearch` | string | Text search within tags field |

### **📅 Date Range Filters**
| Parameter | Type | Format | Description |
|-----------|------|--------|-------------|
| `createdFrom` | datetime | ISO 8601 | Products created after |
| `createdTo` | datetime | ISO 8601 | Products created before |
| `availableFrom` | datetime | ISO 8601 | Available from date |
| `availableUntil` | datetime | ISO 8601 | Available until date |

### **⚖️ Weight & Dimension Filters**
| Parameter | Type | Description |
|-----------|------|-------------|
| `minWeight` | decimal | Minimum weight (grams) |
| `maxWeight` | decimal | Maximum weight (grams) |
| `weightUnit` | string | Weight unit (Gram, Kilogram, Pound) |

### **🎁 Service Filters**
| Parameter | Type | Description |
|-----------|------|-------------|
| `isGiftWrappingAvailable` | boolean | Gift wrapping available |
| `allowPreorder` | boolean | Allow preorder |

---

## 📊 Sorting Options

### **🎯 Sort Parameters**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `sortBy` | string | "created" | Sort field |
| `sortOrder` | string | "desc" | Sort direction (asc/desc) |

### **📋 Available Sort Fields**
| sortBy Value | Description | Use Case |
|-------------|-------------|----------|
| `relevance` | ⭐ Search relevance | Best for search results |
| `popularity` | Views + Sales + Rating | Trending products |
| `rating` | Customer ratings | Quality-focused |
| `price` | Product price | Price comparison |
| `created` | Creation date | Newest products |
| `sold` | Sales count | Best sellers |
| `views` | View count | Popular items |
| `stock` | Stock quantity | Inventory management |
| `discount` | Discount percentage | Sale items |
| `name` | Alphabetical | A-Z sorting |

---

## 🚀 Frontend Implementation Examples

### **1. Basic Product Search Component**
```javascript
import React, { useState, useEffect } from 'react';

const ProductSearch = () => {
  const [searchQuery, setSearchQuery] = useState('');
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [filters, setFilters] = useState({
    page: 1,
    pageSize: 12,
    sortBy: 'relevance',
    sortOrder: 'desc'
  });

  const searchProducts = async (query, filterOptions = {}) => {
    setLoading(true);
    try {
      const params = new URLSearchParams({
        search: query,
        ...filters,
        ...filterOptions
      });

      const response = await fetch(`/api/product?${params}`);
      const data = await response.json();
      
      if (data.success) {
        setProducts(data.data);
        return data;
      }
    } catch (error) {
      console.error('Search error:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="product-search">
      <input
        type="text"
        value={searchQuery}
        onChange={(e) => setSearchQuery(e.target.value)}
        placeholder="Search for products... (try 'japanese snack' or 'bim bim')"
        onKeyPress={(e) => e.key === 'Enter' && searchProducts(searchQuery)}
      />
      
      {loading && <div>Searching...</div>}
      
      <div className="products-grid">
        {products.map(product => (
          <ProductCard key={product.id} product={product} />
        ))}
      </div>
    </div>
  );
};
```

### **2. Advanced Filter Component**
```javascript
const AdvancedFilters = ({ onFiltersChange }) => {
  const [filters, setFilters] = useState({
    categoryId: null,
    brandId: null,
    minPrice: '',
    maxPrice: '',
    inStockOnly: false,
    isFeatured: false,
    japaneseRegion: '',
    sortBy: 'relevance'
  });

  const handleFilterChange = (key, value) => {
    const newFilters = { ...filters, [key]: value };
    setFilters(newFilters);
    onFiltersChange(newFilters);
  };

  return (
    <div className="advanced-filters">
      {/* Category Filter */}
      <select 
        value={filters.categoryId || ''}
        onChange={(e) => handleFilterChange('categoryId', e.target.value || null)}
      >
        <option value="">All Categories</option>
        <option value="1">Japanese Snacks</option>
        <option value="2">Beauty & Cosmetics</option>
        <option value="3">Electronics</option>
      </select>

      {/* Price Range */}
      <div className="price-range">
        <input
          type="number"
          placeholder="Min price"
          value={filters.minPrice}
          onChange={(e) => handleFilterChange('minPrice', e.target.value)}
        />
        <input
          type="number" 
          placeholder="Max price"
          value={filters.maxPrice}
          onChange={(e) => handleFilterChange('maxPrice', e.target.value)}
        />
      </div>

      {/* Boolean Filters */}
      <label>
        <input
          type="checkbox"
          checked={filters.inStockOnly}
          onChange={(e) => handleFilterChange('inStockOnly', e.target.checked)}
        />
        In Stock Only
      </label>

      <label>
        <input
          type="checkbox"
          checked={filters.isFeatured}
          onChange={(e) => handleFilterChange('isFeatured', e.target.checked)}
        />
        Featured Products
      </label>

      {/* Japanese Region */}
      <select
        value={filters.japaneseRegion}
        onChange={(e) => handleFilterChange('japaneseRegion', e.target.value)}
      >
        <option value="">Any Region</option>
        <option value="Tokyo">Tokyo</option>
        <option value="Osaka">Osaka</option>
        <option value="Kyoto">Kyoto</option>
      </select>

      {/* Sort Options */}
      <select
        value={filters.sortBy}
        onChange={(e) => handleFilterChange('sortBy', e.target.value)}
      >
        <option value="relevance">Most Relevant</option>
        <option value="popularity">Most Popular</option>
        <option value="rating">Highest Rated</option>
        <option value="price">Price: Low to High</option>
        <option value="created">Newest First</option>
      </select>
    </div>
  );
};
```

### **3. Search with URL State Management**
```javascript
import { useSearchParams } from 'react-router-dom';

const ProductSearchPage = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  
  const currentFilters = {
    search: searchParams.get('search') || '',
    page: parseInt(searchParams.get('page')) || 1,
    categoryId: searchParams.get('categoryId') || null,
    sortBy: searchParams.get('sortBy') || 'relevance',
    minPrice: searchParams.get('minPrice') || '',
    maxPrice: searchParams.get('maxPrice') || '',
    inStockOnly: searchParams.get('inStockOnly') === 'true'
  };

  const updateFilters = (newFilters) => {
    const params = new URLSearchParams();
    
    Object.entries({ ...currentFilters, ...newFilters }).forEach(([key, value]) => {
      if (value !== null && value !== '' && value !== false) {
        params.set(key, value.toString());
      }
    });
    
    setSearchParams(params);
  };

  return (
    <div>
      <SearchInput 
        value={currentFilters.search}
        onChange={(search) => updateFilters({ search, page: 1 })}
      />
      <Filters filters={currentFilters} onChange={updateFilters} />
      <ProductResults filters={currentFilters} />
    </div>
  );
};
```

---

## 📱 Response Format

### **Standard Product List Response**
```json
{
  "success": true,
  "message": null,
  "data": [
    {
      "id": 1,
      "name": "Pocky Chocolate Sticks",
      "slug": "pocky-chocolate-sticks", 
      "shortDescription": "Classic Japanese chocolate biscuit sticks",
      "mainImage": "/images/products/pocky-chocolate.jpg",
      "price": 45000.00,
      "originalPrice": 50000.00,
      "rating": 4.5,
      "reviewCount": 25,
      "stock": 497,
      "isInStock": true,
      "isOnSale": true,
      "isFeatured": true,
      "isNew": false,
      "isBestseller": true,
      "brand": {
        "id": 3,
        "name": "Pocky",
        "slug": "pocky"
      },
      "category": {
        "id": 1,
        "name": "Japanese Snacks",
        "slug": "japanese-snacks"
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
  "filters": {
    "search": "japanese snack",
    "sortBy": "relevance", 
    "sortOrder": "desc",
    "totalFiltersApplied": 1
  },
  "aggregates": {
    "minPrice": 25000.00,
    "maxPrice": 2500000.00,
    "avgPrice": 156780.50,
    "totalProducts": 156,
    "inStockProducts": 142,
    "outOfStockProducts": 14,
    "onSaleProducts": 34,
    "featuredProducts": 28,
    "newProducts": 12,
    "avgRating": 4.2,
    "topBrands": [
      {"brandId": 3, "brandName": "Pocky", "productCount": 8},
      {"brandId": 1, "brandName": "Shiseido", "productCount": 5}
    ],
    "topCategories": [
      {"categoryId": 1, "categoryName": "Japanese Snacks", "productCount": 24}
    ]
  }
}
```

### **Error Response Format**
```json
{
  "success": false,
  "message": "Invalid filter parameters",
  "errors": [
    "minPrice must be a positive number",
    "pageSize cannot exceed 100"
  ],
  "timestamp": "2025-01-16T10:30:00Z"
}
```

---

## 🎯 Real-World Search Examples

### **1. Category Page with Filters**
```javascript
// Japanese Snacks category with quality filters
const params = {
  categoryId: 1,
  includeSubcategories: true,
  minRating: 4.0,
  inStockOnly: true,
  sortBy: 'popularity',
  page: 1,
  pageSize: 24
};
```

### **2. Sale/Promotion Page**
```javascript
// Sale hunting with price range
const params = {
  hasDiscount: true,
  onSaleOnly: true,
  minPrice: 100000,
  maxPrice: 1000000,
  sortBy: 'discount',
  sortOrder: 'desc'
};
```

### **3. Premium Japanese Products**
```javascript
// High-end authentic Japanese products
const params = {
  japaneseRegion: 'Tokyo',
  authenticityLevel: 'Verified',
  minPrice: 500000,
  minRating: 4.5,
  tagNames: 'Premium,Authentic',
  tagMatchMode: 'All',
  sortBy: 'rating'
};
```

### **4. Gift Shopping**
```javascript
// Gift-appropriate products
const params = {
  isGiftWrappingAvailable: true,
  isFeatured: true,
  minRating: 4.0,
  minPrice: 200000,
  sortBy: 'popularity'
};
```

---

## ⚡ Performance Guidelines

### **🎯 Response Time Targets**
| Operation Type | Target Time | Caching |
|---------------|-------------|---------|
| Simple search | < 150ms | 5 minutes |
| Complex filters (5+ params) | < 300ms | 3 minutes |
| Search with relevance | < 250ms | 5 minutes |
| Category browsing | < 200ms | 10 minutes |

### **📊 Optimization Tips**
1. **Use pagination:** Keep `pageSize` ≤ 24 for optimal performance
2. **Cache results:** Implement client-side caching for repeated searches
3. **Debounce search:** Wait 300ms after user stops typing
4. **Preload popular:** Cache popular categories and featured products
5. **Progressive loading:** Load basic info first, details on demand

---

## 🔧 Error Handling

### **Common Error Scenarios**
```javascript
const handleSearchError = (error, response) => {
  if (response?.status === 400) {
    // Invalid parameters
    showError('Please check your search filters');
  } else if (response?.status === 429) {
    // Rate limiting
    showError('Too many requests, please try again later');
  } else if (response?.status >= 500) {
    // Server error
    showError('Search service temporarily unavailable');
  } else {
    // Network error
    showError('Connection error, please check your internet');
  }
};

const searchWithErrorHandling = async (params) => {
  try {
    const response = await fetch(`/api/product?${new URLSearchParams(params)}`);
    
    if (!response.ok) {
      await handleSearchError(null, response);
      return null;
    }
    
    return await response.json();
  } catch (error) {
    handleSearchError(error);
    return null;
  }
};
```

---

## 🧪 Testing Examples

### **Search Test Cases**
```javascript
// Test the enhanced multi-word search
const testCases = [
  // Multi-word phrases (Fixed in v2.1.0)
  { query: 'bim bim', expect: 'Pocky products' },
  { query: 'japanese snack', expect: 'Japanese snack products' },
  { query: 'green tea', expect: 'Matcha products' },
  { query: 'marine collagen', expect: 'Supplement products' },
  
  // Single words (Always worked)
  { query: 'glico', expect: 'Pocky brand products' },
  { query: 'chocolate', expect: 'Chocolate products' },
  
  // Case insensitive
  { query: 'JAPANESE SNACK', expect: 'Same as japanese snack' },
  { query: 'Pocky', expect: 'Pocky products' }
];
```

---

## 📋 Quick Reference

### **Essential Parameters for Common UI Components**

**Search Bar:**
- `search` - User's search query
- `sortBy=relevance` - Best for search results

**Category Page:**
- `categoryId` - Category filter
- `includeSubcategories=true` - Include child categories
- `sortBy=popularity` - Best for browsing

**Filter Sidebar:**
- `minPrice`, `maxPrice` - Price range
- `inStockOnly=true` - Availability filter
- `minRating` - Quality filter

**Sale Page:**
- `hasDiscount=true` - Products on sale
- `sortBy=discount` - Highest discount first
- `sortOrder=desc` - Descending order

---

## 🚀 Latest Updates (v2.1.0)

### ✅ **What's New:**
1. **🔍 Multi-word Search:** "japanese snack", "bim bim" now work perfectly
2. **📊 Enhanced Relevance:** MetaKeywords matches get high priority  
3. **⚡ Better Performance:** Optimized query structure
4. **🎯 Phrase Priority:** Exact phrases rank higher than individual terms
5. **📈 Rich Aggregates:** More detailed filter statistics

### 🔧 **Migration Notes:**
- All existing search queries continue to work
- No breaking changes to API contracts
- Enhanced results for multi-word searches
- Better relevance scoring across all searches

---

## 📞 Support & Resources

### **Development Team Contacts:**
- **Backend API:** [Backend Team] - API endpoints and business logic
- **Frontend Integration:** [Frontend Team] - UI implementation guidance  
- **Testing:** [QA Team] - Test scenarios and validation

### **Additional Resources:**
- 🔗 **API Documentation:** `/swagger` endpoint
- 🧪 **Test Collections:** `Tests/` directory in repository
- 📊 **Performance Monitoring:** Application Insights dashboard
- 🐛 **Bug Reports:** GitHub Issues

---

**© 2025 SakuraHome API Documentation - Version 2.1.0**