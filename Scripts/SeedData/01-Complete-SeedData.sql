-- ================================================================
-- SAKURA HOME API - COMPLETE DATABASE SETUP SCRIPT
-- ================================================================
-- This script contains all seed data for the Sakura Home project
-- Run this script after database migrations to populate initial data
-- ================================================================

-- ================================================================
-- 1. SYSTEM SETTINGS
-- ================================================================
INSERT INTO [SystemSettings] ([Id], [Key], [Value], [Description], [Type], [Category], [IsPublic], [CreatedAt], [UpdatedAt])
VALUES
    (1, 'SiteName', 'Sakura Home', 'Tên website', 0, 'General', 1, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (2, 'SiteDescription', 'Japanese Products E-commerce Platform', 'Mô tả website', 0, 'General', 1, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (3, 'DefaultCurrency', 'VND', 'Đơn vị tiền tệ mặc định', 0, 'General', 1, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (4, 'DefaultLanguage', 'vi', 'Ngôn ngữ mặc định', 0, 'General', 1, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (5, 'ContactEmail', 'contact@sakurahome.com', 'Email liên hệ', 0, 'Contact', 1, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (6, 'ContactPhone', '+84 123 456 789', 'Số điện thoại liên hệ', 0, 'Contact', 1, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (7, 'EnableRegistration', 'true', 'Cho phép đăng ký tài khoản mới', 1, 'Security', 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (8, 'MaxLoginAttempts', '5', 'Số lần đăng nhập sai tối đa', 2, 'Security', 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000');

-- ================================================================
-- 2. CATEGORIES
-- ================================================================
INSERT INTO [Categories] ([Id], [Name], [Slug], [Description], [ImageUrl], [Icon], [Color], [MetaTitle], [MetaDescription], [MetaKeywords], [DisplayOrder], [IsActive], [IsDeleted], [CreatedAt], [UpdatedAt])
VALUES
    (1, 'Food & Beverages', 'food-beverages', 'Japanese food and drinks including snacks, teas, and traditional ingredients', '/images/categories/food-beverages.jpg', 'fas fa-utensils', '#FF6B6B', 'Japanese Food & Beverages - Sakura Home', 'Discover authentic Japanese food and beverages at Sakura Home', 'japanese food, beverages, snacks, tea', 1, 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (2, 'Beauty & Health', 'beauty-health', 'Japanese cosmetics, skincare, and health products', '/images/categories/beauty-health.jpg', 'fas fa-spa', '#4ECDC4', 'Japanese Beauty & Health Products - Sakura Home', 'Premium Japanese beauty and health products for your wellness', 'japanese beauty, cosmetics, skincare, health', 2, 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (3, 'Fashion', 'fashion', 'Japanese fashion and accessories including clothing and bags', '/images/categories/fashion.jpg', 'fas fa-tshirt', '#45B7D1', 'Japanese Fashion & Accessories - Sakura Home', 'Trendy Japanese fashion and accessories for every style', 'japanese fashion, clothing, accessories, bags', 3, 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (4, 'Electronics', 'electronics', 'Japanese electronics and gadgets including games and tech accessories', '/images/categories/electronics.jpg', 'fas fa-laptop', '#96CEB4', 'Japanese Electronics & Gadgets - Sakura Home', 'Latest Japanese electronics and innovative gadgets', 'japanese electronics, gadgets, games, technology', 4, 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (5, 'Home & Living', 'home-living', 'Japanese home decoration and lifestyle products', '/images/categories/home-living.jpg', 'fas fa-home', '#FFEAA7', 'Japanese Home & Living - Sakura Home', 'Beautiful Japanese home decoration and lifestyle products', 'japanese home, decoration, lifestyle, living', 5, 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000');

-- ================================================================
-- 3. BRANDS
-- ================================================================
INSERT INTO [Brands] ([Id], [Name], [Slug], [Description], [LogoUrl], [Website], [ContactEmail], [ContactPhone], [Country], [FoundedYear], [Headquarters], [MetaTitle], [MetaDescription], [MetaKeywords], [FacebookUrl], [InstagramUrl], [TwitterUrl], [YoutubeUrl], [IsVerified], [IsOfficial], [IsFeatured], [IsActive], [IsDeleted], [CreatedAt], [UpdatedAt])
VALUES
    (1, 'Shiseido', 'shiseido', 'Premium Japanese cosmetics and skincare brand', '/images/brands/shiseido-logo.png', 'https://www.shiseido.com', 'info@shiseido.com', '+81-3-3572-5111', 'Japan', '1872-01-01T00:00:00.0000000', 'Tokyo, Japan', 'Shiseido - Premium Japanese Beauty', 'Discover Shiseido''s premium Japanese beauty and skincare products', 'shiseido, japanese cosmetics, skincare, beauty', 'https://www.facebook.com/Shiseido', 'https://www.instagram.com/shiseido', 'https://twitter.com/shiseido', 'https://www.youtube.com/user/shiseido', 1, 1, 1, 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (2, 'Sony', 'sony', 'Japanese multinational electronics corporation', '/images/brands/sony-logo.png', 'https://www.sony.com', 'info@sony.com', '+81-3-6748-2111', 'Japan', '1946-01-01T00:00:00.0000000', 'Tokyo, Japan', 'Sony - Japanese Electronics Innovation', 'Explore Sony''s innovative Japanese electronics and entertainment products', 'sony, japanese electronics, innovation, technology', 'https://www.facebook.com/Sony', 'https://www.instagram.com/sony', 'https://twitter.com/sony', 'https://www.youtube.com/user/SonyElectronics', 1, 1, 1, 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (3, 'Pocky', 'pocky', 'Famous Japanese snack brand by Glico', '/images/brands/pocky-logo.png', 'https://www.pocky.com', 'info@glico.com', '+81-6-6477-8352', 'Japan', '1966-01-01T00:00:00.0000000', 'Osaka, Japan', 'Pocky - Japanese Snack Sensation', 'Enjoy the famous Japanese Pocky snacks in various flavors', 'pocky, japanese snacks, glico, biscuit sticks', 'https://www.facebook.com/PockyUSA', 'https://www.instagram.com/pocky_global', 'https://twitter.com/PockyUSA', 'https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw', 1, 1, 1, 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (4, 'Ippodo Tea', 'ippodo-tea', 'Traditional Japanese tea company established in Kyoto', '/images/brands/ippodo-logo.png', 'https://www.ippodo-tea.co.jp', 'info@ippodo-tea.co.jp', '+81-75-211-3421', 'Japan', '1717-01-01T00:00:00.0000000', 'Kyoto, Japan', 'Ippodo Tea - Traditional Japanese Tea Master', 'Premium Japanese tea including ceremonial matcha from Kyoto''s oldest tea house', 'ippodo, japanese tea, matcha, ceremonial grade, kyoto', 'https://www.facebook.com/IppodoTea', 'https://www.instagram.com/ippodo_tea', 'https://twitter.com/ippodo_tea', 'https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw', 0, 0, 1, 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (5, 'Meiji', 'meiji', 'Japanese pharmaceutical and health food company', '/images/brands/meiji-logo.png', 'https://www.meiji.com', 'info@meiji.com', '+81-3-3273-3001', 'Japan', '1906-01-01T00:00:00.0000000', 'Tokyo, Japan', 'Meiji - Japanese Health & Nutrition', 'Trusted Japanese brand for health foods, supplements and nutritional products', 'meiji, japanese health, supplements, collagen, nutrition', 'https://www.facebook.com/MeijiGlobal', 'https://www.instagram.com/meiji_global', 'https://twitter.com/meiji_global', 'https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw', 0, 0, 1, 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (6, 'UNIQLO', 'uniqlo', 'Japanese casual wear designer, manufacturer and retailer', '/images/brands/uniqlo-logo.png', 'https://www.uniqlo.com', 'info@uniqlo.com', '+81-3-6252-5181', 'Japan', '1984-01-01T00:00:00.0000000', 'Tokyo, Japan', 'UNIQLO - Japanese Fashion Innovation', 'Innovative Japanese fashion brand with advanced fabric technology like Heattech', 'uniqlo, japanese fashion, heattech, innovative clothing, casual wear', 'https://www.facebook.com/uniqlo', 'https://www.instagram.com/uniqlo', 'https://twitter.com/uniqlo_global', 'https://www.youtube.com/user/UNIQLO', 1, 1, 1, 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (7, 'Kutani Kiln', 'kutani-kiln', 'Traditional Japanese ceramic and craft manufacturer from Ishikawa', '/images/brands/kutani-logo.png', 'https://www.kutani-kiln.com', 'info@kutani-kiln.com', '+81-761-57-3341', 'Japan', '1655-01-01T00:00:00.0000000', 'Ishikawa, Japan', 'Kutani Kiln - Traditional Japanese Ceramics', 'Authentic Japanese ceramics and traditional crafts from the historic Kutani region', 'kutani, japanese ceramics, traditional crafts, pottery, tea sets', 'https://www.facebook.com/KutaniKiln', 'https://www.instagram.com/kutani_kiln', 'https://twitter.com/uniqlo_global', 'https://www.youtube.com/channel/UCKnLVZLq7pGOK8OGnJxZZjw', 0, 0, 0, 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000');

-- ================================================================
-- 4. PRODUCT ATTRIBUTES
-- ================================================================
INSERT INTO [ProductAttributes] ([Id], [Name], [Code], [Description], [Type], [IsRequired], [IsFilterable], [IsSearchable], [IsVariant], [DisplayOrder], [Options], [Unit], [ValidationRegex], [IsActive], [IsDeleted], [CreatedAt], [UpdatedAt])
VALUES
    (1, 'Size', 'size', 'Product size variations', 0, 0, 1, 0, 1, 1, '["XS", "S", "M", "L", "XL", "XXL"]', '', '', 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (2, 'Color', 'color', 'Product color variations', 1, 0, 1, 0, 1, 2, '["Red", "Blue", "Green", "Yellow", "Black", "White", "Pink", "Purple"]', '', '', 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (3, 'Material', 'material', 'Product material composition', 0, 0, 1, 1, 0, 3, '["Cotton", "Polyester", "Silk", "Wool", "Leather", "Plastic", "Metal", "Wood"]', '', '', 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000');

-- ================================================================
-- 5. TAGS
-- ================================================================
INSERT INTO [Tags] ([Id], [Name], [Slug], [Description], [Color], [IsActive], [IsDeleted], [CreatedAt], [UpdatedAt])
VALUES
    (1, 'New Arrival', 'new-arrival', 'Newly arrived products', '#FF6B6B', 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (2, 'Best Seller', 'best-seller', 'Top selling products', '#4ECDC4', 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (3, 'Limited Edition', 'limited-edition', 'Limited edition products', '#45B7D1', 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (4, 'Eco Friendly', 'eco-friendly', 'Environmentally friendly products', '#96CEB4', 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000');

-- ================================================================
-- 6. SHIPPING ZONES
-- ================================================================
INSERT INTO [ShippingZones] ([Id], [Name], [Description], [Countries], [IsActive], [DisplayOrder])
VALUES
    (1, 'Việt Nam', 'Khu vực giao hàng trong nước Việt Nam', '["VN"]', 1, 1),
    (2, 'Đông Nam Á', 'Khu vực giao hàng Đông Nam Á', '["TH", "SG", "MY", "ID", "PH", "LA", "KH", "MM", "BN"]', 1, 2),
    (3, 'Quốc tế', 'Khu vực giao hàng quốc tế', '["US", "CA", "GB", "FR", "DE", "AU", "JP", "KR", "CN"]', 1, 3);

-- ================================================================
-- 7. NOTIFICATION TEMPLATES
-- ================================================================
INSERT INTO [NotificationTemplates] ([Id], [Name], [Type], [Subject], [BodyTemplate], [Language], [IsActive])
VALUES
    (1, 'Welcome Email', 'Email', 'Chào mừng đến với Sakura Home!', 'Xin chào {UserName}, cảm ơn bạn đã đăng ký tài khoản tại Sakura Home.', 'vi', 1),
    (2, 'Order Confirmation', 'Email', 'Xác nhận đơn hàng #{OrderNumber}', 'Đơn hàng {OrderNumber} của bạn đã được xác nhận. Tổng tiền: {TotalAmount}', 'vi', 1),
    (3, 'Order Shipped', 'Email', 'Đơn hàng #{OrderNumber} đã được giao cho vận chuyển', 'Đơn hàng {OrderNumber} đã được giao cho đơn vị vận chuyển. Mã tracking: {TrackingNumber}', 'vi', 1);

-- ================================================================
-- 8. PAYMENT METHODS
-- ================================================================
INSERT INTO [PaymentMethods] ([Id], [Name], [Description], [Code], [LogoUrl], [IsActive], [DisplayOrder], [FeePercentage], [FixedFee], [MinAmount], [MaxAmount])
VALUES
    (1, 'Thanh toán khi nhận hàng (COD)', 'Thanh toán bằng tiền mặt khi nhận hàng', 'COD', '/images/payment/cod.png', 1, 1, 0.0000, 0.00, 0.00, 5000000.00),
    (2, 'Chuyển khoản ngân hàng', 'Chuyển khoản qua ngân hàng', 'BANK_TRANSFER', '/images/payment/bank-transfer.png', 1, 2, 0.0000, 0.00, 0.00, 0.00),
    (3, 'Ví MoMo', 'Thanh toán qua ví điện tử MoMo', 'MOMO', '/images/payment/momo.png', 1, 3, 2.5000, 0.00, 10000.00, 50000000.00),
    (4, 'ZaloPay', 'Thanh toán qua ví điện tử ZaloPay', 'ZALOPAY', '/images/payment/zalopay.png', 1, 4, 2.0000, 0.00, 10000.00, 50000000.00);

-- ================================================================
-- 9. PRODUCTS (Sample Data)
-- ================================================================
INSERT INTO [Products] ([Id], [Name], [SKU], [Slug], [ShortDescription], [Description], [MainImage], [Price], [OriginalPrice], [CostPrice], [Stock], [MinStock], [MaxStock], [TrackInventory], [AllowBackorder], [AllowPreorder], [BrandId], [CategoryId], [Origin], [JapaneseRegion], [AuthenticityLevel], [AuthenticityInfo], [UsageGuide], [Ingredients], [ExpiryDate], [ManufactureDate], [BatchNumber], [AgeRestriction], [Weight], [WeightUnit], [Dimensions], [Length], [Width], [Height], [DimensionUnit], [Status], [Condition], [Visibility], [IsFeatured], [IsNew], [IsBestseller], [IsLimitedEdition], [IsDiscontinued], [AvailableFrom], [Rating], [ReviewCount], [ViewCount], [SoldCount], [WishlistCount], [MetaTitle], [MetaDescription], [MetaKeywords], [Tags], [MarketingDescription], [IsGiftWrappingAvailable], [GiftWrappingFee], [DisplayOrder], [IsActive], [IsDeleted], [CreatedAt], [UpdatedAt])
VALUES
    (1, 'Pocky Chocolate Sticks', 'POCKY-CHOC-001', 'pocky-chocolate-sticks', 'Classic Japanese chocolate biscuit sticks', 'Delicious Japanese Pocky chocolate biscuit sticks. Perfect snack for any time of the day. Made with high-quality ingredients and authentic Japanese recipe.', '/images/products/pocky-chocolate.jpg', 45000.00, 50000.00, 30000.00, 500, 50, 1000, 1, 0, 0, 3, 1, 'Osaka', 2, 3, 'Authentic Glico Pocky imported directly from Japan', 'Ready to eat. Store in cool, dry place.', 'Wheat flour, sugar, chocolate, vegetable oil, whole milk powder', '2024-07-01T00:00:00.0000000', '2023-12-01T00:00:00.0000000', 'PCK20240101', 0, 47.00, 1, '15.5x2.0x1.2', 15.5, 2.0, 1.2, 1, 1, 1, 1, 1, 1, 0, 0, 0, '2023-12-02T00:00:00.0000000', 4.5, 25, 1250, 150, 45, 'Pocky Chocolate Sticks - Authentic Japanese Snack', 'Buy authentic Pocky chocolate biscuit sticks online. Premium Japanese snack perfect for sharing.', 'pocky, chocolate, japanese snack, biscuit sticks, glico', 'New Arrival, Best Seller, Japanese Snack', 'The most beloved Japanese snack worldwide!', 1, 10000.00, 1, 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    
    (2, 'Matcha Green Tea Powder - Ceremonial Grade', 'MATCHA-PREM-001', 'matcha-green-tea-powder-ceremonial', 'Premium ceremonial grade matcha powder from Uji, Kyoto', 'Authentic Japanese ceremonial grade matcha green tea powder from the renowned tea fields of Uji, Kyoto. Perfect for traditional tea ceremony, matcha lattes, and baking. Rich in antioxidants and L-theanine.', '/images/products/matcha-powder.jpg', 350000.00, 400000.00, 250000.00, 100, 10, 200, 1, 0, 1, 4, 1, 'Uji, Kyoto', 3, 5, 'Certified ceremonial grade matcha from century-old tea gardens in Uji', 'Sift powder, add 70°C water, whisk in M-shape motion until frothy. 1-2g per serving.', '100% pure stone-ground green tea leaves (Tencha)', '2026-01-01T00:00:00.0000000', '2023-11-01T00:00:00.0000000', 'MTH20231201', 0, 100.00, 1, '8.0x8.0x8.0', 8.0, 8.0, 8.0, 1, 1, 1, 1, 1, 0, 1, 0, 0, '2023-11-02T00:00:00.0000000', 4.8, 42, 2100, 89, 78, 'Premium Ceremonial Matcha Powder - Uji Kyoto', 'Authentic ceremonial grade matcha from Uji, Kyoto. Perfect for tea ceremony and culinary use.', 'matcha, green tea, ceremonial grade, uji, kyoto, japanese tea', 'Premium, Best Seller, Traditional, Organic', 'Experience the authentic taste of Japanese tea ceremony', 1, 15000.00, 2, 1, 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000');

-- ================================================================
-- END OF SEED DATA SCRIPT
-- ================================================================

PRINT 'Sakura Home seed data has been successfully imported!'
PRINT 'Total records inserted:'
PRINT '- System Settings: 8'
PRINT '- Categories: 5' 
PRINT '- Brands: 7'
PRINT '- Product Attributes: 3'
PRINT '- Tags: 4'
PRINT '- Shipping Zones: 3'
PRINT '- Notification Templates: 3'
PRINT '- Payment Methods: 4'
PRINT '- Products: 2 (sample products)'
PRINT ''
PRINT 'Database is ready for use!'