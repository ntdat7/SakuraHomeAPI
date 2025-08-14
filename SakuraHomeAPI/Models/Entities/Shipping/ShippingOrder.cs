using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Orders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities.Shipping
{
    [Table("ShippingOrders")]
    public class ShippingOrder : AuditableEntity
    {
        public int OrderId { get; set; }
        
        [Required, MaxLength(50)]
        public string TrackingNumber { get; set; }
        
        [MaxLength(20)]
        public string ServiceType { get; set; } = "STANDARD";
        
        [MaxLength(100)]
        public string ServiceName { get; set; } = "Giao hàng tiêu chuẩn";
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal CODFee { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalFee { get; set; }
        
        [MaxLength(50)]
        public string Status { get; set; } = "PENDING";
        
        [MaxLength(200)]
        public string StatusDescription { get; set; }
        
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        
        // Sender info
        [MaxLength(100)]
        public string SenderName { get; set; }
        
        [MaxLength(20)]
        public string SenderPhone { get; set; }
        
        [MaxLength(500)]
        public string SenderAddress { get; set; }
        
        // Receiver info
        [MaxLength(100)]
        public string ReceiverName { get; set; }
        
        [MaxLength(20)]
        public string ReceiverPhone { get; set; }
        
        [MaxLength(500)]
        public string ReceiverAddress { get; set; }
        
        // Package info
        [Column(TypeName = "decimal(8,2)")]
        public decimal Weight { get; set; }
        
        [MaxLength(50)]
        public string Dimensions { get; set; }
        
        public bool IsCOD { get; set; } = true;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal CODAmount { get; set; }
        
        [MaxLength(1000)]
        public string Notes { get; set; }
        
        // Navigation properties
        public virtual Order Order { get; set; }
        public virtual ICollection<ShippingTracking> ShippingTrackings { get; set; } = new List<ShippingTracking>();
    }
}