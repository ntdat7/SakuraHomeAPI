using SakuraHomeAPI.DTOs.Reviews.Requests;
using SakuraHomeAPI.DTOs.Reviews.Responses;
using SakuraHomeAPI.Models.DTOs;

namespace SakuraHomeAPI.Services.Interfaces
{
    public interface IReviewService
    {
        // Customer operations
        Task<ApiResponse<DTOs.Reviews.Responses.ReviewResponse>> CreateReviewAsync(CreateReviewRequest request);
        Task<ApiResponse<DTOs.Reviews.Responses.ReviewResponse>> UpdateReviewAsync(int reviewId, UpdateReviewRequest request, Guid userId);
        Task<ApiResponse<bool>> DeleteReviewAsync(int reviewId, Guid userId);
        Task<ApiResponse<bool>> VoteReviewAsync(int reviewId, VoteReviewRequest request, Guid userId);
        Task<ApiResponse<bool>> ReportReviewAsync(int reviewId, ReportReviewRequest request, Guid userId);
        
        // Public operations
        Task<ApiResponse<object>> GetProductReviewsAsync(ProductReviewFilterRequest filter);
        Task<ApiResponse<object>> GetProductReviewSummaryAsync(int productId);
        Task<ApiResponse<DTOs.Reviews.Responses.ReviewResponse>> GetReviewByIdAsync(int reviewId);
        Task<ApiResponse<object>> GetUserReviewsAsync(MyReviewFilterRequest filter);
        
        // Staff operations
        Task<ApiResponse<object>> GetAllReviewsAsync(AdminReviewFilterRequest filter);
        Task<ApiResponse<DTOs.Reviews.Responses.ReviewResponse>> ApproveReviewAsync(int reviewId, Guid staffId);
        Task<ApiResponse<DTOs.Reviews.Responses.ReviewResponse>> RejectReviewAsync(int reviewId, RejectReviewRequest request, Guid staffId);
        Task<ApiResponse<DTOs.Reviews.Responses.ReviewResponse>> RespondToReviewAsync(int reviewId, RespondToReviewRequest request, Guid staffId);
        Task<ApiResponse<bool>> AddReviewImagesAsync(int reviewId, AddReviewImagesRequest request, Guid userId);
        Task<ApiResponse<bool>> RemoveReviewImageAsync(int reviewId, int imageId, Guid userId);
        
        // Helper methods
        Task<bool> CanUserReviewProductAsync(Guid userId, int productId);
        Task<bool> HasUserPurchasedProductAsync(Guid userId, int productId);
    }
}