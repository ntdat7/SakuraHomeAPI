# 💳 Payment API Documentation

## Tổng quan

API Payment của SakuraHome cung cấp đầy đủ các chức năng thanh toán, hỗ trợ nhiều phương thức thanh toán khác nhau bao gồm VNPay, MoMo, chuyển khoản ngân hàng và COD.

**Base URL:** `https://localhost:7240/api/payment`

## 📋 Mục lục

### 💰 General Payment Endpoints
- [Lấy phương thức thanh toán](#lấy-phương-thức-thanh-toán)
- [Tính phí thanh toán](#tính-phí-thanh-toán)
- [Tạo thanh toán](#tạo-thanh-toán)
- [Lấy thông tin thanh toán](#lấy-thông-tin-thanh-toán)
- [Lấy thanh toán của tôi](#lấy-thanh-toán-của-tôi)
- [Lấy thanh toán theo đơn hàng](#lấy-thanh-toán-theo-đơn-hàng)
- [Hủy thanh toán](#hủy-thanh-toán)

### 🏦 Gateway-Specific Endpoints
- [Tạo thanh toán VNPay](#tạo-thanh-toán-vnpay)
- [VNPay callback](#vnpay-callback)
- [Tạo thanh toán MoMo](#tạo-thanh-toán-momo)
- [MoMo callback](#momo-callback)
- [Tạo chuyển khoản](#tạo-chuyển-khoản)

### 👨‍💼 Staff Endpoints
- [Cập nhật trạng thái](#cập-nhật-trạng-thái)
- [Xử lý hoàn tiền](#xử-lý-hoàn-tiền)
- [Xác nhận chuyển khoản](#xác-nhận-chuyển-khoản)
- [Thống kê thanh toán](#thống-kê-thanh-toán)

---

## 💰 General Payment Endpoints

## 📋 Lấy phương thức thanh toán

### POST `/methods`

Lấy danh sách phương thức thanh toán có sẵn cho đơn hàng.

#### Request Body
{
  "amount": 500000.00,
  "currency": "VND",
  "country": "VN",
  "shippingAddress": {
    "city": "TP. Hồ Chí Minh",
    "district": "Quận 1"
  }
}
#### Response Success (200)
{
  "success": true,
  "message": "Payment methods retrieved successfully",
  "data": [
    {
      "method": "VNPay",
      "name": "VNPay",
      "displayName": "Thanh toán qua VNPay",
      "description": "Thanh toán an toàn qua cổng VNPay",
      "icon": "https://example.com/icons/vnpay.png",
      "isAvailable": true,
      "fee": 0.00,
      "feePercentage": 0.0,
      "minAmount": 10000.00,
      "maxAmount": 50000000.00,
      "supportedBanks": [
        {
          "code": "VCB",
          "name": "Vietcombank",
          "icon": "https://example.com/banks/vcb.png"
        },
        {
          "code": "TCB", 
          "name": "Techcombank",
          "icon": "https://example.com/banks/tcb.png"
        }
      ]
    },
    {
      "method": "MoMo",
      "name": "MoMo",
      "displayName": "Ví MoMo",
      "description": "Thanh toán nhanh chóng qua ví MoMo",
      "icon": "https://example.com/icons/momo.png",
      "isAvailable": true,
      "fee": 0.00,
      "feePercentage": 0.0,
      "minAmount": 5000.00,
      "maxAmount": 20000000.00
    },
    {
      "method": "BankTransfer",
      "name": "BankTransfer",
      "displayName": "Chuyển khoản ngân hàng",
      "description": "Chuyển khoản trực tiếp vào tài khoản ngân hàng",
      "icon": "https://example.com/icons/bank-transfer.png",
      "isAvailable": true,
      "fee": 0.00,
      "feePercentage": 0.0,
      "minAmount": 0.00,
      "maxAmount": 100000000.00,
      "bankAccounts": [
        {
          "bankName": "Vietcombank",
          "accountNumber": "1234567890",
          "accountHolder": "CONG TY SAKURA HOME",
          "branch": "Chi nhánh TP.HCM"
        }
      ]
    },
    {
      "method": "COD",
      "name": "COD",
      "displayName": "Thanh toán khi nhận hàng",
      "description": "Thanh toán bằng tiền mặt khi nhận hàng",
      "icon": "https://example.com/icons/cod.png",
      "isAvailable": true,
      "fee": 15000.00,
      "feePercentage": 0.0,
      "minAmount": 0.00,
      "maxAmount": 5000000.00
    }
  ],
  "timestamp": "2024-01-15T15:00:00Z"
}
#### Frontend Integration
// Payment service
class PaymentService {
  constructor() {
    this.baseURL = process.env.REACT_APP_API_URL;
  }

  async getPaymentMethods(orderInfo) {
    try {
      const response = await fetch(`${this.baseURL}/api/payment/methods`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(orderInfo)
      });

      const result = await response.json();
      
      if (result.success) {
        return result.data;
      } else {
        throw new Error(result.message);
      }
    } catch (error) {
      console.error('Get payment methods error:', error);
      throw error;
    }
  }
}

// Payment methods selector component
const PaymentMethodSelector = ({ orderAmount, shippingAddress, onMethodSelect }) => {
  const [methods, setMethods] = useState([]);
  const [selectedMethod, setSelectedMethod] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const fetchMethods = async () => {
      try {
        setLoading(true);
        const paymentService = new PaymentService();
        
        const methodsData = await paymentService.getPaymentMethods({
          amount: orderAmount,
          currency: 'VND',
          country: 'VN',
          shippingAddress
        });
        
        // Filter available methods
        const availableMethods = methodsData.filter(method => method.isAvailable);
        setMethods(availableMethods);
        
        // Select first method as default
        if (availableMethods.length > 0) {
          setSelectedMethod(availableMethods[0].method);
          onMethodSelect(availableMethods[0]);
        }
      } catch (error) {
        showError('Không thể tải phương thức thanh toán');
      } finally {
        setLoading(false);
      }
    };

    if (orderAmount && shippingAddress) {
      fetchMethods();
    }
  }, [orderAmount, shippingAddress]);

  const handleMethodChange = (method) => {
    setSelectedMethod(method.method);
    onMethodSelect(method);
  };

  if (loading) return <div>Đang tải phương thức thanh toán...</div>;

  return (
    <div className="payment-methods">
      <h3>Chọn phương thức thanh toán</h3>
      
      {methods.map(method => (
        <div
          key={method.method}
          className={`payment-method ${selectedMethod === method.method ? 'selected' : ''}`}
          onClick={() => handleMethodChange(method)}
        >
          <div className="method-header">
            <img src={method.icon} alt={method.name} />
            <div className="method-info">
              <h4>{method.displayName}</h4>
              <p>{method.description}</p>
            </div>
            <div className="method-fee">
              {method.fee > 0 ? (
                <span>+{method.fee.toLocaleString('vi-VN')}đ</span>
              ) : (
                <span className="free">Miễn phí</span>
              )}
            </div>
          </div>
          
          {method.method === 'VNPay' && method.supportedBanks && (
            <div className="supported-banks">
              <span>Ngân hàng hỗ trợ:</span>
              <div className="bank-list">
                {method.supportedBanks.map(bank => (
                  <img key={bank.code} src={bank.icon} alt={bank.name} title={bank.name} />
                ))}
              </div>
            </div>
          )}
          
          {method.method === 'BankTransfer' && method.bankAccounts && (
            <div className="bank-accounts">
              {method.bankAccounts.map((account, index) => (
                <div key={index} className="bank-account">
                  <div className="account-info">
                    <strong>{account.bankName}</strong>
                    <div>STK: {account.accountNumber}</div>
                    <div>Chủ TK: {account.accountHolder}</div>
                    <div>Chi nhánh: {account.branch}</div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      ))}
    </div>
  );
};
---

## 💵 Tính phí thanh toán

### POST `/calculate-fee`

Tính phí thanh toán cho phương thức và số tiền cụ thể.

#### Request Body
{
  "paymentMethod": "VNPay",
  "amount": 500000.00
}
#### Response Success (200)
{
  "success": true,
  "message": "Payment fee calculated successfully",
  "data": 0.00,
  "timestamp": "2024-01-15T15:00:00Z"
}
---

## ➕ Tạo thanh toán

### POST `/`

Tạo một giao dịch thanh toán mới.

**Yêu cầu:** Bearer Token

#### Request Body
{
  "orderId": 1001,
  "paymentMethod": "VNPay",
  "amount": 500000.00,
  "currency": "VND",
  "description": "Thanh toán đơn hàng #ORD-2024-001001",
  "returnUrl": "https://sakurahome.vn/payment/success",
  "cancelUrl": "https://sakurahome.vn/payment/cancel",
  "bankCode": "VCB"
}
#### Response Success (201)
{
  "success": true,
  "message": "Payment created successfully",
  "data": {
    "transactionId": "PAY_20240115_001",
    "orderId": 1001,
    "paymentMethod": "VNPay",
    "status": "Pending",
    "amount": 500000.00,
    "currency": "VND",
    "description": "Thanh toán đơn hàng #ORD-2024-001001",
    "createdAt": "2024-01-15T15:00:00Z",
    "expiresAt": "2024-01-15T15:15:00Z",
    "paymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?...",
    "qrCode": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA..."
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
#### Frontend Integration
// Create payment function
const createPayment = async (paymentData) => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_URL}/api/payment`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      },
      body: JSON.stringify(paymentData)
    });

    const result = await response.json();
    
    if (result.success) {
      return result.data;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Create payment error:', error);
    throw error;
  }
};

// Payment processing component
const PaymentProcessor = ({ order, paymentMethod, onSuccess, onError }) => {
  const [processing, setProcessing] = useState(false);

  const handlePayment = async () => {
    try {
      setProcessing(true);

      const paymentData = {
        orderId: order.id,
        paymentMethod: paymentMethod.method,
        amount: order.totalAmount,
        currency: 'VND',
        description: `Thanh toán đơn hàng #${order.orderNumber}`,
        returnUrl: `${window.location.origin}/payment/success`,
        cancelUrl: `${window.location.origin}/payment/cancel`
      };

      const payment = await createPayment(paymentData);

      // Handle different payment methods
      switch (paymentMethod.method) {
        case 'VNPay':
        case 'MoMo':
          // Redirect to payment gateway
          window.location.href = payment.paymentUrl;
          break;
          
        case 'BankTransfer':
          // Show bank transfer instructions
          showBankTransferInstructions(payment);
          break;
          
        case 'COD':
          // Mark as pending, will be paid on delivery
          onSuccess(payment);
          break;
          
        default:
          throw new Error('Unsupported payment method');
      }
    } catch (error) {
      onError(error);
    } finally {
      setProcessing(false);
    }
  };

  const showBankTransferInstructions = (payment) => {
    // Show modal with bank transfer details
    const modal = (
      <BankTransferModal 
        payment={payment}
        onClose={() => onSuccess(payment)}
      />
    );
    
    showModal(modal);
  };

  return (
    <button
      onClick={handlePayment}
      disabled={processing}
      className="payment-btn"
    >
      {processing ? 'Đang xử lý...' : `Thanh toán ${order.totalAmount.toLocaleString('vi-VN')}đ`}
    </button>
  );
};
---

## 📖 Lấy thông tin thanh toán

### GET `/{transactionId}`

Lấy thông tin chi tiết của một giao dịch thanh toán.

**Yêu cầu:** Bearer Token

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `transactionId` | string | ID giao dịch thanh toán |

#### Response Success (200)
{
  "success": true,
  "message": "Payment retrieved successfully",
  "data": {
    "transactionId": "PAY_20240115_001",
    "orderId": 1001,
    "orderNumber": "ORD-2024-001001",
    "paymentMethod": "VNPay",
    "status": "Completed",
    "statusDisplay": "Đã thanh toán",
    "amount": 500000.00,
    "currency": "VND",
    "description": "Thanh toán đơn hàng #ORD-2024-001001",
    "gatewayTransactionId": "VNP_TXN_123456789",
    "gatewayResponse": {
      "responseCode": "00",
      "responseMessage": "Successful",
      "bankCode": "VCB",
      "cardType": "ATM"
    },
    "fee": 0.00,
    "netAmount": 500000.00,
    "createdAt": "2024-01-15T15:00:00Z",
    "paidAt": "2024-01-15T15:05:00Z",
    "expiresAt": "2024-01-15T15:15:00Z",
    "customer": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fullName": "Nguyễn Văn A",
      "email": "customer@example.com"
    }
  },
  "timestamp": "2024-01-15T17:00:00Z"
}
---

## 📋 Lấy thanh toán của tôi

### GET `/my-payments`

Lấy danh sách thanh toán của người dùng hiện tại.

**Yêu cầu:** Bearer Token

#### Query Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | integer | 1 | Trang hiện tại |
| `pageSize` | integer | 20 | Số giao dịch trên mỗi trang |

#### Response Success (200)
{
  "success": true,
  "message": "User payments retrieved successfully",
  "data": [
    {
      "transactionId": "PAY_20240115_001",
      "orderId": 1001,
      "orderNumber": "ORD-2024-001001",
      "paymentMethod": "VNPay",
      "methodDisplay": "VNPay",
      "status": "Completed",
      "statusDisplay": "Đã thanh toán",
      "amount": 500000.00,
      "currency": "VND",
      "createdAt": "2024-01-15T15:00:00Z",
      "paidAt": "2024-01-15T15:05:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 8,
    "totalPages": 1,
    "hasNext": false,
    "hasPrevious": false
  },
  "timestamp": "2024-01-15T17:00:00Z"
}
---

## 🛍️ Lấy thanh toán theo đơn hàng

### GET `/order/{orderId}`

Lấy danh sách thanh toán của một đơn hàng cụ thể.

**Yêu cầu:** Bearer Token

#### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `orderId` | integer | ID đơn hàng |

#### Response Success (200)
{
  "success": true,
  "message": "Order payments retrieved successfully",
  "data": [
    {
      "transactionId": "PAY_20240115_001",
      "paymentMethod": "VNPay",
      "status": "Completed",
      "amount": 500000.00,
      "createdAt": "2024-01-15T15:00:00Z",
      "paidAt": "2024-01-15T15:05:00Z"
    }
  ],
  "timestamp": "2024-01-15T17:00:00Z"
}
---

## ❌ Hủy thanh toán

### DELETE `/{transactionId}`

Hủy một giao dịch thanh toán (chỉ cho thanh toán Pending).

**Yêu cầu:** Bearer Token

#### Request Body
{
  "reason": "Khách hàng đổi ý"
}
#### Response Success (200)
{
  "success": true,
  "message": "Payment cancelled successfully",
  "timestamp": "2024-01-15T17:00:00Z"
}
---

## 🏦 Gateway-Specific Endpoints

## 🇻🇳 Tạo thanh toán VNPay

### POST `/vnpay`

Tạo thanh toán qua cổng VNPay.

**Yêu cầu:** Bearer Token

#### Request Body
{
  "orderId": 1001,
  "amount": 500000.00,
  "description": "Thanh toán đơn hàng #ORD-2024-001001",
  "bankCode": "VCB",
  "returnUrl": "https://sakurahome.vn/payment/vnpay/return",
  "locale": "vn"
}
#### Response Success (200)
{
  "success": true,
  "message": "VNPay payment created successfully",
  "data": {
    "transactionId": "PAY_20240115_001",
    "paymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_Version=2.1.0&vnp_Command=pay...",
    "qrCode": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...",
    "expiresAt": "2024-01-15T15:15:00Z"
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
---

## 🔄 VNPay callback

### POST `/vnpay/callback`

Endpoint nhận callback từ VNPay sau khi thanh toán.

**Không yêu cầu xác thực** (Public endpoint)

#### Response Success (200)
{
  "success": true,
  "message": "VNPay callback processed successfully",
  "data": {
    "transactionId": "PAY_20240115_001",
    "status": "Completed",
    "orderId": 1001,
    "amount": 500000.00,
    "responseCode": "00",
    "responseMessage": "Successful"
  },
  "timestamp": "2024-01-15T15:05:00Z"
}
---

## 📱 Tạo thanh toán MoMo

### POST `/momo`

Tạo thanh toán qua ví MoMo.

**Yêu cầu:** Bearer Token

#### Request Body
{
  "orderId": 1001,
  "amount": 500000.00,
  "description": "Thanh toán đơn hàng #ORD-2024-001001",
  "returnUrl": "https://sakurahome.vn/payment/momo/return",
  "notifyUrl": "https://sakurahome.vn/api/payment/momo/callback"
}
#### Response Success (200)
{
  "success": true,
  "message": "MoMo payment created successfully",
  "data": {
    "transactionId": "PAY_20240115_002",
    "paymentUrl": "https://payment.momo.vn/gw_payment/payment/processor",
    "deeplink": "momo://app?action=payWithAppInApp&...",
    "qrCode": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...",
    "expiresAt": "2024-01-15T15:15:00Z"
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
---

## 🔄 MoMo callback

### POST `/momo/callback`

Endpoint nhận callback từ MoMo sau khi thanh toán.

**Không yêu cầu xác thực** (Public endpoint)

#### Response Success (200)
{
  "success": true,
  "message": "MoMo callback processed successfully",
  "data": {
    "transactionId": "PAY_20240115_002",
    "status": "Completed",
    "orderId": 1001,
    "amount": 500000.00,
    "resultCode": 0,
    "message": "Success"
  },
  "timestamp": "2024-01-15T15:05:00Z"
}
---

## 🏦 Tạo chuyển khoản

### POST `/bank-transfer`

Tạo thanh toán bằng chuyển khoản ngân hàng.

**Yêu cầu:** Bearer Token

#### Request Body
{
  "orderId": 1001,
  "amount": 500000.00,
  "description": "Thanh toán đơn hàng #ORD-2024-001001",
  "bankAccount": {
    "bankName": "Vietcombank",
    "accountNumber": "1234567890",
    "accountHolder": "CONG TY SAKURA HOME"
  }
}
#### Response Success (200)
{
  "success": true,
  "message": "Bank transfer created successfully",
  "data": {
    "transactionId": "PAY_20240115_003",
    "transferCode": "TRANSFER_001001",
    "amount": 500000.00,
    "bankAccount": {
      "bankName": "Vietcombank", 
      "accountNumber": "1234567890",
      "accountHolder": "CONG TY SAKURA HOME",
      "branch": "Chi nhánh TP.HCM"
    },
    "transferContent": "SAKURA 001001 Nguyen Van A",
    "qrCode": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...",
    "instructions": [
      "Chuyển khoản đúng số tiền: 500,000đ",
      "Nội dung chuyển khoản: SAKURA 001001 Nguyen Van A",
      "Sau khi chuyển khoản, vui lòng chụp ảnh biên lai và gửi cho chúng tôi"
    ],
    "expiresAt": "2024-01-17T15:00:00Z"
  },
  "timestamp": "2024-01-15T15:00:00Z"
}
#### Frontend Integration
// Bank transfer modal component
const BankTransferModal = ({ payment, onClose }) => {
  const [uploaded, setUploaded] = useState(false);

  const handleFileUpload = async (file) => {
    try {
      const formData = new FormData();
      formData.append('receipt', file);
      formData.append('transactionId', payment.transactionId);

      const response = await fetch(`${process.env.REACT_APP_API_URL}/api/payment/upload-receipt`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
        },
        body: formData
      });

      if (response.ok) {
        setUploaded(true);
        showSuccess('Đã upload biên lai thành công');
      }
    } catch (error) {
      showError('Không thể upload biên lai');
    }
  };

  const copyToClipboard = (text) => {
    navigator.clipboard.writeText(text);
    showSuccess('Đã sao chép');
  };

  return (
    <div className="bank-transfer-modal">
      <div className="modal-header">
        <h3>Hướng dẫn chuyển khoản</h3>
        <button onClick={onClose}>×</button>
      </div>

      <div className="modal-body">
        <div className="bank-info">
          <h4>Thông tin tài khoản</h4>
          <div className="account-details">
            <div className="detail-row">
              <span>Ngân hàng:</span>
              <strong>{payment.bankAccount.bankName}</strong>
            </div>
            <div className="detail-row">
              <span>Số tài khoản:</span>
              <div className="copyable">
                <strong>{payment.bankAccount.accountNumber}</strong>
                <button onClick={() => copyToClipboard(payment.bankAccount.accountNumber)}>
                  Sao chép
                </button>
              </div>
            </div>
            <div className="detail-row">
              <span>Chủ tài khoản:</span>
              <strong>{payment.bankAccount.accountHolder}</strong>
            </div>
            <div className="detail-row">
              <span>Số tiền:</span>
              <div className="copyable">
                <strong className="amount">{payment.amount.toLocaleString('vi-VN')}đ</strong>
                <button onClick={() => copyToClipboard(payment.amount.toString())}>
                  Sao chép
                </button>
              </div>
            </div>
            <div className="detail-row">
              <span>Nội dung:</span>
              <div className="copyable">
                <strong>{payment.transferContent}</strong>
                <button onClick={() => copyToClipboard(payment.transferContent)}>
                  Sao chép
                </button>
              </div>
            </div>
          </div>
        </div>

        {payment.qrCode && (
          <div className="qr-code">
            <h4>Quét mã QR để chuyển khoản</h4>
            <img src={payment.qrCode} alt="QR Code" />
          </div>
        )}

        <div className="instructions">
          <h4>Hướng dẫn</h4>
          <ol>
            {payment.instructions.map((instruction, index) => (
              <li key={index}>{instruction}</li>
            ))}
          </ol>
        </div>

        <div className="receipt-upload">
          <h4>Upload biên lai chuyển khoản</h4>
          <FileUpload
            accept="image/*"
            onUpload={handleFileUpload}
            uploaded={uploaded}
          />
        </div>
      </div>

      <div className="modal-footer">
        <button onClick={onClose} className="confirm-btn">
          Đã chuyển khoản
        </button>
      </div>
    </div>
  );
};
---

## 👨‍💼 Staff Endpoints

## 🔄 Cập nhật trạng thái

### PATCH `/{transactionId}/status`

Cập nhật trạng thái thanh toán (Staff only).

**Yêu cầu:** Bearer Token với role Staff

#### Request Body
{
  "status": "Completed",
  "notes": "Đã xác nhận thanh toán thành công",
  "gatewayResponse": {
    "responseCode": "00",
    "responseMessage": "Success"
  }
}
#### Response Success (200)
{
  "success": true,
  "message": "Payment status updated successfully",
  "data": {
    // Updated payment details
  },
  "timestamp": "2024-01-15T17:00:00Z"
}
---

## 💰 Xử lý hoàn tiền

### POST `/{transactionId}/refund`

Xử lý hoàn tiền cho thanh toán (Staff only).

**Yêu cầu:** Bearer Token với role Staff

#### Request Body
{
  "transactionId": "PAY_20240115_001",
  "amount": 250000.00,
  "reason": "Hoàn tiền một phần do sản phẩm lỗi",
  "refundMethod": "Original"
}
#### Response Success (200)
{
  "success": true,
  "message": "Refund processed successfully",
  "data": {
    "refundId": "REF_20240115_001",
    "transactionId": "PAY_20240115_001",
    "amount": 250000.00,
    "refundMethod": "Original",
    "status": "Processing",
    "reason": "Hoàn tiền một phần do sản phẩm lỗi",
    "estimatedCompletionDate": "2024-01-20T17:00:00Z",
    "createdAt": "2024-01-15T17:00:00Z"
  },
  "timestamp": "2024-01-15T17:00:00Z"
}
---

## ✅ Xác nhận chuyển khoản

### POST `/{transactionId}/confirm-transfer`

Xác nhận đã nhận được chuyển khoản (Staff only).

**Yêu cầu:** Bearer Token với role Staff

#### Request Body
{
  "bankAccount": {
    "bankName": "Vietcombank",
    "accountNumber": "1234567890"
  },
  "transferAmount": 500000.00,
  "transferDate": "2024-01-15T16:00:00Z",
  "transferReference": "FT24015123456",
  "notes": "Đã xác nhận nhận được chuyển khoản"
}
#### Response Success (200)
{
  "success": true,
  "message": "Bank transfer confirmed successfully",
  "data": {
    // Updated payment details
  },
  "timestamp": "2024-01-15T17:00:00Z"
}
---

## 📊 Thống kê thanh toán

### GET `/stats`

Lấy thống kê thanh toán (Staff only).

**Yêu cầu:** Bearer Token với role Staff

#### Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `fromDate` | datetime | Từ ngày |
| `toDate` | datetime | Đến ngày |
| `method` | string | Phương thức thanh toán |

#### Response Success (200)
{
  "success": true,
  "message": "Payment statistics retrieved successfully",
  "data": {
    "totalTransactions": 1250,
    "totalAmount": 125000000.00,
    "completedTransactions": 1180,
    "completedAmount": 118000000.00,
    "averageTransactionValue": 100000.00,
    "successRate": 94.4,
    "methodBreakdown": {
      "VNPay": {
        "count": 650,
        "amount": 65000000.00,
        "percentage": 52.0
      },
      "MoMo": {
        "count": 300,
        "amount": 30000000.00,
        "percentage": 24.0
      },
      "BankTransfer": {
        "count": 200,
        "amount": 20000000.00,
        "percentage": 16.0
      },
      "COD": {
        "count": 100,
        "amount": 10000000.00,
        "percentage": 8.0
      }
    },
    "dailyStats": [
      {
        "date": "2024-01-15",
        "transactionCount": 45,
        "amount": 4500000.00,
        "successRate": 95.6
      }
    ],
    "topFailureReasons": [
      {
        "reason": "Insufficient balance",
        "count": 25,
        "percentage": 35.7
      },
      {
        "reason": "Card expired",
        "count": 18,
        "percentage": 25.7
      }
    ]
  },
  "timestamp": "2024-01-15T17:00:00Z"
}
---

## 🛠️ Payment Management Hook
// Complete payment management hook
export const usePaymentManagement = () => {
  const paymentService = new PaymentService();

  const getPaymentMethods = async (orderInfo) => {
    return await paymentService.getPaymentMethods(orderInfo);
  };

  const calculateFee = async (method, amount) => {
    return await paymentService.calculateFee(method, amount);
  };

  const createPayment = async (paymentData) => {
    return await paymentService.createPayment(paymentData);
  };

  const getPayment = async (transactionId) => {
    return await paymentService.getPayment(transactionId);
  };

  const cancelPayment = async (transactionId, reason) => {
    return await paymentService.cancelPayment(transactionId, reason);
  };

  const createVNPayPayment = async (vnpayData) => {
    return await paymentService.createVNPayPayment(vnpayData);
  };

  const createMoMoPayment = async (momoData) => {
    return await paymentService.createMoMoPayment(momoData);
  };

  const createBankTransfer = async (transferData) => {
    return await paymentService.createBankTransfer(transferData);
  };

  return {
    getPaymentMethods,
    calculateFee,
    createPayment,
    getPayment,
    cancelPayment,
    createVNPayPayment,
    createMoMoPayment,
    createBankTransfer
  };
};
---

## 💳 Payment Status Enums
export const PaymentStatus = {
  PENDING: 'Pending',
  PROCESSING: 'Processing',
  COMPLETED: 'Completed',
  FAILED: 'Failed',
  CANCELLED: 'Cancelled',
  REFUNDED: 'Refunded',
  PARTIALLY_REFUNDED: 'PartiallyRefunded'
};

export const PaymentStatusDisplayNames = {
  [PaymentStatus.PENDING]: 'Chờ thanh toán',
  [PaymentStatus.PROCESSING]: 'Đang xử lý',
  [PaymentStatus.COMPLETED]: 'Đã thanh toán',
  [PaymentStatus.FAILED]: 'Thất bại',
  [PaymentStatus.CANCELLED]: 'Đã hủy',
  [PaymentStatus.REFUNDED]: 'Đã hoàn tiền',
  [PaymentStatus.PARTIALLY_REFUNDED]: 'Hoàn tiền một phần'
};

export const PaymentMethod = {
  VNPAY: 'VNPay',
  MOMO: 'MoMo',
  BANK_TRANSFER: 'BankTransfer',
  COD: 'COD',
  CREDIT_CARD: 'CreditCard',
  PAYPAL: 'PayPal'
};

export const RefundMethod = {
  ORIGINAL: 'Original',
  BANK_TRANSFER: 'BankTransfer',
  STORE_CREDIT: 'StoreCredit'
};
---

## 🔧 Payment Utilities
// Payment status badge component
const PaymentStatusBadge = ({ status }) => {
  const getStatusColor = (status) => {
    switch (status) {
      case PaymentStatus.COMPLETED:
        return 'success';
      case PaymentStatus.PENDING:
      case PaymentStatus.PROCESSING:
        return 'warning';
      case PaymentStatus.FAILED:
      case PaymentStatus.CANCELLED:
        return 'danger';
      case PaymentStatus.REFUNDED:
      case PaymentStatus.PARTIALLY_REFUNDED:
        return 'info';
      default:
        return 'secondary';
    }
  };

  return (
    <span className={`badge badge-${getStatusColor(status)}`}>
      {PaymentStatusDisplayNames[status] || status}
    </span>
  );
};

// Payment method icon component
const PaymentMethodIcon = ({ method, size = 'medium' }) => {
  const getMethodIcon = (method) => {
    switch (method) {
      case PaymentMethod.VNPAY:
        return '/icons/vnpay.png';
      case PaymentMethod.MOMO:
        return '/icons/momo.png';
      case PaymentMethod.BANK_TRANSFER:
        return '/icons/bank-transfer.png';
      case PaymentMethod.COD:
        return '/icons/cod.png';
      default:
        return '/icons/payment-default.png';
    }
  };

  return (
    <img
      src={getMethodIcon(method)}
      alt={method}
      className={`payment-method-icon ${size}`}
    />
  );
};

// Format payment amount
export const formatPaymentAmount = (amount, currency = 'VND') => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: currency,
    minimumFractionDigits: 0
  }).format(amount);
};

// Validate payment data
export const validatePaymentData = (paymentData) => {
  const errors = [];

  if (!paymentData.orderId) {
    errors.push('Order ID is required');
  }

  if (!paymentData.paymentMethod) {
    errors.push('Payment method is required');
  }

  if (!paymentData.amount || paymentData.amount <= 0) {
    errors.push('Amount must be greater than 0');
  }

  return {
    isValid: errors.length === 0,
    errors
  };
};
---

Tài liệu này cung cấp đầy đủ thông tin để frontend team tích hợp với Payment API của SakuraHome. API hỗ trợ nhiều phương thức thanh toán phổ biến tại Việt Nam và cung cấp đầy đủ tính năng quản lý thanh toán chuyên nghiệp.