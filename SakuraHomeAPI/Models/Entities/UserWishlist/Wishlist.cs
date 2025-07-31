using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SakuraHomeAPI.Models.Entities.UserWishlist
{
    /// <summary>
    /// Wishlist entity
    /// </summary>
    [Table("Wishlists")]
    public class Wishlist : AuditableEntity
    {
        [Required]
        public int UserId { get; set; }

        [MaxLength(255)]
        public string Name { get; set; } = "My Wishlist";

        [MaxLength(1000)]
        public string Description { get; set; }

        public bool IsPublic { get; set; } = false;
        public bool IsDefault { get; set; } = true;

        public virtual User User { get; set; }
        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

        #region Computed Properties

        [NotMapped]
        public int TotalItems => WishlistItems.Count;

        [NotMapped]
        public bool IsEmpty => !WishlistItems.Any();

        [NotMapped]
        public bool HasItems => WishlistItems.Any();

        [NotMapped]
        public decimal TotalValue => WishlistItems.Sum(wi => wi.Product?.Price ?? 0);

        [NotMapped]
        public int AvailableItems => WishlistItems.Count(wi =>
            wi.Product != null && wi.Product.IsActive && !wi.Product.IsDeleted);

        #endregion

        #region Methods

        public WishlistItem AddItem(int productId, string notes = null)
        {
            var existing = WishlistItems.FirstOrDefault(wi => wi.ProductId == productId);
            if (existing != null)
            {
                if (!string.IsNullOrEmpty(notes))
                {
                    existing.Notes = notes;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                return existing;
            }
            var item = new WishlistItem
            {
                WishlistId = Id,
                ProductId = productId,
                Notes = notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            WishlistItems.Add(item);
            return item;
        }

        public bool RemoveItem(int productId)
        {
            var item = WishlistItems.FirstOrDefault(wi => wi.ProductId == productId);
            if (item == null) return false;
            WishlistItems.Remove(item);
            UpdatedAt = DateTime.UtcNow;
            return true;
        }

        public bool ContainsProduct(int productId) =>
            WishlistItems.Any(wi => wi.ProductId == productId);

        public void Clear()
        {
            WishlistItems.Clear();
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveUnavailableItems()
        {
            var toRemove = WishlistItems
                .Where(wi => wi.Product == null || !wi.Product.IsActive || wi.Product.IsDeleted)
                .ToList();
            foreach (var i in toRemove) WishlistItems.Remove(i);
            if (toRemove.Any()) UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
