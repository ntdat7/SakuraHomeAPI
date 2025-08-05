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
    /// Category management controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryController> _logger;
        private readonly IMapper _mapper;

        public CategoryController(
            ApplicationDbContext context,
            ILogger<CategoryController> logger,
            IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all categories with hierarchy
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<CategoryDto>>>> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Include(c => c.Children)
                    .Where(c => c.IsActive && !c.IsDeleted)
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .ToListAsync();

                var categoryDtos = categories.Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    Icon = c.Icon,
                    Color = c.Color,
                    ParentId = c.ParentId,
                    Level = c.Level,
                    IsFeatured = c.IsFeatured,
                    ShowInMenu = c.ShowInMenu,
                    ProductCount = c.ProductCount,
                    DisplayOrder = c.DisplayOrder,
                    Slug = c.Slug,
                    IsActive = c.IsActive,
                    ChildrenCount = c.ChildrenCount,
                    FullPath = c.FullPath
                }).ToList();

                return Ok(ApiResponseDto<List<CategoryDto>>.SuccessResult(
                    categoryDtos, 
                    $"Retrieved {categoryDtos.Count} categories successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting categories");
                return StatusCode(500, ApiResponseDto<List<CategoryDto>>.ErrorResult(
                    "An error occurred while retrieving categories"));
            }
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<CategoryDto>>> GetCategory(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Parent)
                    .Include(c => c.Children)
                    .Include(c => c.Products.Where(p => p.IsActive && !p.IsDeleted))
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsActive && !c.IsDeleted);

                if (category == null)
                {
                    return NotFound(ApiResponseDto<CategoryDto>.ErrorResult("Category not found"));
                }

                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    Icon = category.Icon,
                    Color = category.Color,
                    ParentId = category.ParentId,
                    Level = category.Level,
                    IsFeatured = category.IsFeatured,
                    ShowInMenu = category.ShowInMenu,
                    ProductCount = category.ProductCount,
                    DisplayOrder = category.DisplayOrder,
                    Slug = category.Slug,
                    IsActive = category.IsActive,
                    ChildrenCount = category.ChildrenCount,
                    FullPath = category.FullPath,
                    ParentName = category.Parent?.Name,
                    Children = category.Children.Select(child => new CategoryDto
                    {
                        Id = child.Id,
                        Name = child.Name,
                        Description = child.Description,
                        ProductCount = child.ProductCount,
                        DisplayOrder = child.DisplayOrder
                    }).ToList()
                };

                return Ok(ApiResponseDto<CategoryDto>.SuccessResult(categoryDto, "Category retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting category {CategoryId}", id);
                return StatusCode(500, ApiResponseDto<CategoryDto>.ErrorResult(
                    "An error occurred while retrieving the category"));
            }
        }

        /// <summary>
        /// Get root categories (for navigation menu)
        /// </summary>
        [HttpGet("root")]
        public async Task<ActionResult<ApiResponseDto<List<CategoryDto>>>> GetRootCategories()
        {
            try
            {
                var rootCategories = await _context.Categories
                    .Include(c => c.Children.Where(child => child.IsActive && !child.IsDeleted))
                    .Where(c => c.ParentId == null && c.IsActive && !c.IsDeleted && c.ShowInMenu)
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .ToListAsync();

                var categoryDtos = rootCategories.Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    Icon = c.Icon,
                    Color = c.Color,
                    IsFeatured = c.IsFeatured,
                    ProductCount = c.ProductCount,
                    DisplayOrder = c.DisplayOrder,
                    Slug = c.Slug,
                    ChildrenCount = c.ChildrenCount,
                    Children = c.Children.Select(child => new CategoryDto
                    {
                        Id = child.Id,
                        Name = child.Name,
                        ProductCount = child.ProductCount,
                        DisplayOrder = child.DisplayOrder,
                        Slug = child.Slug
                    }).OrderBy(x => x.DisplayOrder).ToList()
                }).ToList();

                return Ok(ApiResponseDto<List<CategoryDto>>.SuccessResult(
                    categoryDtos, 
                    $"Retrieved {categoryDtos.Count} root categories successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting root categories");
                return StatusCode(500, ApiResponseDto<List<CategoryDto>>.ErrorResult(
                    "An error occurred while retrieving root categories"));
            }
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<CategoryDto>>> CreateCategory([FromBody] CreateCategoryDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(ApiResponseDto<CategoryDto>.ErrorResult("Category name is required"));
                }

                // Check if category name already exists at the same level
                var existingCategory = await _context.Categories
                    .AnyAsync(c => c.Name == request.Name && c.ParentId == request.ParentId && !c.IsDeleted);

                if (existingCategory)
                {
                    return BadRequest(ApiResponseDto<CategoryDto>.ErrorResult(
                        "A category with this name already exists at this level"));
                }

                // Validate parent category if specified
                Category parentCategory = null;
                if (request.ParentId.HasValue)
                {
                    parentCategory = await _context.Categories.FindAsync(request.ParentId.Value);
                    if (parentCategory == null)
                    {
                        return BadRequest(ApiResponseDto<CategoryDto>.ErrorResult("Parent category not found"));
                    }
                }

                var category = new Category
                {
                    Name = request.Name,
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    Icon = request.Icon,
                    Color = request.Color,
                    ParentId = request.ParentId,
                    Level = parentCategory?.Level + 1 ?? 0,
                    IsFeatured = request.IsFeatured,
                    ShowInMenu = request.ShowInMenu,
                    ShowInHome = request.ShowInHome,
                    DisplayOrder = request.DisplayOrder,
                    Slug = GenerateSlug(request.Name),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                // Update parent's children count
                if (parentCategory != null)
                {
                    parentCategory.ChildrenCount = await _context.Categories
                        .CountAsync(c => c.ParentId == parentCategory.Id && !c.IsDeleted);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Category created successfully with ID {CategoryId}", category.Id);

                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    Icon = category.Icon,
                    Color = category.Color,
                    ParentId = category.ParentId,
                    Level = category.Level,
                    IsFeatured = category.IsFeatured,
                    ShowInMenu = category.ShowInMenu,
                    DisplayOrder = category.DisplayOrder,
                    Slug = category.Slug,
                    IsActive = category.IsActive,
                    ChildrenCount = category.ChildrenCount
                };

                return CreatedAtAction(
                    nameof(GetCategory),
                    new { id = category.Id },
                    ApiResponseDto<CategoryDto>.SuccessResult(categoryDto, "Category created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating category");
                return StatusCode(500, ApiResponseDto<CategoryDto>.ErrorResult(
                    "An error occurred while creating the category"));
            }
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<CategoryDto>>> UpdateCategory(int id, [FromBody] UpdateCategoryDto request)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Parent)
                    .Include(c => c.Children)
                    .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

                if (category == null)
                {
                    return NotFound(ApiResponseDto<CategoryDto>.ErrorResult("Category not found"));
                }

                // Check if new name conflicts with existing categories at the same level
                if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != category.Name)
                {
                    var existingCategory = await _context.Categories
                        .AnyAsync(c => c.Name == request.Name && c.ParentId == (request.ParentId ?? category.ParentId) && c.Id != id && !c.IsDeleted);

                    if (existingCategory)
                    {
                        return BadRequest(ApiResponseDto<CategoryDto>.ErrorResult("A category with this name already exists at this level"));
                    }
                }

                // Validate new parent category if changing parent
                Category newParentCategory = null;
                if (request.ParentId.HasValue && request.ParentId != category.ParentId)
                {
                    // Prevent setting self or descendant as parent
                    if (request.ParentId == id)
                    {
                        return BadRequest(ApiResponseDto<CategoryDto>.ErrorResult("Cannot set category as its own parent"));
                    }

                    // Check if new parent exists
                    newParentCategory = await _context.Categories.FindAsync(request.ParentId.Value);
                    if (newParentCategory == null)
                    {
                        return BadRequest(ApiResponseDto<CategoryDto>.ErrorResult("Parent category not found"));
                    }

                    // Check for circular reference (prevent setting descendant as parent)
                    var isDescendant = await IsDescendantOf(newParentCategory.Id, id);
                    if (isDescendant)
                    {
                        return BadRequest(ApiResponseDto<CategoryDto>.ErrorResult("Cannot set a descendant category as parent"));
                    }
                }

                var oldParentId = category.ParentId;

                // Update category fields
                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    category.Name = request.Name;
                    category.Slug = GenerateSlug(request.Name);
                }

                if (request.Description != null)
                    category.Description = request.Description;

                if (request.ImageUrl != null)
                    category.ImageUrl = request.ImageUrl;

                if (request.Icon != null)
                    category.Icon = request.Icon;

                if (request.Color != null)
                    category.Color = request.Color;

                // Handle parent change
                if (request.ParentId != category.ParentId)
                {
                    category.ParentId = request.ParentId;
                    category.Level = newParentCategory?.Level + 1 ?? 0;
                    
                    // Update full path for this category and all descendants
                    await UpdateCategoryHierarchy(category);
                }

                category.IsFeatured = request.IsFeatured;
                category.ShowInMenu = request.ShowInMenu;
                category.ShowInHome = request.ShowInHome;
                category.DisplayOrder = request.DisplayOrder;
                category.IsActive = request.IsActive;
                category.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Update children count for old and new parents
                if (oldParentId.HasValue)
                {
                    var oldParent = await _context.Categories.FindAsync(oldParentId.Value);
                    if (oldParent != null)
                    {
                        oldParent.ChildrenCount = await _context.Categories
                            .CountAsync(c => c.ParentId == oldParent.Id && !c.IsDeleted);
                    }
                }

                if (newParentCategory != null)
                {
                    newParentCategory.ChildrenCount = await _context.Categories
                        .CountAsync(c => c.ParentId == newParentCategory.Id && !c.IsDeleted);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Category {CategoryId} updated successfully", id);

                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    Icon = category.Icon,
                    Color = category.Color,
                    ParentId = category.ParentId,
                    Level = category.Level,
                    IsFeatured = category.IsFeatured,
                    ShowInMenu = category.ShowInMenu,
                    ProductCount = category.ProductCount,
                    DisplayOrder = category.DisplayOrder,
                    Slug = category.Slug,
                    IsActive = category.IsActive,
                    ChildrenCount = category.ChildrenCount,
                    FullPath = category.FullPath,
                    ParentName = category.Parent?.Name
                };

                return Ok(ApiResponseDto<CategoryDto>.SuccessResult(categoryDto, "Category updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating category {CategoryId}", id);
                return StatusCode(500, ApiResponseDto<CategoryDto>.ErrorResult(
                    "An error occurred while updating the category"));
            }
        }

        /// <summary>
        /// Delete a category (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto>> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Children)
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return NotFound(ApiResponseDto.ErrorResult("Category not found"));
                }

                // Check if category has products
                if (category.Products.Any(p => !p.IsDeleted))
                {
                    return BadRequest(ApiResponseDto.ErrorResult(
                        "Cannot delete category that contains products. Please move or delete products first."));
                }

                // Check if category has children
                if (category.Children.Any(c => !c.IsDeleted))
                {
                    return BadRequest(ApiResponseDto.ErrorResult(
                        "Cannot delete category that has subcategories. Please delete subcategories first."));
                }

                // Soft delete
                category.IsDeleted = true;
                category.DeletedAt = DateTime.UtcNow;
                category.IsActive = false;

                await _context.SaveChangesAsync();

                // Update parent's children count
                if (category.ParentId.HasValue)
                {
                    var parent = await _context.Categories.FindAsync(category.ParentId.Value);
                    if (parent != null)
                    {
                        parent.ChildrenCount = await _context.Categories
                            .CountAsync(c => c.ParentId == parent.Id && !c.IsDeleted);
                        await _context.SaveChangesAsync();
                    }
                }

                _logger.LogInformation("Category {CategoryId} deleted successfully", id);

                return Ok(ApiResponseDto.SuccessResult("Category deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting category {CategoryId}", id);
                return StatusCode(500, ApiResponseDto.ErrorResult(
                    "An error occurred while deleting the category"));
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

        private async Task<bool> IsDescendantOf(int potentialAncestorId, int categoryId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == potentialAncestorId);

            while (category?.ParentId != null)
            {
                if (category.ParentId == categoryId)
                    return true;
                
                category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == category.ParentId);
            }

            return false;
        }

        private async Task UpdateCategoryHierarchy(Category category)
        {
            // Update the current category's level
            var parent = category.Parent ?? (category.ParentId.HasValue 
                ? await _context.Categories.FindAsync(category.ParentId.Value) 
                : null);
            
            category.Level = parent?.Level + 1 ?? 0;

            // Update all descendants
            var descendants = await _context.Categories
                .Where(c => c.ParentId == category.Id && !c.IsDeleted)
                .ToListAsync();

            foreach (var descendant in descendants)
            {
                descendant.Level = category.Level + 1;
                await UpdateCategoryHierarchy(descendant);
            }
        }

        private async Task<string> BuildFullPath(Category category)
        {
            var path = new List<string>();
            var current = category;

            while (current != null)
            {
                path.Insert(0, current.Name);
                
                if (current.ParentId.HasValue)
                {
                    current = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Id == current.ParentId);
                }
                else
                {
                    break;
                }
            }

            return string.Join(" > ", path);
        }

        private async Task<int> CalculateLevel(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            int level = 0;

            while (category?.ParentId != null)
            {
                level++;
                category = await _context.Categories.FindAsync(category.ParentId);
            }

            return level;
        }

        #endregion
    }

    #region DTOs

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int Level { get; set; }
        public bool IsFeatured { get; set; }
        public bool ShowInMenu { get; set; }
        public int ProductCount { get; set; }
        public int DisplayOrder { get; set; }
        public string Slug { get; set; }
        public bool IsActive { get; set; }
        public int ChildrenCount { get; set; }
        public string FullPath { get; set; }
        public List<CategoryDto> Children { get; set; } = new();
    }

    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(100)]
        public string? Icon { get; set; }

        [MaxLength(7)]
        public string? Color { get; set; }

        public int? ParentId { get; set; }
        public bool IsFeatured { get; set; } = false;
        public bool ShowInMenu { get; set; } = true;
        public bool ShowInHome { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
    }

    public class UpdateCategoryDto
    {
        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(100)]
        public string? Icon { get; set; }

        [MaxLength(7)]
        public string? Color { get; set; }

        public int? ParentId { get; set; }
        public bool IsFeatured { get; set; } = false;
        public bool ShowInMenu { get; set; } = true;
        public bool ShowInHome { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    #endregion
}