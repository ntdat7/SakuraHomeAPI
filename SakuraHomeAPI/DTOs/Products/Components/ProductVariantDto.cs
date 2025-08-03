namespace SakuraHomeAPI.DTOs.Products.Components
{
    /// <summary>
    /// Product variant information
    /// </summary>
    public class ProductVariantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}