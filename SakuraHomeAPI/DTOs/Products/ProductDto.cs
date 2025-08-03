using System.ComponentModel.DataAnnotations;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.DTOs.Common;

namespace SakuraHomeAPI.DTOs.Products
{
    /// <summary>
    /// Product summary for list views
    /// </summary>
    public class ProductSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? MainImage { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public int Stock { get; set; }
        public ProductStatus Status { get; set; }
        public ProductCondition Condition { get; set; }
        public bool IsInStock { get; set; }
        public bool IsOnSale { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsNew { get; set; }
        public bool IsBestseller { get; set; }
        public bool IsLimitedEdition { get; set; }
        public bool IsGiftWrappingAvailable { get; set; }
        public bool AllowBackorder { get; set; }
        public bool AllowPreorder { get; set; }
        public int ViewCount { get; set; }
        public int SoldCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Related entities - using DTOs from Common namespace
        public BrandSummaryDto? Brand { get; set; }
        public CategorySummaryDto? Category { get; set; }
        public List<ProductImageDto> Images { get; set; } = new();
    }

    /// <summary>
    /// Detailed product information
    /// </summary>
    public class ProductDetailDto : ProductSummaryDto
    {
        public string? Description { get; set; }
        public decimal? CostPrice { get; set; }
        public int? MinStock { get; set; }
        public bool TrackInventory { get; set; }
        public decimal? Weight { get; set; }
        public WeightUnit WeightUnit { get; set; }
        public string? Dimensions { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public DimensionUnit DimensionUnit { get; set; }
        public ProductVisibility Visibility { get; set; }
        public int WishlistCount { get; set; }
        public string? Origin { get; set; }
        public JapaneseOrigin? JapaneseRegion { get; set; }
        public AuthenticityLevel AuthenticityLevel { get; set; }
        public string? AuthenticityInfo { get; set; }
        public string? UsageGuide { get; set; }
        public string? Ingredients { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public string? BatchNumber { get; set; }
        public AgeRestriction AgeRestriction { get; set; }
        public decimal? GiftWrappingFee { get; set; }
        public string? Tags { get; set; }
        public string? MarketingDescription { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableUntil { get; set; }
        public bool IsAvailable { get; set; }
        
        // Override with detailed versions - using DTOs from Common namespace
        public new BrandDetailDto? Brand { get; set; }
        public new CategoryDetailDto? Category { get; set; }
        public List<ProductVariantDto> Variants { get; set; } = new();
        public List<ProductAttributeDto> Attributes { get; set; } = new();
        public List<TagDto> ProductTags { get; set; } = new();
        public List<ReviewSummaryDto> Reviews { get; set; } = new();
    }

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
}