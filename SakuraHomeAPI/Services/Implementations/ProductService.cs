using AutoMapper;
using Microsoft.Extensions.Logging;
using SakuraHomeAPI.DTOs.Products.Responses;
using SakuraHomeAPI.DTOs.Products.Requests;
using SakuraHomeAPI.DTOs.Products.Components;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Services.Common;
using SakuraHomeAPI.Services.Interfaces;
using SakuraHomeAPI.Repositories.Interfaces;

namespace SakuraHomeAPI.Services.Implementations
{
    /// <summary>
    /// Product service implementation with comprehensive business logic
    /// Handles product management, validation, and complex operations
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            IMapper mapper,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Product CRUD Operations

        public async Task<ServiceResult<ProductDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting product with ID: {ProductId}", id);

                var product = await _productRepository.GetDetailAsync(id, cancellationToken);
                if (product == null)
                {
                    _logger.LogWarning("Product not found with ID: {ProductId}", id);
                    return ServiceResult<ProductDetailDto>.NotFound("Product not found");
                }

                var productDto = _mapper.Map<ProductDetailDto>(product);
                return ServiceResult<ProductDetailDto>.Success(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product with ID: {ProductId}", id);
                return ServiceResult<ProductDetailDto>.InternalError("An error occurred while retrieving the product");
            }
        }

        public async Task<ServiceResult<ProductDetailDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    return ServiceResult<ProductDetailDto>.Failure("Product slug is required");
                }

                _logger.LogInformation("Getting product with slug: {Slug}", slug);

                var product = await _productRepository.GetBySlugAsync(slug, cancellationToken);
                if (product == null)
                {
                    _logger.LogWarning("Product not found with slug: {Slug}", slug);
                    return ServiceResult<ProductDetailDto>.NotFound("Product not found");
                }

                var productDto = _mapper.Map<ProductDetailDto>(product);
                return ServiceResult<ProductDetailDto>.Success(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product with slug: {Slug}", slug);
                return ServiceResult<ProductDetailDto>.InternalError("An error occurred while retrieving the product");
            }
        }

        public async Task<ServiceResult<ProductListResponseDto>> GetListAsync(
            ProductFilterRequestDto filter,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting product list with filter: {@Filter}", filter);

                var (products, totalCount) = await _productRepository.SearchAsync(filter, cancellationToken);
                
                var productDtos = _mapper.Map<IEnumerable<ProductSummaryDto>>(products);

                // Calculate aggregates for better filtering UX
                var aggregates = new ProductAggregateInfoDto();
                if (productDtos.Any())
                {
                    aggregates.MinPrice = productDtos.Min(p => p.Price);
                    aggregates.MaxPrice = productDtos.Max(p => p.Price);
                    aggregates.AvgPrice = Math.Round(productDtos.Average(p => p.Price), 2);
                    aggregates.TotalProducts = totalCount;
                    aggregates.InStockProducts = productDtos.Count(p => p.IsInStock);
                    aggregates.OutOfStockProducts = productDtos.Count(p => !p.IsInStock);
                    aggregates.OnSaleProducts = productDtos.Count(p => p.IsOnSale);
                    aggregates.FeaturedProducts = productDtos.Count(p => p.IsFeatured);
                    aggregates.NewProducts = productDtos.Count(p => p.IsNew);
                    aggregates.AvgRating = productDtos.Any() ? Math.Round(productDtos.Average(p => (double)p.Rating), 2) : 0;

                    // Top brands in results
                    aggregates.TopBrands = productDtos
                        .GroupBy(p => new { p.Brand.Id, p.Brand.Name })
                        .Select(g => new BrandCountDto
                        {
                            BrandId = g.Key.Id,
                            BrandName = g.Key.Name,
                            ProductCount = g.Count()
                        })
                        .OrderByDescending(b => b.ProductCount)
                        .Take(5)
                        .ToList();

                    // Top categories in results
                    aggregates.TopCategories = productDtos
                        .GroupBy(p => new { p.Category.Id, p.Category.Name })
                        .Select(g => new CategoryCountDto
                        {
                            CategoryId = g.Key.Id,
                            CategoryName = g.Key.Name,
                            ProductCount = g.Count()
                        })
                        .OrderByDescending(c => c.ProductCount)
                        .Take(5)
                        .ToList();
                }
                
                var response = new ProductListResponseDto
                {
                    Data = productDtos,
                    Pagination = new PaginationResponseDto
                    {
                        CurrentPage = filter.Page,
                        PageSize = filter.PageSize,
                        TotalItems = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize),
                        HasNext = filter.Page * filter.PageSize < totalCount,
                        HasPrevious = filter.Page > 1
                    },
                    Filters = _mapper.Map<ProductFilterInfoDto>(filter),
                    Aggregates = aggregates
                };

                // Add applied filters count to filters
                response.Filters.TotalFiltersApplied = GetAppliedFiltersCount(filter);

                return ServiceResult<ProductListResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product list with filter: {@Filter}", filter);
                return ServiceResult<ProductListResponseDto>.InternalError("An error occurred while retrieving products");
            }
        }

        public async Task<ServiceResult<ProductDetailDto>> CreateAsync(
            CreateProductRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating new product with name: {ProductName}", request.Name);

                // Business validation
                var validationResult = await ValidateProductForCreation(request, cancellationToken);
                if (!validationResult.IsSuccess)
                {
                    return ServiceResult<ProductDetailDto>.ValidationError(validationResult.Errors);
                }

                // Map to entity
                var product = _mapper.Map<Product>(request);
                
                // Generate slug if not provided
                if (string.IsNullOrEmpty(product.Slug))
                {
                    product.Slug = GenerateSlug(product.Name);
                }

                // Ensure unique slug
                product.Slug = await EnsureUniqueSlug(product.Slug, null, cancellationToken);

                // Create product
                var createdProduct = await _productRepository.AddAsync(product, cancellationToken);
                await _productRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Product created successfully with ID: {ProductId}", createdProduct.Id);

                // Get full details
                var detailProduct = await _productRepository.GetDetailAsync(createdProduct.Id, cancellationToken);
                var productDto = _mapper.Map<ProductDetailDto>(detailProduct);

                return ServiceResult<ProductDetailDto>.Created(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product with name: {ProductName}", request.Name);
                return ServiceResult<ProductDetailDto>.InternalError("An error occurred while creating the product");
            }
        }

        public async Task<ServiceResult<ProductDetailDto>> UpdateAsync(
            int id,
            UpdateProductRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating product with ID: {ProductId}", id);

                var existingProduct = await _productRepository.GetByIdAsync(id, cancellationToken);
                if (existingProduct == null)
                {
                    return ServiceResult<ProductDetailDto>.NotFound("Product not found");
                }

                // Business validation
                var validationResult = await ValidateProductForUpdate(request, id, cancellationToken);
                if (!validationResult.IsSuccess)
                {
                    return ServiceResult<ProductDetailDto>.ValidationError(validationResult.Errors);
                }

                // Map updates
                _mapper.Map(request, existingProduct);

                // Update slug if name changed
                if (!string.IsNullOrEmpty(request.Name) && request.Name != existingProduct.Name)
                {
                    var newSlug = GenerateSlug(request.Name);
                    existingProduct.Slug = await EnsureUniqueSlug(newSlug, id, cancellationToken);
                }

                await _productRepository.UpdateAsync(existingProduct, cancellationToken);
                await _productRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Product updated successfully with ID: {ProductId}", id);

                // Get updated details
                var updatedProduct = await _productRepository.GetDetailAsync(id, cancellationToken);
                var productDto = _mapper.Map<ProductDetailDto>(updatedProduct);

                return ServiceResult<ProductDetailDto>.Success(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {ProductId}", id);
                return ServiceResult<ProductDetailDto>.InternalError("An error occurred while updating the product");
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deleting product with ID: {ProductId}", id);

                var result = await _productRepository.SoftDeleteAsync(id, cancellationToken);
                if (!result)
                {
                    return ServiceResult<bool>.NotFound("Product not found");
                }

                await _productRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Product deleted successfully with ID: {ProductId}", id);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
                return ServiceResult<bool>.InternalError("An error occurred while deleting the product");
            }
        }

        public async Task<ServiceResult<bool>> RestoreAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Restoring product with ID: {ProductId}", id);

                var result = await _productRepository.RestoreAsync(id, cancellationToken);
                if (!result)
                {
                    return ServiceResult<bool>.NotFound("Product not found");
                }

                await _productRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Product restored successfully with ID: {ProductId}", id);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring product with ID: {ProductId}", id);
                return ServiceResult<bool>.InternalError("An error occurred while restoring the product");
            }
        }

        #endregion

        #region Product Discovery

        public async Task<ServiceResult<ProductListResponseDto>> SearchAsync(
            ProductFilterRequestDto filter,
            CancellationToken cancellationToken = default)
        {
            return await GetListAsync(filter, cancellationToken);
        }

        public async Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetFeaturedAsync(
            int count = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await _productRepository.GetFeaturedAsync(count, cancellationToken);
                var productDtos = _mapper.Map<IEnumerable<ProductSummaryDto>>(products);
                return ServiceResult<IEnumerable<ProductSummaryDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured products");
                return ServiceResult<IEnumerable<ProductSummaryDto>>.InternalError("An error occurred while retrieving featured products");
            }
        }

        public async Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetNewestAsync(
            int count = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await _productRepository.GetNewestAsync(count, cancellationToken);
                var productDtos = _mapper.Map<IEnumerable<ProductSummaryDto>>(products);
                return ServiceResult<IEnumerable<ProductSummaryDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting newest products");
                return ServiceResult<IEnumerable<ProductSummaryDto>>.InternalError("An error occurred while retrieving newest products");
            }
        }

        public async Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetBestSellersAsync(
            int count = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await _productRepository.GetBestSellersAsync(count, cancellationToken);
                var productDtos = _mapper.Map<IEnumerable<ProductSummaryDto>>(products);
                return ServiceResult<IEnumerable<ProductSummaryDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting best selling products");
                return ServiceResult<IEnumerable<ProductSummaryDto>>.InternalError("An error occurred while retrieving best selling products");
            }
        }

        public async Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetOnSaleAsync(
            int count = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await _productRepository.GetOnSaleAsync(count, cancellationToken);
                var productDtos = _mapper.Map<IEnumerable<ProductSummaryDto>>(products);
                return ServiceResult<IEnumerable<ProductSummaryDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products on sale");
                return ServiceResult<IEnumerable<ProductSummaryDto>>.InternalError("An error occurred while retrieving products on sale");
            }
        }

        public async Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetRelatedAsync(
            int productId,
            int count = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await _productRepository.GetRelatedAsync(productId, count, cancellationToken);
                var productDtos = _mapper.Map<IEnumerable<ProductSummaryDto>>(products);
                return ServiceResult<IEnumerable<ProductSummaryDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting related products for product ID: {ProductId}", productId);
                return ServiceResult<IEnumerable<ProductSummaryDto>>.InternalError("An error occurred while retrieving related products");
            }
        }

        public async Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetTrendingAsync(
            int count = 10,
            int daysPeriod = 7,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await _productRepository.GetTrendingAsync(count, daysPeriod, cancellationToken);
                var productDtos = _mapper.Map<IEnumerable<ProductSummaryDto>>(products);
                return ServiceResult<IEnumerable<ProductSummaryDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending products");
                return ServiceResult<IEnumerable<ProductSummaryDto>>.InternalError("An error occurred while retrieving trending products");
            }
        }

        public async Task<ServiceResult<ProductListResponseDto>> GetByCategoryAsync(
            int categoryId,
            int pageNumber = 1,
            int pageSize = 10,
            bool includeSubcategories = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var (products, totalCount) = await _productRepository.GetByCategoryAsync(
                    categoryId, pageNumber, pageSize, includeSubcategories, cancellationToken);
                
                var productDtos = _mapper.Map<IEnumerable<ProductSummaryDto>>(products);
                
                var response = new ProductListResponseDto
                {
                    Data = productDtos,
                    Pagination = new PaginationResponseDto
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalItems = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        HasNext = pageNumber * pageSize < totalCount,
                        HasPrevious = pageNumber > 1
                    }
                };

                return ServiceResult<ProductListResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category ID: {CategoryId}", categoryId);
                return ServiceResult<ProductListResponseDto>.InternalError("An error occurred while retrieving products by category");
            }
        }

        public async Task<ServiceResult<ProductListResponseDto>> GetByBrandAsync(
            int brandId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var (products, totalCount) = await _productRepository.GetByBrandAsync(
                    brandId, pageNumber, pageSize, cancellationToken);
                
                var productDtos = _mapper.Map<IEnumerable<ProductSummaryDto>>(products);
                
                var response = new ProductListResponseDto
                {
                    Data = productDtos,
                    Pagination = new PaginationResponseDto
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalItems = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        HasNext = pageNumber * pageSize < totalCount,
                        HasPrevious = pageNumber > 1
                    }
                };

                return ServiceResult<ProductListResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by brand ID: {BrandId}", brandId);
                return ServiceResult<ProductListResponseDto>.InternalError("An error occurred while retrieving products by brand");
            }
        }

        #endregion

        #region Product Management

        public async Task<ServiceResult<bool>> UpdateStatusAsync(
            int productId,
            ProductStatus status,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productRepository.UpdateStatusAsync(productId, status, cancellationToken);
                if (!result)
                {
                    return ServiceResult<bool>.NotFound("Product not found");
                }

                await _productRepository.SaveChangesAsync(cancellationToken);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product status for ID: {ProductId}", productId);
                return ServiceResult<bool>.InternalError("An error occurred while updating product status");
            }
        }

        public async Task<ServiceResult<bool>> UpdateStockAsync(
            int productId,
            UpdateStockRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productRepository.UpdateStockAsync(
                    productId, request.NewStock, request.Reason, cancellationToken);
                
                if (!result)
                {
                    return ServiceResult<bool>.NotFound("Product not found");
                }

                await _productRepository.SaveChangesAsync(cancellationToken);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for product ID: {ProductId}", productId);
                return ServiceResult<bool>.InternalError("An error occurred while updating stock");
            }
        }

        public async Task<ServiceResult<bool>> TrackViewAsync(
            int productId,
            Guid? userId = null,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _productRepository.IncrementViewCountAsync(productId, cancellationToken);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error tracking view for product ID: {ProductId}", productId);
                // Don't fail the request for view tracking errors
                return ServiceResult<bool>.Success(false);
            }
        }

        public async Task<ServiceResult<bool>> IsSlugAvailableAsync(
            string slug,
            int? excludeProductId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var exists = await _productRepository.SlugExistsAsync(slug, excludeProductId, cancellationToken);
                return ServiceResult<bool>.Success(!exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking slug availability: {Slug}", slug);
                return ServiceResult<bool>.InternalError("An error occurred while checking slug availability");
            }
        }

        public async Task<ServiceResult<bool>> IsSkuAvailableAsync(
            string sku,
            int? excludeProductId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var exists = await _productRepository.SkuExistsAsync(sku, excludeProductId, cancellationToken);
                return ServiceResult<bool>.Success(!exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking SKU availability: {SKU}", sku);
                return ServiceResult<bool>.InternalError("An error occurred while checking SKU availability");
            }
        }

        #endregion

        #region Stock Management

        public async Task<ServiceResult<bool>> CheckStockAvailabilityAsync(
            int productId,
            int quantity,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var isAvailable = await _productRepository.IsStockAvailableAsync(productId, quantity, cancellationToken);
                return ServiceResult<bool>.Success(isAvailable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock availability for product ID: {ProductId}", productId);
                return ServiceResult<bool>.InternalError("An error occurred while checking stock availability");
            }
        }

        public async Task<ServiceResult<bool>> ReserveStockAsync(
            int productId,
            int quantity,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productRepository.ReserveStockAsync(productId, quantity, cancellationToken);
                if (result)
                {
                    await _productRepository.SaveChangesAsync(cancellationToken);
                }
                return ServiceResult<bool>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving stock for product ID: {ProductId}", productId);
                return ServiceResult<bool>.InternalError("An error occurred while reserving stock");
            }
        }

        public async Task<ServiceResult<bool>> ReleaseStockAsync(
            int productId,
            int quantity,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productRepository.ReleaseStockAsync(productId, quantity, cancellationToken);
                if (result)
                {
                    await _productRepository.SaveChangesAsync(cancellationToken);
                }
                return ServiceResult<bool>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing stock for product ID: {ProductId}", productId);
                return ServiceResult<bool>.InternalError("An error occurred while releasing stock");
            }
        }

        public async Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetLowStockProductsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await _productRepository.GetLowStockAsync(cancellationToken);
                var productDtos = _mapper.Map<IEnumerable<ProductSummaryDto>>(products);
                return ServiceResult<IEnumerable<ProductSummaryDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock products");
                return ServiceResult<IEnumerable<ProductSummaryDto>>.InternalError("An error occurred while retrieving low stock products");
            }
        }

        public async Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetOutOfStockProductsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await _productRepository.GetOutOfStockAsync(cancellationToken);
                var productDtos = _mapper.Map<IEnumerable<ProductSummaryDto>>(products);
                return ServiceResult<IEnumerable<ProductSummaryDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting out of stock products");
                return ServiceResult<IEnumerable<ProductSummaryDto>>.InternalError("An error occurred while retrieving out of stock products");
            }
        }

        #endregion

        #region Product Components

        public Task<ServiceResult<ProductImageDto>> AddImageAsync(
            int productId,
            CreateProductImageDto request,
            CancellationToken cancellationToken = default)
        {
            // Implementation would involve image processing and saving
            throw new NotImplementedException("Image management will be implemented in a separate phase");
        }

        public Task<ServiceResult<ProductImageDto>> UpdateImageAsync(
            int productId,
            int imageId,
            SakuraHomeAPI.DTOs.Products.Components.UpdateProductImageDto request,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Image management will be implemented in a separate phase");
        }

        public Task<ServiceResult<bool>> DeleteImageAsync(
            int productId,
            int imageId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Image management will be implemented in a separate phase");
        }

        public Task<ServiceResult<ProductVariantDto>> AddVariantAsync(
            int productId,
            CreateProductVariantDto request,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Variant management will be implemented in a separate phase");
        }

        public Task<ServiceResult<ProductVariantDto>> UpdateVariantAsync(
            int productId,
            int variantId,
            SakuraHomeAPI.DTOs.Products.Components.UpdateProductVariantDto request,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Variant management will be implemented in a separate phase");
        }

        public Task<ServiceResult<bool>> DeleteVariantAsync(
            int productId,
            int variantId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Variant management will be implemented in a separate phase");
        }

        public Task<ServiceResult<ProductAttributeDto>> SetAttributeAsync(
            int productId,
            CreateProductAttributeDto request,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Attribute management will be implemented in a separate phase");
        }

        public Task<ServiceResult<bool>> RemoveAttributeAsync(
            int productId,
            int attributeId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Attribute management will be implemented in a separate phase");
        }

        #endregion

        #region Analytics & Reports

        public async Task<ServiceResult<ProductStatisticsDto>> GetStatisticsAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var statistics = await _productRepository.GetStatisticsAsync(productId, cancellationToken);
                return ServiceResult<ProductStatisticsDto>.Success(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for product ID: {ProductId}", productId);
                return ServiceResult<ProductStatisticsDto>.InternalError("An error occurred while retrieving product statistics");
            }
        }

        public Task<ServiceResult<ProductPerformanceDto>> GetPerformanceAsync(
            int productId,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Performance analytics will be implemented in a separate phase");
        }

        public async Task<ServiceResult<IEnumerable<ProductSummaryDto>>> GetByPriceRangeAsync(
            decimal minPrice,
            decimal maxPrice,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await _productRepository.GetByPriceRangeAsync(minPrice, maxPrice, cancellationToken);
                var productDtos = _mapper.Map<IEnumerable<ProductSummaryDto>>(products);
                return ServiceResult<IEnumerable<ProductSummaryDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by price range: {MinPrice}-{MaxPrice}", minPrice, maxPrice);
                return ServiceResult<IEnumerable<ProductSummaryDto>>.InternalError("An error occurred while retrieving products by price range");
            }
        }

        #endregion

        #region Batch Operations

        public async Task<ServiceResult<int>> BulkUpdateStatusAsync(
            BulkUpdateStatusRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var count = await _productRepository.BulkUpdateStatusAsync(request.ProductIds, request.Status, cancellationToken);
                await _productRepository.SaveChangesAsync(cancellationToken);
                return ServiceResult<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating product status");
                return ServiceResult<int>.InternalError("An error occurred while updating product status");
            }
        }

        public async Task<ServiceResult<int>> BulkUpdatePricesAsync(
            BulkUpdatePricesRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var count = await _productRepository.BulkUpdatePricesAsync(request.ProductPrices, cancellationToken);
                await _productRepository.SaveChangesAsync(cancellationToken);
                return ServiceResult<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating product prices");
                return ServiceResult<int>.InternalError("An error occurred while updating product prices");
            }
        }

        public async Task<ServiceResult<int>> BulkUpdateStockAsync(
            BulkUpdateStockRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var count = await _productRepository.BulkUpdateStockAsync(request.ProductStocks, request.Reason, cancellationToken);
                await _productRepository.SaveChangesAsync(cancellationToken);
                return ServiceResult<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating product stock");
                return ServiceResult<int>.InternalError("An error occurred while updating product stock");
            }
        }

        public Task<ServiceResult<BulkImportResultDto>> BulkImportAsync(
            IEnumerable<CreateProductRequestDto> products,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Bulk import will be implemented in a separate phase");
        }

        #endregion

        #region Private Helper Methods

        private async Task<ServiceResult> ValidateProductForCreation(
            CreateProductRequestDto request,
            CancellationToken cancellationToken)
        {
            var errors = new List<string>();

            // Check SKU uniqueness
            if (!string.IsNullOrEmpty(request.SKU))
            {
                var skuExists = await _productRepository.SkuExistsAsync(request.SKU, null, cancellationToken);
                if (skuExists)
                {
                    errors.Add("SKU already exists");
                }
            }

            // Add more business validation as needed
            if (request.Price <= 0)
            {
                errors.Add("Price must be greater than 0");
            }

            if (request.Stock < 0)
            {
                errors.Add("Stock cannot be negative");
            }

            return errors.Any()
                ? ServiceResult.ValidationError(errors)
                : ServiceResult.Success();
        }

        private async Task<ServiceResult> ValidateProductForUpdate(
            UpdateProductRequestDto request,
            int productId,
            CancellationToken cancellationToken)
        {
            var errors = new List<string>();

            // Check SKU uniqueness (excluding current product)
            if (!string.IsNullOrEmpty(request.SKU))
            {
                var skuExists = await _productRepository.SkuExistsAsync(request.SKU, productId, cancellationToken);
                if (skuExists)
                {
                    errors.Add("SKU already exists for another product");
                }
            }

            // Add more business validation as needed
            if (request.Price <= 0)
            {
                errors.Add("Price must be greater than 0");
            }

            if (request.Stock < 0)
            {
                errors.Add("Stock cannot be negative");
            }

            return errors.Any()
                ? ServiceResult.ValidationError(errors)
                : ServiceResult.Success();
        }

        private static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return text.ToLowerInvariant()
                .Replace(' ', '-')
                .Replace('_', '-')
                .Trim('-');
        }

        private async Task<string> EnsureUniqueSlug(string baseSlug, int? excludeProductId, CancellationToken cancellationToken)
        {
            var slug = baseSlug;
            var counter = 1;

            while (await _productRepository.SlugExistsAsync(slug, excludeProductId, cancellationToken))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
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
            if (filter.Status.HasValue) count++;
            if (filter.Condition.HasValue) count++;
            if (!string.IsNullOrWhiteSpace(filter.Origin)) count++;
            if (filter.JapaneseRegion.HasValue) count++;
            if (filter.AuthenticityLevel.HasValue) count++;

            return count;
        }

        #endregion
    }
}