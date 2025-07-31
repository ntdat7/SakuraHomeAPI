using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;

namespace SakuraHomeAPI.Models.Entities.Reviews
{
    /// <summary>
    /// Review responses (from seller/admin)
    /// </summary>
    [Table("ReviewResponses")]
    public class ReviewResponse : AuditableEntity
    {
        public int ReviewId { get; set; }

        [Required]
        public string Response { get; set; }

        public bool IsOfficial { get; set; } = false; // From brand/seller
        public bool IsPublic { get; set; } = true;

        public virtual Review Review { get; set; }

        [NotMapped]
        public string AuthorName => CreatedByUser?.FirstName ?? "Customer Service";
    }
}
