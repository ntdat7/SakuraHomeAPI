# ?? Sakura Home API - Order Management System

## T?ng quan

H? th?ng Order Management cung c?p ??y ?? các tính n?ng qu?n lý ??n hàng t? khách hàng và nhân viên:

## ?? **Endpoints Khách hàng**

### ?? **Tính toán t?ng ??n hàng**POST /api/order/calculate-total**Body:**
{
  "items": [
    {
      "productId": 1,
      "productVariantId": null,
      "quantity": 2
    }
  ],
  "shippingAddressId": 1,
  "couponCode": "NEWUSER10"
}
### ? **Ki?m tra tính h?p l? ??n hàng**POST /api/order/validate
Authorization: Bearer {token}
### ?? **T?o ??n hàng t? gi? hàng**POST /api/order
Authorization: Bearer {token}**Body:**
{
  "shippingAddressId": 1,
  "billingAddressId": null,
  "paymentMethod": "COD",
  "orderNotes": "G?i tr??c khi giao",
  "couponCode": null,
  "giftWrap": false,
  "giftMessage": null,
  "expressDelivery": false
}
### ?? **Xem danh sách ??n hàng**GET /api/order
Authorization: Bearer {token}
**Filters:**
- `?status=1` - L?c theo tr?ng thái
- `?fromDate=2024-01-01` - T? ngày
- `?toDate=2024-12-31` - ??n ngày
- `?page=1&pageSize=10` - Phân trang
- `?searchTerm=ORD202401` - Tìm ki?m

### ?? **Xem chi ti?t ??n hàng**GET /api/order/{orderId}
Authorization: Bearer {token}
### ?? **C?p nh?t ??n hàng (ch? ??n Pending)**PUT /api/order/{orderId}
Authorization: Bearer {token}
### ?? **Thêm ghi chú ??n hàng**POST /api/order/{orderId}/notes
Authorization: Bearer {token}
### ? **H?y ??n hàng**DELETE /api/order/{orderId}
Authorization: Bearer {token}
### ?? **Yêu c?u tr? hàng**POST /api/order/{orderId}/return
Authorization: Bearer {token}
## ????? **Endpoints Nhân viên (Staff Only)**

### ? **Xác nh?n ??n hàng**PATCH /api/order/{orderId}/confirm
Authorization: Bearer {staffToken}
### ?? **X? lý ??n hàng**PATCH /api/order/{orderId}/process
Authorization: Bearer {staffToken}
### ?? **Giao ??n hàng cho v?n chuy?n**PATCH /api/order/{orderId}/ship
Authorization: Bearer {staffToken}
### ? **Xác nh?n ?ã giao hàng**PATCH /api/order/{orderId}/deliver
Authorization: Bearer {staffToken}
### ?? **C?p nh?t tr?ng thái ??n hàng**PATCH /api/order/{orderId}/status
Authorization: Bearer {staffToken}
### ?? **Th?ng kê ??n hàng**GET /api/order/stats
Authorization: Bearer {staffToken}
### ?? **??n hàng g?n ?ây**GET /api/order/recent?count=10
Authorization: Bearer {staffToken}
## ?? **Tr?ng thái ??n hàng**

| Status | Tên | Mô t? |
|--------|-----|-------|
| 1 | Pending | ?ang ch? x? lý |
| 2 | Confirmed | ?ã xác nh?n |
| 3 | Processing | ?ang chu?n b? hàng |
| 4 | Shipped | ?ã giao cho v?n chuy?n |
| 5 | OutForDelivery | ?ang giao hàng |
| 6 | Delivered | ?ã giao thành công |
| 7 | Cancelled | ?ã h?y |
| 8 | Returned | ?ã tr? hàng |
| 9 | Refunded | ?ã hoàn ti?n |

## ?? **Ph??ng th?c thanh toán**

| Code | Tên | Mô t? |
|------|-----|-------|
| COD | Cash on Delivery | Thanh toán khi nh?n hàng |
| BankTransfer | Chuy?n kho?n | Chuy?n kho?n ngân hàng |
| CreditCard | Th? tín d?ng | Thanh toán qua th? tín d?ng |
| EWallet | Ví ?i?n t? | MoMo, ZaloPay, etc. |

## ?? **Ph??ng th?c giao hàng**

| Method | Tên | Th?i gian | Phí |
|--------|-----|-----------|-----|
| Standard | Giao hàng tiêu chu?n | 3-5 ngày | 30,000 VND |
| Express | Giao hàng nhanh | 1-2 ngày | 50,000 VND |
| SuperFast | Giao hàng siêu t?c | Trong ngày | 100,000 VND |
| SelfPickup | T? ??n l?y | Ngay | Mi?n phí |

## ?? **Response Examples**

