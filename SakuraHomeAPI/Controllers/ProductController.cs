using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.DTOs.Products.Requests;
using SakuraHomeAPI.DTOs.Products.Responses;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Services.Interfaces;

namespace SakuraHomeAPI.Controllers
{
    /// <summary>
    /// Product management controller - Refactored to use service layer
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            ILogger<ProductController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all products with filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ProductListResponseDto>> GetProducts(
            [FromQuery] ProductFilterRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting products with filters: {@Filters}", request);

                var result = await _productService.GetListAsync(request, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Failed to get products: {Error}", result.ErrorMessage);
                    return BadRequest(new ProductListResponseDto
                    {
                        Success = false,
                        Message = result.ErrorMessage,
                        Data = new List<ProductSummaryDto>(),
                        Pagination = new PaginationResponseDto()
                    });
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting products with filters: {@Filters}", request);
                return StatusCode(500, new ProductListResponseDto
                {
                    Success = false,
                    Message = "An error occurred while processing your request",
                    Data = new List<ProductSummaryDto>(),
                    Pagination = new PaginationResponseDto()
                });
            }
        }

        /// <summary>
        /// Get product by ID with full details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<ProductDetailDto>>> GetProduct(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting product with ID: {ProductId}", id);

                var result = await _productService.GetByIdAsync(id, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    if (result.StatusCode == 404)
                    {
                        return NotFound(ApiResponseDto<ProductDetailDto>.ErrorResult("Product not found"));
                    }
                    
                    return BadRequest(ApiResponseDto<ProductDetailDto>.ErrorResult(result.ErrorMessage));
                }

                // Track product view
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                await _productService.TrackViewAsync(id, null, ipAddress, userAgent, cancellationToken);

