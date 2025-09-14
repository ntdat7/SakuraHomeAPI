using AutoMapper;
using SakuraHomeAPI.DTOs.Users;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Models.Entities;

namespace SakuraHomeAPI.Mappings
{
    /// <summary>
    /// AutoMapper profile for User-related mappings
    /// </summary>
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // User mappings
            CreateMap<User, UserSummaryDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()));

            CreateMap<User, UserProfileDto>()
                .IncludeBase<User, UserSummaryDto>()
                .ForMember(dest => dest.Addresses, opt => opt.MapFrom(src => src.Addresses.Where(a => !a.IsDeleted).OrderByDescending(a => a.IsDefault)));

            // Registration mapping
            CreateMap<RegisterRequestDto, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.PreferredLanguage, opt => opt.MapFrom(src => src.PreferredLanguage))
                .ForMember(dest => dest.EmailNotifications, opt => opt.MapFrom(_ => true)) // Default to true
                .ForMember(dest => dest.Role, opt => opt.MapFrom(_ => Models.Enums.UserRole.Customer))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => Models.Enums.AccountStatus.Pending))
                .ForMember(dest => dest.Tier, opt => opt.MapFrom(_ => Models.Enums.UserTier.Bronze))
                .ForMember(dest => dest.Provider, opt => opt.MapFrom(_ => Models.Enums.LoginProvider.Local))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Points, opt => opt.MapFrom(_ => 0))
                .ForMember(dest => dest.TotalSpent, opt => opt.MapFrom(_ => 0))
                .ForMember(dest => dest.TotalOrders, opt => opt.MapFrom(_ => 0))
                .ForMember(dest => dest.EmailVerified, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.PhoneVerified, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false))
                // Ignore navigation and computed properties
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.Addresses, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore())
                .ForMember(dest => dest.Wishlist, opt => opt.Ignore())
                .ForMember(dest => dest.Carts, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.ContactMessages, opt => opt.Ignore())
                .ForMember(dest => dest.Activities, opt => opt.Ignore())
                .ForMember(dest => dest.ProductViews, opt => opt.Ignore())
                .ForMember(dest => dest.SearchLogs, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedByUser, opt => opt.Ignore());

            // Profile update mapping
            CreateMap<UpdateProfileRequestDto, User>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.PreferredLanguage, opt => opt.MapFrom(src => src.PreferredLanguage))
                .ForMember(dest => dest.PreferredCurrency, opt => opt.MapFrom(src => src.PreferredCurrency))
                .ForMember(dest => dest.EmailNotifications, opt => opt.MapFrom(src => src.EmailNotifications))
                .ForMember(dest => dest.SmsNotifications, opt => opt.MapFrom(src => src.SmsNotifications))
                .ForMember(dest => dest.PushNotifications, opt => opt.MapFrom(src => src.PushNotifications))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                // Ignore all other properties
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Address mappings
            CreateMap<Address, AddressSummaryDto>();

            CreateMap<CreateAddressRequestDto, Address>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<UpdateAddressRequestDto, Address>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // User Activity mappings
            CreateMap<UserActivity, UserActivityDto>()
                .ForMember(dest => dest.RelatedEntityId, opt => opt.MapFrom(src => src.RelatedEntityId))
                .ForMember(dest => dest.RelatedEntityType, opt => opt.MapFrom(src => src.RelatedEntityType));

            // Login response mapping
            CreateMap<User, LoginResponseDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.AccessToken, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore());
        }
    }
}