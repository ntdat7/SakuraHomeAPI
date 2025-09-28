using FluentValidation;
using SakuraHomeAPI.DTOs.Products.Requests;
using SakuraHomeAPI.Validators.Common;

namespace SakuraHomeAPI.Validators.Products
{
    /// <summary>
    /// Validator for product filter requests
    /// </summary>
    public class ProductFilterRequestValidator : AbstractValidator<ProductFilterRequestDto>
    {
        public ProductFilterRequestValidator()
        {
            // Pagination validation
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100");

            // Search validation
            RuleFor(x => x.Search)
                .MaximumLength(100)
                .WithMessage("Search term cannot exceed 100 characters")
                .Must(CommonValidators.BeValidSearchTerm)
                .WithMessage("Search term contains invalid characters")
                .When(x => !string.IsNullOrEmpty(x.Search));

            // Category and Brand validation
            RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                .WithMessage("Category ID must be greater than 0")
                .When(x => x.CategoryId.HasValue);

            RuleFor(x => x.BrandId)
                .GreaterThan(0)
                .WithMessage("Brand ID must be greater than 0")
                .When(x => x.BrandId.HasValue);

            // Price validation
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
                .WithMessage("Maximum price must be greater than or equal to minimum price")
                .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue);

            // Rating validation
            RuleFor(x => x.MinRating)
                .InclusiveBetween(0, 5)
                .WithMessage("Minimum rating must be between 0 and 5")
                .When(x => x.MinRating.HasValue);

            // Weight validation
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
                .WithMessage("Maximum weight must be greater than or equal to minimum weight")
                .When(x => x.MinWeight.HasValue && x.MaxWeight.HasValue);

            // Sorting validation
            RuleFor(x => x.SortBy)
                .Must(BeValidSortField)
                .WithMessage("Sort by must be one of: name, price, rating, created, updated, sold, views, stock");

            RuleFor(x => x.SortOrder)
                .Must(BeValidSortOrder)
                .WithMessage("Sort order must be either 'asc' or 'desc'");

            // Collection validation
            RuleFor(x => x.TagIds)
                .Must(tags => tags == null || tags.All(id => id > 0))
                .WithMessage("All tag IDs must be greater than 0")
                .When(x => x.TagIds != null);

            RuleFor(x => x.Attributes)
                .Must(attrs => attrs == null || attrs.All(attr => !string.IsNullOrWhiteSpace(attr)))
                .WithMessage("All attributes must be non-empty")
                .When(x => x.Attributes != null);

            RuleFor(x => x.Colors)
                .Must(colors => colors == null || colors.All(id => id > 0))
                .WithMessage("All color IDs must be greater than 0")
                .When(x => x.Colors != null);

            RuleFor(x => x.Sizes)
                .Must(sizes => sizes == null || sizes.All(id => id > 0))
                .WithMessage("All size IDs must be greater than 0")
                .When(x => x.Sizes != null);

            // Date validation
            RuleFor(x => x)
                .Must(x => x.IsValidDateRange())
                .WithMessage("Created to date must be after created from date")
                .When(x => x.CreatedFrom.HasValue && x.CreatedTo.HasValue);

            RuleFor(x => x)
                .Must(x => x.IsValidAvailabilityRange())
                .WithMessage("Available until date must be after available from date")
                .When(x => x.AvailableFrom.HasValue && x.AvailableUntil.HasValue);

            // Origin validation
            RuleFor(x => x.Origin)
                .MaximumLength(100)
                .WithMessage("Origin cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Origin));

            // Enum validation (FluentValidation automatically validates enum values)
            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid product status")
                .When(x => x.Status.HasValue);

            RuleFor(x => x.Condition)
                .IsInEnum()
                .WithMessage("Invalid product condition")
                .When(x => x.Condition.HasValue);

            RuleFor(x => x.JapaneseRegion)
                .IsInEnum()
                .WithMessage("Invalid Japanese region")
                .When(x => x.JapaneseRegion.HasValue);

            RuleFor(x => x.AuthenticityLevel)
                .IsInEnum()
                .WithMessage("Invalid authenticity level")
                .When(x => x.AuthenticityLevel.HasValue);

            RuleFor(x => x.AgeRestriction)
                .IsInEnum()
                .WithMessage("Invalid age restriction")
                .When(x => x.AgeRestriction.HasValue);

            RuleFor(x => x.WeightUnit)
                .IsInEnum()
                .WithMessage("Invalid weight unit")
                .When(x => x.WeightUnit.HasValue);
        }

        private bool BeValidSortField(string sortBy)
        {
            var validSortFields = new[] { "name", "price", "rating", "created", "updated", "sold", "views", "stock" };
            return validSortFields.Contains(sortBy.ToLower());
        }

        private bool BeValidSortOrder(string sortOrder)
        {
            var validSortOrders = new[] { "asc", "desc" };
            return validSortOrders.Contains(sortOrder.ToLower());
        }
    }

    /// <summary>
    /// Validator for the legacy filter product request (for backward compatibility)
    /// </summary>
    public class FilterProductRequestValidator : AbstractValidator<ProductFilterRequestDto>
    {
        public FilterProductRequestValidator()
        {
            // Delegate to the main validator for consistency
            Include(new ProductFilterRequestValidator());
        }
    }
}