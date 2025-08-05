namespace SakuraHomeAPI.DTOs.Wishlist.Responses
{
    public class WishlistResponseDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public bool IsDefault { get; set; }
        public string? ShareToken { get; set; }
        public List<WishlistItemDto> Items { get; set; } = new();
        public int ItemCount { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class WishlistSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public bool IsDefault { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class WishlistItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public string ProductName { get; set; }
        public string? ProductSku { get; set; }
        public string? VariantName { get; set; }
        public string ProductImage { get; set; }
        public string ProductSlug { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public bool IsOnSale { get; set; }
        public bool IsInStock { get; set; }
        public bool IsAvailable { get; set; }
        public string? Notes { get; set; }
        public DateTime AddedAt { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
    }
}