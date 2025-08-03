using FluentValidation;
using SakuraHomeAPI.DTOs.Users;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.Validators.Users
{
    /// <summary>
    /// Validator for user filter requests
    /// </summary>
    public class UserFilterRequestValidator : AbstractValidator<UserFilterRequestDto>
    {
        public UserFilterRequestValidator()
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

            RuleFor(x => x.MinTotalSpent)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Min total spent must be greater than or equal to 0")
                .When(x => x.MinTotalSpent.HasValue);

            RuleFor(x => x.MaxTotalSpent)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Max total spent must be greater than or equal to 0")
                .When(x => x.MaxTotalSpent.HasValue);

            RuleFor(x => x)
                .Must(x => x.IsValidTotalSpentRange())
                .WithMessage("Max total spent must be greater than or equal to min total spent")
                .When(x => x.MinTotalSpent.HasValue && x.MaxTotalSpent.HasValue);

            RuleFor(x => x.MinTotalOrders)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Min total orders must be greater than or equal to 0")
                .When(x => x.MinTotalOrders.HasValue);

            RuleFor(x => x.MaxTotalOrders)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Max total orders must be greater than or equal to 0")
                .When(x => x.MaxTotalOrders.HasValue);

            RuleFor(x => x)
                .Must(x => x.IsValidTotalOrdersRange())
                .WithMessage("Max total orders must be greater than or equal to min total orders")
                .When(x => x.MinTotalOrders.HasValue && x.MaxTotalOrders.HasValue);

            RuleFor(x => x)
                .Must(x => x.IsValidCreatedDateRange())
                .WithMessage("Created to date must be after created from date")
                .When(x => x.CreatedFrom.HasValue && x.CreatedTo.HasValue);

            RuleFor(x => x)
                .Must(x => x.IsValidLastLoginDateRange())
                .WithMessage("Last login to date must be after last login from date")
                .When(x => x.LastLoginFrom.HasValue && x.LastLoginTo.HasValue);

            RuleFor(x => x.SortBy)
                .Must(BeValidSortField)
                .WithMessage("Sort by must be one of: name, email, created, lastLogin, totalSpent, totalOrders");

            RuleFor(x => x.SortOrder)
                .Must(BeValidSortOrder)
                .WithMessage("Sort order must be either 'asc' or 'desc'");
        }

        private bool BeValidSearchTerm(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return true;
            
            var invalidChars = new[] { '<', '>', '"', '\'', '&', '%', ';', '(', ')', '+', '=' };
            return !invalidChars.Any(searchTerm.Contains);
        }

        private bool BeValidSortField(string sortBy)
        {
            var validSortFields = new[] { "name", "email", "created", "lastLogin", "totalSpent", "totalOrders" };
            return validSortFields.Contains(sortBy.ToLower());
        }

        private bool BeValidSortOrder(string sortOrder)
        {
            var validSortOrders = new[] { "asc", "desc" };
            return validSortOrders.Contains(sortOrder.ToLower());
        }
    }
}