using SakuraHomeAPI.DTOs.Admin.Requests;
using SakuraHomeAPI.DTOs.Admin.Responses;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.DTOs.Users.Responses;
using System;
using System.Threading.Tasks;


namespace SakuraHomeAPI.Services.Interfaces
{
    public interface IAdminService
    {
        /// <summary>
        /// Get paginated list of users with filters
        /// </summary>
        Task<ApiResponse<PagedResult<UserSummaryDto>>> GetUsersAsync(UserFilterRequest filter);

        /// <summary>
        /// Get user details by ID
        /// </summary>
        Task<ApiResponse<UserProfileDto>> GetUserDetailAsync(Guid userId);

        /// <summary>
        /// Create new user (Customer or Staff/Admin)
        /// </summary>
        Task<ApiResponse<UserSummaryDto>> CreateUserAsync(CreateUserRequest request, Guid adminId);

        /// <summary>
        /// Update existing user
        /// </summary>
        Task<ApiResponse<UserSummaryDto>> UpdateUserAsync(Guid userId, UpdateUserRequest request, Guid adminId);

        /// <summary>
        /// Soft delete user
        /// </summary>
        Task<ApiResponse> DeleteUserAsync(Guid userId, Guid adminId);

        /// <summary>
        /// Toggle user active status
        /// </summary>
        Task<ApiResponse> ToggleUserStatusAsync(Guid userId, Guid adminId);

        /// <summary>
        /// Check if email exists
        /// </summary>
        Task<ApiResponse<EmailCheckResponse>> CheckEmailAsync(string email);

        /// <summary>
        /// Check if national ID card exists
        /// </summary>
        Task<ApiResponse<NationalIdCheckResponse>> CheckNationalIdAsync(string nationalIdCard);

        Task<ApiResponse<AdminUserListResponseDto>> GetUserListAsync(AdminUserFilterRequestDto filter);
        Task<ApiResponse<AdminUserDetailDto>> GetUserByIdAsync(Guid userId);
        Task<ApiResponse<AdminUserDetailDto>> CreateUserAsync(AdminCreateUserRequestDto request, Guid? createdBy);
        Task<ApiResponse<AdminUserDetailDto>> UpdateUserAsync(Guid userId, AdminUpdateUserRequestDto request, Guid? updatedBy);
        Task<ApiResponse> DeleteUserAsync(Guid userId, Guid? deletedBy);
        Task<ApiResponse> ChangeUserStatusAsync(Guid userId, AdminChangeUserStatusRequestDto request, Guid? changedBy);
        Task<ApiResponse<AdminUserStatisticsDto>> GetUserStatisticsAsync();
        Task<ApiResponse> ResetUserPasswordAsync(Guid userId, string newPassword);
        Task<ApiResponse> UnlockUserAccountAsync(Guid userId);
        Task<ApiResponse> VerifyUserEmailAsync(Guid userId);
        Task<ApiResponse> VerifyUserPhoneAsync(Guid userId);
    }
}