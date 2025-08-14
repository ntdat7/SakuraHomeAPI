using SakuraHomeAPI.DTOs.Coupons.Requests;
using SakuraHomeAPI.DTOs.Coupons.Responses;
using SakuraHomeAPI.Services.Common;

namespace SakuraHomeAPI.Services.Interfaces
{
    public interface ICouponService
    {
        // Public operations
        Task<ServiceResult<CouponValidationResponse>> ValidateCouponAsync(ValidateCouponRequest request);
        Task<ServiceResult<CouponResponse>> GetCouponByCodeAsync(string code);
        
        // Admin operations
        Task<ServiceResult<CouponListResponse>> GetCouponsAsync(CouponFilterRequest filter);
        Task<ServiceResult<CouponResponse>> GetCouponByIdAsync(int id);
        Task<ServiceResult<CouponResponse>> CreateCouponAsync(CreateCouponRequest request);
        Task<ServiceResult<CouponResponse>> UpdateCouponAsync(int id, UpdateCouponRequest request);
        Task<ServiceResult<bool>> DeleteCouponAsync(int id);
        Task<ServiceResult<bool>> ToggleCouponStatusAsync(int id, bool isActive);
        
        // Statistics
        Task<ServiceResult<CouponStatsResponse>> GetCouponStatsAsync();
        Task<ServiceResult<List<CouponResponse>>> GetExpiredCouponsAsync();
        Task<ServiceResult<List<CouponResponse>>> GetExpiringCouponsAsync(int days = 7);
        
        // Utility methods
        Task<decimal> CalculateDiscountAsync(string couponCode, decimal orderAmount);
        Task<bool> IsCouponCodeUniqueAsync(string code, int? excludeId = null);
        Task<ServiceResult<bool>> UseCouponAsync(string code, Guid userId, decimal orderAmount);
        Task<ServiceResult<bool>> RevertCouponUsageAsync(string code);
        
        // Missing methods
        Task<ServiceResult<CouponListResponse>> GetActiveCouponsAsync(CouponFilterRequest filter);
        Task<ServiceResult<object>> GetCouponUsageHistoryAsync(Guid userId);
    }
}