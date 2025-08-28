using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.Services.Interfaces;
using SakuraHomeAPI.Models.DTOs;
using SakuraHomeAPI.DTOs.Users.Requests;
using SakuraHomeAPI.DTOs.Users.Responses;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Models.Enums;
using AutoMapper;

namespace SakuraHomeAPI.Services.Implementations
{
    /// <summary>
    /// User service implementation for profile and address management
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IMapper mapper,
            ILogger<UserService> logger)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted || !user.IsActive)
                    return ApiResponse.ErrorResult<UserProfileDto>("User not found");

                var userProfile = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = $"{user.FirstName} {user.LastName}".Trim(),
                    PhoneNumber = user.PhoneNumber,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    Avatar = user.Avatar,
                    Points = user.Points,
                    TotalSpent = user.TotalSpent,
                    TotalOrders = user.TotalOrders,
                    Tier = user.Tier,
                    Role = user.Role,
                    Status = user.Status,
                    PreferredLanguage = user.PreferredLanguage,
                    PreferredCurrency = user.PreferredCurrency,
                    EmailNotifications = user.EmailNotifications,
                    SmsNotifications = user.SmsNotifications,
                    PushNotifications = user.PushNotifications,
                    EmailVerified = user.EmailVerified,
                    PhoneVerified = user.PhoneVerified,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                };

                return ApiResponse.SuccessResult(userProfile, "Profile retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile for user {UserId}", userId);
                return ApiResponse.ErrorResult<UserProfileDto>("Failed to retrieve profile");
            }
        }

        public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted || !user.IsActive)
                    return ApiResponse.ErrorResult<UserProfileDto>("User not found");

                // Update user properties
                if (!string.IsNullOrEmpty(request.FirstName))
                    user.FirstName = request.FirstName;

                if (!string.IsNullOrEmpty(request.LastName))
                    user.LastName = request.LastName;

                if (!string.IsNullOrEmpty(request.PhoneNumber))
                    user.PhoneNumber = request.PhoneNumber;

                if (request.DateOfBirth.HasValue)
                    user.DateOfBirth = request.DateOfBirth.Value;

                if (request.Gender.HasValue)
                    user.Gender = request.Gender.Value;

                if (!string.IsNullOrEmpty(request.Avatar))
                    user.Avatar = request.Avatar;

                if (!string.IsNullOrEmpty(request.PreferredLanguage))
                    user.PreferredLanguage = request.PreferredLanguage;

                if (!string.IsNullOrEmpty(request.PreferredCurrency))
                    user.PreferredCurrency = request.PreferredCurrency;

                if (request.EmailNotifications.HasValue)
                    user.EmailNotifications = request.EmailNotifications.Value;

                if (request.SmsNotifications.HasValue)
                    user.SmsNotifications = request.SmsNotifications.Value;

                if (request.PushNotifications.HasValue)
                    user.PushNotifications = request.PushNotifications.Value;

                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult<UserProfileDto>("Failed to update profile", errors);
                }

                return await GetProfileAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                return ApiResponse.ErrorResult<UserProfileDto>("Failed to update profile");
            }
        }

        public async Task<ApiResponse> DeleteProfileAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted)
                    return ApiResponse.ErrorResult("User not found");

                // Soft delete
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                user.IsActive = false;
                user.Status = AccountStatus.Inactive;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult("Failed to delete profile", errors);
                }

                _logger.LogInformation("User profile deleted for user {UserId}", userId);
                return ApiResponse.SuccessResult("Profile deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile for user {UserId}", userId);
                return ApiResponse.ErrorResult("Failed to delete profile");
            }
        }

        public async Task<ApiResponse<List<AddressResponseDto>>> GetAddressesAsync(Guid userId)
        {
            try
            {
                var addresses = await _context.Addresses
                    .Where(a => a.UserId == userId && !a.IsDeleted)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenBy(a => a.CreatedAt)
                    .ToListAsync();

                var addressDtos = addresses.Select(a => new AddressResponseDto
                {
                    Id = a.Id,
                    AddressLine1 = a.AddressLine1,
                    AddressLine2 = a.AddressLine2,
                    ProvinceId = a.ProvinceId,
                    WardId = a.WardId,
                    PostalCode = a.PostalCode,
                    Country = a.Country,
                    Phone = a.Phone,
                    Name = a.Name,
                    IsDefault = a.IsDefault,
                    Type = a.Type,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                }).ToList();

                return ApiResponse.SuccessResult(addressDtos, "Addresses retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses for user {UserId}", userId);
                return ApiResponse.ErrorResult<List<AddressResponseDto>>("Failed to retrieve addresses");
            }
        }

        public async Task<ApiResponse<AddressResponseDto>> CreateAddressAsync(CreateAddressRequestDto request, Guid userId)
        {
            try
            {
                // If this is the first address or set as default, make it default
                var existingAddresses = await _context.Addresses
                    .Where(a => a.UserId == userId && !a.IsDeleted)
                    .ToListAsync();

                var isFirstAddress = !existingAddresses.Any();
                var shouldBeDefault = isFirstAddress || request.IsDefault;

                // If setting as default, unset other defaults
                if (shouldBeDefault)
                {
                    foreach (var addr in existingAddresses.Where(a => a.IsDefault))
                    {
                        addr.IsDefault = false;
                        addr.UpdatedAt = DateTime.UtcNow;
                    }
                }

                var address = new Address
                {
                    UserId = userId,
                    Name = request.Name,
                    Phone = request.Phone,
                    AddressLine1 = request.AddressLine1,
                    AddressLine2 = request.AddressLine2,
                    ProvinceId = request.ProvinceId,
                    WardId = request.WardId,
                    PostalCode = request.PostalCode,
                    Country = request.Country ?? "Vietnam",
                    IsDefault = shouldBeDefault,
                    Type = request.Type,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();

                var addressDto = new AddressResponseDto
                {
                    Id = address.Id,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    ProvinceId = address.ProvinceId,
                    WardId = address.WardId,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    Phone = address.Phone,
                    Name = address.Name,
                    IsDefault = address.IsDefault,
                    Type = address.Type,
                    Notes = address.Notes,
                    CreatedAt = address.CreatedAt,
                    UpdatedAt = address.UpdatedAt
                };

                _logger.LogInformation("Address created for user {UserId}", userId);
                return ApiResponse.SuccessResult(addressDto, "Address created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address for user {UserId}", userId);
                return ApiResponse.ErrorResult<AddressResponseDto>("Failed to create address");
            }
        }

        public async Task<ApiResponse<AddressResponseDto>> UpdateAddressAsync(int addressId, UpdateAddressRequestDto request, Guid userId)
        {
            try
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && !a.IsDeleted);

                if (address == null)
                    return ApiResponse.ErrorResult<AddressResponseDto>("Address not found");

                // Update properties
                if (!string.IsNullOrEmpty(request.Name))
                    address.Name = request.Name;

                if (!string.IsNullOrEmpty(request.Phone))
                    address.Phone = request.Phone;

                if (!string.IsNullOrEmpty(request.AddressLine1))
                    address.AddressLine1 = request.AddressLine1;

                if (request.AddressLine2 != null)
                    address.AddressLine2 = request.AddressLine2;

                if (request.ProvinceId.HasValue)
                    address.ProvinceId = request.ProvinceId.Value;

                if (request.WardId.HasValue)
                    address.WardId = request.WardId.Value;

                if (!string.IsNullOrEmpty(request.PostalCode))
                    address.PostalCode = request.PostalCode;

                if (!string.IsNullOrEmpty(request.Country))
                    address.Country = request.Country;

                if (request.Type.HasValue)
                    address.Type = request.Type.Value;

                if (request.Notes != null)
                    address.Notes = request.Notes;

                // Handle default address
                if (request.IsDefault.HasValue && request.IsDefault.Value && !address.IsDefault)
                {
                    // Unset other defaults
                    var otherAddresses = await _context.Addresses
                        .Where(a => a.UserId == userId && a.Id != addressId && a.IsDefault && !a.IsDeleted)
                        .ToListAsync();

                    foreach (var addr in otherAddresses)
                    {
                        addr.IsDefault = false;
                        addr.UpdatedAt = DateTime.UtcNow;
                    }

                    address.IsDefault = true;
                }

                address.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var addressDto = new AddressResponseDto
                {
                    Id = address.Id,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    ProvinceId = address.ProvinceId,
                    WardId = address.WardId,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    Phone = address.Phone,
                    Name = address.Name,
                    IsDefault = address.IsDefault,
                    Type = address.Type,
                    Notes = address.Notes,
                    CreatedAt = address.CreatedAt,
                    UpdatedAt = address.UpdatedAt
                };

                return ApiResponse.SuccessResult(addressDto, "Address updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId} for user {UserId}", addressId, userId);
                return ApiResponse.ErrorResult<AddressResponseDto>("Failed to update address");
            }
        }

        public async Task<ApiResponse> DeleteAddressAsync(int addressId, Guid userId)
        {
            try
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && !a.IsDeleted);

                if (address == null)
                    return ApiResponse.ErrorResult("Address not found");

                // Soft delete
                address.IsDeleted = true;
                address.DeletedAt = DateTime.UtcNow;

                // If this was the default address, set another one as default
                if (address.IsDefault)
                {
                    var nextDefault = await _context.Addresses
                        .Where(a => a.UserId == userId && a.Id != addressId && !a.IsDeleted)
                        .OrderBy(a => a.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (nextDefault != null)
                    {
                        nextDefault.IsDefault = true;
                        nextDefault.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Address {AddressId} deleted for user {UserId}", addressId, userId);
                return ApiResponse.SuccessResult("Address deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId} for user {UserId}", addressId, userId);
                return ApiResponse.ErrorResult("Failed to delete address");
            }
        }

        public async Task<ApiResponse> SetDefaultAddressAsync(int addressId, Guid userId)
        {
            try
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && !a.IsDeleted);

                if (address == null)
                    return ApiResponse.ErrorResult("Address not found");

                if (address.IsDefault)
                    return ApiResponse.SuccessResult("Address is already the default");

                // Unset other defaults
                var currentDefaults = await _context.Addresses
                    .Where(a => a.UserId == userId && a.IsDefault && !a.IsDeleted)
                    .ToListAsync();

                foreach (var addr in currentDefaults)
                {
                    addr.IsDefault = false;
                    addr.UpdatedAt = DateTime.UtcNow;
                }

                // Set new default
                address.IsDefault = true;
                address.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Address {AddressId} set as default for user {UserId}", addressId, userId);
                return ApiResponse.SuccessResult("Default address updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address {AddressId} for user {UserId}", addressId, userId);
                return ApiResponse.ErrorResult("Failed to set default address");
            }
        }

        public async Task<ApiResponse<UserStatsDto>> GetUserStatsAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted || !user.IsActive)
                    return ApiResponse.ErrorResult<UserStatsDto>("User not found");

                // Get order statistics
                var orders = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .ToListAsync();

                var stats = new UserStatsDto
                {
                    TotalOrders = user.TotalOrders,
                    TotalSpent = user.TotalSpent,
                    Points = user.Points,
                    Tier = user.Tier,
                    ReviewsCount = await _context.Reviews.CountAsync(r => r.UserId == userId),
                    WishlistItemsCount = await _context.WishlistItems.CountAsync(w => w.Wishlist.UserId == userId),
                    AddressesCount = await _context.Addresses.CountAsync(a => a.UserId == userId && !a.IsDeleted),
                    LastOrderDate = orders.OrderByDescending(o => o.CreatedAt).FirstOrDefault()?.CreatedAt,
                    LastLoginDate = user.LastLoginAt,
                    DaysSinceRegistration = (DateTime.UtcNow - user.CreatedAt).Days,
                    AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
                    CancelledOrdersCount = orders.Count(o => o.Status == OrderStatus.Cancelled),
                    ReturnedOrdersCount = orders.Count(o => o.Status == OrderStatus.Returned),
                    AverageRatingGiven = await _context.Reviews
                        .Where(r => r.UserId == userId)
                        .AverageAsync(r => (double?)r.Rating) ?? 0,
                    IsVipCustomer = user.Tier >= UserTier.Gold,
                    PreferredCategories = "Food & Beverages, Beauty & Health", // TODO: Calculate from orders
                    PreferredBrands = "Pocky, Shiseido" // TODO: Calculate from orders
                };

                return ApiResponse.SuccessResult(stats, "User statistics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats for user {UserId}", userId);
                return ApiResponse.ErrorResult<UserStatsDto>("Failed to retrieve user statistics");
            }
        }

        //public async Task<ApiResponse<UserListResponseDto>> GetUserListAsync(UserFilterRequestDto filter)
        //{
        //    try
        //    {
        //        var query = _context.Users.AsQueryable();

        //        // Filter: search
        //        if (!string.IsNullOrWhiteSpace(filter.Search))
        //        {
        //            var search = filter.Search.ToLower();
        //            query = query.Where(u => u.UserName.ToLower().Contains(search)
        //                || u.Email.ToLower().Contains(search)
        //                || u.FirstName.ToLower().Contains(search)
        //                || u.LastName.ToLower().Contains(search));
        //        }
        //        // Filter: role
        //        if (filter.Role.HasValue)
        //            query = query.Where(u => u.Role == filter.Role.Value);
        //        // Filter: status
        //        if (filter.Status.HasValue)
        //            query = query.Where(u => u.Status == filter.Status.Value);
        //        // Filter: tier
        //        if (filter.Tier.HasValue)
        //            query = query.Where(u => u.Tier == filter.Tier.Value);
        //        // Filter: isActive
        //        if (filter.IsActive.HasValue)
        //            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        //        // Filter: email verified
        //        if (filter.EmailVerified.HasValue)
        //            query = query.Where(u => u.EmailVerified == filter.EmailVerified.Value);
        //        // Filter: phone verified
        //        if (filter.PhoneVerified.HasValue)
        //            query = query.Where(u => u.PhoneVerified == filter.PhoneVerified.Value);
        //        // Filter: provider
        //        if (filter.Provider.HasValue)
        //            query = query.Where(u => u.Provider == filter.Provider.Value);
        //        // Filter: created date
        //        if (filter.CreatedFrom.HasValue)
        //            query = query.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);
        //        if (filter.CreatedTo.HasValue)
        //            query = query.Where(u => u.CreatedAt <= filter.CreatedTo.Value);
        //        // Filter: last login
        //        if (filter.LastLoginFrom.HasValue)
        //            query = query.Where(u => u.LastLoginAt >= filter.LastLoginFrom.Value);
        //        if (filter.LastLoginTo.HasValue)
        //            query = query.Where(u => u.LastLoginAt <= filter.LastLoginTo.Value);
        //        // Filter: total spent
        //        if (filter.MinTotalSpent.HasValue)
        //            query = query.Where(u => u.TotalSpent >= filter.MinTotalSpent.Value);
        //        if (filter.MaxTotalSpent.HasValue)
        //            query = query.Where(u => u.TotalSpent <= filter.MaxTotalSpent.Value);
        //        // Filter: total orders
        //        if (filter.MinTotalOrders.HasValue)
        //            query = query.Where(u => u.TotalOrders >= filter.MinTotalOrders.Value);
        //        if (filter.MaxTotalOrders.HasValue)
        //            query = query.Where(u => u.TotalOrders <= filter.MaxTotalOrders.Value);

        //        // Sorting
        //        switch (filter.SortBy?.ToLower())
        //        {
        //            case "name":
        //                query = filter.SortOrder == "asc" ? query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName) : query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName);
        //                break;
        //            case "email":
        //                query = filter.SortOrder == "asc" ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email);
        //                break;
        //            case "created":
        //                query = filter.SortOrder == "asc" ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt);
        //                break;
        //            case "lastlogin":
        //                query = filter.SortOrder == "asc" ? query.OrderBy(u => u.LastLoginAt) : query.OrderByDescending(u => u.LastLoginAt);
        //                break;
        //            case "totalspent":
        //                query = filter.SortOrder == "asc" ? query.OrderBy(u => u.TotalSpent) : query.OrderByDescending(u => u.TotalSpent);
        //                break;
        //            case "totalorders":
        //                query = filter.SortOrder == "asc" ? query.OrderBy(u => u.TotalOrders) : query.OrderByDescending(u => u.TotalOrders);
        //                break;
        //            default:
        //                query = query.OrderByDescending(u => u.CreatedAt);
        //                break;
        //        }

        //        // Pagination
        //        int page = filter.Page > 0 ? filter.Page : 1;
        //        int pageSize = filter.PageSize > 0 && filter.PageSize <= 100 ? filter.PageSize : 20;
        //        int skip = (page - 1) * pageSize;
        //        int totalItems = await query.CountAsync();
        //        var users = await query.Skip(skip).Take(pageSize).ToListAsync();

        //        // Mapping
        //        var userDtos = users.Select(u => new DTOs.Users.UserSummaryDto
        //        {
        //            Id = u.Id,
        //            UserName = u.UserName,
        //            Email = u.Email,
        //            FirstName = u.FirstName,
        //            LastName = u.LastName,
        //            FullName = $"{u.FirstName} {u.LastName}".Trim(),
        //            Avatar = u.Avatar,
        //            Role = u.Role,
        //            Status = u.Status,
        //            Tier = u.Tier,
        //            IsActive = u.IsActive,
        //            CreatedAt = u.CreatedAt,
        //            LastLoginAt = u.LastLoginAt
        //        }).ToList();

        //        // Statistics (simple)
        //        var statistics = new DTOs.Users.UserStatisticsDto
        //        {
        //            TotalUsers = totalItems,
        //            ActiveUsers = await _context.Users.CountAsync(u => u.IsActive && !u.IsDeleted),
        //            NewUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)),
        //            AvgTotalSpent = totalItems > 0 ? (double)users.Average(u => u.TotalSpent) : 0,
        //            AvgTotalOrders = totalItems > 0 ? users.Average(u => u.TotalOrders) : 0,
        //            RoleCounts = await _context.Users.GroupBy(u => u.Role).Select(g => new DTOs.Users.UserRoleCountDto { Role = g.Key, Count = g.Count() }).ToListAsync(),
        //            TierCounts = await _context.Users.GroupBy(u => u.Tier).Select(g => new DTOs.Users.UserTierCountDto { Tier = g.Key, Count = g.Count() }).ToListAsync(),
        //            StatusCounts = await _context.Users.GroupBy(u => u.Status).Select(g => new DTOs.Users.UserStatusCountDto { Status = g.Key, Count = g.Count() }).ToListAsync()
        //        };

        //        var response = new DTOs.Users.UserListResponseDto
        //        {
        //            Items = userDtos,
        //            Page = page,
        //            PageSize = pageSize,
        //            TotalItems = totalItems,
        //            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
        //            Filters = new DTOs.Users.UserFilterInfoDto
        //            {
        //                Search = filter.Search,
        //                Role = filter.Role,
        //                Status = filter.Status,
        //                Tier = filter.Tier,
        //                IsActive = filter.IsActive,
        //                SortBy = filter.SortBy,
        //                SortOrder = filter.SortOrder,
        //                TotalFiltersApplied = new[] { filter.Search, filter.Role, filter.Status, filter.Tier, filter.IsActive }.Count(x => x != null)
        //            },
        //            Statistics = statistics
        //        };

        //        return ApiResponse.SuccessResult(response, "User list retrieved successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting user list");
        //        return ApiResponse.ErrorResult<UserListResponseDto>("Failed to retrieve user list");
        //    }
        //}
    }
}