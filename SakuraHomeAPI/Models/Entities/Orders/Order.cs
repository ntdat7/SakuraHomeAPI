using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using static Azure.Core.HttpHeader;

namespace SakuraHomeAPI.Models.Entities.Orders
{
    /// <summary>
    /// Order entity
    /// </summary>
    [Table("Orders")]
    public class Order : AuditableEntity
    {
        #region Basic Information

        public int UserId { get; set; }

        [Required, MaxLength(20)]
        public string OrderNumber { get; set; }

        #endregion

        #region Pricing Information

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiftWrapFee { get; set; } = 0;

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "VND";

        #endregion

        #region Status Information

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        #endregion

        #region Shipping Information

        [Required, MaxLength(100)]
        public string ReceiverName { get; set; }

        [Required, MaxLength(20), Phone]
        public string ReceiverPhone { get; set; }

        [MaxLength(255)]
        public string ReceiverEmail { get; set; }

        [Required]
        public string ShippingAddress { get; set; }

        [MaxLength(255)]
        public string BillingAddress { get; set; }

        public DeliveryMethod DeliveryMethod { get; set; } = DeliveryMethod.Standard;
        public DateTime? EstimatedDeliveryDate { get; set; }

        [MaxLength(100)]
        public string TrackingNumber { get; set; }

        [MaxLength(255)]
        public string ShippingCarrier { get; set; }

        #endregion

        #region Coupon & Payment Information

        public int? CouponId { get; set; }

