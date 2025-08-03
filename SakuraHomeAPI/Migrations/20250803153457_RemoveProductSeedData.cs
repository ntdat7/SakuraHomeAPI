using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SakuraHomeAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_PaymentMethods_PaymentMethodDetailsId",
                table: "Orders");

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

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Wishlists",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "TrackingNumber",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCarrier",
                table: "Orders",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnReason",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverEmail",
                table: "Orders",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentMethodDetailsId",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "GiftMessage",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerNotes",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CouponCode",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "CancelReason",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BillingAddress",
                table: "Orders",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "AdminNotes",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_PaymentMethods_PaymentMethodDetailsId",
                table: "Orders",
                column: "PaymentMethodDetailsId",
                principalTable: "PaymentMethods",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_PaymentMethods_PaymentMethodDetailsId",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Wishlists",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrackingNumber",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCarrier",
                table: "Orders",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnReason",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverEmail",
                table: "Orders",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentMethodDetailsId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GiftMessage",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerNotes",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CouponCode",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CancelReason",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BillingAddress",
                table: "Orders",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AdminNotes",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Brands",
                columns: new[] { "Id", "AverageRating", "ContactEmail", "ContactPhone", "Country", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "FacebookUrl", "FollowerCount", "FoundedYear", "Headquarters", "InstagramUrl", "IsActive", "IsDeleted", "IsFeatured", "IsOfficial", "IsVerified", "LogoUrl", "MetaDescription", "MetaKeywords", "MetaTitle", "Name", "ProductCount", "ReviewCount", "Slug", "TwitterUrl", "UpdatedAt", "UpdatedBy", "Website", "YoutubeUrl" },
                values: new object[,]
                {
                    { 1, 0.0, "info@shiseido.com", "+81-3-3572-5111", "Japan", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Premium Japanese cosmetics and skincare brand", 0, "https://www.facebook.com/Shiseido", 0, new DateTime(1872, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tokyo, Japan", "https://www.instagram.com/shiseido", true, false, true, false, false, "/images/brands/shiseido-logo.png", "Discover Shiseido's premium Japanese beauty and skincare products", "shiseido, japanese cosmetics, skincare, beauty", "Shiseido - Premium Japanese Beauty", "Shiseido", 0, 0, "shiseido", "https://twitter.com/shiseido", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "https://www.shiseido.com", "https://www.youtube.com/user/shiseido" },
                    { 2, 0.0, "info@sony.com", "+81-3-6748-2111", "Japan", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Japanese multinational electronics corporation", 0, "https://www.facebook.com/Sony", 0, new DateTime(1946, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tokyo, Japan", "https://www.instagram.com/sony", true, false, true, false, false, "/images/brands/sony-logo.png", "Explore Sony's innovative Japanese electronics and entertainment products", "sony, japanese electronics, innovation, technology", "Sony - Japanese Electronics Innovation", "Sony", 0, 0, "sony", "https://twitter.com/sony", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "https://www.sony.com", "https://www.youtube.com/user/SonyElectronics" },
                    { 3, 0.0, "info@glico.com", "+81-6-6477-8352", "Japan", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Famous Japanese snack brand by Glico", 0, "https://www.facebook.com/PockyUSA", 0, new DateTime(1966, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Osaka, Japan", "https://www.instagram.com/pocky_global", true, false, true, false, false, "/images/brands/pocky-logo.png", "Enjoy the famous Japanese Pocky snacks in various flavors", "pocky, japanese snacks, glico, biscuit sticks", "Pocky - Japanese Snack Sensation", "Pocky", 0, 0, "pocky", "https://twitter.com/PockyUSA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "https://www.pocky.com", "https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw" }
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

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_PaymentMethods_PaymentMethodDetailsId",
                table: "Orders",
                column: "PaymentMethodDetailsId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
