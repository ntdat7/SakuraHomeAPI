using FluentValidation;
using SakuraHomeAPI.DTOs.Products.Requests;

namespace SakuraHomeAPI.Validators.Products
{
    /// <summary>
    /// Comprehensive validator for ProductFilterRequestDto with enhanced validation rules
    /// </summary>
    public class ProductFilterRequestValidator : AbstractValidator<ProductFilterRequestDto>
    {
        public ProductFilterRequestValidator()
        {
            // Basic pagination validation
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100");

            // Search term validation
            RuleFor(x => x.Search)
                .MaximumLength(100)
                .WithMessage("Search term cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Search));

            // ID validation
            RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                .WithMessage("Category ID must be greater than 0")
                .When(x => x.CategoryId.HasValue);

            RuleFor(x => x.BrandId)
                .GreaterThan(0)
                .WithMessage("Brand ID must be greater than 0")
                .When(x => x.BrandId.HasValue);

            // Price range validation
            RuleFor(x => x.MinPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum price must be greater than or equal to 0")
                .When(x => x.MinPrice.HasValue);

            RuleFor(x => x.MaxPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Maximum price must be greater than or equal to 0")
                .When(x => x.MaxPrice.HasValue);

            RuleFor(x => x)
                .Must(x => x.IsValidPriceRange())
                .WithMessage("Minimum price cannot be greater than maximum price")
                .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue);

            // Rating validation
            RuleFor(x => x.MinRating)
                .InclusiveBetween(0, 5)
                .WithMessage("Minimum rating must be between 0 and 5")
                .When(x => x.MinRating.HasValue);

            // Weight range validation
            RuleFor(x => x.MinWeight)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum weight must be greater than or equal to 0")
                .When(x => x.MinWeight.HasValue);

            RuleFor(x => x.MaxWeight)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Maximum weight must be greater than or equal to 0")
                .When(x => x.MaxWeight.HasValue);

            RuleFor(x => x)
                .Must(x => x.IsValidWeightRange())
                .WithMessage("Minimum weight cannot be greater than maximum weight")
                .When(x => x.MinWeight.HasValue && x.MaxWeight.HasValue);

            // Stock range validation
            RuleFor(x => x.MinStock)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum stock must be greater than or equal to 0")
                .When(x => x.MinStock.HasValue);

            RuleFor(x => x.MaxStock)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Maximum stock must be greater than or equal to 0")
                .When(x => x.MaxStock.HasValue);

            RuleFor(x => x)
                .Must(x => x.IsValidStockRange())
                .WithMessage("Minimum stock cannot be greater than maximum stock")
                .When(x => x.MinStock.HasValue && x.MaxStock.HasValue);

            // Date range validation
            RuleFor(x => x)
                .Must(x => x.IsValidDateRange())
                .WithMessage("Created from date cannot be greater than created to date")
                .When(x => x.CreatedFrom.HasValue && x.CreatedTo.HasValue);

            RuleFor(x => x)
                .Must(x => x.IsValidAvailabilityRange())
                .WithMessage("Available from date cannot be greater than available until date")
                .When(x => x.AvailableFrom.HasValue && x.AvailableUntil.HasValue);

            // Tag validation
            RuleFor(x => x.TagIds)
                .Must(tagIds => tagIds == null || tagIds.All(id => id > 0))
                .WithMessage("All tag IDs must be greater than 0")
                .When(x => x.TagIds != null && x.TagIds.Any());

            RuleFor(x => x.TagNames)
                .Must(tagNames => tagNames == null || tagNames.All(name => !string.IsNullOrWhiteSpace(name) && name.Length <= 50))
                .WithMessage("All tag names must be non-empty and not exceed 50 characters")
                .When(x => x.TagNames != null && x.TagNames.Any());

            RuleFor(x => x.TagsSearch)
                .MaximumLength(200)
                .WithMessage("Tags search cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.TagsSearch));

            // Color and size validation
            RuleFor(x => x.Colors)
                .Must(colors => colors == null || colors.All(id => id > 0))
                .WithMessage("All color IDs must be greater than 0")
                .When(x => x.Colors != null && x.Colors.Any());

            RuleFor(x => x.Sizes)
                .Must(sizes => sizes == null || sizes.All(id => id > 0))
                .WithMessage("All size IDs must be greater than 0")
                .When(x => x.Sizes != null && x.Sizes.Any());

            // Sort validation
            RuleFor(x => x.SortBy)
                .Must(sortBy => string.IsNullOrEmpty(sortBy) || IsValidSortField(sortBy))
                .WithMessage("Invalid sort field. Valid options: name, price, rating, created, updated, sold, views, stock, relevance, popularity, discount");

            RuleFor(x => x.SortOrder)
                .Must(sortOrder => string.IsNullOrEmpty(sortOrder) || sortOrder.ToLower() is "asc" or "desc")
                .WithMessage("Sort order must be either 'asc' or 'desc'");

            // Origin validation
            RuleFor(x => x.Origin)
                .MaximumLength(100)
                .WithMessage("Origin cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Origin));

            // Attributes validation
            RuleFor(x => x.Attributes)
                .Must(attrs => attrs == null || ValidateAttributes(attrs))
                .WithMessage("Attribute keys and values must not be empty and not exceed 100 characters each")
                .When(x => x.Attributes != null);

            // Complex validation rules
            RuleFor(x => x)
                .Must(x => !HasConflictingBooleanFilters(x))
                .WithMessage("Cannot have conflicting boolean filters (e.g., FeaturedOnly=true and IsFeatured=false)");

            RuleFor(x => x)
                .Must(x => !HasTooManyFilters(x))
                .WithMessage("Too many filters applied. Please reduce the number of filters for better performance.");
        }

        private static bool IsValidSortField(string sortBy)
        {
            var validSortFields = new[]
            {
                "name", "price", "rating", "created", "updated", 
                "sold", "views", "stock", "relevance", "popularity", "discount"
            };
            
            return validSortFields.Contains(sortBy.ToLower());
        }

        private static bool ValidateAttributes(Dictionary<string, string> attributes)
        {
            return attributes.All(attr => 
                !string.IsNullOrWhiteSpace(attr.Key) && 
                attr.Key.Length <= 100 &&
                !string.IsNullOrWhiteSpace(attr.Value) && 
                attr.Value.Length <= 100);
        }

        private static bool HasConflictingBooleanFilters(ProductFilterRequestDto filter)
        {
            // Check for conflicting featured filters
            if (filter.FeaturedOnly == true && filter.IsFeatured == false)
                return true;

            // Check for conflicting new filters
            if (filter.NewOnly == true && filter.IsNew == false)
                return true;

            // Check for conflicting stock filters
            if (filter.InStockOnly == true && filter.MaxStock == 0)
                return true;

            return false;
        }

        private static bool HasTooManyFilters(ProductFilterRequestDto filter)
        {
            var filterCount = 0;

            // Count applied filters
            if (!string.IsNullOrEmpty(filter.Search)) filterCount++;
            if (filter.CategoryId.HasValue) filterCount++;
            if (filter.BrandId.HasValue) filterCount++;
            if (filter.MinPrice.HasValue || filter.MaxPrice.HasValue) filterCount++;
            if (filter.MinRating.HasValue) filterCount++;
            if (filter.TagIds?.Any() == true) filterCount++;
            if (filter.TagNames?.Any() == true) filterCount++;
            if (!string.IsNullOrEmpty(filter.TagsSearch)) filterCount++;
            if (filter.Status.HasValue) filterCount++;
            if (filter.Condition.HasValue) filterCount++;
            if (filter.InStockOnly.HasValue) filterCount++;
            if (filter.OnSaleOnly.HasValue) filterCount++;
            if (filter.FeaturedOnly.HasValue) filterCount++;
            if (filter.NewOnly.HasValue) filterCount++;
            if (filter.IsFeatured.HasValue) filterCount++;
            if (filter.IsNew.HasValue) filterCount++;
            if (filter.IsBestseller.HasValue) filterCount++;
            if (filter.IsLimitedEdition.HasValue) filterCount++;
            if (filter.IsGiftWrappingAvailable.HasValue) filterCount++;
            if (filter.AllowBackorder.HasValue) filterCount++;
            if (filter.AllowPreorder.HasValue) filterCount++;
            if (filter.HasDiscount.HasValue) filterCount++;
            if (filter.IsJapaneseProduct.HasValue) filterCount++;
            if (filter.IsAuthentic.HasValue) filterCount++;
            if (filter.Attributes?.Any() == true) filterCount++;
            if (!string.IsNullOrEmpty(filter.Origin)) filterCount++;
            if (filter.JapaneseRegion.HasValue) filterCount++;
            if (filter.AuthenticityLevel.HasValue) filterCount++;
            if (filter.AgeRestriction.HasValue) filterCount++;
            if (filter.MinWeight.HasValue || filter.MaxWeight.HasValue) filterCount++;
            if (filter.Colors?.Any() == true) filterCount++;
            if (filter.Sizes?.Any() == true) filterCount++;
            if (filter.CreatedFrom.HasValue || filter.CreatedTo.HasValue) filterCount++;
            if (filter.AvailableFrom.HasValue || filter.AvailableUntil.HasValue) filterCount++;
            if (filter.MinStock.HasValue || filter.MaxStock.HasValue) filterCount++;

            // Limit to 20 simultaneous filters for performance
            return filterCount > 20;
        }
    }

    /// <summary>
    /// Validator for advanced search scenarios
    /// </summary>
    public class AdvancedProductSearchValidator : AbstractValidator<ProductFilterRequestDto>
    {
        public AdvancedProductSearchValidator()
        {
            Include(new ProductFilterRequestValidator());

            // Additional rules for advanced search
            RuleFor(x => x)
                .Must(x => HasMeaningfulSearch(x))
                .WithMessage("Please provide at least one meaningful search criteria")
                .When(x => string.IsNullOrEmpty(x.Search) && !x.HasFilters());

            RuleFor(x => x.Search)
                .MinimumLength(2)
                .WithMessage("Search term must be at least 2 characters long")
                .When(x => !string.IsNullOrEmpty(x.Search));

            RuleFor(x => x)
                .Must(x => HasReasonablePageSize(x))
                .WithMessage("Page size is too large for the number of filters applied. Please reduce page size or filters.");
        }

        private static bool HasMeaningfulSearch(ProductFilterRequestDto filter)
        {
            // If no search term, must have at least one filter
            return filter.HasFilters();
        }

        private static bool HasReasonablePageSize(ProductFilterRequestDto filter)
        {
            // If many filters are applied, suggest smaller page sizes
            var filterCount = GetFilterCount(filter);
            
            return filterCount switch
            {
                <= 5 => filter.PageSize <= 100,
                <= 10 => filter.PageSize <= 50,
                <= 15 => filter.PageSize <= 25,
                _ => filter.PageSize <= 10
            };
        }

        private static int GetFilterCount(ProductFilterRequestDto filter)
        {
            var count = 0;
            
            if (!string.IsNullOrEmpty(filter.Search)) count++;
            if (filter.CategoryId.HasValue) count++;
            if (filter.BrandId.HasValue) count++;
            if (filter.MinPrice.HasValue || filter.MaxPrice.HasValue) count++;
            if (filter.TagIds?.Any() == true || filter.TagNames?.Any() == true) count++;
            if (filter.Status.HasValue || filter.Condition.HasValue) count++;
            
            // Add other significant filters
            count += new bool?[]
            {
                filter.InStockOnly, filter.OnSaleOnly, filter.FeaturedOnly, 
                filter.NewOnly, filter.IsBestseller, filter.IsLimitedEdition
            }.Count(b => b.HasValue);

            return count;
        }
    }
}