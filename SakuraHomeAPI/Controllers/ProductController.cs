using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.DTOs.Products.Requests;
using SakuraHomeAPI.DTOs.Products.Responses;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.DTOs.Products.Components;
using AutoMapper;

namespace SakuraHomeAPI.Controllers
{
    /// <summary>
    /// Product management controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductController> _logger;
        private readonly IMapper _mapper;

        public ProductController(
            ApplicationDbContext context,
            ILogger<ProductController> logger,
            IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Debug endpoint to check database
        /// </summary>
        [HttpGet("debug")]
        public async Task<ActionResult> GetDebugInfo()
        {
            try
            {
                var totalProducts = await _context.Products.CountAsync();
                var activeProducts = await _context.Products.CountAsync(p => p.IsActive && !p.IsDeleted);
                var allProducts = await _context.Products.ToListAsync();

                // Also check other seed data
                var totalCategories = await _context.Categories.CountAsync();
                var totalBrands = await _context.Brands.CountAsync();
                var totalSettings = await _context.SystemSettings.CountAsync();

                var debugInfo = new
                {
                    TotalProducts = totalProducts,
                    ActiveProducts = activeProducts,
                    TotalCategories = totalCategories,
                    TotalBrands = totalBrands,
                    TotalSettings = totalSettings,
                    AllProductIds = allProducts.Select(p => new { p.Id, p.Name, p.IsActive, p.IsDeleted }).ToList(),
                    DatabaseProviderName = _context.Database.ProviderName,
                    CanConnect = await _context.Database.CanConnectAsync(),
                    ConnectionString = _context.Database.GetConnectionString(),
                    AppliedMigrations = await _context.Database.GetAppliedMigrationsAsync(),
                    PendingMigrations = await _context.Database.GetPendingMigrationsAsync()
                };

                return Ok(debugInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in debug endpoint");
                return StatusCode(500, new { Error = ex.Message, InnerException = ex.InnerException?.Message });
            }
        }

        /// <summary>
        /// Get all products with filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ProductListResponseDto>> GetProducts([FromQuery] ProductFilterRequestDto request)
        {
            try
            {
                // Simple validation
                if (request.Page < 1) request.Page = 1;
                if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 20;

                var query = _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages.Where(pi => pi.IsActive))
                    .Where(p => p.IsActive && !p.IsDeleted);

                // Apply filters
                query = ApplyFilters(query, request);

                // Apply sorting
                query = ApplySorting(query, request.SortBy, request.SortOrder);

                // Get total count for pagination
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);

                // Apply pagination
                var products = await query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                // Map to DTOs - use simple mapping for now
                var productDtos = products.Select(p => new ProductSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug,
                    ShortDescription = p.ShortDescription,
                    MainImage = p.MainImage,
                    Price = p.Price,
                    OriginalPrice = p.OriginalPrice,
                    Stock = p.Stock,
                    Status = p.Status,
                    Condition = p.Condition,
                    Rating = p.Rating,
                    ReviewCount = p.ReviewCount,
                    ViewCount = p.ViewCount,
                    SoldCount = p.SoldCount,
                    IsInStock = p.Stock > 0 || p.AllowBackorder,
                    IsOnSale = p.OriginalPrice.HasValue && p.OriginalPrice > p.Price,
                    IsFeatured = p.IsFeatured,
                    IsNew = p.IsNew,
                    IsBestseller = p.IsBestseller,
                    IsLimitedEdition = p.IsLimitedEdition,
                    IsGiftWrappingAvailable = p.IsGiftWrappingAvailable,
                    AllowBackorder = p.AllowBackorder,
                    AllowPreorder = p.AllowPreorder,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Brand = p.Brand != null ? new BrandSummaryDto
                    {
                        Id = p.Brand.Id,
                        Name = p.Brand.Name,
                        Slug = p.Brand.Slug,
                        LogoUrl = p.Brand.LogoUrl,
                        IsActive = p.Brand.IsActive
                    } : null,
                    Category = p.Category != null ? new CategorySummaryDto
                    {
                        Id = p.Category.Id,
                        Name = p.Category.Name,
                        Slug = p.Category.Slug,
                        ImageUrl = p.Category.ImageUrl,
                        IsActive = p.Category.IsActive,
                        ParentId = p.Category.ParentId
                    } : null,
                    // Add images for product list (optional - might want to limit to main image only)
                    Images = p.ProductImages?.Where(pi => pi.IsActive).Select(pi => new SakuraHomeAPI.DTOs.Products.ProductImageDto
                    {
                        Id = pi.Id,
                        ImageUrl = pi.ImageUrl,
                        AltText = pi.AltText,
                        DisplayOrder = pi.DisplayOrder,
                        IsMain = pi.IsMain
                    }).OrderBy(pi => pi.DisplayOrder).ToList() ?? new List<SakuraHomeAPI.DTOs.Products.ProductImageDto>()
                }).ToList();

                // Create response directly without ApiResponseDto wrapper
                var response = new ProductListResponseDto
                {
                    Data = productDtos,
                    Pagination = new PaginationResponseDto
                    {
                        CurrentPage = request.Page,
                        PageSize = request.PageSize,
                        TotalItems = totalItems,
                        TotalPages = totalPages,
                        HasNext = request.Page < totalPages,
                        HasPrevious = request.Page > 1
                    },
                    Success = true,
                    Message = "Products retrieved successfully"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting products with filters: {@Filters}", request);

                // Return error response in same format
                var errorResponse = new ProductListResponseDto
                {
                    Data = new List<ProductSummaryDto>(),
                    Pagination = new PaginationResponseDto(),
                    Success = false,
                    Message = "An error occurred while processing your request"
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Get product by ID with full details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<ProductDetailDto>>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages.Where(pi => pi.IsActive).OrderBy(pi => pi.DisplayOrder))
                    .Include(p => p.Variants.Where(v => v.IsActive && !v.IsDeleted))
                    .Include(p => p.Reviews.Where(r => r.IsActive && !r.IsDeleted))
                        .ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(p => p.Id == id && p.IsActive && !p.IsDeleted);

                if (product == null)
                {
                    return NotFound(ApiResponseDto<ProductDetailDto>.ErrorResult("Product not found"));
                }

                // Record product view
                await RecordProductView(id);

                // Simple mapping for now
                var productDto = new ProductDetailDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Slug = product.Slug,
                    ShortDescription = product.ShortDescription,
                    Description = product.Description,
                    MainImage = product.MainImage,
                    Price = product.Price,
                    OriginalPrice = product.OriginalPrice,
                    Stock = product.Stock,
                    Status = product.Status,
                    Condition = product.Condition,
                    Rating = product.Rating,
                    ReviewCount = product.ReviewCount,
                    ViewCount = product.ViewCount,
                    SoldCount = product.SoldCount,
                    IsInStock = product.Stock > 0 || product.AllowBackorder,
                    IsOnSale = product.OriginalPrice.HasValue && product.OriginalPrice > product.Price,
                    IsFeatured = product.IsFeatured,
                    IsNew = product.IsNew,
                    IsBestseller = product.IsBestseller,
                    IsLimitedEdition = product.IsLimitedEdition,
                    IsGiftWrappingAvailable = product.IsGiftWrappingAvailable,
                    AllowBackorder = product.AllowBackorder,
                    AllowPreorder = product.AllowPreorder,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,
                    Origin = product.Origin,
                    JapaneseRegion = product.JapaneseRegion,
                    AuthenticityLevel = product.AuthenticityLevel,
                    Weight = product.Weight,
                    WeightUnit = product.WeightUnit,
                    Length = product.Length,
                    Width = product.Width,
                    Height = product.Height,
                    DimensionUnit = product.DimensionUnit,
                    TrackInventory = product.TrackInventory,
                    Brand = product.Brand != null ? new BrandSummaryDto
                    {
                        Id = product.Brand.Id,
                        Name = product.Brand.Name,
                        Slug = product.Brand.Slug,
                        LogoUrl = product.Brand.LogoUrl,
                        IsActive = product.Brand.IsActive
                    } : null,
                    Category = product.Category != null ? new CategorySummaryDto
                    {
                        Id = product.Category.Id,
                        Name = product.Category.Name,
                        Slug = product.Category.Slug,
                        ImageUrl = product.Category.ImageUrl,
                        IsActive = product.Category.IsActive,
                        ParentId = product.Category.ParentId
                    } : null,
                    // Add the images mapping that was missing
                    Images = product.ProductImages?.Where(pi => pi.IsActive).Select(pi => new SakuraHomeAPI.DTOs.Products.ProductImageDto
                    {
                        Id = pi.Id,
                        ImageUrl = pi.ImageUrl,
                        AltText = pi.AltText,
                        DisplayOrder = pi.DisplayOrder,
                        IsMain = pi.IsMain,
                        Caption = pi.Caption
                    }).OrderBy(pi => pi.DisplayOrder).ToList() ?? new List<SakuraHomeAPI.DTOs.Products.ProductImageDto>()
                };

                return Ok(ApiResponseDto<ProductDetailDto>.SuccessResult(productDto, "Product retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting product {ProductId}", id);
                return StatusCode(500, ApiResponseDto<ProductDetailDto>.ErrorResult("An error occurred while processing your request"));
            }
        }