                return Ok(ApiResponseDto<ProductDetailDto>.SuccessResult(result.Data, "Product retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting product {ProductId}", id);
                return StatusCode(500, ApiResponseDto<ProductDetailDto>.ErrorResult("An error occurred while processing your request"));
            }
        }

        /// <summary>
        /// Get product by slug
        /// </summary>
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<ApiResponseDto<ProductDetailDto>>> GetProductBySlug(
            string slug,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting product with slug: {Slug}", slug);

                var result = await _productService.GetBySlugAsync(slug, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    if (result.StatusCode == 404)
                    {
                        return NotFound(ApiResponseDto<ProductDetailDto>.ErrorResult("Product not found"));
                    }
                    
                    return BadRequest(ApiResponseDto<ProductDetailDto>.ErrorResult(result.ErrorMessage));
                }

                // Track product view
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                await _productService.TrackViewAsync(result.Data.Id, null, ipAddress, userAgent, cancellationToken);

                return Ok(ApiResponseDto<ProductDetailDto>.SuccessResult(result.Data, "Product retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting product by slug {Slug}", slug);
                return StatusCode(500, ApiResponseDto<ProductDetailDto>.ErrorResult("An error occurred while processing your request"));
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<ProductDetailDto>>> CreateProduct(
            [FromBody] CreateProductRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating new product: {ProductName}", request.Name);

                var result = await _productService.CreateAsync(request, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResponseDto<ProductDetailDto>.ErrorResult(result.ErrorMessage));
                }

                _logger.LogInformation("Product created successfully with ID {ProductId}", result.Data.Id);

                return CreatedAtAction(
                    nameof(GetProduct),
                    new { id = result.Data.Id },
                    ApiResponseDto<ProductDetailDto>.SuccessResult(result.Data, "Product created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating product");
                return StatusCode(500, ApiResponseDto<ProductDetailDto>.ErrorResult("An error occurred while creating the product"));
            }
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<ProductDetailDto>>> UpdateProduct(
            int id,
            [FromBody] UpdateProductRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating product {ProductId}", id);

                var result = await _productService.UpdateAsync(id, request, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    if (result.StatusCode == 404)
                    {
                        return NotFound(ApiResponseDto<ProductDetailDto>.ErrorResult("Product not found"));
                    }
                    
                    return BadRequest(ApiResponseDto<ProductDetailDto>.ErrorResult(result.ErrorMessage));
                }

                _logger.LogInformation("Product {ProductId} updated successfully", id);

                return Ok(ApiResponseDto<ProductDetailDto>.SuccessResult(result.Data, "Product updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating product {ProductId}", id);
                return StatusCode(500, ApiResponseDto<ProductDetailDto>.ErrorResult("An error occurred while updating the product"));
            }
        }

        /// <summary>
        /// Delete a product (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteProduct(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deleting product {ProductId}", id);

                var result = await _productService.DeleteAsync(id, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    if (result.StatusCode == 404)
                    {
                        return NotFound(ApiResponseDto<bool>.ErrorResult("Product not found"));
                    }
                    
                    return BadRequest(ApiResponseDto<bool>.ErrorResult(result.ErrorMessage));
                }

                _logger.LogInformation("Product {ProductId} deleted successfully", id);

                return Ok(ApiResponseDto<bool>.SuccessResult(true, "Product deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting product {ProductId}", id);
                return StatusCode(500, ApiResponseDto<bool>.ErrorResult("An error occurred while deleting the product"));
            }
        }

        /// <summary>
        /// Update product stock
        /// </summary>
        [HttpPatch("{id}/stock")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateStock(
            int id,
            [FromBody] UpdateStockRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating stock for product {ProductId} to {NewStock}", id, request.NewStock);

                var result = await _productService.UpdateStockAsync(id, request, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    if (result.StatusCode == 404)
                    {
                        return NotFound(ApiResponseDto<bool>.ErrorResult("Product not found"));
                    }
                    
                    return BadRequest(ApiResponseDto<bool>.ErrorResult(result.ErrorMessage));
                }

                _logger.LogInformation("Stock updated successfully for product {ProductId}", id);

                return Ok(ApiResponseDto<bool>.SuccessResult(true, "Stock updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating stock for product {ProductId}", id);
                return StatusCode(500, ApiResponseDto<bool>.ErrorResult("An error occurred while updating stock"));
            }
        }

        /// <summary>
        /// Update product status
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateStatus(
            int id,
            [FromBody] ProductStatus status,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating status for product {ProductId} to {Status}", id, status);

                var result = await _productService.UpdateStatusAsync(id, status, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    if (result.StatusCode == 404)
                    {
                        return NotFound(ApiResponseDto<bool>.ErrorResult("Product not found"));
                    }
                    
                    return BadRequest(ApiResponseDto<bool>.ErrorResult(result.ErrorMessage));
                }

                _logger.LogInformation("Status updated successfully for product {ProductId}", id);

                return Ok(ApiResponseDto<bool>.SuccessResult(true, "Product status updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating status for product {ProductId}", id);
                return StatusCode(500, ApiResponseDto<bool>.ErrorResult("An error occurred while updating product status"));
            }
        }

        #region Discovery & Search Endpoints

        /// <summary>
        /// Search products with advanced filtering
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<ProductListResponseDto>> SearchProducts(
            [FromQuery] ProductFilterRequestDto filter,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Searching products with filters: {@Filters}", filter);

                var result = await _productService.SearchAsync(filter, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    return BadRequest(new ProductListResponseDto
                    {
                        Success = false,
                        Message = result.ErrorMessage,
                        Data = new List<ProductSummaryDto>(),
                        Pagination = new PaginationResponseDto()
                    });
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching products");
                return StatusCode(500, new ProductListResponseDto
                {
                    Success = false,
                    Message = "An error occurred while processing your search request",
                    Data = new List<ProductSummaryDto>(),
                    Pagination = new PaginationResponseDto()
                });
            }
        }

        /// <summary>
        /// Get featured products
        /// </summary>
        [HttpGet("featured")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductSummaryDto>>>> GetFeatured(
            [FromQuery] int count = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productService.GetFeaturedAsync(count, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResponseDto<IEnumerable<ProductSummaryDto>>.ErrorResult(result.ErrorMessage));
                }

                return Ok(ApiResponseDto<IEnumerable<ProductSummaryDto>>.SuccessResult(result.Data, "Featured products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting featured products");
                return StatusCode(500, ApiResponseDto<IEnumerable<ProductSummaryDto>>.ErrorResult("An error occurred while processing your request"));
            }
        }

        /// <summary>
        /// Get newest products
        /// </summary>
        [HttpGet("newest")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductSummaryDto>>>> GetNewest(
            [FromQuery] int count = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productService.GetNewestAsync(count, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResponseDto<IEnumerable<ProductSummaryDto>>.ErrorResult(result.ErrorMessage));
                }

                return Ok(ApiResponseDto<IEnumerable<ProductSummaryDto>>.SuccessResult(result.Data, "Newest products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting newest products");
                return StatusCode(500, ApiResponseDto<IEnumerable<ProductSummaryDto>>.ErrorResult("An error occurred while processing your request"));
            }
        }

        /// <summary>
        /// Get best selling products
        /// </summary>
        [HttpGet("bestsellers")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductSummaryDto>>>> GetBestSellers(
            [FromQuery] int count = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productService.GetBestSellersAsync(count, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResponseDto<IEnumerable<ProductSummaryDto>>.ErrorResult(result.ErrorMessage));
                }

                return Ok(ApiResponseDto<IEnumerable<ProductSummaryDto>>.SuccessResult(result.Data, "Best selling products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting best selling products");
                return StatusCode(500, ApiResponseDto<IEnumerable<ProductSummaryDto>>.ErrorResult("An error occurred while processing your request"));
            }
        }

        /// <summary>
        /// Get products on sale
        /// </summary>
        [HttpGet("on-sale")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductSummaryDto>>>> GetOnSale(
            [FromQuery] int count = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productService.GetOnSaleAsync(count, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResponseDto<IEnumerable<ProductSummaryDto>>.ErrorResult(result.ErrorMessage));
                }

                return Ok(ApiResponseDto<IEnumerable<ProductSummaryDto>>.SuccessResult(result.Data, "Products on sale retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting products on sale");
                return StatusCode(500, ApiResponseDto<IEnumerable<ProductSummaryDto>>.ErrorResult("An error occurred while processing your request"));
            }
        }

        /// <summary>
        /// Get trending products
        /// </summary>
        [HttpGet("trending")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductSummaryDto>>>> GetTrending(
            [FromQuery] int count = 10,
            [FromQuery] int daysPeriod = 7,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productService.GetTrendingAsync(count, daysPeriod, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResponseDto<IEnumerable<ProductSummaryDto>>.ErrorResult(result.ErrorMessage));
                }

                return Ok(ApiResponseDto<IEnumerable<ProductSummaryDto>>.SuccessResult(result.Data, "Trending products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting trending products");
                return StatusCode(500, ApiResponseDto<IEnumerable<ProductSummaryDto>>.ErrorResult("An error occurred while processing your request"));
            }
        }

        /// <summary>
        /// Get related products
        /// </summary>
        [HttpGet("{id}/related")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductSummaryDto>>>> GetRelated(
            int id,
            [FromQuery] int count = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productService.GetRelatedAsync(id, count, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResponseDto<IEnumerable<ProductSummaryDto>>.ErrorResult(result.ErrorMessage));
                }

                return Ok(ApiResponseDto<IEnumerable<ProductSummaryDto>>.SuccessResult(result.Data, "Related products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting related products for product {ProductId}", id);
                return StatusCode(500, ApiResponseDto<IEnumerable<ProductSummaryDto>>.ErrorResult("An error occurred while processing your request"));
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<ProductListResponseDto>> GetByCategory(
            int categoryId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeSubcategories = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productService.GetByCategoryAsync(categoryId, pageNumber, pageSize, includeSubcategories, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    return BadRequest(new ProductListResponseDto
                    {
                        Success = false,
                        Message = result.ErrorMessage,
                        Data = new List<ProductSummaryDto>(),
                        Pagination = new PaginationResponseDto()
                    });
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting products by category {CategoryId}", categoryId);
                return StatusCode(500, new ProductListResponseDto
                {
                    Success = false,
                    Message = "An error occurred while processing your request",
                    Data = new List<ProductSummaryDto>(),
                    Pagination = new PaginationResponseDto()
                });
            }
        }

        /// <summary>
        /// Get products by brand
        /// </summary>
        [HttpGet("brand/{brandId}")]
        public async Task<ActionResult<ProductListResponseDto>> GetByBrand(
            int brandId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productService.GetByBrandAsync(brandId, pageNumber, pageSize, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    return BadRequest(new ProductListResponseDto
                    {
                        Success = false,
                        Message = result.ErrorMessage,
                        Data = new List<ProductSummaryDto>(),
                        Pagination = new PaginationResponseDto()
                    });
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting products by brand {BrandId}", brandId);
                return StatusCode(500, new ProductListResponseDto
                {
                    Success = false,
                    Message = "An error occurred while processing your request",
                    Data = new List<ProductSummaryDto>(),
                    Pagination = new PaginationResponseDto()
                });
            }
        }

        #endregion

        #region Stock Management Endpoints

        /// <summary>
        /// Get products with low stock
        /// </summary>
        [HttpGet("low-stock")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductSummaryDto>>>> GetLowStock(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productService.GetLowStockProductsAsync(cancellationToken);
                
                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResponseDto<IEnumerable<ProductSummaryDto>>.ErrorResult(result.ErrorMessage));
                }

                return Ok(ApiResponseDto<IEnumerable<ProductSummaryDto>>.SuccessResult(result.Data, "Low stock products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting low stock products");
                return StatusCode(500, ApiResponseDto<IEnumerable<ProductSummaryDto>>.ErrorResult("An error occurred while processing your request"));
            }
        }

        /// <summary>
        /// Get out of stock products
        /// </summary>
        [HttpGet("out-of-stock")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductSummaryDto>>>> GetOutOfStock(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productService.GetOutOfStockProductsAsync(cancellationToken);
                
                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResponseDto<IEnumerable<ProductSummaryDto>>.ErrorResult(result.ErrorMessage));
                }

                return Ok(ApiResponseDto<IEnumerable<ProductSummaryDto>>.SuccessResult(result.Data, "Out of stock products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting out of stock products");
                return StatusCode(500, ApiResponseDto<IEnumerable<ProductSummaryDto>>.ErrorResult("An error occurred while processing your request"));
            }
        }

        /// <summary>
        /// Check stock availability
        /// </summary>
        [HttpGet("{id}/stock-availability")]
        public async Task<ActionResult<ApiResponseDto<bool>>> CheckStockAvailability(
            int id,
            [FromQuery] int quantity,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productService.CheckStockAvailabilityAsync(id, quantity, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResponseDto<bool>.ErrorResult(result.ErrorMessage));
                }

                return Ok(ApiResponseDto<bool>.SuccessResult(result.Data, "Stock availability checked successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking stock availability for product {ProductId}", id);
                return StatusCode(500, ApiResponseDto<bool>.ErrorResult("An error occurred while checking stock availability"));
            }
        }

        #endregion

        #region Utility Endpoints

        /// <summary>
        /// Check if SKU is available
        /// </summary>
        [HttpGet("sku-available")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<bool>>> CheckSkuAvailability(
            [FromQuery] string sku,
            [FromQuery] int? excludeProductId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productService.IsSkuAvailableAsync(sku, excludeProductId, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResponseDto<bool>.ErrorResult(result.ErrorMessage));
                }

                return Ok(ApiResponseDto<bool>.SuccessResult(result.Data, "SKU availability checked successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking SKU availability: {SKU}", sku);
                return StatusCode(500, ApiResponseDto<bool>.ErrorResult("An error occurred while checking SKU availability"));
            }
        }

        /// <summary>
        /// Check if slug is available
        /// </summary>
        [HttpGet("slug-available")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<bool>>> CheckSlugAvailability(
            [FromQuery] string slug,
            [FromQuery] int? excludeProductId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productService.IsSlugAvailableAsync(slug, excludeProductId, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResponseDto<bool>.ErrorResult(result.ErrorMessage));
                }

                return Ok(ApiResponseDto<bool>.SuccessResult(result.Data, "Slug availability checked successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking slug availability: {Slug}", slug);
                return StatusCode(500, ApiResponseDto<bool>.ErrorResult("An error occurred while checking slug availability"));
            }
        }

        #endregion
    }
}