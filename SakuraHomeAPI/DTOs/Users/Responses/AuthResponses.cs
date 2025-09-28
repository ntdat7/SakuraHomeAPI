using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Users.Responses
{
    /// <summary>
    /// Authentication response with tokens and user info
    /// </summary>
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public UserDto User { get; set; } = new();
    }

    /// <summary>
    /// User information DTO
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public string? Avatar { get; set; }
        public UserRole Role { get; set; }
        public UserTier Tier { get; set; }
        public int Points { get; set; }
        public decimal TotalSpent { get; set; }
        public AccountStatus Status { get; set; }
        public bool EmailVerified { get; set; }
        public bool PhoneVerified { get; set; }
        public string PreferredLanguage { get; set; } = "vi";
        public string PreferredCurrency { get; set; } = "VND";
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    /// <summary>
    /// Login response - alias for AuthResponseDto for backward compatibility
    /// </summary>
    public class LoginResponseDto : AuthResponseDto
    {
        public bool RequiresEmailVerification { get; set; }
        public bool RequiresPhoneVerification { get; set; }
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
        public AuthResponseDto? AuthData { get; set; }
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
}