# ?? Order Management API Documentation

## T?ng quan

API Order Management c?a SakuraHome cung c?p ??y ?? các ch?c n?ng qu?n lý ??n hàng, t? t?o ??n hàng, theo dõi tr?ng thái, ??n x? lý tr? hàng và hoàn ti?n.

**Base URL:** `https://localhost:7240/api/order`

## ?? M?c l?c

### ?? Customer Endpoints
- [T?o ??n hàng](#t?o-??n-hàng)
- [L?y danh sách ??n hàng](#l?y-danh-sách-??n-hàng)
- [L?y chi ti?t ??n hàng](#l?y-chi-ti?t-??n-hàng)
- [C?p nh?t ??n hàng](#c?p-nh?t-??n-hàng)
- [H?y ??n hàng](#h?y-??n-hàng)
- [L?ch s? tr?ng thái](#l?ch-s?-tr?ng-thái)
- [Thêm ghi chú](#thêm-ghi-chú)
- [L?y ghi chú ??n hàng](#l?y-ghi-chú-??n-hàng)
- [Xác th?c ??n hàng](#xác-th?c-??n-hàng)
- [Tính t?ng ti?n](#tính-t?ng-ti?n)
- [Yêu c?u tr? hàng](#yêu-c?u-tr?-hàng)

### ????? Staff Endpoints
- [Xác nh?n ??n hàng](#xác-nh?n-??n-hàng)
- [X? lý ??n hàng](#x?-lý-??n-hàng)
- [Giao hàng](#giao-hàng)
- [?ánh d?u ?ã giao](#?ánh-d?u-?ã-giao)
- [C?p nh?t tr?ng thái](#c?p-nh?t-tr?ng-thái)
- [Thêm ghi chú staff](#thêm-ghi-chú-staff)
- [X? lý tr? hàng](#x?-lý-tr?-hàng)
- [Th?ng kê ??n hàng](#th?ng-kê-??n-hàng)
- [??n hàng g?n ?ây](#??n-hàng-g?n-?ây)

---

## ?? Customer Endpoints

## ? T?o ??n hàng

### POST `/`

T?o ??n hàng m?i t? gi? hàng.

**Yêu c?u:** Bearer Token

#### Request Body

```json
{
  "shippingAddressId": 1,
  "billingAddressId": 1,
  "paymentMethod": "VNPay",
  "shippingMethod": "Standard",
  "couponCode": "SAVE10",
  "notes": "Giao hàng vào bu?i chi?u",
  "items": [
    {
      "productId": 101,
      "quantity": 2,
      "unitPrice": 350000.00,
      "customOptions": {
        "giftWrap": true,
        "giftMessage": "Chúc m?ng sinh nh?t!"
      }
    },
    {
      "productId": 102,
      "quantity": 1,
      "unitPrice": 180000.00
    }
  ],
  "useRewardPoints": true,
  "rewardPointsAmount": 50000.00
}
```

#### Response Success (201)

```json
{
  "success": true,
  "message": "Order created successfully",
  "data": {
    "id": 1001,
    "orderNumber": "ORD-2024-001001",
    "status": "Pending",
    "totalAmount": 880000.00,
    "createdAt": "2024-01-15T15:00:00Z",
    "estimatedDelivery": "2024-01-20T17:00:00Z",
    "customer": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fullName": "Nguy?n V?n A",
      "email": "customer@example.com",
      "phoneNumber": "+84901234567"
    },
    "items": [
      {
        "id": 1,
        "productId": 101,
        "productName": "Sake Dassai 39 Junmai Daiginjo",
        "productImage": "https://example.com/images/dassai-39.jpg",
        "quantity": 2,
        "unitPrice": 350000.00,
        "totalPrice": 700000.00,
        "customOptions": {
          "giftWrap": true,
          "giftMessage": "Chúc m?ng sinh nh?t!"
        }
      },
      {
        "id": 2,
        "productId": 102,
        "productName": "Kimono Traditional Blue",
        "productImage": "https://example.com/images/kimono-blue.jpg",
        "quantity": 1,
        "unitPrice": 180000.00,
        "totalPrice": 180000.00
      }
    ],
    "shippingAddress": {
      "fullName": "Nguy?n V?n A",
      "phoneNumber": "+84901234567",
      "addressLine1": "123 ???ng ABC",
      "addressLine2": "T?ng 4",
      "ward": "Ph??ng XYZ",
      "district": "Qu?n 1",
      "city": "TP. H? Chí Minh",
      "postalCode": "70000",
      "country": "Vietnam"
    },
    "billingAddress": {
      // Same structure as shipping address
    },
    "payment": {
      "method": "VNPay",
      "status": "Pending",
      "amount": 880000.00
    },
    "shipping": {
      "method": "Standard",
      "fee": 30000.00,
      "estimatedDelivery": "2024-01-20T17:00:00Z"
    },
    "summary": {
      "subtotal": 880000.00,
      "shippingFee": 30000.00,
      "discount": 88000.00,
      "rewardPointsDiscount": 50000.00,
      "tax": 0.00,
      "total": 772000.00
    },
    "coupon": {
      "code": "SAVE10",
      "discountAmount": 88000.00
    },
    "notes": "Giao hàng vào bu?i chi?u"
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
```

#### Frontend Integration

```javascript
// Order service
class OrderService {
  constructor() {
    this.baseURL = process.env.REACT_APP_API_URL;
  }

  async createOrder(orderData) {
    try {
      const response = await fetch(`${this.baseURL}/api/order`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
        },
        body: JSON.stringify(orderData)
      });

      const result = await response.json();
      
      if (result.success) {
        return result.data;
      } else {
        throw new Error(result.message);
      }
    } catch (error) {
      console.error('Create order error:', error);
      throw error;
    }
  }
}

// React checkout component
const CheckoutPage = () => {
  const [loading, setLoading] = useState(false);
  const [orderData, setOrderData] = useState({
    shippingAddressId: null,
    billingAddressId: null,
    paymentMethod: 'VNPay',
    shippingMethod: 'Standard',
    couponCode: '',
    notes: '',
    useRewardPoints: false,
    rewardPointsAmount: 0
  });

  const { cart } = useCart();
  const navigate = useNavigate();
  const orderService = new OrderService();

  const handleCreateOrder = async () => {
    try {
      setLoading(true);
      
      // Prepare order items from cart
      const items = cart.items.map(item => ({
        productId: item.productId,
        quantity: item.quantity,
        unitPrice: item.unitPrice,
        customOptions: item.customOptions
      }));

      const order = await orderService.createOrder({
        ...orderData,
        items
      });

      // Redirect to payment or order confirmation
      navigate(`/order/${order.id}/payment`);
    } catch (error) {
      showError('Không th? t?o ??n hàng: ' + error.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="checkout-page">
      <div className="checkout-form">
        {/* Address selection */}
        <AddressSelector
          value={orderData.shippingAddressId}
          onChange={(id) => setOrderData(prev => ({ ...prev, shippingAddressId: id }))}
        />

        {/* Payment method */}
        <PaymentMethodSelector
          value={orderData.paymentMethod}
          onChange={(method) => setOrderData(prev => ({ ...prev, paymentMethod: method }))}
        />

        {/* Shipping method */}
        <ShippingMethodSelector
          value={orderData.shippingMethod}
          onChange={(method) => setOrderData(prev => ({ ...prev, shippingMethod: method }))}
        />

        {/* Order notes */}
        <textarea
          placeholder="Ghi chú ??n hàng"
          value={orderData.notes}
          onChange={(e) => setOrderData(prev => ({ ...prev, notes: e.target.value }))}
        />

        {/* Reward points */}
        <RewardPointsSelector
          value={orderData.useRewardPoints}
          amount={orderData.rewardPointsAmount}
          onChange={(use, amount) => setOrderData(prev => ({ 
            ...prev, 
            useRewardPoints: use,
            rewardPointsAmount: amount 
          }))}
        />
      </div>

      <div className="order-summary">
        <OrderSummary cart={cart} orderData={orderData} />
        
        <button
          onClick={handleCreateOrder}
          disabled={loading || !orderData.shippingAddressId}
          className="create-order-btn"
        >
          {loading ? '?ang t?o ??n hàng...' : '??t hàng'}
        </button>
      </div>
    </div>
  );
};
```

---

## ?? L?y danh sách ??n hàng

### GET `/`

L?y danh sách ??n hàng c?a ng??i dùng v?i b? l?c.

**Yêu c?u:** Bearer Token

#### Query Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | integer | 1 | Trang hi?n t?i |
| `pageSize` | integer | 20 | S? ??n hàng trên m?i trang |
| `status` | string | - | L?c theo tr?ng thái |
| `fromDate` | datetime | - | T? ngày |
| `toDate` | datetime | - | ??n ngày |
| `search` | string | - | Tìm ki?m theo s? ??n hàng |

#### Response Success (200)

```json
{
  "success": true,
  "message": "Orders retrieved successfully",
  "data": [
    {
      "id": 1001,
      "orderNumber": "ORD-2024-001001",
      "status": "Processing",
      "statusDisplay": "?ang x? lý",
      "totalAmount": 772000.00,
      "itemCount": 3,
      "createdAt": "2024-01-15T15:00:00Z",
      "estimatedDelivery": "2024-01-20T17:00:00Z",
      "canCancel": true,
      "canReturn": false,
      "items": [
        {
          "productName": "Sake Dassai 39 Junmai Daiginjo",
          "productImage": "https://example.com/images/dassai-39.jpg",
          "quantity": 2
        }
      ]
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 15,
    "totalPages": 1,
    "hasNext": false,
    "hasPrevious": false
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
```

#### Frontend Integration

```javascript
// Hook for user orders
export const useUserOrders = (filters = {}) => {
  const [orders, setOrders] = useState([]);
  const [pagination, setPagination] = useState({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const fetchOrders = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const queryParams = new URLSearchParams();
      Object.entries(filters).forEach(([key, value]) => {
        if (value) queryParams.append(key, value);
      });

      const response = await fetch(`${process.env.REACT_APP_API_URL}/api/order?${queryParams}`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
        }
      });

      const result = await response.json();
      
      if (result.success) {
        setOrders(result.data);
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
    fetchOrders();
  }, [fetchOrders]);

  return { orders, pagination, loading, error, refetch: fetchOrders };
};

// Orders list component
const OrdersList = () => {
  const [filters, setFilters] = useState({
    page: 1,
    pageSize: 10,
    status: '',
    search: ''
  });

  const { orders, pagination, loading, error } = useUserOrders(filters);

  if (loading) return <OrdersSkeleton />;
  if (error) return <div className="error">Error: {error}</div>;

  return (
    <div className="orders-list">
      <div className="orders-filters">
        <select
          value={filters.status}
          onChange={(e) => setFilters(prev => ({ ...prev, status: e.target.value, page: 1 }))}
        >
          <option value="">T?t c? tr?ng thái</option>
          <option value="Pending">Ch? x? lý</option>
          <option value="Processing">?ang x? lý</option>
          <option value="Shipped">?ã giao</option>
          <option value="Delivered">?ã nh?n</option>
          <option value="Cancelled">?ã h?y</option>
        </select>

        <input
          type="text"
          placeholder="Tìm ki?m s? ??n hàng..."
          value={filters.search}
          onChange={(e) => setFilters(prev => ({ ...prev, search: e.target.value, page: 1 }))}
        />
      </div>

      <div className="orders-grid">
        {orders.map(order => (
          <OrderCard key={order.id} order={order} />
        ))}
      </div>

      <Pagination
        pagination={pagination}
        onPageChange={(page) => setFilters(prev => ({ ...prev, page }))}
      />
    </div>
  );
};
```

---

## ?? L?y chi ti?t ??n hàng

### GET `/{orderId}`

L?y thông tin chi ti?t c?a m?t ??n hàng c? th?.

**Yêu c?u:** Bearer Token

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `orderId` | integer | ID c?a ??n hàng |

#### Response Success (200)

```json
{
  "success": true,
  "message": "Order retrieved successfully",
  "data": {
    "id": 1001,
    "orderNumber": "ORD-2024-001001",
    "status": "Processing",
    "statusDisplay": "?ang x? lý",
    "totalAmount": 772000.00,
    "createdAt": "2024-01-15T15:00:00Z",
    "updatedAt": "2024-01-15T16:30:00Z",
    "estimatedDelivery": "2024-01-20T17:00:00Z",
    "canCancel": true,
    "canReturn": false,
    "customer": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fullName": "Nguy?n V?n A",
      "email": "customer@example.com",
      "phoneNumber": "+84901234567"
    },
    "items": [
      {
        "id": 1,
        "productId": 101,
        "productName": "Sake Dassai 39 Junmai Daiginjo",
        "productImage": "https://example.com/images/dassai-39.jpg",
        "productSlug": "sake-dassai-39-junmai-daiginjo",
        "quantity": 2,
        "unitPrice": 350000.00,
        "totalPrice": 700000.00,
        "customOptions": {
          "giftWrap": true,
          "giftMessage": "Chúc m?ng sinh nh?t!"
        },
        "canReview": false
      }
    ],
    "shippingAddress": {
      "fullName": "Nguy?n V?n A",
      "phoneNumber": "+84901234567",
      "addressLine1": "123 ???ng ABC",
      "addressLine2": "T?ng 4",
      "ward": "Ph??ng XYZ",
      "district": "Qu?n 1",
      "city": "TP. H? Chí Minh",
      "postalCode": "70000",
      "country": "Vietnam"
    },
    "billingAddress": {
      // Same structure
    },
    "payment": {
      "method": "VNPay",
      "methodDisplay": "VNPay",
      "status": "Completed",
      "statusDisplay": "?ã thanh toán",
      "amount": 772000.00,
      "transactionId": "VNP_TXN_123456789",
      "paidAt": "2024-01-15T15:05:00Z"
    },
    "shipping": {
      "method": "Standard",
      "methodDisplay": "Giao hàng tiêu chu?n",
      "fee": 30000.00,
      "trackingNumber": "TRACK123456789",
      "estimatedDelivery": "2024-01-20T17:00:00Z",
      "actualDelivery": null
    },
    "summary": {
      "subtotal": 880000.00,
      "shippingFee": 30000.00,
      "discount": 88000.00,
      "rewardPointsDiscount": 50000.00,
      "tax": 0.00,
      "total": 772000.00
    },
    "coupon": {
      "code": "SAVE10",
      "discountAmount": 88000.00
    },
    "notes": "Giao hàng vào bu?i chi?u",
    "staffNotes": [],
    "statusHistory": [
      {
        "status": "Pending",
        "statusDisplay": "Ch? x? lý",
        "createdAt": "2024-01-15T15:00:00Z",
        "notes": "??n hàng ?ã ???c t?o"
      },
      {
        "status": "Processing",
        "statusDisplay": "?ang x? lý",
        "createdAt": "2024-01-15T16:30:00Z",
        "notes": "??n hàng ?ang ???c chu?n b?"
      }
    ]
  },
  "timestamp": "2024-01-15T17:00:00Z"
}
```

#### Frontend Integration

```javascript
// Order detail component
const OrderDetail = ({ orderId }) => {
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchOrder = async () => {
      try {
        setLoading(true);
        setError(null);

        const response = await fetch(`${process.env.REACT_APP_API_URL}/api/order/${orderId}`, {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
          }
        });

        const result = await response.json();
        
        if (result.success) {
          setOrder(result.data);
        } else {
          setError(result.message);
        }
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchOrder();
  }, [orderId]);

  if (loading) return <OrderDetailSkeleton />;
  if (error) return <div className="error">Error: {error}</div>;
  if (!order) return <div>Không tìm th?y ??n hàng</div>;

  return (
    <div className="order-detail">
      <div className="order-header">
        <h1>??n hàng #{order.orderNumber}</h1>
        <OrderStatusBadge status={order.status} />
        
        <div className="order-meta">
          <span>??t ngày: {formatDate(order.createdAt)}</span>
          <span>T?ng ti?n: {order.totalAmount.toLocaleString('vi-VN')}?</span>
        </div>
      </div>

      <div className="order-content">
        <div className="order-items">
          <h3>S?n ph?m ?ã ??t</h3>
          {order.items.map(item => (
            <OrderItemCard key={item.id} item={item} />
          ))}
        </div>

        <div className="order-summary">
          <h3>Tóm t?t ??n hàng</h3>
          <OrderSummaryDisplay summary={order.summary} />
        </div>

        <div className="shipping-info">
          <h3>Thông tin giao hàng</h3>
          <AddressDisplay address={order.shippingAddress} />
          {order.shipping.trackingNumber && (
            <div className="tracking">
              <span>Mã v?n ??n: {order.shipping.trackingNumber}</span>
              <button onClick={() => trackShipment(order.shipping.trackingNumber)}>
                Theo dõi
              </button>
            </div>
          )}
        </div>

        <div className="payment-info">
          <h3>Thông tin thanh toán</h3>
          <PaymentDisplay payment={order.payment} />
        </div>

        <div className="order-timeline">
          <h3>L?ch s? ??n hàng</h3>
          <OrderTimeline history={order.statusHistory} />
        </div>
      </div>

      <div className="order-actions">
        {order.canCancel && (
          <CancelOrderButton orderId={order.id} />
        )}
        {order.canReturn && (
          <ReturnOrderButton orderId={order.id} />
        )}
        <button onClick={() => downloadInvoice(order.id)}>
          T?i hóa ??n
        </button>
      </div>
    </div>
  );
};
```

---

## ?? C?p nh?t ??n hàng

### PUT `/{orderId}`

C?p nh?t thông tin ??n hàng (ch? cho ??n hàng Pending).

**Yêu c?u:** Bearer Token

#### Request Body

```json
{
  "shippingAddressId": 2,
  "billingAddressId": 2,
  "notes": "C?p nh?t: Giao hàng vào bu?i sáng"
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "Order updated successfully",
  "data": {
    // Updated order details
  },
  "timestamp": "2024-01-15T17:00:00Z"
}
```

---

## ? H?y ??n hàng

### DELETE `/{orderId}`

H?y ??n hàng (ch? cho ??n hàng Pending ho?c Processing).

**Yêu c?u:** Bearer Token

#### Request Body

```json
{
  "reason": "??i ý không mu?n mua n?a"
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "Order cancelled successfully",
  "timestamp": "2024-01-15T17:00:00Z"
}
```

#### Frontend Integration

```javascript
// Cancel order function
const cancelOrder = async (orderId, reason) => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_URL}/api/order/${orderId}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      },
      body: JSON.stringify({ reason })
    });

    const result = await response.json();
    
    if (result.success) {
      showSuccess('?ã h?y ??n hàng thành công');
      return true;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Cancel order error:', error);
    showError('Không th? h?y ??n hàng: ' + error.message);
    throw error;
  }
};

// Cancel order button component
const CancelOrderButton = ({ orderId, onCancelled }) => {
  const [cancelling, setCancelling] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [reason, setReason] = useState('');

  const handleCancel = async () => {
    if (!reason.trim()) {
      showError('Vui lòng nh?p lý do h?y ??n hàng');
      return;
    }

    try {
      setCancelling(true);
      await cancelOrder(orderId, reason);
      setShowModal(false);
      onCancelled && onCancelled();
    } catch (error) {
      // Error already handled
    } finally {
      setCancelling(false);
    }
  };

  return (
    <>
      <button 
        onClick={() => setShowModal(true)}
        className="cancel-order-btn"
      >
        H?y ??n hàng
      </button>

      {showModal && (
        <Modal onClose={() => setShowModal(false)}>
          <div className="cancel-order-modal">
            <h3>H?y ??n hàng</h3>
            <p>B?n có ch?c mu?n h?y ??n hàng này?</p>
            
            <textarea
              placeholder="Nh?p lý do h?y ??n hàng..."
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              required
            />
            
            <div className="modal-actions">
              <button onClick={() => setShowModal(false)}>
                Không
              </button>
              <button 
                onClick={handleCancel}
                disabled={cancelling || !reason.trim()}
                className="confirm-cancel"
              >
                {cancelling ? '?ang h?y...' : 'Xác nh?n h?y'}
              </button>
            </div>
          </div>
        </Modal>
      )}
    </>
  );
};
```

---

## ?? L?ch s? tr?ng thái

### GET `/{orderId}/status-history`

L?y l?ch s? thay ??i tr?ng thái c?a ??n hàng.

**Yêu c?u:** Bearer Token

#### Response Success (200)

```json
{
  "success": true,
  "message": "Order status history retrieved successfully",
  "data": [
    {
      "status": "Pending",
      "statusDisplay": "Ch? x? lý",
      "notes": "??n hàng ?ã ???c t?o",
      "createdAt": "2024-01-15T15:00:00Z",
      "createdBy": null
    },
    {
      "status": "Processing",
      "statusDisplay": "?ang x? lý",
      "notes": "??n hàng ?ang ???c chu?n b?",
      "createdAt": "2024-01-15T16:30:00Z",
      "createdBy": "staff@sakurahome.com"
    }
  ],
  "timestamp": "2024-01-15T17:00:00Z"
}
```

---

## ?? Thêm ghi chú

### POST `/{orderId}/notes`

Thêm ghi chú vào ??n hàng.

**Yêu c?u:** Bearer Token

#### Request Body

```json
{
  "note": "Khách hàng yêu c?u g?i ?i?n tr??c khi giao"
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "Note added successfully",
  "data": {
    // Updated order details
  },
  "timestamp": "2024-01-15T17:00:00Z"
}
```

---

## ?? L?y ghi chú ??n hàng

### GET `/{orderId}/notes`

L?y danh sách ghi chú c?a ??n hàng.

**Yêu c?u:** Bearer Token

#### Response Success (200)

```json
{
  "success": true,
  "message": "Order notes retrieved successfully",
  "data": [
    {
      "id": 1,
      "note": "Khách hàng yêu c?u g?i ?i?n tr??c khi giao",
      "isCustomerVisible": true,
      "createdAt": "2024-01-15T17:00:00Z",
      "createdBy": "customer@example.com"
    }
  ],
  "timestamp": "2024-01-15T17:00:00Z"
}
```

---

## ? Xác th?c ??n hàng

### POST `/validate`

Xác th?c thông tin ??n hàng tr??c khi t?o.

**Yêu c?u:** Bearer Token

#### Request Body

```json
{
  "items": [
    {
      "productId": 101,
      "quantity": 2
    }
  ],
  "shippingAddressId": 1,
  "couponCode": "SAVE10"
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "Order validation successful",
  "data": {
    "isValid": true,
    "warnings": [],
    "estimates": {
      "subtotal": 700000.00,
      "shippingFee": 30000.00,
      "discount": 70000.00,
      "total": 660000.00
    }
  },
  "timestamp": "2024-01-15T17:00:00Z"
}
```

#### Response Error (400)

```json
{
  "success": false,
  "message": "Order validation failed",
  "data": {
    "isValid": false,
    "errors": [
      "Product 'Sake Dassai 39' is out of stock",
      "Coupon 'SAVE10' has expired"
    ],
    "warnings": [
      "Product price has changed"
    ]
  },
  "timestamp": "2024-01-15T17:00:00Z"
}
```

---

## ?? Tính t?ng ti?n

### POST `/calculate-total`

Tính t?ng ti?n ??n hàng v?i các tùy ch?n.

#### Request Body

```json
{
  "items": [
    {
      "productId": 101,
      "quantity": 2,
      "unitPrice": 350000.00
    }
  ],
  "shippingAddressId": 1,
  "couponCode": "SAVE10"
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "Order total calculated successfully",
  "data": 660000.00,
  "timestamp": "2024-01-15T17:00:00Z"
}
```

---

## ?? Yêu c?u tr? hàng

### POST `/{orderId}/return`

Yêu c?u tr? hàng cho ??n hàng ?ã giao.

**Yêu c?u:** Bearer Token

#### Request Body

```json
{
  "items": [
    {
      "orderItemId": 1,
      "quantity": 1,
      "reason": "S?n ph?m b? l?i",
      "description": "Chai sake b? n?t"
    }
  ],
  "returnMethod": "Exchange",
  "notes": "Mong mu?n ??i s?n ph?m khác"
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "Return request submitted successfully",
  "data": {
    // Updated order with return request
  },
  "timestamp": "2024-01-15T17:00:00Z"
}
```

---

## ????? Staff Endpoints

## ? Xác nh?n ??n hàng

### PATCH `/{orderId}/confirm`

Xác nh?n ??n hàng (Staff only).

**Yêu c?u:** Bearer Token v?i role Staff

#### Response Success (200)

```json
{
  "success": true,
  "message": "Order confirmed successfully",
  "data": {
    // Updated order details
  },
  "timestamp": "2024-01-15T17:00:00Z"
}
```

---

## ?? X? lý ??n hàng

### PATCH `/{orderId}/process`

Chuy?n ??n hàng sang tr?ng thái Processing (Staff only).

**Yêu c?u:** Bearer Token v?i role Staff

#### Request Body

```json
{
  "notes": "?ã chu?n b? hàng xong, s?n sàng giao",
  "estimatedShippingDate": "2024-01-18T10:00:00Z"
}
```

---

## ?? Giao hàng

### PATCH `/{orderId}/ship`

Giao hàng và c?p nh?t thông tin v?n chuy?n (Staff only).

**Yêu c?u:** Bearer Token v?i role Staff

#### Request Body

```json
{
  "trackingNumber": "TRACK123456789",
  "shippingCarrier": "Giao Hàng Nhanh",
  "estimatedDelivery": "2024-01-20T17:00:00Z",
  "notes": "?ã giao cho ??n v? v?n chuy?n"
}
```

---

## ?? ?ánh d?u ?ã giao

### PATCH `/{orderId}/deliver`

?ánh d?u ??n hàng ?ã ???c giao thành công (Staff only).

**Yêu c?u:** Bearer Token v?i role Staff

#### Response Success (200)

```json
{
  "success": true,
  "message": "Order marked as delivered successfully",
  "data": {
    // Updated order details
  },
  "timestamp": "2024-01-15T17:00:00Z"
}
```

---

## ??? C?p nh?t tr?ng thái

### PATCH `/{orderId}/status`

C?p nh?t tr?ng thái ??n hàng (Staff only).

**Yêu c?u:** Bearer Token v?i role Staff

#### Request Body

```json
{
  "status": "Shipped",
  "notes": "?ã giao cho ??n v? v?n chuy?n"
}
```

---

## ?? Thêm ghi chú staff

### POST `/{orderId}/staff-notes`

Thêm ghi chú staff cho ??n hàng (Staff only).

**Yêu c?u:** Bearer Token v?i role Staff

#### Request Body

```json
{
  "note": "Khách hàng yêu c?u g?i ?i?n xác nh?n tr??c khi giao",
  "isCustomerVisible": false
}
```

---

## ?? X? lý tr? hàng

### PATCH `/{orderId}/return`

X? lý yêu c?u tr? hàng (Staff only).

**Yêu c?u:** Bearer Token v?i role Staff

#### Request Body

```json
{
  "action": "Approve",
  "refundAmount": 350000.00,
  "refundMethod": "Original",
  "notes": "Ch?p nh?n tr? hàng do s?n ph?m l?i"
}
```

---

## ?? Th?ng kê ??n hàng

### GET `/stats`

L?y th?ng kê ??n hàng (Staff only).

**Yêu c?u:** Bearer Token v?i role Staff

#### Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `fromDate` | datetime | T? ngày |
| `toDate` | datetime | ??n ngày |

#### Response Success (200)

```json
{
  "success": true,
  "message": "Order statistics retrieved successfully",
  "data": {
    "totalOrders": 1250,
    "totalRevenue": 125000000.00,
    "averageOrderValue": 100000.00,
    "ordersByStatus": {
      "Pending": 45,
      "Processing": 78,
      "Shipped": 123,
      "Delivered": 980,
      "Cancelled": 24
    },
    "topProducts": [
      {
        "productId": 101,
        "productName": "Sake Dassai 39",
        "orderCount": 89,
        "revenue": 31150000.00
      }
    ],
    "dailyStats": [
      {
        "date": "2024-01-15",
        "orderCount": 45,
        "revenue": 5600000.00
      }
    ]
  },
  "timestamp": "2024-01-15T17:00:00Z"
}
```

---

## ?? ??n hàng g?n ?ây

### GET `/recent`

L?y danh sách ??n hàng g?n ?ây (Staff only).

**Yêu c?u:** Bearer Token v?i role Staff

#### Query Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `count` | integer | 10 | S? l??ng ??n hàng |

#### Response Success (200)

```json
{
  "success": true,
  "message": "Recent orders retrieved successfully",
  "data": [
    {
      "id": 1001,
      "orderNumber": "ORD-2024-001001",
      "customerName": "Nguy?n V?n A",
      "status": "Processing",
      "totalAmount": 772000.00,
      "createdAt": "2024-01-15T15:00:00Z"
    }
  ],
  "timestamp": "2024-01-15T17:00:00Z"
}
```

---

## ??? Complete Order Management Hook

```javascript
// Complete order management hook
export const useOrderManagement = () => {
  const orderService = new OrderService();

  const createOrder = async (orderData) => {
    return await orderService.createOrder(orderData);
  };

  const updateOrder = async (orderId, updateData) => {
    return await orderService.updateOrder(orderId, updateData);
  };

  const cancelOrder = async (orderId, reason) => {
    return await orderService.cancelOrder(orderId, reason);
  };

  const requestReturn = async (orderId, returnData) => {
    return await orderService.requestReturn(orderId, returnData);
  };

  const addNote = async (orderId, note) => {
    return await orderService.addNote(orderId, note);
  };

  const validateOrder = async (orderData) => {
    return await orderService.validateOrder(orderData);
  };

  const calculateTotal = async (items, shippingAddressId, couponCode) => {
    return await orderService.calculateTotal(items, shippingAddressId, couponCode);
  };

  return {
    createOrder,
    updateOrder,
    cancelOrder,
    requestReturn,
    addNote,
    validateOrder,
    calculateTotal
  };
};
```

---

## ?? Order Status Enums

```javascript
export const OrderStatus = {
  PENDING: 'Pending',
  CONFIRMED: 'Confirmed',
  PROCESSING: 'Processing',
  SHIPPED: 'Shipped',
  DELIVERED: 'Delivered',
  CANCELLED: 'Cancelled',
  RETURNED: 'Returned',
  REFUNDED: 'Refunded'
};

export const OrderStatusDisplayNames = {
  [OrderStatus.PENDING]: 'Ch? x? lý',
  [OrderStatus.CONFIRMED]: '?ã xác nh?n',
  [OrderStatus.PROCESSING]: '?ang x? lý',
  [OrderStatus.SHIPPED]: '?ang giao',
  [OrderStatus.DELIVERED]: '?ã giao',
  [OrderStatus.CANCELLED]: '?ã h?y',
  [OrderStatus.RETURNED]: '?ã tr?',
  [OrderStatus.REFUNDED]: '?ã hoàn ti?n'
};

export const PaymentMethod = {
  VNPAY: 'VNPay',
  MOMO: 'MoMo',
  BANK_TRANSFER: 'BankTransfer',
  COD: 'COD'
};

export const PaymentStatus = {
  PENDING: 'Pending',
  COMPLETED: 'Completed',
  FAILED: 'Failed',
  CANCELLED: 'Cancelled',
  REFUNDED: 'Refunded'
};
```

---

Tài li?u này cung c?p ??y ?? thông tin ?? frontend team tích h?p v?i Order Management API c?a SakuraHome. API h? tr? toàn b? quy trình t? t?o ??n hàng ??n x? lý tr? hàng và hoàn ti?n.