using System.ComponentModel.DataAnnotations;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Orders.Requests
{
    /// <summary>
    /// Create order request DTO
    /// </summary>
    public class CreateOrderRequestDto
    {
        [Required]
        public List<OrderItemRequestDto> Items { get; set; } = new();

        [Required]
        public int ShippingAddressId { get; set; }

        public int? BillingAddressId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? CouponCode { get; set; }

        [MaxLength(1000)]
        public string? OrderNotes { get; set; }

        public bool SavePaymentInfo { get; set; } = false;
        public bool ExpressDelivery { get; set; } = false;
        public bool GiftWrap { get; set; } = false;

        [MaxLength(500)]
        public string? GiftMessage { get; set; }
    }

    /// <summary>
    /// Order item request DTO
    /// </summary>
    public class OrderItemRequestDto
    {
        [Required]
        public int ProductId { get; set; }

        public int? ProductVariantId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }

        [MaxLength(1000)]
        public string? CustomOptions { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Update order request DTO
    /// </summary>
    public class UpdateOrderRequestDto
    {
        [MaxLength(1000)]
        public string? OrderNotes { get; set; }

        public int? ShippingAddressId { get; set; }
        public int? BillingAddressId { get; set; }

        [MaxLength(20)]
        public string? CouponCode { get; set; }

        public bool? ExpressDelivery { get; set; }
        public bool? GiftWrap { get; set; }

        [MaxLength(500)]
        public string? GiftMessage { get; set; }
    }

    /// <summary>
    /// Cancel order request DTO
    /// </summary>
    public class CancelOrderRequestDto
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Add order note request DTO
    /// </summary>
    public class AddOrderNoteRequestDto
    {
        [Required]
        [MaxLength(1000)]
        public string Note { get; set; } = string.Empty;
    }

    /// <summary>
    /// Add staff note request DTO
    /// </summary>
    public class AddStaffNoteRequestDto
    {
        [Required]
        [MaxLength(1000)]
        public string Note { get; set; } = string.Empty;

        public bool IsCustomerVisible { get; set; } = false;
    }

    /// <summary>
    /// Update order status request DTO
    /// </summary>
    public class UpdateOrderStatusRequestDto
    {
        [Required]
        public OrderStatus Status { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Calculate order total request DTO
    /// </summary>
    public class CalculateOrderTotalRequestDto
    {
        [Required]
        public List<OrderItemRequestDto> Items { get; set; } = new();

        public int? ShippingAddressId { get; set; }

        [MaxLength(20)]
        public string? CouponCode { get; set; }

        /// <summary>
        /// Whether to use express delivery (affects shipping cost calculation)
        /// </summary>
        public bool? ExpressDelivery { get; set; }
    }

    /// <summary>
    /// Process order request DTO (for staff)
    /// </summary>
    public class ProcessOrderRequestDto
    {
        [MaxLength(1000)]
        public string? ProcessingNotes { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }

        [MaxLength(100)]
        public string? InternalReference { get; set; }

        public bool? RequiresCustomVerification { get; set; }
    }

    /// <summary>
    /// Ship order request DTO
    /// </summary>
    public class ShipOrderRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string TrackingNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ShippingCarrier { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ShippingNotes { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? ActualShippingCost { get; set; }
    }

    /// <summary>
    /// Add order item request DTO
    /// </summary>
    public class AddOrderItemRequestDto
    {
        [Required]
        public int ProductId { get; set; }

        public int? ProductVariantId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [MaxLength(1000)]
        public string? CustomOptions { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Update order item request DTO
    /// </summary>
    public class UpdateOrderItemRequestDto
    {
        [Range(1, 100)]
        public int? Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? UnitPrice { get; set; }

        [MaxLength(1000)]
        public string? CustomOptions { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Return request DTO
    /// </summary>
    public class ReturnRequestDto
    {
        [Required]
        public List<ReturnItemRequestDto> Items { get; set; } = new();

        [Required]
        [MaxLength(50)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public List<string> Images { get; set; } = new();

        [MaxLength(50)]
        public string? PreferredResolution { get; set; }
    }

    /// <summary>
    /// Return item request DTO
    /// </summary>
    public class ReturnItemRequestDto
    {
        [Required]
        public int OrderItemId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }

        [MaxLength(500)]
        public string? ItemCondition { get; set; }

        [MaxLength(1000)]
        public string? ItemNotes { get; set; }
    }

    /// <summary>
    /// Process return request DTO
    /// </summary>
    public class ProcessReturnRequestDto
    {
        [Required]
        public ReturnStatus Status { get; set; }

        [Required]
        [MaxLength(1000)]
        public string ProcessingNotes { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal? RefundAmount { get; set; }

        [MaxLength(50)]
        public string? RefundMethod { get; set; }

        public DateTime? RefundProcessedDate { get; set; }

        [MaxLength(100)]
        public string? RefundReference { get; set; }
    }

    /// <summary>
    /// Order filter DTO
    /// </summary>
    public class OrderFilterDto
    {
        public OrderStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [MaxLength(100)]
        public string? SearchTerm { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}