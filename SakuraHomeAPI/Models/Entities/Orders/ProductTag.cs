using Azure;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Products;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities.Orders
{
    /// <summary>
    /// Product tags for better organization
    /// </summary>
    [Table("ProductTags")]
    public class ProductTag : AuditableEntity
    {
        public int ProductId { get; set; }
        public int TagId { get; set; }

        public virtual Product Product { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
