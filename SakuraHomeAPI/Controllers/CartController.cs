using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakuraHomeAPI.Services.Interfaces;
using SakuraHomeAPI.DTOs.Cart.Requests;
using SakuraHomeAPI.DTOs.Cart.Responses;
using SakuraHomeAPI.DTOs.Common;
using System.Security.Claims;

namespace SakuraHomeAPI.Controllers
{
    /// <summary>
    /// Shopping cart management controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        /// <summary>
        /// Get user's shopping cart
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<CartResponseDto>>> GetCart([FromQuery] string? sessionId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cartService.GetCartAsync(userId, sessionId);
                
                if (result.IsSuccess)
                    return Ok(ApiResponseDto<CartResponseDto>.SuccessResult(result.Data, result.ErrorMessage));
                
                return BadRequest(ApiResponseDto<CartResponseDto>.ErrorResult(result.ErrorMessage, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<CartResponseDto>.ErrorResult("An error occurred while retrieving the cart"));
            }
        }

        /// <summary>
        /// Get cart summary (lightweight version)
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponseDto<CartSummaryDto>>> GetCartSummary([FromQuery] string? sessionId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cartService.GetCartSummaryAsync(userId, sessionId);
                
                if (result.IsSuccess)
                    return Ok(ApiResponseDto<CartSummaryDto>.SuccessResult(result.Data, result.ErrorMessage));
                
                return BadRequest(ApiResponseDto<CartSummaryDto>.ErrorResult(result.ErrorMessage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart summary for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<CartSummaryDto>.ErrorResult("An error occurred while retrieving the cart summary"));
            }
        }

        /// <summary>
        /// Add item to cart
        /// </summary>
        [HttpPost("items")]
        public async Task<ActionResult<ApiResponseDto<CartResponseDto>>> AddToCart(
            [FromBody] AddToCartRequestDto request, 
            [FromQuery] string? sessionId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cartService.AddToCartAsync(request, userId, sessionId);
                
                if (result.IsSuccess)
                    return Ok(ApiResponseDto<CartResponseDto>.SuccessResult(result.Data, result.ErrorMessage));
                
                return BadRequest(ApiResponseDto<CartResponseDto>.ErrorResult(result.ErrorMessage, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart for user {UserId}, product {ProductId}", 
                    GetCurrentUserId(), request.ProductId);
                return StatusCode(500, ApiResponseDto<CartResponseDto>.ErrorResult("An error occurred while adding item to cart"));
            }
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        [HttpPut("items")]
        public async Task<ActionResult<ApiResponseDto<CartResponseDto>>> UpdateCartItem(
            [FromBody] UpdateCartItemRequestDto request,
            [FromQuery] string? sessionId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cartService.UpdateCartItemAsync(request, userId, sessionId);
                
                if (result.IsSuccess)
                    return Ok(ApiResponseDto<CartResponseDto>.SuccessResult(result.Data, result.ErrorMessage));
                
                return BadRequest(ApiResponseDto<CartResponseDto>.ErrorResult(result.ErrorMessage, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item for user {UserId}, product {ProductId}", 
                    GetCurrentUserId(), request.ProductId);
                return StatusCode(500, ApiResponseDto<CartResponseDto>.ErrorResult("An error occurred while updating cart item"));
            }
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        [HttpDelete("items")]
        public async Task<ActionResult<ApiResponseDto>> RemoveFromCart(
            [FromBody] RemoveFromCartRequestDto request,
            [FromQuery] string? sessionId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cartService.RemoveFromCartAsync(request, userId, sessionId);
                
                if (result.IsSuccess)
                    return Ok(ApiResponseDto.SuccessResult(result.ErrorMessage));
                
                return BadRequest(ApiResponseDto.ErrorResult(result.ErrorMessage, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart for user {UserId}, product {ProductId}", 
                    GetCurrentUserId(), request.ProductId);
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while removing item from cart"));
            }
        }

        /// <summary>
        /// Clear all items from cart
        /// </summary>
        [HttpDelete("clear")]
        public async Task<ActionResult<ApiResponseDto>> ClearCart([FromQuery] string? sessionId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cartService.ClearCartAsync(userId, sessionId);
                
                if (result.IsSuccess)
                    return Ok(ApiResponseDto.SuccessResult(result.ErrorMessage));
                
                return BadRequest(ApiResponseDto.ErrorResult(result.ErrorMessage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while clearing the cart"));
            }
        }

        /// <summary>
        /// Bulk update cart items
        /// </summary>
        [HttpPut("bulk")]
        public async Task<ActionResult<ApiResponseDto<CartResponseDto>>> BulkUpdateCart(
            [FromBody] BulkUpdateCartRequestDto request,
            [FromQuery] string? sessionId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cartService.BulkUpdateCartAsync(request, userId, sessionId);
                
                if (result.IsSuccess)
                    return Ok(ApiResponseDto<CartResponseDto>.SuccessResult(result.Data, result.ErrorMessage));
                
                return BadRequest(ApiResponseDto<CartResponseDto>.ErrorResult(result.ErrorMessage, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating cart for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<CartResponseDto>.ErrorResult("An error occurred while updating the cart"));
            }
        }

        /// <summary>
        /// Validate cart items (check availability, prices, etc.)
        /// </summary>
        [HttpPost("validate")]
        public async Task<ActionResult<ApiResponseDto>> ValidateCart([FromQuery] string? sessionId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cartService.ValidateCartAsync(userId, sessionId);
                
                if (result.IsSuccess)
                    return Ok(ApiResponseDto.SuccessResult(result.ErrorMessage));
                
                return BadRequest(ApiResponseDto.ErrorResult(result.ErrorMessage, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating cart for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while validating the cart"));
            }
        }

        /// <summary>
        /// Merge guest cart with user cart after login
        /// </summary>
        [HttpPost("merge")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<CartResponseDto>>> MergeCarts([FromQuery] string sessionId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<CartResponseDto>.ErrorResult("User not authenticated"));

                var result = await _cartService.MergeCartsAsync(userId.Value, sessionId);
                
                if (result.IsSuccess)
                    return Ok(ApiResponseDto<CartResponseDto>.SuccessResult(result.Data, result.ErrorMessage));
                
                return BadRequest(ApiResponseDto<CartResponseDto>.ErrorResult(result.ErrorMessage, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error merging carts for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<CartResponseDto>.ErrorResult("An error occurred while merging carts"));
            }
        }

        /// <summary>
        /// Apply coupon to cart
        /// </summary>
        [HttpPost("coupon/{couponCode}")]
        public async Task<ActionResult<ApiResponseDto>> ApplyCoupon(
            string couponCode,
            [FromQuery] string? sessionId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cartService.ApplyCouponAsync(couponCode, userId, sessionId);
                
                if (result.IsSuccess)
                    return Ok(ApiResponseDto.SuccessResult(result.ErrorMessage));
                
                return BadRequest(ApiResponseDto.ErrorResult(result.ErrorMessage, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying coupon {CouponCode} for user {UserId}", couponCode, GetCurrentUserId());
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while applying the coupon"));
            }
        }

        /// <summary>
        /// Remove coupon from cart
        /// </summary>
        [HttpDelete("coupon")]
        public async Task<ActionResult<ApiResponseDto>> RemoveCoupon([FromQuery] string? sessionId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cartService.RemoveCouponAsync(userId, sessionId);
                
                if (result.IsSuccess)
                    return Ok(ApiResponseDto.SuccessResult(result.ErrorMessage));
                
                return BadRequest(ApiResponseDto.ErrorResult(result.ErrorMessage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing coupon for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while removing the coupon"));
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