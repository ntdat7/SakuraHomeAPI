using SakuraHomeAPI.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities.Reviews
{
    /// <summary>
    /// Review images
    /// </summary>
    [Table("ReviewImages")]
    public class ReviewImage : BaseEntity
    {
        public int ReviewId { get; set; }

        [Required, MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Caption { get; set; } = string.Empty;

        [MaxLength(100)]
        public string AltText { get; set; } = string.Empty;

        public int SortOrder { get; set; } = 0;

        public long FileSize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        [MaxLength(10)]
        public string FileExtension { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual Review Review { get; set; } = null!;
    }
}
