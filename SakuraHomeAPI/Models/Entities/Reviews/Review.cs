using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Models.Entities.Orders;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.Models.Entities.Reviews
{
    /// <summary>
    /// Review entity for product reviews
    /// </summary>
    [Table("Reviews")]
    public class Review : FullEntity
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int? OrderId { get; set; } // Link to order for verified purchases

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        public string Comment { get; set; }

        public bool IsApproved { get; set; } = false;
        public bool IsVerifiedPurchase { get; set; } = false;
        public bool IsFeatured { get; set; } = false;

        public int HelpfulCount { get; set; } = 0;
        public int UnhelpfulCount { get; set; } = 0;
        public int ReportCount { get; set; } = 0;

        public bool RecommendProduct { get; set; } = true;

        [MaxLength(500)]
        public string Pros { get; set; }

        [MaxLength(500)]
        public string Cons { get; set; }

        public string Attributes { get; set; } // JSON

        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedBy { get; set; }
        public string RejectionReason { get; set; }

        public virtual User User { get; set; }
        public virtual Product Product { get; set; }
        public virtual Order Order { get; set; }
        public virtual User ApprovedByUser { get; set; }
        public virtual ICollection<ReviewImage> ReviewImages { get; set; } = new List<ReviewImage>();
        public virtual ICollection<ReviewVote> ReviewVotes { get; set; } = new List<ReviewVote>();
        public virtual ICollection<ReviewResponse> ReviewResponses { get; set; } = new List<ReviewResponse>();

        #region Computed Properties

        [NotMapped]
        public double HelpfulRatio => HelpfulCount + UnhelpfulCount > 0
            ? (double)HelpfulCount / (HelpfulCount + UnhelpfulCount) : 0;

        [NotMapped]
        public bool HasImages => ReviewImages.Any();

        [NotMapped]
        public bool HasResponse => ReviewResponses.Any();

        [NotMapped]
        public string RatingStars => new string('★', Rating) + new string('☆', 5 - Rating);

        [NotMapped]
        public string DisplayTitle => !string.IsNullOrEmpty(Title) ? Title : $"{Rating} Star Review";

        [NotMapped]
        public bool NeedsModeration => !IsApproved && ReportCount == 0;

        [NotMapped]
        public bool NeedsReview => ReportCount > 0;

        [NotMapped]
        public string AuthorName => User?.FirstName ?? "Anonymous";

        [NotMapped]
        public TimeSpan Age => DateTime.UtcNow - CreatedAt;

        [NotMapped]
        public string AgeDisplay
        {
            get
            {
                var age = Age;
                if (age.TotalDays >= 365)
                    return $"{(int)(age.TotalDays / 365)} year{((int)(age.TotalDays / 365) > 1 ? "s" : "")} ago";
                if (age.TotalDays >= 30)
                    return $"{(int)(age.TotalDays / 30)} month{((int)(age.TotalDays / 30) > 1 ? "s" : "")} ago";
                if (age.TotalDays >= 1)
                    return $"{(int)age.TotalDays} day{((int)age.TotalDays > 1 ? "s" : "")} ago";
                if (age.TotalHours >= 1)
                    return $"{(int)age.TotalHours} hour{((int)age.TotalHours > 1 ? "s" : "")} ago";
                return "Just now";
            }
        }

        #endregion

        #region Methods

        public void Approve(int? approvedBy = null)
        {
            IsApproved = true;
            ApprovedAt = DateTime.UtcNow;
            ApprovedBy = approvedBy;
            RejectionReason = null;
        }

        public void Reject(string reason, int? rejectedBy = null)
        {
            IsApproved = false;
            ApprovedAt = null;
            ApprovedBy = null;
            RejectionReason = reason;
            UpdatedBy = rejectedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetFeatured(bool featured = true)
        {
            IsFeatured = featured;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddHelpfulVote(int userId)
        {
            var existing = ReviewVotes.FirstOrDefault(rv => rv.UserId == userId);
            if (existing != null)
            {
                if (!existing.IsHelpful)
                {
                    existing.IsHelpful = true;
                    UnhelpfulCount = Math.Max(0, UnhelpfulCount - 1);
                    HelpfulCount++;
                }
            }
            else
            {
                ReviewVotes.Add(new ReviewVote
                {
                    ReviewId = Id,
                    UserId = userId,
                    IsHelpful = true,
                    CreatedAt = DateTime.UtcNow
                });
                HelpfulCount++;
            }
        }

        public void AddUnhelpfulVote(int userId)
        {
            var existing = ReviewVotes.FirstOrDefault(rv => rv.UserId == userId);
            if (existing != null)
            {
                if (existing.IsHelpful)
                {
                    existing.IsHelpful = false;
                    HelpfulCount = Math.Max(0, HelpfulCount - 1);
                    UnhelpfulCount++;
                }
            }
            else
            {
                ReviewVotes.Add(new ReviewVote
                {
                    ReviewId = Id,
                    UserId = userId,
                    IsHelpful = false,
                    CreatedAt = DateTime.UtcNow
                });
                UnhelpfulCount++;
            }
        }

        public void Report(int reportedBy, string reason)
        {
            ReportCount++;
            // Extend with detailed tracking if needed
        }

        public double CalculateScore()
        {
            var baseScore = Rating * 20;
            var helpfulBonus = HelpfulRatio * 10;
            var verifiedBonus = IsVerifiedPurchase ? 5 : 0;
            var featuredBonus = IsFeatured ? 10 : 0;
            return Math.Min(100, baseScore + helpfulBonus + verifiedBonus + featuredBonus);
        }

        #endregion
    }
}
