using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.Services.Interfaces;
using SakuraHomeAPI.Services.Common;
using SakuraHomeAPI.DTOs.Cart.Requests;
using SakuraHomeAPI.DTOs.Cart.Responses;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Models.Entities.UserCart;
using SakuraHomeAPI.Models.Entities.Products;
using AutoMapper;

namespace SakuraHomeAPI.Services.Implementations
{
    /// <summary>
    /// Shopping cart service implementation
    /// </summary>
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CartService> _logger;

        public CartService(ApplicationDbContext context, IMapper mapper, ILogger<CartService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResult<CartResponseDto>> GetCartAsync(Guid? userId, string? sessionId = null)
        {
            try
            {
                var cart = await FindOrCreateCartAsync(userId, sessionId);
                
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Product)
                        .ThenInclude(p => p.Brand)
                    .Include(ci => ci.Product)
                        .ThenInclude(p => p.Category)
                    .Include(ci => ci.ProductVariant)
                    .Where(ci => ci.CartId == cart.Id)
                    .ToListAsync();

                // Update cart items with current prices and validate
                foreach (var item in cartItems)
                {
                    item.CalculateTotal();
                }

                await _context.SaveChangesAsync();

                var cartDto = MapCartToDto(cart, cartItems);
                return ServiceResult<CartResponseDto>.Success(cartDto, "Cart retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart for user {UserId}", userId);
                return ServiceResult<CartResponseDto>.Failure("Failed to retrieve cart");
            }
        }

        public async Task<ServiceResult<CartResponseDto>> AddToCartAsync(AddToCartRequestDto request, Guid? userId = null, string? sessionId = null)
        {
            try
            {
                // Validate product exists and is available
                var product = await _context.Products
                    .Include(p => p.Variants)
                    .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive && !p.IsDeleted);

                if (product == null)
                    return ServiceResult<CartResponseDto>.Failure("Product not found or unavailable");

                ProductVariant? variant = null;
                if (request.ProductVariantId.HasValue)
                {
                    variant = product.Variants.FirstOrDefault(v => v.Id == request.ProductVariantId.Value && v.IsActive && !v.IsDeleted);
                    if (variant == null)
                        return ServiceResult<CartResponseDto>.Failure("Product variant not found or unavailable");
                }

                // Check stock availability
                var availableStock = variant?.Stock ?? product.Stock;
                var allowBackorder = variant?.Product?.AllowBackorder ?? product.AllowBackorder;
                
                if (availableStock < request.Quantity && !allowBackorder)
                    return ServiceResult<CartResponseDto>.Failure($"Only {availableStock} items available in stock");

                var cart = await FindOrCreateCartAsync(userId, sessionId);

                // Check if item already exists in cart
                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && 
                                             ci.ProductId == request.ProductId && 
                                             ci.ProductVariantId == request.ProductVariantId);

