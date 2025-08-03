using System.Linq.Expressions;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.DTOs.Products.Requests;
using SakuraHomeAPI.DTOs.Products.Responses;

namespace SakuraHomeAPI.Repositories.Interfaces
{
    /// <summary>
    /// Product repository interface with specialized operations
    /// Extends base repository with product-specific queries and operations
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        #region Product-Specific Queries

        /// <summary>
        /// Get products by category with pagination
        /// </summary>
        Task<(IEnumerable<Product> Products, int TotalCount)> GetByCategoryAsync(
            int categoryId,
            int pageNumber = 1,
            int pageSize = 10,
            bool includeSubcategories = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get products by brand with pagination
        /// </summary>
        Task<(IEnumerable<Product> Products, int TotalCount)> GetByBrandAsync(
            int brandId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Search products with advanced filtering
        /// </summary>
        Task<(IEnumerable<Product> Products, int TotalCount)> SearchAsync(
            ProductFilterRequestDto filter,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get featured products
        /// </summary>
        Task<IEnumerable<Product>> GetFeaturedAsync(
            int count = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get newest products
        /// </summary>
        Task<IEnumerable<Product>> GetNewestAsync(
            int count = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get best selling products
        /// </summary>
        Task<IEnumerable<Product>> GetBestSellersAsync(
            int count = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get products on sale
        /// </summary>
        Task<IEnumerable<Product>> GetOnSaleAsync(
            int count = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get related products based on category/tags
        /// </summary>
        Task<IEnumerable<Product>> GetRelatedAsync(
            int productId,
            int count = 5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get recently viewed products by user
        /// </summary>
        Task<IEnumerable<Product>> GetRecentlyViewedAsync(
            Guid userId,
            int count = 10,
            CancellationToken cancellationToken = default);

        #endregion

        #region Product Management

        /// <summary>
        /// Get product with all related data for detailed view
        /// </summary>
        Task<Product?> GetDetailAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get product by SKU
        /// </summary>
        Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get product by slug
        /// </summary>
        Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if SKU exists (for validation)
        /// </summary>
        Task<bool> SkuExistsAsync(string sku, int? excludeProductId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if slug exists (for validation)
        /// </summary>
        Task<bool> SlugExistsAsync(string slug, int? excludeProductId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update product status
        /// </summary>
        Task<bool> UpdateStatusAsync(int productId, ProductStatus status, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update product stock
        /// </summary>
        Task<bool> UpdateStockAsync(int productId, int newStock, string reason = "", CancellationToken cancellationToken = default);

        /// <summary>
        /// Increment view count
        /// </summary>
        Task IncrementViewCountAsync(int productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update product rating and review count
        /// </summary>
        Task UpdateRatingAsync(int productId, CancellationToken cancellationToken = default);

        #endregion

        #region Stock Management

        /// <summary>
        /// Get products with low stock
        /// </summary>
        Task<IEnumerable<Product>> GetLowStockAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get out of stock products
        /// </summary>
        Task<IEnumerable<Product>> GetOutOfStockAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Reserve stock for order
        /// </summary>
        Task<bool> ReserveStockAsync(int productId, int quantity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Release reserved stock
        /// </summary>
        Task<bool> ReleaseStockAsync(int productId, int quantity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check stock availability
        /// </summary>
        Task<bool> IsStockAvailableAsync(int productId, int quantity, CancellationToken cancellationToken = default);

        #endregion

        #region Analytics & Statistics

        /// <summary>
        /// Get product statistics
        /// </summary>
        Task<ProductStatisticsDto> GetStatisticsAsync(int productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get products by price range
        /// </summary>
        Task<IEnumerable<Product>> GetByPriceRangeAsync(
            decimal minPrice,
            decimal maxPrice,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get product view history
        /// </summary>
        Task<IEnumerable<ProductView>> GetViewHistoryAsync(
            int productId,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get trending products based on views and sales
        /// </summary>
        Task<IEnumerable<Product>> GetTrendingAsync(
            int count = 10,
            int daysPeriod = 7,
            CancellationToken cancellationToken = default);

        #endregion

        #region Batch Operations

        /// <summary>
        /// Bulk update product status
        /// </summary>
        Task<int> BulkUpdateStatusAsync(
            IEnumerable<int> productIds,
            ProductStatus status,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk update product prices
        /// </summary>
        Task<int> BulkUpdatePricesAsync(
            Dictionary<int, decimal> productPrices,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk update product stock
        /// </summary>
        Task<int> BulkUpdateStockAsync(
            Dictionary<int, int> productStocks,
            string reason = "",
            CancellationToken cancellationToken = default);

        #endregion
    }

    /// <summary>
    /// Product statistics DTO
    /// </summary>
    public class ProductStatisticsDto
    {
        public int ProductId { get; set; }
        public int ViewCount { get; set; }
        public int SoldCount { get; set; }
        public int ReviewCount { get; set; }
        public decimal AverageRating { get; set; }
        public int WishlistCount { get; set; }
        public int CartCount { get; set; }
        public decimal Revenue { get; set; }
        public DateTime LastSold { get; set; }
        public DateTime LastViewed { get; set; }
    }
}