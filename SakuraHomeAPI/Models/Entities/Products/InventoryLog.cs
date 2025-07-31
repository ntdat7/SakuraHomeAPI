using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities.Products
{
    /// <summary>
    /// Inventory log for tracking stock changes
    /// </summary>
    [Table("InventoryLogs")]
    public class InventoryLog : LogEntity
    {
        // Override UserId để dùng Guid thay vì int (nếu LogEntity vẫn dùng int)
        public new Guid? UserId { get; set; } // Who made the change

        #region Basic Information
        [Required]
        public Guid ProductId { get; set; }
        public Guid? ProductVariantId { get; set; }
        [Required]
        public InventoryAction Action { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public int PreviousStock { get; set; }
        [Required]
        public int NewStock { get; set; }
        #endregion

        #region Transaction Details
        [MaxLength(500)]
        public string Reason { get; set; }
        [MaxLength(100)]
        public string ReferenceType { get; set; } // Order, Return, Adjustment, etc.
        public Guid? ReferenceId { get; set; } // Order ID, Return ID, etc.
        [MaxLength(50)]
        public string BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        #endregion

        #region User & Location
        [MaxLength(100)]
        public string Location { get; set; } // Warehouse, Store, etc.
        [MaxLength(500)]
        public string Notes { get; set; }
        #endregion

        #region Cost & Value
        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitCost { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalCost { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalValue { get; set; }
        #endregion

        #region Navigation Properties
        public virtual Product Product { get; set; }
        public virtual ProductVariant ProductVariant { get; set; }
        // Override navigation property
        public new virtual User User { get; set; }
        #endregion

        #region Computed Properties
        [NotMapped]
        public string DisplayAction => Action switch
        {
            InventoryAction.Purchase => "Nhập hàng",
            InventoryAction.Sale => "Bán hàng",
            InventoryAction.Return => "Trả hàng",
            InventoryAction.Adjustment => "Điều chỉnh",
            InventoryAction.Damage => "Hư hỏng",
            InventoryAction.Transfer => "Chuyển kho",
            InventoryAction.Lost => "Mất hàng",
            InventoryAction.Found => "Tìm thấy",
            InventoryAction.Expired => "Hết hạn",
            _ => Action.ToString()
        };

        [NotMapped]
        public string FormattedQuantity => Quantity > 0 ? $"+{Quantity}" : Quantity.ToString();

        [NotMapped]
        public string StockChange => $"{PreviousStock} → {NewStock}";
        #endregion
    }
}