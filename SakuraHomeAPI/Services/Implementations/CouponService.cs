using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.DTOs.Coupons.Requests;
using SakuraHomeAPI.DTOs.Coupons.Responses;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.Services.Common;
using SakuraHomeAPI.Services.Interfaces;

namespace SakuraHomeAPI.Services.Implementations
{
    public class CouponService : ICouponService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CouponService> _logger;

        public CouponService(
            ApplicationDbContext context,
            ILogger<CouponService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult<CouponValidationResponse>> ValidateCouponAsync(ValidateCouponRequest request)
        {
            try
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.Code.ToUpper());

                if (coupon == null)
                {
                    return ServiceResult<CouponValidationResponse>.Success(new CouponValidationResponse
                    {
                        IsValid = false,
                        Message = "Mã coupon không tồn tại",
                        DiscountAmount = 0,
                        FinalAmount = request.OrderAmount
                    });
                }

                if (!coupon.IsValidForOrder(request.OrderAmount))
                {
                    var message = GetInvalidCouponMessage(coupon, request.OrderAmount);
                    return ServiceResult<CouponValidationResponse>.Success(new CouponValidationResponse
                    {
                        IsValid = false,
                        Message = message,
                        DiscountAmount = 0,
                        FinalAmount = request.OrderAmount
                    });
                }

                var discountAmount = CalculateDiscount(coupon, request.OrderAmount);
                var finalAmount = request.OrderAmount - discountAmount;

                var response = new CouponValidationResponse
                {
                    IsValid = true,
                    Message = "Mã coupon hợp lệ",
                    DiscountAmount = discountAmount,
                    FinalAmount = finalAmount,
                    Coupon = MapToCouponResponse(coupon)
                };

                return ServiceResult<CouponValidationResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating coupon {Code}", request.Code);
                return ServiceResult<CouponValidationResponse>.Failure("Có lỗi xảy ra khi kiểm tra mã coupon");
            }
        }

        public async Task<ServiceResult<CouponResponse>> CreateCouponAsync(CreateCouponRequest request)
        {
            try
            {
                // Check if code is unique
                if (!await IsCouponCodeUniqueAsync(request.Code))
                {
                    return ServiceResult<CouponResponse>.Failure("Mã coupon đã tồn tại");
                }

                // Validate dates
                if (request.StartDate >= request.EndDate)
                {
                    return ServiceResult<CouponResponse>.Failure("Ngày bắt đầu phải nhỏ hơn ngày kết thúc");
                }

                var coupon = new Coupon
                {
                    Code = request.Code.ToUpper(),
                    Name = request.Name,
                    Description = request.Description,
                    Type = request.Type,
                    Value = request.Value,
                    MinOrderAmount = request.MinOrderAmount,
                    MaxDiscountAmount = request.MaxDiscountAmount,
                    UsageLimit = request.UsageLimit,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsActive = request.IsActive,
                    IsPublic = request.IsPublic,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();

                return ServiceResult<CouponResponse>.Success(MapToCouponResponse(coupon));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating coupon {Code}", request.Code);
                return ServiceResult<CouponResponse>.Failure("Có lỗi xảy ra khi tạo mã coupon");
            }
        }

        public async Task<ServiceResult<CouponResponse>> UpdateCouponAsync(int id, UpdateCouponRequest request)
        {
            try
            {
                var coupon = await _context.Coupons.FindAsync(id);
                if (coupon == null)
                {
                    return ServiceResult<CouponResponse>.Failure("Không tìm thấy mã coupon");
                }

                // Update fields
                coupon.Name = request.Name;
                coupon.Description = request.Description;
                coupon.Type = request.Type;
                coupon.Value = request.Value;
                coupon.MinOrderAmount = request.MinOrderAmount;
                coupon.MaxDiscountAmount = request.MaxDiscountAmount;
                coupon.UsageLimit = request.UsageLimit;
                coupon.StartDate = request.StartDate;
                coupon.EndDate = request.EndDate;
                coupon.IsActive = request.IsActive;
                coupon.IsPublic = request.IsPublic;
                coupon.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ServiceResult<CouponResponse>.Success(MapToCouponResponse(coupon));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating coupon {Id}", id);
                return ServiceResult<CouponResponse>.Failure("Có lỗi xảy ra khi cập nhật mã coupon");
            }
        }

        public async Task<ServiceResult<bool>> DeleteCouponAsync(int id)
        {
            try
            {
                var coupon = await _context.Coupons.FindAsync(id);
                if (coupon == null)
                {
                    return ServiceResult<bool>.Failure("Không tìm thấy mã coupon");
                }

                // Check if coupon has been used
                if (coupon.UsedCount > 0)
                {
                    return ServiceResult<bool>.Failure("Không thể xóa mã coupon đã được sử dụng");
                }

                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting coupon {Id}", id);
                return ServiceResult<bool>.Failure("Có lỗi xảy ra khi xóa mã coupon");
            }
        }

        public async Task<ServiceResult<CouponListResponse>> GetCouponsAsync(CouponFilterRequest filter)
        {
            try
            {
                var query = _context.Coupons.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(c => c.Code.Contains(filter.Search) || 
                                           c.Name.Contains(filter.Search));
                }

                if (filter.Type.HasValue)
                {
                    query = query.Where(c => c.Type == filter.Type.Value);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == filter.IsActive.Value);
                }

                if (filter.IsExpired.HasValue)
                {
                    var now = DateTime.UtcNow;
                    if (filter.IsExpired.Value)
                        query = query.Where(c => c.EndDate < now);
                    else
                        query = query.Where(c => c.EndDate >= now);
                }

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(c => c.StartDate >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(c => c.EndDate <= filter.EndDate.Value);
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "code" => filter.SortOrder == "desc" 
                        ? query.OrderByDescending(c => c.Code)
                        : query.OrderBy(c => c.Code),
                    "name" => filter.SortOrder == "desc"
                        ? query.OrderByDescending(c => c.Name)
                        : query.OrderBy(c => c.Name),
                    "value" => filter.SortOrder == "desc"
                        ? query.OrderByDescending(c => c.Value)
                        : query.OrderBy(c => c.Value),
                    "usedcount" => filter.SortOrder == "desc"
                        ? query.OrderByDescending(c => c.UsedCount)
                        : query.OrderBy(c => c.UsedCount),
                    "enddate" => filter.SortOrder == "desc"
                        ? query.OrderByDescending(c => c.EndDate)
                        : query.OrderBy(c => c.EndDate),
                    _ => filter.SortOrder == "desc"
                        ? query.OrderByDescending(c => c.CreatedAt)
                        : query.OrderBy(c => c.CreatedAt)
                };

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize);

                var coupons = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var response = new CouponListResponse
                {
                    Items = coupons.Select(MapToCouponResponse).ToList(),
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = filter.Page,
                    PageSize = filter.PageSize
                };

                return ServiceResult<CouponListResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coupons");
                return ServiceResult<CouponListResponse>.Failure("Có lỗi xảy ra khi lấy danh sách mã coupon");
            }
        }

        public async Task<ServiceResult<CouponStatsResponse>> GetCouponStatsAsync()
        {
            try
            {
                var coupons = await _context.Coupons.ToListAsync();
                var now = DateTime.UtcNow;

                var stats = new CouponStatsResponse
                {
                    TotalCoupons = coupons.Count,
                    ActiveCoupons = coupons.Count(c => c.IsActive && c.StartDate <= now && c.EndDate >= now),
                    ExpiredCoupons = coupons.Count(c => c.EndDate < now),
                    UsedCoupons = coupons.Count(c => c.UsedCount > 0),
                    TotalUsages = coupons.Sum(c => c.UsedCount),
                    CouponsByType = coupons.GroupBy(c => c.Type)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    TopCoupons = coupons
                        .Where(c => c.UsedCount > 0)
                        .OrderByDescending(c => c.UsedCount)
                        .Take(10)
                        .Select(c => new TopCouponResponse
                        {
                            Code = c.Code,
                            Name = c.Name,
                            UsedCount = c.UsedCount
                        })
                        .ToList()
                };

                return ServiceResult<CouponStatsResponse>.Success(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coupon stats");
                return ServiceResult<CouponStatsResponse>.Failure("Có lỗi xảy ra khi lấy thống kê mã coupon");
            }
        }

        public async Task<ServiceResult<bool>> UseCouponAsync(string code, Guid userId, decimal orderAmount)
        {
            try
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper());

                if (coupon == null || !coupon.IsValidForOrder(orderAmount))
                {
                    return ServiceResult<bool>.Failure("Mã coupon không hợp lệ");
                }

                if (!coupon.TryIncrementUsage())
                {
                    return ServiceResult<bool>.Failure("Mã coupon đã hết lượt sử dụng");
                }

                await _context.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error using coupon {Code} for user {UserId}", code, userId);
                return ServiceResult<bool>.Failure("Có lỗi xảy ra khi sử dụng mã coupon");
            }
        }

        public async Task<decimal> CalculateDiscountAsync(string couponCode, decimal orderAmount)
        {
            try
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code.ToUpper() == couponCode.ToUpper());

                if (coupon == null || !coupon.IsValidForOrder(orderAmount))
                {
                    return 0;
                }

                return CalculateDiscount(coupon, orderAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating discount for coupon {Code}", couponCode);
                return 0;
            }
        }

        public async Task<bool> IsCouponCodeUniqueAsync(string code, int? excludeId = null)
        {
            var query = _context.Coupons.Where(c => c.Code.ToUpper() == code.ToUpper());
            
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        // Helper methods
        private decimal CalculateDiscount(Coupon coupon, decimal orderAmount)
        {
            decimal discount = 0;

            if (coupon.Type == CouponType.Percentage)
            {
                discount = orderAmount * (coupon.Value / 100);
            }
            else if (coupon.Type == CouponType.FixedAmount)
            {
                discount = coupon.Value;
            }

            // Apply maximum discount limit
            if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
            {
                discount = coupon.MaxDiscountAmount.Value;
            }

            // Ensure discount doesn't exceed order amount
            return Math.Min(discount, orderAmount);
        }

        private string GetInvalidCouponMessage(Coupon coupon, decimal orderAmount)
        {
            var now = DateTime.UtcNow;

            if (!coupon.IsActive)
                return "Mã coupon đã bị vô hiệu hóa";

            if (now < coupon.StartDate)
                return $"Mã coupon chưa có hiệu lực. Có hiệu lực từ {coupon.StartDate:dd/MM/yyyy}";

            if (now > coupon.EndDate)
                return $"Mã coupon đã hết hạn vào {coupon.EndDate:dd/MM/yyyy}";

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                return "Mã coupon đã hết lượt sử dụng";

            if (coupon.MinOrderAmount.HasValue && orderAmount < coupon.MinOrderAmount.Value)
                return $"Đơn hàng tối thiểu {coupon.MinOrderAmount.Value:N0}đ để sử dụng mã này";

            return "Mã coupon không hợp lệ";
        }

        private CouponResponse MapToCouponResponse(Coupon coupon)
        {
            var now = DateTime.UtcNow;
            var isExpired = coupon.EndDate < now;
            var isValid = coupon.IsActive && !isExpired && 
                         now >= coupon.StartDate && 
                         (!coupon.UsageLimit.HasValue || coupon.UsedCount < coupon.UsageLimit.Value);

            return new CouponResponse
            {
                Id = coupon.Id,
                Code = coupon.Code,
                Name = coupon.Name,
                Description = coupon.Description,
                Type = coupon.Type,
                Value = coupon.Value,
                MinOrderAmount = coupon.MinOrderAmount,
                MaxDiscountAmount = coupon.MaxDiscountAmount,
                UsageLimit = coupon.UsageLimit,
                UsedCount = coupon.UsedCount,
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                IsActive = coupon.IsActive,
                IsPublic = coupon.IsPublic,
                CreatedAt = coupon.CreatedAt,
                UpdatedAt = coupon.UpdatedAt,
                IsExpired = isExpired,
                IsValid = isValid,
                RemainingUsage = coupon.UsageLimit.HasValue ? 
                    Math.Max(0, coupon.UsageLimit.Value - coupon.UsedCount) : -1,
                UsagePercentage = coupon.UsageLimit.HasValue ? 
                    (double)coupon.UsedCount / coupon.UsageLimit.Value * 100 : 0,
                TypeDisplay = coupon.Type == CouponType.Percentage ? "Phần trăm" : "Số tiền cố định",
                ValueDisplay = coupon.Type == CouponType.Percentage ? 
                    $"{coupon.Value}%" : $"{coupon.Value:N0}đ",
                StatusDisplay = isValid ? "Có hiệu lực" : 
                              (isExpired ? "Đã hết hạn" : 
                              (!coupon.IsActive ? "Đã vô hiệu" : "Chưa có hiệu lực"))
            };
        }

        // Implement remaining interface methods
        public async Task<ServiceResult<CouponResponse>> GetCouponByCodeAsync(string code)
        {
            try
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper());

                if (coupon == null)
                {
                    return ServiceResult<CouponResponse>.Failure("Không tìm thấy mã coupon");
                }

                return ServiceResult<CouponResponse>.Success(MapToCouponResponse(coupon));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coupon by code {Code}", code);
                return ServiceResult<CouponResponse>.Failure("Có lỗi xảy ra khi lấy thông tin mã coupon");
            }
        }

        public async Task<ServiceResult<CouponResponse>> GetCouponByIdAsync(int id)
        {
            try
            {
                var coupon = await _context.Coupons.FindAsync(id);

                if (coupon == null)
                {
                    return ServiceResult<CouponResponse>.Failure("Không tìm thấy mã coupon");
                }

                return ServiceResult<CouponResponse>.Success(MapToCouponResponse(coupon));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coupon by id {Id}", id);
                return ServiceResult<CouponResponse>.Failure("Có lỗi xảy ra khi lấy thông tin mã coupon");
            }
        }

        public async Task<ServiceResult<bool>> ToggleCouponStatusAsync(int id, bool isActive)
        {
            try
            {
                var coupon = await _context.Coupons.FindAsync(id);
                if (coupon == null)
                {
                    return ServiceResult<bool>.Failure("Không tìm thấy mã coupon");
                }

                coupon.IsActive = isActive;
                coupon.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling coupon status {Id}", id);
                return ServiceResult<bool>.Failure("Có lỗi xảy ra khi cập nhật trạng thái mã coupon");
            }
        }

        public async Task<ServiceResult<List<CouponResponse>>> GetExpiredCouponsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var expiredCoupons = await _context.Coupons
                    .Where(c => c.EndDate < now)
                    .OrderByDescending(c => c.EndDate)
                    .ToListAsync();

                return ServiceResult<List<CouponResponse>>.Success(
                    expiredCoupons.Select(MapToCouponResponse).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expired coupons");
                return ServiceResult<List<CouponResponse>>.Failure("Có lỗi xảy ra khi lấy mã coupon đã hết hạn");
            }
        }

        public async Task<ServiceResult<List<CouponResponse>>> GetExpiringCouponsAsync(int days = 7)
        {
            try
            {
                var now = DateTime.UtcNow;
                var expiringDate = now.AddDays(days);
                
                var expiringCoupons = await _context.Coupons
                    .Where(c => c.IsActive && c.EndDate >= now && c.EndDate <= expiringDate)
                    .OrderBy(c => c.EndDate)
                    .ToListAsync();

                return ServiceResult<List<CouponResponse>>.Success(
                    expiringCoupons.Select(MapToCouponResponse).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring coupons");
                return ServiceResult<List<CouponResponse>>.Failure("Có lỗi xảy ra khi lấy mã coupon sắp hết hạn");
            }
        }

        public async Task<ServiceResult<bool>> RevertCouponUsageAsync(string code)
        {
            try
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper());

                if (coupon == null)
                {
                    return ServiceResult<bool>.Failure("Không tìm thấy mã coupon");
                }

                coupon.DecrementUsage();
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reverting coupon usage {Code}", code);
                return ServiceResult<bool>.Failure("Có lỗi xảy ra khi hoàn lại lượt sử dụng mã coupon");
            }
        }

        public async Task<ServiceResult<CouponListResponse>> GetActiveCouponsAsync(CouponFilterRequest filter)
        {
            try
            {
                var now = DateTime.UtcNow;
                var query = _context.Coupons
                    .Where(c => c.IsActive && 
                               c.StartDate <= now && 
                               c.EndDate >= now &&
                               c.IsPublic);

                // Apply additional filters
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(c => c.Code.Contains(filter.Search) || 
                                           c.Name.Contains(filter.Search));
                }

                if (filter.Type.HasValue)
                {
                    query = query.Where(c => c.Type == filter.Type.Value);
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "value" => filter.SortOrder == "desc"
                        ? query.OrderByDescending(c => c.Value)
                        : query.OrderBy(c => c.Value),
                    "enddate" => filter.SortOrder == "desc"
                        ? query.OrderByDescending(c => c.EndDate)
                        : query.OrderBy(c => c.EndDate),
                    _ => filter.SortOrder == "desc"
                        ? query.OrderByDescending(c => c.CreatedAt)
                        : query.OrderBy(c => c.CreatedAt)
                };

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize);

                var coupons = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var response = new CouponListResponse
                {
                    Items = coupons.Select(MapToCouponResponse).ToList(),
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = filter.Page,
                    PageSize = filter.PageSize
                };

                return ServiceResult<CouponListResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active coupons");
                return ServiceResult<CouponListResponse>.Failure("Có lỗi xảy ra khi lấy danh sách mã coupon hoạt động");
            }
        }

        public async Task<ServiceResult<object>> GetCouponUsageHistoryAsync(Guid userId)
        {
            try
            {
                // This would typically require a separate CouponUsage table
                // For now, return a placeholder implementation
                var history = new
                {
                    UserId = userId,
                    TotalUsed = 0,
                    TotalSaved = 0m,
                    UsageHistory = new List<object>()
                };

                return ServiceResult<object>.Success(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coupon usage history for user {UserId}", userId);
                return ServiceResult<object>.Failure("Có lỗi xảy ra khi lấy lịch sử sử dụng mã coupon");
            }
        }
    }
}