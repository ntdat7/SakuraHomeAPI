using System.ComponentModel.DataAnnotations;
using SakuraHomeAPI.Models.Enums;

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

    /// <summary>
    /// Product attribute information
    /// </summary>
    public class ProductAttributeDto
    {
        public int AttributeId { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? DisplayValue { get; set; }
        public AttributeType Type { get; set; }
        public string? Unit { get; set; }
    }

    /// <summary>
    /// Product statistics information
    /// </summary>
    public class ProductStatisticsDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int SoldCount { get; set; }
        public int ReviewCount { get; set; }
        public decimal AverageRating { get; set; }
        public int WishlistCount { get; set; }
        public int CartCount { get; set; }
        public decimal Revenue { get; set; }
        public DateTime LastSold { get; set; }
        public DateTime LastViewed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Create product image request DTO
    /// </summary>
    public class CreateProductImageDto
    {
        [Required(ErrorMessage = "Image URL is required")]
        [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Image URL must be valid")]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Alt text cannot exceed 255 characters")]
        public string? AltText { get; set; }

        [MaxLength(500, ErrorMessage = "Caption cannot exceed 500 characters")]
        public string? Caption { get; set; }

        [Range(0, 999, ErrorMessage = "Display order must be between 0 and 999")]
        public int DisplayOrder { get; set; } = 0;

        public bool IsMain { get; set; } = false;
    }

    /// <summary>
    /// Update product image request DTO
    /// </summary>
    public class UpdateProductImageDto
    {
        [MaxLength(255, ErrorMessage = "Alt text cannot exceed 255 characters")]
        public string? AltText { get; set; }

        [MaxLength(500, ErrorMessage = "Caption cannot exceed 500 characters")]
        public string? Caption { get; set; }

        [Range(0, 999, ErrorMessage = "Display order must be between 0 and 999")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Create product variant request DTO
    /// </summary>
    public class CreateProductVariantDto
    {
        [Required(ErrorMessage = "Variant name is required")]
        [MaxLength(255, ErrorMessage = "Variant name cannot exceed 255 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Variant SKU is required")]
        [MaxLength(100, ErrorMessage = "Variant SKU cannot exceed 100 characters")]
        [RegularExpression(@"^[A-Z0-9\-_]+$", ErrorMessage = "Variant SKU can only contain uppercase letters, numbers, hyphens, and underscores")]
        public string SKU { get; set; } = string.Empty;

        [Range(0, 999999999, ErrorMessage = "Variant price must be between 0 and 999,999,999")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Variant stock must be greater than or equal to 0")]
        public int Stock { get; set; }

        [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Image URL must be valid")]
        public string? ImageUrl { get; set; }

        public Dictionary<string, string> Attributes { get; set; } = new();
    }

    /// <summary>
    /// Update product variant request DTO
    /// </summary>
    public class UpdateProductVariantDto
    {
        [MaxLength(255, ErrorMessage = "Variant name cannot exceed 255 characters")]
        public string? Name { get; set; }

        [MaxLength(100, ErrorMessage = "Variant SKU cannot exceed 100 characters")]
        [RegularExpression(@"^[A-Z0-9\-_]+$", ErrorMessage = "Variant SKU can only contain uppercase letters, numbers, hyphens, and underscores")]
        public string? SKU { get; set; }

        [Range(0, 999999999, ErrorMessage = "Variant price must be between 0 and 999,999,999")]
        public decimal? Price { get; set; }

        [Range(0, 999999999, ErrorMessage = "Original price must be between 0 and 999,999,999")]
        public decimal? OriginalPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Variant stock must be greater than or equal to 0")]
        public int? Stock { get; set; }

        [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Image URL must be valid")]
        public string? ImageUrl { get; set; }

        public Dictionary<string, string>? Attributes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Create product attribute request DTO
    /// </summary>
    public class CreateProductAttributeDto
    {
        [Required(ErrorMessage = "Attribute ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Attribute ID must be greater than 0")]
        public int AttributeId { get; set; }

        [Required(ErrorMessage = "Attribute value is required")]
        [MaxLength(1000, ErrorMessage = "Attribute value cannot exceed 1000 characters")]
        public string Value { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Display value cannot exceed 255 characters")]
        public string? DisplayValue { get; set; }
    }
}