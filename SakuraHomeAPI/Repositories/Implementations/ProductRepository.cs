using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.Repositories.Interfaces;
using SakuraHomeAPI.DTOs.Products.Requests;
using SakuraHomeAPI.DTOs.Products.Components;
using System.Linq.Expressions;

namespace SakuraHomeAPI.Repositories.Implementations
{
    /// <summary>
    /// Enhanced Product repository implementation with comprehensive search and filtering
    /// Provides optimized product management and querying capabilities with tag support
    /// </summary>
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        #region Product-Specific Queries

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetByCategoryAsync(
            int categoryId,
            int pageNumber = 1,
            int pageSize = 10,
            bool includeSubcategories = false,
            CancellationToken cancellationToken = default)
        {
            var query = GetBaseProductQuery();

            if (includeSubcategories)
            {
                // Get all subcategory IDs efficiently
                var subcategoryIds = await _context.Categories
                    .Where(c => c.ParentId == categoryId || c.Id == categoryId)
                    .Select(c => c.Id)
                    .ToListAsync(cancellationToken);

                query = query.Where(p => subcategoryIds.Contains(p.CategoryId));
            }
            else
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            
            var products = await query
                .OrderBy(p => p.DisplayOrder)
                .ThenByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (products, totalCount);
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetByBrandAsync(
            int brandId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = GetBaseProductQuery()
                .Where(p => p.BrandId == brandId);

            var totalCount = await query.CountAsync(cancellationToken);
            
            var products = await query
                .OrderBy(p => p.DisplayOrder)
                .ThenByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (products, totalCount);
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> SearchAsync(
            ProductFilterRequestDto filter,
            CancellationToken cancellationToken = default)
        {
            var query = GetBaseProductQuery();

            // Apply all filters using the enhanced method
            query = ApplyFilters(query, filter);

            // Apply search functionality
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = ApplyTextSearch(query, filter.Search);
            }

            // Apply tag filters
            query = await ApplyTagFilters(query, filter, cancellationToken);

            // Apply sorting
            query = ApplySorting(query, filter);

            var totalCount = await query.CountAsync(cancellationToken);
            
            var products = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            return (products, totalCount);
        }

        public async Task<IEnumerable<Product>> GetFeaturedAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            return await GetBaseProductQuery()
                .Where(p => p.IsFeatured)
                .OrderBy(p => p.DisplayOrder)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetNewestAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            return await GetBaseProductQuery()
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetBestSellersAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            return await GetBaseProductQuery()
                .OrderByDescending(p => p.SoldCount)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetOnSaleAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            return await GetBaseProductQuery()
                .Where(p => p.OriginalPrice.HasValue && p.OriginalPrice > p.Price)
                .OrderByDescending(p => (p.OriginalPrice - p.Price) / p.OriginalPrice) // Highest discount percentage first
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetRelatedAsync(int productId, int count = 5, CancellationToken cancellationToken = default)
        {
            var product = await GetByIdAsync(productId, cancellationToken);
            if (product == null) return Enumerable.Empty<Product>();

            return await GetBaseProductQuery()
                .Where(p => p.Id != productId && 
                           (p.CategoryId == product.CategoryId || p.BrandId == product.BrandId))
                .OrderByDescending(p => p.CategoryId == product.CategoryId ? 2 : 1) // Prioritize same category
                .ThenByDescending(p => p.Rating)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetRecentlyViewedAsync(Guid userId, int count = 10, CancellationToken cancellationToken = default)
        {
            var recentlyViewedIds = await _context.ProductViews
                .Where(pv => pv.UserId == userId)
                .OrderByDescending(pv => pv.LastViewedAt)
                .Select(pv => pv.ProductId)
                .Distinct()
                .Take(count)
                .ToListAsync(cancellationToken);

            if (!recentlyViewedIds.Any())
                return Enumerable.Empty<Product>();

            var products = await GetBaseProductQuery()
                .Where(p => recentlyViewedIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            // Maintain the order of recently viewed
            return recentlyViewedIds.Select(id => products.FirstOrDefault(p => p.Id == id))
                                   .Where(p => p != null)!;
        }

        #endregion

        #region Enhanced Search Methods

        /// <summary>
        /// Get base product query with all necessary includes and active filters
        /// </summary>
        private IQueryable<Product> GetBaseProductQuery()
        {
            return GetQueryable(
                p => p.Brand,
                p => p.Category,
                p => p.ProductImages.Where(pi => pi.IsActive))
                .Where(p => p.Status == ProductStatus.Active && p.IsActive && !p.IsDeleted);
        }

        /// <summary>
        /// Apply comprehensive filters to product query
        /// </summary>
        private IQueryable<Product> ApplyFilters(IQueryable<Product> query, ProductFilterRequestDto filter)
        {
            // Category filter with subcategories support
            if (filter.CategoryId.HasValue)
            {
                if (filter.IncludeSubcategories)
                {
                    // This will be handled separately for async operation
                    query = query.Where(p => p.CategoryId == filter.CategoryId);
                }
                else
                {
                    query = query.Where(p => p.CategoryId == filter.CategoryId);
                }
            }

            // Brand filter
            if (filter.BrandId.HasValue)
            {
                query = query.Where(p => p.BrandId == filter.BrandId);
            }

            // Price range filters
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filter.MinPrice);
            }
            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice);
            }

