-- ================================================================
-- SAKURA HOME API - MINIMAL SEED DATA SCRIPT
-- ================================================================
-- This script contains only essential system configuration
-- Use this for production or minimal setup
-- ================================================================

-- ================================================================
-- 1. ESSENTIAL SYSTEM SETTINGS
-- ================================================================
INSERT INTO [SystemSettings] ([Id], [Key], [Value], [Description], [Type], [Category], [IsPublic], [CreatedAt], [UpdatedAt])
VALUES
    (1, 'SiteName', 'Sakura Home', 'Tên website', 0, 'General', 1, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (2, 'SiteDescription', 'Japanese Products E-commerce Platform', 'Mô tả website', 0, 'General', 1, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (3, 'DefaultCurrency', 'VND', 'Đơn vị tiền tệ mặc định', 0, 'General', 1, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (4, 'DefaultLanguage', 'vi', 'Ngôn ngữ mặc định', 0, 'General', 1, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (7, 'EnableRegistration', 'true', 'Cho phép đăng ký tài khoản mới', 1, 'Security', 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000'),
    (8, 'MaxLoginAttempts', '5', 'Số lần đăng nhập sai tối đa', 2, 'Security', 0, '2024-01-01T00:00:00.0000000', '2024-01-01T00:00:00.0000000');

-- ================================================================
-- 2. ESSENTIAL PAYMENT METHODS
-- ================================================================
INSERT INTO [PaymentMethods] ([Id], [Name], [Description], [Code], [LogoUrl], [IsActive], [DisplayOrder], [FeePercentage], [FixedFee], [MinAmount], [MaxAmount])
VALUES
    (1, 'Thanh toán khi nhận hàng (COD)', 'Thanh toán bằng tiền mặt khi nhận hàng', 'COD', '/images/payment/cod.png', 1, 1, 0.0000, 0.00, 0.00, 5000000.00),
    (2, 'Chuyển khoản ngân hàng', 'Chuyển khoản qua ngân hàng', 'BANK_TRANSFER', '/images/payment/bank-transfer.png', 1, 2, 0.0000, 0.00, 0.00, 0.00);

-- ================================================================
-- 3. BASIC SHIPPING ZONE
-- ================================================================
INSERT INTO [ShippingZones] ([Id], [Name], [Description], [Countries], [IsActive], [DisplayOrder])
VALUES
    (1, 'Việt Nam', 'Khu vực giao hàng trong nước Việt Nam', '["VN"]', 1, 1);

-- ================================================================
-- 4. BASIC NOTIFICATION TEMPLATES
-- ================================================================
INSERT INTO [NotificationTemplates] ([Id], [Name], [Type], [Subject], [BodyTemplate], [Language], [IsActive])
VALUES
    (1, 'Welcome Email', 'Email', 'Chào mừng đến với Sakura Home!', 'Xin chào {UserName}, cảm ơn bạn đã đăng ký tài khoản tại Sakura Home.', 'vi', 1),
    (2, 'Order Confirmation', 'Email', 'Xác nhận đơn hàng #{OrderNumber}', 'Đơn hàng {OrderNumber} của bạn đã được xác nhận. Tổng tiền: {TotalAmount}', 'vi', 1);

PRINT 'Minimal seed data imported successfully!'
PRINT 'System is ready for basic operation.'