using SakuraHomeAPI.Models.DTOs;
using SakuraHomeAPI.DTOs.Users.Requests;
using SakuraHomeAPI.DTOs.Users.Responses;

namespace SakuraHomeAPI.Services.Interfaces
{
    /// <summary>
    /// User profile service interface
    /// </summary>
    public interface IUserService
    {
        // Profile Management
        Task<ApiResponse<UserProfileDto>> GetProfileAsync(Guid userId);
        Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request);
        Task<ApiResponse> DeleteProfileAsync(Guid userId);
        
        // Address Management
        Task<ApiResponse<List<AddressResponseDto>>> GetAddressesAsync(Guid userId);
        Task<ApiResponse<AddressResponseDto>> CreateAddressAsync(CreateAddressRequestDto request, Guid userId);
        Task<ApiResponse<AddressResponseDto>> UpdateAddressAsync(int addressId, UpdateAddressRequestDto request, Guid userId);
        Task<ApiResponse> DeleteAddressAsync(int addressId, Guid userId);
        Task<ApiResponse> SetDefaultAddressAsync(int addressId, Guid userId);
        
        // User Stats & Analytics
        Task<ApiResponse<UserStatsDto>> GetUserStatsAsync(Guid userId);
        // Admin: User List
       // Task<ApiResponse<UserListResponseDto>> GetUserListAsync(UserFilterRequestDto filter);
    }
}