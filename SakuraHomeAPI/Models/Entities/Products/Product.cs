using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Catalog;
using SakuraHomeAPI.Models.Entities.Orders;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Models.Entities.Reviews;
using SakuraHomeAPI.Models.Entities.UserCart;
using SakuraHomeAPI.Models.Entities.UserWishlist;
using SakuraHomeAPI.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SakuraHomeAPI.Models.Entities
{
    /// <summary>
    /// Product entity for Japanese products
    /// </summary>
    [Table("Products")]
    public class Product : ContentEntity, IFeaturable
    {
        #region Basic Information

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string SKU { get; set; }

        [MaxLength(1000)]
        public string ShortDescription { get; set; }

        public string Description { get; set; }

        [MaxLength(500)]
        public string MainImage { get; set; }

        #endregion

        #region Pricing & Inventory

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CostPrice { get; set; } // For profit calculation

        [Required]
        public int Stock { get; set; }

        public int? MinStock { get; set; } = 0; // Low stock warning
        public int? MaxStock { get; set; } = 1000; // Maximum inventory

        public bool TrackInventory { get; set; } = true;
        public bool AllowBackorder { get; set; } = false;
        public bool AllowPreorder { get; set; } = false;

        #endregion

        #region Product Categorization

        public int BrandId { get; set; }
        public int CategoryId { get; set; }

        #endregion

        #region Japanese Product Specific

        [MaxLength(100)]
        public string Origin { get; set; } // Prefecture in Japan

        public JapaneseOrigin? JapaneseRegion { get; set; }

        public AuthenticityLevel AuthenticityLevel { get; set; } = AuthenticityLevel.Verified;

        [MaxLength(1000)]
        public string AuthenticityInfo { get; set; }

        [MaxLength(2000)]
        public string UsageGuide { get; set; } // How to use (especially for Japanese products)

        [MaxLength(1000)]
        public string Ingredients { get; set; } // For food/cosmetics

        public DateTime? ExpiryDate { get; set; }
        public DateTime? ManufactureDate { get; set; }

        [MaxLength(50)]
        public string BatchNumber { get; set; }

        public AgeRestriction AgeRestriction { get; set; } = AgeRestriction.None;

        #endregion

        #region Physical Properties

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Weight { get; set; } // in grams

        public WeightUnit WeightUnit { get; set; } = WeightUnit.Gram;

        [MaxLength(50)]
        public string Dimensions { get; set; } // "10x5x3 cm"

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Length { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Width { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Height { get; set; }

        public DimensionUnit DimensionUnit { get; set; } = DimensionUnit.Centimeter;

        #endregion

        #region Product Status & Visibility

        public ProductStatus Status { get; set; } = ProductStatus.Active;
        public ProductCondition Condition { get; set; } = ProductCondition.New;
        public ProductVisibility Visibility { get; set; } = ProductVisibility.Public;

        public bool IsFeatured { get; set; } = false;
        public bool IsNew { get; set; } = false;
        public bool IsBestseller { get; set; } = false;
        public bool IsLimitedEdition { get; set; } = false;
        public bool IsDiscontinued { get; set; } = false;

        // Dates
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableUntil { get; set; }

        #endregion

        #region Statistics & Analytics

        [Column(TypeName = "decimal(3,2)")]
        public decimal Rating { get; set; } = 0;

        public int ReviewCount { get; set; } = 0;
        public int ViewCount { get; set; } = 0;
        public int SoldCount { get; set; } = 0;
        public int WishlistCount { get; set; } = 0;
        public int CartCount { get; set; } = 0; // How many times added to cart

        public DateTime? LastViewedAt { get; set; }
        public DateTime? LastSoldAt { get; set; }

        #endregion

        #region SEO & Marketing

        [MaxLength(500)]
        public string Tags { get; set; } // Comma-separated tags

        [MaxLength(1000)]
        public string MarketingDescription { get; set; }

        public bool IsGiftWrappingAvailable { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? GiftWrappingFee { get; set; }

        #endregion

        #region Navigation Properties

        public virtual Brand Brand { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
        public virtual ICollection<ProductView> ProductViews { get; set; } = new List<ProductView>();
        public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
        public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();

        #endregion

        #region Computed Properties

        [NotMapped]
        public decimal? DiscountAmount => OriginalPrice.HasValue ? OriginalPrice - Price : null;

        [NotMapped]
        public decimal? DiscountPercentage => OriginalPrice.HasValue && OriginalPrice > 0
            ? Math.Round(((OriginalPrice.Value - Price) / OriginalPrice.Value) * 100, 2)
            : null;

        [NotMapped]
        public bool IsOnSale => OriginalPrice.HasValue && OriginalPrice > Price;

        [NotMapped]
        public bool IsInStock => Stock > 0 || AllowBackorder;

        [NotMapped]
        public bool IsLowStock => MinStock.HasValue && Stock <= MinStock.Value;

        [NotMapped]
        public bool IsOutOfStock => Stock <= 0 && !AllowBackorder;

        [NotMapped]
        public bool IsAvailable
        {
            get
            {
                var now = DateTime.UtcNow;
                return Status == ProductStatus.Active &&
                       (AvailableFrom == null || AvailableFrom <= now) &&
                       (AvailableUntil == null || AvailableUntil >= now) &&
                       !IsDeleted &&
                       IsActive;
            }
        }

        [NotMapped]
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;

        [NotMapped]
        public string FormattedPrice => $"{Price:N0} ₫";

        [NotMapped]
        public string FormattedOriginalPrice => OriginalPrice?.ToString("N0") + " ₫";

        [NotMapped]
        public List<string> TagList => !string.IsNullOrEmpty(Tags)
            ? Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(t => t.Trim())
                  .ToList()
            : new List<string>();

        [NotMapped]
        public string DisplayName => !string.IsNullOrEmpty(Name) ? Name : "Unnamed Product";

        [NotMapped]
        public bool HasVariants => Variants?.Any() == true;

        [NotMapped]
        public decimal? MinVariantPrice => HasVariants ? Variants.Min(v => v.Price) : null;

        [NotMapped]
        public decimal? MaxVariantPrice => HasVariants ? Variants.Max(v => v.Price) : null;

        #endregion

        #region Methods

        /// <summary>
        /// Update product statistics
        /// </summary>
        public void UpdateStatistics()
        {
            if (Reviews?.Any() == true)
            {
                var activeReviews = Reviews.Where(r => r.IsActive && !r.IsDeleted);
                ReviewCount = activeReviews.Count();
                Rating = activeReviews.Any() ? (decimal)activeReviews.Average(r => r.Rating) : 0;
            }

            WishlistCount = WishlistItems?.Count ?? 0;
            SoldCount = OrderItems?.Sum(oi => oi.Quantity) ?? 0;
        }

        /// <summary>
        /// Check if product can be ordered
        /// </summary>
        public bool CanOrder(int quantity = 1)
        {
            return IsAvailable &&
                   (IsInStock || AllowBackorder) &&
                   (Stock >= quantity || AllowBackorder);
        }

        /// <summary>
        /// Reserve stock for order
        /// </summary>
        public bool ReserveStock(int quantity)
        {
            if (!CanOrder(quantity)) return false;

            Stock -= quantity;
            return true;
        }

        /// <summary>
        /// Release reserved stock
        /// </summary>
        public void ReleaseStock(int quantity)
        {
            Stock += quantity;
        }

        /// <summary>
        /// Add tag to product
        /// </summary>
        public void AddTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return;

            var tags = TagList;
            if (!tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            {
                tags.Add(tag.Trim());
                Tags = string.Join(", ", tags);
            }
        }

        /// <summary>
        /// Remove tag from product
        /// </summary>
        public void RemoveTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return;

            var tags = TagList;
            tags.RemoveAll(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
            Tags = string.Join(", ", tags);
        }

        /// <summary>
        /// Get effective price (considering variants)
        /// </summary>
        public decimal GetEffectivePrice()
        {
            if (HasVariants)
            {
                return MinVariantPrice ?? Price;
            }
            return Price;
        }

        /// <summary>
        /// Check if product needs restocking
        /// </summary>
        public bool NeedsRestocking()
        {
            return IsLowStock && TrackInventory;
        }

        #endregion
    }
}