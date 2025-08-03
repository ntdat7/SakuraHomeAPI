using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace SakuraHomeAPI.Migrations
{
    /// <inheritdoc />
    public partial class RestoreSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Re-insert SystemSettings
            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "Description", "IsPublic", "Key", "Type", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { 1, "General", new DateTime(2024, 1, 1), null, "Tên website", true, "SiteName", 1, new DateTime(2024, 1, 1), null, "Sakura Home" },
                    { 2, "General", new DateTime(2024, 1, 1), null, "Mô t? website", true, "SiteDescription", 1, new DateTime(2024, 1, 1), null, "Japanese Products E-commerce Platform" },
                    { 3, "General", new DateTime(2024, 1, 1), null, "??n v? ti?n t? m?c ??nh", true, "DefaultCurrency", 1, new DateTime(2024, 1, 1), null, "VND" },
                    { 4, "General", new DateTime(2024, 1, 1), null, "Ngôn ng? m?c ??nh", true, "DefaultLanguage", 1, new DateTime(2024, 1, 1), null, "vi" },
                    { 5, "Contact", new DateTime(2024, 1, 1), null, "Email liên h?", true, "ContactEmail", 1, new DateTime(2024, 1, 1), null, "contact@sakurahome.com" },
                    { 6, "Contact", new DateTime(2024, 1, 1), null, "S? ?i?n tho?i liên h?", true, "ContactPhone", 1, new DateTime(2024, 1, 1), null, "+84 123 456 789" },
                    { 7, "Security", new DateTime(2024, 1, 1), null, "Cho phép ??ng ký tài kho?n m?i", false, "EnableRegistration", 3, new DateTime(2024, 1, 1), null, "true" },
                    { 8, "Security", new DateTime(2024, 1, 1), null, "S? l?n ??ng nh?p sai t?i ?a", false, "MaxLoginAttempts", 2, new DateTime(2024, 1, 1), null, "5" }
                });

            // Re-insert Categories
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "ChildrenCount", "Color", "CommissionRate", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "Icon", "ImageUrl", "IsActive", "IsDeleted", "IsFeatured", "Level", "MetaDescription", "MetaKeywords", "MetaTitle", "Name", "ParentId", "ProductCount", "ShowInHome", "ShowInMenu", "Slug", "TotalProductCount", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, 0, "#FF6B6B", null, new DateTime(2024, 1, 1), null, null, null, "Japanese food and drinks including snacks, teas, and traditional ingredients", 1, "fas fa-utensils", "/images/categories/food-beverages.jpg", true, false, false, 0, "Discover authentic Japanese food and beverages at Sakura Home", "japanese food, beverages, snacks, tea", "Japanese Food & Beverages - Sakura Home", "Food & Beverages", null, 0, false, true, "food-beverages", 0, new DateTime(2024, 1, 1), null },
                    { 2, 0, "#4ECDC4", null, new DateTime(2024, 1, 1), null, null, null, "Japanese cosmetics, skincare, and health products", 2, "fas fa-spa", "/images/categories/beauty-health.jpg", true, false, false, 0, "Premium Japanese beauty and health products for your wellness", "japanese beauty, cosmetics, skincare, health", "Japanese Beauty & Health Products - Sakura Home", "Beauty & Health", null, 0, false, true, "beauty-health", 0, new DateTime(2024, 1, 1), null },
                    { 3, 0, "#45B7D1", null, new DateTime(2024, 1, 1), null, null, null, "Japanese fashion and accessories including clothing and bags", 3, "fas fa-tshirt", "/images/categories/fashion.jpg", true, false, false, 0, "Trendy Japanese fashion and accessories for every style", "japanese fashion, clothing, accessories, bags", "Japanese Fashion & Accessories - Sakura Home", "Fashion", null, 0, false, true, "fashion", 0, new DateTime(2024, 1, 1), null },
                    { 4, 0, "#96CEB4", null, new DateTime(2024, 1, 1), null, null, null, "Japanese electronics and gadgets including games and tech accessories", 4, "fas fa-laptop", "/images/categories/electronics.jpg", true, false, false, 0, "Latest Japanese electronics and innovative gadgets", "japanese electronics, gadgets, games, technology", "Japanese Electronics & Gadgets - Sakura Home", "Electronics", null, 0, false, true, "electronics", 0, new DateTime(2024, 1, 1), null },
                    { 5, 0, "#FFEAA7", null, new DateTime(2024, 1, 1), null, null, null, "Japanese home decoration and lifestyle products", 5, "fas fa-home", "/images/categories/home-living.jpg", true, false, false, 0, "Beautiful Japanese home decoration and lifestyle products", "japanese home, decoration, lifestyle, living", "Japanese Home & Living - Sakura Home", "Home & Living", null, 0, false, true, "home-living", 0, new DateTime(2024, 1, 1), null }
                });

            // Re-insert Brands
            migrationBuilder.InsertData(
                table: "Brands",
                columns: new[] { "Id", "AverageRating", "ContactEmail", "ContactPhone", "Country", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "FacebookUrl", "FollowerCount", "FoundedYear", "Headquarters", "InstagramUrl", "IsActive", "IsDeleted", "IsFeatured", "IsOfficial", "IsVerified", "LogoUrl", "MetaDescription", "MetaKeywords", "MetaTitle", "Name", "ProductCount", "ReviewCount", "Slug", "TwitterUrl", "UpdatedAt", "UpdatedBy", "Website", "YoutubeUrl" },
                values: new object[,]
                {
                    { 1, 0.0, "info@shiseido.com", "+81-3-3572-5111", "Japan", new DateTime(2024, 1, 1), null, null, null, "Premium Japanese cosmetics and skincare brand", 0, "https://www.facebook.com/Shiseido", 0, new DateTime(1872, 1, 1), "Tokyo, Japan", "https://www.instagram.com/shiseido", true, false, true, true, true, "/images/brands/shiseido-logo.png", "Discover Shiseido's premium Japanese beauty and skincare products", "shiseido, japanese cosmetics, skincare, beauty", "Shiseido - Premium Japanese Beauty", "Shiseido", 0, 0, "shiseido", "https://twitter.com/shiseido", new DateTime(2024, 1, 1), null, "https://www.shiseido.com", "https://www.youtube.com/user/shiseido" },
                    { 2, 0.0, "info@sony.com", "+81-3-6748-2111", "Japan", new DateTime(2024, 1, 1), null, null, null, "Japanese multinational electronics corporation", 0, "https://www.facebook.com/Sony", 0, new DateTime(1946, 1, 1), "Tokyo, Japan", "https://www.instagram.com/sony", true, false, true, true, true, "/images/brands/sony-logo.png", "Explore Sony's innovative Japanese electronics and entertainment products", "sony, japanese electronics, innovation, technology", "Sony - Japanese Electronics Innovation", "Sony", 0, 0, "sony", "https://twitter.com/sony", new DateTime(2024, 1, 1), null, "https://www.sony.com", "https://www.youtube.com/user/SonyElectronics" },
                    { 3, 0.0, "info@glico.com", "+81-6-6477-8352", "Japan", new DateTime(2024, 1, 1), null, null, null, "Famous Japanese snack brand by Glico", 0, "https://www.facebook.com/PockyUSA", 0, new DateTime(1966, 1, 1), "Osaka, Japan", "https://www.instagram.com/pocky_global", true, false, true, true, true, "/images/brands/pocky-logo.png", "Enjoy the famous Japanese Pocky snacks in various flavors", "pocky, japanese snacks, glico, biscuit sticks", "Pocky - Japanese Snack Sensation", "Pocky", 0, 0, "pocky", "https://twitter.com/PockyUSA", new DateTime(2024, 1, 1), null, "https://www.pocky.com", "https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw" },
                    { 4, 0.0, "info@ippodo-tea.co.jp", "+81-75-211-3421", "Japan", new DateTime(2024, 1, 1), null, null, null, "Traditional Japanese tea company established in Kyoto", 0, "https://www.facebook.com/IppodoTea", 0, new DateTime(1717, 1, 1), "Kyoto, Japan", "https://www.instagram.com/ippodo_tea", true, false, true, false, false, "/images/brands/ippodo-logo.png", "Premium Japanese tea including ceremonial matcha from Kyoto's oldest tea house", "ippodo, japanese tea, matcha, ceremonial grade, kyoto", "Ippodo Tea - Traditional Japanese Tea Master", "Ippodo Tea", 0, 0, "ippodo-tea", "https://twitter.com/ippodo_tea", new DateTime(2024, 1, 1), null, "https://www.ippodo-tea.co.jp", "https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw" },
                    { 5, 0.0, "info@meiji.com", "+81-3-3273-3001", "Japan", new DateTime(2024, 1, 1), null, null, null, "Japanese pharmaceutical and health food company", 0, "https://www.facebook.com/MeijiGlobal", 0, new DateTime(1906, 1, 1), "Tokyo, Japan", "https://www.instagram.com/meiji_global", true, false, true, false, false, "/images/brands/meiji-logo.png", "Trusted Japanese brand for health foods, supplements and nutritional products", "meiji, japanese health, supplements, collagen, nutrition", "Meiji - Japanese Health & Nutrition", "Meiji", 0, 0, "meiji", "https://twitter.com/meiji_global", new DateTime(2024, 1, 1), null, "https://www.meiji.com", "https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw" },
                    { 6, 0.0, "info@uniqlo.com", "+81-3-6252-5181", "Japan", new DateTime(2024, 1, 1), null, null, null, "Japanese casual wear designer, manufacturer and retailer", 0, "https://www.facebook.com/uniqlo", 0, new DateTime(1984, 1, 1), "Tokyo, Japan", "https://www.instagram.com/uniqlo", true, false, true, true, true, "/images/brands/uniqlo-logo.png", "Innovative Japanese fashion brand with advanced fabric technology like Heattech", "uniqlo, japanese fashion, heattech, innovative clothing, casual wear", "UNIQLO - Japanese Fashion Innovation", "UNIQLO", 0, 0, "uniqlo", "https://twitter.com/uniqlo_global", new DateTime(2024, 1, 1), null, "https://www.uniqlo.com", "https://www.youtube.com/user/UNIQLO" },
                    { 7, 0.0, "info@kutani-kiln.com", "+81-761-57-3341", "Japan", new DateTime(2024, 1, 1), null, null, null, "Traditional Japanese ceramic and craft manufacturer from Ishikawa", 0, "https://www.facebook.com/KutaniKiln", 0, new DateTime(1655, 1, 1), "Ishikawa, Japan", "https://www.instagram.com/kutani_kiln", true, false, false, false, false, "/images/brands/kutani-logo.png", "Authentic Japanese ceramics and traditional crafts from the historic Kutani region", "kutani, japanese ceramics, traditional crafts, pottery, tea sets", "Kutani Kiln - Traditional Japanese Ceramics", "Kutani Kiln", 0, 0, "kutani-kiln", "https://twitter.com/uniqlo_global", new DateTime(2024, 1, 1), null, "https://www.kutani-kiln.com", "https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw" }
                });

            // Re-insert Tags, ProductAttributes, ShippingZones, NotificationTemplates, PaymentMethods
            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Color", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Name", "Slug", "UpdatedAt", "UpdatedBy", "UsageCount" },
                values: new object[,]
                {
                    { 1, "#FF6B6B", new DateTime(2024, 1, 1), null, null, null, "Newly arrived products", true, false, "New Arrival", "new-arrival", new DateTime(2024, 1, 1), null, 0 },
                    { 2, "#4ECDC4", new DateTime(2024, 1, 1), null, null, null, "Top selling products", true, false, "Best Seller", "best-seller", new DateTime(2024, 1, 1), null, 0 },
                    { 3, "#45B7D1", new DateTime(2024, 1, 1), null, null, null, "Limited edition products", true, false, "Limited Edition", "limited-edition", new DateTime(2024, 1, 1), null, 0 },
                    { 4, "#96CEB4", new DateTime(2024, 1, 1), null, null, null, "Environmentally friendly products", true, false, "Eco Friendly", "eco-friendly", new DateTime(2024, 1, 1), null, 0 }
                });

            migrationBuilder.InsertData(
                table: "ProductAttributes",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsActive", "IsDeleted", "IsFilterable", "IsRequired", "IsSearchable", "IsVariant", "MaxValue", "MinValue", "Name", "Options", "Type", "Unit", "UpdatedAt", "UpdatedBy", "ValidationRegex" },
                values: new object[,]
                {
                    { 1, "size", new DateTime(2024, 1, 1), null, null, null, "Product size variations", 1, true, false, true, false, false, true, null, null, "Size", "[\"XS\", \"S\", \"M\", \"L\", \"XL\", \"XXL\"]", 3, "", new DateTime(2024, 1, 1), null, "" },
                    { 2, "color", new DateTime(2024, 1, 1), null, null, null, "Product color variations", 2, true, false, true, false, false, true, null, null, "Color", "[\"Red\", \"Blue\", \"Green\", \"Yellow\", \"Black\", \"White\", \"Pink\", \"Purple\"]", 6, "", new DateTime(2024, 1, 1), null, "" },
                    { 3, "material", new DateTime(2024, 1, 1), null, null, null, "Product material composition", 3, true, false, true, false, true, false, null, null, "Material", "[\"Cotton\", \"Polyester\", \"Silk\", \"Wool\", \"Leather\", \"Plastic\", \"Metal\", \"Wood\"]", 3, "", new DateTime(2024, 1, 1), null, "" }
                });

            migrationBuilder.InsertData(
                table: "ShippingZones",
                columns: new[] { "Id", "Countries", "Description", "DisplayOrder", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "[\"VN\"]", "Khu v?c giao hàng trong n??c Vi?t Nam", 1, true, "Vi?t Nam" },
                    { 2, "[\"TH\", \"SG\", \"MY\", \"ID\", \"PH\", \"LA\", \"KH\", \"MM\", \"BN\"]", "Khu v?c giao hàng ?ông Nam Á", 2, true, "?ông Nam Á" },
                    { 3, "[\"US\", \"CA\", \"GB\", \"FR\", \"DE\", \"AU\", \"JP\", \"KR\", \"CN\"]", "Khu v?c giao hàng qu?c t?", 3, true, "Qu?c t?" }
                });

            migrationBuilder.InsertData(
                table: "NotificationTemplates",
                columns: new[] { "Id", "BodyTemplate", "IsActive", "Language", "Name", "Subject", "Type" },
                values: new object[,]
                {
                    { 1, "Xin chào {UserName}, c?m ?n b?n ?ã ??ng ký tài kho?n t?i Sakura Home.", true, "vi", "Welcome Email", "Chào m?ng ??n v?i Sakura Home!", "Email" },
                    { 2, "??n hàng {OrderNumber} c?a b?n ?ã ???c xác nh?n. T?ng ti?n: {TotalAmount}", true, "vi", "Order Confirmation", "Xác nh?n ??n hàng #{OrderNumber}", "Email" },
                    { 3, "??n hàng {OrderNumber} ?ã ???c giao cho ??n v? v?n chuy?n. Mã tracking: {TrackingNumber}", true, "vi", "Order Shipped", "??n hàng #{OrderNumber} ?ã ???c giao cho v?n chuy?n", "Email" }
                });

            migrationBuilder.InsertData(
                table: "PaymentMethods",
                columns: new[] { "Id", "Code", "Description", "DisplayOrder", "FeePercentage", "FixedFee", "IsActive", "LogoUrl", "MaxAmount", "MinAmount", "Name" },
                values: new object[,]
                {
                    { 1, "COD", "Thanh toán b?ng ti?n m?t khi nh?n hàng", 1, 0m, 0m, true, "/images/payment/cod.png", 5000000m, 0m, "Thanh toán khi nh?n hàng (COD)" },
                    { 2, "BANK_TRANSFER", "Chuy?n kho?n qua ngân hàng", 2, 0m, 0m, true, "/images/payment/bank-transfer.png", 0m, 0m, "Chuy?n kho?n ngân hàng" },
                    { 3, "MOMO", "Thanh toán qua ví ?i?n t? MoMo", 3, 2.5m, 0m, true, "/images/payment/momo.png", 50000000m, 10000m, "Ví MoMo" },
                    { 4, "ZALOPAY", "Thanh toán qua ví ?i?n t? ZaloPay", 4, 2.0m, 0m, true, "/images/payment/zalopay.png", 50000000m, 10000m, "ZaloPay" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete all the restored data
            migrationBuilder.DeleteData(table: "Brands", keyColumn: "Id", keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7 });
            migrationBuilder.DeleteData(table: "Categories", keyColumn: "Id", keyValues: new object[] { 1, 2, 3, 4, 5 });
            migrationBuilder.DeleteData(table: "SystemSettings", keyColumn: "Id", keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8 });
            migrationBuilder.DeleteData(table: "Tags", keyColumn: "Id", keyValues: new object[] { 1, 2, 3, 4 });
            migrationBuilder.DeleteData(table: "ProductAttributes", keyColumn: "Id", keyValues: new object[] { 1, 2, 3 });
            migrationBuilder.DeleteData(table: "ShippingZones", keyColumn: "Id", keyValues: new object[] { 1, 2, 3 });
            migrationBuilder.DeleteData(table: "NotificationTemplates", keyColumn: "Id", keyValues: new object[] { 1, 2, 3 });
            migrationBuilder.DeleteData(table: "PaymentMethods", keyColumn: "Id", keyValues: new object[] { 1, 2, 3, 4 });
        }
    }
}