        [MaxLength(50)]
        public string CouponCode { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CouponDiscount { get; set; } = 0;

        public int? PaymentMethodId { get; set; }
        public int? ShippingZoneId { get; set; }

        #endregion

        #region Important Dates

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? ProcessingDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public DateTime? RefundedDate { get; set; }

        #endregion

        #region Notes & Additional Information

        public string CustomerNotes { get; set; }
        public string AdminNotes { get; set; }
        public string CancelReason { get; set; }
        public string ReturnReason { get; set; }

        public CancellationReason? CancellationReasonCode { get; set; }
        public ReturnReason? ReturnReasonCode { get; set; }

        public bool IsGift { get; set; } = false;
        public string GiftMessage { get; set; }
        public bool GiftWrapRequested { get; set; } = false;

        public bool IsUrgent { get; set; } = false;
        public bool RequiresSignature { get; set; } = false;
        public bool IsInsured { get; set; } = false;

        #endregion

        #region Navigation Properties

        public virtual User User { get; set; }
        public virtual Coupon Coupon { get; set; }
        public virtual PaymentMethodInfo PaymentMethodDetails { get; set; }
        public virtual ShippingZone ShippingZone { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
        public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
        public virtual ICollection<OrderNote> OrderNotes { get; set; } = new List<OrderNote>();

        #endregion

        #region Computed Properties

        [NotMapped]
        public int TotalItems => OrderItems.Sum(oi => oi.Quantity);

        [NotMapped]
        public int UniqueItems => OrderItems.Count;

        [NotMapped]
        public bool CanCancel => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;

        [NotMapped]
        public bool CanShip => Status == OrderStatus.Confirmed || Status == OrderStatus.Processing;

        [NotMapped]
        public bool CanDeliver => Status == OrderStatus.Shipped || Status == OrderStatus.OutForDelivery;

        [NotMapped]
        public bool CanReturn =>
            Status == OrderStatus.Delivered &&
            DeliveredDate.HasValue &&
            (DateTime.UtcNow - DeliveredDate.Value).TotalDays <= 30;

        [NotMapped]
        public bool IsCompleted => Status == OrderStatus.Delivered;

        [NotMapped]
        public bool IsCancelled => Status == OrderStatus.Cancelled;

        [NotMapped]
        public bool IsReturned => Status == OrderStatus.Returned;

        [NotMapped]
        public bool IsRefunded => Status == OrderStatus.Refunded;

        [NotMapped]
        public bool IsPaid => PaymentStatus == PaymentStatus.Paid;

        [NotMapped]
        public string FormattedOrderNumber => $"ORD-{OrderNumber}";

        [NotMapped]
        public string FormattedTotal => $"{TotalAmount:N0} {Currency}";

        [NotMapped]
        public TimeSpan? ProcessingTime =>
            OrderDate != default && DeliveredDate.HasValue
                ? DeliveredDate.Value - OrderDate
                : null;

        #endregion

        #region Methods

        /// <summary>
        /// Generate unique order number
        /// </summary>
        public static string GenerateOrderNumber()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(100, 999);
            return $"{timestamp}{random}";
        }

        /// <summary>
        /// Calculate order totals
        /// </summary>
        public void CalculateTotals()
        {
            SubTotal = OrderItems.Sum(oi => oi.TotalPrice);
            TotalAmount = SubTotal
                        + ShippingFee
                        + TaxAmount
                        + GiftWrapFee
                        - DiscountAmount
                        - CouponDiscount;
            if (TotalAmount < 0) TotalAmount = 0;
        }

        /// <summary>
        /// Update order status with history tracking
        /// </summary>
        public void UpdateStatus(OrderStatus newStatus, string notes = null, int? updatedBy = null)
        {
            var oldStatus = Status;
            Status = newStatus;

            var now = DateTime.UtcNow;
            switch (newStatus)
            {
                case OrderStatus.Confirmed: ConfirmedDate = now; break;
                case OrderStatus.Processing: ProcessingDate = now; break;
                case OrderStatus.Shipped: ShippedDate = now; break;
                case OrderStatus.Delivered: DeliveredDate = now; break;
                case OrderStatus.Cancelled: CancelledDate = now; break;
                case OrderStatus.Returned: ReturnedDate = now; break;
                case OrderStatus.Refunded: RefundedDate = now; break;
            }

            StatusHistory.Add(new OrderStatusHistory
            {
                OrderId = Id,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Notes = notes,
                CreatedBy = updatedBy,
                CreatedAt = now
            });
        }

        /// <summary>
        /// Check valid status transition
        /// </summary>
        public bool CanUpdateToStatus(OrderStatus newStatus) =>
            newStatus switch
            {
                OrderStatus.Confirmed => Status == OrderStatus.Pending,
                OrderStatus.Processing => Status == OrderStatus.Confirmed,
                OrderStatus.Shipped => Status == OrderStatus.Processing,
                OrderStatus.OutForDelivery => Status == OrderStatus.Shipped,
                OrderStatus.Delivered => Status == OrderStatus.OutForDelivery || Status == OrderStatus.Shipped,
                OrderStatus.Cancelled => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed,
                OrderStatus.Returned => Status == OrderStatus.Delivered,
                OrderStatus.Refunded => Status == OrderStatus.Cancelled || Status == OrderStatus.Returned,
                _ => false
            };

        /// <summary>
        /// Apply coupon discount
        /// </summary>
        public bool ApplyCoupon(Coupon coupon)
        {
            if (coupon == null) return false;

            // Cập nhật subtotal trước khi kiểm tra
            SubTotal = OrderItems?.Sum(oi => oi.TotalPrice) ?? 0;

            if (!coupon.IsValidForOrder(SubTotal)) return false;

            CouponId = coupon.Id;
            CouponCode = coupon.Code;
            CouponDiscount = coupon.Type switch
            {
                CouponType.Percentage => Math.Min(SubTotal * coupon.Value / 100m, coupon.MaxDiscountAmount ?? decimal.MaxValue),
                CouponType.FixedAmount => Math.Min(coupon.Value, SubTotal),
                CouponType.FreeShipping => ShippingFee,
                _ => 0
            };

            CalculateTotals();

            try
            {
                coupon.IncrementUsage();
            }
            catch (InvalidOperationException)
            {
                // Trường hợp bị vượt quá giới hạn sử dụng do race condition
                return false;
            }

            return true;
        }


        /// <summary>
        /// Remove coupon
        /// </summary>
        public void RemoveCoupon()
        {
            CouponId = null;
            CouponCode = null;
            CouponDiscount = 0;
            CalculateTotals();
        }

        #endregion
    }
}
