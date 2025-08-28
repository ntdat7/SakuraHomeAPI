using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Models.Entities.Catalog;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Models.Enums;
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
            SeedProducts(builder);
            SeedSuperAdminUser(builder);
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
                IsVerified = true,
                IsOfficial = true,
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
                IsVerified = true,
                IsOfficial = true,
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
                IsVerified = true,
                IsOfficial = true,
                IsFeatured = true,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = new DateTime(2024, 1, 1),
                UpdatedAt = new DateTime(2024, 1, 1)
            },
            // Brand cho Matcha Green Tea Powder (Product Id = 2)
            new Brand
            {
                Id = 4,
                Name = "Ippodo Tea",
                Slug = "ippodo-tea",
                Description = "Traditional Japanese tea company established in Kyoto",
                LogoUrl = "/images/brands/ippodo-logo.png",
                Website = "https://www.ippodo-tea.co.jp",
                ContactEmail = "info@ippodo-tea.co.jp",
                ContactPhone = "+81-75-211-3421",
                Country = "Japan",
                FoundedYear = new DateTime(1717, 1, 1),
                Headquarters = "Kyoto, Japan",
                MetaTitle = "Ippodo Tea - Traditional Japanese Tea Master",
                MetaDescription = "Premium Japanese tea including ceremonial matcha from Kyoto's oldest tea house",
                MetaKeywords = "ippodo, japanese tea, matcha, ceremonial grade, kyoto",
                FacebookUrl = "https://www.facebook.com/IppodoTea",
                InstagramUrl = "https://www.instagram.com/ippodo_tea",
                TwitterUrl = "https://twitter.com/ippodo_tea",
                YoutubeUrl = "https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw",
                IsFeatured = true,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = new DateTime(2024, 1, 1),
                UpdatedAt = new DateTime(2024, 1, 1)
            },
            // Brand cho Marine Collagen Supplement (Product Id = 4)
            new Brand
            {
                Id = 5,
                Name = "Meiji",
                Slug = "meiji",
                Description = "Japanese pharmaceutical and health food company",
                LogoUrl = "/images/brands/meiji-logo.png",
                Website = "https://www.meiji.com",
                ContactEmail = "info@meiji.com",
                ContactPhone = "+81-3-3273-3001",
                Country = "Japan",
                FoundedYear = new DateTime(1906, 1, 1),
                Headquarters = "Tokyo, Japan",
                MetaTitle = "Meiji - Japanese Health & Nutrition",
                MetaDescription = "Trusted Japanese brand for health foods, supplements and nutritional products",
                MetaKeywords = "meiji, japanese health, supplements, collagen, nutrition",
                FacebookUrl = "https://www.facebook.com/MeijiGlobal",
                InstagramUrl = "https://www.instagram.com/meiji_global",
                TwitterUrl = "https://twitter.com/meiji_global",
                YoutubeUrl = "https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw",
                IsFeatured = true,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = new DateTime(2024, 1, 1),
                UpdatedAt = new DateTime(2024, 1, 1)
            },
            // Brand cho UNIQLO Heattech (Product Id = 6)
            new Brand
            {
                Id = 6,
                Name = "UNIQLO",
                Slug = "uniqlo",
                Description = "Japanese casual wear designer, manufacturer and retailer",
                LogoUrl = "/images/brands/uniqlo-logo.png",
                Website = "https://www.uniqlo.com",
                ContactEmail = "info@uniqlo.com",
                ContactPhone = "+81-3-6252-5181",
                Country = "Japan",
                FoundedYear = new DateTime(1984, 1, 1),
                Headquarters = "Tokyo, Japan",
                MetaTitle = "UNIQLO - Japanese Fashion Innovation",
                MetaDescription = "Innovative Japanese fashion brand with advanced fabric technology like Heattech",
                MetaKeywords = "uniqlo, japanese fashion, heattech, innovative clothing, casual wear",
                FacebookUrl = "https://www.facebook.com/uniqlo",
                InstagramUrl = "https://www.instagram.com/uniqlo",
                TwitterUrl = "https://twitter.com/uniqlo_global",
                YoutubeUrl = "https://www.youtube.com/user/UNIQLO",
                IsVerified = true,
                IsOfficial = true,
                IsFeatured = true,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = new DateTime(2024, 1, 1),
                UpdatedAt = new DateTime(2024, 1, 1)
            },
            // Brand cho Traditional Tea Set & Furoshiki (Products Id = 7, 8)
            new Brand
            {
                Id = 7,
                Name = "Kutani Kiln",
                Slug = "kutani-kiln",
                Description = "Traditional Japanese ceramic and craft manufacturer from Ishikawa",
                LogoUrl = "/images/brands/kutani-logo.png",
                Website = "https://www.kutani-kiln.com",
                ContactEmail = "info@kutani-kiln.com",
                ContactPhone = "+81-761-57-3341",
                Country = "Japan",
                FoundedYear = new DateTime(1655, 1, 1),
                Headquarters = "Ishikawa, Japan",
                MetaTitle = "Kutani Kiln - Traditional Japanese Ceramics",
                MetaDescription = "Authentic Japanese ceramics and traditional crafts from the historic Kutani region",
                MetaKeywords = "kutani, japanese ceramics, traditional crafts, pottery, tea sets",
                FacebookUrl = "https://www.facebook.com/KutaniKiln",
                InstagramUrl = "https://www.instagram.com/kutani_kiln",
                TwitterUrl = "https://twitter.com/uniqlo_global",
                YoutubeUrl = "https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw",
                IsFeatured = false,
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
        /// <summary>
        /// Seed sample products
        /// </summary>
        private static void SeedProducts(ModelBuilder builder)
        {
            // Use static dates instead of DateTime.UtcNow to avoid model changes warning
            var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var productCreationDate = baseDate;
            var productUpdateDate = baseDate;

            var products = new[]
            {
        // Food & Beverages Category
        new Product
        {
            Id = 1,
            Name = "Pocky Chocolate Sticks",
            SKU = "POCKY-CHOC-001",
            Slug = "pocky-chocolate-sticks",
            ShortDescription = "Classic Japanese chocolate biscuit sticks",
            Description = "Delicious Japanese Pocky chocolate biscuit sticks. Perfect snack for any time of the day. Made with high-quality ingredients and authentic Japanese recipe.",
            MainImage = "/images/products/pocky-chocolate.jpg",
            Price = 45000m,
            OriginalPrice = 50000m,
            CostPrice = 30000m,
            Stock = 500,
            MinStock = 50,
            MaxStock = 1000,
            TrackInventory = true,
            AllowBackorder = false,
            AllowPreorder = false,
            BrandId = 3, // Pocky
            CategoryId = 1, // Food & Beverages
            Origin = "Osaka",
            JapaneseRegion = JapaneseOrigin.Osaka, // Fixed: was Tokyo
            AuthenticityLevel = AuthenticityLevel.Verified,
            AuthenticityInfo = "Authentic Glico Pocky imported directly from Japan",
            UsageGuide = "Ready to eat. Store in cool, dry place.",
            Ingredients = "Wheat flour, sugar, chocolate, vegetable oil, whole milk powder",
            ExpiryDate = new DateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddMonths(6)
            ManufactureDate = new DateTime(2023, 12, 1, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddMonths(-1)
            BatchNumber = "PCK20240101",
            AgeRestriction = AgeRestriction.None,
            Weight = 47m,
            WeightUnit = WeightUnit.Gram,
            Dimensions = "15.5x2.0x1.2",
            Length = 15.5m,
            Width = 2.0m,
            Height = 1.2m,
            DimensionUnit = DimensionUnit.Centimeter,
            Status = ProductStatus.Active,
            Condition = ProductCondition.New,
            Visibility = ProductVisibility.Public,
            IsFeatured = true,
            IsNew = true,
            IsBestseller = false,
            IsLimitedEdition = false,
            IsDiscontinued = false,
            AvailableFrom = new DateTime(2023, 12, 2, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddDays(-30)
            Rating = 4.5m,
            ReviewCount = 25,
            ViewCount = 1250,
            SoldCount = 150,
            WishlistCount = 45,
            MetaTitle = "Pocky Chocolate Sticks - Authentic Japanese Snack",
            MetaDescription = "Buy authentic Pocky chocolate biscuit sticks online. Premium Japanese snack perfect for sharing.",
            MetaKeywords = "pocky, chocolate, japanese snack, biscuit sticks, glico",
            Tags = "New Arrival, Best Seller, Japanese Snack",
            MarketingDescription = "The most beloved Japanese snack worldwide!",
            IsGiftWrappingAvailable = true,
            GiftWrappingFee = 10000m,
            DisplayOrder = 1,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = productCreationDate,
            UpdatedAt = productUpdateDate
        },

        new Product
        {
            Id = 2,
            Name = "Matcha Green Tea Powder - Ceremonial Grade",
            SKU = "MATCHA-PREM-001",
            Slug = "matcha-green-tea-powder-ceremonial",
            ShortDescription = "Premium ceremonial grade matcha powder from Uji, Kyoto",
            Description = "Authentic Japanese ceremonial grade matcha green tea powder from the renowned tea fields of Uji, Kyoto. Perfect for traditional tea ceremony, matcha lattes, and baking. Rich in antioxidants and L-theanine.",
            MainImage = "/images/products/matcha-powder.jpg",
            Price = 350000m,
            OriginalPrice = 400000m,
            CostPrice = 250000m,
            Stock = 100,
            MinStock = 10,
            MaxStock = 200,
            TrackInventory = true,
            AllowBackorder = false,
            AllowPreorder = true,
            BrandId = 4,
            CategoryId = 1, // Food & Beverages
            Origin = "Uji, Kyoto",
            JapaneseRegion = JapaneseOrigin.Kyoto, // Fixed: was Osaka
            AuthenticityLevel = AuthenticityLevel.Certified, // Fixed: was Verified
            AuthenticityInfo = "Certified ceremonial grade matcha from century-old tea gardens in Uji",
            UsageGuide = "Sift powder, add 70°C water, whisk in M-shape motion until frothy. 1-2g per serving.",
            Ingredients = "100% pure stone-ground green tea leaves (Tencha)",
            ExpiryDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddYears(2)
            ManufactureDate = new DateTime(2023, 11, 1, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddMonths(-2)
            BatchNumber = "MTH20231201",
            AgeRestriction = AgeRestriction.None,
            Weight = 100m,
            WeightUnit = WeightUnit.Gram,
            Dimensions = "8.0x8.0x8.0",
            Length = 8.0m,
            Width = 8.0m,
            Height = 8.0m,
            DimensionUnit = DimensionUnit.Centimeter,
            Status = ProductStatus.Active,
            Condition = ProductCondition.New,
            Visibility = ProductVisibility.Public,
            IsFeatured = true,
            IsNew = false,
            IsBestseller = true,
            IsLimitedEdition = false,
            IsDiscontinued = false,
            AvailableFrom = new DateTime(2023, 11, 2, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddDays(-60)
            Rating = 4.8m,
            ReviewCount = 42,
            ViewCount = 2100,
            SoldCount = 89,
            WishlistCount = 78,
            MetaTitle = "Premium Ceremonial Matcha Powder - Uji Kyoto",
            MetaDescription = "Authentic ceremonial grade matcha from Uji, Kyoto. Perfect for tea ceremony and culinary use.",
            MetaKeywords = "matcha, green tea, ceremonial grade, uji, kyoto, japanese tea",
            Tags = "Premium, Best Seller, Traditional, Organic",
            MarketingDescription = "Experience the authentic taste of Japanese tea ceremony",
            IsGiftWrappingAvailable = true,
            GiftWrappingFee = 15000m,
            DisplayOrder = 2,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = productCreationDate,
            UpdatedAt = productUpdateDate
        },

        // Beauty & Health Category
        new Product
        {
            Id = 3,
            Name = "Shiseido Ultimune Power Infusing Concentrate",
            SKU = "SHIS-ULTI-050",
            Slug = "shiseido-ultimune-power-concentrate",
            ShortDescription = "Revolutionary anti-aging serum that strengthens skin immunity",
            Description = "Shiseido's breakthrough anti-aging serum featuring ImuGeneration Technology™. Strengthens skin's natural defenses against daily damage while improving texture and radiance. Suitable for all skin types.",
            MainImage = "/images/products/shiseido-ultimune.jpg",
            Price = 2800000m,
            OriginalPrice = 3200000m,
            CostPrice = 2000000m,
            Stock = 50,
            MinStock = 5,
            MaxStock = 100,
            TrackInventory = true,
            AllowBackorder = false,
            AllowPreorder = true,
            BrandId = 1, // Shiseido
            CategoryId = 2, // Beauty & Health
            Origin = "Tokyo",
            JapaneseRegion = JapaneseOrigin.Tokyo, // Fixed: was Hokkaido
            AuthenticityLevel = AuthenticityLevel.Authorized, // Fixed: was Certified
            AuthenticityInfo = "Official Shiseido product with hologram authentication",
            UsageGuide = "Apply 2 pumps to clean face morning and evening before moisturizer",
            Ingredients = "Water, Butylene Glycol, Alcohol, Glycerin, PEG-75, Rosa Damascena Flower Water",
            ExpiryDate = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddYears(3)
            ManufactureDate = new DateTime(2023, 10, 1, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddMonths(-3)
            BatchNumber = "SHI20231001",
            AgeRestriction = AgeRestriction.Over18, // Fixed: was Adult
            Weight = 200m,
            WeightUnit = WeightUnit.Gram,
            Dimensions = "5.0x5.0x12.0",
            Length = 5.0m,
            Width = 5.0m,
            Height = 12.0m,
            DimensionUnit = DimensionUnit.Centimeter,
            Status = ProductStatus.Active,
            Condition = ProductCondition.New,
            Visibility = ProductVisibility.Public,
            IsFeatured = true,
            IsNew = false,
            IsBestseller = true,
            IsLimitedEdition = false,
            IsDiscontinued = false,
            AvailableFrom = new DateTime(2023, 10, 2, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddDays(-90)
            Rating = 4.7m,
            ReviewCount = 38,
            ViewCount = 3200,
            SoldCount = 67,
            WishlistCount = 125,
            MetaTitle = "Shiseido Ultimune Anti-Aging Serum - Power Concentrate",
            MetaDescription = "Transform your skin with Shiseido Ultimune Power Infusing Concentrate. Advanced anti-aging technology for radiant skin.",
            MetaKeywords = "shiseido, ultimune, anti-aging, serum, skincare, japanese cosmetics",
            Tags = "Premium, Anti-Aging, Best Seller, Luxury",
            MarketingDescription = "Unlock your skin's youth potential with cutting-edge Japanese technology",
            IsGiftWrappingAvailable = true,
            GiftWrappingFee = 25000m,
            DisplayOrder = 3,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = productCreationDate,
            UpdatedAt = productUpdateDate
        },

        new Product
        {
            Id = 4,
            Name = "Japanese Marine Collagen Supplement - 30 Days",
            SKU = "COL-MARIN-030",
            Slug = "japanese-marine-collagen-supplement",
            ShortDescription = "Premium marine collagen peptides for skin, hair, and joint health",
            Description = "High-quality marine collagen supplement sourced from Japanese waters. Contains low molecular weight peptides for maximum absorption. Supports skin elasticity, hair strength, nail health, and joint mobility.",
            MainImage = "/images/products/marine-collagen.jpg",
            Price = 850000m,
            OriginalPrice = 950000m,
            CostPrice = 600000m,
            Stock = 200,
            MinStock = 20,
            MaxStock = 500,
            TrackInventory = true,
            AllowBackorder = true,
            AllowPreorder = false,
            BrandId = 5, // Fixed: was 2, should be null
            CategoryId = 2, // Beauty & Health
            Origin = "Hokkaido",
            JapaneseRegion = JapaneseOrigin.Hokkaido,
            AuthenticityLevel = AuthenticityLevel.Verified,
            AuthenticityInfo = "Lab-tested for purity and potency, certified by JFRL",
            UsageGuide = "Take 1 sachet daily mixed with water or your favorite beverage. Best taken on empty stomach.",
            Ingredients = "Marine Collagen Peptides (Fish), Vitamin C, Hyaluronic Acid, Elastin",
            ExpiryDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddYears(2)
            ManufactureDate = new DateTime(2023, 12, 15, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddMonths(-1)
            BatchNumber = "COL20240115",
            AgeRestriction = AgeRestriction.Over18, // Fixed: was Adult
            Weight = 300m,
            WeightUnit = WeightUnit.Gram,
            Dimensions = "12.0x8.0x15.0",
            Length = 12.0m,
            Width = 8.0m,
            Height = 15.0m,
            DimensionUnit = DimensionUnit.Centimeter,
            Status = ProductStatus.Active,
            Condition = ProductCondition.New,
            Visibility = ProductVisibility.Public,
            IsFeatured = false,
            IsNew = true,
            IsBestseller = false,
            IsLimitedEdition = false,
            IsDiscontinued = false,
            AvailableFrom = new DateTime(2023, 12, 17, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddDays(-15)
            Rating = 4.3m,
            ReviewCount = 18,
            ViewCount = 890,
            SoldCount = 45,
            WishlistCount = 32,
            MetaTitle = "Japanese Marine Collagen Supplement - Premium Quality",
            MetaDescription = "Premium Japanese marine collagen supplement for healthy skin and joints. High absorption rate for maximum benefits.",
            MetaKeywords = "marine collagen, supplement, japanese, skin health, joint health, beauty",
            Tags = "New Arrival, Health, Beauty from Within, Natural",
            MarketingDescription = "Beauty starts from within - nourish your body with premium marine collagen",
            IsGiftWrappingAvailable = false,
            DisplayOrder = 4,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = productCreationDate,
            UpdatedAt = productUpdateDate
        },

        // Electronics Category
        new Product
            {
                Id = 5,
                Name = "Sony WH-1000XM5 Wireless Noise Canceling Headphones",
                SKU = "SONY-WH1000XM5-BK",
                Slug = "sony-wh-1000xm5-wireless-headphones",
                ShortDescription = "Industry-leading noise canceling wireless headphones with 30-hour battery",
                Description = "Sony's flagship wireless noise canceling headphones featuring advanced V1 processor and dual noise sensor technology. Exceptional sound quality with 30-hour battery life and quick charge capability.",
                MainImage = "/images/products/sony-wh1000xm5.jpg",
                Price = 8500000m,
                OriginalPrice = 9000000m,
                CostPrice = 6500000m,
                Stock = 75,
                MinStock = 10,
                MaxStock = 150,
                TrackInventory = true,
                AllowBackorder = false,
                AllowPreorder = true,
                BrandId = 2, // Sony
                CategoryId = 4, // Electronics
                Origin = "Tokyo",
                JapaneseRegion = JapaneseOrigin.Tokyo,
                AuthenticityLevel = AuthenticityLevel.Authorized,
                AuthenticityInfo = "Official Sony product with international warranty",
                UsageGuide = "Charge fully before first use. Download Sony Headphones Connect app for customization.",
                Ingredients = "Plastic housing, Memory foam padding, Electronic components, Metal headband", // THÊM
                ExpiryDate = new DateTime(2029, 1, 1, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddYears(5) (warranty period)
                ManufactureDate = new DateTime(2023, 9, 1, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddMonths(-4)
                BatchNumber = "SNY20231120",
                AgeRestriction = AgeRestriction.None,
                Weight = 250m,
                WeightUnit = WeightUnit.Gram,
                Dimensions = "21.0x18.0x8.0",
                Length = 21.0m,
                Width = 18.0m,
                Height = 8.0m,
                DimensionUnit = DimensionUnit.Centimeter,
                Status = ProductStatus.Active,
                Condition = ProductCondition.New,
                Visibility = ProductVisibility.Public,
                IsFeatured = true,
                IsNew = false,
                IsBestseller = true,
                IsLimitedEdition = false,
                IsDiscontinued = false,
                AvailableFrom = new DateTime(2023, 9, 2, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddDays(-120)
                Rating = 4.9m,
                ReviewCount = 156,
                ViewCount = 8900,
                SoldCount = 234,
                WishlistCount = 445,
                MetaTitle = "Sony WH-1000XM5 Wireless Noise Canceling Headphones",
                MetaDescription = "Experience premium audio with Sony WH-1000XM5 wireless headphones. Industry-leading noise cancelation and 30-hour battery.",
                MetaKeywords = "sony, headphones, wireless, noise canceling, WH-1000XM5, premium audio",
                Tags = "Premium, Best Seller, Audio, Technology",
                MarketingDescription = "Silence the world, amplify your music",
                IsGiftWrappingAvailable = true,
                GiftWrappingFee = 30000m,
                DisplayOrder = 5,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = productCreationDate,
                UpdatedAt = productUpdateDate
            },

            // Product ID = 6 (UNIQLO Heattech)
            new Product
            {
                Id = 6,
                Name = "UNIQLO Heattech Ultra Warm Crew Neck Long Sleeve T-Shirt",
                SKU = "UNI-HEAT-ULTRA-M-BK",
                Slug = "uniqlo-heattech-ultra-warm-tshirt",
                ShortDescription = "Advanced heat-generating thermal innerwear with 2.25x more warmth",
                Description = "UNIQLO's most advanced Heattech fabric technology that generates heat using body moisture. Ultra Warm version provides 2.25 times more warmth than regular Heattech. Perfect base layer for cold weather.",
                MainImage = "/images/products/uniqlo-heattech.jpg",
                Price = 590000m,
                OriginalPrice = 650000m,
                CostPrice = 400000m,
                Stock = 300,
                MinStock = 30,
                MaxStock = 500,
                TrackInventory = true,
                AllowBackorder = true,
                AllowPreorder = false,
                BrandId = 6,
                CategoryId = 3, // Fashion
                Origin = "Tokyo",
                JapaneseRegion = JapaneseOrigin.Tokyo,
                AuthenticityLevel = AuthenticityLevel.Verified,
                AuthenticityInfo = "Authentic UNIQLO product with care label",
                UsageGuide = "Wear as base layer. Machine wash cold, hang dry. Do not iron directly on fabric.",
                Ingredients = "Acrylic, Polyester, Rayon, Polyurethane, Heattech fiber blend", // THÊM
                ExpiryDate = new DateTime(2034, 1, 1, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddYears(10) (long-lasting clothing)
                ManufactureDate = new DateTime(2023, 11, 1, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddMonths(-2)
                BatchNumber = "UNI20240101",
                AgeRestriction = AgeRestriction.None,
                Weight = 150m,
                WeightUnit = WeightUnit.Gram,
                Dimensions = "30.0x25.0x2.0",
                Length = 30.0m,
                Width = 25.0m,
                Height = 2.0m,
                DimensionUnit = DimensionUnit.Centimeter,
                Status = ProductStatus.Active,
                Condition = ProductCondition.New,
                Visibility = ProductVisibility.Public,
                IsFeatured = false,
                IsNew = false,
                IsBestseller = true,
                IsLimitedEdition = false,
                IsDiscontinued = false,
                AvailableFrom = new DateTime(2023, 11, 17, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddDays(-45)
                Rating = 4.4m,
                ReviewCount = 89,
                ViewCount = 2340,
                SoldCount = 178,
                WishlistCount = 67,
                MetaTitle = "UNIQLO Heattech Ultra Warm T-Shirt - Advanced Thermal",
                MetaDescription = "Stay warm with UNIQLO Heattech Ultra Warm crew neck t-shirt. Advanced thermal technology for maximum comfort.",
                MetaKeywords = "uniqlo, heattech, thermal wear, ultra warm, innerwear, winter clothing",
                Tags = "Best Seller, Winter Essential, Comfort, Technology",
                MarketingDescription = "Revolutionary warmth technology for the coldest days",
                IsGiftWrappingAvailable = true,
                GiftWrappingFee = 8000m,
                DisplayOrder = 6,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = productCreationDate,
                UpdatedAt = productUpdateDate
            },

            // Product ID = 7 (Tea Set)
            new Product
            {
                Id = 7,
                Name = "Traditional Japanese Ceramic Tea Set - Sakura Collection",
                SKU = "TEA-SET-SAKURA-001",
                Slug = "japanese-ceramic-tea-set-sakura",
                ShortDescription = "Handcrafted ceramic tea set with beautiful sakura blossom design",
                Description = "Exquisite traditional Japanese ceramic tea set featuring delicate sakura (cherry blossom) patterns. Includes teapot, 4 matching cups, and bamboo serving tray. Handcrafted by skilled Kutani artisans.",
                MainImage = "/images/products/ceramic-tea-set.jpg",
                Price = 1200000m,
                OriginalPrice = 1350000m,
                CostPrice = 800000m,
                Stock = 25,
                MinStock = 5,
                MaxStock = 50,
                TrackInventory = true,
                AllowBackorder = false,
                AllowPreorder = true,
                BrandId = 7,
                CategoryId = 5, // Home & Living
                Origin = "Ishikawa",
                JapaneseRegion = JapaneseOrigin.Other,
                AuthenticityLevel = AuthenticityLevel.Certified,
                AuthenticityInfo = "Authentic Kutani-yaki pottery with artisan signature",
                UsageGuide = "Hand wash only. Do not use in microwave or dishwasher. Handle with care.",
                Ingredients = "High-grade ceramic clay, Natural glazes, Lead-free pigments, Bamboo tray", // THÊM
                ExpiryDate = new DateTime(2074, 1, 1, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddYears(50) (lifetime ceramic)
                ManufactureDate = new DateTime(2023, 10, 1, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddMonths(-3)
                BatchNumber = "TEA20231215",
                AgeRestriction = AgeRestriction.None,
                Weight = 1200m,
                WeightUnit = WeightUnit.Gram,
                Dimensions = "35.0x25.0x15.0",
                Length = 35.0m,
                Width = 25.0m,
                Height = 15.0m,
                DimensionUnit = DimensionUnit.Centimeter,
                Status = ProductStatus.Active,
                Condition = ProductCondition.New,
                Visibility = ProductVisibility.Public,
                IsFeatured = true,
                IsNew = false,
                IsBestseller = false,
                IsLimitedEdition = true,
                IsDiscontinued = false,
                AvailableFrom = new DateTime(2023, 10, 18, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddDays(-75)
                Rating = 4.8m,
                ReviewCount = 12,
                ViewCount = 1890,
                SoldCount = 18,
                WishlistCount = 89,
                MetaTitle = "Japanese Ceramic Tea Set - Sakura Collection Kutani",
                MetaDescription = "Elegant Japanese ceramic tea set with sakura design. Handcrafted Kutani pottery perfect for traditional tea ceremony.",
                MetaKeywords = "japanese tea set, ceramic, kutani, sakura, handcrafted, tea ceremony",
                Tags = "Limited Edition, Handcrafted, Traditional, Premium, Art",
                MarketingDescription = "Bring the beauty of Japanese spring to your tea time",
                IsGiftWrappingAvailable = true,
                GiftWrappingFee = 50000m,
                DisplayOrder = 7,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = productCreationDate,
                UpdatedAt = productUpdateDate
            },

            // Product ID = 8 (Furoshiki)
            new Product
            {
                Id = 8,
                Name = "Furoshiki Wrapping Cloth Set - Modern Patterns",
                SKU = "FURO-SET-MOD-003",
                Slug = "furoshiki-wrapping-cloth-set-modern",
                ShortDescription = "Eco-friendly traditional Japanese wrapping cloths in contemporary designs",
                Description = "Beautiful set of 3 Furoshiki wrapping cloths in small, medium, and large sizes. Features modern interpretations of traditional Japanese patterns. Perfect eco-friendly alternative to gift wrap and versatile for carrying items.",
                MainImage = "/images/products/furoshiki-set.jpg",
                Price = 280000m,
                OriginalPrice = 320000m,
                CostPrice = 200000m,
                Stock = 150,
                MinStock = 15,
                MaxStock = 300,
                TrackInventory = true,
                AllowBackorder = true,
                AllowPreorder = false,
                BrandId = 7,
                CategoryId = 5, // Home & Living
                Origin = "Kyoto",
                JapaneseRegion = JapaneseOrigin.Kyoto,
                AuthenticityLevel = AuthenticityLevel.Verified,
                AuthenticityInfo = "Traditional Furoshiki technique with modern designs",
                UsageGuide = "Multiple folding techniques for different uses. Machine washable, air dry recommended.",
                Ingredients = "100% Cotton fabric, Eco-friendly dyes, Traditional Japanese patterns", // THÊM
                ExpiryDate = new DateTime(2044, 1, 1, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddYears(20) (durable fabric)
                ManufactureDate = new DateTime(2023, 12, 1, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddMonths(-1)
                BatchNumber = "FUR20240120",
                AgeRestriction = AgeRestriction.None,
                Weight = 200m,
                WeightUnit = WeightUnit.Gram,
                Dimensions = "25.0x20.0x3.0",
                Length = 25.0m,
                Width = 20.0m,
                Height = 3.0m,
                DimensionUnit = DimensionUnit.Centimeter,
                Status = ProductStatus.Active,
                Condition = ProductCondition.New,
                Visibility = ProductVisibility.Public,
                IsFeatured = false,
                IsNew = false,
                IsBestseller = false,
                IsLimitedEdition = false,
                IsDiscontinued = false,
                AvailableFrom = new DateTime(2023, 12, 2, 0, 0, 0, DateTimeKind.Utc), // Static date instead of DateTime.UtcNow.AddDays(-30)
                Rating = 4.2m,
                ReviewCount = 28,
                ViewCount = 1120,
                SoldCount = 67,
                WishlistCount = 34,
                MetaTitle = "Furoshiki Wrapping Cloth Set - Modern Japanese Patterns",
                MetaDescription = "Eco-friendly Japanese Furoshiki cloths with modern designs. Versatile for gift wrapping and everyday use.",
                MetaKeywords = "furoshiki, wrapping cloth, eco-friendly, japanese, sustainable, gift wrap",
                Tags = "Eco Friendly, Traditional, Sustainable, Versatile",
                MarketingDescription = "Wrap with purpose - beautiful, sustainable, and endlessly reusable",
                IsGiftWrappingAvailable = false,
                DisplayOrder = 8,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = productCreationDate,
                UpdatedAt = productUpdateDate
            }
        };

            builder.Entity<Product>().HasData(products);
        }


        private static void SeedSuperAdminUser(ModelBuilder builder)
        {
            var superAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            builder.Entity<User>().HasData(new User
            {
                Id = superAdminId,
                Email = "superadmin@sakurahome.com",
                UserName = "superadmin",
                EmailConfirmed = true,
                FirstName = "Super",
                LastName = "Admin",
                Role = UserRole.SuperAdmin,
                Status = AccountStatus.Active,
                PasswordHash = "AQAAAAIAAYagAAAAEEV7OG+DPOtR9KwqdzspiSFEm2Q00X7fYjWfn7fhRI0+8R/F1rFeEV1+CLyGtmKtxw==",
                SecurityStamp = "b1e2c3d4-5678-1234-9876-abcdefabcdef",
                ConcurrencyStamp = "11111111-1111-1111-1111-111111111111", 
                IsActive = true,
                IsDeleted = false,
                CreatedAt = new DateTime(2024, 1, 1),
                UpdatedAt = new DateTime(2024, 1, 1)
            });
        }
    }
}