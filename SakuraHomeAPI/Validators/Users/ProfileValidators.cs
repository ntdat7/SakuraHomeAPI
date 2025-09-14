using FluentValidation;
using SakuraHomeAPI.DTOs.Users.Requests;
using SakuraHomeAPI.Validators.Common;

namespace SakuraHomeAPI.Validators.Users
{
    /// <summary>
    /// Validator for profile update requests
    /// </summary>
    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequestDto>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .Length(1, 100)
                .WithMessage("First name must be between 1 and 100 characters")
                .Must(CommonValidators.BeValidName)
                .WithMessage("First name contains invalid characters")
                .When(x => !string.IsNullOrEmpty(x.FirstName));

            RuleFor(x => x.LastName)
                .Length(1, 100)
                .WithMessage("Last name must be between 1 and 100 characters")
                .Must(CommonValidators.BeValidName)
                .WithMessage("Last name contains invalid characters")
                .When(x => !string.IsNullOrEmpty(x.LastName));

            RuleFor(x => x.PhoneNumber)
                .Must(CommonValidators.BeValidPhoneNumber)
                .WithMessage("Invalid phone number format")
                .MaximumLength(20)
                .WithMessage("Phone number cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Today)
                .WithMessage("Date of birth must be in the past")
                .GreaterThan(DateTime.Today.AddYears(-120))
                .WithMessage("Date of birth cannot be more than 120 years ago")
                .When(x => x.DateOfBirth.HasValue);

            RuleFor(x => x.Avatar)
                .Must(CommonValidators.BeValidImageUrl)
                .WithMessage("Avatar must be a valid image URL")
                .MaximumLength(500)
                .WithMessage("Avatar URL cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Avatar));

            RuleFor(x => x.PreferredLanguage)
                .Must(CommonValidators.BeValidLanguageCode)
                .WithMessage("Invalid language code")
                .MaximumLength(5)
                .WithMessage("Language code cannot exceed 5 characters")
                .When(x => !string.IsNullOrEmpty(x.PreferredLanguage));

            RuleFor(x => x.PreferredCurrency)
                .Must(CommonValidators.BeValidCurrencyCode)
                .WithMessage("Invalid currency code")
                .MaximumLength(3)
                .WithMessage("Currency code cannot exceed 3 characters")
                .When(x => !string.IsNullOrEmpty(x.PreferredCurrency));
        }
    }

    /// <summary>
    /// Validator for create address requests
    /// </summary>
    public class CreateAddressRequestValidator : AbstractValidator<CreateAddressRequestDto>
    {
        public CreateAddressRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Recipient name is required")
                .MaximumLength(100)
                .WithMessage("Recipient name cannot exceed 100 characters");

            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("Phone number is required")
                .Must(CommonValidators.BeValidPhoneNumber)
                .WithMessage("Invalid phone number format")
                .MaximumLength(20)
                .WithMessage("Phone number cannot exceed 20 characters");

            RuleFor(x => x.AddressLine1)
                .NotEmpty()
                .WithMessage("Address line 1 is required")
                .MaximumLength(255)
                .WithMessage("Address line 1 cannot exceed 255 characters");

            RuleFor(x => x.AddressLine2)
                .MaximumLength(255)
                .WithMessage("Address line 2 cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.AddressLine2));

            RuleFor(x => x.ProvinceId)
                .GreaterThan(0)
                .WithMessage("Province is required");

            RuleFor(x => x.WardId)
                .GreaterThan(0)
                .WithMessage("Ward is required");

            RuleFor(x => x.PostalCode)
                .MaximumLength(20)
                .WithMessage("Postal code cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.PostalCode));

            RuleFor(x => x.Country)
                .MaximumLength(100)
                .WithMessage("Country cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Country));

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithMessage("Notes cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }

    /// <summary>
    /// Validator for update address requests
    /// </summary>
    public class UpdateAddressRequestValidator : AbstractValidator<UpdateAddressRequestDto>
    {
        public UpdateAddressRequestValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage("Recipient name cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Phone)
                .Must(CommonValidators.BeValidPhoneNumber)
                .WithMessage("Invalid phone number format")
                .MaximumLength(20)
                .WithMessage("Phone number cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.AddressLine1)
                .MaximumLength(255)
                .WithMessage("Address line 1 cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.AddressLine1));

            RuleFor(x => x.AddressLine2)
                .MaximumLength(255)
                .WithMessage("Address line 2 cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.AddressLine2));

            RuleFor(x => x.ProvinceId)
                .GreaterThan(0)
                .WithMessage("Province ID must be greater than 0")
                .When(x => x.ProvinceId.HasValue);

            RuleFor(x => x.WardId)
                .GreaterThan(0)
                .WithMessage("Ward ID must be greater than 0")
                .When(x => x.WardId.HasValue);

            RuleFor(x => x.PostalCode)
                .MaximumLength(20)
                .WithMessage("Postal code cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.PostalCode));

            RuleFor(x => x.Country)
                .MaximumLength(100)
                .WithMessage("Country cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Country));

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithMessage("Notes cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}