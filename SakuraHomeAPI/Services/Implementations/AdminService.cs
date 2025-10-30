using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.DTOs.Admin.Requests;
using SakuraHomeAPI.DTOs.Admin.Responses;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.Services.Interfaces;
using AutoMapper;

namespace SakuraHomeAPI.Services.Implementations
{
    /// <summary>
    /// Professional AdminService for advanced user management and admin operations
    /// </summary>
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IMapper mapper,
            ILogger<AdminService> logger)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<AdminUserListResponseDto>> GetUserListAsync(AdminUserFilterRequestDto filter)
        {
            try
            {
                var query = _context.Users.AsQueryable();
                if (!string.IsNullOrWhiteSpace(filter.Keyword))
                {
                    var keyword = filter.Keyword.ToLower();
                    query = query.Where(u => u.UserName.ToLower().Contains(keyword)
                        || u.Email.ToLower().Contains(keyword));
                }
                if (!string.IsNullOrWhiteSpace(filter.Role) && Enum.TryParse<UserRole>(filter.Role, true, out var roleEnum))
                    query = query.Where(u => u.Role == roleEnum);
                if (filter.IsActive.HasValue)
                    query = query.Where(u => u.IsActive == filter.IsActive.Value);
                query = query.Where(u => !u.IsDeleted);
                int total = await query.CountAsync();
                int page = filter.Page > 0 ? filter.Page : 1;
                int pageSize = filter.PageSize > 0 && filter.PageSize <= 100 ? filter.PageSize : 20;
                var users = await query.OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                var userDtos = users.Select(u => new AdminUserDetailDto
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = u.FullName,
                    Avatar = u.Avatar,
                    Role = u.Role.ToString(),
                    Status = u.Status.ToString(),
                    Tier = u.Tier.ToString(),
                    IsActive = u.IsActive,
                    EmailConfirmed = u.EmailConfirmed,
                    PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    // Add statistical fields
                    TotalSpent = u.TotalSpent,
                    TotalOrders = u.TotalOrders,
                    Points = u.Points,
                    LastOrderDate = u.LastOrderDate
                }).ToList();
                var response = new AdminUserListResponseDto
                {
                    Users = userDtos,
                    TotalCount = total,
                    Page = page,
                    PageSize = pageSize
                };
                return ApiResponse.SuccessResult(response, "User list retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user list");
                return ApiResponse.ErrorResult<AdminUserListResponseDto>("Failed to retrieve user list");
            }
        }

        public async Task<ApiResponse<AdminUserDetailDto>> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted)
                    return ApiResponse.ErrorResult<AdminUserDetailDto>("User not found");
                var dto = new AdminUserDetailDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Avatar = user.Avatar,
                    Role = user.Role.ToString(),
                    Status = user.Status.ToString(),
                    Tier = user.Tier.ToString(),
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    // Add statistical fields
                    TotalSpent = user.TotalSpent,
                    TotalOrders = user.TotalOrders,
                    Points = user.Points,
                    LastOrderDate = user.LastOrderDate
                };
                return ApiResponse.SuccessResult(dto, "User detail retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user detail");
                return ApiResponse.ErrorResult<AdminUserDetailDto>("Failed to retrieve user detail");
            }
        }

        public async Task<ApiResponse<AdminUserDetailDto>> CreateUserAsync(AdminCreateUserRequestDto request, Guid? createdBy)
        {
            try
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    UserName = request.UserName,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Avatar = null,
                    Role = Enum.TryParse<UserRole>(request.Role, true, out var roleEnum) ? roleEnum : UserRole.Staff,
                    Status = AccountStatus.Active,
                    Tier = UserTier.Bronze,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy
                };
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult<AdminUserDetailDto>("Failed to create user", errors);
                }
                var dto = new AdminUserDetailDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Avatar = user.Avatar,
                    Role = user.Role.ToString(),
                    Status = user.Status.ToString(),
                    Tier = user.Tier.ToString(),
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    // Add statistical fields
                    TotalSpent = user.TotalSpent,
                    TotalOrders = user.TotalOrders,
                    Points = user.Points,
                    LastOrderDate = user.LastOrderDate
                };
                return ApiResponse.SuccessResult(dto, "User created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return ApiResponse.ErrorResult<AdminUserDetailDto>("Failed to create user");
            }
        }

        public async Task<ApiResponse<AdminUserDetailDto>> UpdateUserAsync(Guid userId, AdminUpdateUserRequestDto request, Guid? updatedBy)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted)
                    return ApiResponse.ErrorResult<AdminUserDetailDto>("User not found");
                user.Email = request.Email;
                user.UserName = request.UserName;
                if (!string.IsNullOrWhiteSpace(request.Role) && Enum.TryParse<UserRole>(request.Role, true, out var roleEnum))
                    user.Role = roleEnum;
                user.IsActive = request.IsActive;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = updatedBy;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult<AdminUserDetailDto>("Failed to update user", errors);
                }
                var dto = new AdminUserDetailDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Avatar = user.Avatar,
                    Role = user.Role.ToString(),
                    Status = user.Status.ToString(),
                    Tier = user.Tier.ToString(),
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    // Add statistical fields
                    TotalSpent = user.TotalSpent,
                    TotalOrders = user.TotalOrders,
                    Points = user.Points,
                    LastOrderDate = user.LastOrderDate
                };
                return ApiResponse.SuccessResult(dto, "User updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return ApiResponse.ErrorResult<AdminUserDetailDto>("Failed to update user");
            }
        }

        public async Task<ApiResponse> DeleteUserAsync(Guid userId, Guid? deletedBy)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted)
                    return ApiResponse.ErrorResult("User not found");
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                user.DeletedBy = deletedBy;
                user.IsActive = false;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult("Failed to delete user", errors);
                }
                return ApiResponse.SuccessResult("User deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return ApiResponse.ErrorResult("Failed to delete user");
            }
        }

        public async Task<ApiResponse> ChangeUserStatusAsync(Guid userId, AdminChangeUserStatusRequestDto request, Guid? changedBy)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted)
                    return ApiResponse.ErrorResult("User not found");
                user.IsActive = request.IsActive;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = changedBy;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult("Failed to change user status", errors);
                }
                return ApiResponse.SuccessResult("User status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing user status");
                return ApiResponse.ErrorResult("Failed to change user status");
            }
        }

        public async Task<ApiResponse<AdminUserStatisticsDto>> GetUserStatisticsAsync()
        {
            try
            {
                var users = await _context.Users.Where(u => !u.IsDeleted).ToListAsync();
                var stats = new AdminUserStatisticsDto
                {
                    TotalUsers = users.Count,
                    ActiveUsers = users.Count(u => u.IsActive),
                    InactiveUsers = users.Count(u => !u.IsActive),
                    Admins = users.Count(u => u.Role == UserRole.Admin),
                    Staffs = users.Count(u => u.Role == UserRole.Staff),
                    Customers = users.Count(u => u.Role == UserRole.Customer)
                };
                return ApiResponse.SuccessResult(stats, "User statistics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics");
                return ApiResponse.ErrorResult<AdminUserStatisticsDto>("Failed to retrieve user statistics");
            }
        }

        public async Task<ApiResponse> ResetUserPasswordAsync(Guid userId, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted)
                    return ApiResponse.ErrorResult("User not found");
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult("Failed to reset password", errors);
                }
                return ApiResponse.SuccessResult("Password reset successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting user password");
                return ApiResponse.ErrorResult("Failed to reset password");
            }
        }

        public async Task<ApiResponse> UnlockUserAccountAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted)
                    return ApiResponse.ErrorResult("User not found");
                user.LockoutEnd = null;
                user.AccessFailedCount = 0;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult("Failed to unlock user", errors);
                }
                return ApiResponse.SuccessResult("User account unlocked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user account");
                return ApiResponse.ErrorResult("Failed to unlock user");
            }
        }

        public async Task<ApiResponse> VerifyUserEmailAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted)
                    return ApiResponse.ErrorResult("User not found");
                user.EmailConfirmed = true;
                user.EmailVerified = true;
                user.EmailVerifiedAt = DateTime.UtcNow;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult("Failed to verify email", errors);
                }
                return ApiResponse.SuccessResult("User email verified successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying user email");
                return ApiResponse.ErrorResult("Failed to verify email");
            }
        }

        public async Task<ApiResponse> VerifyUserPhoneAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted)
                    return ApiResponse.ErrorResult("User not found");
                user.PhoneNumberConfirmed = true;
                user.PhoneVerified = true;
                user.PhoneVerifiedAt = DateTime.UtcNow;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult("Failed to verify phone", errors);
                }
                return ApiResponse.SuccessResult("User phone verified successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying user phone");
                return ApiResponse.ErrorResult("Failed to verify phone");
            }
        }
    }
}
