# 💰 Calculate Order Total API - Frontend Integration Guide

## 📋 Tổng quan

API **Calculate Order Total** cho phép tính toán tổng tiền đơn hàng bao gồm:
- ✅ Tổng tiền sản phẩm (Subtotal)
- ✅ Phí vận chuyển (Shipping Cost)
- ✅ Giảm giá từ mã coupon (Discount)
- ✅ Thuế (nếu có)
- ✅ Tổng cộng (Total Amount)

**Use cases:**
- Hiển thị giá trước khi checkout
- Tính toán real-time khi thay đổi số lượng
- Preview khi áp dụng coupon
- So sánh phí ship tiêu chuẩn vs nhanh

---

## 🔗 API Endpoint

### Base Information

| Property | Value |
|----------|-------|
| **Method** | `POST` |
| **Endpoint** | `/api/orders/calculate-total` |
| **Base URL** | `https://localhost:8080` (Dev) 
| **Authentication** | ❌ Not Required (Public endpoint) |
| **Content-Type** | `application/json` |

---

## 📤 Request Format

### TypeScript Interface

```typescript
interface CalculateOrderTotalRequest {
  items: OrderItem[];
  shippingAddressId?: number;  // Optional - for accurate shipping
  couponCode?: string;          // Optional - apply discount
  expressDelivery?: boolean;    // Optional - default: false
}

interface OrderItem {
  productId: number;
  productVariantId?: number;
  quantity: number;
  customOptions?: string;       // JSON string of custom options
}
```

### Example Request Body

```json
{
  "items": [
    {
      "productId": 1,
      "quantity": 2
    },
    {
      "productId": 3,
      "quantity": 1,
      "customOptions": "{\"giftWrap\": true}"
    }
  ],
  "shippingAddressId": 5,
  "couponCode": "SAVE10",
  "expressDelivery": true
}
```

---

## 📥 Response Format

### TypeScript Interface

```typescript
interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
  timestamp: string;
}

interface OrderTotalCalculation {
  subtotal: number;
  shippingCost: number;
  taxAmount: number;
  discountAmount: number;
  totalAmount: number;
  couponCode: string | null;
  isExpressDelivery: boolean;
  itemCount: number;
  estimatedDeliveryDays: number;
  savingsSummary: string;
  shippingMessage: string;
}
```

### Example Response Success (200)

```json
{
  "success": true,
  "message": "Order total calculated successfully",
  "data": {
    "subtotal": 750000,
    "shippingCost": 0,
    "taxAmount": 0,
    "discountAmount": 75000,
    "totalAmount": 675000,
    "couponCode": "SAVE10",
    "isExpressDelivery": false,
    "itemCount": 3,
    "estimatedDeliveryDays": 3,
    "savingsSummary": "Bạn tiết kiệm 75,000 VND",
    "shippingMessage": "✓ Miễn phí vận chuyển"
  },
  "timestamp": "2025-01-17T15:30:00Z"
}
```

### Example Response Error (400)

```json
{
  "success": false,
  "message": "No items provided for calculation",
  "errors": [],
  "timestamp": "2025-01-17T15:30:00Z"
}
```

---

## 💻 Implementation Examples

### 1. Vanilla JavaScript / Fetch API

```javascript
/**
 * Calculate order total using Fetch API
 */
async function calculateOrderTotal(items, options = {}) {
  const url = 'https://localhost:8080/api/orders/calculate-total';
  
  const requestBody = {
    items: items,
    shippingAddressId: options.addressId,
    couponCode: options.couponCode,
    expressDelivery: options.expressDelivery || false
  };

  try {
    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(requestBody)
    });

    const result = await response.json();

    if (!response.ok) {
      throw new Error(result.message || 'Failed to calculate order total');
    }

    return result.data;
  } catch (error) {
    console.error('Calculate order total error:', error);
    throw error;
  }
}

// Usage example
const cartItems = [
  { productId: 1, quantity: 2 },
  { productId: 3, quantity: 1 }
];

calculateOrderTotal(cartItems, {
  addressId: 5,
  couponCode: 'SAVE10',
  expressDelivery: true
})
.then(calculation => {
  console.log('Total:', calculation.totalAmount);
  console.log('Savings:', calculation.savingsSummary);
})
.catch(error => {
  console.error('Error:', error);
});
```

---

### 2. React Hook (Recommended)

