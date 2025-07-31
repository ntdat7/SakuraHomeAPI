using SakuraHomeAPI.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities
{
    [Table("Banners")]
    public class Banner : BaseEntity
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required, MaxLength(500)]
        public string ImageUrl { get; set; }

        [MaxLength(500)]
        public string LinkUrl { get; set; }

        [MaxLength(50)]
        public string Position { get; set; } = "Homepage"; // Homepage, Category, Product, etc.

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
        public int ViewCount { get; set; } = 0;
        public int ClickCount { get; set; } = 0;
    }
}
