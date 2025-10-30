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
        /// <summary>
        /// SEED DATA HAS BEEN DISABLED FOR CLEAN PROJECT DISTRIBUTION
        /// Team members should use SQL scripts to populate database instead
        /// See: Scripts/SeedData/ folder for complete database setup
        /// </summary>
        public static void SeedData(ModelBuilder builder)
        {
            // SEED DATA DISABLED - Use SQL scripts instead
            // Uncomment individual methods below if needed for development
            
            /*
            SeedSystemSettings(builder);
            SeedCategories(builder);
            SeedBrands(builder);
            SeedProductAttributes(builder);
            SeedTags(builder);
            SeedShippingZones(builder);
            SeedNotificationTemplates(builder);
            SeedPaymentMethods(builder);
            SeedProducts(builder);
            //SeedSuperAdminUser(builder);
            */
        }

        // All seed methods are preserved but not called by default
        // Team members can uncomment specific methods if needed for development

        /// <summary>
        /// Seed system settings - DISABLED
        /// </summary>
        private static void SeedSystemSettings(ModelBuilder builder)
        {
            // COMMENTED OUT - Use SQL script instead
            /*
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
                // ... other settings
            };

            builder.Entity<SystemSetting>().HasData(settingsData);
            */
        }

        /// <summary>
        /// Seed default categories - DISABLED
        /// </summary>
        private static void SeedCategories(ModelBuilder builder)
        {
            // COMMENTED OUT - Use SQL script instead
            /*
            var categories = new[]
            {
                new Category
                {
                    Id = 1,
                    Name = "Food & Beverages",
                    Slug = "food-beverages",
                    // ... other properties
                },
                // ... other categories
            };

            builder.Entity<Category>().HasData(categories);
            */
        }

        /// <summary>
        /// Seed default brands - DISABLED
        /// </summary>
        private static void SeedBrands(ModelBuilder builder)
        {
            // COMMENTED OUT - Use SQL script instead
            /*
            var brands = new[]
            {
                new Brand
                {
                    Id = 1,
                    Name = "Shiseido",
                    Slug = "shiseido",
                    // ... other properties
                },
                // ... other brands
            };

            builder.Entity<Brand>().HasData(brands);
            */
        }

        /// <summary>
        /// Seed product attributes - DISABLED
        /// </summary>
        private static void SeedProductAttributes(ModelBuilder builder)
        {
            // COMMENTED OUT - Use SQL script instead
            /*
            var attributes = new[]
            {
                new ProductAttribute
                {
                    Id = 1,
                    Name = "Size",
                    Code = "size",
                    // ... other properties
                },
                // ... other attributes
            };

            builder.Entity<ProductAttribute>().HasData(attributes);
            */
        }

        /// <summary>
        /// Seed product tags - DISABLED
        /// </summary>
        private static void SeedTags(ModelBuilder builder)
        {
            // COMMENTED OUT - Use SQL script instead
            /*
            var tags = new[]
            {
                new Tag
                {
                    Id = 1,
                    Name = "New Arrival",
                    Slug = "new-arrival",
                    // ... other properties
                },
                // ... other tags
            };

            builder.Entity<Tag>().HasData(tags);
            */
        }

        /// <summary>
        /// Seed shipping zones - DISABLED
        /// </summary>
        private static void SeedShippingZones(ModelBuilder builder)
        {
            // COMMENTED OUT - Use SQL script instead
            /*
            var shippingZones = new[]
            {
                new ShippingZone
                {
                    Id = 1,
                    Name = "Việt Nam",
                    Description = "Khu vực giao hàng trong nước Việt Nam",
                    // ... other properties
                },
                // ... other shipping zones
            };

            builder.Entity<ShippingZone>().HasData(shippingZones);
            */
        }

        /// <summary>
        /// Seed notification templates - DISABLED
        /// </summary>
        private static void SeedNotificationTemplates(ModelBuilder builder)
        {
            // COMMENTED OUT - Use SQL script instead
            /*
            var templates = new[]
            {
                new NotificationTemplate
                {
                    Id = 1,
                    Name = "Welcome Email",
                    Type = "Email",
                    // ... other properties
                },
                // ... other templates
            };

            builder.Entity<NotificationTemplate>().HasData(templates);
            */
        }

        /// <summary>
        /// Seed payment methods - DISABLED
        /// </summary>
        private static void SeedPaymentMethods(ModelBuilder builder)
        {
            // COMMENTED OUT - Use SQL script instead
            /*
            var paymentMethods = new[]
            {
                new PaymentMethodInfo
                {
                    Id = 1,
                    Name = "Thanh toán khi nhận hàng (COD)",
                    Description = "Thanh toán bằng tiền mặt khi nhận hàng",
                    // ... other properties
                },
                // ... other payment methods
            };

            builder.Entity<PaymentMethodInfo>().HasData(paymentMethods);
            */
        }

        /// <summary>
        /// Seed sample products - DISABLED
        /// </summary>
        private static void SeedProducts(ModelBuilder builder)
        {
            // COMMENTED OUT - Use SQL script instead
            /*
            var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var productCreationDate = baseDate;
            var productUpdateDate = baseDate;

            var products = new[]
            {
                new Product
                {
                    Id = 1,
                    Name = "Pocky Chocolate Sticks",
                    SKU = "POCKY-CHOC-001",
                    // ... other properties
                },
                // ... other products
            };

            builder.Entity<Product>().HasData(products);
            */
        }

        // Method to enable seed data for development (optional)
        /// <summary>
        /// Enable all seed data for development environment
        /// Call this method in development if you need seed data
        /// </summary>
        public static void EnableSeedDataForDevelopment(ModelBuilder builder)
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
        }
    }
}