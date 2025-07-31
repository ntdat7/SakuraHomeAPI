using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Products;

namespace SakuraHomeAPI.Models.Entities.UserCart
{
    /// <summary>
    /// Shopping cart item entity
    /// </summary>
    [Table("CartItems")]
    public class CartItem : AuditableEntity
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public string CustomOptions { get; set; }
        public string GiftMessage { get; set; }
        public bool IsGift { get; set; } = false;

        public virtual Cart Cart { get; set; }
        public virtual Product Product { get; set; }
        public virtual ProductVariant ProductVariant { get; set; }

        public virtual ICollection<CartItem> Items { get; set; }
            = new List<CartItem>();
        #region Computed Properties

        [NotMapped]
        public decimal EffectivePrice =>
            ProductVariant?.Price ?? Product?.Price ?? UnitPrice;

        [NotMapped]
        public string DisplayName =>
            ProductVariant != null
                ? $"{Product?.Name} - {ProductVariant.Name}"
                : Product?.Name ?? "Unknown Product";

        [NotMapped]
        public string DisplayImage =>
            ProductVariant?.ImageUrl ?? Product?.MainImage;

        [NotMapped]
        public bool IsAvailable =>
            (ProductVariant?.IsActive ?? Product?.IsActive ?? false) &&
            !((ProductVariant?.IsDeleted ?? Product?.IsDeleted) ?? true);

        [NotMapped]
        public bool IsInStock =>
            ProductVariant?.IsInStock ?? Product?.IsInStock ?? false;

        [NotMapped]
        public int AvailableStock =>
            ProductVariant?.Stock ?? Product?.Stock ?? 0;

        #endregion

        #region Methods

        public void CalculateTotal()
        {
            UnitPrice = EffectivePrice;
            TotalPrice = UnitPrice * Quantity;
        }

        public bool IsQuantityAvailable() =>
            Quantity <= AvailableStock ||
            (ProductVariant?.Product?.AllowBackorder ?? Product?.AllowBackorder ?? false);

        public (bool IsValid, string Error) Validate()
        {
            if (!IsAvailable)
                return (false, "Product is no longer available");
            if (!IsInStock && !(ProductVariant?.Product?.AllowBackorder ?? Product?.AllowBackorder ?? false))
                return (false, "Product is out of stock");
            if (!IsQuantityAvailable())
                return (false, $"Only {AvailableStock} items available");
            if (Quantity <= 0)
                return (false, "Quantity must be greater than 0");
            return (true, null);
        }

        #endregion
    }
}