### Order Response{
  "success": true,
  "message": "??n hàng ???c t?o thành công",
  "data": {
    "id": 1,
    "orderNumber": "ORD202401001",
    "userId": "guid",
    "status": 1,
    "customerName": "Nguy?n V?n A",
    "customerEmail": "user@example.com",
    "customerPhone": "+84123456789",
    "items": [
      {
        "id": 1,
        "productId": 1,
        "productName": "B?ng hi?u g? handmade",
        "productSku": "SIGN-WOOD-001",
        "quantity": 2,
        "unitPrice": 150000,
        "totalPrice": 300000
      }
    ],
    "shippingAddress": {
      "fullName": "Nguy?n V?n A",
      "phone": "+84123456789",
      "addressLine1": "123 ???ng ABC",
      "city": "H? Chí Minh",
      "country": "Vietnam"
    },
    "subTotal": 300000,
    "shippingCost": 30000,
    "discountAmount": 0,
    "totalAmount": 330000,
    "paymentMethod": "COD",
    "paymentStatus": 1,
    "trackingNumber": null,
    "orderNotes": "G?i tr??c khi giao",
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
### Order Summary List{
  "success": true,
  "data": [
    {
      "id": 1,
      "orderNumber": "ORD202401001",
      "status": 2,
      "statusText": "?ã xác nh?n",
      "totalAmount": 330000,
      "itemCount": 2,
      "createdAt": "2024-01-15T10:30:00Z",
      "trackingNumber": "TN123456789"
    }
  ]
}
## ?? **Workflow ??n hàng**
1. Customer t?o ??n hàng ? Pending
2. Staff xác nh?n ? Confirmed  
3. Staff x? lý ? Processing
4. Staff giao cho v?n chuy?n ? Shipped
5. ?ang giao hàng ? OutForDelivery
6. Giao thành công ? Delivered
**Workflow h?y/tr? hàng:**- Customer/Staff có th? h?y ? tr?ng thái Pending/Confirmed
- Customer có th? yêu c?u tr? hàng sau khi Delivered
- Staff x? lý yêu c?u tr? hàng ? Returned ? Refunded
## ?? **Testing**

S? d?ng file `Tests/Order.http` ?? test:

1. **T?o ??n hàng hoàn ch?nh:**
   - Thêm s?n ph?m vào cart
   - Tính toán t?ng ti?n
   - T?o ??n hàng
   - Xem chi ti?t

2. **Workflow nhân viên:**
   - Xác nh?n ??n hàng
   - X? lý và ?óng gói
   - Giao cho v?n chuy?n
   - Xác nh?n giao hàng

3. **Test các tình hu?ng l?i:**
   - Gi? hàng tr?ng
   - ??a ch? không h?p l?
   - C?p nh?t ??n ?ã h?y
   - Truy c?p ??n hàng c?a ng??i khác

## ?? **B?o m?t**

- **Authentication:** T?t c? endpoints c?n JWT token
- **Authorization:** Staff endpoints c?n role Staff/Admin
- **Validation:** Ki?m tra ownership c?a ??n hàng
- **Data Protection:** Không expose thông tin nh?y c?m

## ? **Performance**

- **Caching:** S? cache thông tin s?n ph?m và ??a ch?
- **Pagination:** H? tr? phân trang cho danh sách ??n hàng
- **Indexing:** Index trên OrderNumber, UserId, Status, CreatedAt
- **Async Processing:** X? lý email/notification không ??ng b?

## ?? **C?u hình**
{
  "OrderSettings": {
    "DefaultShippingFee": 30000,
    "FreeShippingThreshold": 500000,
    "OrderNumberPrefix": "ORD",
    "CancellationTimeLimit": 24,
    "ReturnTimeLimit": 168
  }
}
## ? **Error Codes**

| Code | Message | Description |
|------|---------|-------------|
| 400 | Cart is empty | Gi? hàng tr?ng |
| 400 | Invalid shipping address | ??a ch? giao hàng không h?p l? |
| 400 | Order cannot be modified | ??n hàng không th? s?a |
| 403 | Access denied | Không có quy?n truy c?p |
| 404 | Order not found | Không tìm th?y ??n hàng |
| 409 | Insufficient stock | Không ?? hàng trong kho |

## ?? **Next Steps**

1. **Payment Integration:** Tích h?p gateway thanh toán
2. **Shipping Integration:** API tracking t? ??i tác v?n chuy?n  
3. **Notification System:** Email/SMS thông báo tr?ng thái
4. **Inventory Management:** T? ??ng c?p nh?t kho
5. **Analytics Dashboard:** Báo cáo chi ti?t cho admin
6. **Mobile API:** T?i ?u cho mobile app
7. **Return Management:** Workflow tr? hàng chi ti?t h?n

Order Management System ?ã hoàn thi?n và s?n sàng s? d?ng! ??