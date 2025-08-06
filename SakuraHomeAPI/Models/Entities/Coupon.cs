using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities
{
    [Table("Coupons")]
    public class Coupon : AuditableEntity
    {
        [Required, MaxLength(50)]
        public string Code { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        public CouponType Type { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Value { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinOrderAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxDiscountAmount { get; set; }

        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; } = 0;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsPublic { get; set; } = true;

        [Timestamp]
        public byte[] RowVersion { get; set; }

        // Computed properties
        public bool IsValid => IsActive && 
                              DateTime.UtcNow >= StartDate && 
                              DateTime.UtcNow <= EndDate && 
                              (UsageLimit == null || UsedCount < UsageLimit);

        public bool CanUse(int currentUsage = 0) => 
            IsValid && (UsageLimit == null || (UsedCount + currentUsage) <= UsageLimit);

        public string StatusDisplay => IsValid ? "Đang hoạt động" : 
                                     DateTime.UtcNow < StartDate ? "Chưa bắt đầu" :
                                     DateTime.UtcNow > EndDate ? "Đã hết hạn" : "Ngừng hoạt động";

        public string TypeDisplay => Type switch
        {
            CouponType.Percentage => "Giảm theo %",
            CouponType.FixedAmount => "Giảm cố định",
            CouponType.FreeShipping => "Miễn phí ship",
            _ => Type.ToString()
        };

        public string ValueDisplay => Type switch
        {
            CouponType.Percentage => $"{Value}%",

            CouponType.FixedAmount => $"{Value:N0} VND",
            CouponType.FreeShipping => "Miễn phí ship",
            _ => Value.ToString()
        };

        // Methods for coupon validation and usage
        public bool IsValidForOrder(decimal orderAmount)
        {
            if (!IsValid) return false;
            if (MinOrderAmount.HasValue && orderAmount < MinOrderAmount.Value) return false;
            return true;
        }

        public bool TryIncrementUsage()
        {
            if (!CanUse()) return false;
            UsedCount++;
            return true;
        }

        public void IncrementUsage()
        {
            UsedCount++;
        }

        public void DecrementUsage()
        {
            if (UsedCount > 0)
                UsedCount--;
        }
    }
}
