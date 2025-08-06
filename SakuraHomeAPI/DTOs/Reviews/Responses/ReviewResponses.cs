namespace SakuraHomeAPI.DTOs.Reviews.Responses
{
    public class ReviewResponse
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int ProductId { get; set; }
        public int? OrderId { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public bool IsFeatured { get; set; }
        public int HelpfulCount { get; set; }
        public int UnhelpfulCount { get; set; }
        public bool RecommendProduct { get; set; }
        public string Pros { get; set; } = string.Empty;
        public string Cons { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // User info
        public string UserName { get; set; } = string.Empty;
        public string UserTier { get; set; } = string.Empty;
        
        // Product info
        public string ProductName { get; set; } = string.Empty;
        
        // Images
        public List<ReviewImageDto> Images { get; set; } = new List<ReviewImageDto>();
        
        // Computed fields
        public double HelpfulRatio { get; set; }
        public string RatingStars { get; set; } = string.Empty;
        public string AgeDisplay { get; set; } = string.Empty;
        public bool HasImages { get; set; }
        public bool HasResponse { get; set; }
    }

    public class ReviewImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    public class ReviewStatsResponse
    {
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new Dictionary<int, int>();
        public int VerifiedPurchases { get; set; }
        public int RecommendationCount { get; set; }
        public double RecommendationPercentage { get; set; }
    }

    public class ReviewListResponse
    {
        public List<ReviewResponse> Items { get; set; } = new List<ReviewResponse>();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public ReviewStatsResponse Stats { get; set; } = new();
    }

    public class ReviewVoteResponse
    {
        public bool IsHelpful { get; set; }
        public int NewHelpfulCount { get; set; }
        public int NewUnhelpfulCount { get; set; }
        public double NewHelpfulRatio { get; set; }
    }

    public class ProductReviewSummaryResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingBreakdown { get; set; } = new Dictionary<int, int>();
        public List<ReviewResponse> FeaturedReviews { get; set; } = new List<ReviewResponse>();
        public List<ReviewResponse> RecentReviews { get; set; } = new List<ReviewResponse>();
    }
}