using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Catalog;
using SakuraHomeAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities.Products
{
    /// <summary>
    /// Product attributes for filtering and specifications
    /// </summary>
    [Table("ProductAttributes")]
    public class ProductAttribute : TranslatableEntity, IOrderable
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Code { get; set; } // e.g. "size","color","material"

        [MaxLength(1000)]
        public string Description { get; set; }

        public AttributeType Type { get; set; } = AttributeType.Text;

        public bool IsRequired { get; set; } = false;
        public bool IsFilterable { get; set; } = true;
        public bool IsSearchable { get; set; } = false;
        public bool IsVariant { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;

        [MaxLength(1000)]
        public string Options { get; set; } // JSON array

        [MaxLength(100)]
        public string Unit { get; set; }

        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }

        [MaxLength(500)]
        public string ValidationRegex { get; set; }

        public virtual ICollection<ProductAttributeValue> Values { get; set; } = new List<ProductAttributeValue>();
        public virtual ICollection<CategoryAttribute> CategoryAttributes { get; set; } = new List<CategoryAttribute>();

        #region Methods

        public List<string> GetOptions()
        {
            if (string.IsNullOrEmpty(Options)) return new List<string>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(Options)
                       ?? new List<string>();
            }
            catch
            {
                return Options
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(o => o.Trim())
                    .ToList();
            }
        }

        public void SetOptions(IEnumerable<string> options)
        {
            Options = System.Text.Json.JsonSerializer.Serialize(options);
        }

        #endregion
    }
}
