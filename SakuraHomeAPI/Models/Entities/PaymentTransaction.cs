using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Models.Entities.Orders;
using SakuraHomeAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities
{
    [Table("PaymentTransactions")]
    public class PaymentTransaction : LogEntity
    {
        // Override UserId để dùng Guid thay vì int (nếu LogEntity vẫn dùng int)
        public new Guid? UserId { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required, MaxLength(100)]
        public string TransactionId { get; set; }

        [MaxLength(100)]
        public string ExternalTransactionId { get; set; }

        [Required, MaxLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Fee { get; set; } = 0;

        [Required, MaxLength(3)]
        public string Currency { get; set; } = "VND";

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(1000)]
        public string ResponseData { get; set; }

        public DateTime? ProcessedAt { get; set; }

        // Navigation Properties
        public virtual Order Order { get; set; }
        // Override navigation property
        public new virtual User User { get; set; }
    }
}