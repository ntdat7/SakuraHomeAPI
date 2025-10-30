using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.DTOs.Wishlist.Requests;
using SakuraHomeAPI.DTOs.Wishlist.Responses;
using SakuraHomeAPI.Models.Entities.UserWishlist;
using SakuraHomeAPI.Models.Entities.UserCart;
using SakuraHomeAPI.Services.Interfaces;

namespace SakuraHomeAPI.Services.Implementations
{
    /// <summary>
    /// Wishlist service implementation
    /// </summary>
    public class WishlistService : IWishlistService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICartService _cartService;
        private readonly ILogger<WishlistService> _logger;

        public WishlistService(
            ApplicationDbContext context,
            IMapper mapper,
            ICartService cartService,
            ILogger<WishlistService> logger)
        {
            _context = context;
            _mapper = mapper;
            _cartService = cartService;
            _logger = logger;
        }

        public async Task<ApiResponse<WishlistResponseDto>> GetWishlistAsync(Guid userId, int? wishlistId = null)
        {
            try
            {
                var query = _context.Wishlists
                    .Include(w => w.WishlistItems)
                        .ThenInclude(wi => wi.Product)
                        .ThenInclude(p => p.Brand)
                    .Include(w => w.WishlistItems)
                        .ThenInclude(wi => wi.Product)
                        .ThenInclude(p => p.Category)
                    .Where(w => w.UserId == userId);

                Wishlist? wishlist;
                if (wishlistId.HasValue)
                {
                    wishlist = await query.FirstOrDefaultAsync(w => w.Id == wishlistId.Value);
                }
                else
                {
                    // Get default wishlist
                    wishlist = await query.FirstOrDefaultAsync(w => w.IsDefault);
                    if (wishlist == null)
                    {
                        // Create default wishlist if none exists
                        wishlist = new Wishlist
                        {
                            UserId = userId,
                            Name = "Danh sách yêu thích c?a tôi",
                            IsDefault = true,
                            IsPublic = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.Wishlists.Add(wishlist);
                        await _context.SaveChangesAsync();

                        // Reload with navigation properties
                        wishlist = await query.FirstOrDefaultAsync(w => w.Id == wishlist.Id);
                    }
                }

                if (wishlist == null)
                    return ApiResponse.ErrorResult<WishlistResponseDto>("Wishlist not found");

                var wishlistDto = MapWishlistToDto(wishlist);
                return ApiResponse.SuccessResult(wishlistDto, "Wishlist retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wishlist for user {UserId}", userId);
                return ApiResponse.ErrorResult<WishlistResponseDto>("Failed to retrieve wishlist");
            }
        }

        public async Task<ApiResponse<List<WishlistSummaryDto>>> GetUserWishlistsAsync(Guid userId)
        {
            try
            {
                var wishlists = await _context.Wishlists
                    .Include(w => w.WishlistItems)
                        .ThenInclude(wi => wi.Product)
                    .Where(w => w.UserId == userId)
                    .OrderByDescending(w => w.IsDefault)
                    .ThenBy(w => w.CreatedAt)
                    .ToListAsync();

                var summaries = wishlists.Select(w => new WishlistSummaryDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Description = w.Description,
                    IsPublic = w.IsPublic,
                    IsDefault = w.IsDefault,
                    ItemCount = w.WishlistItems.Count,
                    TotalValue = w.WishlistItems.Sum(wi => wi.Product?.Price ?? 0),
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt
                }).ToList();

                return ApiResponse.SuccessResult(summaries, "User wishlists retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wishlists for user {UserId}", userId);
                return ApiResponse.ErrorResult<List<WishlistSummaryDto>>("Failed to retrieve wishlists");
            }
        }

