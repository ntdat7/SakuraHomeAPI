using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Common
{
    /// <summary>
    /// Generic paged result for API responses
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// Product tag information
    /// </summary>
    public class TagDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Color { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Brand summary for product listings
    /// </summary>
    public class BrandSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Detailed brand information
    /// </summary>
    public class BrandDetailDto : BrandSummaryDto
    {
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? Country { get; set; }
        public DateTime? FoundedYear { get; set; }
        public string? Headquarters { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsVerified { get; set; }
        public int ProductCount { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    /// <summary>
    /// Category summary for product listings
    /// </summary>
    public class CategorySummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int? ParentId { get; set; }
    }

    /// <summary>
    /// Detailed category information
    /// </summary>
    public class CategoryDetailDto : CategorySummaryDto
    {
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public int Level { get; set; }
        public int DisplayOrder { get; set; }
        public int ProductCount { get; set; }
        public int TotalProductCount { get; set; }
        public bool ShowInMenu { get; set; }
        public bool ShowInHome { get; set; }
        public List<CategorySummaryDto> Children { get; set; } = new();
    }

    /// <summary>
    /// Review summary for product details
    /// </summary>
    public class ReviewSummaryDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string? Comment { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int HelpfulCount { get; set; }
        public bool HasImages { get; set; }
        public bool IsVerifiedPurchase { get; set; }
    }
}