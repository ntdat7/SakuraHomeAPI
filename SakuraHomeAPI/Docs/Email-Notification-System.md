# Email Service & Notification System Documentation

## Overview
The Email Service and Notification System provides comprehensive communication features for the Sakura Home platform, including email notifications, in-app notifications, and real-time updates for order status changes.

## Features Implemented

### ?? Email Service
- **SMTP Integration**: Complete email sending with Gmail SMTP
- **Template System**: Dynamic email templates with variable substitution
- **Queue Management**: Email queue for batch processing and retry logic
- **Order Lifecycle Emails**: Welcome, order confirmation, status updates, delivery notifications
- **Security Emails**: Password reset, email verification
- **Promotional Emails**: Marketing campaigns and promotional offers

### ?? Notification Service
- **In-App Notifications**: Real-time notifications for users
- **Bulk Notifications**: Send to multiple users or all users with filters
- **Order Notifications**: Automatic notifications for order status changes
- **System Notifications**: Maintenance alerts, low stock alerts
- **Promotional Notifications**: Marketing campaigns with targeting
- **Notification Preferences**: User-configurable notification settings

## Email Configuration

### SMTP Settings (appsettings.json){
  "Email": {
    "FromEmail": "noreply@sakurahome.vn",
    "FromName": "Sakura Home",
    "SupportEmail": "support@sakurahome.vn",
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "EnableSsl": true,
      "Username": "your-email@gmail.com",
      "Password": "your-app-password"
    }
  }
}
### Gmail Setup
1. Enable 2-Factor Authentication on your Gmail account
2. Generate an App Password: Google Account ? Security ? App passwords
3. Use the app password in the configuration (not your regular password)

## API Endpoints

### User Notification Endpoints
- `GET /api/notification` - Get user notifications
- `GET /api/notification/unread-count` - Get unread count
- `GET /api/notification/{id}` - Get specific notification
- `PATCH /api/notification/{id}/read` - Mark as read
- `PATCH /api/notification/mark-all-read` - Mark all as read
- `DELETE /api/notification/{id}` - Delete notification
- `GET /api/notification/preferences` - Get preferences
- `PUT /api/notification/preferences` - Update preferences

### Admin Notification Endpoints (Staff/Admin Only)
- `POST /api/notification/send` - Send single notification
- `POST /api/notification/send-bulk` - Send bulk notification
- `POST /api/notification/promotional` - Send promotional notification
- `POST /api/notification/maintenance` - Send maintenance notification
- `POST /api/notification/low-stock` - Send low stock alert

## Notification Types

| Type | Description | Auto-triggered |
|------|-------------|----------------|
| General | General notifications | Manual |
| Order | Order-related notifications | ? Auto |
| Payment | Payment confirmations | ? Auto |
| Shipping | Shipping updates | ? Auto |
| Product | Product updates | Manual |
| Promotion | Marketing campaigns | Manual |
| System | System announcements | Manual |
| Security | Security alerts | ? Auto |
| Alert | Important alerts | Manual |

## Email Templates

### Template Variables
Common variables available in all templates:
- `{UserName}` - User's full name
- `{Email}` - User's email address
- `{SupportEmail}` - Support email address
- `{CompanyName}` - Company name

### Order Templates
- **Order Confirmation**: `{OrderNumber}`, `{OrderDate}`, `{TotalAmount}`, `{Items}`, `{ShippingAddress}`
- **Order Status Update**: `{OrderNumber}`, `{NewStatus}`, `{UpdateTime}`, `{TrackingUrl}`
- **Shipment Notification**: `{TrackingNumber}`, `{EstimatedDelivery}`, `{CarrierName}`
- **Delivery Confirmation**: `{DeliveryDate}`, `{ReviewUrl}`

### Security Templates
- **Password Reset**: `{ResetUrl}`, `{ExpiryHours}`
- **Email Verification**: `{VerificationUrl}`
- **Welcome Email**: `{LoginUrl}`, `{WelcomeMessage}`

## Order Integration

The notification system is automatically integrated with the order workflow:

### Order Status Flow
1. **Order Created** ? Order confirmation notification + email
2. **Payment Confirmed** ? Payment confirmation notification
3. **Order Confirmed** ? Order confirmed notification + email
4. **Order Processing** ? Processing notification
5. **Order Shipped** ? Shipment notification + email + tracking info
6. **Order Delivered** ? Delivery confirmation + email + review request
7. **Order Cancelled** ? Cancellation notification + email
8. **Order Returned** ? Return notification + email

