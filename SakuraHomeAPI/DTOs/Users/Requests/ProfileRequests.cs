using System.ComponentModel.DataAnnotations;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.DTOs.Common;

namespace SakuraHomeAPI.DTOs.Users.Requests
{
    /// <summary>
    /// Update user profile request
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
    /// Create address request - Vietnam address structure
    /// </summary>
    public class CreateAddressRequestDto
    {
        [Required(ErrorMessage = "Recipient name is required")]
        [MaxLength(100, ErrorMessage = "Recipient name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address line 1 is required")]
        [MaxLength(255, ErrorMessage = "Address line 1 cannot exceed 255 characters")]
        public string AddressLine1 { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Address line 2 cannot exceed 255 characters")]
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "Province is required")]
        public int ProvinceId { get; set; }

        [Required(ErrorMessage = "Ward is required")]
        public int WardId { get; set; }

        [MaxLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string? PostalCode { get; set; }

        [MaxLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string Country { get; set; } = "Vietnam";

        public bool IsDefault { get; set; } = false;

        public AddressType Type { get; set; } = AddressType.Both;

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Update address request - Vietnam address structure
    /// </summary>
    public class UpdateAddressRequestDto
    {
        [MaxLength(100, ErrorMessage = "Recipient name cannot exceed 100 characters")]
        public string? Name { get; set; }

        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? Phone { get; set; }

        [MaxLength(255, ErrorMessage = "Address line 1 cannot exceed 255 characters")]
        public string? AddressLine1 { get; set; }

        [MaxLength(255, ErrorMessage = "Address line 2 cannot exceed 255 characters")]
        public string? AddressLine2 { get; set; }

        public int? ProvinceId { get; set; }

        public int? WardId { get; set; }

        [MaxLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string? PostalCode { get; set; }

        [MaxLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string? Country { get; set; }

        public bool? IsDefault { get; set; }

        public AddressType? Type { get; set; }

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// User filter request for admin lists
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
}
