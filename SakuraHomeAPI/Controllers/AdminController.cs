using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakuraHomeAPI.Services.Interfaces;
using SakuraHomeAPI.DTOs.Admin.Requests;
using SakuraHomeAPI.DTOs.Admin.Responses;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.DTOs.Products.Responses;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace SakuraHomeAPI.Controllers
{
    /// <summary>
    /// Admin controller for user management and system administration
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    [Authorize(Policy = "StaffOnly")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IAdminService adminService,
            IOrderService orderService,
            IProductService productService,
            ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _orderService = orderService;
            _productService = productService;
            _logger = logger;
        }

        #region Dashboard & Analytics

        /// <summary>
        /// Get complete dashboard overview with all key metrics
        /// </summary>
        [HttpGet("dashboard/overview")]
        public async Task<ActionResult<ApiResponse<DashboardOverviewDto>>> GetDashboardOverview()
        {
            try
            {
                var userStatsResult = await _adminService.GetUserStatisticsAsync();
                var orderStatsResult = await _orderService.GetOrderStatsAsync(null, null, null);
                var recentOrdersResult = await _orderService.GetRecentOrdersAsync(10);
                var totalProductsResult = await _productService.GetListAsync(
                    new DTOs.Products.Requests.ProductFilterRequestDto { Page = 1, PageSize = 1 },
                    CancellationToken.None);

                var overview = new DashboardOverviewDto
                {
                    UserStats = userStatsResult.Success ? userStatsResult.Data : null,
                    OrderStats = orderStatsResult.Success ? orderStatsResult.Data : null,
                    RecentOrders = recentOrdersResult.Success ? recentOrdersResult.Data : new(),
                    TotalProducts = totalProductsResult.IsSuccess ? totalProductsResult.Data?.Pagination?.TotalItems ?? 0 : 0,
                    LowStockProducts = 0,
                    PendingOrders = orderStatsResult.Success ? orderStatsResult.Data?.PendingOrders ?? 0 : 0
                };

                return Ok(ApiResponse.SuccessResult(overview, "Dashboard overview retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard overview");
                return StatusCode(500, ApiResponse.ErrorResult<DashboardOverviewDto>("An error occurred while retrieving dashboard overview"));
            }
        }

        /// <summary>
        /// Get revenue analytics with time period breakdown
        /// </summary>
        [HttpGet("analytics/revenue")]
        public async Task<ActionResult<ApiResponse<RevenueAnalyticsDto>>> GetRevenueAnalytics([FromQuery] string period = "month")
        {
            try
            {
                DateTime fromDate, toDate = DateTime.UtcNow;

                switch (period.ToLower())
                {
                    case "week":
                        fromDate = toDate.AddDays(-7);
                        break;
                    case "month":
                        fromDate = toDate.AddMonths(-1);
                        break;
                    case "year":
                        fromDate = toDate.AddYears(-1);
                        break;
                    default:
                        fromDate = toDate.AddMonths(-1);
                        break;
                }

                var orderStatsResult = await _orderService.GetOrderStatsAsync(null, fromDate, toDate);

                if (!orderStatsResult.Success)
                {
                    return BadRequest(ApiResponse.ErrorResult<RevenueAnalyticsDto>(orderStatsResult.Message));
                }

                var revenueAnalytics = new RevenueAnalyticsDto
                {
                    Period = period,
                    FromDate = fromDate,
                    ToDate = toDate,
                    TotalRevenue = orderStatsResult.Data?.TotalRevenue ?? 0,
                    TotalOrders = orderStatsResult.Data?.TotalOrders ?? 0,
                    AverageOrderValue = orderStatsResult.Data?.AverageOrderValue ?? 0,
                    DailyBreakdown = new()
                };

                return Ok(ApiResponse.SuccessResult(revenueAnalytics, "Revenue analytics retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue analytics for period {Period}", period);
                return StatusCode(500, ApiResponse.ErrorResult<RevenueAnalyticsDto>("An error occurred while retrieving revenue analytics"));
            }
        }

        /// <summary>
        /// Get product statistics for admin dashboard
        /// </summary>
        [HttpGet("products/stats")]
        public async Task<ActionResult<ApiResponse<ProductStatsDto>>> GetProductStats()
        {
            try
            {
                var allProductsResult = await _productService.GetListAsync(
                    new DTOs.Products.Requests.ProductFilterRequestDto { Page = 1, PageSize = 1 },
                    CancellationToken.None);

                var activeProductsResult = await _productService.GetListAsync(
                    new DTOs.Products.Requests.ProductFilterRequestDto
                    {
                        Page = 1,
                        PageSize = 1,
                        Status = Models.Enums.ProductStatus.Active
                    },
                    CancellationToken.None);

                var outOfStockResult = await _productService.GetOutOfStockProductsAsync(CancellationToken.None);
                var lowStockResult = await _productService.GetLowStockProductsAsync(CancellationToken.None);

                var productStats = new ProductStatsDto
                {
                    TotalProducts = allProductsResult.IsSuccess ? allProductsResult.Data?.Pagination?.TotalItems ?? 0 : 0,
                    ActiveProducts = activeProductsResult.IsSuccess ? activeProductsResult.Data?.Pagination?.TotalItems ?? 0 : 0,
                    OutOfStockProducts = outOfStockResult.IsSuccess ? outOfStockResult.Data?.Count() ?? 0 : 0,
                    LowStockProducts = lowStockResult.IsSuccess ? lowStockResult.Data?.Count() ?? 0 : 0,
                    InactiveProducts = 0
                };

                return Ok(ApiResponse.SuccessResult(productStats, "Product statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product statistics");
                return StatusCode(500, ApiResponse.ErrorResult<ProductStatsDto>("An error occurred while retrieving product statistics"));
            }
        }

        /// <summary>
        /// Get top selling products
        /// </summary>
        [HttpGet("products/top-selling")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductSummaryDto>>>> GetTopSellingProducts([FromQuery] int limit = 5)
        {
            try
            {
                var result = await _productService.GetBestSellersAsync(limit, CancellationToken.None);

                if (result.IsSuccess)
                    return Ok(ApiResponse.SuccessResult(result.Data, "Top selling products retrieved successfully"));

                return BadRequest(ApiResponse.ErrorResult<IEnumerable<ProductSummaryDto>>(result.ErrorMessage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top selling products");
                return StatusCode(500, ApiResponse.ErrorResult<IEnumerable<ProductSummaryDto>>("An error occurred while retrieving top selling products"));
            }
        }

        #endregion

        #region User Management

        /// <summary>
        /// Get list of users with filtering and pagination
        /// </summary>
        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<AdminUserListResponseDto>>> GetUsers([FromQuery] AdminUserFilterRequestDto filter)
        {
            var result = await _adminService.GetUserListAsync(filter);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Get user details by ID
        /// </summary>
        [HttpGet("users/{userId:guid}")]
        public async Task<ActionResult<ApiResponse<AdminUserDetailDto>>> GetUser(Guid userId)
        {
            var result = await _adminService.GetUserByIdAsync(userId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost("users")]
        public async Task<ActionResult<ApiResponse<AdminUserDetailDto>>> CreateUser([FromBody] AdminCreateUserRequestDto request)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _adminService.CreateUserAsync(request, currentUserId);
            if (result.Success)
                return CreatedAtAction(nameof(GetUser), new { userId = result.Data!.Id }, result);
            return BadRequest(result);
        }

        /// <summary>
        /// Update user information
        /// </summary>
        [HttpPut("users/{userId:guid}")]
        public async Task<ActionResult<ApiResponse<AdminUserDetailDto>>> UpdateUser(Guid userId, [FromBody] AdminUpdateUserRequestDto request)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _adminService.UpdateUserAsync(userId, request, currentUserId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Delete user (soft delete)
        /// </summary>
        [HttpDelete("users/{userId:guid}")]
        public async Task<ActionResult<ApiResponse>> DeleteUser(Guid userId)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _adminService.DeleteUserAsync(userId, currentUserId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Change user status
        /// </summary>
        [HttpPatch("users/{userId:guid}/status")]
        public async Task<ActionResult<ApiResponse>> ChangeUserStatus(Guid userId, [FromBody] AdminChangeUserStatusRequestDto request)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _adminService.ChangeUserStatusAsync(userId, request, currentUserId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Get paginated list of users with advanced filters
        /// </summary>
        [HttpGet("users/list")]
        public async Task<ActionResult> GetUsersList([FromQuery] UserFilterRequest filter)
        {
            try
            {
                var result = await _adminService.GetUsersAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users list");
                return StatusCode(500, new { success = false, message = "Error occurred" });
            }
        }

        /// <summary>
        /// Get detailed user information
        /// </summary>
        [HttpGet("users/{userId:guid}/detail")]
        public async Task<ActionResult> GetUserDetail(Guid userId)
        {
            try
            {
                var result = await _adminService.GetUserDetailAsync(userId);
                if (result.Success)
                    return Ok(result);
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user detail");
                return StatusCode(500, new { success = false, message = "Error occurred" });
            }
        }

        /// <summary>
        /// Create new user
        /// </summary>
        [HttpPost("users/create")]
        public async Task<ActionResult> CreateNewUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                    return Unauthorized(new { success = false, message = "Not authenticated" });

                var result = await _adminService.CreateUserAsync(request, currentUserId.Value);
                if (result.Success)
                    return CreatedAtAction(nameof(GetUserDetail), new { userId = result.Data!.Id }, result);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { success = false, message = "Error occurred" });
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        [HttpPut("users/{userId:guid}/update")]
        public async Task<ActionResult> UpdateExistingUser(Guid userId, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                    return Unauthorized();

                var result = await _adminService.UpdateUserAsync(userId, request, currentUserId.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return StatusCode(500, new { success = false, message = "Error occurred" });
            }
        }

        /// <summary>
        /// Delete user
        /// </summary>
        [HttpDelete("users/{userId:guid}/delete")]
        public async Task<ActionResult> DeleteExistingUser(Guid userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                    return Unauthorized();

                var result = await _adminService.DeleteUserAsync(userId, currentUserId.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return StatusCode(500, new { success = false, message = "Error occurred" });
            }
        }

        /// <summary>
        /// Toggle user status
        /// </summary>
        [HttpPatch("users/{userId:guid}/toggle")]
        public async Task<ActionResult> ToggleUserStatus(Guid userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                    return Unauthorized();

                var result = await _adminService.ToggleUserStatusAsync(userId, currentUserId.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status");
                return StatusCode(500, new { success = false, message = "Error occurred" });
            }
        }

        /// <summary>
        /// Check email availability
        /// </summary>
        [HttpGet("users/check-email")]
        public async Task<ActionResult> CheckEmail([FromQuery, Required, EmailAddress] string email)
        {
            try
            {
                var result = await _adminService.CheckEmailAsync(email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email");
                return StatusCode(500, new { success = false, message = "Error occurred" });
            }
        }

        /// <summary>
        /// Check CCCD availability
        /// </summary>
        [HttpGet("users/check-cccd")]
        public async Task<ActionResult> CheckNationalId([FromQuery, Required] string nationalIdCard)
        {
            try
            {
                var result = await _adminService.CheckNationalIdAsync(nationalIdCard);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking CCCD");
                return StatusCode(500, new { success = false, message = "Error occurred" });
            }
        }

        #endregion

        #region User Statistics

        /// <summary>
        /// Get user statistics for admin dashboard
        /// </summary>
        [HttpGet("users/stats")]
        public async Task<ActionResult<ApiResponse<AdminUserStatisticsDto>>> GetUserStats()
        {
            var result = await _adminService.GetUserStatisticsAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        #endregion

        #region User Actions

        /// <summary>
        /// Reset user password
        /// </summary>
        [HttpPost("users/{userId:guid}/reset-password")]
        public async Task<ActionResult<ApiResponse>> ResetUserPassword(Guid userId, [FromBody] ResetPasswordRequest request)
        {
            var result = await _adminService.ResetUserPasswordAsync(userId, request.NewPassword);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Unlock user account
        /// </summary>
        [HttpPost("users/{userId:guid}/unlock")]
        public async Task<ActionResult<ApiResponse>> UnlockUser(Guid userId)
        {
            var result = await _adminService.UnlockUserAccountAsync(userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Verify user email
        /// </summary>
        [HttpPost("users/{userId:guid}/verify-email")]
        public async Task<ActionResult<ApiResponse>> VerifyUserEmail(Guid userId)
        {
            var result = await _adminService.VerifyUserEmailAsync(userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Verify user phone
        /// </summary>
        [HttpPost("users/{userId:guid}/verify-phone")]
        public async Task<ActionResult<ApiResponse>> VerifyUserPhone(Guid userId)
        {
            var result = await _adminService.VerifyUserPhoneAsync(userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        #endregion

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