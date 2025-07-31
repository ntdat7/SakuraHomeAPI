using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Products;

namespace SakuraHomeAPI.Models.Entities.Orders
{
    /// <summary>
    /// Order items
    /// </summary>
    [Table("OrderItems")]
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        // Snapshot data (preserved even if product changes)
        [MaxLength(500)]
        public string ProductName { get; set; }

        [MaxLength(100)]
        public string ProductSku { get; set; }

        [MaxLength(500)]
        public string ProductImage { get; set; }

        [MaxLength(255)]
        public string VariantName { get; set; }

        [MaxLength(100)]
        public string VariantSku { get; set; }

        public string ProductAttributes { get; set; } // JSON snapshot

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
        public virtual ProductVariant ProductVariant { get; set; }

        #region Computed Properties

        [NotMapped]
        public string DisplayName =>
            !string.IsNullOrEmpty(VariantName)
                ? $"{ProductName} - {VariantName}"
                : ProductName;

        [NotMapped]
        public string DisplaySku =>
            !string.IsNullOrEmpty(VariantSku)
                ? VariantSku
                : ProductSku;

        #endregion

        #region Methods

        /// <summary>
        /// Calculate total price
        /// </summary>
        public void CalculateTotal()
        {
            TotalPrice = UnitPrice * Quantity;
        }

        /// <summary>
        /// Create snapshot of product data
        /// </summary>
        public void CreateSnapshot()
        {
            if (Product != null)
            {
                ProductName = Product.Name;
                ProductSku = Product.SKU;
                ProductImage = Product.MainImage;
            }

            if (ProductVariant != null)
            {
                VariantName = ProductVariant.Name;
                VariantSku = ProductVariant.SKU;
                UnitPrice = ProductVariant.Price;
            }
            else if (Product != null)
            {
                UnitPrice = Product.Price;
            }

            CalculateTotal();
        }

        #endregion
    }
}