                if (existingItem != null)
                {
                    // Update existing item
                    var newQuantity = existingItem.Quantity + request.Quantity;
                    if (availableStock < newQuantity && !allowBackorder)
                        return ServiceResult<CartResponseDto>.Failure($"Cannot add {request.Quantity} more items. Only {availableStock - existingItem.Quantity} can be added");

                    existingItem.Quantity = newQuantity;
                    // Only update options if provided, otherwise keep existing values
                    existingItem.CustomOptions = !string.IsNullOrEmpty(request.CustomOptions) ? request.CustomOptions : existingItem.CustomOptions;
                    existingItem.GiftMessage = !string.IsNullOrEmpty(request.GiftMessage) ? request.GiftMessage : existingItem.GiftMessage;
                    existingItem.IsGift = request.IsGift;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    existingItem.CalculateTotal();
                }
                else
                {
                    // Create new cart item
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = request.ProductId,
                        ProductVariantId = request.ProductVariantId,
                        Quantity = request.Quantity,
                        UnitPrice = variant?.Price ?? product.Price,
                        CustomOptions = request.CustomOptions ?? string.Empty, // Ensure not null
                        GiftMessage = request.GiftMessage ?? string.Empty, // Ensure not null
                        IsGift = request.IsGift,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    cartItem.CalculateTotal();
                    _context.CartItems.Add(cartItem);
                }

                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetCartAsync(userId, sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {ProductId} to cart", request.ProductId);
                return ServiceResult<CartResponseDto>.Failure("Failed to add item to cart");
            }
        }

        public async Task<ServiceResult<CartResponseDto>> UpdateCartItemAsync(UpdateCartItemRequestDto request, Guid? userId = null, string? sessionId = null)
        {
            try
            {
                var cart = await FindCartAsync(userId, sessionId);
                if (cart == null)
                    return ServiceResult<CartResponseDto>.Failure("Cart not found");

                var cartItem = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Include(ci => ci.ProductVariant)
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && 
                                             ci.ProductId == request.ProductId && 
                                             ci.ProductVariantId == request.ProductVariantId);

                if (cartItem == null)
                    return ServiceResult<CartResponseDto>.Failure("Item not found in cart");

                if (request.Quantity <= 0)
                {
                    // Remove item if quantity is 0 or negative
                    _context.CartItems.Remove(cartItem);
                }
                else
                {
                    // Validate stock
                    var availableStock = cartItem.ProductVariant?.Stock ?? cartItem.Product.Stock;
                    var allowBackorder = cartItem.ProductVariant?.Product?.AllowBackorder ?? cartItem.Product.AllowBackorder;
                    
                    if (availableStock < request.Quantity && !allowBackorder)
                        return ServiceResult<CartResponseDto>.Failure($"Only {availableStock} items available in stock");

                    cartItem.Quantity = request.Quantity;
                    cartItem.CustomOptions = request.CustomOptions ?? cartItem.CustomOptions ?? string.Empty; // Preserve existing or use empty
                    cartItem.GiftMessage = request.GiftMessage ?? cartItem.GiftMessage ?? string.Empty; // Preserve existing or use empty
                    cartItem.IsGift = request.IsGift;
                    cartItem.UpdatedAt = DateTime.UtcNow;
                    cartItem.CalculateTotal();
                }

                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetCartAsync(userId, sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item for product {ProductId}", request.ProductId);
                return ServiceResult<CartResponseDto>.Failure("Failed to update cart item");
            }
        }

        public async Task<ServiceResult<string>> RemoveFromCartAsync(RemoveFromCartRequestDto request, Guid? userId = null, string? sessionId = null)
        {
            try
            {
                var cart = await FindCartAsync(userId, sessionId);
                if (cart == null)
                    return ServiceResult<string>.Failure("Cart not found");

                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && 
                                             ci.ProductId == request.ProductId && 
                                             ci.ProductVariantId == request.ProductVariantId);

                if (cartItem == null)
                    return ServiceResult<string>.Failure("Item not found in cart");

                _context.CartItems.Remove(cartItem);
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ServiceResult<string>.Success("Item removed from cart successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart for product {ProductId}", request.ProductId);
                return ServiceResult<string>.Failure("Failed to remove item from cart");
            }
        }

        public async Task<ServiceResult<string>> ClearCartAsync(Guid? userId = null, string? sessionId = null)
        {
            try
            {
                var cart = await FindCartAsync(userId, sessionId);
                if (cart == null)
                    return ServiceResult<string>.Success("Cart is already empty");

                var cartItems = await _context.CartItems
                    .Where(ci => ci.CartId == cart.Id)
                    .ToListAsync();

                _context.CartItems.RemoveRange(cartItems);
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ServiceResult<string>.Success("Cart cleared successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                return ServiceResult<string>.Failure("Failed to clear cart");
            }
        }

        public async Task<ServiceResult<CartSummaryDto>> GetCartSummaryAsync(Guid? userId, string? sessionId = null)
        {
            try
            {
                var cart = await FindCartAsync(userId, sessionId);
                if (cart == null)
                {
                    return ServiceResult<CartSummaryDto>.Success(new CartSummaryDto
                    {
                        TotalItems = 0,
                        UniqueItems = 0,
                        SubTotal = 0,
                        Total = 0,
                        IsEmpty = true,
                        HasErrors = false
                    }, "Empty cart");
                }

                var cartItems = await _context.CartItems
                    .Where(ci => ci.CartId == cart.Id)
                    .ToListAsync();

                var summary = new CartSummaryDto
                {
                    TotalItems = cartItems.Sum(ci => ci.Quantity),
                    UniqueItems = cartItems.Count,
                    SubTotal = cartItems.Sum(ci => ci.TotalPrice),
                    Total = cartItems.Sum(ci => ci.TotalPrice), // TODO: Add tax, shipping, discounts
                    IsEmpty = !cartItems.Any(),
                    HasErrors = false
                };

                return ServiceResult<CartSummaryDto>.Success(summary, "Cart summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart summary for user {UserId}", userId);
                return ServiceResult<CartSummaryDto>.Failure("Failed to get cart summary");
            }
        }

        public async Task<ServiceResult<string>> ValidateCartAsync(Guid? userId, string? sessionId = null)
        {
            try
            {
                var cart = await FindCartAsync(userId, sessionId);
                if (cart == null)
                    return ServiceResult<string>.Success("No cart to validate");

                var cartItems = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Include(ci => ci.ProductVariant)
                    .Where(ci => ci.CartId == cart.Id)
                    .ToListAsync();

                var errors = new List<string>();
                bool hasChanges = false;

                foreach (var item in cartItems)
                {
                    var validation = item.Validate();
                    if (!validation.IsValid)
                    {
                        errors.Add($"{item.DisplayName}: {validation.Error}");
                        
                        // Auto-fix quantity if possible
                        if (item.Quantity > item.AvailableStock && item.AvailableStock > 0)
                        {
                            item.Quantity = item.AvailableStock;
                            item.CalculateTotal();
                            hasChanges = true;
                        }
                        else if (!item.IsAvailable)
                        {
                            _context.CartItems.Remove(item);
                            hasChanges = true;
                        }
                    }
                }

                if (hasChanges)
                {
                    cart.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                if (errors.Any())
                    return ServiceResult<string>.Failure(errors, "Cart validation failed");

                return ServiceResult<string>.Success("Cart is valid");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating cart for user {UserId}", userId);
                return ServiceResult<string>.Failure("Failed to validate cart");
            }
        }

        public async Task<ServiceResult<CartResponseDto>> MergeCartsAsync(Guid userId, string sessionId)
        {
            try
            {
                var userCart = await FindCartAsync(userId, null);
                var guestCart = await FindCartAsync(null, sessionId);

                if (guestCart == null)
                    return await GetCartAsync(userId);

                if (userCart == null)
                {
                    // Convert guest cart to user cart
                    guestCart.UserId = userId;
                    guestCart.SessionId = null;
                    guestCart.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return await GetCartAsync(userId);
                }

                // Merge guest cart items into user cart
                var guestItems = await _context.CartItems
                    .Where(ci => ci.CartId == guestCart.Id)
                    .ToListAsync();

                foreach (var guestItem in guestItems)
                {
                    var existingItem = await _context.CartItems
                        .FirstOrDefaultAsync(ci => ci.CartId == userCart.Id && 
                                                 ci.ProductId == guestItem.ProductId && 
                                                 ci.ProductVariantId == guestItem.ProductVariantId);

                    if (existingItem != null)
                    {
                        existingItem.Quantity += guestItem.Quantity;
                        existingItem.UpdatedAt = DateTime.UtcNow;
                        existingItem.CalculateTotal();
                    }
                    else
                    {
                        guestItem.CartId = userCart.Id;
                        guestItem.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // Remove guest cart
                _context.CartItems.RemoveRange(guestItems.Where(gi => 
                    _context.CartItems.Any(ci => ci.CartId == userCart.Id && 
                                                ci.ProductId == gi.ProductId && 
                                                ci.ProductVariantId == gi.ProductVariantId)));
                _context.Carts.Remove(guestCart);

                userCart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetCartAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error merging carts for user {UserId}", userId);
                return ServiceResult<CartResponseDto>.Failure("Failed to merge carts");
            }
        }

        public async Task<ServiceResult<CartResponseDto>> BulkUpdateCartAsync(BulkUpdateCartRequestDto request, Guid? userId = null, string? sessionId = null)
        {
            try
            {
                var cart = await FindCartAsync(userId, sessionId);
                if (cart == null)
                    return ServiceResult<CartResponseDto>.Failure("Cart not found");

                var errors = new List<string>();

                foreach (var updateRequest in request.Items)
                {
                    try
                    {
                        await UpdateCartItemAsync(updateRequest, userId, sessionId);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to update item {updateRequest.ProductId}: {ex.Message}");
                    }
                }

                var result = await GetCartAsync(userId, sessionId);
                if (errors.Any())
                {
                    return ServiceResult<CartResponseDto>.Failure(errors, "Cart updated with some errors");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating cart for user {UserId}", userId);
                return ServiceResult<CartResponseDto>.Failure("Failed to bulk update cart");
            }
        }

        public async Task<ServiceResult<string>> ApplyCouponAsync(string couponCode, Guid? userId = null, string? sessionId = null)
        {
            // TODO: Implement coupon logic when Coupon entity is ready
            return ServiceResult<string>.Failure("Coupon functionality not yet implemented");
        }

        public async Task<ServiceResult<string>> RemoveCouponAsync(Guid? userId = null, string? sessionId = null)
        {
            // TODO: Implement coupon logic when Coupon entity is ready
            return ServiceResult<string>.Failure("Coupon functionality not yet implemented");
        }

        #region Private Helper Methods

        private async Task<Cart> FindOrCreateCartAsync(Guid? userId, string? sessionId)
        {
            var cart = await FindCartAsync(userId, sessionId);
            if (cart != null) return cart;

            cart = new Cart
            {
                UserId = userId,
                SessionId = userId.HasValue ? $"user-{userId}" : (sessionId ?? $"guest-{Guid.NewGuid()}"), // Always provide a value
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        private async Task<Cart?> FindCartAsync(Guid? userId, string? sessionId)
        {
            if (userId.HasValue)
            {
                return await _context.Carts
                    .FirstOrDefaultAsync(c => c.UserId == userId);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                return await _context.Carts
                    .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.UserId == null);
            }

            return null;
        }

        private CartResponseDto MapCartToDto(Cart cart, List<CartItem> cartItems)
        {
            return new CartResponseDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                SessionId = cart.SessionId,
                Items = cartItems.Select(MapCartItemToDto).ToList(),
                TotalItems = cartItems.Sum(ci => ci.Quantity),
                UniqueItems = cartItems.Count,
                SubTotal = cartItems.Sum(ci => ci.TotalPrice),
                Total = cartItems.Sum(ci => ci.TotalPrice), // TODO: Add tax, shipping, discounts
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                IsEmpty = !cartItems.Any()
            };
        }

        private CartItemDto MapCartItemToDto(CartItem cartItem)
        {
            return new CartItemDto
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductVariantId = cartItem.ProductVariantId,
                ProductName = cartItem.Product?.Name ?? "Unknown Product",
                ProductSku = cartItem.Product?.SKU,
                VariantName = cartItem.ProductVariant?.Name,
                ProductImage = cartItem.ProductVariant?.ImageUrl ?? cartItem.Product?.MainImage ?? "",
                ProductSlug = cartItem.Product?.Slug ?? "",
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.UnitPrice,
                OriginalPrice = cartItem.ProductVariant?.OriginalPrice ?? cartItem.Product?.OriginalPrice ?? cartItem.UnitPrice,
                TotalPrice = cartItem.TotalPrice,
                IsInStock = cartItem.IsInStock,
                AvailableStock = cartItem.AvailableStock,
                IsOnSale = (cartItem.ProductVariant?.OriginalPrice ?? cartItem.Product?.OriginalPrice ?? 0) > cartItem.UnitPrice,
                CustomOptions = cartItem.CustomOptions,
                GiftMessage = cartItem.GiftMessage,
                IsGift = cartItem.IsGift,
                IsAvailable = cartItem.IsAvailable
            };
        }

        #endregion
    }
}