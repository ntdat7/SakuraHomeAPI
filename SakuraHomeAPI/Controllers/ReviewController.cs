using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.DTOs.Reviews.Requests;
using SakuraHomeAPI.Models.DTOs;
using SakuraHomeAPI.Services.Interfaces;
using System.Security.Claims;

namespace SakuraHomeAPI.Controllers
{
    /// <summary>
    /// Controller quản lý đánh giá sản phẩm
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(
            IReviewService reviewService,
            ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách đánh giá sản phẩm - Public
        /// </summary>
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<ApiResponse<object>>> GetProductReviews(
            int productId,
            [FromQuery] ProductReviewFilterRequest filter)
        {
            filter.ProductId = productId;
            var result = await _reviewService.GetProductReviewsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thống kê đánh giá sản phẩm - Public
        /// </summary>
        [HttpGet("product/{productId}/summary")]
        public async Task<ActionResult<ApiResponse<object>>> GetProductReviewSummary(int productId)
        {
            var result = await _reviewService.GetProductReviewSummaryAsync(productId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết đánh giá - Public
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> GetReview(int id)
        {
            var result = await _reviewService.GetReviewByIdAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Tạo đánh giá mới - Customer only
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<ActionResult<ApiResponse<object>>> CreateReview([FromBody] CreateReviewRequest request)
        {
            var userId = GetCurrentUserId();
            request.UserId = userId;
            
            var result = await _reviewService.CreateReviewAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetReview), new { id = result.Data?.Id ?? 0 }, result);
        }

        /// <summary>
        /// Cập nhật đánh giá - Customer chỉ có thể sửa đánh giá của mình
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateReview(
            int id,
            [FromBody] UpdateReviewRequest request)
        {
            var userId = GetCurrentUserId();
            var result = await _reviewService.UpdateReviewAsync(id, request, userId);
            return Ok(result);
        }

        /// <summary>
        /// Xóa đánh giá - Customer chỉ có thể xóa đánh giá của mình
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteReview(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _reviewService.DeleteReviewAsync(id, userId);
            return Ok(result);
        }

        /// <summary>
        /// Thêm hình ảnh vào đánh giá - Customer only
        /// </summary>
        [HttpPost("{id}/images")]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<ActionResult<ApiResponse<object>>> AddReviewImages(
            int id,
            [FromBody] AddReviewImagesRequest request)
        {
            var userId = GetCurrentUserId();
            var result = await _reviewService.AddReviewImagesAsync(id, request, userId);
            return Ok(result);
        }

        /// <summary>
        /// Xóa hình ảnh khỏi đánh giá - Customer only
        /// </summary>
        [HttpDelete("{reviewId}/images/{imageId}")]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<ActionResult<ApiResponse<object>>> RemoveReviewImage(int reviewId, int imageId)
        {
            var userId = GetCurrentUserId();
            var result = await _reviewService.RemoveReviewImageAsync(reviewId, imageId, userId);
            return Ok(result);
        }

        /// <summary>
        /// Vote cho đánh giá (helpful/not helpful) - Customer only
        /// </summary>
        [HttpPost("{id}/vote")]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<ActionResult<ApiResponse<object>>> VoteReview(
            int id,
            [FromBody] VoteReviewRequest request)
        {
            var userId = GetCurrentUserId();
            var result = await _reviewService.VoteReviewAsync(id, request, userId);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo đánh giá vi phạm - Customer only
        /// </summary>
        [HttpPost("{id}/report")]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<ActionResult<ApiResponse<object>>> ReportReview(
            int id,
            [FromBody] ReportReviewRequest request)
        {
            var userId = GetCurrentUserId();
            var result = await _reviewService.ReportReviewAsync(id, request, userId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy đánh giá của user hiện tại - Customer only
        /// </summary>
        [HttpGet("my-reviews")]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<ActionResult<ApiResponse<object>>> GetMyReviews([FromQuery] MyReviewFilterRequest filter)
        {
            var userId = GetCurrentUserId();
            filter.UserId = userId;
            var result = await _reviewService.GetUserReviewsAsync(filter);
            return Ok(result);
        }

        // ===== ADMIN/STAFF ENDPOINTS =====

        /// <summary>
        /// Lấy tất cả đánh giá (với filter) - Staff only
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> GetAllReviews([FromQuery] AdminReviewFilterRequest filter)
        {
            var result = await _reviewService.GetAllReviewsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Phê duyệt đánh giá - Staff only
        /// </summary>
        [HttpPatch("{id}/approve")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> ApproveReview(int id)
        {
            var staffId = GetCurrentUserId();
            var result = await _reviewService.ApproveReviewAsync(id, staffId);
            return Ok(result);
        }

        /// <summary>
        /// Từ chối đánh giá - Staff only
        /// </summary>
        [HttpPatch("{id}/reject")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> RejectReview(
            int id,
            [FromBody] RejectReviewRequest request)
        {
            var staffId = GetCurrentUserId();
            var result = await _reviewService.RejectReviewAsync(id, request, staffId);
            return Ok(result);
        }

        /// <summary>
        /// Trả lời đánh giá - Staff only
        /// </summary>
        [HttpPost("{id}/respond")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<object>>> RespondToReview(
            int id,
            [FromBody] RespondToReviewRequest request)
        {
            var staffId = GetCurrentUserId();
            var result = await _reviewService.RespondToReviewAsync(id, request, staffId);
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