```typescript
import { useState, useCallback } from 'react';
import axios from 'axios';

interface UseOrderCalculationOptions {
  addressId?: number;
  couponCode?: string;
  expressDelivery?: boolean;
}

interface OrderCalculationResult {
  calculation: OrderTotalCalculation | null;
  loading: boolean;
  error: string | null;
  calculateTotal: (items: OrderItem[], options?: UseOrderCalculationOptions) => Promise<void>;
  reset: () => void;
}

export const useOrderCalculation = (): OrderCalculationResult => {
  const [calculation, setCalculation] = useState<OrderTotalCalculation | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const calculateTotal = useCallback(async (
    items: OrderItem[],
    options: UseOrderCalculationOptions = {}
  ) => {
    if (!items || items.length === 0) {
      setError('No items to calculate');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const response = await axios.post('/api/orders/calculate-total', {
        items,
        shippingAddressId: options.addressId,
        couponCode: options.couponCode,
        expressDelivery: options.expressDelivery || false
      });

      if (response.data.success) {
        setCalculation(response.data.data);
      } else {
        setError(response.data.message);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to calculate total');
    } finally {
      setLoading(false);
    }
  }, []);

  const reset = useCallback(() => {
    setCalculation(null);
    setError(null);
    setLoading(false);
  }, []);

  return { calculation, loading, error, calculateTotal, reset };
};
```

**Usage in React Component:**

```tsx
import React, { useEffect, useState } from 'react';
import { useOrderCalculation } from './hooks/useOrderCalculation';
import { useCart } from './hooks/useCart';

const CheckoutPage: React.FC = () => {
  const { cart } = useCart();
  const { calculation, loading, calculateTotal } = useOrderCalculation();
  const [selectedAddress, setSelectedAddress] = useState<number | undefined>();
  const [couponCode, setCouponCode] = useState('');
  const [isExpress, setIsExpress] = useState(false);

  // Auto-calculate when dependencies change
  useEffect(() => {
    if (cart.items.length > 0) {
      calculateTotal(cart.items, {
        addressId: selectedAddress,
        couponCode: couponCode,
        expressDelivery: isExpress
      });
    }
  }, [cart.items, selectedAddress, couponCode, isExpress, calculateTotal]);

  return (
    <div className="checkout-page">
      <div className="cart-items">
        {/* Cart items display */}
      </div>

      <div className="checkout-options">
        {/* Address selector */}
        <AddressSelector 
          value={selectedAddress}
          onChange={setSelectedAddress}
        />

        {/* Coupon input */}
        <div className="coupon-input">
          <input
            type="text"
            placeholder="Nhập mã giảm giá"
            value={couponCode}
            onChange={(e) => setCouponCode(e.target.value)}
          />
        </div>

        {/* Express delivery toggle */}
        <label>
          <input
            type="checkbox"
            checked={isExpress}
            onChange={(e) => setIsExpress(e.target.checked)}
          />
          Giao hàng nhanh (+20,000 VND)
        </label>
      </div>

      {/* Order Summary */}
      <div className="order-summary">
        {loading ? (
          <div>Đang tính toán...</div>
        ) : calculation ? (
          <>
            <div className="summary-row">
              <span>Tạm tính:</span>
              <span>{calculation.subtotal.toLocaleString('vi-VN')} VND</span>
            </div>
            <div className="summary-row">
              <span>Phí vận chuyển:</span>
              <span>{calculation.shippingCost.toLocaleString('vi-VN')} VND</span>
            </div>
            {calculation.discountAmount > 0 && (
              <div className="summary-row discount">
                <span>Giảm giá:</span>
                <span>-{calculation.discountAmount.toLocaleString('vi-VN')} VND</span>
              </div>
            )}
            <div className="summary-row total">
              <span>Tổng cộng:</span>
              <span className="total-amount">
                {calculation.totalAmount.toLocaleString('vi-VN')} VND
              </span>
            </div>
            
            {/* Savings message */}
            {calculation.discountAmount > 0 && (
              <div className="savings-message">
                🎉 {calculation.savingsSummary}
              </div>
            )}
            
            {/* Shipping message */}
            <div className="shipping-message">
              {calculation.shippingMessage}
            </div>
            
            {/* Delivery estimate */}
            <div className="delivery-estimate">
              Dự kiến giao hàng trong {calculation.estimatedDeliveryDays} ngày
            </div>
          </>
        ) : null}
      </div>

      <button 
        className="checkout-button"
        onClick={handleCheckout}
        disabled={loading || !calculation}
      >
        Đặt hàng
      </button>
    </div>
  );
};

export default CheckoutPage;
```

