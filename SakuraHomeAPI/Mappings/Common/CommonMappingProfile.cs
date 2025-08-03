using AutoMapper;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Models.Entities.Catalog;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Models.Entities.Reviews;

namespace SakuraHomeAPI.Mappings.Common
{
    /// <summary>
    /// AutoMapper profile for common/shared mappings
    /// </summary>
    public class CommonMappingProfile : Profile
    {
        public CommonMappingProfile()
        {
            // Tag mappings
            CreateMap<Tag, TagDto>();

            // Brand mappings
            CreateMap<Brand, BrandSummaryDto>();
            CreateMap<Brand, BrandDetailDto>()
                .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count(p => p.IsActive && !p.IsDeleted)))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => 
                    src.Products.Where(p => p.IsActive && !p.IsDeleted && p.ReviewCount > 0).Any() ?
                    src.Products.Where(p => p.IsActive && !p.IsDeleted && p.ReviewCount > 0).Average(p => (double)p.Rating) : 0))
                .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => 
                    src.Products.Where(p => p.IsActive && !p.IsDeleted).Sum(p => p.ReviewCount)));

            // Category mappings
            CreateMap<Category, CategorySummaryDto>();
            CreateMap<Category, CategoryDetailDto>()
                .ForMember(dest => dest.Level, opt => opt.MapFrom(src => CalculateCategoryLevel(src)))
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count(p => p.IsActive && !p.IsDeleted)))
                .ForMember(dest => dest.TotalProductCount, opt => opt.MapFrom(src => CalculateTotalProductCount(src)))
                .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children.Where(c => c.IsActive && !c.IsDeleted).OrderBy(c => c.DisplayOrder)));

            // Review mappings
            CreateMap<Review, ReviewSummaryDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}".Trim()))
                .ForMember(dest => dest.HelpfulCount, opt => opt.MapFrom(src => src.ReviewVotes.Count(rv => rv.IsHelpful)))
                .ForMember(dest => dest.HasImages, opt => opt.MapFrom(src => src.ReviewImages.Any()));
        }

        private static int CalculateCategoryLevel(Category category)
        {
            int level = 0;
            var current = category;
            while (current.ParentId.HasValue)
            {
                level++;
                current = current.Parent;
                if (current == null) break;
            }
            return level;
        }

        private static int CalculateTotalProductCount(Category category)
        {
            var directCount = category.Products.Count(p => p.IsActive && !p.IsDeleted);
            var childrenCount = category.Children
                .Where(c => c.IsActive && !c.IsDeleted)
                .Sum(c => CalculateTotalProductCount(c));
            return directCount + childrenCount;
        }
    }
}