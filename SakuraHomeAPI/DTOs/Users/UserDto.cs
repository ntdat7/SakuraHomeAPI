using System.ComponentModel.DataAnnotations;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.DTOs.Common;

namespace SakuraHomeAPI.DTOs.Users
{
    /// <summary>
    /// User summary information
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
    /// Detailed user profile
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
    /// Address summary information
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
    /// Login response
    /// </summary>
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserSummaryDto User { get; set; } = new();
    }

    /// <summary>
    /// Update profile request
    /// </summary>
    public class UpdateProfileRequestDto
    {
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [MinLength(1, ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [MinLength(1, ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }

        [MaxLength(500, ErrorMessage = "Avatar URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Avatar must be a valid URL")]
        public string? Avatar { get; set; }

        [MaxLength(5, ErrorMessage = "Language code cannot exceed 5 characters")]
        public string PreferredLanguage { get; set; } = "vi";

        [MaxLength(3, ErrorMessage = "Currency code cannot exceed 3 characters")]
        public string PreferredCurrency { get; set; } = "VND";

        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public bool PushNotifications { get; set; } = true;
    }

    /// <summary>
    /// Create address request
    /// </summary>
    public class CreateAddressRequestDto
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address line 1 is required")]
        [MaxLength(255, ErrorMessage = "Address line 1 cannot exceed 255 characters")]
        public string AddressLine1 { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Address line 2 cannot exceed 255 characters")]
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "City is required")]
        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required")]
        [MaxLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required")]
        [MaxLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
        [MaxLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string Country { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? Phone { get; set; }

        public AddressType Type { get; set; } = AddressType.Both;
        public bool IsDefault { get; set; } = false;

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Update address request
    /// </summary>
    public class UpdateAddressRequestDto : CreateAddressRequestDto
    {
        // Inherits all properties from CreateAddressRequestDto
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
    /// User filter request
    /// </summary>
    public class UserFilterRequestDto : PaginationRequestDto
    {
        [MaxLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
        public string? Search { get; set; }

        public UserRole? Role { get; set; }
        public AccountStatus? Status { get; set; }
        public UserTier? Tier { get; set; }
        public LoginProvider? Provider { get; set; }
        public bool? IsActive { get; set; }
        public bool? EmailVerified { get; set; }
        public bool? PhoneVerified { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? LastLoginFrom { get; set; }
        public DateTime? LastLoginTo { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Min total spent must be greater than or equal to 0")]
        public decimal? MinTotalSpent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Max total spent must be greater than or equal to 0")]
        public decimal? MaxTotalSpent { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Min total orders must be greater than or equal to 0")]
        public int? MinTotalOrders { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Max total orders must be greater than or equal to 0")]
        public int? MaxTotalOrders { get; set; }

        [RegularExpression("^(name|email|created|lastLogin|totalSpent|totalOrders)$",
            ErrorMessage = "SortBy must be one of: name, email, created, lastLogin, totalSpent, totalOrders")]
        public string SortBy { get; set; } = "created";

        [RegularExpression("^(asc|desc)$", ErrorMessage = "SortOrder must be either 'asc' or 'desc'")]
        public string SortOrder { get; set; } = "desc";

        // Validation methods
        public bool IsValidCreatedDateRange()
        {
            return !CreatedFrom.HasValue || !CreatedTo.HasValue || CreatedFrom <= CreatedTo;
        }

        public bool IsValidLastLoginDateRange()
        {
            return !LastLoginFrom.HasValue || !LastLoginTo.HasValue || LastLoginFrom <= LastLoginTo;
        }

        public bool IsValidTotalSpentRange()
        {
            return !MinTotalSpent.HasValue || !MaxTotalSpent.HasValue || MinTotalSpent <= MaxTotalSpent;
        }

        public bool IsValidTotalOrdersRange()
        {
            return !MinTotalOrders.HasValue || !MaxTotalOrders.HasValue || MinTotalOrders <= MaxTotalOrders;
        }
    }

    /// <summary>
    /// User list response
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
    /// User statistics
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
    /// User role count
    /// </summary>
    public class UserRoleCountDto
    {
        public UserRole Role { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// User tier count
    /// </summary>
    public class UserTierCountDto
    {
        public UserTier Tier { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// User status count
    /// </summary>
    public class UserStatusCountDto
    {
        public AccountStatus Status { get; set; }
        public int Count { get; set; }
    }
}