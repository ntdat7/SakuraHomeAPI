using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Services.Common;
using SakuraHomeAPI.DTOs.Cart.Requests;
using SakuraHomeAPI.DTOs.Cart.Responses;

namespace SakuraHomeAPI.Services.Interfaces
{
    /// <summary>
    /// Shopping cart service interface
    /// </summary>
    public interface ICartService
    {
        // Cart Management
        Task<ServiceResult<CartResponseDto>> GetCartAsync(Guid? userId, string? sessionId = null);
        Task<ServiceResult<CartResponseDto>> AddToCartAsync(AddToCartRequestDto request, Guid? userId = null, string? sessionId = null);
        Task<ServiceResult<CartResponseDto>> UpdateCartItemAsync(UpdateCartItemRequestDto request, Guid? userId = null, string? sessionId = null);
        Task<ServiceResult<string>> RemoveFromCartAsync(RemoveFromCartRequestDto request, Guid? userId = null, string? sessionId = null);
        Task<ServiceResult<string>> ClearCartAsync(Guid? userId = null, string? sessionId = null);
        
        // Cart Operations
        Task<ServiceResult<CartSummaryDto>> GetCartSummaryAsync(Guid? userId, string? sessionId = null);
        Task<ServiceResult<string>> ValidateCartAsync(Guid? userId, string? sessionId = null);
        Task<ServiceResult<CartResponseDto>> MergeCartsAsync(Guid userId, string sessionId);
        
        // Bulk Operations
        Task<ServiceResult<CartResponseDto>> BulkUpdateCartAsync(BulkUpdateCartRequestDto request, Guid? userId = null, string? sessionId = null);
        Task<ServiceResult<string>> ApplyCouponAsync(string couponCode, Guid? userId = null, string? sessionId = null);
        Task<ServiceResult<string>> RemoveCouponAsync(Guid? userId = null, string? sessionId = null);
    }
}