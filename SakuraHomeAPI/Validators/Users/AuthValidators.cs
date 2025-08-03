using FluentValidation;
using SakuraHomeAPI.DTOs.Users.Requests;
using SakuraHomeAPI.Validators.Common;

namespace SakuraHomeAPI.Validators.Users
{
    /// <summary>
    /// Validator for user registration
    /// </summary>
    public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format")
                .MaximumLength(255)
                .WithMessage("Email cannot exceed 255 characters")
                .Must(CommonValidators.BeValidEmailDomain)
                .WithMessage("Email domain is not allowed");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters")
                .MaximumLength(100)
                .WithMessage("Password cannot exceed 100 characters")
                .Must(CommonValidators.BeStrongPassword)
                .WithMessage("Password must contain at least one uppercase letter, lowercase letter, digit, and special character");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Confirm password is required")
                .Equal(x => x.Password)
                .WithMessage("Passwords do not match");

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required")
                .Length(1, 100)
                .WithMessage("First name must be between 1 and 100 characters")
                .Must(CommonValidators.BeValidName)
                .WithMessage("First name contains invalid characters");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required")
                .Length(1, 100)
                .WithMessage("Last name must be between 1 and 100 characters")
                .Must(CommonValidators.BeValidName)
                .WithMessage("Last name contains invalid characters");

            RuleFor(x => x.PhoneNumber)
                .Must(CommonValidators.BeValidPhoneNumber)
                .WithMessage("Invalid phone number format")
                .MaximumLength(20)
                .WithMessage("Phone number cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.AcceptTerms)
                .Equal(true)
                .WithMessage("You must accept the terms and conditions");

            RuleFor(x => x.PreferredLanguage)
                .Must(CommonValidators.BeValidLanguageCode)
                .WithMessage("Invalid language code")
                .MaximumLength(5)
                .WithMessage("Language code cannot exceed 5 characters");
        }
    }

    /// <summary>
    /// Validator for login requests
    /// </summary>
    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required");
        }
    }

    /// <summary>
    /// Validator for refresh token requests
    /// </summary>
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequestDto>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage("Refresh token is required");
        }
    }

    /// <summary>
    /// Validator for forgot password requests
    /// </summary>
    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestDto>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format");
        }
    }

    /// <summary>
    /// Validator for reset password requests
    /// </summary>
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequestDto>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format");

            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Reset token is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("New password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters")
                .MaximumLength(100)
                .WithMessage("Password cannot exceed 100 characters")
                .Must(CommonValidators.BeStrongPassword)
                .WithMessage("Password must contain at least one uppercase letter, lowercase letter, digit, and special character");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Confirm password is required")
                .Equal(x => x.NewPassword)
                .WithMessage("Passwords do not match");
        }
    }

    /// <summary>
    /// Validator for change password requests
    /// </summary>
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequestDto>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage("Current password is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("New password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters")
                .MaximumLength(100)
                .WithMessage("Password cannot exceed 100 characters")
                .Must(CommonValidators.BeStrongPassword)
                .WithMessage("Password must contain at least one uppercase letter, lowercase letter, digit, and special character")
                .NotEqual(x => x.CurrentPassword)
                .WithMessage("New password must be different from current password");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Confirm password is required")
                .Equal(x => x.NewPassword)
                .WithMessage("Passwords do not match");
        }
    }
}