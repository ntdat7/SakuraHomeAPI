using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.Models.Entities.Orders
{
    /// <summary>
    /// Order status history for tracking changes
    /// </summary>
    [Table("OrderStatusHistory")]
    public class OrderStatusHistory : AuditableEntity
    {
        public int OrderId { get; set; }
        public OrderStatus OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public string Notes { get; set; }

        public virtual Order Order { get; set; }
    }
}
