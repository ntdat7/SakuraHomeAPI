using System.ComponentModel.DataAnnotations;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Products.Requests
{
    /// <summary>
    /// Product search and filter request with comprehensive filtering capabilities
    /// </summary>
    public class ProductFilterRequestDto : PaginationRequestDto
    {
        #region Basic Search and Filtering

        /// <summary>
        /// Search term for name, description, SKU, brand name, category name
        /// </summary>
        [MaxLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
        public string? Search { get; set; }

        /// <summary>
        /// Category ID filter
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Category ID must be greater than 0")]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Include subcategories in category filter
        /// </summary>
        public bool IncludeSubcategories { get; set; } = false;

        /// <summary>
        /// Brand ID filter
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Brand ID must be greater than 0")]
        public int? BrandId { get; set; }

        #endregion

        #region Price and Rating Filters

        /// <summary>
        /// Minimum price filter
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Min price must be greater than or equal to 0")]
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Maximum price filter
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Max price must be greater than or equal to 0")]
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Minimum rating filter (0-5)
        /// </summary>
        [Range(0, 5, ErrorMessage = "Min rating must be between 0 and 5")]
        public decimal? MinRating { get; set; }

        #endregion

        #region Tag and Text-based Filters

        /// <summary>
        /// Filter by tag IDs
        /// </summary>
        public List<int>? TagIds { get; set; }

        /// <summary>
        /// Filter by tag names (comma-separated or individual strings)
        /// </summary>
        public List<string>? TagNames { get; set; }

        /// <summary>
        /// Tag matching mode: All (AND) or Any (OR)
        /// </summary>
        public TagMatchMode TagMatchMode { get; set; } = TagMatchMode.Any;

        /// <summary>
        /// Search in Tags field (for string-based tags in Tags column)
        /// </summary>
        public string? TagsSearch { get; set; }

        #endregion

        #region Status and Condition Filters

        /// <summary>
        /// Product status filter
        /// </summary>
        public ProductStatus? Status { get; set; }

        /// <summary>
        /// Product condition filter
        /// </summary>
        public ProductCondition? Condition { get; set; }

        #endregion

        #region Boolean Filters

        /// <summary>
        /// Show only products in stock
        /// </summary>
        public bool? InStockOnly { get; set; }

        /// <summary>
        /// Show only products on sale (with original price > current price)
        /// </summary>
        public bool? OnSaleOnly { get; set; }

        /// <summary>
        /// Show only featured products
        /// </summary>
        public bool? FeaturedOnly { get; set; }

        /// <summary>
        /// Show only new products
        /// </summary>
        public bool? NewOnly { get; set; }

        /// <summary>
        /// Featured products filter
        /// </summary>
        public bool? IsFeatured { get; set; }

        /// <summary>
        /// New products filter
        /// </summary>
        public bool? IsNew { get; set; }

        /// <summary>
        /// Bestseller products filter
        /// </summary>
        public bool? IsBestseller { get; set; }

        /// <summary>
        /// Limited edition products filter
        /// </summary>
        public bool? IsLimitedEdition { get; set; }

        /// <summary>
        /// Gift wrapping available filter
        /// </summary>
        public bool? IsGiftWrappingAvailable { get; set; }

        /// <summary>
        /// Allow backorder filter
        /// </summary>
        public bool? AllowBackorder { get; set; }

        /// <summary>
        /// Allow preorder filter
        /// </summary>
        public bool? AllowPreorder { get; set; }

        /// <summary>
        /// Show only products with discount
        /// </summary>
        public bool? HasDiscount { get; set; }

        /// <summary>
        /// Japanese product filter
        /// </summary>
        public bool? IsJapaneseProduct { get; set; }

        /// <summary>
        /// Authentic product filter
        /// </summary>
        public bool? IsAuthentic { get; set; }

        #endregion

        #region Sorting

        /// <summary>
        /// Sort field
        /// </summary>
        [RegularExpression("^(name|price|rating|created|updated|sold|views|stock|relevance|popularity|discount)$", 
            ErrorMessage = "SortBy must be one of: name, price, rating, created, updated, sold, views, stock, relevance, popularity, discount")]
        public string SortBy { get; set; } = "created";

        /// <summary>
        /// Sort order
        /// </summary>
        [RegularExpression("^(asc|desc)$", ErrorMessage = "SortOrder must be either 'asc' or 'desc'")]
        public string SortOrder { get; set; } = "desc";

        #endregion

        #region Advanced Filters

        /// <summary>
        /// Product attributes (key:value pairs)
        /// </summary>
        public Dictionary<string, string>? Attributes { get; set; }

        /// <summary>
        /// Product origin filter
        /// </summary>
        [MaxLength(100, ErrorMessage = "Origin cannot exceed 100 characters")]
        public string? Origin { get; set; }

        /// <summary>
        /// Japanese region filter
        /// </summary>
        public JapaneseOrigin? JapaneseRegion { get; set; }

        /// <summary>
        /// Authenticity level filter
        /// </summary>
        public AuthenticityLevel? AuthenticityLevel { get; set; }

        /// <summary>
        /// Age restriction filter
        /// </summary>
        public AgeRestriction? AgeRestriction { get; set; }

        #endregion

        #region Weight and Dimension Filters

        /// <summary>
        /// Minimum weight filter
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Min weight must be greater than or equal to 0")]
        public decimal? MinWeight { get; set; }

        /// <summary>
        /// Maximum weight filter
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Max weight must be greater than or equal to 0")]
        public decimal? MaxWeight { get; set; }

        /// <summary>
        /// Weight unit filter
        /// </summary>
        public WeightUnit? WeightUnit { get; set; }

        #endregion

        #region Variant Filters

        /// <summary>
        /// Color filters (for variants)
        /// </summary>
        public List<int>? Colors { get; set; }

        /// <summary>
        /// Size filters (for variants)
        /// </summary>
        public List<int>? Sizes { get; set; }

        #endregion

        #region Date Filters

        /// <summary>
        /// Created from date filter
        /// </summary>
        public DateTime? CreatedFrom { get; set; }

        /// <summary>
        /// Created to date filter
        /// </summary>
        public DateTime? CreatedTo { get; set; }

        /// <summary>
        /// Available from date filter
        /// </summary>
        public DateTime? AvailableFrom { get; set; }

        /// <summary>
        /// Available until date filter
        /// </summary>
        public DateTime? AvailableUntil { get; set; }

        #endregion

        #region Stock Filters

        /// <summary>
        /// Minimum stock quantity
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Min stock must be greater than or equal to 0")]
        public int? MinStock { get; set; }

        /// <summary>
        /// Maximum stock quantity
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Max stock must be greater than or equal to 0")]
        public int? MaxStock { get; set; }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validate price range
        /// </summary>
        public bool IsValidPriceRange()
        {
            return !MinPrice.HasValue || !MaxPrice.HasValue || MinPrice <= MaxPrice;
        }

        /// <summary>
        /// Validate weight range
        /// </summary>
        public bool IsValidWeightRange()
        {
            return !MinWeight.HasValue || !MaxWeight.HasValue || MinWeight <= MaxWeight;
        }

        /// <summary>
        /// Validate date range for creation
        /// </summary>
        public bool IsValidDateRange()
        {
            return !CreatedFrom.HasValue || !CreatedTo.HasValue || CreatedFrom <= CreatedTo;
        }

        /// <summary>
        /// Validate availability date range
        /// </summary>
        public bool IsValidAvailabilityRange()
        {
            return !AvailableFrom.HasValue || !AvailableUntil.HasValue || AvailableFrom <= AvailableUntil;
        }

        /// <summary>
        /// Validate stock range
        /// </summary>
        public bool IsValidStockRange()
        {
            return !MinStock.HasValue || !MaxStock.HasValue || MinStock <= MaxStock;
        }

        /// <summary>
        /// Check if any filter is applied
        /// </summary>
        public bool HasFilters()
        {
            return !string.IsNullOrEmpty(Search) ||
                   CategoryId.HasValue ||
                   BrandId.HasValue ||
                   MinPrice.HasValue ||
                   MaxPrice.HasValue ||
                   MinRating.HasValue ||
                   TagIds?.Any() == true ||
                   TagNames?.Any() == true ||
                   !string.IsNullOrEmpty(TagsSearch) ||
                   Status.HasValue ||
                   Condition.HasValue ||
                   InStockOnly.HasValue ||
                   OnSaleOnly.HasValue ||
                   FeaturedOnly.HasValue ||
                   NewOnly.HasValue ||
                   IsFeatured.HasValue ||
                   IsNew.HasValue ||
                   IsBestseller.HasValue ||
                   IsLimitedEdition.HasValue ||
                   IsGiftWrappingAvailable.HasValue ||
                   AllowBackorder.HasValue ||
                   AllowPreorder.HasValue ||
                   HasDiscount.HasValue ||
                   IsJapaneseProduct.HasValue ||
                   IsAuthentic.HasValue ||
                   Attributes?.Any() == true ||
                   !string.IsNullOrEmpty(Origin) ||
                   JapaneseRegion.HasValue ||
                   AuthenticityLevel.HasValue ||
                   AgeRestriction.HasValue ||
                   MinWeight.HasValue ||
                   MaxWeight.HasValue ||
                   Colors?.Any() == true ||
                   Sizes?.Any() == true ||
                   CreatedFrom.HasValue ||
                   CreatedTo.HasValue ||
                   AvailableFrom.HasValue ||
                   AvailableUntil.HasValue ||
                   MinStock.HasValue ||
                   MaxStock.HasValue;
        }

        /// <summary>
        /// Get search terms as list for advanced processing
        /// </summary>
        public List<string> GetSearchTerms()
        {
            if (string.IsNullOrWhiteSpace(Search))
                return new List<string>();

            return Search.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim().ToLower())
                        .Where(t => t.Length > 1) // Ignore single character terms
                        .Distinct()
                        .ToList();
        }

        #endregion
    }

    /// <summary>
    /// Tag matching mode for filtering
    /// </summary>
    public enum TagMatchMode
    {
        /// <summary>
        /// Match any of the specified tags (OR)
        /// </summary>
        Any = 0,
        
        /// <summary>
        /// Match all of the specified tags (AND)
        /// </summary>
        All = 1
    }
}