using System;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Identity;

namespace SakuraHomeAPI.Models.Entities.Reviews
{
    /// <summary>
    /// Review votes (helpful/unhelpful)
    /// </summary>
    [Table("ReviewVotes")]
    public class ReviewVote : BaseEntity
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public bool IsHelpful { get; set; } // true = helpful, false = unhelpful
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Review Review { get; set; }
        public virtual User User { get; set; }
    }
}
