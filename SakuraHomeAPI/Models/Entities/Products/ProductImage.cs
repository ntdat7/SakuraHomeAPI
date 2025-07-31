using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.Models.Entities.Products
{
    /// <summary>
    /// Product images
    /// </summary>
    [Table("ProductImages")]
    public class ProductImage : AuditableEntity, IOrderable
    {
        public int ProductId { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [MaxLength(255)]
        public string AltText { get; set; }

        [MaxLength(500)]
        public string Caption { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsMain { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public int? Width { get; set; }
        public int? Height { get; set; }
        public long? FileSize { get; set; }

        [MaxLength(50)]
        public string FileExtension { get; set; }

        public virtual Product Product { get; set; }

        #region Computed Properties

        [NotMapped]
        public string FormattedFileSize
        {
            get
            {
                if (!FileSize.HasValue) return "Unknown";
                string[] sizes = { "B", "KB", "MB", "GB" };
                double len = FileSize.Value;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len /= 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
        }

        [NotMapped]
        public string Dimensions => Width.HasValue && Height.HasValue
            ? $"{Width}x{Height}"
            : "Unknown";

        #endregion
    }
}
