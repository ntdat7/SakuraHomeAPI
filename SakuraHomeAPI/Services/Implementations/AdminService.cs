using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.DTOs.Admin.Requests;
using SakuraHomeAPI.DTOs.Admin.Responses;
using SakuraHomeAPI.DTOs.Users.Responses;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.Services.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace SakuraHomeAPI.Services.Implementations
{
    /// <summary>
    /// Professional AdminService for advanced user management and admin operations
    /// </summary>
    public partial class AdminService : IAdminService
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
        public async Task<ApiResponse<PagedResult<UserSummaryDto>>> GetUsersAsync(UserFilterRequest filter)
        {
            try
            {
                var query = _context.Users.Where(u => !u.IsDeleted).AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchLower = filter.SearchTerm.ToLower();
                    query = query.Where(u =>
                        u.FirstName.ToLower().Contains(searchLower) ||
                        u.LastName.ToLower().Contains(searchLower) ||
                        u.Email.ToLower().Contains(searchLower) ||
                        (u.NationalIdCard != null && u.NationalIdCard.Contains(filter.SearchTerm)));
                }

                if (filter.Role.HasValue)
                    query = query.Where(u => u.Role == filter.Role.Value);

                if (filter.Status.HasValue)
                    query = query.Where(u => u.Status == filter.Status.Value);

                if (filter.IsActive.HasValue)
                    query = query.Where(u => u.IsActive == filter.IsActive.Value);

                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var userDtos = _mapper.Map<List<UserSummaryDto>>(users);

                var result = new PagedResult<UserSummaryDto>
                {
                    Items = userDtos,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return ApiResponse.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users list");
                return ApiResponse.ErrorResult<PagedResult<UserSummaryDto>>("Failed to get users");
            }
        }

        public async Task<ApiResponse<UserProfileDto>> GetUserDetailAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Addresses.Where(a => !a.IsDeleted))
                    .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

                if (user == null)
                    return ApiResponse.ErrorResult<UserProfileDto>("User not found");

                var userDto = _mapper.Map<UserProfileDto>(user);
                return ApiResponse.SuccessResult(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user detail for {UserId}", userId);
                return ApiResponse.ErrorResult<UserProfileDto>("Failed to get user detail");
            }
        }

        public async Task<ApiResponse<UserSummaryDto>> CreateUserAsync(CreateUserRequest request, Guid adminId)
        {
            try
            {
                var existingEmail = await _userManager.FindByEmailAsync(request.Email);
                if (existingEmail != null)
                    return ApiResponse.ErrorResult<UserSummaryDto>("Email already exists");

                if (!string.IsNullOrWhiteSpace(request.NationalIdCard))
                {
                    var existingCCCD = await _context.Users
                        .AnyAsync(u => u.NationalIdCard == request.NationalIdCard && !u.IsDeleted);
                    if (existingCCCD)
                        return ApiResponse.ErrorResult<UserSummaryDto>("CCCD already exists");
                }

                if (request.Role >= UserRole.Staff)
                {
                    if (string.IsNullOrWhiteSpace(request.NationalIdCard))
                        return ApiResponse.ErrorResult<UserSummaryDto>("CCCD is required for Staff/Admin");
                    if (!request.HireDate.HasValue)
                        return ApiResponse.ErrorResult<UserSummaryDto>("Hire date is required for Staff/Admin");
                    if (!request.BaseSalary.HasValue || request.BaseSalary.Value <= 0)
                        return ApiResponse.ErrorResult<UserSummaryDto>("Base salary is required for Staff/Admin");
                    if (request.HireDate.Value > DateTime.Now)
                        return ApiResponse.ErrorResult<UserSummaryDto>("Hire date cannot be in the future");
                }

                var user = new User
                {
                    Email = request.Email,
                    UserName = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender ?? Gender.Unknown,
                    Avatar = request.Avatar,
                    Role = request.Role,
                    Status = request.Status,
                    IsActive = request.IsActive,
                    NationalIdCard = request.NationalIdCard,
                    HireDate = request.HireDate,
                    BaseSalary = request.BaseSalary,
                    Tier = request.Tier ?? UserTier.Bronze,
                    Points = request.InitialPoints ?? 0,
                    TotalSpent = request.InitialSpent ?? 0,
                    PreferredLanguage = request.PreferredLanguage ?? "vi",
                    PreferredCurrency = request.PreferredCurrency ?? "VND",
                    EmailNotifications = request.EmailNotifications,
                    SmsNotifications = request.SmsNotifications,
                    PushNotifications = request.PushNotifications,
                    CreatedBy = adminId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    EmailVerified = true,
                    EmailVerifiedAt = DateTime.UtcNow,
                    Provider = LoginProvider.Local
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult<UserSummaryDto>("Failed to create user", errors);
                }

                _logger.LogInformation("User created by admin {AdminId}: {Email}", adminId, user.Email);
                var userDto = _mapper.Map<UserSummaryDto>(user);
                return ApiResponse.SuccessResult(userDto, "User created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return ApiResponse.ErrorResult<UserSummaryDto>("Failed to create user");
            }
        }

        public async Task<ApiResponse<UserSummaryDto>> UpdateUserAsync(Guid userId, UpdateUserRequest request, Guid adminId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted)
                    return ApiResponse.ErrorResult<UserSummaryDto>("User not found");

                if (!string.IsNullOrWhiteSpace(request.NationalIdCard) && request.NationalIdCard != user.NationalIdCard)
                {
                    var existingCCCD = await _context.Users
                        .AnyAsync(u => u.NationalIdCard == request.NationalIdCard && u.Id != userId && !u.IsDeleted);
                    if (existingCCCD)
                        return ApiResponse.ErrorResult<UserSummaryDto>("CCCD already exists");
                }

                if (!string.IsNullOrWhiteSpace(request.FirstName)) user.FirstName = request.FirstName;
                if (!string.IsNullOrWhiteSpace(request.LastName)) user.LastName = request.LastName;
                if (!string.IsNullOrWhiteSpace(request.PhoneNumber)) user.PhoneNumber = request.PhoneNumber;
                if (request.DateOfBirth.HasValue) user.DateOfBirth = request.DateOfBirth;
                if (request.Gender.HasValue) user.Gender = request.Gender;
                if (!string.IsNullOrWhiteSpace(request.Avatar)) user.Avatar = request.Avatar;

                if (user.IsEmployee)
                {
                    if (!string.IsNullOrWhiteSpace(request.NationalIdCard)) user.NationalIdCard = request.NationalIdCard;
                    if (request.HireDate.HasValue) user.HireDate = request.HireDate;
                    if (request.BaseSalary.HasValue) user.BaseSalary = request.BaseSalary;
                }

                if (request.Role.HasValue) user.Role = request.Role.Value;
                if (request.Status.HasValue) user.Status = request.Status.Value;
                if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;
                if (request.Tier.HasValue && user.Role == UserRole.Customer) user.Tier = request.Tier.Value;

                user.UpdatedBy = adminId;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult<UserSummaryDto>("Failed to update user", errors);
                }

                _logger.LogInformation("User {UserId} updated by admin {AdminId}", userId, adminId);
                var userDto = _mapper.Map<UserSummaryDto>(user);
                return ApiResponse.SuccessResult(userDto, "User updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return ApiResponse.ErrorResult<UserSummaryDto>("Failed to update user");
            }
        }

        public async Task<ApiResponse> DeleteUserAsync(Guid userId, Guid adminId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted)
                    return ApiResponse.ErrorResult("User not found");

                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                user.DeletedBy = adminId;
                user.IsActive = false;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult("Failed to delete user", errors);
                }

                _logger.LogInformation("User {UserId} deleted by admin {AdminId}", userId, adminId);
                return ApiResponse.SuccessResult("User deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return ApiResponse.ErrorResult("Failed to delete user");
            }
        }

        public async Task<ApiResponse> ToggleUserStatusAsync(Guid userId, Guid adminId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted)
                    return ApiResponse.ErrorResult("User not found");

                user.IsActive = !user.IsActive;
                user.UpdatedBy = adminId;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult("Failed to toggle user status", errors);
                }

                var message = user.IsActive ? "User activated successfully" : "User deactivated successfully";
                _logger.LogInformation("User {UserId} status toggled to {Status} by admin {AdminId}", userId, user.IsActive, adminId);
                return ApiResponse.SuccessResult(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status {UserId}", userId);
                return ApiResponse.ErrorResult("Failed to toggle user status");
            }
        }

        public async Task<ApiResponse<EmailCheckResponse>> CheckEmailAsync(string email)
        {
            try
            {
                var exists = await _userManager.FindByEmailAsync(email) != null;
                var response = new EmailCheckResponse
                {
                    Exists = exists,
                    Available = !exists,
                    Message = exists ? "Email already exists" : "Email is available"
                };
                return ApiResponse.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email {Email}", email);
                return ApiResponse.ErrorResult<EmailCheckResponse>("Failed to check email");
            }
        }

        public async Task<ApiResponse<NationalIdCheckResponse>> CheckNationalIdAsync(string nationalIdCard)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.NationalIdCard == nationalIdCard && !u.IsDeleted);
                var exists = user != null;
                var response = new NationalIdCheckResponse
                {
                    Exists = exists,
                    Available = !exists,
                    Message = exists ? "CCCD already exists" : "CCCD is available",
                    ExistingUser = exists ? _mapper.Map<UserSummaryDto>(user) : null
                };
                return ApiResponse.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking national ID card {CCCD}", nationalIdCard);
                return ApiResponse.ErrorResult<NationalIdCheckResponse>("Failed to check CCCD");
            }
        }
    }}
