using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Users.Responses
{
    /// <summary>
    /// Address response DTO
    /// </summary>
    public class AddressResponseDto
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

        /// <summary>
        /// Get formatted full address
        /// </summary>
        public string FullAddress => 
            $"{AddressLine1}{(!string.IsNullOrEmpty(AddressLine2) ? ", " + AddressLine2 : "")}, {City}, {StateProvince} {PostalCode}, {Country}";
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
    /// Extended user profile DTO with additional details
    /// </summary>
    public class UserProfileDetailDto : UserProfileDto
    {
        public List<AddressResponseDto> Addresses { get; set; } = new();
        public UserStatsDto Stats { get; set; } = new();
        public List<string> RecentActivities { get; set; } = new();
        public Dictionary<string, object> Preferences { get; set; } = new();
    }
}