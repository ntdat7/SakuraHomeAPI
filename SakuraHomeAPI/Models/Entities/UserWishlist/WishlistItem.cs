using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Products;

namespace SakuraHomeAPI.Models.Entities.UserWishlist
{
    /// <summary>
    /// Wishlist item entity
    /// </summary>
    [Table("WishlistItems")]
    public class WishlistItem : AuditableEntity
    {
        public int WishlistId { get; set; }
        public int ProductId { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        public int Priority { get; set; } = 0;

        public bool NotifyOnSale { get; set; } = false;
        public bool NotifyOnRestock { get; set; } = false;
        public bool NotifyOnPriceChange { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DesiredPrice { get; set; }

        public virtual Wishlist Wishlist { get; set; }
        public virtual Product Product { get; set; }

        #region Computed Properties

        [NotMapped]
        public bool IsAvailable =>
            Product != null && Product.IsActive && !Product.IsDeleted;

        [NotMapped]
        public bool IsInStock => Product?.IsInStock ?? false;

        [NotMapped]
        public bool IsOnSale => Product?.IsOnSale ?? false;

        [NotMapped]
        public bool IsPriceReached =>
            DesiredPrice.HasValue && Product?.Price <= DesiredPrice.Value;

        [NotMapped]
        public string DisplayName => Product?.Name ?? "Unknown Product";

        [NotMapped]
        public string DisplayImage => Product?.MainImage;

        [NotMapped]
        public decimal? CurrentPrice => Product?.Price;

        [NotMapped]
        public decimal? OriginalPrice => Product?.OriginalPrice;

        #endregion

        #region Methods

        public bool ShouldNotify()
        {
            if (!IsAvailable) return false;
            return NotifyOnSale && IsOnSale
                || NotifyOnRestock && IsInStock
                || NotifyOnPriceChange && IsPriceReached;
        }

        public List<string> GetNotificationReasons()
        {
            var reasons = new List<string>();
            if (NotifyOnSale && IsOnSale) reasons.Add("On Sale");
            if (NotifyOnRestock && IsInStock) reasons.Add("Back in Stock");
            if (NotifyOnPriceChange && IsPriceReached) reasons.Add("Price Reached");
            return reasons;
        }

        #endregion
    }
}
