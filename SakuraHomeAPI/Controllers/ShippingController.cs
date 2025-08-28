using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.DTOs.Shipping.Requests;
using SakuraHomeAPI.Models.DTOs;
using SakuraHomeAPI.Services.Interfaces;
using System.Security.Claims;

namespace SakuraHomeAPI.Controllers
{
    /// <summary>
    /// Controller quản lý vận chuyển
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ShippingController : ControllerBase
    {
        private readonly IShippingService _shippingService;
        private readonly ILogger<ShippingController> _logger;

        public ShippingController(
            IShippingService shippingService,
            ILogger<ShippingController> logger)
        {
            _shippingService = shippingService;
            _logger = logger;
        }

        /// <summary>
        /// Tính phí vận chuyển - Public
        /// </summary>
        [HttpPost("calculate")]
        public async Task<ActionResult<ApiResponse<object>>> CalculateShippingFee([FromBody] CalculateShippingRequest request)
        {
            var result = await _shippingService.CalculateShippingFeeAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Lấy các khu vực vận chuyển - Public
        /// </summary>
        [HttpGet("zones")]
        public async Task<ActionResult<ApiResponse<object>>> GetShippingZones()
        {
            var result = await _shippingService.GetShippingZonesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin khu vực theo ID - Public
        /// </summary>
        [HttpGet("zones/{id}")]
        public async Task<ActionResult<ApiResponse<object>>> GetShippingZone(int id)
        {
            var result = await _shippingService.GetShippingZoneByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Tra cứu đơn vận chuyển - Public
        /// </summary>
        [HttpGet("track/{trackingNumber}")]
        public async Task<ActionResult<ApiResponse<object>>> TrackShipping(string trackingNumber)
        {
            var result = await _shippingService.TrackShippingAsync(trackingNumber);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Tạo đơn vận chuyển mới - Internal/Staff only
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> CreateShippingOrder([FromBody] CreateShippingOrderRequest request)
        {
            // Convert CreateShippingOrderRequest to CreateShippingRequest
            var createRequest = new CreateShippingRequest
            {
                OrderId = request.OrderId,
                ServiceType = request.ServiceType ?? "STANDARD",
                IsCOD = request.IsCOD,
                CODAmount = request.CODAmount,
                Notes = request.Notes ?? ""
            };

            var result = await _shippingService.CreateShippingOrderAsync(createRequest);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetShippingOrder), new { id = result.Data?.Id ?? 0 }, result);
        }

        /// <summary>
        /// Lấy chi tiết đơn vận chuyển - Staff only
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> GetShippingOrder(int id)
        {
            var result = await _shippingService.GetShippingOrderByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Cập nhật trạng thái đơn vận chuyển - Staff only
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateShippingStatus(
            int id,
            [FromBody] UpdateShippingStatusRequest request)
        {
            var result = await _shippingService.UpdateShippingStatusAsync(id, request.Status, request.StatusDescription);
            return Ok(result);
        }

        /// <summary>
        /// Hủy đơn vận chuyển - Staff only
        /// </summary>
        [HttpPatch("{id}/cancel")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> CancelShippingOrder(
            int id,
            [FromBody] CancelShippingOrderRequest request)
        {
            // Convert to CancelShippingRequest
            var cancelRequest = new CancelShippingRequest
            {
                CancelReason = request.CancelReason ?? "",
                Notes = request.Notes ?? ""
            };

            var result = await _shippingService.CancelShippingOrderAsync(id, cancelRequest);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách đơn vận chuyển - Staff only
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> GetShippingOrders([FromQuery] ShippingOrderFilterRequest filter)
        {
            // Convert to ShippingFilterRequest
            var shippingFilter = new ShippingFilterRequest
            {
                Status = filter.Status ?? "",
                ServiceType = filter.ServiceType ?? "",
                DateFrom = filter.DateFrom,
                DateTo = filter.DateTo,
                Search = filter.Search ?? "",
                Page = filter.Page,
                PageSize = filter.PageSize,
                SortBy = filter.SortBy,
                SortOrder = filter.SortOrder
            };

            var result = await _shippingService.GetShippingOrdersAsync(shippingFilter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy lịch sử theo dõi - Staff only
        /// </summary>
        [HttpGet("{id}/tracking-history")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> GetTrackingHistory(int id)
        {
            // Get shipping first to get tracking number
            var shipping = await _shippingService.GetShippingOrderByIdAsync(id);
            if (!shipping.IsSuccess)
            {
                return NotFound(shipping);
            }

            var result = await _shippingService.GetTrackingHistoryAsync(shipping.Data?.TrackingNumber ?? "");
            return Ok(result);
        }

        // ===== ADMIN ENDPOINTS =====

        /// <summary>
        /// Tạo khu vực vận chuyển mới - Admin only
        /// </summary>
        [HttpPost("zones")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<object>>> CreateShippingZone([FromBody] CreateShippingZoneRequest request)
        {
            var result = await _shippingService.CreateShippingZoneAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetShippingZone), new { id = result.Data.Id }, result);
        }

        /// <summary>
        /// Cập nhật khu vực vận chuyển - Admin only
        /// </summary>
        [HttpPut("zones/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateShippingZone(
            int id,
            [FromBody] UpdateShippingZoneRequest request)
        {
            var result = await _shippingService.UpdateShippingZoneAsync(id, request);
            return Ok(result);
        }

        /// <summary>
        /// Xóa khu vực vận chuyển - Admin only
        /// </summary>
        [HttpDelete("zones/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteShippingZone(int id)
        {
            var result = await _shippingService.DeleteShippingZoneAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Lấy biểu phí vận chuyển - Admin/Staff only
        /// </summary>
        [HttpGet("rates")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> GetShippingRates()
        {
            var result = await _shippingService.GetShippingRatesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật biểu phí vận chuyển - Admin only
        /// </summary>
        [HttpPut("rates")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateShippingRates([FromBody] UpdateShippingRatesRequest request)
        {
            var result = await _shippingService.UpdateShippingRatesAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thống kê vận chuyển - Admin only
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<object>>> GetShippingStats([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var result = await _shippingService.GetShippingStatsAsync(fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Xuất báo cáo vận chuyển - Admin only
        /// </summary>
        [HttpPost("export")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<object>>> ExportShippingReport([FromBody] ShippingReportRequest request)
        {
            var result = await _shippingService.ExportShippingReportAsync(request);
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