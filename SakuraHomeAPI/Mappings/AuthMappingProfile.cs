using AutoMapper;
using SakuraHomeAPI.DTOs.Users;
using SakuraHomeAPI.Models.Entities.Identity;

namespace SakuraHomeAPI.Mappings
{
    /// <summary>
    /// AutoMapper profile for authentication-related mappings
    /// </summary>
    public class AuthMappingProfile : Profile
    {
        public AuthMappingProfile()
        {
            // User to UserDto mapping
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

            // RegisterRequestDto to User mapping
            CreateMap<RegisterRequestDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}