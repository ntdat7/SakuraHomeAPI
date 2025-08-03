using SakuraHomeAPI.DTOs.Common;

namespace SakuraHomeAPI.DTOs.Products.Responses
{
    /// <summary>
    /// Product list response with pagination
    /// </summary>
    public class ProductListResponseDto : PagedResponseDto<ProductSummaryDto>
    {
        public ProductFilterInfoDto? Filter { get; set; }
        public ProductStatsDto? Stats { get; set; }
    }

    /// <summary>
    /// Product filter information for response
    /// </summary>
    public class ProductFilterInfoDto
    {
        public string? Search { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public List<int>? TagIds { get; set; }
        public string SortBy { get; set; } = "created";
        public string SortOrder { get; set; } = "desc";
        public bool AppliedFilters { get; set; }
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