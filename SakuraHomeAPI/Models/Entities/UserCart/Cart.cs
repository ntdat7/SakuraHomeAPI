using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SakuraHomeAPI.Models.Entities.UserCart
{
    /// <summary>
    /// Shopping cart entity
    /// </summary>
    [Table("Carts")]
    public class Cart : AuditableEntity
    {
        public Guid UserId { get; set; } // Changed from int to Guid to match User.Id

        [MaxLength(100)]
        public string SessionId { get; set; } // For guest users

        public virtual User User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        #region Computed Properties

        [NotMapped]
        public int TotalItems => CartItems.Sum(ci => ci.Quantity);

        [NotMapped]
        public int UniqueItems => CartItems.Count;

        [NotMapped]
        public decimal SubTotal => CartItems.Sum(ci => ci.TotalPrice);

        [NotMapped]
        public bool IsEmpty => !CartItems.Any();

        [NotMapped]
        public bool HasItems => CartItems.Any();

        #endregion

        #region Methods

        public CartItem AddItem(int productId, int quantity, int? variantId = null)
        {
            var existing = CartItems.FirstOrDefault(ci =>
                ci.ProductId == productId && ci.ProductVariantId == variantId);
            if (existing != null)
            {
                existing.Quantity += quantity;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.CalculateTotal();
                return existing;
            }

            var item = new CartItem
            {
                CartId = Id,
                ProductId = productId,
                ProductVariantId = variantId,
                Quantity = quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            CartItems.Add(item);
            return item;
        }

        public bool RemoveItem(int productId, int? variantId = null)
        {
            var item = CartItems.FirstOrDefault(ci =>
                ci.ProductId == productId && ci.ProductVariantId == variantId);
            if (item == null) return false;
            CartItems.Remove(item);
            return true;
        }

        public bool UpdateItemQuantity(int productId, int quantity, int? variantId = null)
        {
            var item = CartItems.FirstOrDefault(ci =>
                ci.ProductId == productId && ci.ProductVariantId == variantId);
            if (item == null) return false;
            if (quantity <= 0) return RemoveItem(productId, variantId);
            item.Quantity = quantity;
            item.UpdatedAt = DateTime.UtcNow;
            item.CalculateTotal();
            return true;
        }

        public void Clear()
        {
            CartItems.Clear();
            UpdatedAt = DateTime.UtcNow;
        }

        public bool ContainsProduct(int productId, int? variantId = null) =>
            CartItems.Any(ci => ci.ProductId == productId && ci.ProductVariantId == variantId);

        public int GetItemQuantity(int productId, int? variantId = null) =>
            CartItems.FirstOrDefault(ci => ci.ProductId == productId && ci.ProductVariantId == variantId)?.Quantity ?? 0;

        public void RemoveExpiredItems(int daysOld = 30)
        {
            var cutoff = DateTime.UtcNow.AddDays(-daysOld);
            var expired = CartItems.Where(ci => ci.CreatedAt < cutoff).ToList();
            foreach (var i in expired) CartItems.Remove(i);
            if (expired.Any()) UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
