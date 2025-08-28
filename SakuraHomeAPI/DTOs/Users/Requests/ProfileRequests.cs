using System.ComponentModel.DataAnnotations;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Users.Requests
{
    /// <summary>
    /// Update user profile request
    /// </summary>
    public class UpdateProfileRequestDto
    {
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string? FirstName { get; set; }

        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string? LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public Gender? Gender { get; set; }

        [MaxLength(500, ErrorMessage = "Avatar URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Avatar must be a valid URL")]
        public string? Avatar { get; set; }

        [MaxLength(5, ErrorMessage = "Language code cannot exceed 5 characters")]
        public string? PreferredLanguage { get; set; }

        [MaxLength(3, ErrorMessage = "Currency code cannot exceed 3 characters")]
        public string? PreferredCurrency { get; set; }

        public bool? EmailNotifications { get; set; }
        public bool? SmsNotifications { get; set; }
        public bool? PushNotifications { get; set; }
    }

    /// <summary>
    /// Create address request - Vietnam address structure only
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

        // Backward compatibility properties - will be ignored but prevent compilation errors
        public string? RecipientName { get => Name; set => Name = value ?? string.Empty; }
        public string? PhoneNumber { get => Phone; set => Phone = value ?? string.Empty; }
        public string? City { get; set; } // Will be ignored
        public string? StateProvince { get; set; } // Will be ignored
    }

    /// <summary>
    /// Update address request - Vietnam address structure only
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

        // Backward compatibility properties - will be ignored but prevent compilation errors
        public string? RecipientName { get => Name; set => Name = value; }
        public string? PhoneNumber { get => Phone; set => Phone = value; }
        public string? City { get; set; } // Will be ignored
        public string? StateProvince { get; set; } // Will be ignored
    }
}
