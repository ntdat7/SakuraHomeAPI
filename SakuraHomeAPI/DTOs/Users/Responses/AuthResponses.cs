namespace SakuraHomeAPI.DTOs.Users.Responses
{
    /// <summary>
    /// Login response with user info and tokens
    /// </summary>
    public class LoginResponseDto
    {
        public UserSummaryDto User { get; set; } = new();
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool RequiresEmailVerification { get; set; }
        public bool RequiresPhoneVerification { get; set; }
    }

    /// <summary>
    /// Token refresh response
    /// </summary>
    public class RefreshTokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// Registration response
    /// </summary>
    public class RegisterResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool RequiresEmailVerification { get; set; }
        public string? VerificationToken { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}