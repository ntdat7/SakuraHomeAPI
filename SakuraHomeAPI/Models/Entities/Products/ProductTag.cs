using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Catalog;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities.Products
{
    /// <summary>
    /// Product tags for better organization - join table with audit tracking
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