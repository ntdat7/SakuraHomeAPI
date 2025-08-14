# SakuraHome API - Notification Management

## Tổng quan

Hệ thống thông báo SakuraHome cung cấp đầy đủ các chức năng quản lý thông báo cho người dùng, bao gồm thông báo cá nhân, thông báo hàng loạt, và cài đặt thông báo.

## Base URL
```
https://localhost:7240/api/notification
```

## Authentication
Tất cả endpoints (trừ callback) yêu cầu JWT Bearer token:
```
Authorization: Bearer your-jwt-token
```

## Endpoints

### 📱 User Notification Management

#### 1. Lấy danh sách thông báo của người dùng
```http
GET /api/notification
```

**Query Parameters:**
- `page` (int, optional): Trang hiện tại (default: 1)
- `pageSize` (int, optional): Số item mỗi trang (default: 20)
- `unreadOnly` (bool, optional): Chỉ lấy thông báo chưa đọc (default: false)

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy danh sách thông báo thành công",
  "data": [
    {
      "id": 1,
      "title": "Đơn hàng #12345 đã được xác nhận",
      "message": "Đơn hàng của bạn đã được xác nhận và đang được chuẩn bị",
      "type": "OrderUpdate",
      "isRead": false,
      "createdAt": "2024-01-01T12:00:00Z",
      "data": {
        "orderId": 12345,
        "orderStatus": "Confirmed"
      }
    }
  ],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 2. Lấy chi tiết thông báo
```http
GET /api/notification/{id}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy thông báo thành công",
  "data": {
    "id": 1,
    "title": "Đơn hàng #12345 đã được xác nhận",
    "message": "Đơn hàng của bạn đã được xác nhận và đang được chuẩn bị",
    "type": "OrderUpdate",
    "isRead": true,
    "readAt": "2024-01-01T12:30:00Z",
    "createdAt": "2024-01-01T12:00:00Z",
    "data": {
      "orderId": 12345,
      "orderStatus": "Confirmed"
    }
  }
}
```

#### 3. Lấy số lượng thông báo chưa đọc
```http
GET /api/notification/unread-count
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy số thông báo chưa đọc thành công",
  "data": 5,
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 4. Đánh dấu thông báo đã đọc
```http
PATCH /api/notification/{id}/read
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Đã đánh dấu thông báo đã đọc",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 5. Đánh dấu tất cả thông báo đã đọc
```http
PATCH /api/notification/mark-all-read
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Đã đánh dấu tất cả thông báo đã đọc",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### 6. Xóa thông báo
```http
DELETE /api/notification/{id}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Xóa thông báo thành công",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### ⚙️ Notification Preferences

#### 7. Lấy cài đặt thông báo
```http
GET /api/notification/preferences
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy cài đặt thông báo thành công",
  "data": {
    "emailNotifications": true,
    "smsNotifications": false,
    "pushNotifications": true,
    "orderUpdates": true,
    "promotionalOffers": false,
    "stockAlerts": true,
    "securityAlerts": true,
    "maintenanceNotices": true
  }
}
```

#### 8. Cập nhật cài đặt thông báo
```http
PUT /api/notification/preferences
```

