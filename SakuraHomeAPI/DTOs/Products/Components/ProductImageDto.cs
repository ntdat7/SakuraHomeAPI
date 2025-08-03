namespace SakuraHomeAPI.DTOs.Products.Components
{
    /// <summary>
    /// Product image information
    /// </summary>
    public class ProductImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public string? Caption { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsMain { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}