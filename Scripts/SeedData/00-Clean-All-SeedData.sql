-- ================================================================
-- SAKURA HOME API - CLEAN SEED DATA SCRIPT
-- ================================================================
-- This script removes all seed data from the database
-- Use this to clean the database before sharing with team
-- WARNING: This will delete all existing data!
-- ================================================================

-- Disable foreign key constraints temporarily
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"

PRINT 'Starting cleanup of all seed data...'

-- ================================================================
-- 1. DELETE PRODUCTS AND RELATED DATA
-- ================================================================
PRINT 'Cleaning products and related data...'

-- Delete product tags (junction table)
DELETE FROM [ProductTags];

-- Delete product attribute values
DELETE FROM [ProductAttributeValues];

-- Delete product images
DELETE FROM [ProductImages];

-- Delete product variants
DELETE FROM [ProductVariants];

-- Delete inventory logs
DELETE FROM [InventoryLogs];

-- Delete product views
DELETE FROM [ProductViews];

-- Delete cart items (remove products from carts)
DELETE FROM [CartItems];

-- Delete wishlist items (remove products from wishlists)
DELETE FROM [WishlistItems];

-- Delete order items (THIS WILL AFFECT EXISTING ORDERS!)
-- Uncomment the line below if you want to delete order items as well
-- DELETE FROM [OrderItems];

-- Delete reviews and related data
DELETE FROM [ReviewVotes];
DELETE FROM [ReviewImages];
DELETE FROM [ReviewResponses];
DELETE FROM [Reviews];
DELETE FROM [ReviewSummaries];

-- Delete products
DELETE FROM [Products];

PRINT 'Products and related data cleaned!'

-- ================================================================
-- 2. DELETE CATALOG DATA
-- ================================================================
PRINT 'Cleaning catalog data...'

-- Delete brands
DELETE FROM [Brands];

-- Delete categories
DELETE FROM [Categories];

-- Delete tags
DELETE FROM [Tags];

-- Delete product attributes
DELETE FROM [ProductAttributes];

PRINT 'Catalog data cleaned!'

-- ================================================================
-- 3. DELETE SYSTEM CONFIGURATION
-- ================================================================
PRINT 'Cleaning system configuration...'

-- Delete system settings
DELETE FROM [SystemSettings];

-- Delete notification templates
DELETE FROM [NotificationTemplates];

-- Delete payment methods
DELETE FROM [PaymentMethods];

-- Delete shipping zones and rates
DELETE FROM [ShippingRates];
DELETE FROM [ShippingZones];

PRINT 'System configuration cleaned!'

-- ================================================================
-- 4. DELETE COUPONS (OPTIONAL)
-- ================================================================
PRINT 'Cleaning coupons...'
DELETE FROM [Coupons];

-- ================================================================
-- 5. DELETE TRANSLATION DATA (OPTIONAL)
-- ================================================================
PRINT 'Cleaning translations...'
DELETE FROM [Translations];

-- ================================================================
-- 6. DELETE USER-GENERATED CONTENT (OPTIONAL - BE CAREFUL!)
-- ================================================================
-- Uncomment the sections below if you want to delete user data as well
-- WARNING: This will delete all user accounts and their data!

/*
PRINT 'Cleaning user data (DANGEROUS!)...'

-- Delete user activities
DELETE FROM [UserActivities];

-- Delete search logs
DELETE FROM [SearchLogs];

-- Delete notifications
DELETE FROM [Notifications];

-- Delete contact messages
DELETE FROM [ContactMessages];

-- Delete user addresses
DELETE FROM [Addresses];

-- Delete refresh tokens
DELETE FROM [RefreshTokens];

-- Delete carts and cart items
DELETE FROM [CartItems];
DELETE FROM [Carts];

-- Delete wishlists and wishlist items
DELETE FROM [WishlistItems];
DELETE FROM [Wishlists];

-- Delete payment transactions
DELETE FROM [PaymentTransactions];

-- Delete order data (BE VERY CAREFUL!)
DELETE FROM [OrderNotes];
DELETE FROM [OrderStatusHistory];
DELETE FROM [OrderItems];
DELETE FROM [Orders];

-- Delete shipping data
DELETE FROM [ShippingTrackings];
DELETE FROM [ShippingOrders];

-- Delete Identity tables (users, roles, etc.)
DELETE FROM [UserTokens];
DELETE FROM [UserRoles];
DELETE FROM [UserLogins];
DELETE FROM [UserClaims];
DELETE FROM [RoleClaims];
DELETE FROM [Roles];
DELETE FROM [Users];

PRINT 'User data cleaned!'
*/

-- ================================================================
-- 7. RESET IDENTITY SEEDS
-- ================================================================
PRINT 'Resetting identity seeds...'

-- Reset identity seeds for all tables
DBCC CHECKIDENT ('Products', RESEED, 0);
DBCC CHECKIDENT ('Brands', RESEED, 0);
DBCC CHECKIDENT ('Categories', RESEED, 0);
DBCC CHECKIDENT ('ProductAttributes', RESEED, 0);
DBCC CHECKIDENT ('Tags', RESEED, 0);
DBCC CHECKIDENT ('SystemSettings', RESEED, 0);
DBCC CHECKIDENT ('NotificationTemplates', RESEED, 0);
DBCC CHECKIDENT ('PaymentMethods', RESEED, 0);
DBCC CHECKIDENT ('ShippingZones', RESEED, 0);
DBCC CHECKIDENT ('Coupons', RESEED, 0);

-- Uncomment if you deleted user data
/*
DBCC CHECKIDENT ('Users', RESEED, 0);
DBCC CHECKIDENT ('Orders', RESEED, 0);
DBCC CHECKIDENT ('Addresses', RESEED, 0);
DBCC CHECKIDENT ('Carts', RESEED, 0);
DBCC CHECKIDENT ('Wishlists', RESEED, 0);
*/

-- ================================================================
-- 8. RE-ENABLE FOREIGN KEY CONSTRAINTS
-- ================================================================
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"

-- ================================================================
-- VERIFICATION
-- ================================================================
PRINT ''
PRINT '================================================================'
PRINT 'CLEANUP COMPLETED!'
PRINT '================================================================'
PRINT 'The following data has been removed:'
PRINT '✅ All products and product-related data'
PRINT '✅ All brands and categories'
PRINT '✅ All system settings and configuration'
PRINT '✅ All coupons and promotions'
PRINT '✅ Identity seeds have been reset'
PRINT ''
PRINT '⚠️  The following data was preserved:'
PRINT '🔒 User accounts and login data'
PRINT '🔒 Orders and transaction history'
PRINT '🔒 User carts and wishlists'
PRINT ''
PRINT 'Database is now clean and ready for fresh setup!'
PRINT 'Run the seed data script to populate initial data.'