using Microsoft.Extensions.Caching.Memory;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.DTOs.Products.Requests;
using SakuraHomeAPI.DTOs.Products.Responses;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Repositories.Interfaces;
using SakuraHomeAPI.Services.Common;
using SakuraHomeAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace SakuraHomeAPI.Services.Implementations
{
    /// <summary>
    /// Enhanced product search service with advanced search capabilities, caching, and analytics
    /// </summary>
    public class ProductSearchService : IProductSearchService
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ProductSearchService> _logger;

        // Cache settings
        private readonly TimeSpan _searchCacheDuration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _facetsCacheDuration = TimeSpan.FromMinutes(10);
        private readonly TimeSpan _suggestionsCacheDuration = TimeSpan.FromMinutes(15);

        public ProductSearchService(
            IProductRepository productRepository,
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<ProductSearchService> logger)
        {
            _productRepository = productRepository;
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ServiceResult<ProductListResponseDto>> AdvancedSearchAsync(
            ProductFilterRequestDto filter, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Generate cache key for this search
                var cacheKey = GenerateSearchCacheKey(filter);
                
                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out ProductListResponseDto? cachedResult) && cachedResult != null)
                {
                    _logger.LogInformation("Returning cached search results for key: {CacheKey}", cacheKey);
                    return ServiceResult<ProductListResponseDto>.Success(cachedResult);
                }

                // Perform the search
                var (products, totalCount) = await _productRepository.SearchAsync(filter, cancellationToken);

                // Map to response DTOs
                var productDtos = products.Select(MapToProductSummaryDto).ToList();

                var response = new ProductListResponseDto
                {
                    Data = productDtos,
                    Pagination = new PaginationResponseDto
                    {
                        CurrentPage = filter.Page,
                        PageSize = filter.PageSize,
                        TotalItems = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize),
                        HasNext = filter.Page < (int)Math.Ceiling((double)totalCount / filter.PageSize),
                        HasPrevious = filter.Page > 1
                    },
                    Success = true,
                    Message = null,
                    Filters = new ProductFilterInfoDto
                    {
                        Search = filter.Search,
                        CategoryId = filter.CategoryId,
                        BrandId = filter.BrandId,
                        MinPrice = filter.MinPrice,
                        MaxPrice = filter.MaxPrice,
                        IsFeatured = filter.IsFeatured,
                        IsNew = filter.IsNew,
                        IsBestseller = filter.IsBestseller,
                        InStock = filter.InStockOnly,
                        OnSale = filter.OnSaleOnly,
                        JapaneseRegion = filter.JapaneseRegion,
                        AuthenticityLevel = filter.AuthenticityLevel,
                        AgeRestriction = filter.AgeRestriction,
                        SortBy = filter.SortBy,
                        SortOrder = filter.SortOrder,
                        TagIds = filter.TagIds,
                        TotalFiltersApplied = GetAppliedFiltersCount(filter)
                    }
                };

                // Cache the results
                _cache.Set(cacheKey, response, _searchCacheDuration);

                // Track the search for analytics
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    _ = Task.Run(async () => await TrackSearchQueryAsync(filter.Search, totalCount, null, cancellationToken));
                }

                _logger.LogInformation("Search completed. Query: {Query}, Results: {Count}", filter.Search, totalCount);

                return ServiceResult<ProductListResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing advanced search with filter: {@Filter}", filter);
                return ServiceResult<ProductListResponseDto>.Failure("An error occurred while searching products");
            }
        }

        public async Task<ServiceResult<List<string>>> GetSearchSuggestionsAsync(
            string partialInput, 
            int maxSuggestions = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(partialInput) || partialInput.Length < 2)
                {
                    return ServiceResult<List<string>>.Success(new List<string>());
                }

                var cacheKey = $"search_suggestions_{partialInput.ToLower().Trim()}_{maxSuggestions}";
                
                if (_cache.TryGetValue(cacheKey, out List<string>? cachedSuggestions) && cachedSuggestions != null)
                {
                    return ServiceResult<List<string>>.Success(cachedSuggestions);
                }

                var suggestions = new List<string>();
                var inputLower = partialInput.ToLower().Trim();

                // Get suggestions from product names
                var productNameSuggestions = await _context.Products
                    .Where(p => p.IsActive && !p.IsDeleted && p.Name.ToLower().Contains(inputLower))
                    .Select(p => p.Name)
                    .Distinct()
                    .Take(maxSuggestions / 2)
                    .ToListAsync(cancellationToken);

                suggestions.AddRange(productNameSuggestions);

                // Get suggestions from brand names
                var brandSuggestions = await _context.Brands
                    .Where(b => b.IsActive && !b.IsDeleted && b.Name.ToLower().Contains(inputLower))
                    .Select(b => b.Name)
                    .Distinct()
                    .Take(maxSuggestions / 4)
                    .ToListAsync(cancellationToken);

                suggestions.AddRange(brandSuggestions);

                // Get suggestions from category names
                var categorySuggestions = await _context.Categories
                    .Where(c => c.IsActive && !c.IsDeleted && c.Name.ToLower().Contains(inputLower))
                    .Select(c => c.Name)
                    .Distinct()
                    .Take(maxSuggestions / 4)
                    .ToListAsync(cancellationToken);

                suggestions.AddRange(categorySuggestions);

                // Remove duplicates and trim to max suggestions
                var uniqueSuggestions = suggestions
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(maxSuggestions)
                    .ToList();

                _cache.Set(cacheKey, uniqueSuggestions, _suggestionsCacheDuration);

                return ServiceResult<List<string>>.Success(uniqueSuggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions for input: {Input}", partialInput);
                return ServiceResult<List<string>>.Failure("An error occurred while getting search suggestions");
            }
        }

        public async Task<ServiceResult<List<string>>> GetPopularSearchTermsAsync(
            int count = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = $"popular_search_terms_{count}";
                
                if (_cache.TryGetValue(cacheKey, out List<string>? cachedTerms) && cachedTerms != null)
                {
                    return ServiceResult<List<string>>.Success(cachedTerms);
                }

                // For now, return a simple list since SearchLog table structure is not confirmed
                var popularTerms = new List<string>
                {
                    "Japanese snacks",
                    "Sake",
                    "Matcha",
                    "Premium",
                    "Authentic",
                    "Tokyo",
                    "Traditional",
                    "Limited edition"
                };

                _cache.Set(cacheKey, popularTerms, TimeSpan.FromHours(1));

                return ServiceResult<List<string>>.Success(popularTerms.Take(count).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular search terms");
                return ServiceResult<List<string>>.Failure("An error occurred while getting popular search terms");
            }
        }

        public async Task<ServiceResult<SearchFacetsDto>> GetSearchFacetsAsync(
            ProductFilterRequestDto baseFilter,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = GenerateFacetsCacheKey(baseFilter);
                
                if (_cache.TryGetValue(cacheKey, out SearchFacetsDto? cachedFacets) && cachedFacets != null)
                {
                    return ServiceResult<SearchFacetsDto>.Success(cachedFacets);
                }

                var facets = new SearchFacetsDto();

                // Get base query for facet calculations
                var baseQuery = _context.Products
                    .Where(p => p.IsActive && !p.IsDeleted);

                // Apply only the text search and essential filters to get relevant facets
                if (!string.IsNullOrWhiteSpace(baseFilter.Search))
                {
                    var searchTerm = baseFilter.Search.ToLower();
                    baseQuery = baseQuery.Where(p =>
                        p.Name.ToLower().Contains(searchTerm) ||
                        (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                        p.SKU.ToLower().Contains(searchTerm) ||
                        p.Brand.Name.ToLower().Contains(searchTerm));
                }

                // Category facets
                facets.Categories = await baseQuery
                    .GroupBy(p => new { p.CategoryId, p.Category.Name })
                    .Select(g => new FacetGroupDto
                    {
                        Id = g.Key.CategoryId,
                        Name = g.Key.Name,
                        Count = g.Count(),
                        IsSelected = baseFilter.CategoryId == g.Key.CategoryId
                    })
                    .OrderByDescending(f => f.Count)
                    .Take(10)
                    .ToListAsync(cancellationToken);

                // Brand facets
                facets.Brands = await baseQuery
                    .GroupBy(p => new { p.BrandId, p.Brand.Name })
                    .Select(g => new FacetGroupDto
                    {
                        Id = g.Key.BrandId,
                        Name = g.Key.Name,
                        Count = g.Count(),
                        IsSelected = baseFilter.BrandId == g.Key.BrandId
                    })
                    .OrderByDescending(f => f.Count)
                    .Take(10)
                    .ToListAsync(cancellationToken);

                // Price range facets
                facets.PriceRanges = new List<PriceRangeDto>
                {
                    new() { Label = "Under 100K", MinPrice = 0, MaxPrice = 100000, Count = await baseQuery.CountAsync(p => p.Price < 100000, cancellationToken) },
                    new() { Label = "100K - 500K", MinPrice = 100000, MaxPrice = 500000, Count = await baseQuery.CountAsync(p => p.Price >= 100000 && p.Price <= 500000, cancellationToken) },
                    new() { Label = "500K - 1M", MinPrice = 500000, MaxPrice = 1000000, Count = await baseQuery.CountAsync(p => p.Price >= 500000 && p.Price <= 1000000, cancellationToken) },
                    new() { Label = "1M - 5M", MinPrice = 1000000, MaxPrice = 5000000, Count = await baseQuery.CountAsync(p => p.Price >= 1000000 && p.Price <= 5000000, cancellationToken) },
                    new() { Label = "Over 5M", MinPrice = 5000000, MaxPrice = decimal.MaxValue, Count = await baseQuery.CountAsync(p => p.Price > 5000000, cancellationToken) }
                };

                // Rating facets
                facets.Ratings = new RatingFacetsDto
                {
                    FiveStars = await baseQuery.CountAsync(p => p.Rating >= 5, cancellationToken),
                    FourStarsAndUp = await baseQuery.CountAsync(p => p.Rating >= 4, cancellationToken),
                    ThreeStarsAndUp = await baseQuery.CountAsync(p => p.Rating >= 3, cancellationToken),
                    TwoStarsAndUp = await baseQuery.CountAsync(p => p.Rating >= 2, cancellationToken),
                    OneStarAndUp = await baseQuery.CountAsync(p => p.Rating >= 1, cancellationToken)
                };

                // Stock facets
                facets.Stock = new StockFacetsDto
                {
                    InStock = await baseQuery.CountAsync(p => p.Stock > 0, cancellationToken),
                    LowStock = await baseQuery.CountAsync(p => p.Stock > 0 && p.Stock <= 10, cancellationToken),
                    OutOfStock = await baseQuery.CountAsync(p => p.Stock == 0, cancellationToken),
                    AllowBackorder = await baseQuery.CountAsync(p => p.AllowBackorder, cancellationToken)
                };

                _cache.Set(cacheKey, facets, _facetsCacheDuration);

                return ServiceResult<SearchFacetsDto>.Success(facets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search facets");
                return ServiceResult<SearchFacetsDto>.Failure("An error occurred while getting search facets");
            }
        }

        public async Task TrackSearchQueryAsync(
            string query, 
            int resultCount, 
            Guid? userId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return;

                // For now, just log the search query
                _logger.LogInformation("Search tracked: Query='{Query}', Results={ResultCount}, UserId={UserId}", 
                    query, resultCount, userId);

                // You can add actual search tracking logic here when SearchLog table is properly set up
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking search query: {Query}", query);
                // Don't throw - search tracking shouldn't break the main search functionality
            }
        }

        public async Task<ServiceResult<List<TrendingSearchDto>>> GetTrendingSearchesAsync(
            int days = 7,
            int count = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = $"trending_searches_{days}d_{count}";
                
                if (_cache.TryGetValue(cacheKey, out List<TrendingSearchDto>? cachedTrending) && cachedTrending != null)
                {
                    return ServiceResult<List<TrendingSearchDto>>.Success(cachedTrending);
                }

                // Mock trending searches for now
                var trendingSearches = new List<TrendingSearchDto>
                {
                    new() { Query = "japanese snacks", SearchCount = 150, ResultCount = 25, LastSearched = DateTime.UtcNow.AddHours(-2), TrendScore = 95.5m },
                    new() { Query = "sake premium", SearchCount = 120, ResultCount = 18, LastSearched = DateTime.UtcNow.AddHours(-1), TrendScore = 88.2m },
                    new() { Query = "matcha tea", SearchCount = 98, ResultCount = 12, LastSearched = DateTime.UtcNow.AddMinutes(-30), TrendScore = 76.8m },
                    new() { Query = "tokyo authentic", SearchCount = 85, ResultCount = 22, LastSearched = DateTime.UtcNow.AddHours(-3), TrendScore = 71.2m },
                    new() { Query = "traditional japanese", SearchCount = 75, ResultCount = 35, LastSearched = DateTime.UtcNow.AddHours(-4), TrendScore = 68.9m }
                };

                _cache.Set(cacheKey, trendingSearches, TimeSpan.FromHours(2));

                return ServiceResult<List<TrendingSearchDto>>.Success(trendingSearches.Take(count).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending searches");
                return ServiceResult<List<TrendingSearchDto>>.Failure("An error occurred while getting trending searches");
            }
        }

        public async Task ClearSearchCacheAsync(string? pattern = null)
        {
            try
            {
                // Note: IMemoryCache doesn't have a built-in way to clear by pattern
                // In a real application, you might want to use a more sophisticated caching solution
                // like Redis with pattern-based clearing
                
                if (pattern == null)
                {
                    // Clear all cache - would need custom implementation
                    _logger.LogInformation("Search cache clear requested");
                }
                else
                {
                    _logger.LogInformation("Search cache clear requested for pattern: {Pattern}", pattern);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing search cache");
            }
        }

        #region Private Helper Methods

        private string GenerateSearchCacheKey(ProductFilterRequestDto filter)
        {
            var keyData = new
            {
                filter.Search,
                filter.CategoryId,
                filter.BrandId,
                filter.MinPrice,
                filter.MaxPrice,
                filter.MinRating,
                filter.TagNames,
                filter.TagIds,
                filter.TagsSearch,
                filter.TagMatchMode,
                filter.InStockOnly,
                filter.OnSaleOnly,
                filter.FeaturedOnly,
                filter.NewOnly,
                filter.IsFeatured,
                filter.IsNew,
                filter.IsBestseller,
                filter.IsLimitedEdition,
                filter.SortBy,
                filter.SortOrder,
                filter.Page,
                filter.PageSize
            };

            var json = JsonSerializer.Serialize(keyData);
            var hash = json.GetHashCode();
            return $"product_search_{hash}";
        }

        private string GenerateFacetsCacheKey(ProductFilterRequestDto filter)
        {
            var keyData = new
            {
                filter.Search,
                filter.CategoryId,
                filter.BrandId
            };

            var json = JsonSerializer.Serialize(keyData);
            var hash = json.GetHashCode();
            return $"search_facets_{hash}";
        }

        private ProductSummaryDto MapToProductSummaryDto(SakuraHomeAPI.Models.Entities.Product product)
        {
            return new ProductSummaryDto
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug ?? $"product-{product.Id}",
                SKU = product.SKU,
                ShortDescription = product.ShortDescription,
                Price = product.Price,
                OriginalPrice = product.OriginalPrice,
                MainImage = product.MainImage,
                Rating = product.Rating,
                ReviewCount = product.ReviewCount,
                IsInStock = product.Stock > 0 || product.AllowBackorder,
                Stock = product.Stock,
                IsFeatured = product.IsFeatured,
                IsNew = product.IsNew,
                IsBestseller = product.IsBestseller,
                IsOnSale = product.OriginalPrice.HasValue && product.OriginalPrice > product.Price,
                Brand = new BrandSummaryDto
                {
                    Id = product.Brand.Id,
                    Name = product.Brand.Name,
                    Slug = product.Brand.Slug ?? $"brand-{product.Brand.Id}"
                },
                Category = new CategorySummaryDto
                {
                    Id = product.Category.Id,
                    Name = product.Category.Name,
                    Slug = product.Category.Slug ?? $"category-{product.Category.Id}"
                },
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Status = product.Status,
                Condition = product.Condition,
                ViewCount = product.ViewCount,
                SoldCount = product.SoldCount,
                WishlistCount = product.WishlistCount,
                IsLimitedEdition = product.IsLimitedEdition,
                IsGiftWrappingAvailable = product.IsGiftWrappingAvailable,
                AllowBackorder = product.AllowBackorder,
                AllowPreorder = product.AllowPreorder
            };
        }

        private int GetAppliedFiltersCount(ProductFilterRequestDto filter)
        {
            var count = 0;

            if (!string.IsNullOrWhiteSpace(filter.Search)) count++;
            if (filter.CategoryId.HasValue) count++;
            if (filter.BrandId.HasValue) count++;
            if (filter.MinPrice.HasValue) count++;
            if (filter.MaxPrice.HasValue) count++;
            if (filter.MinRating.HasValue) count++;
            if (filter.TagNames?.Any() == true) count++;
            if (filter.TagIds?.Any() == true) count++;
            if (!string.IsNullOrWhiteSpace(filter.TagsSearch)) count++;
            if (filter.InStockOnly == true) count++;
            if (filter.OnSaleOnly == true) count++;
            if (filter.FeaturedOnly == true) count++;
            if (filter.NewOnly == true) count++;
            if (filter.IsFeatured.HasValue) count++;
            if (filter.IsNew.HasValue) count++;
            if (filter.IsBestseller.HasValue) count++;
            if (filter.IsLimitedEdition.HasValue) count++;

            return count;
        }

        #endregion
    }
}