        /// <summary>
        /// Get product by SKU
        /// </summary>
        [HttpGet("sku/{sku}")]
        public async Task<ActionResult<ApiResponseDto<ProductDetailDto>>> GetProductBySku(string sku)
        {
            try
            {
                var product = await _context.Products
                    .Where(p => p.SKU == sku && p.IsActive && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    return NotFound(ApiResponseDto<ProductDetailDto>.ErrorResult("Product not found"));
                }

                return await GetProduct(product.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting product by SKU {SKU}", sku);
                return StatusCode(500, ApiResponseDto<ProductDetailDto>.ErrorResult("An error occurred while processing your request"));
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<ProductDetailDto>>> CreateProduct([FromBody] CreateProductRequestDto request)
        {
            try
            {
                // Simple mapping for now
                var product = new Product
                {
                    Name = request.Name,
                    Slug = GenerateSlug(request.Name), // Generate slug from name
                    ShortDescription = request.ShortDescription,
                    Description = request.Description,
                    MainImage = request.MainImage,
                    Price = request.Price,
                    OriginalPrice = request.OriginalPrice,
                    Stock = request.Stock,
                    Status = request.Status,
                    Condition = request.Condition,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    BrandId = request.BrandId,
                    CategoryId = request.CategoryId,
                    SKU = request.SKU,
                    Tags = request.Tags
                };

                // Add to context
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product created successfully with ID {ProductId}", product.Id);

                // Get created product with full details
                var createdProduct = await _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages.Where(pi => pi.IsActive).OrderBy(pi => pi.DisplayOrder))
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                var productDto = new ProductDetailDto
                {
                    Id = createdProduct.Id,
                    Name = createdProduct.Name,
                    Slug = createdProduct.Slug,
                    ShortDescription = createdProduct.ShortDescription,
                    Description = createdProduct.Description,
                    MainImage = createdProduct.MainImage,
                    Price = createdProduct.Price,
                    OriginalPrice = createdProduct.OriginalPrice,
                    Stock = createdProduct.Stock,
                    Status = createdProduct.Status,
                    Condition = createdProduct.Condition,
                    Rating = createdProduct.Rating,
                    ReviewCount = createdProduct.ReviewCount,
                    ViewCount = createdProduct.ViewCount,
                    SoldCount = createdProduct.SoldCount,
                    IsInStock = createdProduct.Stock > 0 || createdProduct.AllowBackorder,
                    IsOnSale = createdProduct.OriginalPrice.HasValue && createdProduct.OriginalPrice > createdProduct.Price,
                    IsFeatured = createdProduct.IsFeatured,
                    IsNew = createdProduct.IsNew,
                    IsBestseller = createdProduct.IsBestseller,
                    IsLimitedEdition = createdProduct.IsLimitedEdition,
                    IsGiftWrappingAvailable = createdProduct.IsGiftWrappingAvailable,
                    AllowBackorder = createdProduct.AllowBackorder,
                    AllowPreorder = createdProduct.AllowPreorder,
                    CreatedAt = createdProduct.CreatedAt,
                    UpdatedAt = createdProduct.UpdatedAt,
                    Origin = createdProduct.Origin,
                    JapaneseRegion = createdProduct.JapaneseRegion,
                    AuthenticityLevel = createdProduct.AuthenticityLevel,
                    Weight = createdProduct.Weight,
                    WeightUnit = createdProduct.WeightUnit,
                    Length = createdProduct.Length,
                    Width = createdProduct.Width,
                    Height = createdProduct.Height,
                    DimensionUnit = createdProduct.DimensionUnit,
                    TrackInventory = createdProduct.TrackInventory,
                    Brand = createdProduct.Brand != null ? new BrandSummaryDto
                    {
                        Id = createdProduct.Brand.Id,
                        Name = createdProduct.Brand.Name,
                        Slug = createdProduct.Brand.Slug,
                        LogoUrl = createdProduct.Brand.LogoUrl,
                        IsActive = createdProduct.Brand.IsActive
                    } : null,
                    Category = createdProduct.Category != null ? new CategorySummaryDto
                    {
                        Id = createdProduct.Category.Id,
                        Name = createdProduct.Category.Name,
                        Slug = createdProduct.Category.Slug,
                        ImageUrl = createdProduct.Category.ImageUrl,
                        IsActive = createdProduct.Category.IsActive,
                        ParentId = createdProduct.Category.ParentId
                    } : null,
                    // Add images mapping for created product
                    Images = createdProduct.ProductImages?.Where(pi => pi.IsActive).Select(pi => new SakuraHomeAPI.DTOs.Products.ProductImageDto
                    {
                        Id = pi.Id,
                        ImageUrl = pi.ImageUrl,
                        AltText = pi.AltText,
                        DisplayOrder = pi.DisplayOrder,
                        IsMain = pi.IsMain,
                        Caption = pi.Caption
                    }).OrderBy(pi => pi.DisplayOrder).ToList() ?? new List<SakuraHomeAPI.DTOs.Products.ProductImageDto>()
                };

                return CreatedAtAction(
                    nameof(GetProduct),
                    new { id = product.Id },
                    ApiResponseDto<ProductDetailDto>.SuccessResult(productDto, "Product created successfully"));
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
        public async Task<ActionResult<ApiResponseDto<ProductDetailDto>>> UpdateProduct(int id, [FromBody] UpdateProductRequestDto request)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(ApiResponseDto<ProductDetailDto>.ErrorResult("Product not found"));
                }

                // Map updates to entity - simple mapping for now
                product.Name = request.Name;
                product.Slug = GenerateSlug(request.Name); // Generate slug from name
                product.ShortDescription = request.ShortDescription;
                product.Description = request.Description;
                product.MainImage = request.MainImage;
                product.Price = request.Price;
                product.OriginalPrice = request.OriginalPrice;
                product.Stock = request.Stock;
                product.Status = request.Status;
                product.Condition = request.Condition;
                product.IsFeatured = request.IsFeatured;
                product.IsNew = request.IsNew;
                product.IsBestseller = request.IsBestseller;
                product.IsLimitedEdition = request.IsLimitedEdition;
                product.IsGiftWrappingAvailable = request.IsGiftWrappingAvailable;
                product.AllowBackorder = request.AllowBackorder;
                product.AllowPreorder = request.AllowPreorder;
                product.UpdatedAt = DateTime.UtcNow;
                product.BrandId = request.BrandId;
                product.CategoryId = request.CategoryId;
                product.SKU = request.SKU;
                product.Tags = request.Tags;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} updated successfully", id);

                // Get updated product with full details
                var result = await GetProduct(id);
                if (result.Result is OkObjectResult okResult)
                {
                    return Ok(okResult.Value);
                }

                return result;
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
        public async Task<ActionResult<ApiResponseDto>> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(ApiResponseDto.ErrorResult("Product not found"));
                }

                // Soft delete
                product.IsDeleted = true;
                product.DeletedAt = DateTime.UtcNow;
                product.IsActive = false;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} deleted successfully", id);

