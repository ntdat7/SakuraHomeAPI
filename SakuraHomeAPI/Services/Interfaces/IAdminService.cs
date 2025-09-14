using System;
using System.Threading.Tasks;
using SakuraHomeAPI.DTOs.Admin.Requests;
using SakuraHomeAPI.DTOs.Admin.Responses;
using SakuraHomeAPI.DTOs.Common;

namespace SakuraHomeAPI.Services.Interfaces
{
    public interface IAdminService
    {
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