using System.ComponentModel.DataAnnotations;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.DTOs.Common;

namespace SakuraHomeAPI.DTOs.Products
{
    /// <summary>
    /// Product search and filter request
    /// </summary>
    public class ProductFilterRequestDto : PaginationRequestDto
    {
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "MinPrice must be greater than or equal to 0")]
        public decimal? MinPrice { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "MaxPrice must be greater than or equal to 0")]
        public decimal? MaxPrice { get; set; }
        
        [MaxLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
        public string? Search { get; set; }
        
        public ProductStatus? Status { get; set; }
        
        [RegularExpression("^(name|price|rating|created|sold|views|stock)$", 
            ErrorMessage = "SortBy must be one of: name, price, rating, created, sold, views, stock")]
        public string SortBy { get; set; } = "created";
        
        [RegularExpression("^(asc|desc)$", ErrorMessage = "SortOrder must be either 'asc' or 'desc'")]
        public string SortOrder { get; set; } = "desc";
        
        public bool? IsFeatured { get; set; }
        public bool? IsNew { get; set; }
        public bool? IsBestseller { get; set; }
        public bool? InStock { get; set; }
        public bool? OnSale { get; set; }
        public JapaneseOrigin? JapaneseRegion { get; set; }
        public AuthenticityLevel? AuthenticityLevel { get; set; }
        public AgeRestriction? AgeRestriction { get; set; }
        
        public List<int>? TagIds { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "MinWeight must be greater than or equal to 0")]
        public decimal? MinWeight { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "MaxWeight must be greater than or equal to 0")]
        public decimal? MaxWeight { get; set; }
        
        public WeightUnit? WeightUnit { get; set; }

        // Custom validation for price range
        public bool IsValidPriceRange()
        {
            return !MinPrice.HasValue || !MaxPrice.HasValue || MinPrice <= MaxPrice;
        }

        // Custom validation for weight range
        public bool IsValidWeightRange()
        {
            return !MinWeight.HasValue || !MaxWeight.HasValue || MinWeight <= MaxWeight;
        }
    }

    /// <summary>
    /// Product filter result information
    /// </summary>
    public class ProductFilterInfoDto
    {
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Search { get; set; }
        public ProductStatus? Status { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsNew { get; set; }
        public bool? IsBestseller { get; set; }
        public bool? InStock { get; set; }
        public bool? OnSale { get; set; }
        public JapaneseOrigin? JapaneseRegion { get; set; }
        public AuthenticityLevel? AuthenticityLevel { get; set; }
        public string SortBy { get; set; } = "created";
        public string SortOrder { get; set; } = "desc";
        
        // Applied filter statistics
        public int TotalFiltersApplied { get; set; }
        public decimal? ActualMinPrice { get; set; }
        public decimal? ActualMaxPrice { get; set; }
    }

    /// <summary>
    /// Product list response with filters
    /// </summary>
    public class ProductListResponseDto : PagedResponseDto<ProductSummaryDto>
    {
        public ProductFilterInfoDto Filters { get; set; } = new();
        public ProductAggregateInfoDto Aggregates { get; set; } = new();
    }

    /// <summary>
    /// Product aggregate information for filtering
    /// </summary>
    public class ProductAggregateInfoDto
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal AvgPrice { get; set; }
        public int TotalProducts { get; set; }
        public int InStockProducts { get; set; }
        public int OnSaleProducts { get; set; }
        public int FeaturedProducts { get; set; }
        public double AvgRating { get; set; }
        public List<BrandCountDto> TopBrands { get; set; } = new();
        public List<CategoryCountDto> TopCategories { get; set; } = new();
    }

    /// <summary>
    /// Brand count for aggregates
    /// </summary>
    public class BrandCountDto
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }

    /// <summary>
    /// Category count for aggregates
    /// </summary>
    public class CategoryCountDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }

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
    /// Update product request
    /// </summary>
    public class UpdateProductRequestDto : CreateProductRequestDto
    {
        // Inherits all properties from CreateProductRequestDto
        // Can add specific update-only properties here if needed
        public string? UpdateReason { get; set; }
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

    /// <summary>
    /// Update product stock request
    /// </summary>
    public class UpdateStockRequestDto
    {
        [Required(ErrorMessage = "New stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "New stock must be greater than or equal to 0")]
        public int NewStock { get; set; }

        [Required(ErrorMessage = "Inventory action is required")]
        public InventoryAction Action { get; set; }

        [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? Reason { get; set; }

        [MaxLength(50, ErrorMessage = "Batch number cannot exceed 50 characters")]
        public string? BatchNumber { get; set; }

        [MaxLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
        public string? Location { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit cost must be greater than or equal to 0")]
        public decimal? UnitCost { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit price must be greater than or equal to 0")]
        public decimal? UnitPrice { get; set; }
    }
}