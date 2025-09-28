using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.DTOs.Common;

namespace SakuraHomeAPI.DTOs.Users.Responses
{
    /// <summary>
    /// Address response DTO - Vietnam address structure
    /// </summary>
    public class AddressResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public int ProvinceId { get; set; }
        public int WardId { get; set; }
        public string? PostalCode { get; set; }
        public string Country { get; set; } = "Vietnam";
        public bool IsDefault { get; set; }
        public AddressType Type { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Display properties - filled by service layer with lookups
        public string? ProvinceName { get; set; }
        public string? WardName { get; set; }

        /// <summary>
        /// Get formatted full address
        /// Note: This requires ProvinceName and WardName to be populated by service layer
        /// </summary>
        public string FullAddress =>
            $"{AddressLine1}" +
            (!string.IsNullOrEmpty(AddressLine2) ? $", {AddressLine2}" : "") +
            (!string.IsNullOrEmpty(WardName) ? $", {WardName}" : "") +
            (!string.IsNullOrEmpty(ProvinceName) ? $", {ProvinceName}" : "") +
            (!string.IsNullOrEmpty(PostalCode) ? $" {PostalCode}" : "") +
            $", {Country}";

        /// <summary>
        /// Short address for display in lists
        /// </summary>
        public string ShortAddress => AddressLine1;
    }

    /// <summary>
    /// Simple address response for dropdown/selection
    /// </summary>
    public class AddressSimpleResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ShortAddress { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public AddressType Type { get; set; }
    }

    /// <summary>
    /// Address summary information (for backward compatibility)
    /// </summary>
    public class AddressSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public AddressType Type { get; set; }
        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// User summary information for lists and basic display
    /// </summary>
    public class UserSummaryDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public UserRole Role { get; set; }
        public AccountStatus Status { get; set; }
        public UserTier Tier { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    /// <summary>
    /// Detailed user profile extending UserSummaryDto
    /// </summary>
    public class UserProfileDto : UserSummaryDto
    {
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public string PreferredLanguage { get; set; } = "vi";
        public string PreferredCurrency { get; set; } = "VND";
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public int Points { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalOrders { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public bool EmailVerified { get; set; }
        public bool PhoneVerified { get; set; }
        public LoginProvider Provider { get; set; }
        public List<AddressSummaryDto> Addresses { get; set; } = new();
    }

    /// <summary>
    /// User activity information
    /// </summary>
    public class UserActivityDto
    {
        public int Id { get; set; }
        public ActivityType ActivityType { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? IpAddress { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
    }

    /// <summary>
    /// User statistics DTO
    /// </summary>
    public class UserStatsDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public int Points { get; set; }
        public UserTier Tier { get; set; }
        public int ReviewsCount { get; set; }
        public int WishlistItemsCount { get; set; }
        public int AddressesCount { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int DaysSinceRegistration { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int CancelledOrdersCount { get; set; }
        public int ReturnedOrdersCount { get; set; }
        public double AverageRatingGiven { get; set; }
        public bool IsVipCustomer { get; set; }
        public string PreferredCategories { get; set; } = string.Empty;
        public string PreferredBrands { get; set; } = string.Empty;
    }

    /// <summary>
    /// Extended user profile DTO with detailed information
    /// </summary>
    public class UserProfileDetailDto
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
        
        // Extended data
        public List<AddressResponseDto> Addresses { get; set; } = new();
        public UserStatsDto Stats { get; set; } = new();
        public List<string> RecentActivities { get; set; } = new();
        public Dictionary<string, object> Preferences { get; set; } = new();
    }

    /// <summary>
    /// User list response for admin
    /// </summary>
    public class UserListResponseDto : PagedResponseDto<UserSummaryDto>
    {
        public UserFilterInfoDto Filters { get; set; } = new();
        public UserStatisticsDto Statistics { get; set; } = new();
    }

    /// <summary>
    /// User filter information
    /// </summary>
    public class UserFilterInfoDto
    {
        public string? Search { get; set; }
        public UserRole? Role { get; set; }
        public AccountStatus? Status { get; set; }
        public UserTier? Tier { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "created";
        public string SortOrder { get; set; } = "desc";
        public int TotalFiltersApplied { get; set; }
    }

    /// <summary>
    /// User statistics for admin dashboard
    /// </summary>
    public class UserStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public double AvgTotalSpent { get; set; }
        public double AvgTotalOrders { get; set; }
        public List<UserRoleCountDto> RoleCounts { get; set; } = new();
        public List<UserTierCountDto> TierCounts { get; set; } = new();
        public List<UserStatusCountDto> StatusCounts { get; set; } = new();
    }

    /// <summary>
    /// User role count for statistics
    /// </summary>
    public class UserRoleCountDto
    {
        public UserRole Role { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// User tier count for statistics
    /// </summary>
    public class UserTierCountDto
    {
        public UserTier Tier { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// User status count for statistics
    /// </summary>
    public class UserStatusCountDto
    {
        public AccountStatus Status { get; set; }
        public int Count { get; set; }
    }
}