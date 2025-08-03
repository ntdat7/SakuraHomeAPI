using SakuraHomeAPI.DTOs.Common;

namespace SakuraHomeAPI.DTOs.Products.Requests
{
    /// <summary>
    /// Product filter request for searching and listing
    /// </summary>
    public class ProductFilterRequestDto : PaginationRequestDto
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public bool IncludeSubcategories { get; set; } = false;
        public int? BrandId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinRating { get; set; }
        public List<int>? TagIds { get; set; }
        public bool InStockOnly { get; set; } = false;
        public bool OnSaleOnly { get; set; } = false;
        public bool FeaturedOnly { get; set; } = false;
        public bool NewOnly { get; set; } = false;
        public string SortBy { get; set; } = "created";
        public string SortOrder { get; set; } = "desc";
        public List<string>? Attributes { get; set; }
        public string? Origin { get; set; }
        public List<int>? JapaneseRegions { get; set; }
        public int? AuthenticityLevel { get; set; }
        public int? Condition { get; set; }
        public int? AgeRestriction { get; set; }
        public decimal? MinWeight { get; set; }
        public decimal? MaxWeight { get; set; }
        public List<int>? Colors { get; set; }
        public List<int>? Sizes { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableUntil { get; set; }
        public bool? HasDiscount { get; set; }
        public bool? IsLimitedEdition { get; set; }
        public bool? AllowBackorder { get; set; }
        public bool? AllowPreorder { get; set; }
    }
}