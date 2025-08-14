# ❤️ Wishlist API Documentation

## Tổng quan

API Wishlist của SakuraHome cung cấp đầy đủ các chức năng quản lý danh sách yêu thích, bao gồm tạo nhiều wishlist, thêm/xóa sản phẩm, chia sẻ wishlist, và chuyển sang giỏ hàng.

**Base URL:** `https://localhost:7240/api/wishlist`

## 📋 Mục lục

- [Lấy danh sách wishlist](#lấy-danh-sách-wishlist)
- [Lấy wishlist cụ thể](#lấy-wishlist-cụ-thể)
- [Lấy wishlist mặc định](#lấy-wishlist-mặc-định)
- [Tạo wishlist mới](#tạo-wishlist-mới)
- [Cập nhật wishlist](#cập-nhật-wishlist)
- [Xóa wishlist](#xóa-wishlist)
- [Thêm sản phẩm vào wishlist](#thêm-sản-phẩm-vào-wishlist)
- [Xóa sản phẩm khỏi wishlist](#xóa-sản-phẩm-khỏi-wishlist)
- [Chuyển sản phẩm sang giỏ hàng](#chuyển-sản-phẩm-sang-giỏ-hàng)
- [Chuyển tất cả sang giỏ hàng](#chuyển-tất-cả-sang-giỏ-hàng)
- [Thêm hàng loạt](#thêm-hàng-loạt)
- [Xóa hàng loạt](#xóa-hàng-loạt)
- [Chia sẻ wishlist](#chia-sẻ-wishlist)
- [Xem wishlist được chia sẻ](#xem-wishlist-được-chia-sẻ)
- [Cài đặt quyền riêng tư](#cài-đặt-quyền-riêng-tư)

---

## 📋 Lấy danh sách wishlist

### GET `/`

Lấy tất cả wishlist của người dùng hiện tại.

**Yêu cầu:** Bearer Token

#### Response Success (200)
{
  "success": true,
  "message": "Wishlists retrieved successfully",
  "data": [
    {
      "id": 1,
      "name": "Yêu thích",
      "description": "Danh sách yêu thích mặc định",
      "isDefault": true,
      "isPublic": false,
      "itemCount": 5,
      "totalValue": 2500000.00,
      "createdAt": "2024-01-01T10:00:00Z",
      "updatedAt": "2024-01-15T14:30:00Z",
      "items": [
        {
          "id": 1,
          "productId": 101,
          "product": {
            "id": 101,
            "name": "Sake Dassai 39 Junmai Daiginjo",
            "slug": "sake-dassai-39-junmai-daiginjo",
            "mainImage": "https://example.com/images/dassai-39.jpg",
            "price": 350000.00,
            "originalPrice": 420000.00,
            "isInStock": true,
            "isOnSale": true,
            "brand": {
              "id": 1,
              "name": "Dassai",
              "slug": "dassai"
            }
          },
          "addedAt": "2024-01-10T15:00:00Z"
        }
      ]
    },
    {
      "id": 2,
      "name": "Quà tặng",
      "description": "Danh sách quà tặng cho bạn bè",
      "isDefault": false,
      "isPublic": true,
      "itemCount": 3,
      "totalValue": 1200000.00,
      "createdAt": "2024-01-05T12:00:00Z",
      "updatedAt": "2024-01-12T16:00:00Z",
      "shareToken": "SHARE_ABC123",
      "items": []
    }
  ],
  "timestamp": "2024-01-15T15:00:00Z"
}
#### Frontend Integration
// Wishlist service
class WishlistService {
  constructor() {
    this.baseURL = process.env.REACT_APP_API_URL;
  }

  async getWishlists() {
    try {
      const response = await fetch(`${this.baseURL}/api/wishlist`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
        }
      });

      const result = await response.json();
      
      if (result.success) {
        return result.data;
      } else {
        throw new Error(result.message);
      }
    } catch (error) {
      console.error('Get wishlists error:', error);
      throw error;
    }
  }
}

// React hook for wishlist management
export const useWishlists = () => {
  const [wishlists, setWishlists] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const wishlistService = new WishlistService();

  const fetchWishlists = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const wishlistsData = await wishlistService.getWishlists();
      setWishlists(wishlistsData);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchWishlists();
  }, [fetchWishlists]);

  return {
    wishlists,
    loading,
    error,
    refetch: fetchWishlists
  };
};

// Wishlists overview component
const WishlistsOverview = () => {
  const { wishlists, loading, error, refetch } = useWishlists();

  if (loading) return <WishlistsSkeleton />;
  if (error) return <div className="error">Error: {error}</div>;

  return (
    <div className="wishlists-overview">
      <div className="wishlists-header">
        <h2>Danh sách yêu thích của tôi</h2>
        <CreateWishlistButton onCreated={refetch} />
      </div>

      <div className="wishlists-grid">
        {wishlists.map(wishlist => (
          <WishlistCard key={wishlist.id} wishlist={wishlist} />
        ))}
      </div>

      {wishlists.length === 0 && (
        <div className="empty-wishlists">
          <p>Bạn chưa có wishlist nào</p>
          <CreateWishlistButton onCreated={refetch} />
        </div>
      )}
    </div>
  );
};
---

## 📖 Lấy wishlist cụ thể

### GET `/{wishlistId}`

Lấy thông tin chi tiết của một wishlist cụ thể.

**Yêu cầu:** Bearer Token

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `wishlistId` | integer | ID của wishlist |

#### Response Success (200)
{
  "success": true,
  "message": "Wishlist retrieved successfully",
  "data": {
    "id": 1,
    "name": "Yêu thích",
    "description": "Danh sách yêu thích mặc định",
    "isDefault": true,
    "isPublic": false,
    "itemCount": 5,
    "totalValue": 2500000.00,
    "createdAt": "2024-01-01T10:00:00Z",
    "updatedAt": "2024-01-15T14:30:00Z",
    "items": [
      {
        "id": 1,
        "productId": 101,
        "product": {
          "id": 101,
          "name": "Sake Dassai 39 Junmai Daiginjo",
          "slug": "sake-dassai-39-junmai-daiginjo",
          "shortDescription": "Sake cao cấp từ Nhật Bản",
          "mainImage": "https://example.com/images/dassai-39.jpg",
          "price": 350000.00,
          "originalPrice": 420000.00,
          "stock": 15,
          "isInStock": true,
          "isOnSale": true,
          "isFeatured": true,
          "rating": 4.8,
          "reviewCount": 24,
          "brand": {
            "id": 1,
            "name": "Dassai",
            "slug": "dassai",
            "logoUrl": "https://example.com/brands/dassai.png"
          },
          "category": {
            "id": 1,
            "name": "Sake",
            "slug": "sake"
          }
        },
        "addedAt": "2024-01-10T15:00:00Z",
        "notes": "Muốn mua làm quà"
      }
    ],
    "shareToken": null,
    "owner": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fullName": "Nguyễn Văn A",
      "firstName": "Nguyễn",
      "lastName": "Văn A"
    }
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
#### Frontend Integration
// Wishlist detail component
const WishlistDetail = ({ wishlistId }) => {
  const [wishlist, setWishlist] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchWishlist = async () => {
      try {
        setLoading(true);
        setError(null);

        const response = await fetch(`${process.env.REACT_APP_API_URL}/api/wishlist/${wishlistId}`, {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
          }
        });

        const result = await response.json();
        
        if (result.success) {
          setWishlist(result.data);
        } else {
          setError(result.message);
        }
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchWishlist();
  }, [wishlistId]);

  if (loading) return <WishlistDetailSkeleton />;
  if (error) return <div className="error">Error: {error}</div>;
  if (!wishlist) return <div>Không tìm thấy wishlist</div>;

  return (
    <div className="wishlist-detail">
      <div className="wishlist-header">
        <div className="wishlist-info">
          <h1>{wishlist.name}</h1>
          {wishlist.description && <p>{wishlist.description}</p>}
          
          <div className="wishlist-meta">
            <span>{wishlist.itemCount} sản phẩm</span>
            <span>Tổng giá trị: {wishlist.totalValue.toLocaleString('vi-VN')}đ</span>
            {wishlist.isPublic && <span className="public-badge">Công khai</span>}
            {wishlist.isDefault && <span className="default-badge">Mặc định</span>}
          </div>
        </div>

        <div className="wishlist-actions">
          <EditWishlistButton wishlist={wishlist} onUpdated={() => window.location.reload()} />
          <ShareWishlistButton wishlist={wishlist} />
          <MoveAllToCartButton wishlistId={wishlist.id} />
          {!wishlist.isDefault && (
            <DeleteWishlistButton wishlistId={wishlist.id} />
          )}
        </div>
      </div>

      <div className="wishlist-items">
        {wishlist.items.length > 0 ? (
          <div className="items-grid">
            {wishlist.items.map(item => (
              <WishlistItemCard 
                key={item.id} 
                item={item} 
                onRemove={() => window.location.reload()}
              />
            ))}
          </div>
        ) : (
          <div className="empty-wishlist">
            <p>Wishlist này chưa có sản phẩm nào</p>
            <Link to="/products">Khám phá sản phẩm</Link>
          </div>
        )}
      </div>
    </div>
  );
};
---

## ⭐ Lấy wishlist mặc định

### GET `/default`

Lấy wishlist mặc định của người dùng.

**Yêu cầu:** Bearer Token

#### Response Success (200)
{
  "success": true,
  "message": "Default wishlist retrieved successfully",
  "data": {
    // Same structure as wishlist detail
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
---

## ➕ Tạo wishlist mới

### POST `/`

Tạo wishlist mới.

**Yêu cầu:** Bearer Token

#### Request Body
{
  "name": "Quà tặng Tết",
  "description": "Danh sách quà tặng cho dịp Tết Nguyên Đán",
  "isPublic": false
}
#### Response Success (201)
{
  "success": true,
  "message": "Wishlist created successfully",
  "data": {
    "id": 3,
    "name": "Quà tặng Tết",
    "description": "Danh sách quà tặng cho dịp Tết Nguyên Đán",
    "isDefault": false,
    "isPublic": false,
    "itemCount": 0,
    "totalValue": 0.00,
    "createdAt": "2024-01-15T15:00:00Z",
    "updatedAt": "2024-01-15T15:00:00Z",
    "items": [],
    "shareToken": null
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
#### Frontend Integration
// Create wishlist function
const createWishlist = async (wishlistData) => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_URL}/api/wishlist`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      },
      body: JSON.stringify(wishlistData)
    });

    const result = await response.json();
    
    if (result.success) {
      showSuccess('Tạo wishlist thành công');
      return result.data;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Create wishlist error:', error);
    showError('Không thể tạo wishlist: ' + error.message);
    throw error;
  }
};

// Create wishlist modal component
const CreateWishlistModal = ({ isOpen, onClose, onCreated }) => {
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    isPublic: false
  });
  const [creating, setCreating] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!formData.name.trim()) {
      showError('Tên wishlist không được để trống');
      return;
    }

    try {
      setCreating(true);
      const newWishlist = await createWishlist(formData);
      onCreated && onCreated(newWishlist);
      onClose();
      
      // Reset form
      setFormData({ name: '', description: '', isPublic: false });
    } catch (error) {
      // Error already handled
    } finally {
      setCreating(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="modal-overlay">
      <div className="create-wishlist-modal">
        <div className="modal-header">
          <h3>Tạo wishlist mới</h3>
          <button onClick={onClose}>×</button>
        </div>

        <form onSubmit={handleSubmit} className="modal-body">
          <div className="form-group">
            <label>Tên wishlist *</label>
            <input
              type="text"
              placeholder="VD: Quà tặng Tết"
              value={formData.name}
              onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
              maxLength={100}
              required
            />
          </div>

          <div className="form-group">
            <label>Mô tả</label>
            <textarea
              placeholder="Mô tả ngắn về wishlist này..."
              value={formData.description}
              onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
              maxLength={500}
              rows={3}
            />
          </div>

          <div className="form-group">
            <label className="checkbox-label">
              <input
                type="checkbox"
                checked={formData.isPublic}
                onChange={(e) => setFormData(prev => ({ ...prev, isPublic: e.target.checked }))}
              />
              Công khai (người khác có thể xem)
            </label>
          </div>

          <div className="modal-footer">
            <button type="button" onClick={onClose}>Hủy</button>
            <button type="submit" disabled={creating || !formData.name.trim()}>
              {creating ? 'Đang tạo...' : 'Tạo wishlist'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
---

## ✏️ Cập nhật wishlist

### PUT `/{wishlistId}`

Cập nhật thông tin wishlist.

**Yêu cầu:** Bearer Token

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `wishlistId` | integer | ID của wishlist |

#### Request Body
{
  "name": "Quà tặng Tết 2024",
  "description": "Danh sách quà tặng cho dịp Tết Nguyên Đán 2024",
  "isPublic": true
}
#### Response Success (200)
{
  "success": true,
  "message": "Wishlist updated successfully",
  "data": {
    // Updated wishlist data
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
---

## 🗑️ Xóa wishlist

### DELETE `/{wishlistId}`

Xóa wishlist (không thể xóa wishlist mặc định).

**Yêu cầu:** Bearer Token

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `wishlistId` | integer | ID của wishlist |

#### Response Success (200)
{
  "success": true,
  "message": "Wishlist deleted successfully",
  "timestamp": "2024-01-15T15:00:00Z"
}
---

## ➕ Thêm sản phẩm vào wishlist

### POST `/items`

Thêm sản phẩm vào wishlist.

**Yêu cầu:** Bearer Token

#### Request Body
{
  "productId": 101,
  "wishlistId": 1,
  "notes": "Muốn mua làm quà"
}
> **Lưu ý:** Nếu không chỉ định `wishlistId`, sản phẩm sẽ được thêm vào wishlist mặc định.

#### Response Success (200)
{
  "success": true,
  "message": "Item added to wishlist successfully",
  "data": {
    // Updated wishlist data
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
#### Response Error (400)
{
  "success": false,
  "message": "Product already exists in wishlist",
  "errors": [
    "Sản phẩm đã có trong wishlist này"
  ],
  "timestamp": "2024-01-15T15:00:00Z"
}
#### Frontend Integration
// Add to wishlist function
const addToWishlist = async (productId, wishlistId = null, notes = '') => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_URL}/api/wishlist/items`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      },
      body: JSON.stringify({
        productId,
        wishlistId,
        notes
      })
    });

    const result = await response.json();
    
    if (result.success) {
      showSuccess('Đã thêm vào wishlist');
      return result.data;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Add to wishlist error:', error);
    showError('Không thể thêm vào wishlist: ' + error.message);
    throw error;
  }
};

// Add to wishlist button component
const AddToWishlistButton = ({ product, className = '' }) => {
  const [adding, setAdding] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const { wishlists } = useWishlists();

  const handleQuickAdd = async () => {
    try {
      setAdding(true);
      await addToWishlist(product.id);
    } catch (error) {
      // Error already handled
    } finally {
      setAdding(false);
    }
  };

  const handleAddWithOptions = async (wishlistId, notes) => {
    try {
      setAdding(true);
      await addToWishlist(product.id, wishlistId, notes);
      setShowModal(false);
    } catch (error) {
      // Error already handled
    } finally {
      setAdding(false);
    }
  };

  return (
    <>
      <div className={`wishlist-button ${className}`}>
        <button
          onClick={handleQuickAdd}
          disabled={adding}
          className="quick-add"
          title="Thêm vào wishlist mặc định"
        >
          {adding ? '⏳' : '🤍'}
        </button>
        
        {wishlists.length > 1 && (
          <button
            onClick={() => setShowModal(true)}
            className="add-options"
            title="Chọn wishlist"
          >
            ▼
          </button>
        )}
      </div>

      {showModal && (
        <WishlistSelectionModal
          product={product}
          wishlists={wishlists}
          onAdd={handleAddWithOptions}
          onClose={() => setShowModal(false)}
        />
      )}
    </>
  );
};

// Wishlist selection modal
const WishlistSelectionModal = ({ product, wishlists, onAdd, onClose }) => {
  const [selectedWishlistId, setSelectedWishlistId] = useState(wishlists[0]?.id);
  const [notes, setNotes] = useState('');

  const handleSubmit = (e) => {
    e.preventDefault();
    onAdd(selectedWishlistId, notes);
  };

  return (
    <div className="modal-overlay">
      <div className="wishlist-selection-modal">
        <div className="modal-header">
          <h3>Thêm vào wishlist</h3>
          <button onClick={onClose}>×</button>
        </div>

        <div className="product-info">
          <img src={product.mainImage} alt={product.name} />
          <div>
            <h4>{product.name}</h4>
            <span className="price">{product.price.toLocaleString('vi-VN')}đ</span>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="modal-body">
          <div className="form-group">
            <label>Chọn wishlist</label>
            <select
              value={selectedWishlistId}
              onChange={(e) => setSelectedWishlistId(parseInt(e.target.value))}
            >
              {wishlists.map(wishlist => (
                <option key={wishlist.id} value={wishlist.id}>
                  {wishlist.name} ({wishlist.itemCount} sản phẩm)
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label>Ghi chú (tùy chọn)</label>
            <textarea
              placeholder="VD: Muốn mua làm quà..."
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              maxLength={200}
              rows={2}
            />
          </div>

          <div className="modal-footer">
            <button type="button" onClick={onClose}>Hủy</button>
            <button type="submit">Thêm vào wishlist</button>
          </div>
        </form>
      </div>
    </div>
  );
};
---

## 🗑️ Xóa sản phẩm khỏi wishlist

### DELETE `/items`

Xóa sản phẩm khỏi wishlist.

**Yêu cầu:** Bearer Token

#### Request Body
{
  "wishlistItemId": 1
}
#### Response Success (200)
{
  "success": true,
  "message": "Item removed from wishlist successfully",
  "timestamp": "2024-01-15T15:00:00Z"
}
#### Frontend Integration
// Remove from wishlist function
const removeFromWishlist = async (wishlistItemId) => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_URL}/api/wishlist/items`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      },
      body: JSON.stringify({ wishlistItemId })
    });

    const result = await response.json();
    
    if (result.success) {
      showSuccess('Đã xóa khỏi wishlist');
      return true;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Remove from wishlist error:', error);
    showError('Không thể xóa khỏi wishlist');
    throw error;
  }
};

// Wishlist item card component
const WishlistItemCard = ({ item, onRemove, onMoveToCart }) => {
  const [removing, setRemoving] = useState(false);
  const [movingToCart, setMovingToCart] = useState(false);

  const handleRemove = async () => {
    try {
      setRemoving(true);
      await removeFromWishlist(item.id);
      onRemove && onRemove();
    } catch (error) {
      // Error already handled
    } finally {
      setRemoving(false);
    }
  };

  const handleMoveToCart = async () => {
    try {
      setMovingToCart(true);
      await moveToCart(item.id, 1);
      onMoveToCart && onMoveToCart();
    } catch (error) {
      // Error already handled
    } finally {
      setMovingToCart(false);
    }
  };

  return (
    <div className="wishlist-item-card">
      <div className="item-image">
        <Link to={`/products/${item.product.slug}`}>
          <img src={item.product.mainImage} alt={item.product.name} />
        </Link>
        
        {item.product.isOnSale && <div className="sale-badge">Giảm giá</div>}
        {!item.product.isInStock && <div className="out-of-stock-overlay">Hết hàng</div>}
      </div>

      <div className="item-info">
        <h4 className="item-name">
          <Link to={`/products/${item.product.slug}`}>
            {item.product.name}
          </Link>
        </h4>
        
        <p className="item-brand">{item.product.brand?.name}</p>
        
        <div className="item-price">
          <span className="current-price">
            {item.product.price.toLocaleString('vi-VN')}đ
          </span>
          {item.product.originalPrice && (
            <span className="original-price">
              {item.product.originalPrice.toLocaleString('vi-VN')}đ
            </span>
          )}
        </div>

        {item.product.rating && (
          <div className="item-rating">
            ⭐ {item.product.rating} ({item.product.reviewCount})
          </div>
        )}

        {item.notes && (
          <div className="item-notes">
            <small>📝 {item.notes}</small>
          </div>
        )}

        <div className="item-meta">
          <small>Thêm vào: {formatDate(item.addedAt)}</small>
        </div>
      </div>

      <div className="item-actions">
        <button
          onClick={handleMoveToCart}
          disabled={movingToCart || !item.product.isInStock}
          className="move-to-cart-btn"
        >
          {movingToCart ? 'Đang thêm...' : 'Thêm vào giỏ'}
        </button>
        
        <button
          onClick={handleRemove}
          disabled={removing}
          className="remove-btn"
        >
          {removing ? 'Đang xóa...' : 'Xóa'}
        </button>
      </div>
    </div>
  );
};
---

## 🛒 Chuyển sản phẩm sang giỏ hàng

### POST `/items/{wishlistItemId}/move-to-cart`

Chuyển sản phẩm từ wishlist sang giỏ hàng.

**Yêu cầu:** Bearer Token

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `wishlistItemId` | integer | ID của item trong wishlist |

#### Query Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `quantity` | integer | 1 | Số lượng muốn thêm vào giỏ |

#### Response Success (200)
{
  "success": true,
  "message": "Item moved to cart successfully",
  "timestamp": "2024-01-15T15:00:00Z"
}
---

## 🛒 Chuyển tất cả sang giỏ hàng

### POST `/{wishlistId}/move-all-to-cart`

Chuyển tất cả sản phẩm từ wishlist sang giỏ hàng.

**Yêu cầu:** Bearer Token

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `wishlistId` | integer | ID của wishlist |

#### Response Success (200)
{
  "success": true,
  "message": "All items moved to cart successfully",
  "timestamp": "2024-01-15T15:00:00Z"
}
#### Response Error (400)
{
  "success": false,
  "message": "Some items could not be moved to cart",
  "errors": [
    "Product 'Sake Dassai 39' is out of stock",
    "Product 'Kimono Vintage' exceeded available quantity"
  ],
  "timestamp": "2024-01-15T15:00:00Z"
}
---

## 📦 Thêm hàng loạt

### POST `/bulk/add`

Thêm nhiều sản phẩm vào wishlist cùng lúc.

**Yêu cầu:** Bearer Token

#### Request Body
{
  "wishlistId": 1,
  "items": [
    {
      "productId": 101,
      "notes": "Muốn mua làm quà"
    },
    {
      "productId": 102,
      "notes": "Sản phẩm yêu thích"
    },
    {
      "productId": 103
    }
  ]
}
#### Response Success (200)
{
  "success": true,
  "message": "Items added to wishlist successfully",
  "data": {
    // Updated wishlist data
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
---

## 🗑️ Xóa hàng loạt

### DELETE `/bulk/remove`

Xóa nhiều sản phẩm khỏi wishlist cùng lúc.

**Yêu cầu:** Bearer Token

#### Request Body
{
  "wishlistItemIds": [1, 2, 3]
}
#### Response Success (200)
{
  "success": true,
  "message": "Items removed from wishlist successfully",
  "timestamp": "2024-01-15T15:00:00Z"
}
---

## 🔗 Chia sẻ wishlist

### POST `/{wishlistId}/share`

Tạo link chia sẻ cho wishlist công khai.

**Yêu cầu:** Bearer Token

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `wishlistId` | integer | ID của wishlist |

#### Response Success (200)
{
  "success": true,
  "message": "Wishlist shared successfully",
  "data": "https://sakurahome.vn/wishlist/shared/SHARE_ABC123DEF456",
  "timestamp": "2024-01-15T15:00:00Z"
}
#### Frontend Integration
// Share wishlist function
const shareWishlist = async (wishlistId) => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_URL}/api/wishlist/${wishlistId}/share`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      }
    });

    const result = await response.json();
    
    if (result.success) {
      return result.data; // Share URL
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Share wishlist error:', error);
    throw error;
  }
};

// Share wishlist button component
const ShareWishlistButton = ({ wishlist }) => {
  const [sharing, setSharing] = useState(false);
  const [shareUrl, setShareUrl] = useState(null);

  const handleShare = async () => {
    try {
      setSharing(true);
      const url = await shareWishlist(wishlist.id);
      setShareUrl(url);
    } catch (error) {
      showError('Không thể tạo link chia sẻ');
    } finally {
      setSharing(false);
    }
  };

  const copyToClipboard = () => {
    navigator.clipboard.writeText(shareUrl);
    showSuccess('Đã sao chép link chia sẻ');
  };

  const shareViaWhatsApp = () => {
    const message = `Hãy xem wishlist "${wishlist.name}" của tôi: ${shareUrl}`;
    window.open(`https://wa.me/?text=${encodeURIComponent(message)}`);
  };

  const shareViaFacebook = () => {
    window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(shareUrl)}`);
  };

  if (!wishlist.isPublic) {
    return (
      <button disabled title="Chỉ wishlist công khai mới có thể chia sẻ">
        🔒 Riêng tư
      </button>
    );
  }

  return (
    <div className="share-wishlist">
      {!shareUrl ? (
        <button 
          onClick={handleShare} 
          disabled={sharing}
          className="share-btn"
        >
          {sharing ? 'Đang tạo link...' : '🔗 Chia sẻ'}
        </button>
      ) : (
        <div className="share-options">
          <div className="share-url">
            <input type="text" value={shareUrl} readOnly />
            <button onClick={copyToClipboard}>Sao chép</button>
          </div>
          
          <div className="share-social">
            <button onClick={shareViaWhatsApp} className="whatsapp">
              WhatsApp
            </button>
            <button onClick={shareViaFacebook} className="facebook">
              Facebook
            </button>
          </div>
        </div>
      )}
    </div>
  );
};
---

## 👁️ Xem wishlist được chia sẻ

### GET `/shared/{shareToken}`

Xem wishlist được chia sẻ (endpoint công khai).

**Không yêu cầu xác thực**

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `shareToken` | string | Token chia sẻ wishlist |

#### Response Success (200)
{
  "success": true,
  "message": "Shared wishlist retrieved successfully",
  "data": {
    "id": 2,
    "name": "Quà tặng Tết",
    "description": "Danh sách quà tặng cho dịp Tết Nguyên Đán",
    "itemCount": 3,
    "totalValue": 1200000.00,
    "createdAt": "2024-01-05T12:00:00Z",
    "updatedAt": "2024-01-12T16:00:00Z",
    "items": [
      // Similar structure to regular wishlist items
    ],
    "owner": {
      "firstName": "Nguyễn",
      "lastName": "Văn A",
      "fullName": "Nguyễn Văn A"
    },
    "isShared": true
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
#### Frontend Integration
// Shared wishlist page component
const SharedWishlistPage = () => {
  const { shareToken } = useParams();
  const [wishlist, setWishlist] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchSharedWishlist = async () => {
      try {
        setLoading(true);
        setError(null);

        const response = await fetch(`${process.env.REACT_APP_API_URL}/api/wishlist/shared/${shareToken}`);
        const result = await response.json();
        
        if (result.success) {
          setWishlist(result.data);
        } else {
          setError(result.message);
        }
      } catch (err) {
        setError('Không thể tải wishlist được chia sẻ');
      } finally {
        setLoading(false);
      }
    };

    fetchSharedWishlist();
  }, [shareToken]);

  if (loading) return <WishlistDetailSkeleton />;
  if (error) return <div className="error">{error}</div>;
  if (!wishlist) return <div>Không tìm thấy wishlist</div>;

  return (
    <div className="shared-wishlist-page">
      <div className="wishlist-header">
        <div className="shared-badge">
          🔗 Wishlist được chia sẻ
        </div>
        
        <h1>{wishlist.name}</h1>
        {wishlist.description && <p>{wishlist.description}</p>}
        
        <div className="owner-info">
          <span>Được chia sẻ bởi: <strong>{wishlist.owner.fullName}</strong></span>
        </div>
        
        <div className="wishlist-meta">
          <span>{wishlist.itemCount} sản phẩm</span>
          <span>Tổng giá trị: {wishlist.totalValue.toLocaleString('vi-VN')}đ</span>
        </div>
      </div>

      <div className="wishlist-items">
        {wishlist.items.length > 0 ? (
          <div className="items-grid">
            {wishlist.items.map(item => (
              <SharedWishlistItemCard key={item.id} item={item} />
            ))}
          </div>
        ) : (
          <div className="empty-wishlist">
            <p>Wishlist này chưa có sản phẩm nào</p>
          </div>
        )}
      </div>

      <div className="call-to-action">
        <Link to="/register" className="cta-button">
          Tạo wishlist của riêng bạn
        </Link>
      </div>
    </div>
  );
};

// Shared wishlist item card (view only)
const SharedWishlistItemCard = ({ item }) => {
  return (
    <div className="shared-wishlist-item-card">
      <div className="item-image">
        <Link to={`/products/${item.product.slug}`}>
          <img src={item.product.mainImage} alt={item.product.name} />
        </Link>
        
        {item.product.isOnSale && <div className="sale-badge">Giảm giá</div>}
        {!item.product.isInStock && <div className="out-of-stock-overlay">Hết hàng</div>}
      </div>

      <div className="item-info">
        <h4 className="item-name">
          <Link to={`/products/${item.product.slug}`}>
            {item.product.name}
          </Link>
        </h4>
        
        <p className="item-brand">{item.product.brand?.name}</p>
        
        <div className="item-price">
          <span className="current-price">
            {item.product.price.toLocaleString('vi-VN')}đ
          </span>
          {item.product.originalPrice && (
            <span className="original-price">
              {item.product.originalPrice.toLocaleString('vi-VN')}đ
            </span>
          )}
        </div>

        {item.product.rating && (
          <div className="item-rating">
            ⭐ {item.product.rating} ({item.product.reviewCount})
          </div>
        )}
      </div>

      <div className="item-actions">
        <Link 
          to={`/products/${item.product.slug}`}
          className="view-product-btn"
        >
          Xem sản phẩm
        </Link>
      </div>
    </div>
  );
};
---

## 🔐 Cài đặt quyền riêng tư

### PATCH `/{wishlistId}/privacy`

Thay đổi cài đặt công khai/riêng tư của wishlist.

**Yêu cầu:** Bearer Token

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `wishlistId` | integer | ID của wishlist |

#### Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `isPublic` | boolean | true = công khai, false = riêng tư |

#### Response Success (200)
{
  "success": true,
  "message": "Wishlist privacy updated successfully",
  "timestamp": "2024-01-15T15:00:00Z"
}
---

## 🛠️ Complete Wishlist Management Hook
// Complete wishlist management hook
export const useWishlistManagement = () => {
  const [wishlists, setWishlists] = useState([]);
  const [loading, setLoading] = useState(false);

  const wishlistService = new WishlistService();

  const fetchWishlists = async () => {
    try {
      setLoading(true);
      const data = await wishlistService.getWishlists();
      setWishlists(data);
    } catch (error) {
      showError('Không thể tải wishlist');
    } finally {
      setLoading(false);
    }
  };

  const createWishlist = async (wishlistData) => {
    const newWishlist = await wishlistService.createWishlist(wishlistData);
    setWishlists(prev => [...prev, newWishlist]);
    return newWishlist;
  };

  const updateWishlist = async (wishlistId, updateData) => {
    const updatedWishlist = await wishlistService.updateWishlist(wishlistId, updateData);
    setWishlists(prev => prev.map(w => w.id === wishlistId ? updatedWishlist : w));
    return updatedWishlist;
  };

  const deleteWishlist = async (wishlistId) => {
    await wishlistService.deleteWishlist(wishlistId);
    setWishlists(prev => prev.filter(w => w.id !== wishlistId));
  };

  const addToWishlist = async (productId, wishlistId, notes) => {
    await wishlistService.addToWishlist(productId, wishlistId, notes);
    await fetchWishlists(); // Refresh to get updated counts
  };

  const removeFromWishlist = async (wishlistItemId) => {
    await wishlistService.removeFromWishlist(wishlistItemId);
    await fetchWishlists();
  };

  const moveToCart = async (wishlistItemId, quantity = 1) => {
    await wishlistService.moveToCart(wishlistItemId, quantity);
    await fetchWishlists();
  };

  const moveAllToCart = async (wishlistId) => {
    await wishlistService.moveAllToCart(wishlistId);
    await fetchWishlists();
  };

  const shareWishlist = async (wishlistId) => {
    return await wishlistService.shareWishlist(wishlistId);
  };

  const setPrivacy = async (wishlistId, isPublic) => {
    await wishlistService.setPrivacy(wishlistId, isPublic);
    await fetchWishlists();
  };

  useEffect(() => {
    fetchWishlists();
  }, []);

  return {
    wishlists,
    loading,
    actions: {
      createWishlist,
      updateWishlist,
      deleteWishlist,
      addToWishlist,
      removeFromWishlist,
      moveToCart,
      moveAllToCart,
      shareWishlist,
      setPrivacy,
      refresh: fetchWishlists
    }
  };
};
---

## 📱 Wishlist Widget Component
// Compact wishlist widget for product pages
const WishlistWidget = ({ product }) => {
  const { wishlists, actions } = useWishlistManagement();
  const [isInWishlist, setIsInWishlist] = useState(false);
  const [wishlistItems, setWishlistItems] = useState([]);

  useEffect(() => {
    // Check if product is in any wishlist
    const items = [];
    wishlists.forEach(wishlist => {
      const item = wishlist.items?.find(item => item.productId === product.id);
      if (item) {
        items.push({ ...item, wishlistName: wishlist.name });
      }
    });
    
    setWishlistItems(items);
    setIsInWishlist(items.length > 0);
  }, [wishlists, product.id]);

  const handleToggleWishlist = async () => {
    if (isInWishlist) {
      // Remove from all wishlists
      for (const item of wishlistItems) {
        await actions.removeFromWishlist(item.id);
      }
    } else {
      // Add to default wishlist
      await actions.addToWishlist(product.id);
    }
  };

  return (
    <div className="wishlist-widget">
      <button
        onClick={handleToggleWishlist}
        className={`wishlist-toggle ${isInWishlist ? 'active' : ''}`}
        title={isInWishlist ? 'Xóa khỏi wishlist' : 'Thêm vào wishlist'}
      >
        {isInWishlist ? '❤️' : '🤍'}
      </button>
      
      {isInWishlist && (
        <div className="wishlist-status">
          <small>
            Có trong {wishlistItems.length} wishlist
            {wishlistItems.length === 1 && `: ${wishlistItems[0].wishlistName}`}
          </small>
        </div>
      )}
    </div>
  );
};
---

Tài liệu này cung cấp đầy đủ thông tin để frontend team tích hợp với Wishlist API của SakuraHome. API hỗ trợ quản lý nhiều wishlist, chia sẻ công khai, và tích hợp mượt mà với giỏ hàng.