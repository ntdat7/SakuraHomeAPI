using FluentValidation;
using SakuraHomeAPI.DTOs.Products.Requests;
using SakuraHomeAPI.Validators.Common;

namespace SakuraHomeAPI.Validators.Products
{
    /// <summary>
    /// Validator for create product requests
    /// </summary>
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequestDto>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Product name is required")
                .Length(2, 500)
                .WithMessage("Product name must be between 2 and 500 characters")
                .Must(BeValidProductName)
                .WithMessage("Product name contains invalid characters");

            RuleFor(x => x.SKU)
                .NotEmpty()
                .WithMessage("SKU is required")
                .Length(3, 100)
                .WithMessage("SKU must be between 3 and 100 characters")
                .Matches(@"^[A-Z0-9\-_]+$")
                .WithMessage("SKU can only contain uppercase letters, numbers, hyphens, and underscores");

            RuleFor(x => x.ShortDescription)
                .MaximumLength(1000)
                .WithMessage("Short description cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.ShortDescription));

            RuleFor(x => x.Description)
                .MaximumLength(10000)
                .WithMessage("Description cannot exceed 10000 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.MainImage)
                .Must(CommonValidators.BeValidImageUrl)
                .WithMessage("Main image must be a valid image URL")
                .MaximumLength(500)
                .WithMessage("Main image URL cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.MainImage));

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0")
                .LessThanOrEqualTo(999999999)
                .WithMessage("Price cannot exceed 999,999,999");

            RuleFor(x => x.OriginalPrice)
                .GreaterThan(0)
                .WithMessage("Original price must be greater than 0")
                .LessThanOrEqualTo(999999999)
                .WithMessage("Original price cannot exceed 999,999,999")
                .When(x => x.OriginalPrice.HasValue);

            RuleFor(x => x.CostPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Cost price must be greater than or equal to 0")
                .LessThanOrEqualTo(999999999)
                .WithMessage("Cost price cannot exceed 999,999,999")
                .When(x => x.CostPrice.HasValue);

            RuleFor(x => x)
                .Must(x => x.IsValidPriceRange())
                .WithMessage("Original price must be greater than or equal to sale price")
                .When(x => x.OriginalPrice.HasValue);

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Stock must be greater than or equal to 0");

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
                .WithMessage("Maximum stock must be greater than or equal to minimum stock")
                .When(x => x.MinStock.HasValue && x.MaxStock.HasValue);

            RuleFor(x => x.BrandId)
                .GreaterThan(0)
                .WithMessage("Brand ID must be greater than 0");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                .WithMessage("Category ID must be greater than 0");

            RuleFor(x => x.Origin)
                .MaximumLength(100)
                .WithMessage("Origin cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Origin));

            RuleFor(x => x.AuthenticityInfo)
                .MaximumLength(1000)
                .WithMessage("Authenticity info cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.AuthenticityInfo));

            RuleFor(x => x.UsageGuide)
                .MaximumLength(2000)
                .WithMessage("Usage guide cannot exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.UsageGuide));

            RuleFor(x => x.Ingredients)
                .MaximumLength(1000)
                .WithMessage("Ingredients cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.Ingredients));

            RuleFor(x => x.BatchNumber)
                .MaximumLength(50)
                .WithMessage("Batch number cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.BatchNumber));

            RuleFor(x => x)
                .Must(x => x.IsValidManufactureDateRange())
                .WithMessage("Expiry date must be after manufacture date")
                .When(x => x.ManufactureDate.HasValue && x.ExpiryDate.HasValue);

            RuleFor(x => x.Weight)
                .GreaterThan(0)
                .WithMessage("Weight must be greater than 0")
                .When(x => x.Weight.HasValue);

            RuleFor(x => x.Length)
                .GreaterThan(0)
                .WithMessage("Length must be greater than 0")
                .When(x => x.Length.HasValue);

            RuleFor(x => x.Width)
                .GreaterThan(0)
                .WithMessage("Width must be greater than 0")
                .When(x => x.Width.HasValue);

            RuleFor(x => x.Height)
                .GreaterThan(0)
                .WithMessage("Height must be greater than 0")
                .When(x => x.Height.HasValue);

            RuleFor(x => x.GiftWrappingFee)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Gift wrapping fee must be greater than or equal to 0")
                .When(x => x.GiftWrappingFee.HasValue);

            RuleFor(x => x.Tags)
                .MaximumLength(500)
                .WithMessage("Tags cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Tags));

            RuleFor(x => x.MarketingDescription)
                .MaximumLength(1000)
                .WithMessage("Marketing description cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.MarketingDescription));

            RuleFor(x => x)
                .Must(x => x.IsValidDateRange())
                .WithMessage("Available until date must be after available from date")
                .When(x => x.AvailableFrom.HasValue && x.AvailableUntil.HasValue);

            RuleFor(x => x.ExpiryDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Expiry date must be in the future")
                .When(x => x.ExpiryDate.HasValue);

            // Validate images
            RuleForEach(x => x.Images)
                .SetValidator(new CreateProductImageRequestValidator())
                .When(x => x.Images != null);

            // Validate variants
            RuleForEach(x => x.Variants)
                .SetValidator(new CreateProductVariantRequestValidator())
                .When(x => x.Variants != null);

            // Validate attributes
            RuleForEach(x => x.Attributes)
                .SetValidator(new SetProductAttributeRequestValidator())
                .When(x => x.Attributes != null);

            // Validate tag IDs
            RuleFor(x => x.TagIds)
                .Must(tags => tags == null || tags.All(id => id > 0))
                .WithMessage("All tag IDs must be greater than 0")
                .When(x => x.TagIds != null);

            // Business logic validations
            RuleFor(x => x)
                .Must(HaveValidGiftWrappingSetup)
                .WithMessage("Gift wrapping fee is required when gift wrapping is available")
                .When(x => x.IsGiftWrappingAvailable);
        }

        private bool BeValidProductName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            
            // No dangerous special characters
            var invalidChars = new[] { '<', '>', '"', '&', '%', ';' };
            return !invalidChars.Any(name.Contains);
        }

        private bool HaveValidGiftWrappingSetup(CreateProductRequestDto product)
        {
            return !product.IsGiftWrappingAvailable || product.GiftWrappingFee.HasValue;
        }
    }

    /// <summary>
    /// Validator for update product requests
    /// </summary>
    public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequestDto>
    {
        public UpdateProductRequestValidator()
        {
            Include(new CreateProductRequestValidator());

            RuleFor(x => x.UpdateReason)
                .MaximumLength(500)
                .WithMessage("Update reason cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.UpdateReason));
        }
    }
}