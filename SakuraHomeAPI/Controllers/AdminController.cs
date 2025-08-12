using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakuraHomeAPI.Services.Interfaces;
using SakuraHomeAPI.DTOs.Admin.Requests;
using SakuraHomeAPI.DTOs.Admin.Responses;
using SakuraHomeAPI.Models.DTOs;
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
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

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
}
