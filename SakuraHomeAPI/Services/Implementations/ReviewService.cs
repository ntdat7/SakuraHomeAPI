using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.DTOs.Reviews.Requests;
using SakuraHomeAPI.DTOs.Reviews.Responses;
using SakuraHomeAPI.Models.Entities.Reviews;
using SakuraHomeAPI.Models.DTOs;
using SakuraHomeAPI.Services.Interfaces;

namespace SakuraHomeAPI.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<ReviewService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<DTOs.Reviews.Responses.ReviewResponse>> CreateReviewAsync(CreateReviewRequest request)
        {
            try
            {
                // Check if user can review this product
                if (!await CanUserReviewProductAsync(request.UserId, request.ProductId))
                {
                    return ApiResponse.ErrorResult<DTOs.Reviews.Responses.ReviewResponse>("Bạn không thể đánh giá sản phẩm này");
                }

                // Check if user already reviewed this product
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.UserId == request.UserId && r.ProductId == request.ProductId);

                if (existingReview != null)
                {
                    return ApiResponse.ErrorResult<DTOs.Reviews.Responses.ReviewResponse>("Bạn đã đánh giá sản phẩm này rồi");
                }

                // Check if this is a verified purchase
                bool isVerifiedPurchase = await HasUserPurchasedProductAsync(request.UserId, request.ProductId);

                var review = new Review
                {
                    UserId = request.UserId,
                    ProductId = request.ProductId,
                    OrderId = request.OrderId,
                    Rating = request.Rating,
                    Title = request.Title,
                    Comment = request.Comment,
                    RecommendProduct = request.RecommendProduct,
                    Pros = request.Pros,
                    Cons = request.Cons,
                    IsVerifiedPurchase = isVerifiedPurchase,
                    IsApproved = isVerifiedPurchase, // Auto-approve verified purchases
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                // Add images if provided
                if (request.ImageUrls?.Any() == true)
                {
                    var images = request.ImageUrls.Select((url, index) => new ReviewImage
                    {
                        ReviewId = review.Id,
                        ImageUrl = url,
                        SortOrder = index + 1,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    _context.ReviewImages.AddRange(images);
                    await _context.SaveChangesAsync();
                }

                // Load review with related data
                var createdReview = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Product)
                    .Include(r => r.ReviewImages)
                    .FirstOrDefaultAsync(r => r.Id == review.Id);

                return ApiResponse.SuccessResult(MapToReviewResponse(createdReview), "Tạo đánh giá thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for user {UserId}, product {ProductId}", request.UserId, request.ProductId);
                return ApiResponse.ErrorResult<DTOs.Reviews.Responses.ReviewResponse>("Có lỗi xảy ra khi tạo đánh giá");
            }
        }

        public async Task<ApiResponse<DTOs.Reviews.Responses.ReviewResponse>> UpdateReviewAsync(int reviewId, UpdateReviewRequest request, Guid userId)
        {
            try
            {
                var review = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Product)
                    .Include(r => r.ReviewImages)
                    .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

                if (review == null)
                {
                    return ApiResponse.ErrorResult<DTOs.Reviews.Responses.ReviewResponse>("Không tìm thấy đánh giá");
                }

                // Update fields
                review.Rating = request.Rating;
                review.Title = request.Title;
                review.Comment = request.Comment;
                review.RecommendProduct = request.RecommendProduct;
                review.Pros = request.Pros;
                review.Cons = request.Cons;
                review.UpdatedAt = DateTime.UtcNow;

                // If review was approved, mark for re-approval if significantly changed
                if (review.IsApproved && !review.IsVerifiedPurchase)
                {
                    review.IsApproved = false;
                }

                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResult(MapToReviewResponse(review), "Cập nhật đánh giá thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId} by user {UserId}", reviewId, userId);
                return ApiResponse.ErrorResult<DTOs.Reviews.Responses.ReviewResponse>("Có lỗi xảy ra khi cập nhật đánh giá");
            }
        }

        public async Task<ApiResponse<bool>> DeleteReviewAsync(int reviewId, Guid userId)
        {
            try
            {
                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

                if (review == null)
                {
                    return ApiResponse.ErrorResult<bool>("Không tìm thấy đánh giá");
                }

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResult(true, "Xóa đánh giá thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId} by user {UserId}", reviewId, userId);
                return ApiResponse.ErrorResult<bool>("Có lỗi xảy ra khi xóa đánh giá");
            }
        }

        public async Task<ApiResponse<object>> GetProductReviewsAsync(ProductReviewFilterRequest filter)
        {
            try
            {
                var query = _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.ReviewImages)
                    .Where(r => r.ProductId == filter.ProductId && r.IsApproved);

                // Apply filters
                if (filter.Rating.HasValue)
                {
                    query = query.Where(r => r.Rating == filter.Rating.Value);
                }

                if (filter.VerifiedPurchase.HasValue)
                {
                    query = query.Where(r => r.IsVerifiedPurchase == filter.VerifiedPurchase.Value);
                }

                if (filter.HasImages.HasValue)
                {
                    if (filter.HasImages.Value)
                        query = query.Where(r => r.ReviewImages.Any());
                    else
                        query = query.Where(r => !r.ReviewImages.Any());
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "rating" => filter.SortOrder == "desc" 
                        ? query.OrderByDescending(r => r.Rating)
                        : query.OrderBy(r => r.Rating),
                    "helpful" => filter.SortOrder == "desc"
                        ? query.OrderByDescending(r => r.HelpfulCount)
                        : query.OrderBy(r => r.HelpfulCount),
                    _ => filter.SortOrder == "desc"
                        ? query.OrderByDescending(r => r.CreatedAt)
                        : query.OrderBy(r => r.CreatedAt)
                };

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize);

                var reviews = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var response = new
                {
                    Items = reviews.Select(MapToReviewResponse).ToList(),
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = filter.Page,
                    PageSize = filter.PageSize
                };

                return ApiResponse.SuccessResult<object>(response, "Lấy danh sách đánh giá thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for product {ProductId}", filter.ProductId);
                return ApiResponse.ErrorResult<object>("Có lỗi xảy ra khi lấy đánh giá");
            }
        }

        // Implement remaining methods...
        // Continue implementing the rest of the interface methods following similar patterns
        
        public async Task<bool> CanUserReviewProductAsync(Guid userId, int productId)
        {
            var existingReview = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId);

            return !existingReview;
        }

        public async Task<bool> HasUserPurchasedProductAsync(Guid userId, int productId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .AnyAsync(oi => oi.Order.UserId == userId && 
                               oi.ProductId == productId && 
                               oi.Order.Status == Models.Enums.OrderStatus.Delivered);
        }

        private DTOs.Reviews.Responses.ReviewResponse MapToReviewResponse(Review review)
        {
            return new DTOs.Reviews.Responses.ReviewResponse
            {
                Id = review.Id,
                UserId = review.UserId,
                ProductId = review.ProductId,
                OrderId = review.OrderId,
                Rating = review.Rating,
                Title = review.Title,
                Comment = review.Comment,
                IsApproved = review.IsApproved,
                IsVerifiedPurchase = review.IsVerifiedPurchase,
                IsFeatured = review.IsFeatured,
                HelpfulCount = review.HelpfulCount,
                UnhelpfulCount = review.UnhelpfulCount,
                RecommendProduct = review.RecommendProduct,
                Pros = review.Pros,
                Cons = review.Cons,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,
                UserName = review.User?.FirstName + " " + review.User?.LastName,
                UserTier = review.User?.Tier.ToString(),
                ProductName = review.Product?.Name,
                Images = review.ReviewImages?.Select(i => new ReviewImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    SortOrder = i.SortOrder
                }).ToList() ?? new List<ReviewImageDto>(),
                HelpfulRatio = review.HelpfulRatio,
                RatingStars = review.RatingStars,
                AgeDisplay = review.AgeDisplay,
                HasImages = review.HasImages,
                HasResponse = review.HasResponse
            };
        }

        // Placeholder implementations for remaining interface methods
        public async Task<ApiResponse<object>> GetProductReviewSummaryAsync(int productId)
        {
            return ApiResponse.SuccessResult<object>(null, "Not implemented yet");
        }

        public async Task<ApiResponse<DTOs.Reviews.Responses.ReviewResponse>> GetReviewByIdAsync(int reviewId)
        {
            return ApiResponse.ErrorResult<DTOs.Reviews.Responses.ReviewResponse>("Not implemented yet");
        }

        public async Task<ApiResponse<object>> GetUserReviewsAsync(MyReviewFilterRequest filter)
        {
            return ApiResponse.SuccessResult<object>(null, "Not implemented yet");
        }

        public async Task<ApiResponse<object>> GetAllReviewsAsync(AdminReviewFilterRequest filter)
        {
            return ApiResponse.SuccessResult<object>(null, "Not implemented yet");
        }

        public async Task<ApiResponse<DTOs.Reviews.Responses.ReviewResponse>> ApproveReviewAsync(int reviewId, Guid staffId)
        {
            return ApiResponse.ErrorResult<DTOs.Reviews.Responses.ReviewResponse>("Not implemented yet");
        }

        public async Task<ApiResponse<DTOs.Reviews.Responses.ReviewResponse>> RejectReviewAsync(int reviewId, RejectReviewRequest request, Guid staffId)
        {
            return ApiResponse.ErrorResult<DTOs.Reviews.Responses.ReviewResponse>("Not implemented yet");
        }

        public async Task<ApiResponse<DTOs.Reviews.Responses.ReviewResponse>> RespondToReviewAsync(int reviewId, RespondToReviewRequest request, Guid staffId)
        {
            return ApiResponse.ErrorResult<DTOs.Reviews.Responses.ReviewResponse>("Not implemented yet");
        }

        public async Task<ApiResponse<bool>> VoteReviewAsync(int reviewId, VoteReviewRequest request, Guid userId)
        {
            return ApiResponse.ErrorResult<bool>("Not implemented yet");
        }

        public async Task<ApiResponse<bool>> ReportReviewAsync(int reviewId, ReportReviewRequest request, Guid userId)
        {
            return ApiResponse.ErrorResult<bool>("Not implemented yet");
        }

        public async Task<ApiResponse<bool>> AddReviewImagesAsync(int reviewId, AddReviewImagesRequest request, Guid userId)
        {
            return ApiResponse.ErrorResult<bool>("Not implemented yet");
        }

        public async Task<ApiResponse<bool>> RemoveReviewImageAsync(int reviewId, int imageId, Guid userId)
        {
            return ApiResponse.ErrorResult<bool>("Not implemented yet");
        }
    }
}