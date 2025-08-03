using System.ComponentModel.DataAnnotations;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.DTOs.Common;

namespace SakuraHomeAPI.DTOs.Products.Requests
{
    /// <summary>
    /// Create product request
    /// </summary>
    public class CreateProductRequestDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(500, ErrorMessage = "Product name cannot exceed 500 characters")]
        [MinLength(2, ErrorMessage = "Product name must be at least 2 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU is required")]
        [MaxLength(100, ErrorMessage = "SKU cannot exceed 100 characters")]
        [RegularExpression(@"^[A-Z0-9\-_]+$", ErrorMessage = "SKU can only contain uppercase letters, numbers, hyphens, and underscores")]
        public string SKU { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Short description cannot exceed 1000 characters")]
        public string? ShortDescription { get; set; }

        [MaxLength(10000, ErrorMessage = "Description cannot exceed 10000 characters")]
        public string? Description { get; set; }

        [MaxLength(500, ErrorMessage = "Main image URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Main image must be a valid URL")]
        public string? MainImage { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, 999999999, ErrorMessage = "Price must be between 0 and 999,999,999")]
        public decimal Price { get; set; }

        [Range(0, 999999999, ErrorMessage = "Original price must be between 0 and 999,999,999")]
        public decimal? OriginalPrice { get; set; }

        [Range(0, 999999999, ErrorMessage = "Cost price must be between 0 and 999,999,999")]
        public decimal? CostPrice { get; set; }

        [Required(ErrorMessage = "Stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be greater than or equal to 0")]
        public int Stock { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Min stock must be greater than or equal to 0")]
        public int? MinStock { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Max stock must be greater than or equal to 0")]
        public int? MaxStock { get; set; }

        public bool TrackInventory { get; set; } = true;
        public bool AllowBackorder { get; set; } = false;
        public bool AllowPreorder { get; set; } = false;

        [Required(ErrorMessage = "Brand is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Brand ID must be greater than 0")]
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Category ID must be greater than 0")]
        public int CategoryId { get; set; }

        [MaxLength(100, ErrorMessage = "Origin cannot exceed 100 characters")]
        public string? Origin { get; set; }

        public JapaneseOrigin? JapaneseRegion { get; set; }
        public AuthenticityLevel AuthenticityLevel { get; set; } = AuthenticityLevel.Verified;

        [MaxLength(1000, ErrorMessage = "Authenticity info cannot exceed 1000 characters")]
        public string? AuthenticityInfo { get; set; }

        [MaxLength(2000, ErrorMessage = "Usage guide cannot exceed 2000 characters")]
        public string? UsageGuide { get; set; }

        [MaxLength(1000, ErrorMessage = "Ingredients cannot exceed 1000 characters")]
        public string? Ingredients { get; set; }

        public DateTime? ExpiryDate { get; set; }
        public DateTime? ManufactureDate { get; set; }

        [MaxLength(50, ErrorMessage = "Batch number cannot exceed 50 characters")]
        public string? BatchNumber { get; set; }

        public AgeRestriction AgeRestriction { get; set; } = AgeRestriction.None;

        [Range(0, double.MaxValue, ErrorMessage = "Weight must be greater than or equal to 0")]
        public decimal? Weight { get; set; }

        public WeightUnit WeightUnit { get; set; } = WeightUnit.Gram;

        [MaxLength(50, ErrorMessage = "Dimensions cannot exceed 50 characters")]
        public string? Dimensions { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Length must be greater than or equal to 0")]
        public decimal? Length { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Width must be greater than or equal to 0")]
        public decimal? Width { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Height must be greater than or equal to 0")]
        public decimal? Height { get; set; }

        public DimensionUnit DimensionUnit { get; set; } = DimensionUnit.Centimeter;
        public ProductStatus Status { get; set; } = ProductStatus.Active;
        public ProductCondition Condition { get; set; } = ProductCondition.New;
        public ProductVisibility Visibility { get; set; } = ProductVisibility.Public;
        public bool IsFeatured { get; set; } = false;
        public bool IsNew { get; set; } = false;
        public bool IsBestseller { get; set; } = false;
        public bool IsLimitedEdition { get; set; } = false;
        public bool IsGiftWrappingAvailable { get; set; } = false;

        [Range(0, double.MaxValue, ErrorMessage = "Gift wrapping fee must be greater than or equal to 0")]
        public decimal? GiftWrappingFee { get; set; }

        [MaxLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
        public string? Tags { get; set; }

        [MaxLength(1000, ErrorMessage = "Marketing description cannot exceed 1000 characters")]
        public string? MarketingDescription { get; set; }

        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableUntil { get; set; }

        // Images to upload
        public List<CreateProductImageRequestDto>? Images { get; set; }

        // Variants to create
        public List<CreateProductVariantRequestDto>? Variants { get; set; }

        // Attributes to set
        public List<SetProductAttributeRequestDto>? Attributes { get; set; }

        // Tags to assign
        public List<int>? TagIds { get; set; }

        // Custom validation methods
        public bool IsValidPriceRange()
        {
            return !OriginalPrice.HasValue || OriginalPrice >= Price;
        }

        public bool IsValidStockRange()
        {
            return !MinStock.HasValue || !MaxStock.HasValue || MinStock <= MaxStock;
        }

        public bool IsValidDateRange()
        {
            return !AvailableFrom.HasValue || !AvailableUntil.HasValue || AvailableFrom <= AvailableUntil;
        }

        public bool IsValidManufactureDateRange()
        {
            return !ManufactureDate.HasValue || !ExpiryDate.HasValue || ManufactureDate <= ExpiryDate;
        }
    }

    /// <summary>
    /// Create product image request
    /// </summary>
    public class CreateProductImageRequestDto
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
    /// Create product variant request
    /// </summary>
    public class CreateProductVariantRequestDto
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
    /// Set product attribute request
    /// </summary>
    public class SetProductAttributeRequestDto
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