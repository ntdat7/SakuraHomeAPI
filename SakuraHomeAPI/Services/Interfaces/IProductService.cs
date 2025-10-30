using SakuraHomeAPI.DTOs.Products.Responses;
using SakuraHomeAPI.DTOs.Products.Requests;
using SakuraHomeAPI.DTOs.Products.Components;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.Services.Common;
using SakuraHomeAPI.Repositories.Interfaces;

namespace SakuraHomeAPI.Services.Interfaces
{
    /// <summary>
    /// Product service interface defining business logic operations
    /// Handles complex business rules, validations, and orchestrations
    /// </summary>
    public interface IProductService
    {
        #region Product CRUD Operations

        /// <summary>
        /// Get product by ID with detailed information
        /// </summary>
        Task<ServiceResult<ProductDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get product by slug with detailed information
        /// </summary>
        Task<ServiceResult<ProductDetailDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paginated list of products with filtering
        /// </summary>
        Task<ServiceResult<ProductListResponseDto>> GetListAsync(
            ProductFilterRequestDto filter,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create new product
        /// </summary>
        Task<ServiceResult<ProductDetailDto>> CreateAsync(
            CreateProductRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update existing product
        /// </summary>
        Task<ServiceResult<ProductDetailDto>> UpdateAsync(
            int id,
            UpdateProductRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete product (soft delete)
        /// </summary>
        Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Restore soft deleted product
        /// </summary>
        Task<ServiceResult<bool>> RestoreAsync(int id, CancellationToken cancellationToken = default);

        #endregion

        #region Product Discovery

        /// <summary>
        /// Search products with advanced filtering
        /// </summary>
        Task<ServiceResult<ProductListResponseDto>> SearchAsync(
            ProductFilterRequestDto filter,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get featured products
        /// </summary>
        Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetFeaturedAsync(
            int count = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get newest products
        /// </summary>
        Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetNewestAsync(
            int count = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get best selling products
        /// </summary>
        Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetBestSellersAsync(
            int count = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get products on sale
        /// </summary>
        Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetOnSaleAsync(
            int count = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get related products
        /// </summary>
        Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetRelatedAsync(
            int productId,
            int count = 5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get trending products
        /// </summary>
        Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetTrendingAsync(
            int count = 10,
            int daysPeriod = 7,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get products by category
        /// </summary>
        Task<ServiceResult<ProductListResponseDto>> GetByCategoryAsync(
            int categoryId,
            int pageNumber = 1,
            int pageSize = 10,
            bool includeSubcategories = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get products by brand
        /// </summary>
        Task<ServiceResult<ProductListResponseDto>> GetByBrandAsync(
            int brandId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        #endregion

        #region Product Management

        /// <summary>
        /// Update product status
        /// </summary>
        Task<ServiceResult<bool>> UpdateStatusAsync(
            int productId,
            ProductStatus status,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update product stock
        /// </summary>
        Task<ServiceResult<bool>> UpdateStockAsync(
            int productId,
            UpdateStockRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Track product view
        /// </summary>
        Task<ServiceResult<bool>> TrackViewAsync(
            int productId,
            Guid? userId = null,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if product slug is available
        /// </summary>
        Task<ServiceResult<bool>> IsSlugAvailableAsync(
            string slug,
            int? excludeProductId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if product SKU is available
        /// </summary>
        Task<ServiceResult<bool>> IsSkuAvailableAsync(
            string sku,
            int? excludeProductId = null,
            CancellationToken cancellationToken = default);

        #endregion

        #region Stock Management

        /// <summary>
        /// Check stock availability for purchase
        /// </summary>
        Task<ServiceResult<bool>> CheckStockAvailabilityAsync(
            int productId,
            int quantity,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Reserve stock for order
        /// </summary>
        Task<ServiceResult<bool>> ReserveStockAsync(
            int productId,
            int quantity,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Release reserved stock
        /// </summary>
        Task<ServiceResult<bool>> ReleaseStockAsync(
            int productId,
            int quantity,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get products with low stock
        /// </summary>
        Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetLowStockProductsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get out of stock products
        /// </summary>
        Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetOutOfStockProductsAsync(
            CancellationToken cancellationToken = default);

        #endregion

        #region Product Components

        /// <summary>
        /// Add product image
        /// </summary>
        Task<ServiceResult<ProductImageDto>> AddImageAsync(
            int productId,
            CreateProductImageDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update product image
        /// </summary>
        Task<ServiceResult<ProductImageDto>> UpdateImageAsync(
            int productId,
            int imageId,
            SakuraHomeAPI.DTOs.Products.Components.UpdateProductImageDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete product image
        /// </summary>
        Task<ServiceResult<bool>> DeleteImageAsync(
            int productId,
            int imageId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Add product variant
        /// </summary>
        Task<ServiceResult<ProductVariantDto>> AddVariantAsync(
            int productId,
            CreateProductVariantDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update product variant
        /// </summary>
        Task<ServiceResult<ProductVariantDto>> UpdateVariantAsync(
            int productId,
            int variantId,
            SakuraHomeAPI.DTOs.Products.Components.UpdateProductVariantDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete product variant
        /// </summary>
        Task<ServiceResult<bool>> DeleteVariantAsync(
            int productId,
            int variantId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Set product attribute
        /// </summary>
        Task<ServiceResult<ProductAttributeDto>> SetAttributeAsync(
            int productId,
            CreateProductAttributeDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove product attribute
        /// </summary>
        Task<ServiceResult<bool>> RemoveAttributeAsync(
            int productId,
            int attributeId,
            CancellationToken cancellationToken = default);

        #endregion

        #region Analytics & Reports

        /// <summary>
        /// Get product statistics
        /// </summary>
        Task<ServiceResult<ProductStatisticsDto>> GetStatisticsAsync(
            int productId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get product performance metrics
        /// </summary>
        Task<ServiceResult<ProductPerformanceDto>> GetPerformanceAsync(
            int productId,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get products by price range
        /// </summary>
        Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetByPriceRangeAsync(
            decimal minPrice,
            decimal maxPrice,
            CancellationToken cancellationToken = default);

        #endregion

        #region Batch Operations

        /// <summary>
        /// Bulk update product status
        /// </summary>
        Task<ServiceResult<int>> BulkUpdateStatusAsync(
            BulkUpdateStatusRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk update product prices
        /// </summary>
        Task<ServiceResult<int>> BulkUpdatePricesAsync(
            BulkUpdatePricesRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk update product stock
        /// </summary>
        Task<ServiceResult<int>> BulkUpdateStockAsync(
            BulkUpdateStockRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk import products
        /// </summary>
        Task<ServiceResult<BulkImportResultDto>> BulkImportAsync(
            IEnumerable<CreateProductRequestDto> products,
            CancellationToken cancellationToken = default);

        #endregion
    }

    #region Support DTOs

    /// <summary>
    /// Product performance metrics DTO
    /// </summary>
    public class ProductPerformanceDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int UniqueViewCount { get; set; }
        public int SoldQuantity { get; set; }
        public decimal Revenue { get; set; }
        public decimal ConversionRate { get; set; }
        public int CartAdditions { get; set; }
        public int WishlistAdditions { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    /// <summary>
    /// Bulk update status request DTO
    /// </summary>
    public class BulkUpdateStatusRequestDto
    {
        public IEnumerable<int> ProductIds { get; set; } = new List<int>();
        public ProductStatus Status { get; set; }
    }

    /// <summary>
    /// Bulk update prices request DTO
    /// </summary>
    public class BulkUpdatePricesRequestDto
    {
        public Dictionary<int, decimal> ProductPrices { get; set; } = new Dictionary<int, decimal>();
    }

    /// <summary>
    /// Bulk update stock request DTO
    /// </summary>
    public class BulkUpdateStockRequestDto
    {
        public Dictionary<int, int> ProductStocks { get; set; } = new Dictionary<int, int>();
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Bulk import result DTO
    /// </summary>
    public class BulkImportResultDto
    {
        public int TotalProducts { get; set; }
        public int SuccessfulImports { get; set; }
        public int FailedImports { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
        public IEnumerable<ProductSummaryDto> ImportedProducts { get; set; } = new List<ProductSummaryDto>();
    }

    /// <summary>
    /// Update product image request DTO
    /// </summary>
    public class UpdateProductImageDto
    {
        public string? AltText { get; set; }
        public string? Caption { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Update product variant request DTO
    /// </summary>
    public class UpdateProductVariantDto
    {
        public string? Name { get; set; }
        public string? SKU { get; set; }
        public decimal? Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int? Stock { get; set; }
        public string? ImageUrl { get; set; }
        public Dictionary<string, string>? Attributes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    #endregion
}