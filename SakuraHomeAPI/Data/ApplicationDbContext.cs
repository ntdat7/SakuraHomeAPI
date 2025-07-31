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
using SakuraHomeAPI.Models.Enums;
using System.Reflection;

namespace SakuraHomeAPI.Data
{
    /// <summary>
    /// Application database context
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        #region DbSets

        // Identity & User Management
        public DbSet<User> Users { get; set; }
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

            // Configure indexes
            ConfigureIndexes(builder);

            // Configure relationships
            ConfigureRelationships(builder);

            // Configure constraints
            ConfigureConstraints(builder);

            // Configure value conversions
            ConfigureValueConversions(builder);

            // Seed data
            SeedMasterData(builder);
        }

        /// <summary>
        /// Configure Identity table names and relationships
        /// </summary>
        private void ConfigureIdentityTables(ModelBuilder builder)
        {
            // Rename Identity tables
            builder.Entity<User>().ToTable("Users");
            builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            // Configure User entity
            builder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.PhoneNumber);
                entity.HasIndex(u => u.CreatedAt);
                entity.HasIndex(u => u.LastLoginAt);
                entity.HasIndex(u => u.Status);
                entity.HasIndex(u => u.Role);
            });
        }

        /// <summary>
        /// Configure database indexes for performance
        /// </summary>
        private void ConfigureIndexes(ModelBuilder builder)
        {
            // Product indexes
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

            // Order indexes
            builder.Entity<Order>(entity =>
            {
                entity.HasIndex(o => o.OrderNumber).IsUnique();
                entity.HasIndex(o => o.UserId);
                entity.HasIndex(o => o.Status);
                entity.HasIndex(o => o.PaymentStatus);
                entity.HasIndex(o => o.OrderDate);
                entity.HasIndex(o => o.CreatedAt);
            });

            // Review indexes
            builder.Entity<Review>(entity =>
            {
                entity.HasIndex(r => r.ProductId);
                entity.HasIndex(r => r.UserId);
                entity.HasIndex(r => r.Rating);
                entity.HasIndex(r => r.IsApproved);
                entity.HasIndex(r => r.CreatedAt);
                entity.HasIndex(r => new { r.ProductId, r.IsApproved, r.IsActive, r.IsDeleted });
            });

            // Translation indexes
            builder.Entity<Translation>(entity =>
            {
                entity.HasIndex(t => new { t.EntityType, t.EntityId, t.FieldName, t.Language }).IsUnique();
                entity.HasIndex(t => t.Language);
            });

            // Cart indexes
            builder.Entity<CartItem>(entity =>
            {
                entity.HasIndex(ci => ci.CartId);
                entity.HasIndex(ci => new { ci.CartId, ci.ProductId, ci.ProductVariantId }).IsUnique();
            });

            // Wishlist indexes
            builder.Entity<WishlistItem>(entity =>
            {
                entity.HasIndex(wi => wi.WishlistId);
                entity.HasIndex(wi => new { wi.WishlistId, wi.ProductId }).IsUnique();
            });

            // Analytics indexes
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

            // Inventory indexes
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

            // Contact message indexes
            builder.Entity<ContactMessage>(entity =>
            {
                entity.HasIndex(cm => cm.UserId);
                entity.HasIndex(cm => cm.Status);
                entity.HasIndex(cm => cm.CreatedAt);
            });

            // System settings indexes
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
        /// Configure entity relationships
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

            // Product variants
            builder.Entity<ProductVariant>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product images
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
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithOne(u => u.Wishlist)
                .HasForeignKey<Wishlist>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart items
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Wishlist items
            builder.Entity<WishlistItem>()
                .HasOne(wi => wi.Wishlist)
                .WithMany(w => w.WishlistItems)
                .HasForeignKey(wi => wi.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WishlistItem>()
                .HasOne(wi => wi.Product)
                .WithMany(p => p.WishlistItems)
                .HasForeignKey(wi => wi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

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
                .OnDelete(DeleteBehavior.Cascade);

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
                .HasForeignKey(pt => pt.ProductId);

            builder.Entity<ProductTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.ProductTags)
                .HasForeignKey(pt => pt.TagId);

            // Review votes composite key
            builder.Entity<ReviewVote>()
                .HasKey(rv => new { rv.ReviewId, rv.UserId });
        }

        /// <summary>
        /// Configure database constraints
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
            builder.Entity<Coupon>()
                .HasCheckConstraint("CK_Coupon_Value", "Value >= 0");

            builder.Entity<Coupon>()
                .HasCheckConstraint("CK_Coupon_UsageLimit", "UsageLimit IS NULL OR UsageLimit >= 0");

            builder.Entity<Coupon>()
                .HasCheckConstraint("CK_Coupon_UsedCount", "UsedCount >= 0");

            // Shipping constraints
            builder.Entity<ShippingRate>()
                .HasCheckConstraint("CK_ShippingRate_Rate", "Rate >= 0");
        }

        /// <summary>
        /// Configure value conversions for enums, etc.
        /// </summary>
        private void ConfigureValueConversions(ModelBuilder builder)
        {
            // Configure enum to string conversions if needed
            // This helps with better readability in database

            // Example: Convert OrderStatus enum to string
            // builder.Entity<Order>()
            //     .Property(e => e.Status)
            //     .HasConversion<string>();
        }

        /// <summary>
        /// Seed master data
        /// </summary>
        private void SeedMasterData(ModelBuilder builder)
        {

        var settingsData = new[]
        {
            new SystemSetting {
                Id = 1,
                Key = "SiteName",
                Value = "Sakura Home",
                Description = "Website name",
                Type = SettingType.String,
                Category = "General",
                IsPublic = true,
                CreatedAt = new DateTime(2024, 1, 1),
                UpdatedAt = new DateTime(2024, 1, 1)
            },
            new SystemSetting {
                Id = 2,
                Key = "DefaultLanguage",
                Value = "vi",
                Description = "Default language",
                Type = SettingType.String,
                Category = "General",
                IsPublic = true,
                CreatedAt = new DateTime(2024, 1, 1),
                UpdatedAt = new DateTime(2024, 1, 1)
            },
            new SystemSetting {
                Id = 3,
                Key = "DefaultCurrency",
                Value = "VND",
                Description = "Default currency",
                Type = SettingType.String,
                Category = "General",
                IsPublic = true,
                CreatedAt = new DateTime(2024, 1, 1),
                UpdatedAt = new DateTime(2024, 1, 1)
            },
            new SystemSetting {
                Id = 4,
                Key = "ItemsPerPage",
                Value = "20",
                Description = "Items per page",
                Type = SettingType.Number,
                Category = "General",
                IsPublic = true,
                CreatedAt = new DateTime(2024, 1, 1),
                UpdatedAt = new DateTime(2024, 1, 1)
            },
            new SystemSetting {
                Id = 5,
                Key = "AllowGuestCheckout",
                Value = "false",
                Description = "Allow guest checkout",
                Type = SettingType.Boolean,
                Category = "Orders",
                IsPublic = true,
                CreatedAt = new DateTime(2024, 1, 1),
                UpdatedAt = new DateTime(2024, 1, 1)
            },
            new SystemSetting {
                Id = 6,
                Key = "AutoApproveReviews",
                Value = "false",
                Description = "Auto approve reviews",
                Type = SettingType.Boolean,
                Category = "Reviews",
                IsPublic = false,
                CreatedAt = new DateTime(2024, 1, 1),
                UpdatedAt = new DateTime(2024, 1, 1)
            }
        };


            builder.Entity<SystemSetting>().HasData(settingsData);

            // Seed default categories
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Food & Beverages", Slug = "food-beverages", Description = "Japanese food and drinks", DisplayOrder = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1), UpdatedAt = new DateTime(2024, 1, 1) },
                new Category { Id = 2, Name = "Beauty & Health", Slug = "beauty-health", Description = "Japanese cosmetics and health products", DisplayOrder = 2, IsActive = true, CreatedAt = new DateTime(2024, 1, 1), UpdatedAt = new DateTime(2024, 1, 1) },
                new Category { Id = 3, Name = "Fashion", Slug = "fashion", Description = "Japanese fashion and accessories", DisplayOrder = 3, IsActive = true, CreatedAt = new DateTime(2024, 1, 1), UpdatedAt = new DateTime(2024, 1, 1) },
                new Category { Id = 4, Name = "Electronics", Slug = "electronics", Description = "Japanese electronics and gadgets", DisplayOrder = 4, IsActive = true, CreatedAt = new DateTime(2024, 1, 1), UpdatedAt = new DateTime(2024, 1, 1) },
                new Category { Id = 5, Name = "Home & Living", Slug = "home-living", Description = "Japanese home decor and living items", DisplayOrder = 5, IsActive = true, CreatedAt = new DateTime(2024, 1, 1), UpdatedAt = new DateTime(2024, 1, 1) }
            );
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
        /// Update audit fields for entities
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
                }

                entity.UpdatedAt = now;

                // Set CreatedBy and UpdatedBy from current user context if available
                // This would typically come from HttpContext or a service
                // For now, we'll leave them as they are set by the application
            }

            // Handle soft delete
            var softDeleteEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is ISoftDelete && e.State == EntityState.Deleted);

            foreach (var entry in softDeleteEntries)
            {
                entry.State = EntityState.Modified;
                var entity = (ISoftDelete)entry.Entity;
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
            }
        }
    }
}