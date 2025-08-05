# Sakura Home API - Endpoints Reference

## Authentication Endpoints
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - User logout
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password with token

## User Management Endpoints
- `GET /api/user/profile` - Get current user profile
- `PUT /api/user/profile` - Update user profile
- `PUT /api/user/change-password` - Change password
- `GET /api/user/addresses` - Get user addresses
- `POST /api/user/addresses` - Add new address
- `PUT /api/user/addresses/{id}` - Update address
- `DELETE /api/user/addresses/{id}` - Delete address

## Cart Management Endpoints ?
- `GET /api/cart` - Get user's cart
- `POST /api/cart/add` - Add item to cart
- `PUT /api/cart/update` - Update cart item quantity
- `DELETE /api/cart/remove/{productVariantId}` - Remove item from cart
- `DELETE /api/cart/clear` - Clear entire cart
- `POST /api/cart/apply-coupon` - Apply coupon to cart
- `DELETE /api/cart/remove-coupon` - Remove applied coupon

## Wishlist Management Endpoints ?
- `GET /api/wishlist` - Get user's wishlist
- `POST /api/wishlist/add` - Add item to wishlist
- `DELETE /api/wishlist/remove/{productId}` - Remove item from wishlist
- `DELETE /api/wishlist/clear` - Clear entire wishlist
- `POST /api/wishlist/move-to-cart/{productId}` - Move item to cart

## Order Management Endpoints ?
- `GET /api/order` - Get user orders (with pagination and filtering)
- `GET /api/order/{id}` - Get specific order details
- `POST /api/order` - Create new order
- `PUT /api/order/{id}/cancel` - Cancel order
- `GET /api/order/{id}/tracking` - Get order tracking information

## Payment Endpoints ?
- `GET /api/payment/methods` - Get available payment methods
- `POST /api/payment` - Create payment transaction
- `GET /api/payment/{transactionId}` - Get payment details
- `GET /api/payment/my-payments` - Get user payment history
- `GET /api/payment/order/{orderId}` - Get order payments
- `POST /api/payment/calculate-fee` - Calculate payment fee

## Product Catalog Endpoints
- `GET /api/products` - Get products (with filtering, sorting, pagination)
- `GET /api/products/{id}` - Get product details
- `GET /api/products/featured` - Get featured products
- `GET /api/products/best-sellers` - Get best selling products
- `GET /api/products/new-arrivals` - Get new arrival products
- `GET /api/products/{id}/variants` - Get product variants
- `GET /api/products/{id}/reviews` - Get product reviews

## Category & Brand Endpoints ?
- `GET /api/categories` - Get all categories
- `GET /api/categories/{id}` - Get category details
- `GET /api/categories/{id}/products` - Get products in category
- `GET /api/brands` - Get all brands
- `GET /api/brands/{id}` - Get brand details
- `GET /api/brands/{id}/products` - Get products by brand

## Search & Filter Endpoints
- `GET /api/search` - Search products
- `GET /api/search/suggestions` - Get search suggestions
- `GET /api/filters` - Get available filters
- `GET /api/filters/categories/{categoryId}` - Get filters for category

## Review Endpoints
- `GET /api/reviews/product/{productId}` - Get product reviews
- `POST /api/reviews` - Create product review
- `PUT /api/reviews/{id}` - Update review
- `DELETE /api/reviews/{id}` - Delete review
- `POST /api/reviews/{id}/vote` - Vote on review (helpful/not helpful)

## Admin Endpoints
- `GET /api/admin/dashboard` - Admin dashboard data
- `GET /api/admin/orders` - Manage orders
- `PUT /api/admin/orders/{id}/status` - Update order status
- `GET /api/admin/users` - Manage users
- `GET /api/admin/products` - Manage products
- `POST /api/admin/products` - Create product
- `PUT /api/admin/products/{id}` - Update product
- `DELETE /api/admin/products/{id}` - Delete product

## Notification Endpoints
- `GET /api/notifications` - Get user notifications
- `PUT /api/notifications/{id}/read` - Mark notification as read
- `PUT /api/notifications/mark-all-read` - Mark all notifications as read
- `DELETE /api/notifications/{id}` - Delete notification

## System Endpoints
- `GET /api/system/settings` - Get public system settings
- `GET /api/system/banners` - Get active banners
- `GET /api/system/shipping-zones` - Get shipping zones and rates
- `POST /api/contact` - Submit contact message
- `GET /api/health` - Health check endpoint

## Implementation Status
- ? **Completed**: Cart, Wishlist, Orders, Categories/Brands, Payments (COD only)
- ?? **In Progress**: None
- ? **Pending**: Product Catalog, Search, Reviews, Admin, Notifications, System

## Payment Methods Status
- ? **COD (Cash on Delivery)**: Fully implemented
- ? **VNPay**: Not implemented yet
- ? **MoMo**: Not implemented yet
- ? **Bank Transfer**: Not implemented yet
- ? **Credit/Debit Cards**: Not implemented yet

## Notes
- All endpoints return responses in the `ApiResponseDto<T>` format
- Authentication endpoints use JWT tokens
- Pagination uses `page` and `pageSize` query parameters
- All authenticated endpoints require Bearer token in Authorization header
- Error responses include appropriate HTTP status codes and error messages