//using FluentValidation;
//using SakuraHomeAPI.Controllers;
//using SakuraHomeAPI.DTOs.Catalog;
//using SakuraHomeAPI.Models.Entities.Catalog;

//namespace SakuraHomeAPI.Validators.Catalog
//{
//    public class CreateBrandDtoValidator : AbstractValidator<CreateBrandDto>
//    {
//        public CreateBrandDtoValidator()
//        {
//            RuleFor(x => x.Name)
//                .NotEmpty().WithMessage("Brand name is required")
//                .MaximumLength(255).WithMessage("Brand name cannot exceed 255 characters");

//            RuleFor(x => x.Description)
//                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
//                .When(x => !string.IsNullOrEmpty(x.Description));

//            RuleFor(x => x.LogoUrl)
//                .Must(BeValidUrlOrEmpty).WithMessage("Logo URL must be a valid URL format")
//                .MaximumLength(500).WithMessage("Logo URL cannot exceed 500 characters")
//                .When(x => !string.IsNullOrEmpty(x.LogoUrl));

//            RuleFor(x => x.Country)
//                .MaximumLength(100).WithMessage("Country cannot exceed 100 characters")
//                .When(x => !string.IsNullOrEmpty(x.Country));

//            RuleFor(x => x.Website)
//                .Must(BeValidUrlOrEmpty).WithMessage("Website must be a valid URL format")
//                .MaximumLength(500).WithMessage("Website cannot exceed 500 characters")
//                .When(x => !string.IsNullOrEmpty(x.Website));

//            RuleFor(x => x.ContactEmail)
//                .EmailAddress().WithMessage("Contact email must be a valid email address")
//                .MaximumLength(255).WithMessage("Contact email cannot exceed 255 characters")
//                .When(x => !string.IsNullOrEmpty(x.ContactEmail));

//            RuleFor(x => x.ContactPhone)
//                .Must(BeValidPhoneOrEmpty).WithMessage("Contact phone must be a valid phone number")
//                .MaximumLength(20).WithMessage("Contact phone cannot exceed 20 characters")
//                .When(x => !string.IsNullOrEmpty(x.ContactPhone));

//            RuleFor(x => x.Headquarters)
//                .MaximumLength(500).WithMessage("Headquarters cannot exceed 500 characters")
//                .When(x => !string.IsNullOrEmpty(x.Headquarters));

//            // Social Media URLs - All Optional
//            RuleFor(x => x.FacebookUrl)
//                .Must(BeValidFacebookUrlOrEmpty).WithMessage("Facebook URL must be a valid Facebook URL")
//                .MaximumLength(255).WithMessage("Facebook URL cannot exceed 255 characters")
//                .When(x => !string.IsNullOrEmpty(x.FacebookUrl));

//            RuleFor(x => x.InstagramUrl)
//                .Must(BeValidInstagramUrlOrEmpty).WithMessage("Instagram URL must be a valid Instagram URL")
//                .MaximumLength(255).WithMessage("Instagram URL cannot exceed 255 characters")
//                .When(x => !string.IsNullOrEmpty(x.InstagramUrl));

//            RuleFor(x => x.TwitterUrl)
//                .Must(BeValidTwitterUrlOrEmpty).WithMessage("Twitter URL must be a valid Twitter/X URL")
//                .MaximumLength(255).WithMessage("Twitter URL cannot exceed 255 characters")
//                .When(x => !string.IsNullOrEmpty(x.TwitterUrl));

//            RuleFor(x => x.YoutubeUrl)
//                .Must(BeValidYouTubeUrlOrEmpty).WithMessage("YouTube URL must be a valid YouTube URL")
//                .MaximumLength(255).WithMessage("YouTube URL cannot exceed 255 characters")
//                .When(x => !string.IsNullOrEmpty(x.YoutubeUrl));
//        }

//        private bool BeValidUrlOrEmpty(string url)
//        {
//            if (string.IsNullOrEmpty(url)) return true;
//            return Uri.TryCreate(url, UriKind.Absolute, out Uri result)
//                   && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
//        }