---

### 3. Vue 3 Composition API

```typescript
import { ref, computed, watch } from 'vue';
import axios from 'axios';

export function useOrderCalculation() {
  const calculation = ref<OrderTotalCalculation | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const calculateTotal = async (
    items: OrderItem[],
    options: {
      addressId?: number;
      couponCode?: string;
      expressDelivery?: boolean;
    } = {}
  ) => {
    if (!items || items.length === 0) {
      error.value = 'No items to calculate';
      return;
    }

    loading.value = true;
    error.value = null;

    try {
      const response = await axios.post('/api/orders/calculate-total', {
        items,
        shippingAddressId: options.addressId,
        couponCode: options.couponCode,
        expressDelivery: options.expressDelivery || false
      });

      if (response.data.success) {
        calculation.value = response.data.data;
      } else {
        error.value = response.data.message;
      }
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Failed to calculate total';
    } finally {
      loading.value = false;
    }
  };

  const formattedTotal = computed(() => {
    return calculation.value 
      ? calculation.value.totalAmount.toLocaleString('vi-VN') + ' VND'
      : '0 VND';
  });

  const hasFreeShipping = computed(() => {
    return calculation.value?.shippingCost === 0;
  });

  return {
    calculation,
    loading,
    error,
    calculateTotal,
    formattedTotal,
    hasFreeShipping
  };
}
```

---