        public async Task<ApiResponse<WishlistResponseDto>> CreateWishlistAsync(CreateWishlistRequestDto request, Guid userId)
        {
            try
            {
                // Check if this is the first wishlist
                var existingWishlists = await _context.Wishlists
                    .Where(w => w.UserId == userId)
                    .ToListAsync();

                var isFirstWishlist = !existingWishlists.Any();

                var wishlist = new Wishlist
                {
                    UserId = userId,
                    Name = request.Name,
                    Description = request.Description,
                    IsPublic = request.IsPublic,
                    IsDefault = isFirstWishlist, // First wishlist becomes default
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();

                var wishlistDto = MapWishlistToDto(wishlist);
                return ApiResponse.SuccessResult(wishlistDto, "Wishlist created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating wishlist for user {UserId}", userId);
                return ApiResponse.ErrorResult<WishlistResponseDto>("Failed to create wishlist");
            }
        }

        public async Task<ApiResponse<WishlistResponseDto>> UpdateWishlistAsync(int wishlistId, UpdateWishlistRequestDto request, Guid userId)
        {
            try
            {
                var wishlist = await _context.Wishlists
                    .Include(w => w.WishlistItems)
                        .ThenInclude(wi => wi.Product)
                    .FirstOrDefaultAsync(w => w.Id == wishlistId && w.UserId == userId);

                if (wishlist == null)
                    return ApiResponse.ErrorResult<WishlistResponseDto>("Wishlist not found");

                wishlist.Name = request.Name;
                wishlist.Description = request.Description;
                wishlist.IsPublic = request.IsPublic;
                wishlist.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var wishlistDto = MapWishlistToDto(wishlist);
                return ApiResponse.SuccessResult(wishlistDto, "Wishlist updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating wishlist {WishlistId} for user {UserId}", wishlistId, userId);
                return ApiResponse.ErrorResult<WishlistResponseDto>("Failed to update wishlist");
            }
        }

        public async Task<ApiResponse> DeleteWishlistAsync(int wishlistId, Guid userId)
        {
            try
            {
                var wishlist = await _context.Wishlists
                    .FirstOrDefaultAsync(w => w.Id == wishlistId && w.UserId == userId);

                if (wishlist == null)
                    return ApiResponse.ErrorResult("Wishlist not found");

                if (wishlist.IsDefault)
                {
                    // Check if there are other wishlists to promote
                    var otherWishlist = await _context.Wishlists
                        .Where(w => w.UserId == userId && w.Id != wishlistId)
                        .OrderBy(w => w.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (otherWishlist != null)
                    {
                        otherWishlist.IsDefault = true;
                        otherWishlist.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // Hard delete since we don't have soft delete fields
                _context.Wishlists.Remove(wishlist);
                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResult("Wishlist deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting wishlist {WishlistId} for user {UserId}", wishlistId, userId);
                return ApiResponse.ErrorResult("Failed to delete wishlist");
            }
        }

        public async Task<ApiResponse<WishlistResponseDto>> AddItemAsync(AddToWishlistRequestDto request, Guid userId)
        {
            try
            {
                // Get or create default wishlist if none specified
                var wishlistId = request.WishlistId ?? await GetOrCreateDefaultWishlistId(userId);

                var wishlist = await _context.Wishlists
                    .Include(w => w.WishlistItems)
                    .FirstOrDefaultAsync(w => w.Id == wishlistId && w.UserId == userId);

                if (wishlist == null)
                    return ApiResponse.ErrorResult<WishlistResponseDto>("Wishlist not found");

                // Check if product exists
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive);

                if (product == null)
                    return ApiResponse.ErrorResult<WishlistResponseDto>("Product not found");

                // Check if item already exists
                var existingItem = await _context.WishlistItems
                    .FirstOrDefaultAsync(wi => wi.WishlistId == wishlistId && 
                                             wi.ProductId == request.ProductId);

                if (existingItem != null)
                    return ApiResponse.ErrorResult<WishlistResponseDto>("Item already exists in wishlist");

                var wishlistItem = new WishlistItem
                {
                    WishlistId = wishlistId,
                    ProductId = request.ProductId,
                    Notes = request.Notes ?? "",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.WishlistItems.Add(wishlistItem);
                wishlist.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetWishlistAsync(userId, wishlistId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to wishlist for user {UserId}", userId);
                return ApiResponse.ErrorResult<WishlistResponseDto>("Failed to add item to wishlist");
            }
        }

        public async Task<ApiResponse> RemoveItemAsync(RemoveFromWishlistRequestDto request, Guid userId)
        {
            try
            {
                var wishlistItem = await _context.WishlistItems
                    .Include(wi => wi.Wishlist)
                    .FirstOrDefaultAsync(wi => wi.Id == request.WishlistItemId && 
                                             wi.Wishlist.UserId == userId);

                if (wishlistItem == null)
                    return ApiResponse.ErrorResult("Wishlist item not found");

                _context.WishlistItems.Remove(wishlistItem);
                wishlistItem.Wishlist.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResult("Item removed from wishlist successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from wishlist for user {UserId}", userId);
                return ApiResponse.ErrorResult("Failed to remove item from wishlist");
            }
        }

        public async Task<ApiResponse> MoveItemToCartAsync(int wishlistItemId, Guid userId, int quantity = 1)
        {
            try
            {
                var wishlistItem = await _context.WishlistItems
                    .Include(wi => wi.Wishlist)
                    .Include(wi => wi.Product)
                    .FirstOrDefaultAsync(wi => wi.Id == wishlistItemId && 
                                             wi.Wishlist.UserId == userId);

                if (wishlistItem == null)
                    return ApiResponse.ErrorResult("Wishlist item not found");

                // Add to cart
                var addToCartRequest = new DTOs.Cart.Requests.AddToCartRequestDto
                {
                    ProductId = wishlistItem.ProductId,
                    ProductVariantId = null, // WishlistItem doesn't have ProductVariantId
                    Quantity = quantity
                };

                var cartResult = await _cartService.AddToCartAsync(addToCartRequest, userId);
                if (!cartResult.IsSuccess)
                    return ApiResponse.ErrorResult(cartResult.ErrorMessage ?? "Failed to add to cart", cartResult.Errors?.ToList());

                // Remove from wishlist
                _context.WishlistItems.Remove(wishlistItem);
                wishlistItem.Wishlist.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResult("Item moved to cart successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving wishlist item {ItemId} to cart for user {UserId}", wishlistItemId, userId);
                return ApiResponse.ErrorResult("Failed to move item to cart");
            }
        }

        public async Task<ApiResponse<WishlistResponseDto>> BulkAddItemsAsync(BulkAddToWishlistRequestDto request, Guid userId)
        {
            try
            {
                var wishlistId = request.WishlistId ?? await GetOrCreateDefaultWishlistId(userId);
                var errors = new List<string>();

                foreach (var item in request.Items)
                {
                    try
                    {
                        await AddItemAsync(new AddToWishlistRequestDto
                        {
                            ProductId = item.ProductId,
                            ProductVariantId = null, // Not used in this entity model
                            WishlistId = wishlistId,
                            Notes = item.Notes
                        }, userId);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to add product {item.ProductId}: {ex.Message}");
                    }
                }

                var result = await GetWishlistAsync(userId, wishlistId);
                if (errors.Any())
                {
                    result.Errors = errors;
                    result.Message = "Some items could not be added to wishlist";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk adding items to wishlist for user {UserId}", userId);
                return ApiResponse.ErrorResult<WishlistResponseDto>("Failed to bulk add items to wishlist");
            }
        }

        public async Task<ApiResponse> BulkRemoveItemsAsync(BulkRemoveFromWishlistRequestDto request, Guid userId)
        {
            try
            {
                var wishlistItems = await _context.WishlistItems
                    .Include(wi => wi.Wishlist)
                    .Where(wi => request.WishlistItemIds.Contains(wi.Id) && 
                               wi.Wishlist.UserId == userId)
                    .ToListAsync();

                if (!wishlistItems.Any())
                    return ApiResponse.ErrorResult("No valid wishlist items found");

                _context.WishlistItems.RemoveRange(wishlistItems);

                // Update wishlist timestamps
                var wishlists = wishlistItems.Select(wi => wi.Wishlist).Distinct();
                foreach (var wishlist in wishlists)
                {
                    wishlist.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResult($"Removed {wishlistItems.Count} items from wishlist");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk removing items from wishlist for user {UserId}", userId);
                return ApiResponse.ErrorResult("Failed to bulk remove items from wishlist");
            }
        }

        public async Task<ApiResponse> MoveAllToCartAsync(int wishlistId, Guid userId)
        {
            try
            {
                var wishlistItems = await _context.WishlistItems
                    .Include(wi => wi.Wishlist)
                    .Include(wi => wi.Product)
                    .Where(wi => wi.WishlistId == wishlistId && 
                               wi.Wishlist.UserId == userId)
                    .ToListAsync();

                if (!wishlistItems.Any())
                    return ApiResponse.ErrorResult("No items found in wishlist");

                var errors = new List<string>();
                var successCount = 0;

                foreach (var item in wishlistItems)
                {
                    try
                    {
                        var addToCartRequest = new DTOs.Cart.Requests.AddToCartRequestDto
                        {
                            ProductId = item.ProductId,
                            ProductVariantId = null,
                            Quantity = 1
                        };

                        var cartResult = await _cartService.AddToCartAsync(addToCartRequest, userId);
                        if (cartResult.IsSuccess)
                        {
                            _context.WishlistItems.Remove(item);
                            successCount++;
                        }
                        else
                        {
                            errors.Add($"Failed to add {item.Product.Name}: {cartResult.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to add {item.Product.Name}: {ex.Message}");
                    }
                }

                if (successCount > 0)
                {
                    var wishlist = wishlistItems.First().Wishlist;
                    wishlist.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                var message = $"Moved {successCount} items to cart";
                if (errors.Any())
                {
                    return ApiResponse.ErrorResult(message, errors);
                }

                return ApiResponse.SuccessResult(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving all items from wishlist {WishlistId} to cart for user {UserId}", wishlistId, userId);
                return ApiResponse.ErrorResult("Failed to move items to cart");
            }
        }

        public async Task<ApiResponse<string>> ShareWishlistAsync(int wishlistId, Guid userId)
        {
            try
            {
                var wishlist = await _context.Wishlists
                    .FirstOrDefaultAsync(w => w.Id == wishlistId && w.UserId == userId);

                if (wishlist == null)
                    return ApiResponse.ErrorResult<string>("Wishlist not found");

                // Generate a simple share token (since entity doesn't have ShareToken field)
                var shareToken = GenerateShareToken();

                return ApiResponse.SuccessResult(shareToken, "Share token generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sharing wishlist {WishlistId} for user {UserId}", wishlistId, userId);
                return ApiResponse.ErrorResult<string>("Failed to share wishlist");
            }
        }

        public async Task<ApiResponse<WishlistResponseDto>> GetSharedWishlistAsync(string shareToken)
        {
            try
            {
                // Since we don't have ShareToken in the entity, this is a placeholder implementation
                // You would need to add ShareToken field to Wishlist entity or implement a different sharing mechanism
                return ApiResponse.ErrorResult<WishlistResponseDto>("Sharing feature not fully implemented");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shared wishlist with token {ShareToken}", shareToken);
                return ApiResponse.ErrorResult<WishlistResponseDto>("Failed to retrieve shared wishlist");
            }
        }

        public async Task<ApiResponse> SetWishlistPrivacyAsync(int wishlistId, bool isPublic, Guid userId)
        {
            try
            {
                var wishlist = await _context.Wishlists
                    .FirstOrDefaultAsync(w => w.Id == wishlistId && w.UserId == userId);

                if (wishlist == null)
                    return ApiResponse.ErrorResult("Wishlist not found");

                wishlist.IsPublic = isPublic;
                wishlist.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResult($"Wishlist privacy set to {(isPublic ? "public" : "private")}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting privacy for wishlist {WishlistId} for user {UserId}", wishlistId, userId);
                return ApiResponse.ErrorResult("Failed to update wishlist privacy");
            }
        }

        #region Private Helper Methods

        private async Task<int> GetOrCreateDefaultWishlistId(Guid userId)
        {
            var defaultWishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.IsDefault);

            if (defaultWishlist != null)
                return defaultWishlist.Id;

            // Create default wishlist
            var wishlist = new Wishlist
            {
                UserId = userId,
                Name = "Danh sách yêu thích c?a tôi",
                IsDefault = true,
                IsPublic = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync();

            return wishlist.Id;
        }

        private string GenerateShareToken()
        {
            return Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
        }

        private WishlistResponseDto MapWishlistToDto(Wishlist wishlist)
        {
            return new WishlistResponseDto
            {
                Id = wishlist.Id,
                UserId = wishlist.UserId,
                Name = wishlist.Name,
                Description = wishlist.Description,
                IsPublic = wishlist.IsPublic,
                IsDefault = wishlist.IsDefault,
                ShareToken = GenerateShareToken(), // Generated since not stored
                Items = wishlist.WishlistItems?.Select(MapWishlistItemToDto).ToList() ?? new List<WishlistItemDto>(),
                ItemCount = wishlist.WishlistItems?.Count ?? 0,
                TotalValue = wishlist.WishlistItems?.Sum(wi => wi.Product?.Price ?? 0) ?? 0,
                CreatedAt = wishlist.CreatedAt,
                UpdatedAt = wishlist.UpdatedAt
            };
        }

        private WishlistItemDto MapWishlistItemToDto(WishlistItem item)
        {
            return new WishlistItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductVariantId = null, // Not used in this entity model
                ProductName = item.Product?.Name ?? "Unknown Product",
                ProductSku = item.Product?.SKU,
                VariantName = null,
                ProductImage = item.Product?.MainImage ?? "",
                ProductSlug = item.Product?.Slug ?? "",
                CurrentPrice = item.Product?.Price ?? 0,
                OriginalPrice = item.Product?.OriginalPrice ?? 0,
                IsOnSale = (item.Product?.OriginalPrice ?? 0) > (item.Product?.Price ?? 0),
                IsInStock = item.Product?.IsInStock ?? false,
                IsAvailable = (item.Product?.IsActive ?? false) && !(item.Product?.IsDeleted ?? true),
                Notes = item.Notes,
                AddedAt = item.CreatedAt,
                BrandName = item.Product?.Brand?.Name ?? "",
                CategoryName = item.Product?.Category?.Name ?? ""
            };
        }

        #endregion
    }
}