**Request Body:**
```json
{
  "emailNotifications": true,
  "smsNotifications": false,
  "pushNotifications": true,
  "orderUpdates": true,
  "promotionalOffers": false,
  "stockAlerts": true,
  "securityAlerts": true,
  "maintenanceNotices": true
}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Cập nhật cài đặt thông báo thành công",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### 👨‍💼 Staff & Admin Endpoints

#### 9. Gửi thông báo đến người dùng cụ thể (Staff Only)
```http
POST /api/notification/send
Authorization: Bearer staff-token
```

**Request Body:**
```json
{
  "userId": "user-guid-here",
  "title": "Thông báo từ hệ thống",
  "message": "Nội dung thông báo chi tiết",
  "type": "System",
  "data": {
    "customField": "customValue"
  }
}
```

**Response Success (201):**
```json
{
  "success": true,
  "message": "Gửi thông báo thành công",
  "data": {
    "id": 123,
    "title": "Thông báo từ hệ thống",
    "message": "Nội dung thông báo chi tiết",
    "type": "System",
    "isRead": false,
    "createdAt": "2024-01-01T12:00:00Z"
  }
}
```

#### 10. Gửi thông báo hàng loạt (Staff Only)
```http
POST /api/notification/send-bulk
Authorization: Bearer staff-token
```

**Request Body:**
```json
{
  "userIds": ["user1-guid", "user2-guid", "user3-guid"],
  "title": "Thông báo hàng loạt",
  "message": "Nội dung thông báo cho nhiều người dùng",
  "type": "Promotional",
  "data": {
    "campaignId": "campaign-123"
  }
}
```

#### 11. Gửi thông báo khuyến mãi (Staff Only)
```http
POST /api/notification/promotional
Authorization: Bearer staff-token
```

**Request Body:**
```json
{
  "title": "🎉 Flash Sale 50% Off!",
  "message": "Giảm giá lên đến 50% cho tất cả sản phẩm điện tử. Chỉ trong hôm nay!",
  "imageUrl": "https://example.com/promotion-banner.jpg",
  "actionUrl": "https://sakurahome.com/flash-sale",
  "targetAudience": "All", // All, NewCustomers, VIPCustomers, etc.
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-01-01T23:59:59Z"
}
```

#### 12. Gửi thông báo bảo trì (Admin Only)
```http
POST /api/notification/maintenance
Authorization: Bearer admin-token
```

**Request Body:**
```json
{
  "startTime": "2024-01-01T02:00:00Z",
  "endTime": "2024-01-01T04:00:00Z",
  "message": "Hệ thống sẽ bảo trì để cải thiện hiệu suất. Cảm ơn sự thông cảm của quý khách."
}
```

#### 13. Gửi cảnh báo hết hàng (Staff Only)
```http
POST /api/notification/low-stock
Authorization: Bearer staff-token
```

**Request Body:**
```json
{
  "productId": 123,
  "currentStock": 5,
  "threshold": 10
}
```

## Notification Types

### 📝 Các loại thông báo được hỗ trợ:

1. **OrderUpdate** - Cập nhật đơn hàng
   - Đơn hàng được xác nhận
   - Đơn hàng đang chuẩn bị
   - Đơn hàng đang giao
   - Đơn hàng đã giao thành công

2. **PaymentUpdate** - Cập nhật thanh toán
   - Thanh toán thành công
   - Thanh toán thất bại
   - Hoàn tiền

3. **Promotional** - Khuyến mãi
   - Flash sale
   - Coupon codes
   - Special offers

4. **System** - Hệ thống
   - Bảo trì hệ thống
   - Cập nhật chính sách
   - Thông báo bảo mật

5. **Inventory** - Kho hàng
   - Sản phẩm có hàng trở lại
   - Cảnh báo hết hàng
   - Sản phẩm mới

6. **Security** - Bảo mật
   - Đăng nhập từ thiết bị mới
   - Thay đổi mật khẩu
   - Hoạt động bất thường

## Error Responses

### 400 Bad Request
```json
{
  "success": false,
  "message": "Dữ liệu không hợp lệ",
  "errors": ["Chi tiết lỗi"],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### 401 Unauthorized
```json
{
  "success": false,
  "message": "Không có quyền truy cập",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### 404 Not Found
```json
{
  "success": false,
  "message": "Không tìm thấy thông báo",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "message": "Lỗi hệ thống",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## Best Practices

### 🔔 Real-time Notifications
- Sử dụng SignalR để push thông báo real-time
- Notification badge updates tự động
- Sound notifications cho các thông báo quan trọng

### 📱 Mobile Push Notifications
- Tích hợp với Firebase Cloud Messaging (FCM)
- Rich notifications với hình ảnh và actions
- Deep linking đến các màn hình cụ thể

### 📧 Email Notifications
- HTML email templates đẹp mắt
- Unsubscribe links trong mọi email
- Email tracking và analytics

### 💾 Data Management
- Tự động xóa thông báo cũ sau 90 ngày
- Compress dữ liệu thông báo để tiết kiệm storage
- Index database để tìm kiếm nhanh

## Example Usage

### Frontend Integration
```javascript
// Get notifications
const getNotifications = async (page = 1, unreadOnly = false) => {
  const response = await fetch(`/api/notification?page=${page}&unreadOnly=${unreadOnly}`, {
    headers: {
      'Authorization': `Bearer ${userToken}`
    }
  });
  return await response.json();
};

// Mark as read
const markAsRead = async (notificationId) => {
  await fetch(`/api/notification/${notificationId}/read`, {
    method: 'PATCH',
    headers: {
      'Authorization': `Bearer ${userToken}`
    }
  });
};

// Get unread count
const getUnreadCount = async () => {
  const response = await fetch('/api/notification/unread-count', {
    headers: {
      'Authorization': `Bearer ${userToken}`
    }
  });
  const result = await response.json();
  return result.data;
};
```

### Real-time Updates với SignalR
```javascript
// Connect to notification hub
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/notificationHub")
  .build();

// Listen for new notifications
connection.on("NewNotification", (notification) => {
  // Update UI với notification mới
  updateNotificationBadge();
  showNotificationToast(notification);
});

// Listen for read status updates
connection.on("NotificationRead", (notificationId) => {
  // Update UI khi notification được đánh dấu đã đọc
  markNotificationAsRead(notificationId);
});
```

## Testing

Test các endpoints với file HTTP:
```http
### Get user notifications
GET {{baseUrl}}/api/notification
Authorization: Bearer {{userToken}}

### Mark notification as read
PATCH {{baseUrl}}/api/notification/1/read
Authorization: Bearer {{userToken}}

### Send notification (Staff)
POST {{baseUrl}}/api/notification/send
Authorization: Bearer {{staffToken}}
Content-Type: application/json

{
  "userId": "{{testUserId}}",
  "title": "Test Notification",
  "message": "This is a test notification",
  "type": "System"
}
```

## Conclusion

Hệ thống thông báo SakuraHome cung cấp:
- ✅ Quản lý thông báo cá nhân đầy đủ
- ✅ Cài đặt thông báo linh hoạt
- ✅ Gửi thông báo hàng loạt cho staff
- ✅ Thông báo theo thời gian thực
- ✅ Tích hợp email và push notifications
- ✅ Phân quyền rõ ràng (User/Staff/Admin)
- ✅ Error handling toàn diện
- ✅ Mobile-friendly APIs

Hệ thống đã sẵn sàng để tích hợp với frontend và mobile apps, hỗ trợ đầy đủ các tính năng thông báo hiện đại cho một ứng dụng e-commerce chuyên nghiệp.