### 4. Angular Service

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class OrderCalculationService {
  private apiUrl = 'https://localhost:8080/api/orders';
  private calculationSubject = new BehaviorSubject<OrderTotalCalculation | null>(null);
  public calculation$ = this.calculationSubject.asObservable();

  constructor(private http: HttpClient) {}

  calculateTotal(
    items: OrderItem[],
    options: {
      addressId?: number;
      couponCode?: string;
      expressDelivery?: boolean;
    } = {}
  ): Observable<OrderTotalCalculation> {
    const requestBody = {
      items,
      shippingAddressId: options.addressId,
      couponCode: options.couponCode,
      expressDelivery: options.expressDelivery || false
    };

    return this.http.post<ApiResponse<OrderTotalCalculation>>(
      `${this.apiUrl}/calculate-total`,
      requestBody
    ).pipe(
      map(response => {
        if (response.success) {
          this.calculationSubject.next(response.data);
          return response.data;
        }
        throw new Error(response.message);
      }),
      catchError(error => {
        console.error('Calculate total error:', error);
        throw error;
      })
    );
  }

  resetCalculation() {
    this.calculationSubject.next(null);
  }
}
```

---

## 🎯 Common Use Cases

### 1. Real-time Cart Total Update

```typescript
// When user changes quantity in cart
const updateCartItemQuantity = async (productId: number, newQuantity: number) => {
  // Update cart items
  const updatedItems = cartItems.map(item => 
    item.productId === productId 
      ? { ...item, quantity: newQuantity }
      : item
  );
  
  // Recalculate total
  await calculateTotal(updatedItems, {
    addressId: selectedAddress,
    couponCode: appliedCoupon
  });
};
```

### 2. Apply Coupon Code

```typescript
const applyCoupon = async (code: string) => {
  try {
    await calculateTotal(cartItems, {
      addressId: selectedAddress,
      couponCode: code,
      expressDelivery: isExpress
    });
    
    if (calculation?.discountAmount > 0) {
      showSuccess(`Đã áp dụng mã ${code}. Bạn tiết kiệm ${calculation.discountAmount.toLocaleString()} VND`);
    } else {
      showError('Mã giảm giá không hợp lệ hoặc không áp dụng được');
    }
  } catch (error) {
    showError('Không thể áp dụng mã giảm giá');
  }
};
```

### 3. Compare Shipping Options

```typescript
const compareShippingOptions = async () => {
  // Calculate with standard shipping
  const standardResult = await calculateTotal(cartItems, {
    addressId: selectedAddress,
    expressDelivery: false
  });
  
  // Calculate with express shipping
  const expressResult = await calculateTotal(cartItems, {
    addressId: selectedAddress,
    expressDelivery: true
  });
  
  const difference = expressResult.totalAmount - standardResult.totalAmount;
  
  return {
    standard: standardResult,
    express: expressResult,
    difference: difference
  };
};
```

---

## 💡 Business Rules & Important Notes

### Shipping Cost Calculation

| Order Total (VND) | Standard Shipping | Express Shipping |
|-------------------|-------------------|------------------|
| < 700,000 | 30,000 VND | 50,000 VND |
| ≥ 700,000 | **FREE** ✓ | **FREE** ✓ |

### Delivery Time

| Method | Estimated Days |
|--------|----------------|
| Standard (Tiêu chuẩn) | 3-5 days |
| Express (Nhanh) | 1-2 days |

### Coupon Validation

✅ **Valid conditions:**
- Coupon is active
- Current date within coupon validity period
- Order amount meets minimum requirement
- Usage limit not exceeded

❌ **Invalid conditions:**
- Expired coupon
- Order amount below minimum
- Coupon already fully used
- Coupon disabled

---

## ⚠️ Error Handling

### Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| `No items provided for calculation` | Empty items array | Ensure cart has items |
| `Invalid product ID` | Product doesn't exist | Validate product IDs |
| `Product not available` | Product out of stock | Check product availability |
| `Invalid coupon code` | Coupon doesn't exist/expired | Validate coupon before applying |

### Error Handling Example

```typescript
try {
  const result = await calculateTotal(items, options);
  return result;
} catch (error: any) {
  if (error.response) {
    // Server responded with error
    switch (error.response.status) {
      case 400:
        showError('Dữ liệu không hợp lệ. Vui lòng kiểm tra lại giỏ hàng.');
        break;
      case 404:
        showError('Không tìm thấy sản phẩm. Vui lòng làm mới trang.');
        break;
      case 500:
        showError('Lỗi hệ thống. Vui lòng thử lại sau.');
        break;
      default:
        showError(error.response.data.message || 'Có lỗi xảy ra');
    }
  } else if (error.request) {
    // Request made but no response
    showError('Không thể kết nối đến server. Kiểm tra kết nối mạng.');
  } else {
    // Something else happened
    showError('Có lỗi xảy ra: ' + error.message);
  }
}
```

---

## 🧪 Testing

### Test Cases

```typescript
describe('Calculate Order Total API', () => {
  it('should calculate basic total correctly', async () => {
    const items = [{ productId: 1, quantity: 2 }];
    const result = await calculateTotal(items);
    
    expect(result.success).toBe(true);
    expect(result.data.totalAmount).toBeGreaterThan(0);
  });

  it('should apply free shipping for orders over 700k', async () => {
    const items = [{ productId: 1, quantity: 10 }]; // Assume total > 700k
    const result = await calculateTotal(items);
    
    expect(result.data.shippingCost).toBe(0);
    expect(result.data.shippingMessage).toContain('Miễn phí');
  });

  it('should apply coupon discount correctly', async () => {
    const items = [{ productId: 1, quantity: 2 }];
    const result = await calculateTotal(items, { couponCode: 'SAVE10' });
    
    expect(result.data.discountAmount).toBeGreaterThan(0);
    expect(result.data.couponCode).toBe('SAVE10');
  });

  it('should calculate express shipping correctly', async () => {
    const items = [{ productId: 1, quantity: 1 }];
    const standard = await calculateTotal(items, { expressDelivery: false });
    const express = await calculateTotal(items, { expressDelivery: true });
    
    expect(express.data.shippingCost).toBeGreaterThan(standard.data.shippingCost);
    expect(express.data.estimatedDeliveryDays).toBeLessThan(standard.data.estimatedDeliveryDays);
  });
});
```

---

## 📚 Related APIs

| API | Purpose | Documentation |
|-----|---------|---------------|
| `POST /api/orders` | Create order | [Order API](./Order-API.md) |
| `POST /api/orders/validate` | Validate order | [Order API](./Order-API.md) |
| `GET /api/cart` | Get cart items | [Cart API](./Cart-API.md) |
| `POST /api/coupon/validate` | Validate coupon | [Coupon API](./Coupon-API.md) |

---

## 🔗 Additional Resources

- [Full Order Management API Documentation](./Order-API.md)
- [API Testing Guide](../Tests/Order.http)
- [Backend Implementation](../Services/Implementations/OrderService.cs)
- [Postman Collection](#) (Coming soon)

---

## 📞 Support

Nếu có vấn đề khi tích hợp, vui lòng liên hệ:
- **Backend Team Lead:** [Your Name]
- **Email:** backend@sakurahome.com
- **Slack:** #backend-support

---

**Last Updated:** 2025-01-17  
**API Version:** 1.0  
**Stability:** ✅ Production Ready
