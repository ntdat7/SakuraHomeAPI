using System.ComponentModel.DataAnnotations;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Products.Requests
{
    /// <summary>
    /// Filter products request
    /// </summary>
    public class FilterProductRequestDto : PaginationRequestDto
    {
        [MaxLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
        public string? Search { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Min price must be greater than or equal to 0")]
        public decimal? MinPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Max price must be greater than or equal to 0")]
        public decimal? MaxPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Min weight must be greater than or equal to 0")]
        public decimal? MinWeight { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Max weight must be greater than or equal to 0")]
        public decimal? MaxWeight { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Category ID must be greater than 0")]
        public int? CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Brand ID must be greater than 0")]
        public int? BrandId { get; set; }

        public List<int>? TagIds { get; set; }

        public ProductStatus? Status { get; set; }
        public ProductCondition? Condition { get; set; }
        public JapaneseOrigin? JapaneseRegion { get; set; }
        public AuthenticityLevel? AuthenticityLevel { get; set; }
        public AgeRestriction? AgeRestriction { get; set; }

        public bool? IsFeatured { get; set; }
        public bool? IsNew { get; set; }
        public bool? IsBestseller { get; set; }
        public bool? IsLimitedEdition { get; set; }
        public bool? IsGiftWrappingAvailable { get; set; }
        public bool? IsInStock { get; set; }
        public bool? IsOnSale { get; set; }
        public bool? AllowBackorder { get; set; }

        [Range(1, 5, ErrorMessage = "Min rating must be between 1 and 5")]
        public int? MinRating { get; set; }

        [MaxLength(20, ErrorMessage = "Sort by field cannot exceed 20 characters")]
        public string SortBy { get; set; } = "created";

        [MaxLength(4, ErrorMessage = "Sort order must be 'asc' or 'desc'")]
        [RegularExpression("^(asc|desc)$", ErrorMessage = "Sort order must be either 'asc' or 'desc'")]
        public string SortOrder { get; set; } = "desc";

        // Date filters
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableTo { get; set; }

        // Japanese specific filters
        public bool? IsJapaneseProduct { get; set; }
        public bool? IsAuthentic { get; set; }

        // Custom validation methods
        public bool IsValidPriceRange()
        {
            return !MinPrice.HasValue || !MaxPrice.HasValue || MinPrice <= MaxPrice;
        }

        public bool IsValidWeightRange()
        {
            return !MinWeight.HasValue || !MaxWeight.HasValue || MinWeight <= MaxWeight;
        }

        public bool IsValidDateRange()
        {
            return !CreatedFrom.HasValue || !CreatedTo.HasValue || CreatedFrom <= CreatedTo;
        }

        public bool IsValidAvailabilityRange()
        {
            return !AvailableFrom.HasValue || !AvailableTo.HasValue || AvailableFrom <= AvailableTo;
        }
    }
}