                return Ok(ApiResponseDto.SuccessResult("Product deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting product {ProductId}", id);
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while deleting the product"));
            }
        }

        /// <summary>
        /// Update product stock
        /// </summary>
        [HttpPatch("{id}/stock")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<object>>> UpdateStock(int id, [FromBody] UpdateStockRequestDto request)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(ApiResponseDto<object>.ErrorResult("Product not found"));
                }

                var oldStock = product.Stock;
                product.Stock = request.NewStock;
                product.UpdatedAt = DateTime.UtcNow;

                // Create inventory log
                var inventoryLog = new InventoryLog
                {
                    ProductId = id,
                    Quantity = request.NewStock - oldStock,
                    PreviousStock = oldStock,
                    NewStock = request.NewStock,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.InventoryLogs.Add(inventoryLog);
                await _context.SaveChangesAsync();

                // Check if low stock
                var isLowStock = product.MinStock.HasValue && product.Stock <= product.MinStock.Value && product.TrackInventory;

                _logger.LogInformation("Stock updated for product {ProductId} from {OldStock} to {NewStock}",
                    id, oldStock, request.NewStock);

                var response = new
                {
                    message = "Stock updated successfully",
                    oldStock,
                    newStock = request.NewStock,
                    isLowStock,
                    stockChange = $"{oldStock} ? {request.NewStock}",
                    inventoryLogId = inventoryLog.Id
                };

                return Ok(ApiResponseDto<object>.SuccessResult(response, "Stock updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating stock for product {ProductId}", id);
                return StatusCode(500, ApiResponseDto<object>.ErrorResult("An error occurred while updating stock"));
            }
        }

        #region Private Methods

        private IQueryable<Product> ApplyFilters(IQueryable<Product> query, ProductFilterRequestDto request)
        {
            if (request.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);

            if (request.BrandId.HasValue)
                query = query.Where(p => p.BrandId == request.BrandId.Value);

            if (request.MinPrice.HasValue)
                query = query.Where(p => p.Price >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= request.MaxPrice.Value);

            if (request.InStockOnly)
                query = query.Where(p => p.Stock > 0 || p.AllowBackorder);

            if (request.OnSaleOnly)
                query = query.Where(p => p.OriginalPrice.HasValue && p.OriginalPrice > p.Price);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchTerm = request.Search.ToLower().Trim();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                    (p.Tags != null && p.Tags.ToLower().Contains(searchTerm)) ||
                    p.Brand.Name.ToLower().Contains(searchTerm) ||
                    p.Category.Name.ToLower().Contains(searchTerm));
            }

            if (request.FeaturedOnly)
                query = query.Where(p => p.IsFeatured);

            if (request.NewOnly)
                query = query.Where(p => p.IsNew);

            return query;
        }

        private IQueryable<Product> ApplySorting(IQueryable<Product> query, string sortBy, string sortOrder)
        {
            var isAscending = sortOrder.ToLower() == "asc";

            return sortBy.ToLower() switch
            {
                "name" => isAscending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "price" => isAscending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                "rating" => isAscending
                    ? query.OrderBy(p => p.Rating).ThenBy(p => p.ReviewCount)
                    : query.OrderByDescending(p => p.Rating).ThenByDescending(p => p.ReviewCount),
                "sold" => isAscending ? query.OrderBy(p => p.SoldCount) : query.OrderByDescending(p => p.SoldCount),
                "views" => isAscending ? query.OrderBy(p => p.ViewCount) : query.OrderByDescending(p => p.ViewCount),
                "stock" => isAscending ? query.OrderBy(p => p.Stock) : query.OrderByDescending(p => p.Stock),
                _ => isAscending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt)
            };
        }

        private async Task RecordProductView(int productId)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product != null)
                {
                    product.ViewCount++;
                    product.LastViewedAt = DateTime.UtcNow;

                    // Also create a ProductView record for analytics
                    var productView = new ProductView
                    {
                        ProductId = productId,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                        LastViewedAt = DateTime.UtcNow
                    };

                    _context.ProductViews.Add(productView);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to record product view for product {ProductId}", productId);
                // Don't throw as this is not critical
            }
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

        #endregion
    }
}