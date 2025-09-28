using FluentValidation;
using SakuraHomeAPI.DTOs.Products.Components;
using SakuraHomeAPI.Validators.Common;

namespace SakuraHomeAPI.Validators.Products
{
    /// <summary>
    /// Validator for create product image requests
    /// </summary>
    public class CreateProductImageValidator : AbstractValidator<CreateProductImageDto>
    {
        public CreateProductImageValidator()
        {
            RuleFor(x => x.ImageUrl)
                .NotEmpty()
                .WithMessage("Image URL is required")
                .MaximumLength(500)
                .WithMessage("Image URL cannot exceed 500 characters")
                .Must(CommonValidators.BeValidImageUrl)
                .WithMessage("Image URL must be a valid image URL");

            RuleFor(x => x.AltText)
                .MaximumLength(255)
                .WithMessage("Alt text cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.AltText));

            RuleFor(x => x.Caption)
                .MaximumLength(500)
                .WithMessage("Caption cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Caption));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Display order must be greater than or equal to 0")
                .LessThan(1000)
                .WithMessage("Display order must be less than 1000");
        }
    }

    /// <summary>
    /// Validator for update product image requests
    /// </summary>
    public class UpdateProductImageValidator : AbstractValidator<UpdateProductImageDto>
    {
        public UpdateProductImageValidator()
        {
            RuleFor(x => x.AltText)
                .MaximumLength(255)
                .WithMessage("Alt text cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.AltText));

            RuleFor(x => x.Caption)
                .MaximumLength(500)
                .WithMessage("Caption cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Caption));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Display order must be greater than or equal to 0")
                .LessThan(1000)
                .WithMessage("Display order must be less than 1000");
        }
    }

    /// <summary>
    /// Validator for create product variant requests
    /// </summary>
    public class CreateProductVariantValidator : AbstractValidator<CreateProductVariantDto>
    {
        public CreateProductVariantValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Variant name is required")
                .Length(1, 255)
                .WithMessage("Variant name must be between 1 and 255 characters");

            RuleFor(x => x.SKU)
                .NotEmpty()
                .WithMessage("Variant SKU is required")
                .Length(3, 100)
                .WithMessage("Variant SKU must be between 3 and 100 characters")
                .Matches(@"^[A-Z0-9\-_]+$")
                .WithMessage("Variant SKU can only contain uppercase letters, numbers, hyphens, and underscores");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Variant price must be greater than or equal to 0")
                .LessThanOrEqualTo(999999999)
                .WithMessage("Variant price cannot exceed 999,999,999");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Variant stock must be greater than or equal to 0");

            RuleFor(x => x.ImageUrl)
                .Must(CommonValidators.BeValidImageUrl)
                .WithMessage("Variant image must be a valid image URL")
                .MaximumLength(500)
                .WithMessage("Variant image URL cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.ImageUrl));

            RuleFor(x => x.Attributes)
                .Must(BeValidAttributes)
                .WithMessage("Variant attributes must have valid key-value pairs");
        }

        private bool BeValidAttributes(Dictionary<string, string> attributes)
        {
            if (attributes == null) return true;
            
            return attributes.All(kvp => 
                !string.IsNullOrWhiteSpace(kvp.Key) && 
                !string.IsNullOrWhiteSpace(kvp.Value) &&
                kvp.Key.Length <= 100 &&
                kvp.Value.Length <= 255);
        }
    }

    /// <summary>
    /// Validator for update product variant requests
    /// </summary>
    public class UpdateProductVariantValidator : AbstractValidator<UpdateProductVariantDto>
    {
        public UpdateProductVariantValidator()
        {
            RuleFor(x => x.Name)
                .Length(1, 255)
                .WithMessage("Variant name must be between 1 and 255 characters")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.SKU)
                .Length(3, 100)
                .WithMessage("Variant SKU must be between 3 and 100 characters")
                .Matches(@"^[A-Z0-9\-_]+$")
                .WithMessage("Variant SKU can only contain uppercase letters, numbers, hyphens, and underscores")
                .When(x => !string.IsNullOrEmpty(x.SKU));

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Variant price must be greater than or equal to 0")
                .LessThanOrEqualTo(999999999)
                .WithMessage("Variant price cannot exceed 999,999,999")
                .When(x => x.Price.HasValue);

            RuleFor(x => x.OriginalPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Original price must be greater than or equal to 0")
                .LessThanOrEqualTo(999999999)
                .WithMessage("Original price cannot exceed 999,999,999")
                .When(x => x.OriginalPrice.HasValue);

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Variant stock must be greater than or equal to 0")
                .When(x => x.Stock.HasValue);

            RuleFor(x => x.ImageUrl)
                .Must(CommonValidators.BeValidImageUrl)
                .WithMessage("Variant image must be a valid image URL")
                .MaximumLength(500)
                .WithMessage("Variant image URL cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.ImageUrl));

            RuleFor(x => x.Attributes)
                .Must(BeValidAttributes)
                .WithMessage("Variant attributes must have valid key-value pairs")
                .When(x => x.Attributes != null);

            RuleFor(x => x)
                .Must(HaveValidPriceRange)
                .WithMessage("Original price must be greater than or equal to price")
                .When(x => x.Price.HasValue && x.OriginalPrice.HasValue);
        }

        private bool BeValidAttributes(Dictionary<string, string> attributes)
        {
            if (attributes == null) return true;
            
            return attributes.All(kvp => 
                !string.IsNullOrWhiteSpace(kvp.Key) && 
                !string.IsNullOrWhiteSpace(kvp.Value) &&
                kvp.Key.Length <= 100 &&
                kvp.Value.Length <= 255);
        }

        private bool HaveValidPriceRange(UpdateProductVariantDto variant)
        {
            return !variant.Price.HasValue || !variant.OriginalPrice.HasValue || variant.OriginalPrice >= variant.Price;
        }
    }

    /// <summary>
    /// Validator for create product attribute requests
    /// </summary>
    public class CreateProductAttributeValidator : AbstractValidator<CreateProductAttributeDto>
    {
        public CreateProductAttributeValidator()
        {
            RuleFor(x => x.AttributeId)
                .GreaterThan(0)
                .WithMessage("Attribute ID must be greater than 0");

            RuleFor(x => x.Value)
                .NotEmpty()
                .WithMessage("Attribute value is required")
                .MaximumLength(1000)
                .WithMessage("Attribute value cannot exceed 1000 characters");

            RuleFor(x => x.DisplayValue)
                .MaximumLength(255)
                .WithMessage("Display value cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.DisplayValue));
        }
    }

    // Keep backward compatibility validators
    /// <summary>
    /// Legacy validator for create product image requests (backward compatibility)
    /// </summary>
    public class CreateProductImageRequestValidator : AbstractValidator<CreateProductImageDto>
    {
        public CreateProductImageRequestValidator()
        {
            Include(new CreateProductImageValidator());
        }
    }

    /// <summary>
    /// Legacy validator for create product variant requests (backward compatibility)
    /// </summary>
    public class CreateProductVariantRequestValidator : AbstractValidator<CreateProductVariantDto>
    {
        public CreateProductVariantRequestValidator()
        {
            Include(new CreateProductVariantValidator());
        }
    }

    /// <summary>
    /// Legacy validator for set product attribute requests (backward compatibility)
    /// </summary>
    public class SetProductAttributeRequestValidator : AbstractValidator<CreateProductAttributeDto>
    {
        public SetProductAttributeRequestValidator()
        {
            Include(new CreateProductAttributeValidator());
        }
    }
}