using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Users.Responses
{
    /// <summary>
    /// User profile information
    /// </summary>
    public class UserProfileDto
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
        public string PreferredLanguage { get; set; } = "vi";
        public string? PreferredCurrency { get; set; }
        public UserRole Role { get; set; }
        public AccountStatus Status { get; set; }
        public UserTier Tier { get; set; }
        public LoginProvider Provider { get; set; }
        public bool EmailVerified { get; set; }
        public bool PhoneVerified { get; set; }
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public int Points { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalOrders { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Related data
        public List<AddressSummaryDto> Addresses { get; set; } = new();
    }

    /// <summary>
    /// User summary for admin lists
    /// </summary>
    public class UserSummaryDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public AccountStatus Status { get; set; }
        public UserTier Tier { get; set; }
        public bool EmailVerified { get; set; }
        public bool PhoneVerified { get; set; }
        public int Points { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalOrders { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    /// <summary>
    /// Address summary information
    /// </summary>
    public class AddressSummaryDto
    {
        public int Id { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string StateProvince { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? RecipientName { get; set; }
        public bool IsDefault { get; set; }
        public AddressType Type { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// User activity information
    /// </summary>
    public class UserActivityDto
    {
        public int Id { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Details { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}