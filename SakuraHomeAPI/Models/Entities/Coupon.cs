using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.Models.Entities
{
    [Table("Coupons")]
    public class Coupon : BaseEntity
    {
        [Required, MaxLength(50)]
        public string Code { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public CouponType Type { get; set; } = CouponType.Percentage;

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

        /// <summary>
        /// Concurrency token to prevent race conditions when updating usage count.
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; }

        #region Validation / Usage

        /// <summary>
        /// Determines whether this coupon is valid for a given order amount.
        /// Checks active flag, date range, usage limit, and minimum order amount.
        /// </summary>
        public bool IsValidForOrder(decimal orderAmount)
        {
            if (!IsActive) return false;

            var now = DateTime.UtcNow;
            if (now < StartDate || now > EndDate) return false;

            if (UsageLimit.HasValue && UsedCount >= UsageLimit.Value) return false;

            if (MinOrderAmount.HasValue && orderAmount < MinOrderAmount.Value) return false;

            return true;
        }

        /// <summary>
        /// Increments the usage count. Throws if limit exceeded.
        /// </summary>
        public void IncrementUsage()
        {
            if (UsageLimit.HasValue && UsedCount >= UsageLimit.Value)
                throw new InvalidOperationException("Coupon usage limit exceeded.");

            UsedCount++;
        }

        /// <summary>
        /// Attempts to increment usage count; returns false if limit reached.
        /// </summary>
        public bool TryIncrementUsage()
        {
            if (UsageLimit.HasValue && UsedCount >= UsageLimit.Value)
                return false;

            UsedCount++;
            return true;
        }

        /// <summary>
        /// Decrements usage count (e.g., rollback on order cancellation), never below zero.
        /// </summary>
        public void DecrementUsage()
        {
            if (UsedCount > 0)
                UsedCount--;
        }

        #endregion
    }
}
