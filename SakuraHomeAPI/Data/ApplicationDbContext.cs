using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Models.Entities.Catalog;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Models.Entities.Orders;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Models.Entities.Reviews;
using SakuraHomeAPI.Models.Entities.UserCart;
using SakuraHomeAPI.Models.Entities.UserWishlist;
using SakuraHomeAPI.Data;
using System.Reflection;

namespace SakuraHomeAPI.Data
{
    /// <summary>
    /// Application database context with proper Guid support
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid,
        IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        #region DbSets

        // Identity & User Management - override to avoid warning
        public new DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }

        // Product Catalog
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<ProductAttributeValue> ProductAttributeValues { get; set; }
        public DbSet<CategoryAttribute> CategoryAttributes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }

        // Shopping
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }

        // Orders
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistory { get; set; }
        public DbSet<OrderNote> OrderNotes { get; set; }

        // Reviews
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewImage> ReviewImages { get; set; }
        public DbSet<ReviewVote> ReviewVotes { get; set; }
        public DbSet<ReviewResponse> ReviewResponses { get; set; }
        public DbSet<ReviewSummary> ReviewSummaries { get; set; }

        // Communications & Notifications
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<EmailQueue> EmailQueue { get; set; }

        // Analytics & Tracking
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<ProductView> ProductViews { get; set; }
        public DbSet<SearchLog> SearchLogs { get; set; }
        public DbSet<InventoryLog> InventoryLogs { get; set; }

        // System & Configuration
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<Translation> Translations { get; set; }

        // Payment & Financial
        public DbSet<PaymentMethodInfo> PaymentMethods { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Coupon> Coupons { get; set; }

        // Shipping & Delivery
        public DbSet<ShippingZone> ShippingZones { get; set; }
        public DbSet<ShippingRate> ShippingRates { get; set; }

        // Marketing
        public DbSet<Banner> Banners { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply all configurations from the assembly
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Configure Identity tables
            ConfigureIdentityTables(builder);

            // Configure indexes for performance
            ConfigureIndexes(builder);

            // Configure entity relationships
            ConfigureRelationships(builder);

            // Configure database constraints
            ConfigureConstraints(builder);

            // Configure value conversions
            ConfigureValueConversions(builder);

            // Fix audit navigation properties for entities with mixed audit types
            // These entities inherit from AuditableEntity (int-based) but have Guid foreign keys
            ConfigureAuditNavigationProperties(builder);

            // Seed master data using external seeder
            DatabaseSeeder.SeedData(builder);
        }

        /// <summary>
        /// Configure Identity table names and relationships
        /// </summary>
        private void ConfigureIdentityTables(ModelBuilder builder)
        {
            // Rename Identity tables for better naming
            builder.Entity<User>().ToTable("Users");
            builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            // Configure User entity with indexes and relationships
            builder.Entity<User>(entity =>
            {
                // Indexes for performance
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.PhoneNumber);
                entity.HasIndex(u => u.CreatedAt);
                entity.HasIndex(u => u.LastLoginAt);
                entity.HasIndex(u => u.Status);
                entity.HasIndex(u => u.Role);

                // Configure self-referencing relationships for audit fields
                entity.HasOne(u => u.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(u => u.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.UpdatedByUser)
                    .WithMany()
                    .HasForeignKey(u => u.UpdatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.DeletedByUser)
                    .WithMany()
                    .HasForeignKey(u => u.DeletedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        /// <summary>
        /// Configure database indexes for optimal performance
        /// </summary>
        private void ConfigureIndexes(ModelBuilder builder)
        {
            // Product indexes for search and filtering
            builder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.SKU).IsUnique();
                entity.HasIndex(p => p.Name);
                entity.HasIndex(p => p.Price);
                entity.HasIndex(p => p.Status);
                entity.HasIndex(p => p.IsActive);
                entity.HasIndex(p => p.IsDeleted);
                entity.HasIndex(p => p.CreatedAt);
                entity.HasIndex(p => p.Rating);
                entity.HasIndex(p => p.SoldCount);
                entity.HasIndex(p => p.IsFeatured);
                entity.HasIndex(p => p.IsNew);
                entity.HasIndex(p => p.IsBestseller);
                // Composite indexes for common queries
                entity.HasIndex(p => new { p.CategoryId, p.IsActive, p.IsDeleted });
                entity.HasIndex(p => new { p.BrandId, p.IsActive, p.IsDeleted });
            });

            // Category indexes
            builder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Slug).IsUnique();
                entity.HasIndex(c => c.ParentId);
                entity.HasIndex(c => c.DisplayOrder);
                entity.HasIndex(c => new { c.IsActive, c.IsDeleted });
            });

            // Brand indexes
            builder.Entity<Brand>(entity =>
            {
                entity.HasIndex(b => b.Slug).IsUnique();
                entity.HasIndex(b => b.Name);
                entity.HasIndex(b => new { b.IsActive, b.IsDeleted });
            });

            // Order indexes for admin and user queries
            builder.Entity<Order>(entity =>
            {
                entity.HasIndex(o => o.OrderNumber).IsUnique();
                entity.HasIndex(o => o.UserId);
                entity.HasIndex(o => o.Status);
                entity.HasIndex(o => o.PaymentStatus);
                entity.HasIndex(o => o.OrderDate);
                entity.HasIndex(o => o.CreatedAt);
            });

            builder.Entity<OrderItem>(entity =>
            {
                entity.HasIndex(oi => oi.OrderId);
                entity.HasIndex(oi => oi.ProductId);
                entity.HasIndex(oi => oi.ProductVariantId);
            });

            // Review indexes for product pages
            builder.Entity<Review>(entity =>
            {
                entity.HasIndex(r => r.ProductId);
                entity.HasIndex(r => r.UserId);
                entity.HasIndex(r => r.Rating);
                entity.HasIndex(r => r.IsApproved);
                entity.HasIndex(r => r.CreatedAt);
                entity.HasIndex(r => new { r.ProductId, r.IsApproved, r.IsActive, r.IsDeleted });
            });

            // Translation indexes for multi-language support
            builder.Entity<Translation>(entity =>
            {
                entity.HasIndex(t => new { t.EntityType, t.EntityId, t.FieldName, t.Language }).IsUnique();
                entity.HasIndex(t => t.Language);
            });

            // Cart and Wishlist indexes
            builder.Entity<CartItem>(entity =>
            {
                entity.HasIndex(ci => ci.CartId);
                entity.HasIndex(ci => new { ci.CartId, ci.ProductId, ci.ProductVariantId }).IsUnique();
            });

            builder.Entity<WishlistItem>(entity =>
            {
                entity.HasIndex(wi => wi.WishlistId);
                entity.HasIndex(wi => new { wi.WishlistId, wi.ProductId }).IsUnique();
            });

            // Analytics indexes for reporting
            builder.Entity<ProductView>(entity =>
            {
                entity.HasIndex(pv => pv.ProductId);
                entity.HasIndex(pv => pv.UserId);
                entity.HasIndex(pv => pv.LastViewedAt);
                entity.HasIndex(pv => pv.IpAddress);
            });

            builder.Entity<SearchLog>(entity =>
            {
                entity.HasIndex(sl => sl.SearchTerm);
                entity.HasIndex(sl => sl.CreatedAt);
                entity.HasIndex(sl => sl.UserId);
                entity.HasIndex(sl => sl.HasResults);
            });

            builder.Entity<UserActivity>(entity =>
            {
                entity.HasIndex(ua => ua.UserId);
                entity.HasIndex(ua => ua.ActivityType);
                entity.HasIndex(ua => ua.CreatedAt);
                entity.HasIndex(ua => new { ua.RelatedEntityType, ua.RelatedEntityId });
            });

            // Inventory tracking indexes
            builder.Entity<InventoryLog>(entity =>
            {
                entity.HasIndex(il => il.ProductId);
                entity.HasIndex(il => il.Action);
                entity.HasIndex(il => il.CreatedAt);
                entity.HasIndex(il => new { il.ReferenceType, il.ReferenceId });
            });

            // Notification indexes
            builder.Entity<Notification>(entity =>
            {
                entity.HasIndex(n => n.UserId);
                entity.HasIndex(n => n.Type);
                entity.HasIndex(n => n.IsRead);
                entity.HasIndex(n => n.CreatedAt);
            });

            // System indexes
            builder.Entity<ContactMessage>(entity =>
            {
                entity.HasIndex(cm => cm.UserId);
                entity.HasIndex(cm => cm.Status);
                entity.HasIndex(cm => cm.CreatedAt);
            });

            builder.Entity<SystemSetting>(entity =>
            {
                entity.HasIndex(ss => ss.Key).IsUnique();
                entity.HasIndex(ss => ss.Category);
                entity.HasIndex(ss => ss.IsPublic);
            });

            // Payment indexes
            builder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasIndex(pt => pt.TransactionId).IsUnique();
                entity.HasIndex(pt => pt.OrderId);
                entity.HasIndex(pt => pt.Status);
                entity.HasIndex(pt => pt.CreatedAt);
            });

            // Coupon indexes
            builder.Entity<Coupon>(entity =>
            {
                entity.HasIndex(c => c.Code).IsUnique();
                entity.HasIndex(c => c.IsActive);
                entity.HasIndex(c => new { c.StartDate, c.EndDate });
            });
        }

        /// <summary>
        /// Configure entity relationships and foreign keys
        /// </summary>
        private void ConfigureRelationships(ModelBuilder builder)
        {
            // Product relationships
            builder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category hierarchy
            builder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product variants and images
            builder.Entity<ProductVariant>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product attributes
            builder.Entity<ProductAttributeValue>()
                .HasOne(pav => pav.Product)
                .WithMany(p => p.AttributeValues)
                .HasForeignKey(pav => pav.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductAttributeValue>()
                .HasOne(pav => pav.Attribute)
                .WithMany(pa => pa.Values)
                .HasForeignKey(pav => pav.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);

            // User relationships
            builder.Entity<Address>()
                .HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Cart>()
                .HasIndex(c => c.UserId);

            builder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithOne(u => u.Wishlist)
                .HasForeignKey<Wishlist>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Shopping cart relationships
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.ProductVariant)
                .WithMany()
                .HasForeignKey(ci => ci.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);

            // Wishlist relationships
            builder.Entity<WishlistItem>()
                .HasOne(wi => wi.Wishlist)
                .WithMany(w => w.WishlistItems)
                .HasForeignKey(wi => wi.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WishlistItem>()
                .HasOne(wi => wi.Product)
                .WithMany(p => p.WishlistItems)
                .HasForeignKey(wi => wi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order relationships
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review relationships
            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Analytics relationships
            builder.Entity<ProductView>()
                .HasOne(pv => pv.User)
                .WithMany(u => u.ProductViews)
                .HasForeignKey(pv => pv.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ProductView>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.ProductViews)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SearchLog>()
                .HasOne(sl => sl.User)
                .WithMany(u => u.SearchLogs)
                .HasForeignKey(sl => sl.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<UserActivity>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.Activities)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Inventory relationships
            builder.Entity<InventoryLog>()
                .HasOne(il => il.Product)
                .WithMany(p => p.InventoryLogs)
                .HasForeignKey(il => il.ProductId)
                .OnDelete(DeleteBehavior.NoAction); // Changed from Cascade to NoAction to avoid multiple cascade paths

            builder.Entity<InventoryLog>()
                .HasOne(il => il.ProductVariant)
                .WithMany()
                .HasForeignKey(il => il.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<InventoryLog>()
                .HasOne(il => il.User)
                .WithMany()
                .HasForeignKey(il => il.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Notification relationships
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Contact message relationships
            builder.Entity<ContactMessage>()
                .HasOne(cm => cm.User)
                .WithMany(u => u.ContactMessages)
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Payment relationships
            builder.Entity<PaymentTransaction>()
                .HasOne(pt => pt.Order)
                .WithMany()
                .HasForeignKey(pt => pt.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Shipping relationships
            builder.Entity<ShippingRate>()
                .HasOne(sr => sr.ShippingZone)
                .WithMany(sz => sz.ShippingRates)
                .HasForeignKey(sr => sr.ShippingZoneId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-many relationships
            builder.Entity<ProductTag>()
                .HasKey(pt => new { pt.ProductId, pt.TagId });

            builder.Entity<ProductTag>()
                .HasOne(pt => pt.Product)
                .WithMany(p => p.ProductTags)
                .HasForeignKey(pt => pt.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.ProductTags)
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review votes composite key
            builder.Entity<ReviewVote>()
                .HasKey(rv => new { rv.ReviewId, rv.UserId });

            builder.Entity<ReviewVote>()
                .HasOne(rv => rv.Review)
                .WithMany(r => r.ReviewVotes)
                .HasForeignKey(rv => rv.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ReviewVote>()
                .HasOne(rv => rv.User)
                .WithMany()
                .HasForeignKey(rv => rv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure additional entities that have cascade issues
            builder.Entity<ReviewImage>()
                .HasOne(ri => ri.Review)
                .WithMany(r => r.ReviewImages)
                .HasForeignKey(ri => ri.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderStatusHistory>()
                .HasOne(osh => osh.Order)
                .WithMany()
                .HasForeignKey(osh => osh.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Additional missing relationships
            builder.Entity<ReviewResponse>()
                .HasOne(rr => rr.Review)
                .WithMany(r => r.ReviewResponses)
                .HasForeignKey(rr => rr.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ReviewSummary>()
                .HasOne(rs => rs.Product)
                .WithMany()
                .HasForeignKey(rs => rs.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductVariant)
                .WithMany()
                .HasForeignKey(oi => oi.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);
        }

        /// <summary>
        /// Configure database constraints for data integrity
        /// </summary>
        private void ConfigureConstraints(ModelBuilder builder)
        {
            // Product constraints
            builder.Entity<Product>()
                .HasCheckConstraint("CK_Product_Price", "Price >= 0");

            builder.Entity<Product>()
                .HasCheckConstraint("CK_Product_Stock", "Stock >= 0");

            builder.Entity<Product>()
                .HasCheckConstraint("CK_Product_Rating", "Rating >= 0 AND Rating <= 5");

            builder.Entity<Product>()
                .HasCheckConstraint("CK_Product_Weight", "Weight IS NULL OR Weight >= 0");

            // Order constraints
            builder.Entity<Order>()
                .HasCheckConstraint("CK_Order_TotalAmount", "TotalAmount >= 0");

            builder.Entity<OrderItem>()
                .HasCheckConstraint("CK_OrderItem_Quantity", "Quantity > 0");

            builder.Entity<OrderItem>()
                .HasCheckConstraint("CK_OrderItem_UnitPrice", "UnitPrice >= 0");

            builder.Entity<OrderItem>()
                .HasCheckConstraint("CK_OrderItem_TotalPrice", "TotalPrice >= 0");

            // Review constraints
            builder.Entity<Review>()
                .HasCheckConstraint("CK_Review_Rating", "Rating >= 1 AND Rating <= 5");

            // Cart constraints
            builder.Entity<CartItem>()
                .HasCheckConstraint("CK_CartItem_Quantity", "Quantity > 0");

            // User constraints
            builder.Entity<User>()
                .HasCheckConstraint("CK_User_Points", "Points >= 0");

            builder.Entity<User>()
                .HasCheckConstraint("CK_User_TotalSpent", "TotalSpent >= 0");

            builder.Entity<User>()
                .HasCheckConstraint("CK_User_TotalOrders", "TotalOrders >= 0");

            builder.Entity<User>()
                .HasCheckConstraint("CK_User_FailedLoginAttempts", "FailedLoginAttempts >= 0");

            // Payment constraints
            builder.Entity<PaymentTransaction>()
                .HasCheckConstraint("CK_PaymentTransaction_Amount", "Amount >= 0");

            builder.Entity<PaymentTransaction>()
                .HasCheckConstraint("CK_PaymentTransaction_Fee", "Fee >= 0");

            // Coupon constraints
            builder.Entity<Coupon>(entity =>
            {
                entity.HasCheckConstraint("CK_Coupon_Value", "Value >= 0");

                entity.HasCheckConstraint("CK_Coupon_UsageLimit", "UsageLimit IS NULL OR UsageLimit >= 0");

                entity.HasCheckConstraint("CK_Coupon_UsedCount", "UsedCount >= 0");
            });

            // Shipping constraints
            builder.Entity<ShippingRate>()
                .HasCheckConstraint("CK_ShippingRate_Rate", "Rate >= 0");

            // ReviewSummary constraints and precision configuration
            builder.Entity<ReviewSummary>(entity =>
            {
                entity.HasCheckConstraint("CK_ReviewSummary_AverageRating", "AverageRating >= 0 AND AverageRating <= 5");
                entity.HasCheckConstraint("CK_ReviewSummary_TotalReviews", "TotalReviews >= 0");
                entity.HasCheckConstraint("CK_ReviewSummary_StarCounts", 
                    "OneStar >= 0 AND TwoStar >= 0 AND ThreeStar >= 0 AND FourStar >= 0 AND FiveStar >= 0");
                entity.HasCheckConstraint("CK_ReviewSummary_VerifiedPurchases", "VerifiedPurchases >= 0");
                entity.HasCheckConstraint("CK_ReviewSummary_WithImages", "WithImages >= 0");
                entity.HasCheckConstraint("CK_ReviewSummary_Recommended", "Recommended >= 0");
                
                // Configure decimal precision for AverageRating
                entity.Property(rs => rs.AverageRating)
                    .HasPrecision(3, 2); // 3 digits total, 2 after decimal (allows values like 5.00, 4.99, etc.)
            });

            // ProductAttribute decimal precision configuration
            builder.Entity<ProductAttribute>(entity =>
            {
                entity.Property(pa => pa.MinValue)
                    .HasPrecision(18, 4); // Allows values like 9999999999999.9999

                entity.Property(pa => pa.MaxValue)
                    .HasPrecision(18, 4); // Allows values like 9999999999999.9999
            });
        }

        /// <summary>
        /// Configure value conversions for enums and custom types
        /// </summary>
        private void ConfigureValueConversions(ModelBuilder builder)
        {
            // Example: Convert enum to string for better database readability
            // Uncomment and modify as needed based on your enums

            // builder.Entity<Order>()
            //     .Property(e => e.Status)
            //     .HasConversion<string>();

            // builder.Entity<Product>()
            //     .Property(e => e.Status)
            //     .HasConversion<string>();
        }

        /// <summary>
        /// Configure audit navigation properties for entities with mixed audit types
        /// This method handles entities that inherit from AuditableEntity (int-based) but have Guid foreign keys to User
        /// </summary>
        private void ConfigureAuditNavigationProperties(ModelBuilder builder)
        {
            // Since AuditableEntity uses int? for audit foreign keys but User has Guid primary key,
            // we need to explicitly configure these relationships as they won't work by convention
            
            // The issue is that some entities inherit from AuditableEntity (int audit fields) 
            // but try to reference User entities (Guid primary key)
            // We need to ignore the navigation properties for these mismatched relationships
            
            // Configure Review entity - now that ApprovedBy is Guid?, we can configure the relationship properly
            builder.Entity<Review>(entity =>
            {
                entity.Ignore(r => r.CreatedByUser);
                entity.Ignore(r => r.UpdatedByUser);
                entity.Ignore(r => r.DeletedByUser);
                
                // Now configure the ApprovedByUser relationship properly since ApprovedBy is Guid?
                entity.HasOne(r => r.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(r => r.ApprovedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Brand entity - inherits from ContentEntity -> ... -> AuditableEntity
            builder.Entity<Brand>(entity =>
            {
                entity.Ignore(b => b.CreatedByUser);
                entity.Ignore(b => b.UpdatedByUser);
                entity.Ignore(b => b.DeletedByUser);
            });

            // Configure Category entity - inherits from ContentEntity -> ... -> AuditableEntity  
            builder.Entity<Category>(entity =>
            {
                entity.Ignore(c => c.CreatedByUser);
                entity.Ignore(c => c.UpdatedByUser);
                entity.Ignore(c => c.DeletedByUser);
            });

            // Configure Product entity - inherits from ContentEntity -> ... -> AuditableEntity
            builder.Entity<Product>(entity =>
            {
                entity.Ignore(p => p.CreatedByUser);
                entity.Ignore(p => p.UpdatedByUser);
                entity.Ignore(p => p.DeletedByUser);
            });

            // Configure Tag entity - inherits from FullEntity -> ... -> AuditableEntity
            builder.Entity<Tag>(entity =>
            {
                entity.Ignore(t => t.CreatedByUser);
                entity.Ignore(t => t.UpdatedByUser);
                entity.Ignore(t => t.DeletedByUser);
            });

            // Configure Notification entity - it inherits from AuditableEntity but doesn't need User audit navigation
            builder.Entity<Notification>(entity =>
            {
                // Ignore the audit navigation properties since they're incompatible
                entity.Ignore(n => n.CreatedByUser);
                entity.Ignore(n => n.UpdatedByUser);
            });

            // Configure other entities that inherit from AuditableEntity and have similar issues
            builder.Entity<SystemSetting>(entity =>
            {
                entity.Ignore(s => s.CreatedByUser);
                entity.Ignore(s => s.UpdatedByUser);
            });

            builder.Entity<Translation>(entity =>
            {
                entity.Ignore(t => t.CreatedByUser);
                entity.Ignore(t => t.UpdatedByUser);
            });

            builder.Entity<ProductImage>(entity =>
            {
                entity.Ignore(pi => pi.CreatedByUser);
                entity.Ignore(pi => pi.UpdatedByUser);
            });

            builder.Entity<CartItem>(entity =>
            {
                entity.Ignore(ci => ci.CreatedByUser);
                entity.Ignore(ci => ci.UpdatedByUser);
            });

            builder.Entity<WishlistItem>(entity =>
            {
                entity.Ignore(wi => wi.CreatedByUser);
                entity.Ignore(wi => wi.UpdatedByUser);
            });

            builder.Entity<Order>(entity =>
            {
                entity.Ignore(o => o.CreatedByUser);
                entity.Ignore(o => o.UpdatedByUser);
            });

            builder.Entity<OrderNote>(entity =>
            {
                entity.Ignore(on => on.CreatedByUser);
                entity.Ignore(on => on.UpdatedByUser);
            });

            builder.Entity<ReviewResponse>(entity =>
            {
                entity.Ignore(rr => rr.CreatedByUser);
                entity.Ignore(rr => rr.UpdatedByUser);
            });

            builder.Entity<Wishlist>(entity =>
            {
                entity.Ignore(w => w.CreatedByUser);
                entity.Ignore(w => w.UpdatedByUser);
            });

            builder.Entity<Cart>(entity =>
            {
                entity.Ignore(c => c.CreatedByUser);
                entity.Ignore(c => c.UpdatedByUser);
            });

            builder.Entity<ProductAttribute>(entity =>
            {
                entity.Ignore(pa => pa.CreatedByUser);
                entity.Ignore(pa => pa.UpdatedByUser);
                entity.Ignore(pa => pa.DeletedByUser); // Added this line
            });

            builder.Entity<ProductAttributeValue>(entity =>
            {
                entity.Ignore(pav => pav.CreatedByUser);
                entity.Ignore(pav => pav.UpdatedByUser);
            });

            builder.Entity<CategoryAttribute>(entity =>
            {
                entity.Ignore(ca => ca.CreatedByUser);
                entity.Ignore(ca => ca.UpdatedByUser);
                entity.Ignore(ca => ca.DeletedByUser);
            });

            // Configure other entities that inherit from FullEntity -> AuditableEntity
            builder.Entity<ProductVariant>(entity =>
            {
                entity.Ignore(pv => pv.CreatedByUser);
                entity.Ignore(pv => pv.UpdatedByUser);
                entity.Ignore(pv => pv.DeletedByUser);
            });

            // Configure entities that have shadow properties being created by EF
            builder.Entity<ProductTag>(entity =>
            {
                entity.Ignore(pt => pt.CreatedByUser);
                entity.Ignore(pt => pt.UpdatedByUser);
            });

            builder.Entity<OrderStatusHistory>(entity =>
            {
                entity.Ignore(osh => osh.CreatedByUser);
                entity.Ignore(osh => osh.UpdatedByUser);
            });

            builder.Entity<ReviewImage>(entity =>
            {
                entity.Ignore(ri => ri.CreatedByUser);
                entity.Ignore(ri => ri.UpdatedByUser);
            });

            // Note: For entities that properly need audit tracking with User references,
            // they should inherit from AuditableGuidEntity instead of AuditableEntity
        }

        /// <summary>
        /// Override SaveChanges to handle audit fields automatically
        /// </summary>
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to handle audit fields automatically
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Update audit fields for entities automatically
        /// </summary>
        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IAuditable && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (IAuditable)entry.Entity;
                var now = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = now;
                    // Set CreatedBy if you have current user context
                    // entity.CreatedBy = GetCurrentUserId();
                }

                entity.UpdatedAt = now;
                // Set UpdatedBy if you have current user context
                // entity.UpdatedBy = GetCurrentUserId();
            }

            // Handle soft delete automatically
            var softDeleteEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is ISoftDelete && e.State == EntityState.Deleted);

            foreach (var entry in softDeleteEntries)
            {
                entry.State = EntityState.Modified;
                var entity = (ISoftDelete)entry.Entity;
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
                // Set DeletedBy if you have current user context
                // entity.DeletedBy = GetCurrentUserId();
            }
        }

        /// <summary>
        /// Helper method to get current user ID - implement based on your authentication system
        /// </summary>
        /*
        private Guid? GetCurrentUserId()
        {
            // This would typically be implemented using IHttpContextAccessor
            // to get the current user's ID from the JWT token or claims
            // Example implementation:
            // return _httpContextAccessor.HttpContext?.User?
            //     .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return null;
        }
        */
    }
}