using AutoMapper;
using SakuraHomeAPI.DTOs.Orders.Requests;
using SakuraHomeAPI.DTOs.Orders.Responses;
using SakuraHomeAPI.Models.Entities.Orders;

namespace SakuraHomeAPI.Mappings
{
    /// <summary>
    /// AutoMapper profile for Order entities and DTOs
    /// </summary>
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            // Order mappings
            CreateMap<Order, OrderResponseDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.ReceiverName))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.ReceiverEmail))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.ReceiverPhone))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => "COD")) // Default
                .ForMember(dest => dest.ShippingCost, opt => opt.MapFrom(src => src.ShippingFee))
                .ForMember(dest => dest.ExpressDelivery, opt => opt.MapFrom(src => src.DeliveryMethod == Models.Enums.DeliveryMethod.Express))
                .ForMember(dest => dest.GiftWrap, opt => opt.MapFrom(src => src.GiftWrapRequested))
                .ForMember(dest => dest.OrderNotes, opt => opt.MapFrom(src => src.CustomerNotes))
                .ForMember(dest => dest.CancellationReason, opt => opt.MapFrom(src => src.CancelReason));

            CreateMap<Order, OrderSummaryDto>()
                .ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => GetStatusText(src.Status)))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => "COD")) // Default
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.OrderItems.Count))
                .ForMember(dest => dest.ItemNames, opt => opt.MapFrom(src => src.OrderItems.Select(oi => oi.ProductName).ToList()));

            // Order Item mappings
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductSlug, opt => opt.MapFrom(src => src.Product != null ? src.Product.Slug : ""))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Product != null && src.Product.Brand != null ? src.Product.Brand.Name : ""))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Product != null && src.Product.Category != null ? src.Product.Category.Name : ""))
                .ForMember(dest => dest.CurrentPrice, opt => opt.MapFrom(src => src.Product != null ? src.Product.Price : (decimal?)null))
                .ForMember(dest => dest.IsStillAvailable, opt => opt.MapFrom(src => src.Product != null && src.Product.IsActive && !src.Product.IsDeleted));

            // Order Status History mappings
            CreateMap<OrderStatusHistory, OrderStatusHistoryDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.NewStatus))
                .ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => GetStatusText(src.NewStatus)))
                .ForMember(dest => dest.IsSystemGenerated, opt => opt.MapFrom(src => true));

            // Order Note mappings
            CreateMap<OrderNote, OrderNoteDto>()
                .ForMember(dest => dest.IsCustomerVisible, opt => opt.MapFrom(src => src.IsPublic))
                .ForMember(dest => dest.IsFromCustomer, opt => opt.MapFrom(src => !src.IsSystem));

            // Request mappings (if needed)
            CreateMap<CreateOrderRequestDto, Order>()
                .ForMember(dest => dest.CustomerNotes, opt => opt.MapFrom(src => src.OrderNotes))
                .ForMember(dest => dest.GiftWrapRequested, opt => opt.MapFrom(src => src.GiftWrap))
                .ForMember(dest => dest.DeliveryMethod, opt => opt.MapFrom(src => src.ExpressDelivery ? Models.Enums.DeliveryMethod.Express : Models.Enums.DeliveryMethod.Standard))
                .ForMember(dest => dest.IsGift, opt => opt.MapFrom(src => src.GiftWrap));

            CreateMap<OrderItemRequestDto, OrderItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore());
        }

        /// <summary>
        /// Get status text in Vietnamese
        /// </summary>
        private static string GetStatusText(Models.Enums.OrderStatus status)
        {
            return status switch
            {
                Models.Enums.OrderStatus.Pending => "?ang ch? x? lý",
                Models.Enums.OrderStatus.Confirmed => "?ã xác nh?n",
                Models.Enums.OrderStatus.Processing => "?ang chu?n b? hàng",
                Models.Enums.OrderStatus.Shipped => "?ã giao cho v?n chuy?n",
                Models.Enums.OrderStatus.OutForDelivery => "?ang giao hàng",
                Models.Enums.OrderStatus.Delivered => "?ã giao thành công",
                Models.Enums.OrderStatus.Cancelled => "?ã h?y",
                Models.Enums.OrderStatus.Returned => "?ã tr? hàng",
                Models.Enums.OrderStatus.Refunded => "?ã hoàn ti?n",
                _ => "Không xác ??nh"
            };
        }
    }
}