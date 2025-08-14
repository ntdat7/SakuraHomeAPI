using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.Models.Entities.Catalog;
using SakuraHomeAPI.DTOs.Common;
using AutoMapper;
using System.ComponentModel.DataAnnotations;

namespace SakuraHomeAPI.Controllers
{
    /// <summary>
    /// Brand management controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BrandController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BrandController> _logger;
        private readonly IMapper _mapper;

        public BrandController(
            ApplicationDbContext context,
            ILogger<BrandController> logger,
            IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all brands
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<BrandDto>>>> GetBrands([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Brands
                    .Where(b => b.IsActive && !b.IsDeleted)
                    .OrderBy(b => b.DisplayOrder)
                    .ThenBy(b => b.Name);

                var totalItems = await query.CountAsync();
                var brands = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var brandDtos = brands.Select(b => new BrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    LogoUrl = b.LogoUrl,
                    Country = b.Country,
                    Website = b.Website,
                    IsVerified = b.IsVerified,
                    IsOfficial = b.IsOfficial,
                    IsFeatured = b.IsFeatured,
                    ProductCount = b.ProductCount,
                    AverageRating = b.AverageRating,
                    FoundedYear = b.FoundedYear?.Year,
                    Headquarters = b.Headquarters,
                    Slug = b.Slug,
                    DisplayOrder = b.DisplayOrder
                }).ToList();

                return Ok(ApiResponseDto<List<BrandDto>>.SuccessResult(
                    brandDtos, 
                    $"Retrieved {brandDtos.Count} brands successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting brands");
                return StatusCode(500, ApiResponseDto<List<BrandDto>>.ErrorResult(
                    "An error occurred while retrieving brands"));
            }
        }

        /// <summary>
        /// Get brand by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<BrandDto>>> GetBrand(int id)
        {
            try
            {
                var brand = await _context.Brands
                    .Include(b => b.Products.Where(p => p.IsActive && !p.IsDeleted))
                    .FirstOrDefaultAsync(b => b.Id == id && b.IsActive && !b.IsDeleted);

                if (brand == null)
                {
                    return NotFound(ApiResponseDto<BrandDto>.ErrorResult("Brand not found"));
                }

                var brandDto = new BrandDto
                {
                    Id = brand.Id,
                    Name = brand.Name,
                    Description = brand.Description,
                    LogoUrl = brand.LogoUrl,
                    Country = brand.Country,
                    Website = brand.Website,
                    ContactEmail = brand.ContactEmail,
                    ContactPhone = brand.ContactPhone,
                    IsVerified = brand.IsVerified,
                    IsOfficial = brand.IsOfficial,
                    IsFeatured = brand.IsFeatured,
                    ProductCount = brand.ProductCount,
                    AverageRating = brand.AverageRating,
                    ReviewCount = brand.ReviewCount,
                    FoundedYear = brand.FoundedYear?.Year,
                    Headquarters = brand.Headquarters,
                    FacebookUrl = brand.FacebookUrl,
                    InstagramUrl = brand.InstagramUrl,
                    TwitterUrl = brand.TwitterUrl,
                    YoutubeUrl = brand.YoutubeUrl,
                    Slug = brand.Slug,
                    DisplayOrder = brand.DisplayOrder
                };

                return Ok(ApiResponseDto<BrandDto>.SuccessResult(brandDto, "Brand retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting brand {BrandId}", id);
                return StatusCode(500, ApiResponseDto<BrandDto>.ErrorResult(
                    "An error occurred while retrieving the brand"));
            }
        }

        /// <summary>
        /// Get featured brands
        /// </summary>
        [HttpGet("featured")]
        public async Task<ActionResult<ApiResponseDto<List<BrandDto>>>> GetFeaturedBrands()
        {
            try
            {
                var brands = await _context.Brands
                    .Where(b => b.IsFeatured && b.IsActive && !b.IsDeleted)
                    .OrderBy(b => b.DisplayOrder)
                    .ThenBy(b => b.Name)
                    .ToListAsync();

                var brandDtos = brands.Select(b => new BrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    LogoUrl = b.LogoUrl,
                    Country = b.Country,
                    IsVerified = b.IsVerified,
                    IsOfficial = b.IsOfficial,
                    ProductCount = b.ProductCount,
                    AverageRating = b.AverageRating,
                    Slug = b.Slug
                }).ToList();

                return Ok(ApiResponseDto<List<BrandDto>>.SuccessResult(
                    brandDtos, 
                    $"Retrieved {brandDtos.Count} featured brands successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting featured brands");
                return StatusCode(500, ApiResponseDto<List<BrandDto>>.ErrorResult(
                    "An error occurred while retrieving featured brands"));
            }
        }

        /// <summary>
        /// Create a new brand
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<BrandDto>>> CreateBrand([FromBody] CreateBrandDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(ApiResponseDto<BrandDto>.ErrorResult("Brand name is required"));
                }

                // Check if brand name already exists
                var existingBrand = await _context.Brands
                    .AnyAsync(b => b.Name == request.Name && !b.IsDeleted);

                if (existingBrand)
                {
                    return BadRequest(ApiResponseDto<BrandDto>.ErrorResult("A brand with this name already exists"));
                }

                var brand = new Brand
                {
                    Name = request.Name,
                    Description = request.Description,
                    LogoUrl = request.LogoUrl,
                    Country = request.Country ?? "Japan",
                    Website = request.Website,
                    ContactEmail = request.ContactEmail,
                    ContactPhone = request.ContactPhone,
                    FoundedYear = request.FoundedYear.HasValue ? new DateTime(request.FoundedYear.Value, 1, 1) : null,
                    Headquarters = request.Headquarters,
                    IsFeatured = request.IsFeatured,
                    IsVerified = false,
                    IsOfficial = false,
                    DisplayOrder = request.DisplayOrder,
                    Slug = GenerateSlug(request.Name),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Brand created successfully with ID {BrandId}", brand.Id);

                var brandDto = new BrandDto
                {
                    Id = brand.Id,
                    Name = brand.Name,
                    Description = brand.Description,
                    LogoUrl = brand.LogoUrl,
                    Country = brand.Country,
                    Website = brand.Website,
                    IsFeatured = brand.IsFeatured,
                    DisplayOrder = brand.DisplayOrder,
                    Slug = brand.Slug,
                    IsActive = brand.IsActive
                };

                return CreatedAtAction(
                    nameof(GetBrand),
                    new { id = brand.Id },
                    ApiResponseDto<BrandDto>.SuccessResult(brandDto, "Brand created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating brand");
                return StatusCode(500, ApiResponseDto<BrandDto>.ErrorResult(
                    "An error occurred while creating the brand"));
            }
        }

        /// <summary>
        /// Update an existing brand
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<BrandDto>>> UpdateBrand(int id, [FromBody] UpdateBrandDto request)
        {
            try
            {
                var brand = await _context.Brands
                    .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

                if (brand == null)
                {
                    return NotFound(ApiResponseDto<BrandDto>.ErrorResult("Brand not found"));
                }

                // Check if new name conflicts with existing brands (excluding current brand)
                if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != brand.Name)
                {
                    var existingBrand = await _context.Brands
                        .AnyAsync(b => b.Name == request.Name && b.Id != id && !b.IsDeleted);

                    if (existingBrand)
                    {
                        return BadRequest(ApiResponseDto<BrandDto>.ErrorResult("A brand with this name already exists"));
                    }
                }

                // Update brand fields
                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    brand.Name = request.Name;
                    brand.Slug = GenerateSlug(request.Name);
                }

                if (request.Description != null)
                    brand.Description = request.Description;

                if (request.LogoUrl != null)
                    brand.LogoUrl = request.LogoUrl;

                if (!string.IsNullOrWhiteSpace(request.Country))
                    brand.Country = request.Country;

                if (request.Website != null)
                    brand.Website = request.Website;

                if (request.ContactEmail != null)
                    brand.ContactEmail = request.ContactEmail;

                if (request.ContactPhone != null)
                    brand.ContactPhone = request.ContactPhone;

                if (request.FoundedYear.HasValue)
                    brand.FoundedYear = new DateTime(request.FoundedYear.Value, 1, 1);

                if (request.Headquarters != null)
                    brand.Headquarters = request.Headquarters;

                if (request.FacebookUrl != null)
                    brand.FacebookUrl = request.FacebookUrl;

                if (request.InstagramUrl != null)
                    brand.InstagramUrl = request.InstagramUrl;

                if (request.TwitterUrl != null)
                    brand.TwitterUrl = request.TwitterUrl;

                if (request.YoutubeUrl != null)
                    brand.YoutubeUrl = request.YoutubeUrl;

                brand.IsFeatured = request.IsFeatured;
                brand.IsVerified = request.IsVerified;
                brand.IsOfficial = request.IsOfficial;
                brand.DisplayOrder = request.DisplayOrder;
                brand.IsActive = request.IsActive;
                brand.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Brand {BrandId} updated successfully", id);

                var brandDto = new BrandDto
                {
                    Id = brand.Id,
                    Name = brand.Name,
                    Description = brand.Description,
                    LogoUrl = brand.LogoUrl,
                    Country = brand.Country,
                    Website = brand.Website,
                    ContactEmail = brand.ContactEmail,
                    ContactPhone = brand.ContactPhone,
                    IsVerified = brand.IsVerified,
                    IsOfficial = brand.IsOfficial,
                    IsFeatured = brand.IsFeatured,
                    ProductCount = brand.ProductCount,
                    AverageRating = brand.AverageRating,
                    ReviewCount = brand.ReviewCount,
                    FoundedYear = brand.FoundedYear?.Year,
                    Headquarters = brand.Headquarters,
                    FacebookUrl = brand.FacebookUrl,
                    InstagramUrl = brand.InstagramUrl,
                    TwitterUrl = brand.TwitterUrl,
                    YoutubeUrl = brand.YoutubeUrl,
                    Slug = brand.Slug,
                    DisplayOrder = brand.DisplayOrder,
                    IsActive = brand.IsActive
                };

                return Ok(ApiResponseDto<BrandDto>.SuccessResult(brandDto, "Brand updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating brand {BrandId}", id);
                return StatusCode(500, ApiResponseDto<BrandDto>.ErrorResult(
                    "An error occurred while updating the brand"));
            }
        }

        /// <summary>
        /// Delete a brand (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto>> DeleteBrand(int id)
        {
            try
            {
                var brand = await _context.Brands
                    .Include(b => b.Products)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (brand == null)
                {
                    return NotFound(ApiResponseDto.ErrorResult("Brand not found"));
                }

                // Check if brand has products
                if (brand.Products.Any(p => !p.IsDeleted))
                {
                    return BadRequest(ApiResponseDto.ErrorResult(
                        "Cannot delete brand that has products. Please move or delete products first."));
                }

                // Soft delete
                brand.IsDeleted = true;
                brand.DeletedAt = DateTime.UtcNow;
                brand.IsActive = false;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Brand {BrandId} deleted successfully", id);

                return Ok(ApiResponseDto.SuccessResult("Brand deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting brand {BrandId}", id);
                return StatusCode(500, ApiResponseDto.ErrorResult(
                    "An error occurred while deleting the brand"));
            }
        }

        #region Private Methods

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

    #region DTOs

    public class BrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string Country { get; set; } = "Japan";
        public string? Website { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public int? FoundedYear { get; set; }
        public string? Headquarters { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsVerified { get; set; }
        public bool IsOfficial { get; set; }
        public int ProductCount { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? YoutubeUrl { get; set; }
        public string Slug { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateBrandDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; } = "Japan";

        [MaxLength(500)]
        public string? Website { get; set; }

        [MaxLength(255)]
        public string? ContactEmail { get; set; }

        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        public int? FoundedYear { get; set; }

        [MaxLength(500)]
        public string? Headquarters { get; set; }

        public bool IsFeatured { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
    }

    public class UpdateBrandDto
    {
        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(500)]
        public string? Website { get; set; }

        [MaxLength(255)]
        public string? ContactEmail { get; set; }

        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        public int? FoundedYear { get; set; }

        [MaxLength(500)]
        public string? Headquarters { get; set; }

        [MaxLength(500)]
        public string? FacebookUrl { get; set; }

        [MaxLength(500)]
        public string? InstagramUrl { get; set; }

        [MaxLength(500)]
        public string? TwitterUrl { get; set; }

        [MaxLength(500)]
        public string? YoutubeUrl { get; set; }

        public bool IsFeatured { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public bool IsOfficial { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    #endregion
}