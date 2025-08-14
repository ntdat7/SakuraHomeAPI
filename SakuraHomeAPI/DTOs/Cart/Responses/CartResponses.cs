namespace SakuraHomeAPI.DTOs.Cart.Responses
{
    public class CartResponseDto
    {
        public int Id { get; set; }
        public Guid? UserId { get; set; }
        public string? SessionId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int UniqueItems { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public string? CouponCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsEmpty { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
    }

    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public string ProductName { get; set; }
        public string? ProductSku { get; set; }
        public string? VariantName { get; set; }
        public string ProductImage { get; set; }
        public string ProductSlug { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsInStock { get; set; }
        public int AvailableStock { get; set; }
        public bool IsOnSale { get; set; }
        public string? CustomOptions { get; set; }
        public string? GiftMessage { get; set; }
        public bool IsGift { get; set; }
        public bool IsAvailable { get; set; }
        public string? Error { get; set; }
    }

    public class CartSummaryDto
    {
        public int TotalItems { get; set; }
        public int UniqueItems { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public bool IsEmpty { get; set; }
        public bool HasErrors { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}