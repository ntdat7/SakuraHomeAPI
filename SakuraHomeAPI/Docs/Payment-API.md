# Payment API Documentation

## Overview
The Payment API handles payment processing for orders in the Sakura Home system. Currently supports **COD (Cash on Delivery)** payment method only.

## Base URLhttps://localhost:7171/api/payment
## Authentication
Most endpoints require Bearer token authentication.

## Endpoints

### 1. Get Available Payment Methods
**GET** `/methods`

Returns available payment methods based on amount and other criteria.

**Query Parameters:**
- `amount` (decimal, optional): Order amount to filter applicable methods
- `currency` (string, optional): Currency code (default: "VND")
- `activeOnly` (boolean, optional): Only return active methods (default: true)

**Response:**{
  "success": true,
  "message": "Payment methods retrieved successfully",
  "data": [
    {
      "id": 1,
      "name": "Thanh toán khi nh?n hàng (COD)",
      "description": "Thanh toán b?ng ti?n m?t khi nh?n hàng",
      "code": "COD",
      "logoUrl": "/images/payment/cod.png",
      "isActive": true,
      "displayOrder": 1,
      "feePercentage": 0,
      "fixedFee": 0,
      "minAmount": 0,
      "maxAmount": 5000000,
      "isAvailable": true,
      "instructions": "Thanh toán b?ng ti?n m?t khi nh?n hàng. Shipper s? thu ti?n khi giao hàng."
    }
  ]
}
### 2. Create Payment
**POST** `/`

Creates a new payment transaction for an order.

**Authentication:** Required

**Request Body:**{
  "orderId": 1,
  "paymentMethod": 1,
  "description": "Payment for order",
  "returnUrl": "https://example.com/return",
  "cancelUrl": "https://example.com/cancel"
}
**Payment Methods:**
- `1` = COD (Cash on Delivery)
- Other methods are not implemented yet

**Response:**{
  "success": true,
  "message": "Payment transaction created successfully",
  "data": {
    "id": 1,
    "transactionId": "PAY1731123456789",
    "orderId": 1,
    "orderNumber": "ORD-20241101-001",
    "userId": "guid",
    "userEmail": "user@example.com",
    "paymentMethod": 1,
    "paymentMethodName": "Thanh toán khi nh?n hàng",
    "status": 9,
    "statusText": "?ã xác nh?n",
    "amount": 500000,
    "currency": "VND",
    "description": "Payment for order ORD-20241101-001",
    "createdAt": "2024-11-01T10:00:00Z",
    "canRefund": false,
    "canCancel": false
  }
}
### 3. Get Payment Details
**GET** `/{transactionId}`

Retrieves payment details by transaction ID.

**Authentication:** Required

**Response:**{
  "success": true,
  "message": "Payment retrieved successfully",
  "data": {
    "id": 1,
    "transactionId": "PAY1731123456789",
    "orderId": 1,
    "orderNumber": "ORD-20241101-001",
    "userId": "guid",
    "userEmail": "user@example.com",
    "paymentMethod": 1,
    "paymentMethodName": "Thanh toán khi nh?n hàng",
    "status": 9,
    "statusText": "?ã xác nh?n",
    "amount": 500000,
    "currency": "VND",
    "createdAt": "2024-11-01T10:00:00Z",
    "canRefund": false,
    "canCancel": false
  }
}
### 4. Get User Payments
**GET** `/my-payments`

Retrieves current user's payment history.

**Authentication:** Required

**Query Parameters:**
- `page` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Items per page (default: 20)

**Response:**{
  "success": true,
  "message": "User payments retrieved successfully",
  "data": [
    {
      "id": 1,
      "transactionId": "PAY1731123456789",
      "orderId": 1,
      "orderNumber": "ORD-20241101-001",
      "paymentMethod": 1,
      "paymentMethodName": "Thanh toán khi nh?n hàng",
      "status": 9,
      "statusText": "?ã xác nh?n",
      "amount": 500000,
      "currency": "VND",
      "createdAt": "2024-11-01T10:00:00Z"
    }
  ]
}
### 5. Get Order Payments
**GET** `/order/{orderId}`

Retrieves all payments for a specific order.

**Authentication:** Required

**Response:**{
  "success": true,
  "message": "Order payments retrieved successfully",
  "data": [
    {
      "id": 1,
      "transactionId": "PAY1731123456789",
      "orderId": 1,
      "orderNumber": "ORD-20241101-001",
      "paymentMethod": 1,
      "paymentMethodName": "Thanh toán khi nh?n hàng",
      "status": 9,
      "statusText": "?ã xác nh?n",
      "amount": 500000,
      "currency": "VND",
      "createdAt": "2024-11-01T10:00:00Z"
    }
  ]
}
### 6. Calculate Payment Fee
**POST** `/calculate-fee`

Calculates the fee for a payment method and amount.

**Request Body:**{
  "paymentMethod": 1,
  "amount": 500000
}
**Response:**
{
  "success": true,
  "message": "COD payment has no fee",
  "data": 0
}
## Payment Methods

### COD (Cash on Delivery)
- **ID:** 1
- **Code:** "COD"
- **Name:** "Thanh toán khi nh?n hàng"
- **Fee:** 0 VND (no fee)
- **Min Amount:** 0 VND
- **Max Amount:** 5,000,000 VND
- **Instructions:** Customer pays cash when receiving the order

## Payment Status

| Status ID | Status Name | Description |
|-----------|-------------|-------------|
| 1 | Pending | Ch? thanh toán |
| 2 | Processing | ?ang x? lý |
| 3 | Paid | ?ã thanh toán |
| 4 | Failed | Thanh toán th?t b?i |
| 5 | Cancelled | ?ã h?y |
| 6 | Refunded | ?ã hoàn ti?n |
| 7 | PartiallyRefunded | Hoàn ti?n m?t ph?n |
| 8 | Expired | H?t h?n |
| 9 | Confirmed | ?ã xác nh?n |

**Note:** For COD payments, status immediately changes to "Confirmed" (9) upon creation.

## Error Responses

### 400 Bad Request{
  "success": false,
  "message": "Currently only COD payment is supported"
}
### 401 Unauthorized{
  "success": false,
  "message": "User not authenticated"
}
### 404 Not Found{
  "success": false,
  "message": "Order not found"
}
### 409 Conflict{
  "success": false,
  "message": "Order is already paid"
}
## Business Logic

### COD Payment Flow
1. User creates an order
2. User selects COD payment method
3. Payment transaction is created with "Confirmed" status
4. Order payment status is updated to "Confirmed"
5. Order proceeds to fulfillment
6. Customer pays cash when receiving the order

### Validation Rules
- Order must exist and belong to the authenticated user
- Order must not already be paid
- Only COD payment method is currently supported
- Payment amount matches order total amount

## Future Enhancements
- VNPay integration for online banking
- MoMo integration for e-wallet payments
- Bank transfer with QR codes
- Credit/debit card payments
- Installment payments
- Payment refund functionality
- Payment statistics for admin