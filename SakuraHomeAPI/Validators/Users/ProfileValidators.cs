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
            RuleFor(x => x.AddressLine1)
                .NotEmpty()
                .WithMessage("Address line 1 is required")
                .MaximumLength(255)
                .WithMessage("Address line 1 cannot exceed 255 characters");

            RuleFor(x => x.AddressLine2)
                .MaximumLength(255)
                .WithMessage("Address line 2 cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.AddressLine2));

            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage("City is required")
                .MaximumLength(100)
                .WithMessage("City cannot exceed 100 characters");

            RuleFor(x => x.StateProvince)
                .NotEmpty()
                .WithMessage("State/Province is required")
                .MaximumLength(100)
                .WithMessage("State/Province cannot exceed 100 characters");

            RuleFor(x => x.PostalCode)
                .NotEmpty()
                .WithMessage("Postal code is required")
                .MaximumLength(20)
                .WithMessage("Postal code cannot exceed 20 characters");

            RuleFor(x => x.Country)
                .NotEmpty()
                .WithMessage("Country is required")
                .MaximumLength(100)
                .WithMessage("Country cannot exceed 100 characters");

            RuleFor(x => x.PhoneNumber)
                .Must(CommonValidators.BeValidPhoneNumber)
                .WithMessage("Invalid phone number format")
                .MaximumLength(100)
                .WithMessage("Phone number cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.RecipientName)
                .MaximumLength(100)
                .WithMessage("Recipient name cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.RecipientName));

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
            RuleFor(x => x.AddressLine1)
                .MaximumLength(255)
                .WithMessage("Address line 1 cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.AddressLine1));

            RuleFor(x => x.AddressLine2)
                .MaximumLength(255)
                .WithMessage("Address line 2 cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.AddressLine2));

            RuleFor(x => x.City)
                .MaximumLength(100)
                .WithMessage("City cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.City));

            RuleFor(x => x.StateProvince)
                .MaximumLength(100)
                .WithMessage("State/Province cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.StateProvince));

            RuleFor(x => x.PostalCode)
                .MaximumLength(20)
                .WithMessage("Postal code cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.PostalCode));

            RuleFor(x => x.Country)
                .MaximumLength(100)
                .WithMessage("Country cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Country));

            RuleFor(x => x.PhoneNumber)
                .Must(CommonValidators.BeValidPhoneNumber)
                .WithMessage("Invalid phone number format")
                .MaximumLength(100)
                .WithMessage("Phone number cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.RecipientName)
                .MaximumLength(100)
                .WithMessage("Recipient name cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.RecipientName));

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithMessage("Notes cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}