//        private bool BeValidPhoneOrEmpty(string phone)
//        {
//            if (string.IsNullOrEmpty(phone)) return true;
//            // Remove common phone formatting characters
//            var cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");
//            return cleanPhone.All(char.IsDigit) && cleanPhone.Length >= 10 && cleanPhone.Length <= 15;
//        }

//        private bool BeValidFacebookUrlOrEmpty(string url)
//        {
//            if (string.IsNullOrEmpty(url)) return true;
//            if (!BeValidUrlOrEmpty(url)) return false;
//            var lowerUrl = url.ToLower();
//            return lowerUrl.Contains("facebook.com") || lowerUrl.Contains("fb.com");
//        }

//        private bool BeValidInstagramUrlOrEmpty(string url)
//        {
//            if (string.IsNullOrEmpty(url)) return true;
//            if (!BeValidUrlOrEmpty(url)) return false;
//            return url.ToLower().Contains("instagram.com");
//        }

//        private bool BeValidTwitterUrlOrEmpty(string url)
//        {
//            if (string.IsNullOrEmpty(url)) return true;
//            if (!BeValidUrlOrEmpty(url)) return false;
//            var lowerUrl = url.ToLower();
//            return lowerUrl.Contains("twitter.com") || lowerUrl.Contains("x.com");
//        }

//        private bool BeValidYouTubeUrlOrEmpty(string url)
//        {
//            if (string.IsNullOrEmpty(url)) return true;
//            if (!BeValidUrlOrEmpty(url)) return false;
//            var lowerUrl = url.ToLower();
//            return lowerUrl.Contains("youtube.com") || lowerUrl.Contains("youtu.be");
//        }
//    }

//    public class UpdateBrandDtoValidator : AbstractValidator<UpdateBrandDto>
//    {
//        public UpdateBrandDtoValidator()
//        {
//            // Inherit all rules from CreateBrandDtoValidator
//            Include(new CreateBrandDtoValidator());

//            // Add any update-specific rules here if needed
//        }
//    }

//    public class BrandEntityValidator : AbstractValidator<Brand>
//    {
//        public BrandEntityValidator()
//        {
//            RuleFor(x => x.Name)
//                .NotEmpty().WithMessage("Brand name is required")
//                .MaximumLength(255).WithMessage("Brand name cannot exceed 255 characters");

//            // All other fields are optional at entity level since they can be null in database
//            RuleFor(x => x.Description)
//                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
//                .When(x => x.Description != null);

//            RuleFor(x => x.LogoUrl)
//                .MaximumLength(500).WithMessage("Logo URL cannot exceed 500 characters")
//                .When(x => x.LogoUrl != null);

//            RuleFor(x => x.Country)
//                .MaximumLength(100).WithMessage("Country cannot exceed 100 characters")
//                .When(x => x.Country != null);

//            RuleFor(x => x.Website)
//                .MaximumLength(500).WithMessage("Website cannot exceed 500 characters")
//                .When(x => x.Website != null);

//            RuleFor(x => x.ContactEmail)
//                .MaximumLength(255).WithMessage("Contact email cannot exceed 255 characters")
//                .When(x => x.ContactEmail != null);

//            RuleFor(x => x.ContactPhone)
//                .MaximumLength(20).WithMessage("Contact phone cannot exceed 20 characters")
//                .When(x => x.ContactPhone != null);

//            RuleFor(x => x.Headquarters)
//                .MaximumLength(500).WithMessage("Headquarters cannot exceed 500 characters")
//                .When(x => x.Headquarters != null);

//            // Social Media URLs
//            RuleFor(x => x.FacebookUrl)
//                .MaximumLength(255).WithMessage("Facebook URL cannot exceed 255 characters")
//                .When(x => x.FacebookUrl != null);

//            RuleFor(x => x.InstagramUrl)
//                .MaximumLength(255).WithMessage("Instagram URL cannot exceed 255 characters")
//                .When(x => x.InstagramUrl != null);

//            RuleFor(x => x.TwitterUrl)
//                .MaximumLength(255).WithMessage("Twitter URL cannot exceed 255 characters")
//                .When(x => x.TwitterUrl != null);

//            RuleFor(x => x.YoutubeUrl)
//                .MaximumLength(255).WithMessage("YouTube URL cannot exceed 255 characters")
//                .When(x => x.YoutubeUrl != null);
//        }
//    }
//}