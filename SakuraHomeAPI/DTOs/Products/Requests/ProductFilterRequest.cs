using System.ComponentModel.DataAnnotations;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Products.Requests
{
    /// <summary>
    /// Product search and filter request
    /// </summary>
    public class ProductFilterRequestDto : PaginationRequestDto
    {
        // Basic search and filtering
        [MaxLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
        public string? Search { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Category ID must be greater than 0")]
        public int? CategoryId { get; set; }

        public bool IncludeSubcategories { get; set; } = false;

        [Range(1, int.MaxValue, ErrorMessage = "Brand ID must be greater than 0")]
        public int? BrandId { get; set; }

        // Price filtering
        [Range(0, double.MaxValue, ErrorMessage = "Min price must be greater than or equal to 0")]
        public decimal? MinPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Max price must be greater than or equal to 0")]
        public decimal? MaxPrice { get; set; }

        // Rating filtering
        [Range(0, 5, ErrorMessage = "Min rating must be between 0 and 5")]
        public decimal? MinRating { get; set; }

        // Tag filtering
        public List<int>? TagIds { get; set; }

        // Status and condition filters
        public ProductStatus? Status { get; set; }
        public ProductCondition? Condition { get; set; }

        // Boolean filters
        public bool? InStockOnly { get; set; } = false;
        public bool? OnSaleOnly { get; set; } = false;
        public bool? FeaturedOnly { get; set; } = false;
        public bool? NewOnly { get; set; } = false;
        public bool? IsFeatured { get; set; }
        public bool? IsNew { get; set; }
        public bool? IsBestseller { get; set; }
        public bool? IsLimitedEdition { get; set; }
        public bool? IsGiftWrappingAvailable { get; set; }
        public bool? AllowBackorder { get; set; }
        public bool? AllowPreorder { get; set; }

        // Sorting
        [RegularExpression("^(name|price|rating|created|updated|sold|views|stock)$", 
            ErrorMessage = "SortBy must be one of: name, price, rating, created, updated, sold, views, stock")]
        public string SortBy { get; set; } = "created";

        [RegularExpression("^(asc|desc)$", ErrorMessage = "SortOrder must be either 'asc' or 'desc'")]
        public string SortOrder { get; set; } = "desc";

        // Advanced filters
        public List<string>? Attributes { get; set; }
        public string? Origin { get; set; }
        public JapaneseOrigin? JapaneseRegion { get; set; }
        public AuthenticityLevel? AuthenticityLevel { get; set; }
        public AgeRestriction? AgeRestriction { get; set; }

        // Weight filtering
        [Range(0, double.MaxValue, ErrorMessage = "Min weight must be greater than or equal to 0")]
        public decimal? MinWeight { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Max weight must be greater than or equal to 0")]
        public decimal? MaxWeight { get; set; }

        public WeightUnit? WeightUnit { get; set; }

        // Color and size filters (for variants)
        public List<int>? Colors { get; set; }
        public List<int>? Sizes { get; set; }

        // Date filters
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableUntil { get; set; }

        // Special filters
        public bool? HasDiscount { get; set; }
        public bool? IsJapaneseProduct { get; set; }
        public bool? IsAuthentic { get; set; }

        // Custom validation methods
        public bool IsValidPriceRange()
        {
            return !MinPrice.HasValue || !MaxPrice.HasValue || MinPrice <= MaxPrice;
        }

        public bool IsValidWeightRange()
        {
            return !MinWeight.HasValue || !MaxWeight.HasValue || MinWeight <= MaxWeight;
        }

        public bool IsValidDateRange()
        {
            return !CreatedFrom.HasValue || !CreatedTo.HasValue || CreatedFrom <= CreatedTo;
        }

        public bool IsValidAvailabilityRange()
        {
            return !AvailableFrom.HasValue || !AvailableUntil.HasValue || AvailableFrom <= AvailableUntil;
        }
    }
}