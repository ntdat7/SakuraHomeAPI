using SakuraHomeAPI.Models.DTOs;
using SakuraHomeAPI.DTOs.Wishlist.Requests;
using SakuraHomeAPI.DTOs.Wishlist.Responses;

namespace SakuraHomeAPI.Services.Interfaces
{
    /// <summary>
    /// Wishlist service interface
    /// </summary>
    public interface IWishlistService
    {
        // Wishlist Management
        Task<ApiResponse<WishlistResponseDto>> GetWishlistAsync(Guid userId, int? wishlistId = null);
        Task<ApiResponse<List<WishlistSummaryDto>>> GetUserWishlistsAsync(Guid userId);
        Task<ApiResponse<WishlistResponseDto>> CreateWishlistAsync(CreateWishlistRequestDto request, Guid userId);
        Task<ApiResponse<WishlistResponseDto>> UpdateWishlistAsync(int wishlistId, UpdateWishlistRequestDto request, Guid userId);
        Task<ApiResponse> DeleteWishlistAsync(int wishlistId, Guid userId);
        
        // Wishlist Items
        Task<ApiResponse<WishlistResponseDto>> AddItemAsync(AddToWishlistRequestDto request, Guid userId);
        Task<ApiResponse> RemoveItemAsync(RemoveFromWishlistRequestDto request, Guid userId);
        Task<ApiResponse> MoveItemToCartAsync(int wishlistItemId, Guid userId, int quantity = 1);
        
        // Bulk Operations
        Task<ApiResponse<WishlistResponseDto>> BulkAddItemsAsync(BulkAddToWishlistRequestDto request, Guid userId);
        Task<ApiResponse> BulkRemoveItemsAsync(BulkRemoveFromWishlistRequestDto request, Guid userId);
        Task<ApiResponse> MoveAllToCartAsync(int wishlistId, Guid userId);
        
        // Wishlist Sharing
        Task<ApiResponse<string>> ShareWishlistAsync(int wishlistId, Guid userId);
        Task<ApiResponse<WishlistResponseDto>> GetSharedWishlistAsync(string shareToken);
        Task<ApiResponse> SetWishlistPrivacyAsync(int wishlistId, bool isPublic, Guid userId);
    }
}