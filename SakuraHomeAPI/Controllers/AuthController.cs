using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakuraHomeAPI.Models.DTOs;
using SakuraHomeAPI.Services.Interfaces;
using System.Security.Claims;

namespace SakuraHomeAPI.Controllers
{
    /// <summary>
    /// Authentication controller for user login, registration, and token management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// User login with email and password
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Authentication response with JWT token</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.ErrorResult<AuthResponseDto>("D? li?u không h?p l?", errors));
            }

            var ipAddress = GetIpAddress();
            var result = await _authService.LoginAsync(request, ipAddress);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// User registration
        /// </summary>
        /// <param name="request">Registration information</param>
        /// <returns>Authentication response with JWT token</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.ErrorResult<AuthResponseDto>("D? li?u không h?p l?", errors));
            }

            // Set AcceptTerms to true automatically for simplified registration
            request.AcceptTerms = true;

            var ipAddress = GetIpAddress();
            var result = await _authService.RegisterAsync(request, ipAddress);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// User logout
        /// </summary>
        /// <param name="request">Logout request (optional refresh token)</param>
        /// <returns>Success response</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public async Task<ActionResult<ApiResponse>> Logout([FromBody] RevokeTokenRequestDto? request = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse.ErrorResult("Không th? xác ??nh ng??i dùng"));
            }

            var result = await _authService.LogoutAsync(userId.Value, request?.RefreshToken);
            return Ok(result);
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New authentication response</returns>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.ErrorResult<AuthResponseDto>("D? li?u không h?p l?", errors));
            }

            var ipAddress = GetIpAddress();
            var result = await _authService.RefreshTokenAsync(request, ipAddress);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Send password reset email
        /// </summary>
        /// <param name="request">Forgot password request</param>
        /// <returns>Success response</returns>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.ErrorResult("D? li?u không h?p l?", errors));
            }

            var result = await _authService.ForgotPasswordAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Reset password using reset token
        /// </summary>
        /// <param name="request">Reset password request</param>
        /// <returns>Success response</returns>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.ErrorResult("D? li?u không h?p l?", errors));
            }

            var result = await _authService.ResetPasswordAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="request">Change password request</param>
        /// <returns>Success response</returns>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.ErrorResult("D? li?u không h?p l?", errors));
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse.ErrorResult("Không th? xác ??nh ng??i dùng"));
            }

            var result = await _authService.ChangePasswordAsync(userId.Value, request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Verify user email
        /// </summary>
        /// <param name="request">Email verification request</param>
        /// <returns>Success response</returns>
        [HttpPost("verify-email")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        public async Task<ActionResult<ApiResponse>> VerifyEmail([FromBody] VerifyEmailRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.ErrorResult("D? li?u không h?p l?", errors));
            }

            var result = await _authService.VerifyEmailAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Resend email verification
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>Success response</returns>
        [HttpPost("resend-email-verification")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        public async Task<ActionResult<ApiResponse>> ResendEmailVerification([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(ApiResponse.ErrorResult("Email là b?t bu?c"));
            }

            var result = await _authService.ResendEmailVerificationAsync(email);
            return Ok(result);
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        /// <returns>Current user data</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse.ErrorResult<UserDto>("Không th? xác ??nh ng??i dùng"));
            }

            var result = await _authService.GetCurrentUserAsync(userId.Value);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Revoke refresh token
        /// </summary>
        /// <param name="request">Revoke token request</param>
        /// <returns>Success response</returns>
        [HttpPost("revoke-token")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public async Task<ActionResult<ApiResponse>> RevokeToken([FromBody] RevokeTokenRequestDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse.ErrorResult("Không th? xác ??nh ng??i dùng"));
            }

            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(ApiResponse.ErrorResult("Refresh token là b?t bu?c"));
            }

            var result = await _authService.RevokeTokenAsync(userId.Value, request.RefreshToken);
            return Ok(result);
        }

        /// <summary>
        /// Revoke all user tokens (logout from all devices)
        /// </summary>
        /// <returns>Success response</returns>
        [HttpPost("revoke-all-tokens")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public async Task<ActionResult<ApiResponse>> RevokeAllTokens()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse.ErrorResult("Không th? xác ??nh ng??i dùng"));
            }

            var result = await _authService.RevokeAllTokensAsync(userId.Value);
            return Ok(result);
        }

        /// <summary>
        /// Check if current user is authenticated and token is valid
        /// </summary>
        /// <returns>Authentication status</returns>
        [HttpGet("check")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public ActionResult<ApiResponse> CheckAuthentication()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse.ErrorResult("Token không h?p l?"));
            }

            return Ok(ApiResponse.SuccessResult("Token h?p l?"));
        }

        #region Helper Methods

        /// <summary>
        /// Get current user ID from JWT claims
        /// </summary>
        /// <returns>User ID if authenticated, null otherwise</returns>
        private Guid? GetCurrentUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdString, out var userId))
            {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// Get client IP address
        /// </summary>
        /// <returns>IP address string</returns>
        private string GetIpAddress()
        {
            // Try to get IP from X-Forwarded-For header (for load balancers/proxies)
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // Try to get IP from X-Real-IP header
            var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fallback to connection remote IP
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        #endregion
    }
}