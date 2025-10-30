using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.DTOs.Users;
using SakuraHomeAPI.DTOs.Users.Requests;
using SakuraHomeAPI.DTOs.Users.Responses;
using SakuraHomeAPI.Models.Entities.Identity;

namespace SakuraHomeAPI.Services.Interfaces
{
    /// <summary>
    /// Interface for authentication service
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticate user with email and password
        /// </summary>
        /// <param name="request">Login request</param>
        /// <param name="ipAddress">Client IP address</param>
        /// <returns>Authentication response</returns>
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request, string ipAddress);

        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="request">Registration request</param>
        /// <param name="ipAddress">Client IP address</param>
        /// <returns>Authentication response</returns>
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, string ipAddress);

        /// <summary>
        /// Logout user and revoke refresh token
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="refreshToken">Refresh token to revoke</param>
        /// <returns>Success response</returns>
        Task<ApiResponse> LogoutAsync(Guid userId, string? refreshToken = null);

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <param name="ipAddress">Client IP address</param>
        /// <returns>New authentication response</returns>
        Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request, string ipAddress);

        /// <summary>
        /// Send password reset email
        /// </summary>
        /// <param name="request">Forgot password request</param>
        /// <returns>Success response</returns>
        Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequestDto request);

        /// <summary>
        /// Reset password using reset token
        /// </summary>
        /// <param name="request">Reset password request</param>
        /// <returns>Success response</returns>
        Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequestDto request);

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="request">Change password request</param>
        /// <returns>Success response</returns>
        Task<ApiResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);

        /// <summary>
        /// Verify user email
        /// </summary>
        /// <param name="request">Email verification request</param>
        /// <returns>Success response</returns>
        Task<ApiResponse> VerifyEmailAsync(VerifyEmailRequestDto request);

        /// <summary>
        /// Resend email verification
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>Success response</returns>
        Task<ApiResponse> ResendEmailVerificationAsync(string email);

        /// <summary>
        /// Get current user information
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User information</returns>
        Task<ApiResponse<UserDto>> GetCurrentUserAsync(Guid userId);

        /// <summary>
        /// Revoke refresh token
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="token">Refresh token to revoke</param>
        /// <returns>Success response</returns>
        Task<ApiResponse> RevokeTokenAsync(Guid userId, string token);

        /// <summary>
        /// Revoke all user tokens
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Success response</returns>
        Task<ApiResponse> RevokeAllTokensAsync(Guid userId);
    }
}