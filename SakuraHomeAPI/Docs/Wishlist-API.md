# ?? Wishlist Management API Documentation

## ?? Overview

The Wishlist Management System allows users to create and manage multiple wishlists, add/remove products, and organize their favorite items. Users can create multiple wishlists for different purposes (e.g., "Birthday Gifts", "Home Essentials", "Favorites").

## ?? Base URL
```
https://localhost:7240/api/wishlist
```

## ?? Authentication
All endpoints require authentication except for shared wishlist viewing. Include the JWT token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

## ?? API Endpoints

### ?? **Wishlist Management**

#### 1. Get User's Wishlists
```http
GET /api/wishlist
```

**Description:** Retrieve all wishlists for the authenticated user.

**Response:**
```json
{
  "success": true,
  "message": "User wishlists retrieved successfully",
  "data": [
    {
      "id": 1,
      "name": "Danh sách yêu thích c?a tôi",
      "description": "My default wishlist",
      "isPublic": false,
      "isDefault": true,
      "itemCount": 5,
      "totalValue": 450000,
      "createdAt": "2024-01-01T10:00:00Z",
      "updatedAt": "2024-01-02T15:30:00Z"
    }
  ]
}
```

#### 2. Get Default Wishlist
```http
GET /api/wishlist/default
```

**Description:** Get the user's default wishlist. Creates one if none exists.

**Response:**
```json
{
  "success": true,
  "message": "Wishlist retrieved successfully",
  "data": {
    "id": 1,
    "userId": "user-guid",
    "name": "Danh sách yêu thích c?a tôi",
    "description": null,
    "isPublic": false,
    "isDefault": true,
    "shareToken": "ABC123DEF456",
    "items": [
      {
        "id": 1,
        "productId": 1,
        "productVariantId": null,
        "productName": "Pocky Chocolate",
        "productSku": "POCKY001",
        "variantName": null,
        "productImage": "/images/products/pocky-chocolate.jpg",
        "productSlug": "pocky-chocolate",
        "currentPrice": 25000,
        "originalPrice": 30000,
        "isOnSale": true,
        "isInStock": true,
        "isAvailable": true,
        "notes": "Mu?n mua ?? th?",
        "addedAt": "2024-01-01T10:00:00Z",
        "brandName": "Glico",
        "categoryName": "Snacks"
      }
    ],
    "itemCount": 1,
    "totalValue": 25000,
    "createdAt": "2024-01-01T10:00:00Z",
    "updatedAt": "2024-01-02T15:30:00Z"
  }
}
```

#### 3. Get Specific Wishlist
```http
GET /api/wishlist/{wishlistId}
```

**Parameters:**
- `wishlistId` (path): ID of the wishlist

**Response:** Same as default wishlist response

#### 4. Create New Wishlist
```http
POST /api/wishlist
```

**Request Body:**
```json
{
  "name": "Birthday Gifts",
  "description": "Gift ideas for upcoming birthdays",
  "isPublic": false
}
```

**Response:**
```json
{
  "success": true,
  "message": "Wishlist created successfully",
  "data": {
    "id": 2,
    "userId": "user-guid",
    "name": "Birthday Gifts",
    "description": "Gift ideas for upcoming birthdays",
    "isPublic": false,
    "isDefault": false,
    "shareToken": "XYZ789ABC123",
    "items": [],
    "itemCount": 0,
    "totalValue": 0,
    "createdAt": "2024-01-02T16:00:00Z",
    "updatedAt": "2024-01-02T16:00:00Z"
  }
}
```

#### 5. Update Wishlist
```http
PUT /api/wishlist/{wishlistId}
```

**Request Body:**
```json
{
  "name": "Updated Wishlist Name",
  "description": "Updated description",
  "isPublic": true
}
```

**Response:** Returns updated wishlist data

#### 6. Delete Wishlist
```http
DELETE /api/wishlist/{wishlistId}
```

**Response:**
```json
{
  "success": true,
  "message": "Wishlist deleted successfully"
}
```

### ?? **Wishlist Items Management**

#### 7. Add Item to Wishlist
```http
POST /api/wishlist/items
```

**Request Body:**
```json
{
  "productId": 1,
  "wishlistId": 1,
  "notes": "Want to try this product"
}
```

**Notes:**
- If `wishlistId` is null, item will be added to default wishlist
- Prevents duplicate items in the same wishlist

**Response:** Returns updated wishlist with new item

#### 8. Remove Item from Wishlist
```http
DELETE /api/wishlist/items
```

**Request Body:**
```json
{
  "wishlistItemId": 1
}
```

**Response:**
```json
{
  "success": true,
  "message": "Item removed from wishlist successfully"
}
```

#### 9. Move Item to Cart
```http
POST /api/wishlist/items/{wishlistItemId}/move-to-cart?quantity=2
```

**Parameters:**
- `wishlistItemId` (path): ID of the wishlist item
- `quantity` (query): Quantity to add to cart (default: 1)

**Response:**
```json
{
  "success": true,
  "message": "Item moved to cart successfully"
}
```

#### 10. Move All Items to Cart
```http
POST /api/wishlist/{wishlistId}/move-all-to-cart
```

**Description:** Moves all items from the specified wishlist to the user's cart.

**Response:**
```json
{
  "success": true,
  "message": "Moved 5 items to cart"
}
```

### ?? **Bulk Operations**

#### 11. Bulk Add Items
```http
POST /api/wishlist/bulk/add
```

**Request Body:**
```json
{
  "wishlistId": 1,
  "items": [
    {
      "productId": 1,
      "notes": "First product note"
    },
    {
      "productId": 2,
      "notes": "Second product note"
    }
  ]
}
```

