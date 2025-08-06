using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Coupons.Responses
{
    public class CouponResponse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public CouponType Type { get; set; }
        public decimal Value { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Computed properties
        public bool IsExpired { get; set; }
        public bool IsValid { get; set; }
        public int RemainingUsage { get; set; }
        public double UsagePercentage { get; set; }
        public string TypeDisplay { get; set; }
        public string ValueDisplay { get; set; }
        public string StatusDisplay { get; set; }
    }

    public class CouponValidationResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public CouponResponse Coupon { get; set; }
    }

    public class CouponListResponse
    {
        public List<CouponResponse> Items { get; set; } = new List<CouponResponse>();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    public class CouponStatsResponse
    {
        public int TotalCoupons { get; set; }
        public int ActiveCoupons { get; set; }
        public int ExpiredCoupons { get; set; }
        public int UsedCoupons { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public int TotalUsages { get; set; }
        public Dictionary<CouponType, int> CouponsByType { get; set; } = new Dictionary<CouponType, int>();
        public List<TopCouponResponse> TopCoupons { get; set; } = new List<TopCouponResponse>();
    }

    public class TopCouponResponse
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int UsedCount { get; set; }
        public decimal TotalDiscountAmount { get; set; }
    }
}