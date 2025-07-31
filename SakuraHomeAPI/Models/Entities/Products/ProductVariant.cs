using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Orders;
using SakuraHomeAPI.Models.Entities.UserCart;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.Models.Entities.Products
{
    /// <summary>
    /// Product variants (size, color, etc.)
    /// </summary>
    [Table("ProductVariants")]
    public class ProductVariant : FullEntity
    {
        public int ProductId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SKU { get; set; }

        [MaxLength(255)]
        public string Name { get; set; } // "Size L - Red"

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }

        [Required]
        public int Stock { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        // Variant attributes stored as JSON
        public string Attributes { get; set; } // {"size":"L","color":"red"}

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Weight { get; set; }

        [MaxLength(50)]
        public string Dimensions { get; set; }

        public int SoldCount { get; set; } = 0;
        public int ViewCount { get; set; } = 0;

        public virtual Product Product { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        #region Computed Properties

        [NotMapped]
        public bool IsOnSale => OriginalPrice.HasValue && OriginalPrice > Price;

        [NotMapped]
        public bool IsInStock => Stock > 0;

        [NotMapped]
        public decimal? DiscountPercentage => OriginalPrice.HasValue && OriginalPrice > 0
            ? Math.Round((OriginalPrice.Value - Price) / OriginalPrice.Value * 100, 2)
            : null;

        [NotMapped]
        public string DisplayName => !string.IsNullOrEmpty(Name)
            ? Name
            : $"Variant {Id}";

        #endregion

        #region Methods

        public bool ReserveStock(int quantity)
        {
            if (Stock < quantity) return false;
            Stock -= quantity;
            return true;
        }

        public void ReleaseStock(int quantity) => Stock += quantity;

        #endregion
    }
}
