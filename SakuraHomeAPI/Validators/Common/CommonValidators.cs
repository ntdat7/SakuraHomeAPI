using FluentValidation;

namespace SakuraHomeAPI.Validators.Common
{
    /// <summary>
    /// Common validation methods and rules
    /// </summary>
    public static class CommonValidators
    {
        public static bool BeValidEmailDomain(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;

            var blacklistedDomains = new[]
            {
                "tempmail.org", "10minutemail.com", "guerrillamail.com", 
                "mailinator.com", "yopmail.com", "temp-mail.org"
            };

            var domain = email.Split('@').LastOrDefault()?.ToLower();
            return domain != null && !blacklistedDomains.Contains(domain);
        }

        public static bool BeStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;

            return password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) &&
                   password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c));
        }

        public static bool BeValidName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;

            // Only letters, spaces, hyphens, and apostrophes
            return name.All(c => char.IsLetter(c) || c == ' ' || c == '-' || c == '\'');
        }

        public static bool BeValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return true;

            // Basic phone number format validation
            return phoneNumber.All(c => char.IsDigit(c) || c == '+' || c == '-' || c == ' ' || c == '(' || c == ')');
        }

        public static bool BeValidLanguageCode(string languageCode)
        {
            var validLanguages = new[] { "vi", "en", "ja", "ko", "zh" };
            return validLanguages.Contains(languageCode.ToLower());
        }

        public static bool BeValidCurrencyCode(string currencyCode)
        {
            var validCurrencies = new[] { "VND", "USD", "JPY", "EUR", "GBP" };
            return validCurrencies.Contains(currencyCode.ToUpper());
        }

        public static bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static bool BeValidImageUrl(string url)
        {
            if (!BeValidUrl(url)) return false;
            
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
            return validExtensions.Any(ext => url.ToLower().Contains(ext));
        }

        public static bool BeValidSearchTerm(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return true;
            
            // No dangerous special characters
            var invalidChars = new[] { '<', '>', '"', '\'', '&', '%', ';', '(', ')', '+', '=' };
            return !invalidChars.Any(searchTerm.Contains);
        }
    }
}