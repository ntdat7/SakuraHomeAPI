using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.DTOs.Coupons.Requests;
using SakuraHomeAPI.Services.Interfaces;
using System.Security.Claims;

namespace SakuraHomeAPI.Controllers
{
    /// <summary>
    /// Controller quản lý mã giảm giá (Coupon)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CouponController : ControllerBase
    {
        private readonly ICouponService _couponService;
        private readonly ILogger<CouponController> _logger;

        public CouponController(
            ICouponService couponService,
            ILogger<CouponController> logger)
        {
            _couponService = couponService;
            _logger = logger;
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của mã coupon - Public
        /// </summary>
        [HttpPost("validate")]
        public async Task<ActionResult<ApiResponse<object>>> ValidateCoupon([FromBody] ValidateCouponRequest request)
        {
            var result = await _couponService.ValidateCouponAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin mã coupon theo code - Public (chỉ thông tin cơ bản)
        /// </summary>
        [HttpGet("code/{code}")]
        public async Task<ActionResult<ApiResponse<object>>> GetCouponByCode(string code)
        {
            var result = await _couponService.GetCouponByCodeAsync(code);
            
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            // Chỉ trả về thông tin công khai
            var publicInfo = new
            {
                result.Data?.Code,
                result.Data?.Name,
                result.Data?.Description,
                result.Data?.Type,
                result.Data?.TypeDisplay,
                result.Data?.ValueDisplay,
                result.Data?.MinOrderAmount,
                result.Data?.IsValid,
                result.Data?.StatusDisplay
            };

            return Ok(ApiResponse.SuccessResult(publicInfo, "Lấy thông tin mã coupon thành công"));
        }

        // ===== ADMIN ENDPOINTS =====

        /// <summary>
        /// Lấy danh sách mã coupon - Chỉ Admin/Staff
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> GetCoupons([FromQuery] CouponFilterRequest filter)
        {
            var result = await _couponService.GetCouponsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết mã coupon - Chỉ Admin/Staff
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> GetCoupon(int id)
        {
            var result = await _couponService.GetCouponByIdAsync(id);
            
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Tạo mã coupon mới - Chỉ Admin/Staff
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> CreateCoupon([FromBody] CreateCouponRequest request)
        {
            var result = await _couponService.CreateCouponAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetCoupon), new { id = result.Data?.Id ?? 0 }, result);
        }

        /// <summary>
        /// Cập nhật mã coupon - Chỉ Admin/Staff
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateCoupon(
            int id,
            [FromBody] UpdateCouponRequest request)
        {
            var result = await _couponService.UpdateCouponAsync(id, request);
            return Ok(result);
        }

        /// <summary>
        /// Xóa mã coupon - Chỉ Admin/Staff
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteCoupon(int id)
        {
            var result = await _couponService.DeleteCouponAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Bật/Tắt trạng thái mã coupon - Chỉ Admin/Staff
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> ToggleCouponStatus(
            int id,
            [FromBody] bool isActive)
        {
            var result = await _couponService.ToggleCouponStatusAsync(id, isActive);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thống kê mã coupon - Chỉ Admin/Staff
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> GetCouponStats()
        {
            var result = await _couponService.GetCouponStatsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy mã coupon đã hết hạn - Chỉ Admin/Staff
        /// </summary>
        [HttpGet("expired")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> GetExpiredCoupons()
        {
            var result = await _couponService.GetExpiredCouponsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy mã coupon sắp hết hạn - Chỉ Admin/Staff
        /// </summary>
        [HttpGet("expiring")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> GetExpiringCoupons([FromQuery] int days = 7)
        {
            var result = await _couponService.GetExpiringCouponsAsync(days);
            return Ok(result);
        }

        /// <summary>
        /// Tính toán giảm giá cho đơn hàng - Internal/Public API
        /// </summary>
        [HttpPost("calculate-discount")]
        public async Task<ActionResult<ApiResponse<object>>> CalculateDiscount([FromBody] ValidateCouponRequest request)
        {
            var discountAmount = await _couponService.CalculateDiscountAsync(request.Code, request.OrderAmount);

            var result = new
            {
                CouponCode = request.Code,
                OrderAmount = request.OrderAmount,
                DiscountAmount = discountAmount,
                FinalAmount = request.OrderAmount - discountAmount
            };

            return Ok(ApiResponse.SuccessResult(result, "Tính toán giảm giá thành công"));
        }

        /// <summary>
        /// Sử dụng mã coupon trong đơn hàng - Internal API (sẽ được gọi từ OrderController)
        /// </summary>
        [HttpPost("use")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> UseCoupon([FromBody] ValidateCouponRequest request)
        {
            var userId = GetCurrentUserId();
            var result = await _couponService.UseCouponAsync(request.Code, userId, request.OrderAmount);
            return Ok(result);
        }

        /// <summary>
        /// Hoàn lại lượt sử dụng mã coupon - Internal API (khi hủy đơn hàng)
        /// </summary>
        [HttpPost("revert/{code}")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> RevertCouponUsage(string code)
        {
            var result = await _couponService.RevertCouponUsageAsync(code);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách mã giảm giá đang hoạt động - Public
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<object>>> GetActiveCoupons([FromQuery] CouponFilterRequest filter)
        {
            var result = await _couponService.GetActiveCouponsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy lịch sử sử dụng mã giảm giá của người dùng - Internal API (sẽ được gọi từ OrderController hoặc ProfileController)
        /// </summary>
        [HttpGet("my-history")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> GetMyCouponUsageHistory()
        {
            var userId = GetCurrentUserId();
            var result = await _couponService.GetCouponUsageHistoryAsync(userId);
            return Ok(result);
        }

        // ===== HELPER METHODS =====

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }
    }
}