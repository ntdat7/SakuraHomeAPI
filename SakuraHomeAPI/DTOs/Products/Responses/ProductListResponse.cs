using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Products.Responses
{
    /// <summary>
    /// Product list response with pagination and filters
    /// </summary>
    public class ProductListResponseDto : PagedResponseDto<ProductSummaryDto>
    {
        public ProductFilterInfoDto Filters { get; set; } = new();
        public ProductAggregateInfoDto Aggregates { get; set; } = new();
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
        public AgeRestriction? AgeRestriction { get; set; }
        public string SortBy { get; set; } = "created";
        public string SortOrder { get; set; } = "desc";
        public List<int>? TagIds { get; set; }
        
        // Applied filter statistics
        public int TotalFiltersApplied { get; set; }
        public decimal? ActualMinPrice { get; set; }
        public decimal? ActualMaxPrice { get; set; }
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
        public int OutOfStockProducts { get; set; }
        public int OnSaleProducts { get; set; }
        public int FeaturedProducts { get; set; }
        public int NewProducts { get; set; }
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
    /// Product statistics for listing
    /// </summary>
    public class ProductStatsDto
    {
        public int TotalProducts { get; set; }
        public int InStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int FeaturedProducts { get; set; }
        public int NewProducts { get; set; }
        public int OnSaleProducts { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal AverageRating { get; set; }
    }
}