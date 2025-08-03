using FluentValidation;
using SakuraHomeAPI.DTOs.Products;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.Validators.Products
{
    /// <summary>
    /// Validator for product filter requests
    /// </summary>
    public class ProductFilterRequestValidator : AbstractValidator<ProductFilterRequestDto>
    {
        public ProductFilterRequestValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.Search)
                .MaximumLength(100)
                .WithMessage("Search term cannot exceed 100 characters")
                .Must(BeValidSearchTerm)
                .WithMessage("Search term contains invalid characters")
                .When(x => !string.IsNullOrEmpty(x.Search));

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

            RuleFor(x => x.SortBy)
                .Must(BeValidSortField)
                .WithMessage("Sort by must be one of: name, price, rating, created, sold, views, stock");

            RuleFor(x => x.SortOrder)
                .Must(BeValidSortOrder)
                .WithMessage("Sort order must be either 'asc' or 'desc'");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                .WithMessage("Category ID must be greater than 0")
                .When(x => x.CategoryId.HasValue);

            RuleFor(x => x.BrandId)
                .GreaterThan(0)
                .WithMessage("Brand ID must be greater than 0")
                .When(x => x.BrandId.HasValue);

            RuleFor(x => x.TagIds)
                .Must(tags => tags == null || tags.All(id => id > 0))
                .WithMessage("All tag IDs must be greater than 0")
                .When(x => x.TagIds != null);
        }

        private bool BeValidSearchTerm(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return true;
            
            // Không ch?a các ký t? ??c bi?t nguy hi?m
            var invalidChars = new[] { '<', '>', '"', '\'', '&', '%', ';', '(', ')', '+', '=' };
            return !invalidChars.Any(searchTerm.Contains);
        }

        private bool BeValidSortField(string sortBy)
        {
            var validSortFields = new[] { "name", "price", "rating", "created", "sold", "views", "stock" };
            return validSortFields.Contains(sortBy.ToLower());
        }

        private bool BeValidSortOrder(string sortOrder)
        {
            var validSortOrders = new[] { "asc", "desc" };
            return validSortOrders.Contains(sortOrder.ToLower());
        }
    }
}