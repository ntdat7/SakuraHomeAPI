using SakuraHomeAPI.Models.Entities.Identity;
using System.Security.Claims;

namespace SakuraHomeAPI.Services.Interfaces
{
    /// <summary>
    /// Interface for JWT token service
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generate JWT access token for user
        /// </summary>
        /// <param name="user">User to generate token for</param>
        /// <returns>JWT token string</returns>
        string GenerateAccessToken(User user);

        /// <summary>
        /// Generate refresh token
        /// </summary>
        /// <returns>Refresh token string</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Validate JWT token and return claims principal
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>Claims principal if valid, null otherwise</returns>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Get user ID from token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>User ID if valid, null otherwise</returns>
        Guid? GetUserIdFromToken(string token);

        /// <summary>
        /// Check if token is expired
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>True if expired, false otherwise</returns>
        bool IsTokenExpired(string token);

        /// <summary>
        /// Get token expiration time
        /// </summary>
        /// <param name="durationInMinutes">Token duration in minutes</param>
        /// <returns>Expiration DateTime</returns>
        DateTime GetTokenExpiration(int? durationInMinutes = null);
    }
}