using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SakuraHomeAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixDecimalPrecisionAndStaticDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MinValue",
                table: "ProductAttributes",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxValue",
                table: "ProductAttributes",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Brands",
                columns: new[] { "Id", "AverageRating", "ContactEmail", "ContactPhone", "Country", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "FacebookUrl", "FollowerCount", "FoundedYear", "Headquarters", "InstagramUrl", "IsActive", "IsDeleted", "IsFeatured", "IsOfficial", "IsVerified", "LogoUrl", "MetaDescription", "MetaKeywords", "MetaTitle", "Name", "ProductCount", "ReviewCount", "Slug", "TwitterUrl", "UpdatedAt", "UpdatedBy", "Website", "YoutubeUrl" },
                values: new object[,]
                {
                    { 1, 0.0, "info@shiseido.com", "+81-3-3572-5111", "Japan", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Premium Japanese cosmetics and skincare brand", 0, "https://www.facebook.com/Shiseido", 0, new DateTime(1872, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tokyo, Japan", "https://www.instagram.com/shiseido", true, false, true, true, true, "/images/brands/shiseido-logo.png", "Discover Shiseido's premium Japanese beauty and skincare products", "shiseido, japanese cosmetics, skincare, beauty", "Shiseido - Premium Japanese Beauty", "Shiseido", 0, 0, "shiseido", "https://twitter.com/shiseido", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "https://www.shiseido.com", "https://www.youtube.com/user/shiseido" },
                    { 2, 0.0, "info@sony.com", "+81-3-6748-2111", "Japan", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Japanese multinational electronics corporation", 0, "https://www.facebook.com/Sony", 0, new DateTime(1946, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tokyo, Japan", "https://www.instagram.com/sony", true, false, true, true, true, "/images/brands/sony-logo.png", "Explore Sony's innovative Japanese electronics and entertainment products", "sony, japanese electronics, innovation, technology", "Sony - Japanese Electronics Innovation", "Sony", 0, 0, "sony", "https://twitter.com/sony", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "https://www.sony.com", "https://www.youtube.com/user/SonyElectronics" },
                    { 3, 0.0, "info@glico.com", "+81-6-6477-8352", "Japan", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Famous Japanese snack brand by Glico", 0, "https://www.facebook.com/PockyUSA", 0, new DateTime(1966, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Osaka, Japan", "https://www.instagram.com/pocky_global", true, false, true, true, true, "/images/brands/pocky-logo.png", "Enjoy the famous Japanese Pocky snacks in various flavors", "pocky, japanese snacks, glico, biscuit sticks", "Pocky - Japanese Snack Sensation", "Pocky", 0, 0, "pocky", "https://twitter.com/PockyUSA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "https://www.pocky.com", "https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw" },
                    { 4, 0.0, "info@ippodo-tea.co.jp", "+81-75-211-3421", "Japan", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Traditional Japanese tea company established in Kyoto", 0, "https://www.facebook.com/IppodoTea", 0, new DateTime(1717, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kyoto, Japan", "https://www.instagram.com/ippodo_tea", true, false, true, false, false, "/images/brands/ippodo-logo.png", "Premium Japanese tea including ceremonial matcha from Kyoto's oldest tea house", "ippodo, japanese tea, matcha, ceremonial grade, kyoto", "Ippodo Tea - Traditional Japanese Tea Master", "Ippodo Tea", 0, 0, "ippodo-tea", "https://twitter.com/ippodo_tea", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "https://www.ippodo-tea.co.jp", "https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw" },
                    { 5, 0.0, "info@meiji.com", "+81-3-3273-3001", "Japan", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Japanese pharmaceutical and health food company", 0, "https://www.facebook.com/MeijiGlobal", 0, new DateTime(1906, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tokyo, Japan", "https://www.instagram.com/meiji_global", true, false, true, false, false, "/images/brands/meiji-logo.png", "Trusted Japanese brand for health foods, supplements and nutritional products", "meiji, japanese health, supplements, collagen, nutrition", "Meiji - Japanese Health & Nutrition", "Meiji", 0, 0, "meiji", "https://twitter.com/meiji_global", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "https://www.meiji.com", "https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw" },
                    { 6, 0.0, "info@uniqlo.com", "+81-3-6252-5181", "Japan", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Japanese casual wear designer, manufacturer and retailer", 0, "https://www.facebook.com/uniqlo", 0, new DateTime(1984, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tokyo, Japan", "https://www.instagram.com/uniqlo", true, false, true, true, true, "/images/brands/uniqlo-logo.png", "Innovative Japanese fashion brand with advanced fabric technology like Heattech", "uniqlo, japanese fashion, heattech, innovative clothing, casual wear", "UNIQLO - Japanese Fashion Innovation", "UNIQLO", 0, 0, "uniqlo", "https://twitter.com/uniqlo_global", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "https://www.uniqlo.com", "https://www.youtube.com/user/UNIQLO" },
                    { 7, 0.0, "info@kutani-kiln.com", "+81-761-57-3341", "Japan", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Traditional Japanese ceramic and craft manufacturer from Ishikawa", 0, "https://www.facebook.com/KutaniKiln", 0, new DateTime(1655, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ishikawa, Japan", "https://www.instagram.com/kutani_kiln", true, false, false, false, false, "/images/brands/kutani-logo.png", "Authentic Japanese ceramics and traditional crafts from the historic Kutani region", "kutani, japanese ceramics, traditional crafts, pottery, tea sets", "Kutani Kiln - Traditional Japanese Ceramics", "Kutani Kiln", 0, 0, "kutani-kiln", "https://twitter.com/uniqlo_global", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "https://www.kutani-kiln.com", "https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "ChildrenCount", "Color", "CommissionRate", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "Icon", "ImageUrl", "IsActive", "IsDeleted", "IsFeatured", "Level", "MetaDescription", "MetaKeywords", "MetaTitle", "Name", "ParentId", "ProductCount", "ShowInHome", "ShowInMenu", "Slug", "TotalProductCount", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, 0, "#FF6B6B", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Japanese food and drinks including snacks, teas, and traditional ingredients", 1, "fas fa-utensils", "/images/categories/food-beverages.jpg", true, false, false, 0, "Discover authentic Japanese food and beverages at Sakura Home", "japanese food, beverages, snacks, tea", "Japanese Food & Beverages - Sakura Home", "Food & Beverages", null, 0, false, true, "food-beverages", 0, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 2, 0, "#4ECDC4", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Japanese cosmetics, skincare, and health products", 2, "fas fa-spa", "/images/categories/beauty-health.jpg", true, false, false, 0, "Premium Japanese beauty and health products for your wellness", "japanese beauty, cosmetics, skincare, health", "Japanese Beauty & Health Products - Sakura Home", "Beauty & Health", null, 0, false, true, "beauty-health", 0, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 3, 0, "#45B7D1", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Japanese fashion and accessories including clothing and bags", 3, "fas fa-tshirt", "/images/categories/fashion.jpg", true, false, false, 0, "Trendy Japanese fashion and accessories for every style", "japanese fashion, clothing, accessories, bags", "Japanese Fashion & Accessories - Sakura Home", "Fashion", null, 0, false, true, "fashion", 0, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 4, 0, "#96CEB4", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Japanese electronics and gadgets including games and tech accessories", 4, "fas fa-laptop", "/images/categories/electronics.jpg", true, false, false, 0, "Latest Japanese electronics and innovative gadgets", "japanese electronics, gadgets, games, technology", "Japanese Electronics & Gadgets - Sakura Home", "Electronics", null, 0, false, true, "electronics", 0, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 5, 0, "#FFEAA7", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Japanese home decoration and lifestyle products", 5, "fas fa-home", "/images/categories/home-living.jpg", true, false, false, 0, "Beautiful Japanese home decoration and lifestyle products", "japanese home, decoration, lifestyle, living", "Japanese Home & Living - Sakura Home", "Home & Living", null, 0, false, true, "home-living", 0, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null }
                });

            migrationBuilder.InsertData(
                table: "NotificationTemplates",
                columns: new[] { "Id", "BodyTemplate", "IsActive", "Language", "Name", "Subject", "Type" },
                values: new object[,]
                {
                    { 1, "Xin chào {UserName}, cảm ơn bạn đã đăng ký tài khoản tại Sakura Home.", true, "vi", "Welcome Email", "Chào mừng đến với Sakura Home!", "Email" },
                    { 2, "Đơn hàng {OrderNumber} của bạn đã được xác nhận. Tổng tiền: {TotalAmount}", true, "vi", "Order Confirmation", "Xác nhận đơn hàng #{OrderNumber}", "Email" },
                    { 3, "Đơn hàng {OrderNumber} đã được giao cho đơn vị vận chuyển. Mã tracking: {TrackingNumber}", true, "vi", "Order Shipped", "Đơn hàng #{OrderNumber} đã được giao cho vận chuyển", "Email" }
                });

            migrationBuilder.InsertData(
                table: "PaymentMethods",
                columns: new[] { "Id", "Code", "Description", "DisplayOrder", "FeePercentage", "FixedFee", "IsActive", "LogoUrl", "MaxAmount", "MinAmount", "Name" },
                values: new object[,]
                {
                    { 1, "COD", "Thanh toán bằng tiền mặt khi nhận hàng", 1, 0m, 0m, true, "/images/payment/cod.png", 5000000m, 0m, "Thanh toán khi nhận hàng (COD)" },
                    { 2, "BANK_TRANSFER", "Chuyển khoản qua ngân hàng", 2, 0m, 0m, true, "/images/payment/bank-transfer.png", 0m, 0m, "Chuyển khoản ngân hàng" },
                    { 3, "MOMO", "Thanh toán qua ví điện tử MoMo", 3, 2.5m, 0m, true, "/images/payment/momo.png", 50000000m, 10000m, "Ví MoMo" },
                    { 4, "ZALOPAY", "Thanh toán qua ví điện tử ZaloPay", 4, 2.0m, 0m, true, "/images/payment/zalopay.png", 50000000m, 10000m, "ZaloPay" }
                });

            migrationBuilder.InsertData(
                table: "ProductAttributes",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsActive", "IsDeleted", "IsFilterable", "IsRequired", "IsSearchable", "IsVariant", "MaxValue", "MinValue", "Name", "Options", "Type", "Unit", "UpdatedAt", "UpdatedBy", "ValidationRegex" },
                values: new object[,]
                {
                    { 1, "size", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Product size variations", 1, true, false, true, false, false, true, null, null, "Size", "[\"XS\", \"S\", \"M\", \"L\", \"XL\", \"XXL\"]", 3, "", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "" },
                    { 2, "color", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Product color variations", 2, true, false, true, false, false, true, null, null, "Color", "[\"Red\", \"Blue\", \"Green\", \"Yellow\", \"Black\", \"White\", \"Pink\", \"Purple\"]", 6, "", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "" },
                    { 3, "material", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Product material composition", 3, true, false, true, false, true, false, null, null, "Material", "[\"Cotton\", \"Polyester\", \"Silk\", \"Wool\", \"Leather\", \"Plastic\", \"Metal\", \"Wood\"]", 3, "", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "" }
                });

            migrationBuilder.InsertData(
                table: "ShippingZones",
                columns: new[] { "Id", "Countries", "Description", "DisplayOrder", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "[\"VN\"]", "Khu vực giao hàng trong nước Việt Nam", 1, true, "Việt Nam" },
                    { 2, "[\"TH\", \"SG\", \"MY\", \"ID\", \"PH\", \"LA\", \"KH\", \"MM\", \"BN\"]", "Khu vực giao hàng Đông Nam Á", 2, true, "Đông Nam Á" },
                    { 3, "[\"US\", \"CA\", \"GB\", \"FR\", \"DE\", \"AU\", \"JP\", \"KR\", \"CN\"]", "Khu vực giao hàng quốc tế", 3, true, "Quốc tế" }
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "Description", "IsPublic", "Key", "Type", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { 1, "General", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Tên website", true, "SiteName", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Sakura Home" },
                    { 2, "General", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Mô tả website", true, "SiteDescription", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Japanese Products E-commerce Platform" },
                    { 3, "General", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Đơn vị tiền tệ mặc định", true, "DefaultCurrency", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "VND" },
                    { 4, "General", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Ngôn ngữ mặc định", true, "DefaultLanguage", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "vi" },
                    { 5, "Contact", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Email liên hệ", true, "ContactEmail", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "contact@sakurahome.com" },
                    { 6, "Contact", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Số điện thoại liên hệ", true, "ContactPhone", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "+84 123 456 789" },
                    { 7, "Security", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Cho phép đăng ký tài khoản mới", false, "EnableRegistration", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "true" },
                    { 8, "Security", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Số lần đăng nhập sai tối đa", false, "MaxLoginAttempts", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "5" }
                });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Color", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Name", "Slug", "UpdatedAt", "UpdatedBy", "UsageCount" },
                values: new object[,]
                {
                    { 1, "#FF6B6B", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Newly arrived products", true, false, "New Arrival", "new-arrival", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0 },
                    { 2, "#4ECDC4", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Top selling products", true, false, "Best Seller", "best-seller", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0 },
                    { 3, "#45B7D1", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Limited edition products", true, false, "Limited Edition", "limited-edition", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0 },
                    { 4, "#96CEB4", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Environmentally friendly products", true, false, "Eco Friendly", "eco-friendly", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "AgeRestriction", "AllowBackorder", "AllowPreorder", "AuthenticityInfo", "AuthenticityLevel", "AvailableFrom", "AvailableUntil", "BatchNumber", "BrandId", "CartCount", "CategoryId", "Condition", "CostPrice", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DimensionUnit", "Dimensions", "DisplayOrder", "ExpiryDate", "GiftWrappingFee", "Height", "Ingredients", "IsActive", "IsBestseller", "IsDeleted", "IsDiscontinued", "IsFeatured", "IsGiftWrappingAvailable", "IsLimitedEdition", "IsNew", "JapaneseRegion", "LastSoldAt", "LastViewedAt", "Length", "MainImage", "ManufactureDate", "MarketingDescription", "MaxStock", "MetaDescription", "MetaKeywords", "MetaTitle", "MinStock", "Name", "Origin", "OriginalPrice", "Price", "Rating", "ReviewCount", "SKU", "ShortDescription", "Slug", "SoldCount", "Status", "Stock", "Tags", "TrackInventory", "UpdatedAt", "UpdatedBy", "UsageGuide", "ViewCount", "Visibility", "Weight", "WeightUnit", "Width", "WishlistCount" },
                values: new object[,]
                {
                    { 1, 0, false, false, "Authentic Glico Pocky imported directly from Japan", 3, new DateTime(2023, 12, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "PCK20240101", 3, 0, 1, 1, 30000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Delicious Japanese Pocky chocolate biscuit sticks. Perfect snack for any time of the day. Made with high-quality ingredients and authentic Japanese recipe.", 1, "15.5x2.0x1.2", 1, new DateTime(2024, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10000m, 1.2m, "Wheat flour, sugar, chocolate, vegetable oil, whole milk powder", true, false, false, false, true, true, false, true, 2, null, null, 15.5m, "/images/products/pocky-chocolate.jpg", new DateTime(2023, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "The most beloved Japanese snack worldwide!", 1000, "Buy authentic Pocky chocolate biscuit sticks online. Premium Japanese snack perfect for sharing.", "pocky, chocolate, japanese snack, biscuit sticks, glico", "Pocky Chocolate Sticks - Authentic Japanese Snack", 50, "Pocky Chocolate Sticks", "Osaka", 50000m, 45000m, 4.5m, 25, "POCKY-CHOC-001", "Classic Japanese chocolate biscuit sticks", "pocky-chocolate-sticks", 150, 1, 500, "New Arrival, Best Seller, Japanese Snack", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ready to eat. Store in cool, dry place.", 1250, 1, 47m, 1, 2.0m, 45 },
                    { 2, 0, false, true, "Certified ceremonial grade matcha from century-old tea gardens in Uji", 5, new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "MTH20231201", 4, 0, 1, 1, 250000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Authentic Japanese ceremonial grade matcha green tea powder from the renowned tea fields of Uji, Kyoto. Perfect for traditional tea ceremony, matcha lattes, and baking. Rich in antioxidants and L-theanine.", 1, "8.0x8.0x8.0", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 15000m, 8.0m, "100% pure stone-ground green tea leaves (Tencha)", true, true, false, false, true, true, false, false, 3, null, null, 8.0m, "/images/products/matcha-powder.jpg", new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Experience the authentic taste of Japanese tea ceremony", 200, "Authentic ceremonial grade matcha from Uji, Kyoto. Perfect for tea ceremony and culinary use.", "matcha, green tea, ceremonial grade, uji, kyoto, japanese tea", "Premium Ceremonial Matcha Powder - Uji Kyoto", 10, "Matcha Green Tea Powder - Ceremonial Grade", "Uji, Kyoto", 400000m, 350000m, 4.8m, 42, "MATCHA-PREM-001", "Premium ceremonial grade matcha powder from Uji, Kyoto", "matcha-green-tea-powder-ceremonial", 89, 1, 100, "Premium, Best Seller, Traditional, Organic", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Sift powder, add 70°C water, whisk in M-shape motion until frothy. 1-2g per serving.", 2100, 1, 100m, 1, 8.0m, 78 },
                    { 3, 18, false, true, "Official Shiseido product with hologram authentication", 7, new DateTime(2023, 10, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "SHI20231001", 1, 0, 2, 1, 2000000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Shiseido's breakthrough anti-aging serum featuring ImuGeneration Technology™. Strengthens skin's natural defenses against daily damage while improving texture and radiance. Suitable for all skin types.", 1, "5.0x5.0x12.0", 3, new DateTime(2027, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 25000m, 12.0m, "Water, Butylene Glycol, Alcohol, Glycerin, PEG-75, Rosa Damascena Flower Water", true, true, false, false, true, true, false, false, 1, null, null, 5.0m, "/images/products/shiseido-ultimune.jpg", new DateTime(2023, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Unlock your skin's youth potential with cutting-edge Japanese technology", 100, "Transform your skin with Shiseido Ultimune Power Infusing Concentrate. Advanced anti-aging technology for radiant skin.", "shiseido, ultimune, anti-aging, serum, skincare, japanese cosmetics", "Shiseido Ultimune Anti-Aging Serum - Power Concentrate", 5, "Shiseido Ultimune Power Infusing Concentrate", "Tokyo", 3200000m, 2800000m, 4.7m, 38, "SHIS-ULTI-050", "Revolutionary anti-aging serum that strengthens skin immunity", "shiseido-ultimune-power-concentrate", 67, 1, 50, "Premium, Anti-Aging, Best Seller, Luxury", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Apply 2 pumps to clean face morning and evening before moisturizer", 3200, 1, 200m, 1, 5.0m, 125 },
                    { 4, 18, true, false, "Lab-tested for purity and potency, certified by JFRL", 3, new DateTime(2023, 12, 17, 0, 0, 0, 0, DateTimeKind.Utc), null, "COL20240115", 5, 0, 2, 1, 600000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "High-quality marine collagen supplement sourced from Japanese waters. Contains low molecular weight peptides for maximum absorption. Supports skin elasticity, hair strength, nail health, and joint mobility.", 1, "12.0x8.0x15.0", 4, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, 15.0m, "Marine Collagen Peptides (Fish), Vitamin C, Hyaluronic Acid, Elastin", true, false, false, false, false, false, false, true, 4, null, null, 12.0m, "/images/products/marine-collagen.jpg", new DateTime(2023, 12, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Beauty starts from within - nourish your body with premium marine collagen", 500, "Premium Japanese marine collagen supplement for healthy skin and joints. High absorption rate for maximum benefits.", "marine collagen, supplement, japanese, skin health, joint health, beauty", "Japanese Marine Collagen Supplement - Premium Quality", 20, "Japanese Marine Collagen Supplement - 30 Days", "Hokkaido", 950000m, 850000m, 4.3m, 18, "COL-MARIN-030", "Premium marine collagen peptides for skin, hair, and joint health", "japanese-marine-collagen-supplement", 45, 1, 200, "New Arrival, Health, Beauty from Within, Natural", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Take 1 sachet daily mixed with water or your favorite beverage. Best taken on empty stomach.", 890, 1, 300m, 1, 8.0m, 32 },
                    { 5, 0, false, true, "Official Sony product with international warranty", 7, new DateTime(2023, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "SNY20231120", 2, 0, 4, 1, 6500000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Sony's flagship wireless noise canceling headphones featuring advanced V1 processor and dual noise sensor technology. Exceptional sound quality with 30-hour battery life and quick charge capability.", 1, "21.0x18.0x8.0", 5, new DateTime(2029, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30000m, 8.0m, "Plastic housing, Memory foam padding, Electronic components, Metal headband", true, true, false, false, true, true, false, false, 1, null, null, 21.0m, "/images/products/sony-wh1000xm5.jpg", new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Silence the world, amplify your music", 150, "Experience premium audio with Sony WH-1000XM5 wireless headphones. Industry-leading noise cancelation and 30-hour battery.", "sony, headphones, wireless, noise canceling, WH-1000XM5, premium audio", "Sony WH-1000XM5 Wireless Noise Canceling Headphones", 10, "Sony WH-1000XM5 Wireless Noise Canceling Headphones", "Tokyo", 9000000m, 8500000m, 4.9m, 156, "SONY-WH1000XM5-BK", "Industry-leading noise canceling wireless headphones with 30-hour battery", "sony-wh-1000xm5-wireless-headphones", 234, 1, 75, "Premium, Best Seller, Audio, Technology", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Charge fully before first use. Download Sony Headphones Connect app for customization.", 8900, 1, 250m, 1, 18.0m, 445 },
                    { 6, 0, true, false, "Authentic UNIQLO product with care label", 3, new DateTime(2023, 11, 17, 0, 0, 0, 0, DateTimeKind.Utc), null, "UNI20240101", 6, 0, 3, 1, 400000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "UNIQLO's most advanced Heattech fabric technology that generates heat using body moisture. Ultra Warm version provides 2.25 times more warmth than regular Heattech. Perfect base layer for cold weather.", 1, "30.0x25.0x2.0", 6, new DateTime(2034, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8000m, 2.0m, "Acrylic, Polyester, Rayon, Polyurethane, Heattech fiber blend", true, true, false, false, false, true, false, false, 1, null, null, 30.0m, "/images/products/uniqlo-heattech.jpg", new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Revolutionary warmth technology for the coldest days", 500, "Stay warm with UNIQLO Heattech Ultra Warm crew neck t-shirt. Advanced thermal technology for maximum comfort.", "uniqlo, heattech, thermal wear, ultra warm, innerwear, winter clothing", "UNIQLO Heattech Ultra Warm T-Shirt - Advanced Thermal", 30, "UNIQLO Heattech Ultra Warm Crew Neck Long Sleeve T-Shirt", "Tokyo", 650000m, 590000m, 4.4m, 89, "UNI-HEAT-ULTRA-M-BK", "Advanced heat-generating thermal innerwear with 2.25x more warmth", "uniqlo-heattech-ultra-warm-tshirt", 178, 1, 300, "Best Seller, Winter Essential, Comfort, Technology", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Wear as base layer. Machine wash cold, hang dry. Do not iron directly on fabric.", 2340, 1, 150m, 1, 25.0m, 67 },
                    { 7, 0, false, true, "Authentic Kutani-yaki pottery with artisan signature", 5, new DateTime(2023, 10, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, "TEA20231215", 7, 0, 5, 1, 800000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Exquisite traditional Japanese ceramic tea set featuring delicate sakura (cherry blossom) patterns. Includes teapot, 4 matching cups, and bamboo serving tray. Handcrafted by skilled Kutani artisans.", 1, "35.0x25.0x15.0", 7, new DateTime(2074, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 50000m, 15.0m, "High-grade ceramic clay, Natural glazes, Lead-free pigments, Bamboo tray", true, false, false, false, true, true, true, false, 99, null, null, 35.0m, "/images/products/ceramic-tea-set.jpg", new DateTime(2023, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bring the beauty of Japanese spring to your tea time", 50, "Elegant Japanese ceramic tea set with sakura design. Handcrafted Kutani pottery perfect for traditional tea ceremony.", "japanese tea set, ceramic, kutani, sakura, handcrafted, tea ceremony", "Japanese Ceramic Tea Set - Sakura Collection Kutani", 5, "Traditional Japanese Ceramic Tea Set - Sakura Collection", "Ishikawa", 1350000m, 1200000m, 4.8m, 12, "TEA-SET-SAKURA-001", "Handcrafted ceramic tea set with beautiful sakura blossom design", "japanese-ceramic-tea-set-sakura", 18, 1, 25, "Limited Edition, Handcrafted, Traditional, Premium, Art", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Hand wash only. Do not use in microwave or dishwasher. Handle with care.", 1890, 1, 1200m, 1, 25.0m, 89 },
                    { 8, 0, true, false, "Traditional Furoshiki technique with modern designs", 3, new DateTime(2023, 12, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "FUR20240120", 7, 0, 5, 1, 200000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Beautiful set of 3 Furoshiki wrapping cloths in small, medium, and large sizes. Features modern interpretations of traditional Japanese patterns. Perfect eco-friendly alternative to gift wrap and versatile for carrying items.", 1, "25.0x20.0x3.0", 8, new DateTime(2044, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3.0m, "100% Cotton fabric, Eco-friendly dyes, Traditional Japanese patterns", true, false, false, false, false, false, false, false, 3, null, null, 25.0m, "/images/products/furoshiki-set.jpg", new DateTime(2023, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Wrap with purpose - beautiful, sustainable, and endlessly reusable", 300, "Eco-friendly Japanese Furoshiki cloths with modern designs. Versatile for gift wrapping and everyday use.", "furoshiki, wrapping cloth, eco-friendly, japanese, sustainable, gift wrap", "Furoshiki Wrapping Cloth Set - Modern Japanese Patterns", 15, "Furoshiki Wrapping Cloth Set - Modern Patterns", "Kyoto", 320000m, 280000m, 4.2m, 28, "FURO-SET-MOD-003", "Eco-friendly traditional Japanese wrapping cloths in contemporary designs", "furoshiki-wrapping-cloth-set-modern", 67, 1, 150, "Eco Friendly, Traditional, Sustainable, Versatile", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Multiple folding techniques for different uses. Machine washable, air dry recommended.", 1120, 1, 200m, 1, 20.0m, 34 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ProductAttributes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ProductAttributes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ProductAttributes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ShippingZones",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ShippingZones",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ShippingZones",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.AlterColumn<decimal>(
                name: "MinValue",
                table: "ProductAttributes",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxValue",
                table: "ProductAttributes",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);
        }
    }
}