**Response:** Returns updated wishlist with all successfully added items

#### 12. Bulk Remove Items
```http
DELETE /api/wishlist/bulk/remove
```

**Request Body:**
```json
{
  "wishlistItemIds": [1, 2, 3]
}
```

**Response:**
```json
{
  "success": true,
  "message": "Removed 3 items from wishlist"
}
```

### ?? **Sharing & Privacy**

#### 13. Share Wishlist
```http
POST /api/wishlist/{wishlistId}/share
```

**Description:** Generate a share token for the wishlist.

**Response:**
```json
{
  "success": true,
  "message": "Share token generated successfully",
  "data": "ABC123DEF456GHI789"
}
```

#### 14. Get Shared Wishlist (Public)
```http
GET /api/wishlist/shared/{shareToken}
```

**Description:** View a shared wishlist using the share token. No authentication required.

**Response:** Returns wishlist data without sensitive user information

#### 15. Set Wishlist Privacy
```http
PATCH /api/wishlist/{wishlistId}/privacy?isPublic=true
```

**Parameters:**
- `isPublic` (query): Set to true for public, false for private

**Response:**
```json
{
  "success": true,
  "message": "Wishlist privacy set to public"
}
```

## ?? Data Models

### WishlistSummaryDto
```json
{
  "id": 1,
  "name": "My Wishlist",
  "description": "My favorite items",
  "isPublic": false,
  "isDefault": true,
  "itemCount": 5,
  "totalValue": 250000,
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-02T15:30:00Z"
}
```

### WishlistResponseDto
```json
{
  "id": 1,
  "userId": "user-guid",
  "name": "My Wishlist",
  "description": "My favorite items",
  "isPublic": false,
  "isDefault": true,
  "shareToken": "ABC123DEF456",
  "items": [WishlistItemDto],
  "itemCount": 5,
  "totalValue": 250000,
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-02T15:30:00Z"
}
```

### WishlistItemDto
```json
{
  "id": 1,
  "productId": 1,
  "productVariantId": null,
  "productName": "Pocky Chocolate",
  "productSku": "POCKY001",
  "variantName": null,
  "productImage": "/images/products/pocky-chocolate.jpg",
  "productSlug": "pocky-chocolate",
  "currentPrice": 25000,
  "originalPrice": 30000,
  "isOnSale": true,
  "isInStock": true,
  "isAvailable": true,
  "notes": "Want to try this",
  "addedAt": "2024-01-01T10:00:00Z",
  "brandName": "Glico",
  "categoryName": "Snacks"
}
```

## ? Error Responses

### Common Error Codes
- `400 Bad Request`: Invalid request data
- `401 Unauthorized`: Authentication required
- `404 Not Found`: Wishlist or item not found
- `409 Conflict`: Duplicate item in wishlist
- `500 Internal Server Error`: Server error

### Error Response Format
```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Detailed error 1", "Detailed error 2"],
  "timestamp": "2024-01-01T10:00:00Z"
}
```

## ??? Business Logic

### Automatic Default Wishlist
- Each user gets a default wishlist created automatically
- First wishlist created becomes the default
- If default wishlist is deleted, next oldest becomes default

### Duplicate Prevention
- Users cannot add the same product to the same wishlist twice
- Adding duplicate returns a conflict error

### Item Movement
- Moving items to cart removes them from wishlist
- Bulk move operations handle errors gracefully
- Failed moves are reported with specific error messages

### Privacy & Sharing
- Private wishlists are only visible to the owner
- Public wishlists can be viewed by anyone with the share token
- Share tokens are generated dynamically (not stored in database)

## ?? Usage Examples

### Create and Manage Wishlist
```javascript
// Create new wishlist
const wishlist = await fetch('/api/wishlist', {
  method: 'POST',
  headers: {
    'Authorization': 'Bearer ' + token,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    name: 'Holiday Shopping',
    description: 'Items for holiday gifts',
    isPublic: false
  })
});

// Add items to wishlist
const addItem = await fetch('/api/wishlist/items', {
  method: 'POST',
  headers: {
    'Authorization': 'Bearer ' + token,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    productId: 1,
    wishlistId: wishlist.data.id,
    notes: 'Great gift idea'
  })
});
```

### Move Items to Cart
```javascript
// Move single item to cart
await fetch('/api/wishlist/items/1/move-to-cart?quantity=2', {
  method: 'POST',
  headers: {
    'Authorization': 'Bearer ' + token
  }
});

// Move all items from wishlist to cart
await fetch('/api/wishlist/1/move-all-to-cart', {
  method: 'POST',
  headers: {
    'Authorization': 'Bearer ' + token
  }
});
```

## ?? Testing

Use the provided test file `Tests/Wishlist.http` to test all endpoints:

1. **Basic Operations**: Create, read, update, delete wishlists
2. **Item Management**: Add, remove, move items
3. **Bulk Operations**: Bulk add/remove items
4. **Sharing**: Generate share tokens and access shared wishlists
5. **Error Scenarios**: Test invalid requests and edge cases

## ? Performance Considerations

- Use pagination for large wishlists (future enhancement)
- Bulk operations are optimized for multiple items
- Share tokens are generated on-demand to avoid storage overhead
- Database indexes on UserId and ProductId for fast lookups

## ?? Future Enhancements

- **Wishlist Templates**: Pre-defined wishlist categories
- **Collaborative Wishlists**: Multiple users can contribute
- **Price Alerts**: Notify when wished items go on sale
- **Gift Registry**: Public wishlists for special events
- **Social Features**: Follow other users' public wishlists
- **Analytics**: Track wishlist trends and popular items