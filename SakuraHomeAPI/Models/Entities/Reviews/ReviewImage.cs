using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;

namespace SakuraHomeAPI.Models.Entities.Reviews
{
    /// <summary>
    /// Review images
    /// </summary>
    [Table("ReviewImages")]
    public class ReviewImage : AuditableEntity, IOrderable
    {
        public int ReviewId { get; set; }

        [Required, MaxLength(500)]
        public string ImageUrl { get; set; }

        [MaxLength(255)]
        public string Caption { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public int? Width { get; set; }
        public int? Height { get; set; }
        public long? FileSize { get; set; }

        public virtual Review Review { get; set; }
    }
}
