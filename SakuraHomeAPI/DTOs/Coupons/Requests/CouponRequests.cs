using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Coupons.Requests
{
    public class CreateCouponRequest
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public CouponType Type { get; set; } = CouponType.Percentage;
        public decimal Value { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsPublic { get; set; } = true;
    }

    public class UpdateCouponRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public CouponType Type { get; set; }
        public decimal Value { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsPublic { get; set; }
    }

    public class ValidateCouponRequest
    {
        public string Code { get; set; }
        public decimal OrderAmount { get; set; }
        public Guid? UserId { get; set; }
    }

    public class CouponFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string Search { get; set; }
        public CouponType? Type { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsExpired { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc";
    }
}