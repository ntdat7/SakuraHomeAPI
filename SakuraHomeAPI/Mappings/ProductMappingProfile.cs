using AutoMapper;
using SakuraHomeAPI.DTOs.Products.Responses;
using SakuraHomeAPI.DTOs.Products.Components;
using ProductRequests = SakuraHomeAPI.DTOs.Products.Requests;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Models.Entities.Catalog;
using SakuraHomeAPI.Models.Entities.Reviews;

namespace SakuraHomeAPI.Mappings
{
    /// <summary>
    /// AutoMapper profile for Product-related mappings
    /// </summary>
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            // Product mappings
            CreateMap<Product, ProductSummaryDto>()
                .ForMember(dest => dest.IsInStock, opt => opt.MapFrom(src => src.Stock > 0 || src.AllowBackorder))
                .ForMember(dest => dest.IsOnSale, opt => opt.MapFrom(src => src.OriginalPrice.HasValue && src.OriginalPrice > src.Price))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.ProductImages.Where(pi => pi.IsActive).OrderBy(pi => pi.DisplayOrder).Take(3)));

            CreateMap<Product, ProductDetailDto>()
                .IncludeBase<Product, ProductSummaryDto>()
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => 
                    src.Status == Models.Enums.ProductStatus.Active &&
                    (!src.AvailableFrom.HasValue || src.AvailableFrom <= DateTime.UtcNow) &&
                    (!src.AvailableUntil.HasValue || src.AvailableUntil >= DateTime.UtcNow) &&
                    !src.IsDeleted &&
                    src.IsActive))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.ProductImages.Where(pi => pi.IsActive).OrderBy(pi => pi.DisplayOrder)))
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants.Where(v => v.IsActive && !v.IsDeleted)))
                .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => src.AttributeValues))
                .ForMember(dest => dest.ProductTags, opt => opt.MapFrom(src => src.ProductTags.Select(pt => pt.Tag)))
                .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews.Where(r => r.IsActive && !r.IsDeleted && r.IsApproved).Take(5)));

            // Product creation mappings
            CreateMap<ProductRequests.CreateProductRequestDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => GenerateSlug(src.Name)))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(_ => 0))
                .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(_ => 0))
                .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(_ => 0))
                .ForMember(dest => dest.SoldCount, opt => opt.MapFrom(_ => 0))
                .ForMember(dest => dest.WishlistCount, opt => opt.MapFrom(_ => 0))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false))
                // Ignore navigation properties - will be handled separately
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
                .ForMember(dest => dest.Variants, opt => opt.Ignore())
                .ForMember(dest => dest.AttributeValues, opt => opt.Ignore())
                .ForMember(dest => dest.ProductTags, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore());

            // Product update mappings
            CreateMap<ProductRequests.UpdateProductRequestDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Rating, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewCount, opt => opt.Ignore())
                .ForMember(dest => dest.ViewCount, opt => opt.Ignore())
                .ForMember(dest => dest.SoldCount, opt => opt.Ignore())
                .ForMember(dest => dest.WishlistCount, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                // Ignore navigation properties
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
                .ForMember(dest => dest.Variants, opt => opt.Ignore())
                .ForMember(dest => dest.AttributeValues, opt => opt.Ignore())
                .ForMember(dest => dest.ProductTags, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore());

            // Product Image mappings
            CreateMap<ProductImage, ProductImageDto>();
            CreateMap<CreateProductImageDto, ProductImage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Product, opt => opt.Ignore());

            // Product Variant mappings
            CreateMap<ProductVariant, ProductVariantDto>()
                .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => new Dictionary<string, string>()));
            
            CreateMap<CreateProductVariantDto, ProductVariant>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.Product, opt => opt.Ignore());

            // Product Attribute mappings
            CreateMap<ProductAttributeValue, ProductAttributeDto>()
                .ForMember(dest => dest.AttributeName, opt => opt.MapFrom(src => src.Attribute.Name))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Attribute.Type))
                .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Attribute.Unit));

            CreateMap<CreateProductAttributeDto, ProductAttributeValue>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Attribute, opt => opt.Ignore());

            // Tag mappings
            CreateMap<Tag, TagDto>();

            // Brand mappings
            CreateMap<Brand, BrandSummaryDto>();
            CreateMap<Brand, BrandDetailDto>()
                .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(_ => true)) // Assume all brands are verified
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

            // Inventory mappings
            CreateMap<ProductRequests.UpdateStockRequestDto, InventoryLog>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.ProductVariantId, opt => opt.Ignore())
                .ForMember(dest => dest.Quantity, opt => opt.Ignore()) // Will be calculated
                .ForMember(dest => dest.PreviousStock, opt => opt.Ignore()) // Will be set
                .ForMember(dest => dest.ReferenceType, opt => opt.MapFrom(_ => "Manual"))
                .ForMember(dest => dest.ReferenceId, opt => opt.MapFrom(_ => (int?)null))
                .ForMember(dest => dest.BatchNumber, opt => opt.MapFrom(src => src.BatchNumber ?? string.Empty))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location ?? string.Empty))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Reason ?? string.Empty))
                .ForMember(dest => dest.IpAddress, opt => opt.Ignore()) // Will be set from HttpContext
                .ForMember(dest => dest.UserAgent, opt => opt.Ignore()) // Will be set from HttpContext
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.ProductVariant, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // Filter mapping
            CreateMap<ProductRequests.ProductFilterRequestDto, ProductFilterInfoDto>();
        }

        private static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return text.ToLowerInvariant()
                .Replace(' ', '-')
                .Replace('_', '-')
                .Trim('-');
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