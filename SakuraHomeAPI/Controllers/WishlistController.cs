using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakuraHomeAPI.Services.Interfaces;
using SakuraHomeAPI.DTOs.Wishlist.Requests;
using SakuraHomeAPI.DTOs.Wishlist.Responses;
using SakuraHomeAPI.DTOs.Common;
using System.Security.Claims;

namespace SakuraHomeAPI.Controllers
{
    /// <summary>
    /// Wishlist management controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;
        private readonly ILogger<WishlistController> _logger;

        public WishlistController(IWishlistService wishlistService, ILogger<WishlistController> logger)
        {
            _wishlistService = wishlistService;
            _logger = logger;
        }

        /// <summary>
        /// Get user's wishlists
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<WishlistSummaryDto>>>> GetWishlists()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<List<WishlistSummaryDto>>.ErrorResult("User not authenticated"));

                var result = await _wishlistService.GetUserWishlistsAsync(userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<List<WishlistSummaryDto>>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<List<WishlistSummaryDto>>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wishlists for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<List<WishlistSummaryDto>>.ErrorResult("An error occurred while retrieving wishlists"));
            }
        }

        /// <summary>
        /// Get specific wishlist by ID
        /// </summary>
        [HttpGet("{wishlistId}")]
        public async Task<ActionResult<ApiResponseDto<WishlistResponseDto>>> GetWishlist(int wishlistId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<WishlistResponseDto>.ErrorResult("User not authenticated"));

                var result = await _wishlistService.GetWishlistAsync(userId.Value, wishlistId);
                
                if (result.Success)
                    return Ok(ApiResponseDto<WishlistResponseDto>.SuccessResult(result.Data, result.Message));
                
                return NotFound(ApiResponseDto<WishlistResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wishlist {WishlistId} for user {UserId}", wishlistId, GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<WishlistResponseDto>.ErrorResult("An error occurred while retrieving the wishlist"));
            }
        }

        /// <summary>
        /// Get default wishlist
        /// </summary>
        [HttpGet("default")]
        public async Task<ActionResult<ApiResponseDto<WishlistResponseDto>>> GetDefaultWishlist()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<WishlistResponseDto>.ErrorResult("User not authenticated"));

                var result = await _wishlistService.GetWishlistAsync(userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<WishlistResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<WishlistResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default wishlist for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<WishlistResponseDto>.ErrorResult("An error occurred while retrieving the default wishlist"));
            }
        }

        /// <summary>
        /// Create a new wishlist
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<WishlistResponseDto>>> CreateWishlist([FromBody] CreateWishlistRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<WishlistResponseDto>.ErrorResult("User not authenticated"));

                var result = await _wishlistService.CreateWishlistAsync(request, userId.Value);
                
                if (result.Success)
                    return CreatedAtAction(nameof(GetWishlist), new { wishlistId = result.Data.Id }, 
                        ApiResponseDto<WishlistResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<WishlistResponseDto>.ErrorResult(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating wishlist for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<WishlistResponseDto>.ErrorResult("An error occurred while creating the wishlist"));
            }
        }

        /// <summary>
        /// Update wishlist details
        /// </summary>
        [HttpPut("{wishlistId}")]
        public async Task<ActionResult<ApiResponseDto<WishlistResponseDto>>> UpdateWishlist(int wishlistId, [FromBody] UpdateWishlistRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<WishlistResponseDto>.ErrorResult("User not authenticated"));

                var result = await _wishlistService.UpdateWishlistAsync(wishlistId, request, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<WishlistResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<WishlistResponseDto>.ErrorResult(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating wishlist {WishlistId} for user {UserId}", wishlistId, GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<WishlistResponseDto>.ErrorResult("An error occurred while updating the wishlist"));
            }
        }

        /// <summary>
        /// Delete a wishlist
        /// </summary>
        [HttpDelete("{wishlistId}")]
        public async Task<ActionResult<ApiResponseDto>> DeleteWishlist(int wishlistId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto.ErrorResult("User not authenticated"));

                var result = await _wishlistService.DeleteWishlistAsync(wishlistId, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto.SuccessResult(result.Message));
                
                return BadRequest(ApiResponseDto.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting wishlist {WishlistId} for user {UserId}", wishlistId, GetCurrentUserId());
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while deleting the wishlist"));
            }
        }

        /// <summary>
        /// Add item to wishlist
        /// </summary>
        [HttpPost("items")]
        public async Task<ActionResult<ApiResponseDto<WishlistResponseDto>>> AddToWishlist([FromBody] AddToWishlistRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<WishlistResponseDto>.ErrorResult("User not authenticated"));

                var result = await _wishlistService.AddItemAsync(request, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<WishlistResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<WishlistResponseDto>.ErrorResult(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to wishlist for user {UserId}, product {ProductId}", 
                    GetCurrentUserId(), request.ProductId);
                return StatusCode(500, ApiResponseDto<WishlistResponseDto>.ErrorResult("An error occurred while adding item to wishlist"));
            }
        }

        /// <summary>
        /// Remove item from wishlist
        /// </summary>
        [HttpDelete("items")]
        public async Task<ActionResult<ApiResponseDto>> RemoveFromWishlist([FromBody] RemoveFromWishlistRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto.ErrorResult("User not authenticated"));

                var result = await _wishlistService.RemoveItemAsync(request, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto.SuccessResult(result.Message));
                
                return BadRequest(ApiResponseDto.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from wishlist for user {UserId}, item {ItemId}", 
                    GetCurrentUserId(), request.WishlistItemId);
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while removing item from wishlist"));
            }
        }

        /// <summary>
        /// Move wishlist item to cart
        /// </summary>
        [HttpPost("items/{wishlistItemId}/move-to-cart")]
        public async Task<ActionResult<ApiResponseDto>> MoveToCart(int wishlistItemId, [FromQuery] int quantity = 1)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto.ErrorResult("User not authenticated"));

                var result = await _wishlistService.MoveItemToCartAsync(wishlistItemId, userId.Value, quantity);
                
                if (result.Success)
                    return Ok(ApiResponseDto.SuccessResult(result.Message));
                
                return BadRequest(ApiResponseDto.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving wishlist item {ItemId} to cart for user {UserId}", 
                    wishlistItemId, GetCurrentUserId());
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while moving item to cart"));
            }
        }

        /// <summary>
        /// Move all items from wishlist to cart
        /// </summary>
        [HttpPost("{wishlistId}/move-all-to-cart")]
        public async Task<ActionResult<ApiResponseDto>> MoveAllToCart(int wishlistId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto.ErrorResult("User not authenticated"));

                var result = await _wishlistService.MoveAllToCartAsync(wishlistId, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto.SuccessResult(result.Message));
                
                return BadRequest(ApiResponseDto.ErrorResult(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving all items from wishlist {WishlistId} to cart for user {UserId}", 
                    wishlistId, GetCurrentUserId());
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while moving items to cart"));
            }
        }

        /// <summary>
        /// Bulk add items to wishlist
        /// </summary>
        [HttpPost("bulk/add")]
        public async Task<ActionResult<ApiResponseDto<WishlistResponseDto>>> BulkAddToWishlist([FromBody] BulkAddToWishlistRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<WishlistResponseDto>.ErrorResult("User not authenticated"));

                var result = await _wishlistService.BulkAddItemsAsync(request, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<WishlistResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<WishlistResponseDto>.ErrorResult(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk adding items to wishlist for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<WishlistResponseDto>.ErrorResult("An error occurred while adding items to wishlist"));
            }
        }

        /// <summary>
        /// Bulk remove items from wishlist
        /// </summary>
        [HttpDelete("bulk/remove")]
        public async Task<ActionResult<ApiResponseDto>> BulkRemoveFromWishlist([FromBody] BulkRemoveFromWishlistRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto.ErrorResult("User not authenticated"));

                var result = await _wishlistService.BulkRemoveItemsAsync(request, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto.SuccessResult(result.Message));
                
                return BadRequest(ApiResponseDto.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk removing items from wishlist for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while removing items from wishlist"));
            }
        }

        /// <summary>
        /// Share wishlist (generate share link)
        /// </summary>
        [HttpPost("{wishlistId}/share")]
        public async Task<ActionResult<ApiResponseDto<string>>> ShareWishlist(int wishlistId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<string>.ErrorResult("User not authenticated"));

                var result = await _wishlistService.ShareWishlistAsync(wishlistId, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<string>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<string>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sharing wishlist {WishlistId} for user {UserId}", wishlistId, GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<string>.ErrorResult("An error occurred while sharing the wishlist"));
            }
        }

        /// <summary>
        /// Get shared wishlist by token (public endpoint)
        /// </summary>
        [HttpGet("shared/{shareToken}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<WishlistResponseDto>>> GetSharedWishlist(string shareToken)
        {
            try
            {
                var result = await _wishlistService.GetSharedWishlistAsync(shareToken);
                
                if (result.Success)
                    return Ok(ApiResponseDto<WishlistResponseDto>.SuccessResult(result.Data, result.Message));
                
                return NotFound(ApiResponseDto<WishlistResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shared wishlist with token {ShareToken}", shareToken);
                return StatusCode(500, ApiResponseDto<WishlistResponseDto>.ErrorResult("An error occurred while retrieving the shared wishlist"));
            }
        }

        /// <summary>
        /// Set wishlist privacy
        /// </summary>
        [HttpPatch("{wishlistId}/privacy")]
        public async Task<ActionResult<ApiResponseDto>> SetWishlistPrivacy(int wishlistId, [FromQuery] bool isPublic)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto.ErrorResult("User not authenticated"));

                var result = await _wishlistService.SetWishlistPrivacyAsync(wishlistId, isPublic, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto.SuccessResult(result.Message));
                
                return BadRequest(ApiResponseDto.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting privacy for wishlist {WishlistId} for user {UserId}", wishlistId, GetCurrentUserId());
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while updating wishlist privacy"));
            }
        }

        #region Helper Methods

        private Guid? GetCurrentUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdString, out var userId))
            {
                return userId;
            }
            return null;
        }

        #endregion
    }
}