using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.DTOs.Orders.Requests;
using SakuraHomeAPI.DTOs.Orders.Responses;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.Services.Interfaces
{
    /// <summary>
    /// Order management service interface
    /// </summary>
    public interface IOrderService
    {
        // Order Creation & Management
        Task<ApiResponse<OrderResponseDto>> CreateOrderAsync(CreateOrderRequestDto request, Guid userId);
        Task<ApiResponse<OrderResponseDto>> GetOrderAsync(int orderId, Guid userId);
        Task<ApiResponse<List<OrderSummaryDto>>> GetUserOrdersAsync(Guid userId, OrderFilterDto? filter = null);
        Task<ApiResponse<OrderResponseDto>> UpdateOrderAsync(int orderId, UpdateOrderRequestDto request, Guid userId);
        Task<ApiResponse> CancelOrderAsync(int orderId, string reason, Guid userId);
        
        // Order Status Management
        Task<ApiResponse<OrderResponseDto>> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string? notes = null);
        Task<ApiResponse<List<OrderStatusHistoryDto>>> GetOrderStatusHistoryAsync(int orderId);
        
        // Order Processing (Admin/Staff)
        Task<ApiResponse<OrderResponseDto>> ConfirmOrderAsync(int orderId, Guid staffId);
        Task<ApiResponse<OrderResponseDto>> ProcessOrderAsync(int orderId, ProcessOrderRequestDto request, Guid staffId);
        Task<ApiResponse<OrderResponseDto>> ShipOrderAsync(int orderId, ShipOrderRequestDto request, Guid staffId);
        Task<ApiResponse<OrderResponseDto>> DeliverOrderAsync(int orderId, Guid staffId);
        
        // Order Items
        Task<ApiResponse<OrderResponseDto>> AddOrderItemAsync(int orderId, AddOrderItemRequestDto request, Guid staffId);
        Task<ApiResponse<OrderResponseDto>> UpdateOrderItemAsync(int orderId, int itemId, UpdateOrderItemRequestDto request, Guid staffId);
        Task<ApiResponse<OrderResponseDto>> RemoveOrderItemAsync(int orderId, int itemId, Guid staffId);
        
        // Order Notes
        Task<ApiResponse<OrderResponseDto>> AddOrderNoteAsync(int orderId, string note, bool isCustomerVisible, Guid userId);
        Task<ApiResponse<List<OrderNoteDto>>> GetOrderNotesAsync(int orderId, Guid userId);
        
        // Order Analytics
        Task<ApiResponse<OrderStatsDto>> GetOrderStatsAsync(Guid? userId = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<ApiResponse<List<OrderSummaryDto>>> GetRecentOrdersAsync(int count = 10);
        
        // Order Validation & Business Logic
        Task<ApiResponse<OrderValidationDto>> ValidateOrderAsync(CreateOrderRequestDto request, Guid userId);
       // Task<ApiResponse<decimal>> CalculateOrderTotalAsync(List<OrderItemRequestDto> items, int? shippingAddressId = null, string? couponCode = null);
        
        // Return & Refund
        Task<ApiResponse<OrderResponseDto>> RequestReturnAsync(int orderId, ReturnRequestDto request, Guid userId);
        Task<ApiResponse<OrderResponseDto>> ProcessReturnAsync(int orderId, ProcessReturnRequestDto request, Guid staffId);

        /// <summary>
        /// Customer confirms delivery received (or reports issue)
        /// </summary>
        Task<ApiResponse<OrderResponseDto>> ConfirmDeliveryReceivedAsync(int orderId, Guid userId, bool isReceived, string? notes = null);

        /// <summary>
        /// Pack order - Staff đóng gói xong
        /// </summary>
        Task<ApiResponse<OrderResponseDto>> PackOrderAsync(int orderId, Guid staffId, string? notes = null);

        /// <summary>
        /// Mark order as out for delivery
        /// </summary>
        Task<ApiResponse<OrderResponseDto>> MarkOutForDeliveryAsync(int orderId, Guid staffId, string? notes = null);

        /// <summary>
        /// Mark delivery as failed and restore inventory
        /// </summary>
        Task<ApiResponse<OrderResponseDto>> MarkDeliveryFailedAsync(int orderId, Guid staffId, string failureReason);

    }

}