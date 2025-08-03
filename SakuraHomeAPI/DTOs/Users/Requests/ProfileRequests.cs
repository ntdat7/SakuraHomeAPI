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
    /// Create address request
    /// </summary>
    public class CreateAddressRequestDto
    {
        [Required(ErrorMessage = "Address line 1 is required")]
        [MaxLength(255, ErrorMessage = "Address line 1 cannot exceed 255 characters")]
        public string AddressLine1 { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Address line 2 cannot exceed 255 characters")]
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "City is required")]
        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State/Province is required")]
        [MaxLength(100, ErrorMessage = "State/Province cannot exceed 100 characters")]
        public string StateProvince { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required")]
        [MaxLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
        [MaxLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string Country { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Phone number cannot exceed 100 characters")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }

        [MaxLength(100, ErrorMessage = "Recipient name cannot exceed 100 characters")]
        public string? RecipientName { get; set; }

        public bool IsDefault { get; set; } = false;

        public AddressType Type { get; set; } = AddressType.Billing;

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Update address request
    /// </summary>
    public class UpdateAddressRequestDto
    {
        [MaxLength(255, ErrorMessage = "Address line 1 cannot exceed 255 characters")]
        public string? AddressLine1 { get; set; }

        [MaxLength(255, ErrorMessage = "Address line 2 cannot exceed 255 characters")]
        public string? AddressLine2 { get; set; }

        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }

        [MaxLength(100, ErrorMessage = "State/Province cannot exceed 100 characters")]
        public string? StateProvince { get; set; }

        [MaxLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string? PostalCode { get; set; }

        [MaxLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string? Country { get; set; }

        [MaxLength(100, ErrorMessage = "Phone number cannot exceed 100 characters")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }

        [MaxLength(100, ErrorMessage = "Recipient name cannot exceed 100 characters")]
        public string? RecipientName { get; set; }

        public bool? IsDefault { get; set; }

        public AddressType? Type { get; set; }

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}