            // Rating filter
            if (filter.MinRating.HasValue)
            {
                query = query.Where(p => p.Rating >= filter.MinRating);
            }

            // Stock filters
            if (filter.InStockOnly.HasValue && filter.InStockOnly.Value)
            {
                query = query.Where(p => p.Stock > 0 || p.AllowBackorder);
            }

            if (filter.MinStock.HasValue)
            {
                query = query.Where(p => p.Stock >= filter.MinStock);
            }
            if (filter.MaxStock.HasValue)
            {
                query = query.Where(p => p.Stock <= filter.MaxStock);
            }

            // Sale filter
            if (filter.OnSaleOnly.HasValue && filter.OnSaleOnly.Value)
            {
                query = query.Where(p => p.OriginalPrice.HasValue && p.OriginalPrice > p.Price);
            }

            if (filter.HasDiscount.HasValue && filter.HasDiscount.Value)
            {
                query = query.Where(p => p.OriginalPrice.HasValue && p.OriginalPrice > p.Price);
            }

            // Boolean property filters
            if (filter.IsFeatured.HasValue)
            {
                query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);
            }
            if (filter.FeaturedOnly.HasValue && filter.FeaturedOnly.Value)
            {
                query = query.Where(p => p.IsFeatured);
            }

            if (filter.IsNew.HasValue)
            {
                query = query.Where(p => p.IsNew == filter.IsNew.Value);
            }
            if (filter.NewOnly.HasValue && filter.NewOnly.Value)
            {
                query = query.Where(p => p.IsNew);
            }

            if (filter.IsBestseller.HasValue)
            {
                query = query.Where(p => p.IsBestseller == filter.IsBestseller.Value);
            }

            if (filter.IsLimitedEdition.HasValue)
            {
                query = query.Where(p => p.IsLimitedEdition == filter.IsLimitedEdition.Value);
            }

            if (filter.IsGiftWrappingAvailable.HasValue)
            {
                query = query.Where(p => p.IsGiftWrappingAvailable == filter.IsGiftWrappingAvailable.Value);
            }

            if (filter.AllowBackorder.HasValue)
            {
                query = query.Where(p => p.AllowBackorder == filter.AllowBackorder.Value);
            }

            if (filter.AllowPreorder.HasValue)
            {
                query = query.Where(p => p.AllowPreorder == filter.AllowPreorder.Value);
            }

            // Status and condition filters
            if (filter.Status.HasValue)
            {
                query = query.Where(p => p.Status == filter.Status.Value);
            }

            if (filter.Condition.HasValue)
            {
                query = query.Where(p => p.Condition == filter.Condition.Value);
            }

            // Origin and regional filters
            if (!string.IsNullOrWhiteSpace(filter.Origin))
            {
                query = query.Where(p => p.Origin != null && p.Origin.ToLower().Contains(filter.Origin.ToLower()));
            }

            if (filter.JapaneseRegion.HasValue)
            {
                query = query.Where(p => p.JapaneseRegion == filter.JapaneseRegion.Value);
            }

            if (filter.AuthenticityLevel.HasValue)
            {
                query = query.Where(p => p.AuthenticityLevel == filter.AuthenticityLevel.Value);
            }

            if (filter.AgeRestriction.HasValue)
            {
                query = query.Where(p => p.AgeRestriction == filter.AgeRestriction.Value);
            }

            // Weight filters
            if (filter.MinWeight.HasValue)
            {
                query = query.Where(p => p.Weight >= filter.MinWeight);
            }
            if (filter.MaxWeight.HasValue)
            {
                query = query.Where(p => p.Weight <= filter.MaxWeight);
            }

            if (filter.WeightUnit.HasValue)
            {
                query = query.Where(p => p.WeightUnit == filter.WeightUnit.Value);
            }

            // Date filters
            if (filter.CreatedFrom.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= filter.CreatedFrom);
            }
            if (filter.CreatedTo.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= filter.CreatedTo);
            }

            if (filter.AvailableFrom.HasValue)
            {
                query = query.Where(p => p.AvailableFrom >= filter.AvailableFrom);
            }
            if (filter.AvailableUntil.HasValue)
            {
                query = query.Where(p => p.AvailableUntil <= filter.AvailableUntil);
            }

            return query;
        }

        /// <summary>
        /// Apply text search across multiple fields with relevance
        /// </summary>
        private IQueryable<Product> ApplyTextSearch(IQueryable<Product> query, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return query;

            var cleanSearchTerm = searchTerm.Trim().ToLower();

            // First, try exact phrase search (for phrases like "bim bim", "japanese snack")
            var exactPhraseQuery = query.Where(p =>
                p.Name.ToLower().Contains(cleanSearchTerm) ||
                (p.Description != null && p.Description.ToLower().Contains(cleanSearchTerm)) ||
                (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(cleanSearchTerm)) ||
                p.SKU.ToLower().Contains(cleanSearchTerm) ||
                (p.Tags != null && p.Tags.ToLower().Contains(cleanSearchTerm)) ||
                (p.MetaKeywords != null && p.MetaKeywords.ToLower().Contains(cleanSearchTerm)) ||
                p.Brand.Name.ToLower().Contains(cleanSearchTerm) ||
                p.Category.Name.ToLower().Contains(cleanSearchTerm));

            // Split search term for individual word matching
            var searchTerms = cleanSearchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                          .Where(t => t.Length > 1)
                                          .Distinct()
                                          .ToList();

            // If only one term or exact phrase match might exist, return exact phrase query
            if (searchTerms.Count <= 1)
            {
                return exactPhraseQuery;
            }

            // For multi-word searches, combine exact phrase matching with individual term matching
            Expression<Func<Product, bool>>? individualTermsExpression = null;

            foreach (var term in searchTerms)
            {
                Expression<Func<Product, bool>> termExpression = p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Description != null && p.Description.ToLower().Contains(term)) ||
                    (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(term)) ||
                    p.SKU.ToLower().Contains(term) ||
                    (p.Tags != null && p.Tags.ToLower().Contains(term)) ||
                    (p.MetaKeywords != null && p.MetaKeywords.ToLower().Contains(term)) ||
                    p.Brand.Name.ToLower().Contains(term) ||
                    p.Category.Name.ToLower().Contains(term);

                if (individualTermsExpression == null)
                {
                    individualTermsExpression = termExpression;
                }
                else
                {
                    // Combine with AND for individual terms (all terms must be found somewhere)
                    var parameter = Expression.Parameter(typeof(Product), "p");
                    var left = Expression.Invoke(individualTermsExpression, parameter);
                    var right = Expression.Invoke(termExpression, parameter);
                    var body = Expression.AndAlso(left, right);
                    individualTermsExpression = Expression.Lambda<Func<Product, bool>>(body, parameter);
                }
            }

            // Combine exact phrase search with individual terms search using OR
            if (individualTermsExpression != null)
            {
                var parameter = Expression.Parameter(typeof(Product), "p");
                
                // Exact phrase match
                Expression<Func<Product, bool>> exactPhraseExpression = p =>
                    p.Name.ToLower().Contains(cleanSearchTerm) ||
                    (p.Description != null && p.Description.ToLower().Contains(cleanSearchTerm)) ||
                    (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(cleanSearchTerm)) ||
                    p.SKU.ToLower().Contains(cleanSearchTerm) ||
                    (p.Tags != null && p.Tags.ToLower().Contains(cleanSearchTerm)) ||
                    (p.MetaKeywords != null && p.MetaKeywords.ToLower().Contains(cleanSearchTerm)) ||
                    p.Brand.Name.ToLower().Contains(cleanSearchTerm) ||
                    p.Category.Name.ToLower().Contains(cleanSearchTerm);

                var exactLeft = Expression.Invoke(exactPhraseExpression, parameter);
                var individualRight = Expression.Invoke(individualTermsExpression, parameter);
                var combinedBody = Expression.OrElse(exactLeft, individualRight);
                var combinedExpression = Expression.Lambda<Func<Product, bool>>(combinedBody, parameter);
                
                return query.Where(combinedExpression);
            }

            return exactPhraseQuery;
        }

        /// <summary>
        /// Apply comprehensive tag filtering with multiple modes
        /// </summary>
        private async Task<IQueryable<Product>> ApplyTagFilters(IQueryable<Product> query, ProductFilterRequestDto filter, CancellationToken cancellationToken)
        {
            var hasTagFilters = false;

            // Tag string search in Tags field
            if (!string.IsNullOrWhiteSpace(filter.TagsSearch))
            {
                var tagSearch = filter.TagsSearch.Trim().ToLower();
                query = query.Where(p => p.Tags != null && p.Tags.ToLower().Contains(tagSearch));
                hasTagFilters = true;
            }

            // Tag names filtering (assumes Tags field contains comma-separated values)
            if (filter.TagNames?.Any() == true)
            {
                if (filter.TagMatchMode == TagMatchMode.All)
                {
                    // ALL tags must be present (AND logic)
                    foreach (var tagName in filter.TagNames)
                    {
                        var tagLower = tagName.ToLower();
                        query = query.Where(p => p.Tags != null && p.Tags.ToLower().Contains(tagLower));
                    }
                }
                else
                {
                    // ANY tag can be present (OR logic)
                    Expression<Func<Product, bool>>? tagExpression = null;

                    foreach (var tagName in filter.TagNames)
                    {
                        var tagLower = tagName.ToLower();
                        Expression<Func<Product, bool>> singleTagExpression = p => 
                            p.Tags != null && p.Tags.ToLower().Contains(tagLower);

                        if (tagExpression == null)
                        {
                            tagExpression = singleTagExpression;
                        }
                        else
                        {
                            var parameter = Expression.Parameter(typeof(Product), "p");
                            var left = Expression.Invoke(tagExpression, parameter);
                            var right = Expression.Invoke(singleTagExpression, parameter);
                            var body = Expression.OrElse(left, right);
                            tagExpression = Expression.Lambda<Func<Product, bool>>(body, parameter);
                        }
                    }

                    if (tagExpression != null)
                    {
                        query = query.Where(tagExpression);
                    }
                }
                hasTagFilters = true;
            }

            // Tag IDs filtering (if you have a proper Tag entity relationship)
            if (filter.TagIds?.Any() == true)
            {
                // Note: This assumes you have ProductTag relationship table
                // If not implemented yet, this part can be commented out
                /*
                if (filter.TagMatchMode == TagMatchMode.All)
                {
                    // Product must have ALL specified tags
                    foreach (var tagId in filter.TagIds)
                    {
                        query = query.Where(p => p.ProductTags.Any(pt => pt.TagId == tagId));
                    }
                }
                else
                {
                    // Product must have ANY of the specified tags
                    query = query.Where(p => p.ProductTags.Any(pt => filter.TagIds.Contains(pt.TagId)));
                }
                */
                hasTagFilters = true;
            }

            return query;
        }

        /// <summary>
        /// Apply comprehensive sorting with multiple options
        /// </summary>
        private IQueryable<Product> ApplySorting(IQueryable<Product> query, ProductFilterRequestDto filter)
        {
            var sortBy = filter.SortBy?.ToLower() ?? "created";
            var sortOrder = filter.SortOrder?.ToLower() ?? "desc";

            return sortBy switch
            {
                "name" => sortOrder == "asc" 
                    ? query.OrderBy(p => p.Name) 
                    : query.OrderByDescending(p => p.Name),
                    
                "price" => sortOrder == "asc" 
                    ? query.OrderBy(p => p.Price) 
                    : query.OrderByDescending(p => p.Price),
                    
                "rating" => sortOrder == "asc" 
                    ? query.OrderBy(p => p.Rating) 
                    : query.OrderByDescending(p => p.Rating),
                    
                "created" => sortOrder == "asc" 
                    ? query.OrderBy(p => p.CreatedAt) 
                    : query.OrderByDescending(p => p.CreatedAt),
                    
                "updated" => sortOrder == "asc" 
                    ? query.OrderBy(p => p.UpdatedAt) 
                    : query.OrderByDescending(p => p.UpdatedAt),
                    
                "sold" => sortOrder == "asc" 
                    ? query.OrderBy(p => p.SoldCount) 
                    : query.OrderByDescending(p => p.SoldCount),
                    
                "views" => sortOrder == "asc" 
                    ? query.OrderBy(p => p.ViewCount) 
                    : query.OrderByDescending(p => p.ViewCount),
                    
                "stock" => sortOrder == "asc" 
                    ? query.OrderBy(p => p.Stock) 
                    : query.OrderByDescending(p => p.Stock),
                    
                "popularity" => query.OrderByDescending(p => p.ViewCount)
                                   .ThenByDescending(p => p.SoldCount)
                                   .ThenByDescending(p => p.Rating),
                                   
                "discount" => query.Where(p => p.OriginalPrice.HasValue && p.OriginalPrice > p.Price)
                                  .OrderByDescending(p => (p.OriginalPrice - p.Price) / p.OriginalPrice),
                                  
                "relevance" => !string.IsNullOrWhiteSpace(filter.Search)
                    ? ApplyRelevanceSort(query, filter.Search)
                    : query.OrderByDescending(p => p.CreatedAt),
                    
                _ => query.OrderBy(p => p.DisplayOrder)
                         .ThenByDescending(p => p.CreatedAt)
            };
        }

        /// <summary>
        /// Apply relevance-based sorting for search results
        /// </summary>
        private IQueryable<Product> ApplyRelevanceSort(IQueryable<Product> query, string searchTerm)
        {
            var cleanSearchTerm = searchTerm.Trim().ToLower();
            
            // Enhanced relevance scoring with both exact phrase and individual term support:
            // Priority 6: Exact phrase match in Name
            // Priority 5: Name starts with exact phrase
            // Priority 4: Exact phrase match in MetaKeywords
            // Priority 3: Name contains exact phrase or individual terms match
            // Priority 2: MetaKeywords contains individual terms or other high-value fields contain phrase
            // Priority 1: Other fields contain phrase or individual terms
            return query.OrderByDescending(p => 
                        // Priority 6: Exact phrase match in Name
                        p.Name.ToLower() == cleanSearchTerm ? 6 :
                        // Priority 5: Name starts with exact phrase
                        p.Name.ToLower().StartsWith(cleanSearchTerm) ? 5 :
                        // Priority 4: Exact phrase in MetaKeywords
                        (p.MetaKeywords != null && p.MetaKeywords.ToLower().Contains(cleanSearchTerm)) ? 4 :
                        // Priority 3: Name contains exact phrase
                        p.Name.ToLower().Contains(cleanSearchTerm) ? 3 :
                        // Priority 2: High-value fields contain exact phrase
                        ((p.Description != null && p.Description.ToLower().Contains(cleanSearchTerm)) ||
                         (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(cleanSearchTerm))) ? 2 :
                        // Priority 1: Other fields contain exact phrase or any match found
                        ((p.Tags != null && p.Tags.ToLower().Contains(cleanSearchTerm)) ||
                         p.SKU.ToLower().Contains(cleanSearchTerm) ||
                         p.Brand.Name.ToLower().Contains(cleanSearchTerm) ||
                         p.Category.Name.ToLower().Contains(cleanSearchTerm)) ? 1 : 0)
                       .ThenByDescending(p => p.Rating)
                       .ThenByDescending(p => p.SoldCount);
        }

        #endregion

        #region Product Management

        public async Task<Product?> GetDetailAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetQueryable(
                    p => p.Brand,
                    p => p.Category,
                    p => p.ProductImages.Where(pi => pi.IsActive),
                    p => p.Variants.Where(v => v.IsActive && !v.IsDeleted),
                    p => p.AttributeValues,
                    p => p.ProductTags,
                    p => p.Reviews.Where(r => r.IsActive && !r.IsDeleted && r.IsApproved).Take(5))
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            return await GetQueryable()
                .FirstOrDefaultAsync(p => p.SKU == sku, cancellationToken);
        }

        public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await GetQueryable(
                    p => p.Brand,
                    p => p.Category,
                    p => p.ProductImages.Where(pi => pi.IsActive),
                    p => p.Variants.Where(v => v.IsActive && !v.IsDeleted),
                    p => p.AttributeValues,
                    p => p.ProductTags,
                    p => p.Reviews.Where(r => r.IsActive && !r.IsDeleted && r.IsApproved).Take(5))
                .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
        }

        public async Task<bool> SkuExistsAsync(string sku, int? excludeProductId = null, CancellationToken cancellationToken = default)
        {
            var query = GetQueryable().Where(p => p.SKU == sku);
            
            if (excludeProductId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProductId);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> SlugExistsAsync(string slug, int? excludeProductId = null, CancellationToken cancellationToken = default)
        {
            var query = GetQueryable().Where(p => p.Slug == slug);
            
            if (excludeProductId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProductId);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> UpdateStatusAsync(int productId, ProductStatus status, CancellationToken cancellationToken = default)
        {
            var product = await GetByIdAsync(productId, cancellationToken);
            if (product == null) return false;

            product.Status = status;
            await UpdateAsync(product, cancellationToken);
            return true;
        }

        public async Task<bool> UpdateStockAsync(int productId, int newStock, string reason = "", CancellationToken cancellationToken = default)
        {
            var product = await GetByIdAsync(productId, cancellationToken);
            if (product == null) return false;

            var previousStock = product.Stock;
            product.Stock = newStock;
            
            // Log inventory change
            var inventoryLog = new InventoryLog
            {
                ProductId = productId,
                Action = newStock > previousStock ? InventoryAction.Adjustment : InventoryAction.Adjustment,
                Quantity = Math.Abs(newStock - previousStock),
                PreviousStock = previousStock,
                NewStock = newStock,
                Reason = reason,
                ReferenceType = "Manual",
                CreatedAt = DateTime.UtcNow
            };

            await _context.InventoryLogs.AddAsync(inventoryLog, cancellationToken);
            await UpdateAsync(product, cancellationToken);
            
            return true;
        }

        public async Task IncrementViewCountAsync(int productId, CancellationToken cancellationToken = default)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Products SET ViewCount = ViewCount + 1 WHERE Id = {0}",
                productId);
        }

        public async Task UpdateRatingAsync(int productId, CancellationToken cancellationToken = default)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsActive && !r.IsDeleted && r.IsApproved)
                .ToListAsync(cancellationToken);

            var product = await GetByIdAsync(productId, cancellationToken);
            if (product == null) return;

            product.ReviewCount = reviews.Count;
            product.Rating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0;

            await UpdateAsync(product, cancellationToken);
        }

        #endregion

        #region Stock Management

        public async Task<IEnumerable<Product>> GetLowStockAsync(CancellationToken cancellationToken = default)
        {
            return await GetQueryable()
                .Where(p => p.TrackInventory && p.MinStock.HasValue && p.Stock <= p.MinStock)
                .OrderBy(p => p.Stock)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetOutOfStockAsync(CancellationToken cancellationToken = default)
        {
            return await GetQueryable()
                .Where(p => p.Stock == 0 && !p.AllowBackorder)
                .OrderByDescending(p => p.SoldCount)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ReserveStockAsync(int productId, int quantity, CancellationToken cancellationToken = default)
        {
            var product = await GetByIdAsync(productId, cancellationToken);
            if (product == null || product.Stock < quantity) return false;

            product.Stock -= quantity;
            await UpdateAsync(product, cancellationToken);
            
            return true;
        }

        public async Task<bool> ReleaseStockAsync(int productId, int quantity, CancellationToken cancellationToken = default)
        {
            var product = await GetByIdAsync(productId, cancellationToken);
            if (product == null) return false;

            product.Stock += quantity;
            await UpdateAsync(product, cancellationToken);
            
            return true;
        }

        public async Task<bool> IsStockAvailableAsync(int productId, int quantity, CancellationToken cancellationToken = default)
        {
            var product = await GetByIdAsync(productId, cancellationToken);
            return product != null && (product.Stock >= quantity || product.AllowBackorder);
        }

        #endregion

        #region Analytics & Statistics

        public async Task<ProductStatisticsDto> GetStatisticsAsync(int productId, CancellationToken cancellationToken = default)
        {
            var product = await GetByIdAsync(productId, cancellationToken);
            if (product == null) 
                return new ProductStatisticsDto { ProductId = productId };

            var orderItems = await _context.OrderItems
                .Where(oi => oi.ProductId == productId)
                .ToListAsync(cancellationToken);

            var cartCount = await _context.CartItems
                .CountAsync(ci => ci.ProductId == productId, cancellationToken);

            var wishlistCount = await _context.WishlistItems
                .CountAsync(wi => wi.ProductId == productId, cancellationToken);

            return new ProductStatisticsDto
            {
                ProductId = productId,
                ViewCount = product.ViewCount,
                SoldCount = product.SoldCount,
                ReviewCount = product.ReviewCount,
                AverageRating = product.Rating,
                WishlistCount = wishlistCount,
                CartCount = cartCount,
                Revenue = orderItems.Sum(oi => oi.TotalPrice),
                LastSold = orderItems.Any() ? orderItems.Max(oi => oi.Order.OrderDate) : DateTime.MinValue,
                LastViewed = await _context.ProductViews
                    .Where(pv => pv.ProductId == productId)
                    .Select(pv => pv.LastViewedAt)
                    .OrderByDescending(d => d)
                    .FirstOrDefaultAsync(cancellationToken)
            };
        }

        public async Task<IEnumerable<Product>> GetByPriceRangeAsync(
            decimal minPrice,
            decimal maxPrice,
            CancellationToken cancellationToken = default)
        {
            return await GetQueryable()
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice && 
                           p.Status == ProductStatus.Active && p.IsActive && !p.IsDeleted)
                .OrderBy(p => p.Price)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ProductView>> GetViewHistoryAsync(
            int productId,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.ProductViews.Where(pv => pv.ProductId == productId);

            if (fromDate.HasValue)
                query = query.Where(pv => pv.LastViewedAt >= fromDate);

            if (toDate.HasValue)
                query = query.Where(pv => pv.LastViewedAt <= toDate);

            return await query
                .OrderByDescending(pv => pv.LastViewedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetTrendingAsync(
            int count = 10,
            int daysPeriod = 7,
            CancellationToken cancellationToken = default)
        {
            var fromDate = DateTime.UtcNow.AddDays(-daysPeriod);

            var recentViews = await _context.ProductViews
                .Where(pv => pv.LastViewedAt >= fromDate)
                .GroupBy(pv => pv.ProductId)
                .Select(g => new { ProductId = g.Key, ViewCount = g.Count() })
                .OrderByDescending(x => x.ViewCount)
                .Take(count * 2) // Get more to filter later
                .ToListAsync(cancellationToken);

            var productIds = recentViews.Select(rv => rv.ProductId).ToList();

            if (!productIds.Any())
                return Enumerable.Empty<Product>();

            return await GetBaseProductQuery()
                .Where(p => productIds.Contains(p.Id))
                .OrderByDescending(p => recentViews.First(rv => rv.ProductId == p.Id).ViewCount)
                .ThenByDescending(p => p.SoldCount)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        #endregion

        #region Batch Operations

        public async Task<int> BulkUpdateStatusAsync(
            IEnumerable<int> productIds,
            ProductStatus status,
            CancellationToken cancellationToken = default)
        {
            var products = await GetQueryable()
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            foreach (var product in products)
            {
                product.Status = status;
            }

            await UpdateRangeAsync(products, cancellationToken);
            return products.Count;
        }

        public async Task<int> BulkUpdatePricesAsync(
            Dictionary<int, decimal> productPrices,
            CancellationToken cancellationToken = default)
        {
            var productIds = productPrices.Keys.ToList();
            var products = await GetQueryable()
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            foreach (var product in products)
            {
                if (productPrices.TryGetValue(product.Id, out var newPrice))
                {
                    product.Price = newPrice;
                }
            }

            await UpdateRangeAsync(products, cancellationToken);
            return products.Count;
        }

        public async Task<int> BulkUpdateStockAsync(
            Dictionary<int, int> productStocks,
            string reason = "",
            CancellationToken cancellationToken = default)
        {
            var productIds = productStocks.Keys.ToList();
            var products = await GetQueryable()
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            var inventoryLogs = new List<InventoryLog>();

            foreach (var product in products)
            {
                if (productStocks.TryGetValue(product.Id, out var newStock))
                {
                    var previousStock = product.Stock;
                    product.Stock = newStock;

                    // Create inventory log
                    inventoryLogs.Add(new InventoryLog
                    {
                        ProductId = product.Id,
                        Action = InventoryAction.Adjustment,
                        Quantity = Math.Abs(newStock - previousStock),
                        PreviousStock = previousStock,
                        NewStock = newStock,
                        Reason = reason,
                        ReferenceType = "Bulk Update",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.InventoryLogs.AddRangeAsync(inventoryLogs, cancellationToken);
            await UpdateRangeAsync(products, cancellationToken);
            
            return products.Count;
        }

        #endregion
    }
}