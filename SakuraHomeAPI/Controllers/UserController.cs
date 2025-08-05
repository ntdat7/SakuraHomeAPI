using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakuraHomeAPI.Services.Interfaces;
using SakuraHomeAPI.DTOs.Users.Requests;
using SakuraHomeAPI.DTOs.Users.Responses;
using SakuraHomeAPI.DTOs.Common;
using System.Security.Claims;

namespace SakuraHomeAPI.Controllers
{
    /// <summary>
    /// User profile and address management controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponseDto<UserProfileDto>>> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<UserProfileDto>.ErrorResult("User not authenticated"));

                var result = await _userService.GetProfileAsync(userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<UserProfileDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<UserProfileDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<UserProfileDto>.ErrorResult("An error occurred while retrieving profile"));
            }
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponseDto<UserProfileDto>>> UpdateProfile([FromBody] UpdateProfileRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<UserProfileDto>.ErrorResult("User not authenticated"));

                var result = await _userService.UpdateProfileAsync(userId.Value, request);
                
                if (result.Success)
                    return Ok(ApiResponseDto<UserProfileDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<UserProfileDto>.ErrorResult(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<UserProfileDto>.ErrorResult("An error occurred while updating profile"));
            }
        }

        /// <summary>
        /// Delete user profile (deactivate account)
        /// </summary>
        [HttpDelete("profile")]
        public async Task<ActionResult<ApiResponseDto>> DeleteProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto.ErrorResult("User not authenticated"));

                var result = await _userService.DeleteProfileAsync(userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto.SuccessResult(result.Message));
                
                return BadRequest(ApiResponseDto.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while deleting profile"));
            }
        }

        /// <summary>
        /// Get user statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponseDto<UserStatsDto>>> GetUserStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<UserStatsDto>.ErrorResult("User not authenticated"));

                var result = await _userService.GetUserStatsAsync(userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<UserStatsDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<UserStatsDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<UserStatsDto>.ErrorResult("An error occurred while retrieving user statistics"));
            }
        }

        #region Address Management

        /// <summary>
        /// Get user addresses
        /// </summary>
        [HttpGet("addresses")]
        public async Task<ActionResult<ApiResponseDto<List<AddressResponseDto>>>> GetAddresses()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<List<AddressResponseDto>>.ErrorResult("User not authenticated"));

                var result = await _userService.GetAddressesAsync(userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<List<AddressResponseDto>>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<List<AddressResponseDto>>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<List<AddressResponseDto>>.ErrorResult("An error occurred while retrieving addresses"));
            }
        }

        /// <summary>
        /// Create new address
        /// </summary>
        [HttpPost("addresses")]
        public async Task<ActionResult<ApiResponseDto<AddressResponseDto>>> CreateAddress([FromBody] CreateAddressRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<AddressResponseDto>.ErrorResult("User not authenticated"));

                var result = await _userService.CreateAddressAsync(request, userId.Value);
                
                if (result.Success)
                    return CreatedAtAction(nameof(GetAddresses), null, 
                        ApiResponseDto<AddressResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<AddressResponseDto>.ErrorResult(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<AddressResponseDto>.ErrorResult("An error occurred while creating address"));
            }
        }

        /// <summary>
        /// Update address
        /// </summary>
        [HttpPut("addresses/{addressId}")]
        public async Task<ActionResult<ApiResponseDto<AddressResponseDto>>> UpdateAddress(int addressId, [FromBody] UpdateAddressRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<AddressResponseDto>.ErrorResult("User not authenticated"));

                var result = await _userService.UpdateAddressAsync(addressId, request, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<AddressResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<AddressResponseDto>.ErrorResult(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId} for user {UserId}", addressId, GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<AddressResponseDto>.ErrorResult("An error occurred while updating address"));
            }
        }

        /// <summary>
        /// Delete address
        /// </summary>
        [HttpDelete("addresses/{addressId}")]
        public async Task<ActionResult<ApiResponseDto>> DeleteAddress(int addressId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto.ErrorResult("User not authenticated"));

                var result = await _userService.DeleteAddressAsync(addressId, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto.SuccessResult(result.Message));
                
                return BadRequest(ApiResponseDto.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId} for user {UserId}", addressId, GetCurrentUserId());
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while deleting address"));
            }
        }

        /// <summary>
        /// Set default address
        /// </summary>
        [HttpPatch("addresses/{addressId}/set-default")]
        public async Task<ActionResult<ApiResponseDto>> SetDefaultAddress(int addressId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto.ErrorResult("User not authenticated"));

                var result = await _userService.SetDefaultAddressAsync(addressId, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto.SuccessResult(result.Message));
                
                return BadRequest(ApiResponseDto.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address {AddressId} for user {UserId}", addressId, GetCurrentUserId());
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while setting default address"));
            }
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