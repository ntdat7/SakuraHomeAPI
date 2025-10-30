using SakuraHomeAPI.DTOs.Products.Requests;
using SakuraHomeAPI.DTOs.Products.Responses;
using SakuraHomeAPI.Services.Common;

namespace SakuraHomeAPI.Services.Interfaces
{
    /// <summary>
    /// Enhanced product search service interface with advanced search capabilities
    /// </summary>
    public interface IProductSearchService
    {
        /// <summary>
        /// Advanced product search with comprehensive filtering and caching
        /// </summary>
        Task<ServiceResult<ProductListResponseDto>> AdvancedSearchAsync(
            ProductFilterRequestDto filter, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get search suggestions based on partial input
        /// </summary>
        Task<ServiceResult<List<string>>> GetSearchSuggestionsAsync(
            string partialInput, 
            int maxSuggestions = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get popular search terms
        /// </summary>
        Task<ServiceResult<List<string>>> GetPopularSearchTermsAsync(
            int count = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get search facets (filters with counts)
        /// </summary>
        Task<ServiceResult<SearchFacetsDto>> GetSearchFacetsAsync(
            ProductFilterRequestDto baseFilter,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Track search query for analytics
        /// </summary>
        Task TrackSearchQueryAsync(
            string query, 
            int resultCount, 
            Guid? userId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get trending search terms
        /// </summary>
        Task<ServiceResult<List<TrendingSearchDto>>> GetTrendingSearchesAsync(
            int days = 7,
            int count = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Clear search cache
        /// </summary>
        Task ClearSearchCacheAsync(string? pattern = null);
    }
}

namespace SakuraHomeAPI.DTOs.Products.Responses
{
    /// <summary>
    /// Search facets with filter counts
    /// </summary>
    public class SearchFacetsDto
    {
        public List<FacetGroupDto> Categories { get; set; } = new();
        public List<FacetGroupDto> Brands { get; set; } = new();
        public List<FacetGroupDto> Tags { get; set; } = new();
        public List<PriceRangeDto> PriceRanges { get; set; } = new();
        public List<FacetGroupDto> Origins { get; set; } = new();
        public List<FacetGroupDto> Conditions { get; set; } = new();
        public RatingFacetsDto Ratings { get; set; } = new();
        public StockFacetsDto Stock { get; set; } = new();
    }

    /// <summary>
    /// Facet group with items and counts
    /// </summary>
    public class FacetGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }
        public int Count { get; set; }
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// Price range facet
    /// </summary>
    public class PriceRangeDto
    {
        public string Label { get; set; } = string.Empty;
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int Count { get; set; }
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// Rating facets
    /// </summary>
    public class RatingFacetsDto
    {
        public int FiveStars { get; set; }
        public int FourStarsAndUp { get; set; }
        public int ThreeStarsAndUp { get; set; }
        public int TwoStarsAndUp { get; set; }
        public int OneStarAndUp { get; set; }
    }

    /// <summary>
    /// Stock status facets
    /// </summary>
    public class StockFacetsDto
    {
        public int InStock { get; set; }
        public int LowStock { get; set; }
        public int OutOfStock { get; set; }
        public int AllowBackorder { get; set; }
    }

    /// <summary>
    /// Trending search term
    /// </summary>
    public class TrendingSearchDto
    {
        public string Query { get; set; } = string.Empty;
        public int SearchCount { get; set; }
        public int ResultCount { get; set; }
        public DateTime LastSearched { get; set; }
        public decimal TrendScore { get; set; }
    }
}