using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakuraHomeAPI.Services.Interfaces;
using SakuraHomeAPI.DTOs.Admin.Requests;
using SakuraHomeAPI.DTOs.Admin.Responses;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.DTOs.Orders.Responses;
using SakuraHomeAPI.DTOs.Products.Responses;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace SakuraHomeAPI.Controllers
{
    /// <summary>
    /// Admin controller for user management and system administration
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
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
                // Get user stats
                var userStatsResult = await _adminService.GetUserStatisticsAsync();
                
                // Get order stats 
                var orderStatsResult = await _orderService.GetOrderStatsAsync(null, null, null);
                
                // Get recent orders
                var recentOrdersResult = await _orderService.GetRecentOrdersAsync(10);

                // Get product stats (we'll create a basic one)
                var totalProductsResult = await _productService.GetListAsync(new DTOs.Products.Requests.ProductFilterRequestDto 
                { 
                    Page = 1, 
                    PageSize = 1 
                }, CancellationToken.None);

                var overview = new DashboardOverviewDto
                {
                    UserStats = userStatsResult.Success ? userStatsResult.Data : null,
                    OrderStats = orderStatsResult.Success ? orderStatsResult.Data : null,
                    RecentOrders = recentOrdersResult.Success ? recentOrdersResult.Data : new List<OrderSummaryDto>(),
                    TotalProducts = totalProductsResult.IsSuccess ? totalProductsResult.Data?.Pagination?.TotalItems ?? 0 : 0,
                    LowStockProducts = 0, // TODO: Implement low stock count
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
        /// Get order statistics for admin dashboard
        /// </summary>
        [HttpGet("orders/stats")]
        public async Task<ActionResult<ApiResponse<OrderStatsDto>>> GetOrderStats(
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var result = await _orderService.GetOrderStatsAsync(null, fromDate, toDate);

                if (result.Success)
                    return Ok(ApiResponse.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponse.ErrorResult<OrderStatsDto>(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin order statistics");
                return StatusCode(500, ApiResponse.ErrorResult<OrderStatsDto>("An error occurred while retrieving order statistics"));
            }
        }

        /// <summary>
        /// Get revenue analytics with time period breakdown
        /// </summary>
        [HttpGet("analytics/revenue")]
        public async Task<ActionResult<ApiResponse<RevenueAnalyticsDto>>> GetRevenueAnalytics(
            [FromQuery] string period = "month")
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
                    // TODO: Add daily/weekly/monthly breakdowns when available
                    DailyBreakdown = new List<DailyRevenueDto>()
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
                // Get total products count
                var allProductsResult = await _productService.GetListAsync(new DTOs.Products.Requests.ProductFilterRequestDto 
                { 
                    Page = 1, 
                    PageSize = 1 
                }, CancellationToken.None);

                // Get active products count
                var activeProductsResult = await _productService.GetListAsync(new DTOs.Products.Requests.ProductFilterRequestDto 
                { 
                    Page = 1, 
                    PageSize = 1,
                    Status = Models.Enums.ProductStatus.Active
                }, CancellationToken.None);

                // Get out of stock products
                var outOfStockResult = await _productService.GetOutOfStockProductsAsync(CancellationToken.None);

                // Get low stock products
                var lowStockResult = await _productService.GetLowStockProductsAsync(CancellationToken.None);

                var productStats = new ProductStatsDto
                {
                    TotalProducts = allProductsResult.IsSuccess ? allProductsResult.Data?.Pagination?.TotalItems ?? 0 : 0,
                    ActiveProducts = activeProductsResult.IsSuccess ? activeProductsResult.Data?.Pagination?.TotalItems ?? 0 : 0,
                    OutOfStockProducts = outOfStockResult.IsSuccess ? outOfStockResult.Data?.Count() ?? 0 : 0,
                    LowStockProducts = lowStockResult.IsSuccess ? lowStockResult.Data?.Count() ?? 0 : 0,
                    InactiveProducts = 0 // TODO: Calculate inactive products
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
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductSummaryDto>>>> GetTopSellingProducts(
            [FromQuery] int limit = 5)
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

    /// <summary>
    /// Reset password request model
    /// </summary>
    public class ResetPasswordRequest
    {
        [Required, MinLength(8), MaxLength(100)]
        public string NewPassword { get; set; } = string.Empty;
    }

    #region Dashboard DTOs

    /// <summary>
    /// Complete dashboard overview data
    /// </summary>
    public class DashboardOverviewDto
    {
        public AdminUserStatisticsDto? UserStats { get; set; }
        public OrderStatsDto? OrderStats { get; set; }
        public List<OrderSummaryDto> RecentOrders { get; set; } = new();
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int PendingOrders { get; set; }
    }

    /// <summary>
    /// Revenue analytics data
    /// </summary>
    public class RevenueAnalyticsDto
    {
        public string Period { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailyRevenueDto> DailyBreakdown { get; set; } = new();
    }

    /// <summary>
    /// Daily revenue breakdown
    /// </summary>
    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    /// <summary>
    /// Product statistics data
    /// </summary>
    public class ProductStatsDto
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int LowStockProducts { get; set; }
    }

    #endregion
}
