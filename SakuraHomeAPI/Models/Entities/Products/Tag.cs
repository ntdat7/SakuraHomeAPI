using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Orders;

namespace SakuraHomeAPI.Models.Entities.Products
{
    /// <summary>
    /// Tags entity
    /// </summary>
    [Table("Tags")]
    public class Tag : FullEntity, ISlugifiable
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Slug { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(7)]
        public string Color { get; set; }

        public int UsageCount { get; set; } = 0;

        public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();

        #region Methods

        /// <summary>
        /// Update usage count
        /// </summary>
        public void UpdateUsageCount()
        {
            UsageCount = ProductTags?.Count ?? 0;
        }

        #endregion
    }
}
