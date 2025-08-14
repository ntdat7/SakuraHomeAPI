using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Orders.Responses
{
    /// <summary>
    /// Order response DTO
    /// </summary>
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public OrderStatus Status { get; set; }
        
        // Customer Information
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        
        // Order Items
        public List<OrderItemDto> Items { get; set; } = new();
        
        // Addresses
        public OrderAddressDto ShippingAddress { get; set; } = new();
        public OrderAddressDto? BillingAddress { get; set; }
        
        // Financial Information
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        
        // Payment Information
        public string PaymentMethod { get; set; } = string.Empty;
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
        
        // Shipping Information
        public string? TrackingNumber { get; set; }
        public string? ShippingCarrier { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        
        // Order Details
        public string? OrderNotes { get; set; }
        public string? CouponCode { get; set; }
        public bool ExpressDelivery { get; set; }
        public bool GiftWrap { get; set; }
        public string? GiftMessage { get; set; }
        
        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? CancellationReason { get; set; }
        
        // Status Information
        public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();
        public List<OrderNoteDto> Notes { get; set; } = new();
        
        // Computed Properties
        public bool CanCancel => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;
        public bool CanReturn => Status == OrderStatus.Delivered && 
                                DateTime.UtcNow.Subtract(DeliveredDate ?? DateTime.UtcNow).TotalDays <= 30;
        public bool IsCompleted => Status == OrderStatus.Delivered;
        public string StatusText => Status switch
        {
            OrderStatus.Pending => "?ang ch? x? lý",
            OrderStatus.Confirmed => "?ã xác nh?n",
            OrderStatus.Processing => "?ang x? lý",
            OrderStatus.Shipped => "?ã giao cho v?n chuy?n",
            OrderStatus.Delivered => "?ã giao hàng",
            OrderStatus.Cancelled => "?ã h?y",
            OrderStatus.Returned => "?ã tr? hàng",
            _ => "Không xác ??nh"
        };
    }

    /// <summary>
    /// Order item DTO
    /// </summary>
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        
        // Product Information (Snapshot)
        public string ProductName { get; set; } = string.Empty;
        public string ProductSku { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string? VariantName { get; set; }
        public string? VariantSku { get; set; }
        
        // Order Specific Information
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? CustomOptions { get; set; }
        public string? Notes { get; set; }
        
        // Brand and Category
        public string BrandName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        
        // Current Product Information
        public decimal? CurrentPrice { get; set; }
        public bool? IsStillAvailable { get; set; }
    }

    /// <summary>
    /// Order address DTO
    /// </summary>
    public class OrderAddressDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Notes { get; set; }
        
        public string FullAddress => 
            $"{AddressLine1}{(!string.IsNullOrEmpty(AddressLine2) ? ", " + AddressLine2 : "")}, {City}, {State} {PostalCode}, {Country}";
    }

    /// <summary>
    /// Order summary DTO (for lists)
    /// </summary>
    public class OrderSummaryDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public PaymentStatus PaymentStatus { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string? TrackingNumber { get; set; }
        
        // Preview of first few items
        public List<string> ItemNames { get; set; } = new();
        public string ItemsPreview => string.Join(", ", ItemNames.Take(3)) + 
                                     (ItemNames.Count > 3 ? $" và {ItemNames.Count - 3} s?n ph?m khác" : "");
    }

    /// <summary>
    /// Order status history DTO
    /// </summary>
    public class OrderStatusHistoryDto
    {
        public int Id { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public bool IsSystemGenerated { get; set; }
    }

    /// <summary>
    /// Order note DTO
    /// </summary>
    public class OrderNoteDto
    {
        public int Id { get; set; }
        public string Note { get; set; } = string.Empty;
        public bool IsCustomerVisible { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public bool IsFromCustomer { get; set; }
    }

    /// <summary>
    /// Order validation DTO
    /// </summary>
    public class OrderValidationDto
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        
        // Validation Details
        public bool AllItemsAvailable { get; set; }
        public bool SufficientStock { get; set; }
        public bool ValidShippingAddress { get; set; }
        public bool ValidPaymentMethod { get; set; }
        public bool ValidCoupon { get; set; }
        
        // Calculated Values
        public decimal EstimatedTotal { get; set; }
        public decimal EstimatedShipping { get; set; }
        public decimal EstimatedTax { get; set; }
        public decimal DiscountAmount { get; set; }
        
        // Items Status
        public List<OrderItemValidationDto> ItemValidations { get; set; } = new();
    }

    /// <summary>
    /// Order item validation DTO
    /// </summary>
    public class OrderItemValidationDto
    {
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int RequestedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public bool HasSufficientStock { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal RequestedPrice { get; set; }
        public bool PriceChanged { get; set; }
        public List<string> Issues { get; set; } = new();
    }

    /// <summary>
    /// Order statistics DTO
    /// </summary>
    public class OrderStatsDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        
        // Status Breakdown
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int ReturnedOrders { get; set; }
        
        // Payment Status
        public int PaidOrders { get; set; }
        public int PendingPaymentOrders { get; set; }
        public int FailedPaymentOrders { get; set; }
        
        // Time Period
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        
        // Trends
        public decimal RevenueGrowth { get; set; }
        public decimal OrderGrowth { get; set; }
        public double CancellationRate { get; set; }
        public double ReturnRate { get; set; }
        
        // Top Products
        public List<TopProductDto> TopProducts { get; set; } = new();
    }

    /// <summary>
    /// Top product DTO
    /// </summary>
    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
    }

    /// <summary>
    /// Order detail DTO (extended information)
    /// </summary>
    public class OrderDetailDto : OrderResponseDto
    {
        // Additional Financial Details
        public decimal ItemsTotal { get; set; }
        public decimal CouponDiscount { get; set; }
        public decimal LoyaltyDiscount { get; set; }
        public decimal ProcessingFee { get; set; }
        
        // Additional Shipping Details
        public string? ShippingZone { get; set; }
        public string? ShippingMethod { get; set; }
        public decimal? ActualShippingCost { get; set; }
        
        // Additional Order Information
        public string? InternalReference { get; set; }
        public string? CustomerNotes { get; set; }
        public string? StaffNotes { get; set; }
        
        // Return Information
        public List<OrderReturnDto> Returns { get; set; } = new();
        
        // Tracking Events
        public List<OrderTrackingEventDto> TrackingEvents { get; set; } = new();
    }

    /// <summary>
    /// Order return DTO
    /// </summary>
    public class OrderReturnDto
    {
        public int Id { get; set; }
        public ReturnStatus Status { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public List<ReturnItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Return item DTO
    /// </summary>
    public class ReturnItemDto
    {
        public int OrderItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal RefundAmount { get; set; }
        public string? Condition { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Order tracking event DTO
    /// </summary>
    public class OrderTrackingEventDto
    {
        public string Event { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Location { get; set; }
        public string? Carrier { get; set; }
    }
}