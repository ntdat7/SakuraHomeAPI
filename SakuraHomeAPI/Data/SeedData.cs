using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Catalog;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;

namespace SakuraHomeAPI.Data
{
    public static class DatabaseSeeder
    {
        public static void SeedData(ModelBuilder builder)
        {
            SeedSystemSettings(builder);
            SeedCategories(builder);
            SeedBrands(builder);
            SeedProductAttributes(builder);
            SeedTags(builder);
            SeedShippingZones(builder);
            SeedNotificationTemplates(builder);
            SeedPaymentMethods(builder);
        }

        /// <summary>
        /// Seed system settings
        /// </summary>
        private static void SeedSystemSettings(ModelBuilder builder)
        {
            var settingsData = new[]
            {
                new SystemSetting
                {
                    Id = 1,
                    Key = "SiteName",
                    Value = "Sakura Home",
                    Description = "Tên website",
                    Type = SettingType.String,
                    Category = "General",
                    IsPublic = true,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new SystemSetting
                {
                    Id = 2,
                    Key = "SiteDescription",
                    Value = "Japanese Products E-commerce Platform",
                    Description = "Mô tả website",
                    Type = SettingType.String,
                    Category = "General",
                    IsPublic = true,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new SystemSetting
                {
                    Id = 3,
                    Key = "DefaultCurrency",
                    Value = "VND",
                    Description = "Đơn vị tiền tệ mặc định",
                    Type = SettingType.String,
                    Category = "General",
                    IsPublic = true,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new SystemSetting
                {
                    Id = 4,
                    Key = "DefaultLanguage",
                    Value = "vi",
                    Description = "Ngôn ngữ mặc định",
                    Type = SettingType.String,
                    Category = "General",
                    IsPublic = true,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new SystemSetting
                {
                    Id = 5,
                    Key = "ContactEmail",
                    Value = "contact@sakurahome.com",
                    Description = "Email liên hệ",
                    Type = SettingType.String,
                    Category = "Contact",
                    IsPublic = true,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new SystemSetting
                {
                    Id = 6,
                    Key = "ContactPhone",
                    Value = "+84 123 456 789",
                    Description = "Số điện thoại liên hệ",
                    Type = SettingType.String,
                    Category = "Contact",
                    IsPublic = true,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new SystemSetting
                {
                    Id = 7,
                    Key = "EnableRegistration",
                    Value = "true",
                    Description = "Cho phép đăng ký tài khoản mới",
                    Type = SettingType.Boolean,
                    Category = "Security",
                    IsPublic = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new SystemSetting
                {
                    Id = 8,
                    Key = "MaxLoginAttempts",
                    Value = "5",
                    Description = "Số lần đăng nhập sai tối đa",
                    Type = SettingType.Number,
                    Category = "Security",
                    IsPublic = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                }
            };

            builder.Entity<SystemSetting>().HasData(settingsData);
        }

        /// <summary>
        /// Seed default categories
        /// </summary>
        private static void SeedCategories(ModelBuilder builder)
        {
            var categories = new[]
            {
                new Category
                {
                    Id = 1,
                    Name = "Food & Beverages",
                    Slug = "food-beverages",
                    Description = "Japanese food and drinks including snacks, teas, and traditional ingredients",
                    ImageUrl = "/images/categories/food-beverages.jpg",
                    Icon = "fas fa-utensils",
                    Color = "#FF6B6B",
                    MetaTitle = "Japanese Food & Beverages - Sakura Home",
                    MetaDescription = "Discover authentic Japanese food and beverages at Sakura Home",
                    MetaKeywords = "japanese food, beverages, snacks, tea",
                    DisplayOrder = 1,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new Category
                {
                    Id = 2,
                    Name = "Beauty & Health",
                    Slug = "beauty-health",
                    Description = "Japanese cosmetics, skincare, and health products",
                    ImageUrl = "/images/categories/beauty-health.jpg",
                    Icon = "fas fa-spa",
                    Color = "#4ECDC4",
                    MetaTitle = "Japanese Beauty & Health Products - Sakura Home",
                    MetaDescription = "Premium Japanese beauty and health products for your wellness",
                    MetaKeywords = "japanese beauty, cosmetics, skincare, health",
                    DisplayOrder = 2,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new Category
                {
                    Id = 3,
                    Name = "Fashion",
                    Slug = "fashion",
                    Description = "Japanese fashion and accessories including clothing and bags",
                    ImageUrl = "/images/categories/fashion.jpg",
                    Icon = "fas fa-tshirt",
                    Color = "#45B7D1",
                    MetaTitle = "Japanese Fashion & Accessories - Sakura Home",
                    MetaDescription = "Trendy Japanese fashion and accessories for every style",
                    MetaKeywords = "japanese fashion, clothing, accessories, bags",
                    DisplayOrder = 3,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new Category
                {
                    Id = 4,
                    Name = "Electronics",
                    Slug = "electronics",
                    Description = "Japanese electronics and gadgets including games and tech accessories",
                    ImageUrl = "/images/categories/electronics.jpg",
                    Icon = "fas fa-laptop",
                    Color = "#96CEB4",
                    MetaTitle = "Japanese Electronics & Gadgets - Sakura Home",
                    MetaDescription = "Latest Japanese electronics and innovative gadgets",
                    MetaKeywords = "japanese electronics, gadgets, games, technology",
                    DisplayOrder = 4,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new Category
                {
                    Id = 5,
                    Name = "Home & Living",
                    Slug = "home-living",
                    Description = "Japanese home decoration and lifestyle products",
                    ImageUrl = "/images/categories/home-living.jpg",
                    Icon = "fas fa-home",
                    Color = "#FFEAA7",
                    MetaTitle = "Japanese Home & Living - Sakura Home",
                    MetaDescription = "Beautiful Japanese home decoration and lifestyle products",
                    MetaKeywords = "japanese home, decoration, lifestyle, living",
                    DisplayOrder = 5,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                }
            };

            builder.Entity<Category>().HasData(categories);
        }

        /// <summary>
        /// Seed default brands
        /// </summary>
        private static void SeedBrands(ModelBuilder builder)
        {
            var brands = new[]
            {
                new Brand
                {
                    Id = 1,
                    Name = "Shiseido",
                    Slug = "shiseido",
                    Description = "Premium Japanese cosmetics and skincare brand",
                    LogoUrl = "/images/brands/shiseido-logo.png",
                    Website = "https://www.shiseido.com",
                    ContactEmail = "info@shiseido.com",
                    ContactPhone = "+81-3-3572-5111",
                    Country = "Japan",
                    FoundedYear = new DateTime(1872, 1, 1),
                    Headquarters = "Tokyo, Japan",
                    MetaTitle = "Shiseido - Premium Japanese Beauty",
                    MetaDescription = "Discover Shiseido's premium Japanese beauty and skincare products",
                    MetaKeywords = "shiseido, japanese cosmetics, skincare, beauty",
                    FacebookUrl = "https://www.facebook.com/Shiseido",
                    InstagramUrl = "https://www.instagram.com/shiseido",
                    TwitterUrl = "https://twitter.com/shiseido",
                    YoutubeUrl = "https://www.youtube.com/user/shiseido",
                    IsFeatured = true,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new Brand
                {
                    Id = 2,
                    Name = "Sony",
                    Slug = "sony",
                    Description = "Japanese multinational electronics corporation",
                    LogoUrl = "/images/brands/sony-logo.png",
                    Website = "https://www.sony.com",
                    ContactEmail = "info@sony.com",
                    ContactPhone = "+81-3-6748-2111",
                    Country = "Japan",
                    FoundedYear = new DateTime(1946, 1, 1),
                    Headquarters = "Tokyo, Japan",
                    MetaTitle = "Sony - Japanese Electronics Innovation",
                    MetaDescription = "Explore Sony's innovative Japanese electronics and entertainment products",
                    MetaKeywords = "sony, japanese electronics, innovation, technology",
                    FacebookUrl = "https://www.facebook.com/Sony",
                    InstagramUrl = "https://www.instagram.com/sony",
                    TwitterUrl = "https://twitter.com/sony",
                    YoutubeUrl = "https://www.youtube.com/user/SonyElectronics",
                    IsFeatured = true,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new Brand
                {
                    Id = 3,
                    Name = "Pocky",
                    Slug = "pocky",
                    Description = "Famous Japanese snack brand by Glico",
                    LogoUrl = "/images/brands/pocky-logo.png",
                    Website = "https://www.pocky.com",
                    ContactEmail = "info@glico.com",
                    ContactPhone = "+81-6-6477-8352",
                    Country = "Japan",
                    FoundedYear = new DateTime(1966, 1, 1),
                    Headquarters = "Osaka, Japan",
                    MetaTitle = "Pocky - Japanese Snack Sensation",
                    MetaDescription = "Enjoy the famous Japanese Pocky snacks in various flavors",
                    MetaKeywords = "pocky, japanese snacks, glico, biscuit sticks",
                    FacebookUrl = "https://www.facebook.com/PockyUSA",
                    InstagramUrl = "https://www.instagram.com/pocky_global",
                    TwitterUrl = "https://twitter.com/PockyUSA",
                    YoutubeUrl = "https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw",
                    IsFeatured = true,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                }
            };

            builder.Entity<Brand>().HasData(brands);
        }

        /// <summary>
        /// Seed product attributes
        /// </summary>
        private static void SeedProductAttributes(ModelBuilder builder)
        {
            var attributes = new[]
            {
                new ProductAttribute
                {
                    Id = 1,
                    Name = "Size",
                    Code = "size",
                    Description = "Product size variations",
                    Type = AttributeType.Select,
                    IsRequired = false,
                    IsFilterable = true,
                    IsSearchable = false,
                    IsVariant = true,
                    DisplayOrder = 1,
                    Options = "[\"XS\", \"S\", \"M\", \"L\", \"XL\", \"XXL\"]",
                    Unit = "",
                    ValidationRegex = "",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new ProductAttribute
                {
                    Id = 2,
                    Name = "Color",
                    Code = "color",
                    Description = "Product color variations",
                    Type = AttributeType.Color,
                    IsRequired = false,
                    IsFilterable = true,
                    IsSearchable = false,
                    IsVariant = true,
                    DisplayOrder = 2,
                    Options = "[\"Red\", \"Blue\", \"Green\", \"Yellow\", \"Black\", \"White\", \"Pink\", \"Purple\"]",
                    Unit = "",
                    ValidationRegex = "",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new ProductAttribute
                {
                    Id = 3,
                    Name = "Material",
                    Code = "material",
                    Description = "Product material composition",
                    Type = AttributeType.Select,
                    IsRequired = false,
                    IsFilterable = true,
                    IsSearchable = true,
                    IsVariant = false,
                    DisplayOrder = 3,
                    Options = "[\"Cotton\", \"Polyester\", \"Silk\", \"Wool\", \"Leather\", \"Plastic\", \"Metal\", \"Wood\"]",
                    Unit = "",
                    ValidationRegex = "",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                }
            };

            builder.Entity<ProductAttribute>().HasData(attributes);
        }

        /// <summary>
        /// Seed product tags
        /// </summary>
        private static void SeedTags(ModelBuilder builder)
        {
            var tags = new[]
            {
                new Tag
                {
                    Id = 1,
                    Name = "New Arrival",
                    Slug = "new-arrival",
                    Description = "Newly arrived products",
                    Color = "#FF6B6B",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new Tag
                {
                    Id = 2,
                    Name = "Best Seller",
                    Slug = "best-seller",
                    Description = "Top selling products",
                    Color = "#4ECDC4",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new Tag
                {
                    Id = 3,
                    Name = "Limited Edition",
                    Slug = "limited-edition",
                    Description = "Limited edition products",
                    Color = "#45B7D1",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                },
                new Tag
                {
                    Id = 4,
                    Name = "Eco Friendly",
                    Slug = "eco-friendly",
                    Description = "Environmentally friendly products",
                    Color = "#96CEB4",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    UpdatedAt = new DateTime(2024, 1, 1)
                }
            };

            builder.Entity<Tag>().HasData(tags);
        }

        /// <summary>
        /// Seed shipping zones
        /// </summary>
        private static void SeedShippingZones(ModelBuilder builder)
        {
            var shippingZones = new[]
            {
                new ShippingZone
                {
                    Id = 1,
                    Name = "Việt Nam",
                    Description = "Khu vực giao hàng trong nước Việt Nam",
                    Countries = "[\"VN\"]",
                    IsActive = true,
                    DisplayOrder = 1
                },
                new ShippingZone
                {
                    Id = 2,
                    Name = "Đông Nam Á",
                    Description = "Khu vực giao hàng Đông Nam Á",
                    Countries = "[\"TH\", \"SG\", \"MY\", \"ID\", \"PH\", \"LA\", \"KH\", \"MM\", \"BN\"]",
                    IsActive = true,
                    DisplayOrder = 2
                },
                new ShippingZone
                {
                    Id = 3,
                    Name = "Quốc tế",
                    Description = "Khu vực giao hàng quốc tế",
                    Countries = "[\"US\", \"CA\", \"GB\", \"FR\", \"DE\", \"AU\", \"JP\", \"KR\", \"CN\"]",
                    IsActive = true,
                    DisplayOrder = 3
                }
            };

            builder.Entity<ShippingZone>().HasData(shippingZones);
        }

        /// <summary>
        /// Seed notification templates
        /// </summary>
        private static void SeedNotificationTemplates(ModelBuilder builder)
        {
            var templates = new[]
            {
                new NotificationTemplate
                {
                    Id = 1,
                    Name = "Welcome Email",
                    Type = "Email",
                    Subject = "Chào mừng đến với Sakura Home!",
                    BodyTemplate = "Xin chào {UserName}, cảm ơn bạn đã đăng ký tài khoản tại Sakura Home.",
                    Language = "vi",
                    IsActive = true
                },
                new NotificationTemplate
                {
                    Id = 2,
                    Name = "Order Confirmation",
                    Type = "Email",
                    Subject = "Xác nhận đơn hàng #{OrderNumber}",
                    BodyTemplate = "Đơn hàng {OrderNumber} của bạn đã được xác nhận. Tổng tiền: {TotalAmount}",
                    Language = "vi",
                    IsActive = true
                },
                new NotificationTemplate
                {
                    Id = 3,
                    Name = "Order Shipped",
                    Type = "Email",
                    Subject = "Đơn hàng #{OrderNumber} đã được giao cho vận chuyển",
                    BodyTemplate = "Đơn hàng {OrderNumber} đã được giao cho đơn vị vận chuyển. Mã tracking: {TrackingNumber}",
                    Language = "vi",
                    IsActive = true
                }
            };

            builder.Entity<NotificationTemplate>().HasData(templates);
        }

        /// <summary>
        /// Seed payment methods
        /// </summary>
        private static void SeedPaymentMethods(ModelBuilder builder)
        {
            var paymentMethods = new[]
            {
                new PaymentMethodInfo
                {
                    Id = 1,
                    Name = "Thanh toán khi nhận hàng (COD)",
                    Description = "Thanh toán bằng tiền mặt khi nhận hàng",
                    Code = "COD",
                    LogoUrl = "/images/payment/cod.png",
                    IsActive = true,
                    DisplayOrder = 1,
                    FeePercentage = 0,
                    FixedFee = 0,
                    MinAmount = 0,
                    MaxAmount = 5000000
                },
                new PaymentMethodInfo
                {
                    Id = 2,
                    Name = "Chuyển khoản ngân hàng",
                    Description = "Chuyển khoản qua ngân hàng",
                    Code = "BANK_TRANSFER",
                    LogoUrl = "/images/payment/bank-transfer.png",
                    IsActive = true,
                    DisplayOrder = 2,
                    FeePercentage = 0,
                    FixedFee = 0,
                    MinAmount = 0,
                    MaxAmount = 0
                },
                new PaymentMethodInfo
                {
                    Id = 3,
                    Name = "Ví MoMo",
                    Description = "Thanh toán qua ví điện tử MoMo",
                    Code = "MOMO",
                    LogoUrl = "/images/payment/momo.png",
                    IsActive = true,
                    DisplayOrder = 3,
                    FeePercentage = 2.5m,
                    FixedFee = 0,
                    MinAmount = 10000,
                    MaxAmount = 50000000
                },
                new PaymentMethodInfo
                {
                    Id = 4,
                    Name = "ZaloPay",
                    Description = "Thanh toán qua ví điện tử ZaloPay",
                    Code = "ZALOPAY",
                    LogoUrl = "/images/payment/zalopay.png",
                    IsActive = true,
                    DisplayOrder = 4,
                    FeePercentage = 2.0m,
                    FixedFee = 0,
                    MinAmount = 10000,
                    MaxAmount = 50000000
                }
            };

            builder.Entity<PaymentMethodInfo>().HasData(paymentMethods);
        }
    }
}