### Integration Points// In OrderService when status changes
await _notificationService.SendOrderStatusNotificationAsync(orderId, newStatus);

// In PaymentService when payment confirmed
await _notificationService.SendPaymentConfirmationNotificationAsync(orderId, transactionId);
## Notification Preferences

Users can configure their notification preferences:
{
  "emailNotifications": true,
  "smsNotifications": false,
  "pushNotifications": true,
  "inAppNotifications": true,
  "orderUpdates": true,
  "promotionalOffers": false,
  "securityAlerts": true,
  "productUpdates": true,
  "newsletterSubscription": false,
  "weeklyDigest": false,
  "quietHoursStart": "22:00:00",
  "quietHoursEnd": "08:00:00",
  "noNotificationDays": [0, 6],
  "language": "vi",
  "timeZone": "Asia/Ho_Chi_Minh"
}
## Testing

### Test Email ConfigurationPOST /api/email/test
Content-Type: application/json
{
  "testEmail": "your-test@email.com"
}
### Test Order Notifications
1. Create an order through the Order API
2. Update order status through admin endpoints
3. Verify notifications are sent automatically

### Test Bulk NotificationsPOST /api/notification/send-bulk
{
  "title": "System Maintenance",
  "message": "Scheduled maintenance tonight",
  "type": 7,
  "sendToAll": true,
  "sendEmail": true
}
## Performance Considerations

### Email Queue
- Emails are queued for batch processing
- Retry logic with exponential backoff
- Failed emails are marked and can be retried
- Processing happens every 5 minutes (configurable)

### Notification Cleanup
- Notifications expire after 30 days by default
- Soft delete for user deletion tracking
- Automatic cleanup of expired notifications

### Bulk Operations
- Efficient bulk notification creation
- Filtered targeting by user roles, registration date, activity
- Rate limiting for large campaigns

## Error Handling

### Email Failures
- Failed emails are logged with detailed error messages
- Retry mechanism with increasing delays
- Admin alerts for persistent failures
- Bounce handling for invalid email addresses

### Notification Failures
- Graceful degradation if notification service fails
- Logging of all notification attempts
- Fallback to email if real-time delivery fails

## Security

### Email Security
- SMTP over SSL/TLS
- App passwords instead of regular passwords
- No sensitive data in email logs
- Unsubscribe mechanisms for marketing emails

### Notification Security
- User isolation (users only see their notifications)
- Admin authorization for bulk notifications
- Rate limiting to prevent spam
- Input validation and sanitization

## Future Enhancements

### Short Term
- [ ] SMS notification integration (Twilio)
- [ ] Push notification support (Firebase)
- [ ] Real-time notifications with SignalR
- [ ] Email template editor interface

### Long Term
- [ ] A/B testing for email campaigns
- [ ] Advanced analytics and reporting
- [ ] Machine learning for optimal send times
- [ ] Multi-language email templates
- [ ] Advanced segmentation and targeting

## Monitoring

### Email Metrics
- Delivery rates
- Open rates (with tracking pixels)
- Click-through rates
- Bounce rates
- Unsubscribe rates

### Notification Metrics
- Read rates
- Response times
- User engagement
- System performance

## Troubleshooting

### Common Email Issues
1. **Authentication Failed**: Check Gmail app password
2. **SSL/TLS Errors**: Verify SMTP settings and firewall
3. **Rate Limiting**: Gmail has sending limits, use queue for high volume
4. **Bounced Emails**: Implement bounce handling and list cleaning

### Common Notification Issues
1. **Missing Notifications**: Check user preferences and filtering
2. **Duplicate Notifications**: Verify idempotency in service calls
3. **Performance Issues**: Monitor queue size and processing times

## Configuration Examples

### Development Settings{
  "Email": {
    "Smtp": {
      "Host": "localhost",
      "Port": 1025,
      "EnableSsl": false
    }
  },
  "Notification": {
    "EnableRealTime": false,
    "ProcessingIntervalMinutes": 1
  }
}
### Production Settings{
  "Email": {
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "EnableSsl": true
    }
  },
  "Notification": {
    "EnableRealTime": true,
    "ProcessingIntervalMinutes": 5,
    "MaxNotificationsPerUser": 1000
  }
}
This comprehensive Email and Notification system provides a solid foundation for user communication and can be extended with additional features as needed.