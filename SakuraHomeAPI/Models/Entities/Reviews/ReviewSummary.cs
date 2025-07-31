using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Products;

namespace SakuraHomeAPI.Models.Entities.Reviews
{
    /// <summary>
    /// Review summary for products (cached statistics)
    /// </summary>
    [Table("ReviewSummaries")]
    public class ReviewSummary : BaseEntity
    {
        public int ProductId { get; set; }

        public int TotalReviews { get; set; } = 0;
        public decimal AverageRating { get; set; } = 0;

        public int OneStar { get; set; } = 0;
        public int TwoStar { get; set; } = 0;
        public int ThreeStar { get; set; } = 0;
        public int FourStar { get; set; } = 0;
        public int FiveStar { get; set; } = 0;

        public int VerifiedPurchases { get; set; } = 0;
        public int WithImages { get; set; } = 0;
        public int Recommended { get; set; } = 0;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public virtual Product Product { get; set; }

        #region Computed Properties

        [NotMapped]
        public double RecommendationRate =>
            TotalReviews > 0 ? (double)Recommended / TotalReviews * 100 : 0;

        [NotMapped]
        public double VerificationRate =>
            TotalReviews > 0 ? (double)VerifiedPurchases / TotalReviews * 100 : 0;

        [NotMapped]
        public double ImageRate =>
            TotalReviews > 0 ? (double)WithImages / TotalReviews * 100 : 0;

        [NotMapped]
        public Dictionary<int, int> RatingDistribution => new()
        {
            {1,OneStar},{2,TwoStar},{3,ThreeStar},{4,FourStar},{5,FiveStar}
        };

        [NotMapped]
        public Dictionary<int, double> RatingPercentages =>
            RatingDistribution.ToDictionary(
                kvp => kvp.Key,
                kvp => TotalReviews > 0 ? (double)kvp.Value / TotalReviews * 100 : 0);

        #endregion

        #region Methods

        public void UpdateFromReviews(IEnumerable<Review> reviews)
        {
            var list = reviews.Where(r => r.IsApproved && r.IsActive).ToList();
            TotalReviews = list.Count;
            AverageRating = list.Any()
            ? (decimal)list.Average(r => r.Rating)
            : 0m;

            OneStar = list.Count(r => r.Rating == 1);
            TwoStar = list.Count(r => r.Rating == 2);
            ThreeStar = list.Count(r => r.Rating == 3);
            FourStar = list.Count(r => r.Rating == 4);
            FiveStar = list.Count(r => r.Rating == 5);

            VerifiedPurchases = list.Count(r => r.IsVerifiedPurchase);
            WithImages = list.Count(r => r.HasImages);
            Recommended = list.Count(r => r.RecommendProduct);

            LastUpdated = DateTime.UtcNow;
        }

        #endregion
    }
}
