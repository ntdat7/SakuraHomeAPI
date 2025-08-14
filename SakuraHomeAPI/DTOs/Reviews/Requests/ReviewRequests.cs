using SakuraHomeAPI.Models.Entities.Reviews;

namespace SakuraHomeAPI.DTOs.Reviews.Requests
{
    public class CreateReviewRequest
    {
        public Guid UserId { get; set; }
        public int ProductId { get; set; }
        public int? OrderId { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool RecommendProduct { get; set; } = true;
        public string Pros { get; set; } = string.Empty;
        public string Cons { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new List<string>();
    }

    public class UpdateReviewRequest
    {
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool RecommendProduct { get; set; }
        public string Pros { get; set; } = string.Empty;
        public string Cons { get; set; } = string.Empty;
    }

    public class ReviewVoteRequest
    {
        public bool IsHelpful { get; set; }
    }

    public class ReviewReportRequest
    {
        public string Reason { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ReviewModerationRequest
    {
        public bool IsApproved { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class ReviewFilterRequest
    {
        public int ProductId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int? Rating { get; set; }
        public bool? VerifiedPurchase { get; set; }
        public bool? HasImages { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc";
    }

    // Additional specific request classes for different filter contexts
    public class ProductReviewFilterRequest : ReviewFilterRequest
    {
        // Already has ProductId from base class
    }

    public class MyReviewFilterRequest : ReviewFilterRequest
    {
        public Guid UserId { get; set; }
        // Remove ProductId requirement for user's own reviews
        public new int ProductId { get; set; } = 0;
    }

    public class AdminReviewFilterRequest : ReviewFilterRequest
    {
        public bool? IsApproved { get; set; }
        public bool? IsReported { get; set; }
        public Guid? UserId { get; set; }
        // Admin can see all, so ProductId is optional
        public new int ProductId { get; set; } = 0;
    }

    public class VoteReviewRequest : ReviewVoteRequest
    {
        // Inherits IsHelpful from ReviewVoteRequest
    }

    public class ReportReviewRequest : ReviewReportRequest
    {
        // Inherits Reason and Description from ReviewReportRequest
    }

    public class RejectReviewRequest
    {
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class RespondToReviewRequest
    {
        public string Response { get; set; } = string.Empty;
    }

    public class AddReviewImagesRequest
    {
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}