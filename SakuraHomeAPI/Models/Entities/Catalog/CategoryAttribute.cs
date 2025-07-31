using Microsoft.AspNetCore.Mvc;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities.Catalog
{
    /// <summary>
    /// Category-specific attributes for filtering
    /// </summary>
    [Table("CategoryAttributes")]
    public class CategoryAttribute : FullEntity, IOrderable
    {
        public int CategoryId { get; set; }
        public int AttributeId { get; set; }

        public bool IsRequired { get; set; } = false;
        public bool IsFilterable { get; set; } = true;
        public bool IsSearchable { get; set; } = false;
        public bool IsVariant { get; set; } = false; // Used for product variants

        public int DisplayOrder { get; set; } = 0;

        // Navigation
        public virtual Category Category { get; set; }
        public virtual ProductAttribute Attribute { get; set; }
    }
}
