using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.Repositories.Interfaces;
using SakuraHomeAPI.DTOs.Products.Requests;

namespace SakuraHomeAPI.Repositories.Implementations
{
    /// <summary>
    /// Product repository implementation with specialized operations
    /// Provides comprehensive product management and querying capabilities
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
            var query = GetQueryable(p => p.Brand, p => p.Category, p => p.ProductImages.Where(pi => pi.IsActive));

            if (includeSubcategories)
            {
                // Get all subcategory IDs
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

            query = query.Where(p => p.Status == ProductStatus.Active && p.IsActive && !p.IsDeleted);

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
            var query = GetQueryable(p => p.Brand, p => p.Category, p => p.ProductImages.Where(pi => pi.IsActive))
                .Where(p => p.BrandId == brandId && p.Status == ProductStatus.Active && p.IsActive && !p.IsDeleted);

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
            var query = GetQueryable(p => p.Brand, p => p.Category, p => p.ProductImages.Where(pi => pi.IsActive));

            // Apply basic filters
            query = query.Where(p => p.Status == ProductStatus.Active && p.IsActive && !p.IsDeleted);

            // Search term
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var searchTerm = filter.Search.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm) ||
                    p.SKU.ToLower().Contains(searchTerm) ||
                    p.Brand.Name.ToLower().Contains(searchTerm));
            }

            // Category filter
            if (filter.CategoryId.HasValue)
            {
                if (filter.IncludeSubcategories)
                {
                    var subcategoryIds = await _context.Categories
                        .Where(c => c.ParentId == filter.CategoryId || c.Id == filter.CategoryId)
                        .Select(c => c.Id)
                        .ToListAsync(cancellationToken);
                    query = query.Where(p => subcategoryIds.Contains(p.CategoryId));
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

            // Price range filter
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

            // Stock filter
            if (filter.InStockOnly.HasValue && filter.InStockOnly.Value)
                query = query.Where(p => p.Stock > 0 || p.AllowBackorder);

            // On sale filter
            if (filter.OnSaleOnly.HasValue && filter.OnSaleOnly.Value)
                query = query.Where(p => p.OriginalPrice.HasValue && p.OriginalPrice > p.Price);

            // Search in name, description, tags, brand name, category name
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchTerm = filter.Search.ToLower().Trim();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                    (p.Tags != null && p.Tags.ToLower().Contains(searchTerm)) ||
                    p.Brand.Name.ToLower().Contains(searchTerm) ||
                    p.Category.Name.ToLower().Contains(searchTerm));
            }

            if (filter.FeaturedOnly.HasValue && filter.FeaturedOnly.Value)
                query = query.Where(p => p.IsFeatured);

            if (filter.NewOnly.HasValue && filter.NewOnly.Value)
                query = query.Where(p => p.IsNew);

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "rating_desc" => query.OrderByDescending(p => p.Rating),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "popular" => query.OrderByDescending(p => p.ViewCount),
                "bestseller" => query.OrderByDescending(p => p.SoldCount),
                _ => query.OrderBy(p => p.DisplayOrder).ThenByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            
            var products = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            return (products, totalCount);
        }

        public async Task<IEnumerable<Product>> GetFeaturedAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            return await GetQueryable(p => p.Brand, p => p.Category, p => p.ProductImages.Where(pi => pi.IsActive))
                .Where(p => p.IsFeatured && p.Status == ProductStatus.Active && p.IsActive && !p.IsDeleted)
                .OrderBy(p => p.DisplayOrder)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetNewestAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            return await GetQueryable(p => p.Brand, p => p.Category, p => p.ProductImages.Where(pi => pi.IsActive))
                .Where(p => p.Status == ProductStatus.Active && p.IsActive && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetBestSellersAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            return await GetQueryable(p => p.Brand, p => p.Category, p => p.ProductImages.Where(pi => pi.IsActive))
                .Where(p => p.Status == ProductStatus.Active && p.IsActive && !p.IsDeleted)
                .OrderByDescending(p => p.SoldCount)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetOnSaleAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            return await GetQueryable(p => p.Brand, p => p.Category, p => p.ProductImages.Where(pi => pi.IsActive))
                .Where(p => p.Status == ProductStatus.Active && p.IsActive && !p.IsDeleted && 
                           p.OriginalPrice.HasValue && p.OriginalPrice > p.Price)
                .OrderByDescending(p => (p.OriginalPrice - p.Price) / p.OriginalPrice) // Highest discount percentage first
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetRelatedAsync(int productId, int count = 5, CancellationToken cancellationToken = default)
        {
            var product = await GetByIdAsync(productId, cancellationToken);
            if (product == null) return Enumerable.Empty<Product>();

            return await GetQueryable(p => p.Brand, p => p.Category, p => p.ProductImages.Where(pi => pi.IsActive))
                .Where(p => p.Id != productId && 
                           p.Status == ProductStatus.Active && p.IsActive && !p.IsDeleted &&
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

            var products = await GetQueryable(p => p.Brand, p => p.Category, p => p.ProductImages.Where(pi => pi.IsActive))
                .Where(p => recentlyViewedIds.Contains(p.Id) && 
                           p.Status == ProductStatus.Active && p.IsActive && !p.IsDeleted)
                .ToListAsync(cancellationToken);

            // Maintain the order of recently viewed
            return recentlyViewedIds.Select(id => products.FirstOrDefault(p => p.Id == id))
                                   .Where(p => p != null)!;
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

            return await GetQueryable(p => p.Brand, p => p.Category, p => p.ProductImages.Where(pi => pi.IsActive))
                .Where(p => productIds.Contains(p.Id) && 
                           p.Status == ProductStatus.Active && p.IsActive && !p.IsDeleted)
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