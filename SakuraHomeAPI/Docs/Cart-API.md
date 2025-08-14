# ?? Shopping Cart API Documentation

## T?ng quan

API Shopping Cart c?a SakuraHome cung c?p ??y ?? các ch?c n?ng qu?n lý gi? hàng, bao g?m thêm/xóa s?n ph?m, c?p nh?t s? l??ng, áp d?ng coupon, và h? tr? guest cart.

**Base URL:** `https://localhost:7240/api/cart`

## ?? M?c l?c

- [L?y gi? hàng](#l?y-gi?-hàng)
- [L?y tóm t?t gi? hàng](#l?y-tóm-t?t-gi?-hàng)
- [Thêm s?n ph?m vào gi?](#thêm-s?n-ph?m-vào-gi?)
- [C?p nh?t s? l??ng s?n ph?m](#c?p-nh?t-s?-l??ng-s?n-ph?m)
- [Xóa s?n ph?m kh?i gi?](#xóa-s?n-ph?m-kh?i-gi?)
- [Xóa toàn b? gi? hàng](#xóa-toàn-b?-gi?-hàng)
- [C?p nh?t hàng lo?t](#c?p-nh?t-hàng-lo?t)
- [Ki?m tra tính h?p l?](#ki?m-tra-tính-h?p-l?)
- [G?p gi? hàng guest](#g?p-gi?-hàng-guest)
- [Áp d?ng coupon](#áp-d?ng-coupon)
- [Xóa coupon](#xóa-coupon)

---

## ?? L?y gi? hàng

### GET `/`

L?y thông tin chi ti?t gi? hàng c?a ng??i dùng.

**Tùy ch?n xác th?c:** Bearer Token ho?c sessionId

#### Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `sessionId` | string | Session ID cho guest cart (không b?t bu?c n?u ?ã ??ng nh?p) |

#### Response Success (200)

```json
{
  "success": true,
  "message": "Cart retrieved successfully",
  "data": {
    "id": 1,
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sessionId": null,
    "items": [
      {
        "id": 1,
        "productId": 101,
        "quantity": 2,
        "unitPrice": 350000.00,
        "totalPrice": 700000.00,
        "addedAt": "2024-01-15T10:00:00Z",
        "product": {
          "id": 101,
          "name": "Sake Dassai 39 Junmai Daiginjo",
          "slug": "sake-dassai-39-junmai-daiginjo",
          "mainImage": "https://example.com/images/dassai-39.jpg",
          "price": 350000.00,
          "originalPrice": 420000.00,
          "stock": 15,
          "isInStock": true,
          "brand": {
            "id": 1,
            "name": "Dassai",
            "slug": "dassai"
          },
          "category": {
            "id": 1,
            "name": "Sake",
            "slug": "sake"
          }
        },
        "customOptions": {
          "giftWrap": true,
          "giftMessage": "Chúc m?ng sinh nh?t!"
        }
      }
    ],
    "summary": {
      "itemCount": 3,
      "totalItems": 5,
      "subtotal": 1250000.00,
      "shippingFee": 50000.00,
      "discount": 125000.00,
      "tax": 0.00,
      "total": 1175000.00
    },
    "coupon": {
      "code": "SAVE10",
      "discountType": "Percentage",
      "discountValue": 10.0,
      "discountAmount": 125000.00
    },
    "lastUpdated": "2024-01-15T14:30:00Z",
    "isGuestCart": false
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
```

#### Frontend Integration

```javascript
// Cart service
class CartService {
  constructor() {
    this.baseURL = process.env.REACT_APP_API_URL;
  }

  // Get cart (works for both authenticated and guest users)
  async getCart(sessionId = null) {
    try {
      const url = new URL(`${this.baseURL}/api/cart`);
      if (sessionId) {
        url.searchParams.append('sessionId', sessionId);
      }

      const headers = {
        'Content-Type': 'application/json'
      };

      // Add auth header if user is logged in
      const token = localStorage.getItem('accessToken');
      if (token) {
        headers.Authorization = `Bearer ${token}`;
      }

      const response = await fetch(url, { headers });
      const result = await response.json();
      
      if (result.success) {
        return result.data;
      } else {
        throw new Error(result.message);
      }
    } catch (error) {
      console.error('Get cart error:', error);
      throw error;
    }
  }

  // Generate session ID for guest users
  generateSessionId() {
    return 'guest_' + Math.random().toString(36).substr(2, 9) + '_' + Date.now();
  }

  // Get or create session ID
  getSessionId() {
    let sessionId = localStorage.getItem('guestSessionId');
    if (!sessionId) {
      sessionId = this.generateSessionId();
      localStorage.setItem('guestSessionId', sessionId);
    }
    return sessionId;
  }
}

// React hook for cart
export const useCart = () => {
  const [cart, setCart] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const { user } = useAuth();

  const cartService = new CartService();

  const fetchCart = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const sessionId = user ? null : cartService.getSessionId();
      const cartData = await cartService.getCart(sessionId);
      
      setCart(cartData);
    } catch (err) {
      setError(err.message);
      // Initialize empty cart on error
      setCart({
        items: [],
        summary: {
          itemCount: 0,
          totalItems: 0,
          subtotal: 0,
          total: 0
        }
      });
    } finally {
      setLoading(false);
    }
  }, [user]);

  useEffect(() => {
    fetchCart();
  }, [fetchCart]);

  return {
    cart,
    loading,
    error,
    refetch: fetchCart
  };
};
```

---

## ?? L?y tóm t?t gi? hàng

### GET `/summary`

L?y thông tin tóm t?t gi? hàng (phiên b?n nh?).

#### Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `sessionId` | string | Session ID cho guest cart |

#### Response Success (200)

```json
{
  "success": true,
  "message": "Cart summary retrieved successfully",
  "data": {
    "itemCount": 3,
    "totalItems": 5,
    "subtotal": 1250000.00,
    "discount": 125000.00,
    "total": 1175000.00,
    "hasItems": true
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
```

#### Frontend Integration

```javascript
// Cart summary component (for header/navbar)
const CartSummary = () => {
  const [summary, setSummary] = useState({ itemCount: 0, total: 0 });
  const { user } = useAuth();

  useEffect(() => {
    const fetchSummary = async () => {
      try {
        const cartService = new CartService();
        const sessionId = user ? null : cartService.getSessionId();
        const url = new URL(`${process.env.REACT_APP_API_URL}/api/cart/summary`);
        
        if (sessionId) {
          url.searchParams.append('sessionId', sessionId);
        }

        const headers = { 'Content-Type': 'application/json' };
        const token = localStorage.getItem('accessToken');
        if (token) {
          headers.Authorization = `Bearer ${token}`;
        }

        const response = await fetch(url, { headers });
        const result = await response.json();
        
        if (result.success) {
          setSummary(result.data);
        }
      } catch (error) {
        console.error('Fetch cart summary error:', error);
      }
    };

    fetchSummary();
  }, [user]);

  return (
    <div className="cart-summary">
      <span className="cart-icon">??</span>
      <span className="item-count">{summary.itemCount}</span>
      <span className="total">{summary.total.toLocaleString('vi-VN')}?</span>
    </div>
  );
};
```

---

## ? Thêm s?n ph?m vào gi?

### POST `/items`

Thêm s?n ph?m vào gi? hàng.

#### Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `sessionId` | string | Session ID cho guest cart |

#### Request Body

```json
{
  "productId": 101,
  "quantity": 2,
  "customOptions": {
    "giftWrap": true,
    "giftMessage": "Chúc m?ng sinh nh?t!",
    "color": "??",
    "size": "L"
  }
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "Item added to cart successfully",
  "data": {
    // Full cart data (same structure as GET /cart)
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
```

#### Response Error (400)

```json
{
  "success": false,
  "message": "Product is out of stock",
  "errors": [
    "Requested quantity (5) exceeds available stock (3)"
  ],
  "timestamp": "2024-01-15T15:00:00Z"
}
```

#### Frontend Integration

```javascript
// Add to cart function
const addToCart = async (productId, quantity = 1, customOptions = {}) => {
  try {
    const cartService = new CartService();
    const sessionId = user ? null : cartService.getSessionId();
    
    const url = new URL(`${process.env.REACT_APP_API_URL}/api/cart/items`);
    if (sessionId) {
      url.searchParams.append('sessionId', sessionId);
    }

    const headers = { 'Content-Type': 'application/json' };
    const token = localStorage.getItem('accessToken');
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }

    const response = await fetch(url, {
      method: 'POST',
      headers,
      body: JSON.stringify({
        productId,
        quantity,
        customOptions
      })
    });

    const result = await response.json();
    
    if (result.success) {
      showSuccess('?ã thêm s?n ph?m vào gi? hàng');
      return result.data;
    } else {
      showError(result.message);
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Add to cart error:', error);
    throw error;
  }
};

// Add to cart button component
const AddToCartButton = ({ product, quantity = 1, customOptions = {} }) => {
  const [adding, setAdding] = useState(false);
  const { refetch: refetchCart } = useCart();

  const handleAddToCart = async () => {
    try {
      setAdding(true);
      await addToCart(product.id, quantity, customOptions);
      await refetchCart(); // Refresh cart data
    } catch (error) {
      // Error already handled in addToCart
    } finally {
      setAdding(false);
    }
  };

  return (
    <button
      onClick={handleAddToCart}
      disabled={adding || !product.isInStock}
      className={`add-to-cart-btn ${adding ? 'loading' : ''}`}
    >
      {adding ? '?ang thêm...' : 'Thêm vào gi?'}
    </button>
  );
};
```

---

## ?? C?p nh?t s? l??ng s?n ph?m

### PUT `/items`

C?p nh?t s? l??ng s?n ph?m trong gi? hàng.

#### Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `sessionId` | string | Session ID cho guest cart |

#### Request Body

```json
{
  "productId": 101,
  "quantity": 3
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "Cart item updated successfully",
  "data": {
    // Full cart data
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
```

#### Frontend Integration

```javascript
// Update cart item quantity
const updateCartItem = async (productId, quantity) => {
  try {
    const cartService = new CartService();
    const sessionId = user ? null : cartService.getSessionId();
    
    const url = new URL(`${process.env.REACT_APP_API_URL}/api/cart/items`);
    if (sessionId) {
      url.searchParams.append('sessionId', sessionId);
    }

    const headers = { 'Content-Type': 'application/json' };
    const token = localStorage.getItem('accessToken');
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }

    const response = await fetch(url, {
      method: 'PUT',
      headers,
      body: JSON.stringify({
        productId,
        quantity
      })
    });

    const result = await response.json();
    
    if (result.success) {
      return result.data;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Update cart item error:', error);
    throw error;
  }
};

// Quantity selector component
const QuantitySelector = ({ item, onUpdate }) => {
  const [quantity, setQuantity] = useState(item.quantity);
  const [updating, setUpdating] = useState(false);

  const handleQuantityChange = async (newQuantity) => {
    if (newQuantity < 1) return;
    if (newQuantity > item.product.stock) {
      showError(`Ch? còn ${item.product.stock} s?n ph?m trong kho`);
      return;
    }

    try {
      setUpdating(true);
      setQuantity(newQuantity);
      
      await updateCartItem(item.productId, newQuantity);
      onUpdate && onUpdate();
    } catch (error) {
      // Revert quantity on error
      setQuantity(item.quantity);
      showError('Không th? c?p nh?t s? l??ng');
    } finally {
      setUpdating(false);
    }
  };

  return (
    <div className="quantity-selector">
      <button
        onClick={() => handleQuantityChange(quantity - 1)}
        disabled={updating || quantity <= 1}
      >
        -
      </button>
      
      <input
        type="number"
        value={quantity}
        onChange={(e) => {
          const newQty = parseInt(e.target.value);
          if (!isNaN(newQty)) {
            handleQuantityChange(newQty);
          }
        }}
        disabled={updating}
        min="1"
        max={item.product.stock}
      />
      
      <button
        onClick={() => handleQuantityChange(quantity + 1)}
        disabled={updating || quantity >= item.product.stock}
      >
        +
      </button>
    </div>
  );
};
```

---

## ??? Xóa s?n ph?m kh?i gi?

### DELETE `/items`

Xóa s?n ph?m kh?i gi? hàng.

#### Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `sessionId` | string | Session ID cho guest cart |

#### Request Body

```json
{
  "productId": 101
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "Item removed from cart successfully",
  "timestamp": "2024-01-15T15:00:00Z"
}
```

#### Frontend Integration

```javascript
// Remove from cart function
const removeFromCart = async (productId) => {
  try {
    const cartService = new CartService();
    const sessionId = user ? null : cartService.getSessionId();
    
    const url = new URL(`${process.env.REACT_APP_API_URL}/api/cart/items`);
    if (sessionId) {
      url.searchParams.append('sessionId', sessionId);
    }

    const headers = { 'Content-Type': 'application/json' };
    const token = localStorage.getItem('accessToken');
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }

    const response = await fetch(url, {
      method: 'DELETE',
      headers,
      body: JSON.stringify({ productId })
    });

    const result = await response.json();
    
    if (result.success) {
      showSuccess('?ã xóa s?n ph?m kh?i gi? hàng');
      return true;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Remove from cart error:', error);
    showError('Không th? xóa s?n ph?m');
    throw error;
  }
};

// Remove button component
const RemoveFromCartButton = ({ productId, onRemove }) => {
  const [removing, setRemoving] = useState(false);

  const handleRemove = async () => {
    try {
      setRemoving(true);
      await removeFromCart(productId);
      onRemove && onRemove();
    } catch (error) {
      // Error already handled
    } finally {
      setRemoving(false);
    }
  };

  return (
    <button
      onClick={handleRemove}
      disabled={removing}
      className="remove-btn"
    >
      {removing ? '?ang xóa...' : 'Xóa'}
    </button>
  );
};
```

---

## ??? Xóa toàn b? gi? hàng

### DELETE `/clear`

Xóa t?t c? s?n ph?m trong gi? hàng.

#### Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `sessionId` | string | Session ID cho guest cart |

#### Response Success (200)

```json
{
  "success": true,
  "message": "Cart cleared successfully",
  "timestamp": "2024-01-15T15:00:00Z"
}
```

#### Frontend Integration

```javascript
// Clear cart function
const clearCart = async () => {
  try {
    const cartService = new CartService();
    const sessionId = user ? null : cartService.getSessionId();
    
    const url = new URL(`${process.env.REACT_APP_API_URL}/api/cart/clear`);
    if (sessionId) {
      url.searchParams.append('sessionId', sessionId);
    }

    const headers = { 'Content-Type': 'application/json' };
    const token = localStorage.getItem('accessToken');
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }

    const response = await fetch(url, {
      method: 'DELETE',
      headers
    });

    const result = await response.json();
    
    if (result.success) {
      showSuccess('?ã xóa toàn b? gi? hàng');
      return true;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Clear cart error:', error);
    showError('Không th? xóa gi? hàng');
    throw error;
  }
};

// Clear cart button with confirmation
const ClearCartButton = ({ onClear }) => {
  const [clearing, setClearing] = useState(false);

  const handleClear = async () => {
    const confirmed = window.confirm('B?n có ch?c mu?n xóa toàn b? gi? hàng?');
    if (!confirmed) return;

    try {
      setClearing(true);
      await clearCart();
      onClear && onClear();
    } catch (error) {
      // Error already handled
    } finally {
      setClearing(false);
    }
  };

  return (
    <button
      onClick={handleClear}
      disabled={clearing}
      className="clear-cart-btn"
    >
      {clearing ? '?ang xóa...' : 'Xóa toàn b?'}
    </button>
  );
};
```

---

## ?? C?p nh?t hàng lo?t

### PUT `/bulk`

C?p nh?t nhi?u s?n ph?m trong gi? hàng cùng lúc.

#### Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `sessionId` | string | Session ID cho guest cart |

#### Request Body

```json
{
  "items": [
    {
      "productId": 101,
      "quantity": 3
    },
    {
      "productId": 102,
      "quantity": 1
    },
    {
      "productId": 103,
      "quantity": 0
    }
  ]
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "Cart updated successfully",
  "data": {
    // Full cart data
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
```

---

## ? Ki?m tra tính h?p l?

### POST `/validate`

Ki?m tra tính h?p l? c?a gi? hàng (t?n kho, giá c?, v.v.).

#### Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `sessionId` | string | Session ID cho guest cart |

#### Response Success (200)

```json
{
  "success": true,
  "message": "Cart is valid",
  "timestamp": "2024-01-15T15:00:00Z"
}
```

#### Response Error (400)

```json
{
  "success": false,
  "message": "Cart validation failed",
  "errors": [
    "Product 'Sake Dassai 39' is out of stock",
    "Product 'Kimono Vintage' price has changed from 500,000? to 550,000?"
  ],
  "timestamp": "2024-01-15T15:00:00Z"
}
```

---

## ?? G?p gi? hàng guest

### POST `/merge`

G?p gi? hàng guest v?i gi? hàng ng??i dùng sau khi ??ng nh?p.

**Yêu c?u:** Bearer Token

#### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `sessionId` | string | Yes | Session ID c?a guest cart |

#### Response Success (200)

```json
{
  "success": true,
  "message": "Carts merged successfully",
  "data": {
    // Merged cart data
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
```

#### Frontend Integration

```javascript
// Merge carts after login
const mergeCartsAfterLogin = async () => {
  try {
    const guestSessionId = localStorage.getItem('guestSessionId');
    if (!guestSessionId) return;

    const response = await fetch(`${process.env.REACT_APP_API_URL}/api/cart/merge?sessionId=${guestSessionId}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      }
    });

    const result = await response.json();
    
    if (result.success) {
      // Clear guest session after successful merge
      localStorage.removeItem('guestSessionId');
      showSuccess('?ã g?p gi? hàng thành công');
      return result.data;
    }
  } catch (error) {
    console.error('Merge carts error:', error);
    // Don't show error to user, it's not critical
  }
};

// Call this after successful login
const handleLoginSuccess = async (authData) => {
  // Save auth data
  localStorage.setItem('accessToken', authData.token);
  localStorage.setItem('user', JSON.stringify(authData.user));
  
  // Merge guest cart if exists
  await mergeCartsAfterLogin();
  
  // Redirect or update UI
  navigate('/dashboard');
};
```

---

## ?? Áp d?ng coupon

### POST `/coupon/{couponCode}`

Áp d?ng mã gi?m giá cho gi? hàng.

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `couponCode` | string | Mã coupon |

#### Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `sessionId` | string | Session ID cho guest cart |

#### Response Success (200)

```json
{
  "success": true,
  "message": "Coupon applied successfully",
  "timestamp": "2024-01-15T15:00:00Z"
}
```

#### Response Error (400)

```json
{
  "success": false,
  "message": "Invalid coupon code",
  "errors": [
    "Coupon has expired",
    "Minimum order amount not met"
  ],
  "timestamp": "2024-01-15T15:00:00Z"
}
```

#### Frontend Integration

```javascript
// Apply coupon function
const applyCoupon = async (couponCode) => {
  try {
    const cartService = new CartService();
    const sessionId = user ? null : cartService.getSessionId();
    
    const url = new URL(`${process.env.REACT_APP_API_URL}/api/cart/coupon/${couponCode}`);
    if (sessionId) {
      url.searchParams.append('sessionId', sessionId);
    }

    const headers = { 'Content-Type': 'application/json' };
    const token = localStorage.getItem('accessToken');
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }

    const response = await fetch(url, {
      method: 'POST',
      headers
    });

    const result = await response.json();
    
    if (result.success) {
      showSuccess('?ã áp d?ng mã gi?m giá thành công');
      return true;
    } else {
      showError(result.message);
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Apply coupon error:', error);
    throw error;
  }
};

// Coupon input component
const CouponInput = ({ onApplied }) => {
  const [couponCode, setCouponCode] = useState('');
  const [applying, setApplying] = useState(false);

  const handleApply = async (e) => {
    e.preventDefault();
    if (!couponCode.trim()) return;

    try {
      setApplying(true);
      await applyCoupon(couponCode.trim());
      setCouponCode('');
      onApplied && onApplied();
    } catch (error) {
      // Error already handled
    } finally {
      setApplying(false);
    }
  };

  return (
    <form onSubmit={handleApply} className="coupon-form">
      <input
        type="text"
        placeholder="Nh?p mã gi?m giá"
        value={couponCode}
        onChange={(e) => setCouponCode(e.target.value)}
        disabled={applying}
      />
      <button
        type="submit"
        disabled={applying || !couponCode.trim()}
      >
        {applying ? '?ang áp d?ng...' : 'Áp d?ng'}
      </button>
    </form>
  );
};
```

---

## ? Xóa coupon

### DELETE `/coupon`

Xóa mã gi?m giá kh?i gi? hàng.

#### Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `sessionId` | string | Session ID cho guest cart |

#### Response Success (200)

```json
{
  "success": true,
  "message": "Coupon removed successfully",
  "timestamp": "2024-01-15T15:00:00Z"
}
```

---

## ??? Complete Cart Management Hook

```javascript
// Complete cart hook with all functionalities
export const useCartManagement = () => {
  const [cart, setCart] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const { user } = useAuth();

  const cartService = new CartService();

  // Fetch cart data
  const fetchCart = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const sessionId = user ? null : cartService.getSessionId();
      const cartData = await cartService.getCart(sessionId);
      
      setCart(cartData);
    } catch (err) {
      setError(err.message);
      setCart({
        items: [],
        summary: { itemCount: 0, totalItems: 0, subtotal: 0, total: 0 }
      });
    } finally {
      setLoading(false);
    }
  }, [user, cartService]);

  // Add item to cart
  const addItem = async (productId, quantity = 1, customOptions = {}) => {
    try {
      await addToCart(productId, quantity, customOptions);
      await fetchCart();
    } catch (error) {
      throw error;
    }
  };

  // Update item quantity
  const updateItem = async (productId, quantity) => {
    try {
      await updateCartItem(productId, quantity);
      await fetchCart();
    } catch (error) {
      throw error;
    }
  };

  // Remove item
  const removeItem = async (productId) => {
    try {
      await removeFromCart(productId);
      await fetchCart();
    } catch (error) {
      throw error;
    }
  };

  // Clear cart
  const clearAll = async () => {
    try {
      await clearCart();
      await fetchCart();
    } catch (error) {
      throw error;
    }
  };

  // Apply coupon
  const applyCouponCode = async (couponCode) => {
    try {
      await applyCoupon(couponCode);
      await fetchCart();
    } catch (error) {
      throw error;
    }
  };

  // Remove coupon
  const removeCoupon = async () => {
    try {
      await removeCouponFromCart();
      await fetchCart();
    } catch (error) {
      throw error;
    }
  };

  // Initial load
  useEffect(() => {
    fetchCart();
  }, [fetchCart]);

  return {
    cart,
    loading,
    error,
    actions: {
      addItem,
      updateItem,
      removeItem,
      clearAll,
      applyCouponCode,
      removeCoupon,
      refresh: fetchCart
    }
  };
};
```

---

## ?? Complete Cart Component Example

```javascript
const ShoppingCart = () => {
  const { cart, loading, error, actions } = useCartManagement();
  const navigate = useNavigate();

  if (loading) return <CartSkeleton />;
  if (error) return <div className="error">Error: {error}</div>;
  if (!cart?.items?.length) return <EmptyCart />;

  return (
    <div className="shopping-cart">
      <h2>Gi? hàng c?a b?n</h2>
      
      <div className="cart-items">
        {cart.items.map(item => (
          <div key={item.id} className="cart-item">
            <img src={item.product.mainImage} alt={item.product.name} />
            
            <div className="item-details">
              <h3>{item.product.name}</h3>
              <p className="brand">{item.product.brand?.name}</p>
              <p className="price">{item.unitPrice.toLocaleString('vi-VN')}?</p>
            </div>
            
            <QuantitySelector
              item={item}
              onUpdate={actions.refresh}
            />
            
            <div className="item-total">
              {item.totalPrice.toLocaleString('vi-VN')}?
            </div>
            
            <RemoveFromCartButton
              productId={item.productId}
              onRemove={actions.refresh}
            />
          </div>
        ))}
      </div>
      
      <div className="cart-summary">
        <div className="coupon-section">
          <CouponInput onApplied={actions.refresh} />
          {cart.coupon && (
            <div className="applied-coupon">
              <span>Mã: {cart.coupon.code}</span>
              <span>Gi?m: {cart.coupon.discountAmount.toLocaleString('vi-VN')}?</span>
              <button onClick={actions.removeCoupon}>Xóa</button>
            </div>
          )}
        </div>
        
        <div className="summary-details">
          <div className="line">
            <span>T?m tính:</span>
            <span>{cart.summary.subtotal.toLocaleString('vi-VN')}?</span>
          </div>
          
          {cart.summary.discount > 0 && (
            <div className="line discount">
              <span>Gi?m giá:</span>
              <span>-{cart.summary.discount.toLocaleString('vi-VN')}?</span>
            </div>
          )}
          
          <div className="line shipping">
            <span>Phí v?n chuy?n:</span>
            <span>{cart.summary.shippingFee?.toLocaleString('vi-VN') || 'Mi?n phí'}?</span>
          </div>
          
          <div className="line total">
            <span>T?ng c?ng:</span>
            <span>{cart.summary.total.toLocaleString('vi-VN')}?</span>
          </div>
        </div>
        
        <div className="cart-actions">
          <button onClick={() => navigate('/products')} className="continue-shopping">
            Ti?p t?c mua s?m
          </button>
          
          <button onClick={() => navigate('/checkout')} className="checkout-btn">
            Thanh toán
          </button>
          
          <ClearCartButton onClear={actions.refresh} />
        </div>
      </div>
    </div>
  );
};
```

---

## ?? Mobile-Optimized Cart Drawer

```javascript
const CartDrawer = ({ isOpen, onClose }) => {
  const { cart, actions } = useCartManagement();

  return (
    <div className={`cart-drawer ${isOpen ? 'open' : ''}`}>
      <div className="drawer-overlay" onClick={onClose} />
      
      <div className="drawer-content">
        <div className="drawer-header">
          <h3>Gi? hàng ({cart?.summary?.itemCount || 0})</h3>
          <button onClick={onClose}>?</button>
        </div>
        
        <div className="drawer-body">
          {cart?.items?.length ? (
            <>
              <div className="cart-items-mini">
                {cart.items.map(item => (
                  <div key={item.id} className="cart-item-mini">
                    <img src={item.product.mainImage} alt={item.product.name} />
                    <div className="item-info">
                      <span className="name">{item.product.name}</span>
                      <span className="price">{item.totalPrice.toLocaleString('vi-VN')}?</span>
                      <span className="qty">S? l??ng: {item.quantity}</span>
                    </div>
                  </div>
                ))}
              </div>
              
              <div className="drawer-summary">
                <div className="total">
                  T?ng: {cart.summary.total.toLocaleString('vi-VN')}?
                </div>
              </div>
            </>
          ) : (
            <div className="empty-cart">
              <p>Gi? hàng tr?ng</p>
            </div>
          )}
        </div>
        
        <div className="drawer-footer">
          <button onClick={() => navigate('/cart')} className="view-cart">
            Xem gi? hàng
          </button>
          <button onClick={() => navigate('/checkout')} className="checkout">
            Thanh toán
          </button>
        </div>
      </div>
    </div>
  );
};
```

---

Tài li?u này cung c?p ??y ?? thông tin ?? frontend team tích h?p v?i Shopping Cart API c?a SakuraHome. API h? tr? c? ng??i dùng ?ã ??ng nh?p và guest users, v?i ??y ?? tính n?ng qu?n lý gi? hàng hi?n ??i.