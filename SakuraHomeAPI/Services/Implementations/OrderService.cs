using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.DTOs.Notifications.Requests;
using SakuraHomeAPI.DTOs.Orders.Requests;
using SakuraHomeAPI.DTOs.Orders.Responses;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Models.Entities.Orders;
using SakuraHomeAPI.Models.Entities.UserCart;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.Services.Interfaces;
using System.Linq;

namespace SakuraHomeAPI.Services.Implementations
{
    /// <summary>
    /// Order management service implementation with notification integration
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICartService _cartService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public OrderService(
            ApplicationDbContext context,
            IMapper mapper,
            ICartService cartService,
            INotificationService notificationService,
            IEmailService emailService,
            ILogger<OrderService> logger,
            IServiceProvider serviceProvider)
        {
            _context = context;
            _mapper = mapper;
            _cartService = cartService;
            _notificationService = notificationService;
            _emailService = emailService;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<ApiResponse<OrderResponseDto>> CreateOrderAsync(CreateOrderRequestDto request, Guid userId)
        {
            try
            {
                _logger.LogInformation("Creating order for user {UserId} with {ItemCount} items",
                    userId, request.Items?.Count ?? 0);

                // Input validation
                if (request.Items == null || !request.Items.Any())
                {
                    _logger.LogWarning("Order creation failed: No items provided for user {UserId}", userId);
                    return ApiResponse.ErrorResult<OrderResponseDto>("No items provided for order");
                }

                if (request.ShippingAddressId <= 0)
                {
                    _logger.LogWarning("Order creation failed: Invalid shipping address ID {AddressId} for user {UserId}",
                        request.ShippingAddressId, userId);
                    return ApiResponse.ErrorResult<OrderResponseDto>("Invalid shipping address");
                }

                if (string.IsNullOrEmpty(request.PaymentMethod))
                {
                    _logger.LogWarning("Order creation failed: No payment method provided for user {UserId}", userId);
                    return ApiResponse.ErrorResult<OrderResponseDto>("Payment method is required");
                }

                // Use execution strategy WITHOUT nested transaction
                var executionStrategy = _context.Database.CreateExecutionStrategy();

                return await executionStrategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        // 1. Validate user exists
                        var user = await _context.Users.FindAsync(userId);
                        if (user == null)
                        {
                            _logger.LogWarning("Order creation failed: User {UserId} not found", userId);
                            return ApiResponse.ErrorResult<OrderResponseDto>("User not found");
                        }

                        // 2. Validate shipping address
                        var shippingAddress = await _context.Addresses
                            .FirstOrDefaultAsync(a => a.Id == request.ShippingAddressId && a.UserId == userId);

                        if (shippingAddress == null)
                        {
                            _logger.LogWarning("Order creation failed: Shipping address {AddressId} not found for user {UserId}",
                                request.ShippingAddressId, userId);
                            return ApiResponse.ErrorResult<OrderResponseDto>("Shipping address not found or does not belong to you");
                        }

                        // 3. Validate billing address (if different)
                        Address billingAddress = shippingAddress;
                        if (request.BillingAddressId.HasValue && request.BillingAddressId != request.ShippingAddressId)
                        {
                            billingAddress = await _context.Addresses
                                .FirstOrDefaultAsync(a => a.Id == request.BillingAddressId && a.UserId == userId);

                            if (billingAddress == null)
                            {
                                _logger.LogWarning("Order creation failed: Billing address {AddressId} not found for user {UserId}",
                                    request.BillingAddressId, userId);
                                return ApiResponse.ErrorResult<OrderResponseDto>("Billing address not found or does not belong to you");
                            }
                        }

                        // 4. Validate and process order items
                        var orderItemsData = new List<(Product Product, int Quantity, string CustomOptions)>();
                        decimal subtotal = 0;

                        foreach (var requestItem in request.Items)
                        {
                            if (requestItem.ProductId <= 0)
                            {
                                _logger.LogWarning("Order creation failed: Invalid product ID {ProductId}", requestItem.ProductId);
                                return ApiResponse.ErrorResult<OrderResponseDto>($"Invalid product ID: {requestItem.ProductId}");
                            }

                            if (requestItem.Quantity <= 0)
                            {
                                _logger.LogWarning("Order creation failed: Invalid quantity {Quantity} for product {ProductId}",
                                    requestItem.Quantity, requestItem.ProductId);
                                return ApiResponse.ErrorResult<OrderResponseDto>($"Invalid quantity: {requestItem.Quantity}");
                            }

                            // Validate product exists and is available
                            var product = await _context.Products
                                .Include(p => p.Brand)
                                .Include(p => p.Category)
                                .FirstOrDefaultAsync(p => p.Id == requestItem.ProductId);

                            if (product == null)
                            {
                                _logger.LogWarning("Order creation failed: Product {ProductId} not found", requestItem.ProductId);
                                return ApiResponse.ErrorResult<OrderResponseDto>($"Product with ID {requestItem.ProductId} not found");
                            }

                            // Check if product is active/available
                            if (product.Status != ProductStatus.Active)
                            {
                                _logger.LogWarning("Order creation failed: Product {ProductId} is not active. Status: {Status}",
                                    requestItem.ProductId, product.Status);
                                return ApiResponse.ErrorResult<OrderResponseDto>($"Product {product.Name} is not available for purchase");
                            }

                            // Check stock availability
                            if (product.Stock < requestItem.Quantity)
                            {
                                _logger.LogWarning("Order creation failed: Insufficient stock for product {ProductId}. Available: {Stock}, Requested: {Quantity}",
                                    requestItem.ProductId, product.Stock, requestItem.Quantity);
                                return ApiResponse.ErrorResult<OrderResponseDto>(
                                    $"Insufficient stock for {product.Name}. Available: {product.Stock}, Requested: {requestItem.Quantity}");
                            }

                            // Add to order items
                            string customOptions = requestItem.CustomOptions ?? "{}";
                            orderItemsData.Add((product, requestItem.Quantity, customOptions));
                            subtotal += product.Price * requestItem.Quantity;
                        }

                        if (subtotal <= 0)
                        {
                            _logger.LogWarning("Order creation failed: Order subtotal is zero or negative for user {UserId}", userId);
                            return ApiResponse.ErrorResult<OrderResponseDto>("Order total cannot be zero");
                        }

                        // 5. Calculate costs - FIXED: Pass express delivery parameter
                        var isExpressDelivery = request.ExpressDelivery == true;
                        var shippingFee = await CalculateShippingCostForOrderAsync(request.ShippingAddressId, orderItemsData, isExpressDelivery);
                        var taxAmount = 0m;
                        var discountAmount = 0m;

                        // Apply coupon if provided
                        if (!string.IsNullOrEmpty(request.CouponCode))
                        {
                            try
                            {
                                discountAmount = await CalculateCouponDiscountAsync(request.CouponCode, subtotal);
                                _logger.LogInformation("Applied coupon {CouponCode} with discount {Discount} for user {UserId}",
                                    request.CouponCode, discountAmount, userId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to apply coupon {CouponCode} for user {UserId}",
                                    request.CouponCode, userId);
                                // Continue without coupon rather than failing the entire order
                            }
                        }

                        var totalAmount = subtotal + shippingFee + taxAmount - discountAmount;

                        if (totalAmount <= 0)
                        {
                            _logger.LogWarning("Order creation failed: Final total is zero or negative for user {UserId}. Subtotal: {Subtotal}, Shipping: {Shipping}, Discount: {Discount}",
                                userId, subtotal, shippingFee, discountAmount);
                            return ApiResponse.ErrorResult<OrderResponseDto>("Order total cannot be zero or negative");
                        }

                        // 6. Generate unique order number
                        string orderNumber;
                        try
                        {
                            orderNumber = await GenerateUniqueOrderNumberAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to generate order number for user {UserId}", userId);
                            return ApiResponse.ErrorResult<OrderResponseDto>("Failed to generate order number");
                        }

                        // 7. Create order entity - FIXED: Use async FormatAddressAsync
                        var order = new Order
                        {
                            UserId = userId,
                            OrderNumber = orderNumber,
                            Status = OrderStatus.Pending,

                            // Address information - FIXED: Use async address formatting
                            ReceiverName = shippingAddress.Name ?? user.FullName ?? "Unknown",
                            ReceiverPhone = shippingAddress.Phone ?? user.PhoneNumber ?? "",
                            ReceiverEmail = user.Email ?? "",
                            ShippingAddress = await FormatAddressAsync(shippingAddress),
                            BillingAddress = await FormatAddressAsync(billingAddress),

                            // Financial Information
                            SubTotal = subtotal,
                            ShippingFee = shippingFee,
                            TaxAmount = taxAmount,
                            DiscountAmount = discountAmount,
                            TotalAmount = totalAmount,
                            Currency = "VND",

                            // Payment Information
                            PaymentStatus = PaymentStatus.Pending,

                            // Order Details
                            CustomerNotes = request.OrderNotes?.Trim(),
                            CouponCode = string.IsNullOrEmpty(request.CouponCode) ? null : request.CouponCode.Trim(),

                            // FIXED: Handle nullable bool properly
                            IsGift = request.GiftWrap == true,
                            GiftMessage = request.GiftMessage?.Trim(),
                            GiftWrapRequested = request.GiftWrap == true,

                            // FIXED: Handle nullable bool properly
                            DeliveryMethod = request.ExpressDelivery == true ? DeliveryMethod.Express : DeliveryMethod.Standard,

                            OrderDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.Orders.Add(order);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Order entity created with ID {OrderId} for user {UserId}", order.Id, userId);

                        // 8. Create order items
                        foreach (var (product, quantity, customOptions) in orderItemsData)
                        {
                            var orderItem = new OrderItem
                            {
                                OrderId = order.Id,
                                ProductId = product.Id,
                                ProductVariantId = null,

                                // Product snapshot
                                ProductName = product.Name ?? "Unknown Product",
                                ProductSku = product.SKU ?? "",
                                ProductImage = product.MainImage ?? "",
                                VariantName = "",
                                VariantSku = "",
                                ProductAttributes = customOptions,

                                Quantity = quantity,
                                UnitPrice = product.Price,
                                TotalPrice = product.Price * quantity
                            };

                            _context.OrderItems.Add(orderItem);
                        }

                        // 9. Create initial status history
                        var statusHistory = new OrderStatusHistory
                        {
                            OrderId = order.Id,
                            OldStatus = OrderStatus.Pending,
                            NewStatus = OrderStatus.Pending,
                            Notes = "Order created successfully",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.OrderStatusHistory.Add(statusHistory);

                        // 10. Update product stock
                        foreach (var (product, quantity, _) in orderItemsData)
                        {
                            product.Stock = Math.Max(0, product.Stock - quantity);
                            if (product.Stock == 0)
                            {
                                product.Status = ProductStatus.OutOfStock;
                            }
                            product.UpdatedAt = DateTime.UtcNow;
                        }

                        // 11. Update user statistics
                        user.TotalOrders++;
                        user.TotalSpent += order.TotalAmount;

                        // Safe tier update - User.UpdateTier() method exists
                        try
                        {
                            user.UpdateTier();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to update user tier for user {UserId}", userId);
                            // Continue - this is not critical
                        }

                        // 12. Remove ordered items from cart
                        try
                        {
                            await RemoveOrderedItemsFromCartAsync(userId, request.Items);
                            _logger.LogInformation("Removed ordered items from cart for user {UserId}", userId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to remove items from cart for user {UserId}", userId);
                            // Continue - cart cleanup is not critical for order success
                        }

                        // 13. Save all changes
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("Order {OrderNumber} created successfully for user {UserId} with total {Total}",
                            order.OrderNumber, userId, totalAmount);

                        // 14. Send notifications (fire and forget - outside transaction)
                        //var orderNumber = order.OrderNumber;
                        var receiverName = order.ReceiverName;
                        //var totalAmount = order.TotalAmount;
                        var itemCount = order.OrderItems.Count;

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                using var scope = _serviceProvider.CreateScope();
                                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                                // Notification cho customer
                                await notificationService.SendOrderConfirmationNotificationAsync(order.Id);

                                // Notification cho admin
                                await SendNotificationToAllAdminsWithScopeAsync(
                                    scope.ServiceProvider,
                                    "📦 Đơn hàng mới",
                                    $"Khách hàng {receiverName} vừa đặt đơn hàng #{orderNumber}. " +
                                    $"Tổng tiền: {totalAmount:N0} VND. Số lượng sản phẩm: {itemCount}",
                                    NotificationType.Order,
                                    order.Id);

                                _logger.LogInformation("✅ Order notifications sent for order {OrderNumber}", orderNumber);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "⚠️ Failed to send order notifications for order {OrderNumber}", orderNumber);
                            }
                        });

                        // 15. Return order details
                        var orderDto = await MapOrderToDetailDto(order);
                        return ApiResponse.SuccessResult(orderDto, "Order created successfully");
                    }
                    catch (DbUpdateException dbEx)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(dbEx, "Database error creating order for user {UserId}: {Message}",
                            userId, dbEx.Message);

                        if (dbEx.InnerException != null)
                        {
                            _logger.LogError("Inner exception: {InnerMessage}", dbEx.InnerException.Message);
                        }

                        return ApiResponse.ErrorResult<OrderResponseDto>(
                            "Database error occurred while creating order. Please try again.");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Unexpected error creating order for user {UserId}: {Message}",
                            userId, ex.Message);

                        if (ex.InnerException != null)
                        {
                            _logger.LogError("Inner exception: {InnerMessage}", ex.InnerException.Message);
                        }

                        throw; // Re-throw to be caught by outer catch
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error creating order for user {UserId}: {Message}", userId, ex.Message);

                // Return more specific error messages based on exception type
                if (ex is ArgumentException || ex is InvalidOperationException)
                {
                    return ApiResponse.ErrorResult<OrderResponseDto>($"Invalid request: {ex.Message}");
                }

                if (ex is UnauthorizedAccessException)
                {
                    return ApiResponse.ErrorResult<OrderResponseDto>("Access denied");
                }

                if (ex is TimeoutException)
                {
                    return ApiResponse.ErrorResult<OrderResponseDto>("Request timeout. Please try again.");
                }

                return ApiResponse.ErrorResult<OrderResponseDto>("An unexpected error occurred while creating the order. Please try again or contact support.");
            }
        }

        public async Task<ApiResponse<OrderResponseDto>> GetOrderAsync(int orderId, Guid userId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.StatusHistory)
                    .Include(o => o.OrderNotes)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null)
                    return ApiResponse.ErrorResult<OrderResponseDto>("Order not found");

                var orderDto = await MapOrderToDetailDto(order);
                return ApiResponse.SuccessResult(orderDto, "Order retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId} for user {UserId}", orderId, userId);
                return ApiResponse.ErrorResult<OrderResponseDto>("Failed to retrieve order");
            }
        }

        public async Task<ApiResponse<List<OrderSummaryDto>>> GetUserOrdersAsync(Guid userId, OrderFilterDto? filter = null)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Where(o => o.UserId == userId);

                // Apply filters
                if (filter != null)
                {
                    if (filter.Status.HasValue)
                        query = query.Where(o => o.Status == filter.Status.Value);

                    if (filter.FromDate.HasValue)
                        query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

                    if (filter.ToDate.HasValue)
                        query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);

                    if (filter.MinAmount.HasValue)
                        query = query.Where(o => o.TotalAmount >= filter.MinAmount.Value);

                    if (filter.MaxAmount.HasValue)
                        query = query.Where(o => o.TotalAmount <= filter.MaxAmount.Value);

                    if (!string.IsNullOrEmpty(filter.SearchTerm))
                        query = query.Where(o => o.OrderNumber.Contains(filter.SearchTerm) ||
                                               o.ReceiverName.Contains(filter.SearchTerm));
                }

                // Pagination
                var totalCount = await query.CountAsync();
                var page = filter?.Page ?? 1;
                var pageSize = filter?.PageSize ?? 20;

                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var orderSummaries = orders.Select(MapOrderToSummaryDto).ToList();

                return ApiResponse.SuccessResult(orderSummaries, "Orders retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for user {UserId}", userId);
                return ApiResponse.ErrorResult<List<OrderSummaryDto>>("Failed to retrieve orders");
            }
        }

        public async Task<ApiResponse<OrderResponseDto>> UpdateOrderAsync(int orderId, UpdateOrderRequestDto request, Guid userId)
        {
            try
            {
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null)
                    return ApiResponse.ErrorResult<OrderResponseDto>("Order not found");

                // Only allow updates for pending orders
                if (order.Status != OrderStatus.Pending)
                    return ApiResponse.ErrorResult<OrderResponseDto>("Order cannot be modified after confirmation");

                // Update order details
                if (!string.IsNullOrEmpty(request.OrderNotes))
                    order.CustomerNotes = request.OrderNotes;

                if (request.ShippingAddressId.HasValue)
                {
                    var address = await _context.Addresses
                        .FirstOrDefaultAsync(a => a.Id == request.ShippingAddressId && a.UserId == userId);

                    if (address != null)
                    {
                        order.ReceiverName = address.Name;
                        order.ReceiverPhone = address.Phone;

                        // FIXED: Use async FormatAddressAsync instead of manual string building
                        order.ShippingAddress = await FormatAddressAsync(address);

                        // Recalculate shipping fee if delivery method changed
                        if (request.ExpressDelivery.HasValue)
                        {
                            // Get current order items for shipping calculation
                            var orderItems = await _context.OrderItems
                                .Include(oi => oi.Product)
                                .Where(oi => oi.OrderId == orderId)
                                .ToListAsync();

                            var orderItemsData = orderItems.Select(oi => (oi.Product, oi.Quantity, oi.ProductAttributes ?? "{}")).ToList();
                            var newShippingFee = await CalculateShippingCostForOrderAsync(address.Id, orderItemsData, request.ExpressDelivery.Value);

                            // Update shipping fee and total
                            var shippingDiff = newShippingFee - order.ShippingFee;
                            order.ShippingFee = newShippingFee;
                            order.TotalAmount += shippingDiff;
                        }
                    }
                }

                if (request.ExpressDelivery.HasValue)
                    order.DeliveryMethod = request.ExpressDelivery.Value ? DeliveryMethod.Express : DeliveryMethod.Standard;

                if (request.GiftWrap.HasValue)
                {
                    order.IsGift = request.GiftWrap.Value;
                    order.GiftWrapRequested = request.GiftWrap.Value;
                }

                if (!string.IsNullOrEmpty(request.GiftMessage))
                    order.GiftMessage = request.GiftMessage;

                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var orderDto = await MapOrderToDetailDto(order);
                return ApiResponse.SuccessResult(orderDto, "Order updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} for user {UserId}", orderId, userId);
                return ApiResponse.ErrorResult<OrderResponseDto>("Failed to update order");
            }
        }

        public async Task<ApiResponse> CancelOrderAsync(int orderId, string reason, Guid userId)
        {
            _logger.LogInformation("User {UserId} attempting to cancel order {OrderId}", userId, orderId);

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var order = await _context.Orders
                            .Include(o => o.OrderItems) 
                            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                        if (order == null)
                        {
                            _logger.LogWarning("Order {OrderId} not found for user {UserId}", orderId, userId);
                            return ApiResponse.ErrorResult("Không tìm thấy đơn hàng");
                        }

                        if (order.Status != OrderStatus.Pending)
                        {
                            _logger.LogWarning("Order {OrderId} cannot be cancelled. Current status: {Status}", orderId, order.Status);
                            return ApiResponse.ErrorResult(
                                $"❌ Không thể hủy đơn hàng đã được xác nhận. " +
                                $"Trạng thái hiện tại: {GetStatusText(order.Status)}. " +
                                "Vui lòng liên hệ với bộ phận hỗ trợ để được hỗ trợ.");
                        }

                        foreach (var item in order.OrderItems)
                        {
                            var product = await _context.Products.FindAsync(item.ProductId);
                            if (product != null && product.Status != ProductStatus.Discontinued)
                            {
                                product.Stock += item.Quantity;
                                if (product.Stock > 0 && product.Status == ProductStatus.OutOfStock)
                                {
                                    product.Status = ProductStatus.Active;
                                }
                            }
                        }

                        var oldStatus = order.Status;
                        order.Status = OrderStatus.Cancelled;
                        order.CancelledDate = DateTime.UtcNow;
                        order.CancelReason = reason;
                        order.UpdatedAt = DateTime.UtcNow;

                        var statusHistory = new OrderStatusHistory
                        {
                            OrderId = order.Id,
                            OldStatus = oldStatus,
                            NewStatus = OrderStatus.Cancelled,
                            Notes = $"Đơn hàng bị hủy bởi khách hàng. Lý do: {reason}",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.OrderStatusHistory.Add(statusHistory);

                        var user = await _context.Users.FindAsync(userId);
                        if (user != null)
                        {
                            user.TotalOrders = Math.Max(0, user.TotalOrders - 1);
                            user.TotalSpent = Math.Max(0, user.TotalSpent - order.TotalAmount);
                            user.UpdateTier();
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("✅ Order {OrderNumber} cancelled successfully by user {UserId}. Reason: {Reason}",
                            order.OrderNumber, userId, reason);

                        var orderNumber = order.OrderNumber;
                        var receiverName = order.ReceiverName;

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                using var scope = _serviceProvider.CreateScope();
                                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                                await notificationService.SendOrderStatusNotificationAsync(
                                    orderId, OrderStatus.Cancelled,
                                    $"Đơn hàng #{orderNumber} đã bị hủy. Lý do: {reason}");

                                await SendNotificationToAllAdminsWithScopeAsync(
                                    scope.ServiceProvider,
                                    "🚨 Đơn hàng bị hủy",
                                    $"Khách hàng {receiverName} đã hủy đơn hàng #{orderNumber}. Lý do: {reason}.",
                                    NotificationType.Order, orderId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "⚠️ Failed to send cancellation notifications for order {OrderNumber}", orderNumber);
                            }
                        });

                        return ApiResponse.SuccessResult($"✅ Đã hủy đơn hàng thành công. Lý do: {reason}");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error during order cancellation transaction for order {OrderId}", orderId);                        
                        throw;
                    }
                }
            });
        }

        public async Task<ApiResponse<OrderResponseDto>> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string? notes = null)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return ApiResponse.ErrorResult<OrderResponseDto>("Order not found");

                var oldStatus = order.Status;
                order.Status = newStatus;
                order.UpdatedAt = DateTime.UtcNow;

                // Update status-specific fields
                switch (newStatus)
                {
                    case OrderStatus.Confirmed:
                        order.ConfirmedDate = DateTime.UtcNow;
                        if (order.PaymentStatus == PaymentStatus.Pending)
                            order.PaymentStatus = PaymentStatus.Confirmed;
                        break;
                    case OrderStatus.Processing:
                        order.ProcessingDate = DateTime.UtcNow;
                        break;
                    case OrderStatus.Packed:
                        order.PackedDate = DateTime.UtcNow;
                        break;
                    case OrderStatus.Shipped:
                        order.ShippedDate = DateTime.UtcNow;
                        break;
                    case OrderStatus.Delivered:
                        order.DeliveredDate = DateTime.UtcNow;
                        order.PaymentStatus = PaymentStatus.Paid;
                        break;
                }

                // Add status history
                var statusHistory = new OrderStatusHistory
                {
                    OrderId = order.Id,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    Notes = notes ?? $"Status changed from {oldStatus} to {newStatus}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.OrderStatusHistory.Add(statusHistory);

                await _context.SaveChangesAsync();

                // Send notifications for status change
                var orderNumber = order.OrderNumber;
                var receiverName = order.ReceiverName;
                var totalAmount = order.TotalAmount;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        var notificationMessage = GetNotificationMessageForStatus(newStatus, notes);

                        await notificationService.SendOrderStatusNotificationAsync(orderId, newStatus, notificationMessage);

                        if (ShouldNotifyAdminForStatus(newStatus))
                        {
                            var adminTitle = GetAdminNotificationTitle(newStatus);
                            var adminMessage = $"Đơn hàng #{orderNumber} - {receiverName} - " +
                                $"{GetStatusText(newStatus)}. Tổng tiền: {totalAmount:N0} VND";

                            await SendNotificationToAllAdminsWithScopeAsync(
                                scope.ServiceProvider,
                                adminTitle, adminMessage,
                                NotificationType.Order, orderId);
                        }

                        _logger.LogInformation("✅ Notifications sent for order {OrderNumber} - Status: {OldStatus} → {NewStatus}",
                            orderNumber, oldStatus, newStatus);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Failed to send status update notifications for order {OrderNumber}", orderNumber);
                    }
                });

                var orderDto = await MapOrderToDetailDto(order);
                return ApiResponse.SuccessResult(orderDto, "Order status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for order {OrderId}", orderId);
                return ApiResponse.ErrorResult<OrderResponseDto>("Failed to update order status");
            }
        }

        public async Task<ApiResponse<OrderResponseDto>> ShipOrderAsync(int orderId, ShipOrderRequestDto request, Guid staffId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return ApiResponse.ErrorResult<OrderResponseDto>("Order not found");

                // ✅ THÊM: Validate status transition - CHỈ CHO PHÉP SHIP KHI Ở PACKED
                if (order.Status != OrderStatus.Packed)
                {
                    _logger.LogWarning("Order {OrderId} cannot be shipped. Current status: {Status}",
                        orderId, order.Status);
                    return ApiResponse.ErrorResult<OrderResponseDto>(
                        $"Không thể giao vận chuyển từ trạng thái hiện tại: {GetStatusText(order.Status)}. " +
                        "Đơn hàng phải ở trạng thái 'Đã đóng gói' trước.");
                }

                order.TrackingNumber = request.TrackingNumber;
                order.ShippingCarrier = request.ShippingCarrier;
                order.EstimatedDeliveryDate = request.EstimatedDeliveryDate;
                order.ShippedDate = DateTime.UtcNow;

                var result = await UpdateOrderStatusAsync(orderId, OrderStatus.Shipped, request.ShippingNotes);

                // Send specific shipment notification with tracking info
                if (result.Success && !string.IsNullOrEmpty(request.TrackingNumber))
                {
                    try
                    {
                        await _notificationService.SendOrderShipmentNotificationAsync(orderId, request.TrackingNumber);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send shipment notifications for order {OrderNumber}", order.OrderNumber);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shipping order {OrderId}", orderId);
                return ApiResponse.ErrorResult<OrderResponseDto>("Failed to ship order");
            }
        }

        public async Task<ApiResponse<OrderResponseDto>> DeliverOrderAsync(int orderId, Guid staffId)
        {
            try
            {
                // ✅ THÊM: Load order để validate
                var order = await _context.Orders.FindAsync(orderId);

                if (order == null)
                {
                    return ApiResponse.ErrorResult<OrderResponseDto>("Không tìm thấy đơn hàng");
                }

                // ✅ THÊM: Validate status - CHỈ CHO PHÉP DELIVER KHI Ở OutForDelivery
                if (order.Status != OrderStatus.OutForDelivery)
                {
                    _logger.LogWarning("Order {OrderId} cannot be delivered. Current status: {Status}",
                        orderId, order.Status);
                    return ApiResponse.ErrorResult<OrderResponseDto>(
                        $"Không thể đánh dấu đã giao từ trạng thái hiện tại: {GetStatusText(order.Status)}. " +
                        "Đơn hàng phải ở trạng thái 'Đang giao hàng' trước.");
                }

                var result = await UpdateOrderStatusAsync(orderId, OrderStatus.Delivered, "Order delivered successfully");

                if (result.Success)
                {
                    _logger.LogInformation("✅ Order {OrderId} delivered successfully", orderId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delivering order {OrderId}", orderId);
                return ApiResponse.ErrorResult<OrderResponseDto>("Failed to deliver order");
            }
        }

        // Simplified implementations for remaining interface methods that weren't fully implemented
        public async Task<ApiResponse<OrderResponseDto>> ConfirmOrderAsync(int orderId, Guid staffId)
        {
            return await UpdateOrderStatusAsync(orderId, OrderStatus.Confirmed, "Order confirmed by staff");
        }

        public async Task<ApiResponse<OrderResponseDto>> ProcessOrderAsync(int orderId, ProcessOrderRequestDto request, Guid staffId)
        {
            return await UpdateOrderStatusAsync(orderId, OrderStatus.Processing, request.ProcessingNotes);
        }

        public async Task<ApiResponse<OrderResponseDto>> AddOrderItemAsync(int orderId, AddOrderItemRequestDto request, Guid staffId)
        {
            return ApiResponse.ErrorResult<OrderResponseDto>("Feature not implemented yet");
        }

        public async Task<ApiResponse<OrderResponseDto>> UpdateOrderItemAsync(int orderId, int itemId, UpdateOrderItemRequestDto request, Guid staffId)
        {
            return ApiResponse.ErrorResult<OrderResponseDto>("Feature not implemented yet");
        }

        public async Task<ApiResponse<OrderResponseDto>> RemoveOrderItemAsync(int orderId, int itemId, Guid staffId)
        {
            return ApiResponse.ErrorResult<OrderResponseDto>("Feature not implemented yet");
        }

        public async Task<ApiResponse<OrderResponseDto>> AddOrderNoteAsync(int orderId, string note, bool isCustomerVisible, Guid userId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                    return ApiResponse.ErrorResult<OrderResponseDto>("Order not found");

                var orderNote = new OrderNote
                {
                    OrderId = orderId,
                    Note = note,
                    IsPublic = isCustomerVisible,
                    IsSystem = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.OrderNotes.Add(orderNote);
                await _context.SaveChangesAsync();

                var orderDto = await MapOrderToDetailDto(order);
                return ApiResponse.SuccessResult(orderDto, "Order note added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding note to order {OrderId}", orderId);
                return ApiResponse.ErrorResult<OrderResponseDto>("Failed to add order note");
            }
        }

        public async Task<ApiResponse<List<OrderNoteDto>>> GetOrderNotesAsync(int orderId, Guid userId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                    return ApiResponse.ErrorResult<List<OrderNoteDto>>("Order not found");

                var notes = await _context.OrderNotes
                    .Where(n => n.OrderId == orderId && (n.IsPublic || order.UserId == userId))
                    .OrderBy(n => n.CreatedAt)
                    .ToListAsync();

                var noteDtos = notes.Select(n => new OrderNoteDto
                {
                    Id = n.Id,
                    Note = n.Note,
                    IsCustomerVisible = n.IsPublic,
                    CreatedAt = n.CreatedAt,
                    IsFromCustomer = !n.IsSystem
                }).ToList();

                return ApiResponse.SuccessResult(noteDtos, "Order notes retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notes for order {OrderId}", orderId);
                return ApiResponse.ErrorResult<List<OrderNoteDto>>("Failed to retrieve order notes");
            }
        }

        public async Task<ApiResponse<OrderStatsDto>> GetOrderStatsAsync(Guid? userId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                _logger.LogInformation("Getting order statistics. UserId: {UserId}, FromDate: {FromDate}, ToDate: {ToDate}",
                    userId, fromDate, toDate);

                var query = _context.Orders.AsQueryable();

                // Filter by user if specified
                if (userId.HasValue)
                {
                    query = query.Where(o => o.UserId == userId.Value);
                }

                // Apply date filters
                if (fromDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt <= toDate.Value);
                }

                // Get all orders for calculation
                var orders = await query.ToListAsync();

                if (!orders.Any())
                {
                    return ApiResponse.SuccessResult(new OrderStatsDto
                    {
                        TotalOrders = 0,
                        TotalRevenue = 0,
                        AverageOrderValue = 0,
                        FromDate = fromDate ?? DateTime.UtcNow.AddDays(-30),
                        ToDate = toDate ?? DateTime.UtcNow,
                        TopProducts = new List<TopProductDto>()
                    }, "Order statistics retrieved successfully (no data)");
                }

                // Calculate basic statistics
                var totalOrders = orders.Count;
                var totalRevenue = orders.Sum(o => o.TotalAmount);
                var averageOrderValue = totalRevenue / totalOrders;

                // Count by status
                var pendingOrders = orders.Count(o => o.Status == OrderStatus.Pending);
                var confirmedOrders = orders.Count(o => o.Status == OrderStatus.Confirmed);
                var processingOrders = orders.Count(o => o.Status == OrderStatus.Processing);
                var shippedOrders = orders.Count(o => o.Status == OrderStatus.Shipped);
                var deliveredOrders = orders.Count(o => o.Status == OrderStatus.Delivered);
                var cancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled);
                var returnedOrders = orders.Count(o => o.Status == OrderStatus.Returned);

                // Count by payment status
                var paidOrders = orders.Count(o => o.PaymentStatus == PaymentStatus.Paid);
                var pendingPaymentOrders = orders.Count(o => o.PaymentStatus == PaymentStatus.Pending);
                var failedPaymentOrders = orders.Count(o => o.PaymentStatus == PaymentStatus.Failed);

                // Calculate trends (compare with previous period)
                var periodDays = (toDate ?? DateTime.UtcNow).Subtract(fromDate ?? DateTime.UtcNow.AddDays(-30)).Days;
                var previousFromDate = (fromDate ?? DateTime.UtcNow.AddDays(-30)).AddDays(-periodDays);
                var previousToDate = fromDate ?? DateTime.UtcNow.AddDays(-30);

                var previousPeriodQuery = _context.Orders.AsQueryable();
                if (userId.HasValue)
                {
                    previousPeriodQuery = previousPeriodQuery.Where(o => o.UserId == userId.Value);
                }
                previousPeriodQuery = previousPeriodQuery.Where(o => o.CreatedAt >= previousFromDate && o.CreatedAt <= previousToDate);

                var previousOrders = await previousPeriodQuery.ToListAsync();
                var previousRevenue = previousOrders.Sum(o => o.TotalAmount);
                var previousOrderCount = previousOrders.Count;

                var revenueGrowth = previousRevenue > 0 ? ((totalRevenue - previousRevenue) / previousRevenue) * 100 : 0;
                var orderGrowth = previousOrderCount > 0 ? ((totalOrders - previousOrderCount) / (decimal)previousOrderCount) * 100 : 0;

                var cancellationRate = totalOrders > 0 ? (cancelledOrders / (double)totalOrders) * 100 : 0;
                var returnRate = deliveredOrders > 0 ? (returnedOrders / (double)deliveredOrders) * 100 : 0;

                // Get top products from order items
                var topProducts = await GetTopProductsFromOrdersAsync(orders.Select(o => o.Id).ToList());

                var stats = new OrderStatsDto
                {
                    TotalOrders = totalOrders,
                    TotalRevenue = totalRevenue,
                    AverageOrderValue = averageOrderValue,

                    // Status breakdown
                    PendingOrders = pendingOrders,
                    ConfirmedOrders = confirmedOrders,
                    ProcessingOrders = processingOrders,
                    ShippedOrders = shippedOrders,
                    DeliveredOrders = deliveredOrders,
                    CancelledOrders = cancelledOrders,
                    ReturnedOrders = returnedOrders,

                    // Payment status
                    PaidOrders = paidOrders,
                    PendingPaymentOrders = pendingPaymentOrders,
                    FailedPaymentOrders = failedPaymentOrders,

                    // Time period
                    FromDate = fromDate ?? DateTime.UtcNow.AddDays(-30),
                    ToDate = toDate ?? DateTime.UtcNow,

                    // Trends
                    RevenueGrowth = revenueGrowth,
                    OrderGrowth = orderGrowth,
                    CancellationRate = cancellationRate,
                    ReturnRate = returnRate,

                    // Top products
                    TopProducts = topProducts
                };

                _logger.LogInformation("Order statistics calculated successfully. Total orders: {TotalOrders}, Total revenue: {TotalRevenue}",
                    totalOrders, totalRevenue);

                return ApiResponse.SuccessResult(stats, "Order statistics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statistics");
                return ApiResponse.ErrorResult<OrderStatsDto>("Failed to retrieve order statistics");
            }
        }

        public async Task<ApiResponse<List<OrderSummaryDto>>> GetRecentOrdersAsync(int count = 10)
        {
            try
            {
                _logger.LogInformation("Getting {Count} recent orders", count);

                var orders = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(count)
                    .ToListAsync();

                var orderSummaries = orders.Select(MapOrderToSummaryDto).ToList();

                _logger.LogInformation("Retrieved {Count} recent orders", orderSummaries.Count);

                return ApiResponse.SuccessResult(orderSummaries, "Recent orders retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent orders");
                return ApiResponse.ErrorResult<List<OrderSummaryDto>>("Failed to retrieve recent orders");
            }
        }

        public async Task<ApiResponse<OrderValidationDto>> ValidateOrderAsync(CreateOrderRequestDto request, Guid userId)
        {
            try
            {
                var validation = new OrderValidationDto { IsValid = true };
                var errors = new List<string>();

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    errors.Add("User not found");
                    validation.IsValid = false;
                }

                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.Id == request.ShippingAddressId && a.UserId == userId);

                validation.ValidShippingAddress = address != null;
                if (!validation.ValidShippingAddress)
                {
                    errors.Add("Invalid shipping address");
                    validation.IsValid = false;
                }

                var paymentMethods = new[] { "COD", "BANK_TRANSFER", "MOMO", "ZALOPAY" };
                validation.ValidPaymentMethod = paymentMethods.Contains(request.PaymentMethod);
                if (!validation.ValidPaymentMethod)
                {
                    errors.Add("Invalid payment method");
                    validation.IsValid = false;
                }

                validation.Errors = errors;
                return ApiResponse.SuccessResult(validation, "Order validation completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating order for user {UserId}", userId);
                return ApiResponse.ErrorResult<OrderValidationDto>("Failed to validate order");
            }
        }

        public async Task<ApiResponse<decimal>> CalculateOrderTotalAsync(List<OrderItemRequestDto> items, int? shippingAddressId = null, string? couponCode = null, bool isExpressDelivery = false)
        {
            try
            {
                decimal subTotal = 0;
                var orderItemsData = new List<(Product Product, int Quantity, string CustomOptions)>();

                foreach (var item in items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        subTotal += product.Price * item.Quantity;
                        orderItemsData.Add((product, item.Quantity, item.CustomOptions ?? "{}"));
                    }
                }

                // FIXED: Pass express delivery parameter and use updated shipping calculation
                decimal shipping = shippingAddressId.HasValue
                    ? await CalculateShippingCostForOrderAsync(shippingAddressId.Value, orderItemsData, isExpressDelivery)
                    : (isExpressDelivery ? 50000 : 30000);

                decimal discount = !string.IsNullOrEmpty(couponCode)
                    ? await CalculateCouponDiscountAsync(couponCode, subTotal)
                    : 0;

                decimal total = subTotal + shipping - discount;

                return ApiResponse.SuccessResult(total, "Order total calculated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating order total");
                return ApiResponse.ErrorResult<decimal>("Failed to calculate order total");
            }
        }

        public async Task<ApiResponse<OrderResponseDto>> RequestReturnAsync(int orderId, ReturnRequestDto request, Guid userId)
        {
            return ApiResponse.ErrorResult<OrderResponseDto>("Feature not implemented yet");
        }

        public async Task<ApiResponse<OrderResponseDto>> ProcessReturnAsync(int orderId, ProcessReturnRequestDto request, Guid staffId)
        {
            return ApiResponse.ErrorResult<OrderResponseDto>("Feature not implemented yet");
        }
        /// <summary>
        /// Xác nhận đã nhận hàng - Customer confirms delivery
        /// </summary>
        public async Task<ApiResponse<OrderResponseDto>> ConfirmDeliveryReceivedAsync(int orderId, Guid userId, bool isReceived, string? notes = null)
        {
            try
            {
                _logger.LogInformation("User {UserId} confirming delivery for order {OrderId}. Received: {IsReceived}",
                    userId, orderId, isReceived);

                var order = await _context.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null)
                {
                    return ApiResponse.ErrorResult<OrderResponseDto>("Không tìm thấy đơn hàng");
                }

                if (order.Status != OrderStatus.Delivered)
                {
                    return ApiResponse.ErrorResult<OrderResponseDto>(
                        $"Không thể xác nhận nhận hàng cho đơn hàng ở trạng thái {GetStatusText(order.Status)}. " +
                        "Đơn hàng phải ở trạng thái 'Đã giao hàng'.");
                }

                if (isReceived)
                {
                    // ✅ CASE 1: Đã nhận hàng → Completed
                    var confirmNote = new OrderNote
                    {
                        OrderId = orderId,
                        Note = $"✅ Khách hàng xác nhận đã nhận được hàng. {notes ?? ""}".Trim(),
                        IsPublic = true,
                        IsSystem = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.OrderNotes.Add(confirmNote);

                    var oldStatus = order.Status;
                    order.Status = OrderStatus.Completed; // ✅ Chuyển sang Completed
                    order.UpdatedAt = DateTime.UtcNow;

                    var statusHistory = new OrderStatusHistory
                    {
                        OrderId = order.Id,
                        OldStatus = oldStatus,
                        NewStatus = OrderStatus.Completed,
                        Notes = $"✅ Khách hàng xác nhận đã nhận được hàng lúc {DateTime.UtcNow:dd/MM/yyyy HH:mm}. Đơn hàng hoàn thành.",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.OrderStatusHistory.Add(statusHistory);

                    await _context.SaveChangesAsync();

                    // ✅ SỬA: Gửi notifications với scope riêng
                    var orderNumber = order.OrderNumber;
                    var receiverName = order.ReceiverName;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                            // Notification cho customer
                            await notificationService.SendOrderStatusNotificationAsync(
                                orderId,
                                OrderStatus.Completed,
                                "🎉 Đơn hàng đã hoàn thành. Cảm ơn bạn đã mua hàng tại Sakura Home!");

                            // Notification cho admin
                            await SendNotificationToAllAdminsWithScopeAsync(
                                scope.ServiceProvider,
                                "✅ Đơn hàng hoàn thành",
                                $"Đơn hàng #{orderNumber} - {receiverName} đã được khách hàng xác nhận và hoàn thành.",
                                NotificationType.Order,
                                orderId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to send completion notifications for order {OrderId}", orderId);
                        }
                    });

                    _logger.LogInformation("✅ Order {OrderNumber} completed by customer", order.OrderNumber);

                    var orderDto = await MapOrderToDetailDto(order);
                    return ApiResponse.SuccessResult(orderDto, "✅ Cảm ơn bạn đã xác nhận! Đơn hàng đã hoàn thành.");
                }
                else
                {
                    // ✅ CASE 2: Chưa nhận hàng → URGENT
                    var issueNote = new OrderNote
                    {
                        OrderId = orderId,
                        Note = $"⚠️ Khách hàng báo CHƯA nhận được hàng. Lý do: {notes ?? "Không nêu rõ"}",
                        IsPublic = true,
                        IsSystem = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.OrderNotes.Add(issueNote);

                    var statusHistory = new OrderStatusHistory
                    {
                        OrderId = order.Id,
                        OldStatus = OrderStatus.Delivered,
                        NewStatus = OrderStatus.Delivered,
                        Notes = $"⚠️ Khách hàng báo CHƯA nhận được hàng. Cần liên hệ xử lý. Lý do: {notes ?? "Không nêu rõ"}",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.OrderStatusHistory.Add(statusHistory);

                    order.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    var orderNumber = order.OrderNumber;
                    var receiverName = order.ReceiverName;
                    var receiverPhone = order.ReceiverPhone;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                            await notificationService.SendOrderStatusNotificationAsync(
                                orderId,
                                OrderStatus.Delivered,
                                "⚠️ Chúng tôi đã ghi nhận vấn đề của bạn. Nhân viên sẽ liên hệ với bạn sớm nhất có thể.");

                            await SendNotificationToAllAdminsWithScopeAsync(
                                scope.ServiceProvider,
                                "🚨 KHẨN CẤP: Khách hàng CHƯA nhận được hàng",
                                $"Đơn hàng #{orderNumber} - {receiverName} ({receiverPhone}) " +
                                $"báo CHƯA nhận được hàng. Lý do: {notes ?? "Không nêu rõ"}. VUI LÒNG LIÊN HỆ NGAY!",
                                NotificationType.Order,
                                orderId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send delivery issue notifications");
                        }
                    });

                    var orderDto = await MapOrderToDetailDto(order);
                    return ApiResponse.SuccessResult(orderDto,
                        "⚠️ Chúng tôi đã ghi nhận vấn đề của bạn. Nhân viên chăm sóc khách hàng sẽ liên hệ với bạn trong thời gian sớm nhất.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing delivery confirmation for order {OrderId}", orderId);
                return ApiResponse.ErrorResult<OrderResponseDto>("Lỗi khi xác nhận giao hàng");
            }
        }

        private async Task<OrderResponseDto> MapOrderToDetailDto(Order order)
        {
            // Reload with navigation properties if needed
            if (order.OrderItems == null)
            {
                order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.StatusHistory)
                    .Include(o => o.OrderNotes)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);
            }

            return new OrderResponseDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                Status = order.Status,
                CustomerName = order.ReceiverName,
                CustomerEmail = order.ReceiverEmail ?? "",
                CustomerPhone = order.ReceiverPhone,

                Items = order.OrderItems?.Select(MapOrderItemToDto).ToList() ?? new List<OrderItemDto>(),

                ShippingAddress = new OrderAddressDto
                {
                    FullName = order.ReceiverName,
                    Phone = order.ReceiverPhone,
                    AddressLine1 = order.ShippingAddress,
                    AddressLine2 = "",
                    City = "",
                    State = "",
                    PostalCode = "",
                    Country = "",
                    Notes = ""
                },

                BillingAddress = new OrderAddressDto
                {
                    FullName = order.ReceiverName,
                    Phone = order.ReceiverPhone,
                    AddressLine1 = order.BillingAddress ?? order.ShippingAddress,
                    AddressLine2 = "",
                    City = "",
                    State = "",
                    PostalCode = "",
                    Country = ""
                },

                SubTotal = order.SubTotal,
                ShippingCost = order.ShippingFee,
                TaxAmount = order.TaxAmount,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,

                PaymentMethod = "COD", // Default - you can enhance this
                PaymentStatus = order.PaymentStatus,
                PaymentDate = null,

                TrackingNumber = order.TrackingNumber,
                ShippingCarrier = order.ShippingCarrier,
                ShippedDate = order.ShippedDate,
                DeliveredDate = order.DeliveredDate,
                EstimatedDeliveryDate = order.EstimatedDeliveryDate,

                OrderNotes = order.CustomerNotes ?? "",
                CouponCode = order.CouponCode,
                ExpressDelivery = order.DeliveryMethod == DeliveryMethod.Express,
                GiftWrap = order.GiftWrapRequested,
                GiftMessage = order.GiftMessage,

                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                CancelledDate = order.CancelledDate,
                CancellationReason = order.CancelReason,

                StatusHistory = order.StatusHistory?.Select(sh => new OrderStatusHistoryDto
                {
                    Id = sh.Id,
                    Status = sh.NewStatus,
                    StatusText = GetStatusText(sh.NewStatus),
                    Notes = sh.Notes,
                    CreatedAt = sh.CreatedAt,
                    IsSystemGenerated = true
                }).ToList() ?? new List<OrderStatusHistoryDto>(),

                Notes = order.OrderNotes?.Select(n => new OrderNoteDto
                {
                    Id = n.Id,
                    Note = n.Note,
                    IsCustomerVisible = n.IsPublic,
                    CreatedAt = n.CreatedAt,
                    IsFromCustomer = !n.IsSystem
                }).ToList() ?? new List<OrderNoteDto>()
            };
        }

            private OrderSummaryDto MapOrderToSummaryDto(Order order)
            {
                return new OrderSummaryDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    Status = order.Status,
                    StatusText = GetStatusText(order.Status),
                    TotalAmount = order.TotalAmount,
                    PaymentMethod = "COD", // Default
                    PaymentStatus = order.PaymentStatus,
                    ItemCount = order.OrderItems?.Count ?? 0,
                    CreatedAt = order.CreatedAt,
                    EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                    TrackingNumber = order.TrackingNumber,
                    CustomerName = order.User?.FullName ?? order.ReceiverName,
                    CustomerEmail = order.User?.Email ?? order.ReceiverEmail ?? string.Empty,
                    CustomerPhone = order.User?.PhoneNumber ?? order.ReceiverPhone,
                    ItemNames = order.OrderItems?.Select(oi => oi.ProductName).ToList() ?? new List<string>()
                };
            }

        private OrderItemDto MapOrderItemToDto(OrderItem item)
        {
            return new OrderItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductVariantId = item.ProductVariantId,
                ProductName = item.ProductName,
                ProductSku = item.ProductSku,
                ProductImage = item.ProductImage,
                ProductSlug = item.Product?.Slug ?? "",
                VariantName = item.VariantName,
                VariantSku = item.VariantSku,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
                CustomOptions = item.ProductAttributes,
                BrandName = item.Product?.Brand?.Name ?? "",
                CategoryName = item.Product?.Category?.Name ?? "",
                CurrentPrice = item.Product?.Price,
                IsStillAvailable = item.Product?.IsActive == true && item.Product?.IsDeleted == false
            };
        }

        private string GetStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Đang chờ xử lý",
                OrderStatus.Confirmed => "Đã xác nhận",
                OrderStatus.Processing => "Đang xử lý",
                OrderStatus.Packed => "Đã đóng gói",
                OrderStatus.Shipped => "Đã giao cho vận chuyển",
                OrderStatus.OutForDelivery => "Đang giao hàng",
                OrderStatus.Delivered => "Đã giao hàng",
                OrderStatus.Cancelled => "Đã hủy",
                OrderStatus.Returned => "Đã trả hàng",
                OrderStatus.Refunded => "Đã hoàn tiền",
                OrderStatus.Completed => "Đã hoàn thành",
                _ => "Không xác định"
            };
        }

        public async Task<ApiResponse<List<OrderStatusHistoryDto>>> GetOrderStatusHistoryAsync(int orderId)
        {
            try
            {
                var statusHistories = await _context.OrderStatusHistory
                    .Where(sh => sh.OrderId == orderId)
                    .OrderBy(sh => sh.CreatedAt)
                    .ToListAsync();

                var historyDtos = statusHistories.Select(sh => new OrderStatusHistoryDto
                {
                    Id = sh.Id,
                    Status = sh.NewStatus,
                    StatusText = GetStatusText(sh.NewStatus),
                    Notes = sh.Notes,
                    CreatedAt = sh.CreatedAt,
                    IsSystemGenerated = true
                }).ToList();

                return ApiResponse.SuccessResult(historyDtos, "Order status history retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status history for order {OrderId}", orderId);
                return ApiResponse.ErrorResult<List<OrderStatusHistoryDto>>("Failed to retrieve order status history");
            }
        }

        /// <summary>
        /// Get top products from a list of orders
        /// </summary>
        private async Task<List<TopProductDto>> GetTopProductsFromOrdersAsync(List<int> orderIds)
        {
            try
            {
                if (!orderIds.Any())
                {
                    return new List<TopProductDto>();
                }

                var topProducts = await _context.OrderItems
                    .Where(oi => orderIds.Contains(oi.OrderId))
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new TopProductDto
                    {
                        ProductId = g.Key,
                        ProductName = g.First().ProductName,
                        TotalQuantitySold = g.Sum(oi => oi.Quantity),
                        TotalRevenue = g.Sum(oi => oi.TotalPrice),
                        TotalOrders = g.Count()
                    })
                    .OrderByDescending(tp => tp.TotalRevenue)
                    .Take(5)
                    .ToListAsync();

                return topProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top products from orders");
                return new List<TopProductDto>();
            }
        }

        /// <summary>
        /// Pack order - Đóng gói đơn hàng (Staff only)
        /// </summary>
        public async Task<ApiResponse<OrderResponseDto>> PackOrderAsync(int orderId, Guid staffId, string? notes = null)
        {
            try
            {
                _logger.LogInformation("Staff {StaffId} packing order {OrderId}", staffId, orderId);

                var order = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponse.ErrorResult<OrderResponseDto>("Không tìm thấy đơn hàng");
                }

                // ✅ Validate transition: Chỉ cho phép Pack khi ở Processing
                if (order.Status != OrderStatus.Processing)
                {
                    _logger.LogWarning("Order {OrderId} cannot be packed. Current status: {Status}",
                        orderId, order.Status);
                    return ApiResponse.ErrorResult<OrderResponseDto>(
                        $"Không thể đóng gói đơn hàng từ trạng thái hiện tại: {GetStatusText(order.Status)}. " +
                        "Đơn hàng phải ở trạng thái 'Đang xử lý'.");
                }

                var packingNotes = notes ?? "Đơn hàng đã được đóng gói và sẵn sàng giao";
                var result = await UpdateOrderStatusAsync(orderId, OrderStatus.Packed, packingNotes);

                if (result.Success)
                {
                    _logger.LogInformation("✅ Order {OrderId} packed successfully by staff {StaffId}",
                        orderId, staffId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error packing order {OrderId}", orderId);
                return ApiResponse.ErrorResult<OrderResponseDto>("Lỗi khi đóng gói đơn hàng");
            }
        }

        /// <summary>
        /// Mark order as out for delivery - Đang giao hàng
        /// </summary>
        public async Task<ApiResponse<OrderResponseDto>> MarkOutForDeliveryAsync(int orderId, Guid staffId, string? notes = null)
        {
            try
            {
                _logger.LogInformation("Staff {StaffId} marking order {OrderId} as out for delivery", staffId, orderId);

                var order = await _context.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponse.ErrorResult<OrderResponseDto>("Không tìm thấy đơn hàng");
                }

                // ✅ Validate transition: Chỉ cho phép OutForDelivery khi ở Shipped
                if (order.Status != OrderStatus.Shipped)
                {
                    _logger.LogWarning("Order {OrderId} cannot be marked as out for delivery. Current status: {Status}",
                        orderId, order.Status);
                    return ApiResponse.ErrorResult<OrderResponseDto>(
                        $"Không thể chuyển sang trạng thái 'Đang giao hàng' từ trạng thái hiện tại: {GetStatusText(order.Status)}. " +
                        "Đơn hàng phải ở trạng thái 'Đã giao cho vận chuyển' trước.");
                }

                var deliveryNotes = notes ?? "Đơn hàng đang được giao đến khách hàng";
                var result = await UpdateOrderStatusAsync(orderId, OrderStatus.OutForDelivery, deliveryNotes);

                if (result.Success)
                {
                    _logger.LogInformation("✅ Order {OrderId} marked as out for delivery by staff {StaffId}",
                        orderId, staffId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order {OrderId} as out for delivery", orderId);
                return ApiResponse.ErrorResult<OrderResponseDto>("Lỗi khi cập nhật trạng thái giao hàng");
            }
        }

        /// <summary>
        /// Handle delivery failure - Giao hàng thất bại
        /// </summary>
        /// <summary>
        /// Handle delivery failure - Giao hàng thất bại và hoàn kho
        /// </summary>
        public async Task<ApiResponse<OrderResponseDto>> MarkDeliveryFailedAsync(int orderId, Guid staffId, string failureReason)
        {
            try
            {
                _logger.LogInformation("Staff {StaffId} marking order {OrderId} as delivery failed. Reason: {Reason}",
                    staffId, orderId, failureReason);

                var order = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)  // ✅ THÊM: ThenInclude để lấy Product
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponse.ErrorResult<OrderResponseDto>("Không tìm thấy đơn hàng");
                }

                // ✅ Only allow from OutForDelivery or Shipped status
                if (order.Status != OrderStatus.OutForDelivery && order.Status != OrderStatus.Shipped)
                {
                    return ApiResponse.ErrorResult<OrderResponseDto>(
                        "Chỉ có thể đánh dấu giao hàng thất bại cho đơn hàng đang trong quá trình vận chuyển");
                }

                // ✅ ====== THÊM ĐOẠN NÀY: RESTORE INVENTORY ======
                _logger.LogInformation("Restoring inventory for failed delivery order {OrderId}", orderId);

                foreach (var item in order.OrderItems)
                {
                    var product = item.Product;
                    if (product != null && product.Status != ProductStatus.Discontinued)
                    {
                        var oldStock = product.Stock;
                        product.Stock += item.Quantity;

                        // Nếu sản phẩm đang OutOfStock, đổi về Active
                        if (product.Stock > 0 && product.Status == ProductStatus.OutOfStock)
                        {
                            product.Status = ProductStatus.Active;
                        }

                        product.UpdatedAt = DateTime.UtcNow;

                        _logger.LogInformation("✅ Restored {Quantity} units of product {ProductId} to stock. " +
                            "Old stock: {OldStock}, New stock: {NewStock}",
                            item.Quantity, product.Id, oldStock, product.Stock);
                    }
                }
                // ✅ ====== KẾT THÚC RESTORE INVENTORY ======

                // ✅ Add a note about delivery failure - CẬP NHẬT MESSAGE
                var noteText = $"⚠️ GIAO HÀNG THẤT BẠI: {failureReason}. Hàng đã được hoàn về kho.";

                var orderNote = new OrderNote
                {
                    OrderId = orderId,
                    Note = noteText,
                    IsPublic = true, // Visible to customer
                    IsSystem = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.OrderNotes.Add(orderNote);

                // ✅ Keep status as Shipped or OutForDelivery (will retry)
                var statusHistory = new OrderStatusHistory
                {
                    OrderId = order.Id,
                    OldStatus = order.Status,
                    NewStatus = order.Status, // Same status - keep for retry
                    Notes = noteText,
                    CreatedAt = DateTime.UtcNow
                };
                _context.OrderStatusHistory.Add(statusHistory);

                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // ✅ Send notification about delivery failure - CẬP NHẬT MESSAGE
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Notification cho khách hàng
                        await _notificationService.SendOrderStatusNotificationAsync(orderId, order.Status,
                            $"⚠️ Giao hàng thất bại: {failureReason}. Hàng đã được hoàn về kho. " +
                            "Chúng tôi sẽ liên hệ với bạn để sắp xếp giao lại.");

                        // Notification cho admin - CẬP NHẬT MESSAGE
                        await SendNotificationToAllAdminsAsync(
                            "⚠️ Giao hàng thất bại - Đã hoàn kho",
                            $"Đơn hàng #{order.OrderNumber} - {order.ReceiverName} giao hàng thất bại. " +
                            $"Lý do: {failureReason}. Hàng đã được hoàn về kho.",
                            NotificationType.Order,
                            orderId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send delivery failure notifications for order {OrderId}", orderId);
                    }
                });

                _logger.LogInformation("✅ Order {OrderId} marked as delivery failed and inventory restored", orderId);

                var orderDto = await MapOrderToDetailDto(order);
                return ApiResponse.SuccessResult(orderDto, "Đã ghi nhận giao hàng thất bại và hoàn hàng về kho");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order {OrderId} as delivery failed", orderId);
                return ApiResponse.ErrorResult<OrderResponseDto>("Lỗi khi ghi nhận giao hàng thất bại");
            }
        }

        #region Helper Methods 

        /// <summary>
        /// Calculate shipping cost - NEW LOGIC: Simple standard/express rates + free shipping threshold
        /// </summary>
        private async Task<decimal> CalculateShippingCostForOrderAsync(int shippingAddressId,
            List<(Product Product, int Quantity, string CustomOptions)> orderItems, bool isExpressDelivery = false)
        {
            try
            {
                // Calculate subtotal first
                decimal subtotal = 0;
                foreach (var (product, quantity, _) in orderItems)
                {
                    subtotal += product.Price * quantity;
                }

                // FREE SHIPPING for orders > 700,000 VND
                if (subtotal > 700000)
                {
                    _logger.LogInformation("Free shipping applied for order subtotal {Subtotal} VND", subtotal);
                    return 0;
                }

                // Standard shipping logic based on delivery method
                decimal shippingFee = isExpressDelivery ? 50000 : 30000;

                _logger.LogInformation("Calculated shipping cost: {ShippingFee} VND (Express: {IsExpress}, Subtotal: {Subtotal})",
                    shippingFee, isExpressDelivery, subtotal);

                return shippingFee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating shipping cost for address {AddressId}", shippingAddressId);
                return 30000; // Return default shipping fee on error
            }
        }

        /// <summary>
        /// Format address with province/ward lookup - ASYNC METHOD
        /// </summary>
        private async Task<string> FormatAddressAsync(Address address)
        {
            if (address == null) return "";

            try
            {
                var parts = new List<string>();

                // Add address lines
                if (!string.IsNullOrEmpty(address.AddressLine1)) parts.Add(address.AddressLine1);
                if (!string.IsNullOrEmpty(address.AddressLine2)) parts.Add(address.AddressLine2);

                // Lookup and add province/ward names
                if (address.WardId > 0)
                {
                    var ward = await _context.VietnamWards.FindAsync(address.WardId);
                    if (ward != null) parts.Add(ward.Name);
                }

                if (address.ProvinceId > 0)
                {
                    var province = await _context.VietnamProvinces.FindAsync(address.ProvinceId);
                    if (province != null) parts.Add(province.Name);
                }

                // Add postal code and country if available
                if (!string.IsNullOrEmpty(address.PostalCode)) parts.Add(address.PostalCode);
                if (!string.IsNullOrEmpty(address.Country)) parts.Add(address.Country);

                return string.Join(", ", parts.Where(p => !string.IsNullOrEmpty(p)));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error formatting address for AddressId {AddressId}", address?.Id);

                // Fallback to basic address
                var fallbackParts = new List<string>();
                if (!string.IsNullOrEmpty(address?.AddressLine1)) fallbackParts.Add(address.AddressLine1);
                if (!string.IsNullOrEmpty(address?.Country)) fallbackParts.Add(address.Country);

                return string.Join(", ", fallbackParts);
            }
        }

        /// <summary>
        /// Remove ordered items from cart
        /// </summary>
        private async Task RemoveOrderedItemsFromCartAsync(Guid userId, List<OrderItemRequestDto> orderedItems)
        {
            if (orderedItems == null || !orderedItems.Any())
            {
                _logger.LogDebug("No items to remove from cart for user {UserId}", userId);
                return;
            }

            try
            {
                _logger.LogDebug("Removing {Count} ordered items from cart for user {UserId}",
                    orderedItems.Count, userId);

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    _logger.LogDebug("No cart found for user {UserId}", userId);
                    return;
                }

                if (!cart.CartItems.Any())
                {
                    _logger.LogDebug("Cart is empty for user {UserId}", userId);
                    return;
                }

                var itemsToRemove = new List<CartItem>();

                foreach (var orderedItem in orderedItems)
                {
                    var cartItem = cart.CartItems.FirstOrDefault(ci =>
                        ci.ProductId == orderedItem.ProductId &&
                        (ci.ProductVariantId ?? 0) == (orderedItem.ProductVariantId ?? 0));

                    if (cartItem == null)
                    {
                        _logger.LogDebug("Cart item not found for ProductId={ProductId}, VariantId={VariantId}",
                            orderedItem.ProductId, orderedItem.ProductVariantId);
                        continue;
                    }

                    if (cartItem.Quantity <= orderedItem.Quantity)
                    {
                        _logger.LogDebug("Marking cart item for removal: ProductId={ProductId}, CartQty={CartQty}, OrderedQty={OrderedQty}",
                            orderedItem.ProductId, cartItem.Quantity, orderedItem.Quantity);
                        itemsToRemove.Add(cartItem);
                    }
                    else
                    {
                        var newQuantity = cartItem.Quantity - orderedItem.Quantity;
                        _logger.LogDebug("Updating cart item quantity: ProductId={ProductId}, From={From}, To={To}",
                            orderedItem.ProductId, cartItem.Quantity, newQuantity);

                        cartItem.Quantity = newQuantity;
                        cartItem.TotalPrice = cartItem.UnitPrice * newQuantity;
                        cartItem.UpdatedAt = DateTime.UtcNow;
                    }
                }

                if (itemsToRemove.Any())
                {
                    _context.CartItems.RemoveRange(itemsToRemove);
                    _logger.LogDebug("Removing {Count} cart items", itemsToRemove.Count);
                }

                cart.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Successfully processed cart updates for user {UserId}: {RemovedCount} items removed",
                    userId, itemsToRemove.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing ordered items from cart for user {UserId}", userId);
            }
        }

        private async Task<string> GenerateUniqueOrderNumberAsync()
        {
            const int maxAttempts = 10;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
                    var random = Random.Shared.Next(1000, 9999);
                    var orderNumber = $"ORD{timestamp}{random}";

                    var exists = await _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber);
                    if (!exists)
                    {
                        _logger.LogDebug("Generated unique order number: {OrderNumber} on attempt {Attempt}",
                            orderNumber, attempt);
                        return orderNumber;
                    }

                    _logger.LogDebug("Order number {OrderNumber} already exists, retrying... (attempt {Attempt})",
                        orderNumber, attempt);

                    await Task.Delay(10);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error generating order number on attempt {Attempt}", attempt);

                    if (attempt == maxAttempts)
                    {
                        throw new InvalidOperationException($"Failed to generate unique order number after {maxAttempts} attempts", ex);
                    }

                    await Task.Delay(50);
                }
            }

            throw new InvalidOperationException($"Failed to generate unique order number after {maxAttempts} attempts");
        }

        private async Task<decimal> CalculateCouponDiscountAsync(string couponCode, decimal subtotal)
        {
            if (string.IsNullOrWhiteSpace(couponCode) || subtotal <= 0) return 0;

            try
            {
                _logger.LogDebug("Calculating coupon discount for code {CouponCode} on subtotal {Subtotal}",
                    couponCode, subtotal);

                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code.ToUpper() == couponCode.ToUpper() &&
                        c.IsActive &&
                        c.StartDate <= DateTime.UtcNow &&
                        c.EndDate >= DateTime.UtcNow);

                if (coupon == null)
                {
                    _logger.LogWarning("Coupon {CouponCode} not found or not valid", couponCode);
                    return 0;
                }

                if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit)
                {
                    _logger.LogWarning("Coupon {CouponCode} has exceeded usage limit: {UsedCount}/{Limit}",
                        couponCode, coupon.UsedCount, coupon.UsageLimit);
                    return 0;
                }

                if (coupon.MinOrderAmount.HasValue && subtotal < coupon.MinOrderAmount)
                {
                    _logger.LogWarning("Order amount {Subtotal} is below minimum {Minimum} for coupon {CouponCode}",
                        subtotal, coupon.MinOrderAmount, couponCode);
                    return 0;
                }

                decimal discount = 0;

                if (coupon.Type == CouponType.Percentage)
                {
                    discount = subtotal * (coupon.Value / 100);
                }
                else if (coupon.Type == CouponType.FixedAmount)
                {
                    discount = coupon.Value;
                }
                else
                {
                    _logger.LogWarning("Unknown coupon type {Type} for coupon {CouponCode}",
                        coupon.Type, couponCode);
                    return 0;
                }

                if (coupon.MaxDiscountAmount.HasValue)
                {
                    discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);
                }

                discount = Math.Min(discount, subtotal);

                coupon.UsedCount++;
                if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit)
                {
                    coupon.IsActive = false;
                    _logger.LogInformation("Coupon {CouponCode} deactivated after reaching usage limit", couponCode);
                }

                _logger.LogInformation("Applied coupon {CouponCode} with discount {Discount} (type: {Type}, value: {Value})",
                    couponCode, discount, coupon.Type, coupon.Value);

                return discount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating discount for coupon {CouponCode}", couponCode);
                return 0;
            }
        }

        /// <summary>
        /// Gửi notification real-time cho tất cả Admin và Staff
        /// </summary>
        private async Task SendNotificationToAllAdminsAsync(string title, string message, NotificationType type, int orderId)
        {
            try
            {
                // Lấy tất cả user có role Admin hoặc Staff
                var adminUsers = await _context.Users
                    .Where(u => u.Role == UserRole.Admin || u.Role == UserRole.Staff)
                    .ToListAsync();

                if (!adminUsers.Any())
                {
                    _logger.LogWarning("No admin users found to send notification for order {OrderId}", orderId);
                    return;
                }

                // Gửi notification cho từng admin
                foreach (var admin in adminUsers)
                {
                    var notification = new CreateNotificationRequestDto
                    {
                        UserId = admin.Id,
                        Title = title,
                        Message = message,
                        Type = type,
                        Priority = Priority.High,
                        ActionUrl = $"/admin/orders/{orderId}",
                        Data = new Dictionary<string, object>
                {
                    { "orderId", orderId },
                    { "timestamp", DateTime.UtcNow }
                }
                    };

                    await _notificationService.SendNotificationAsync(notification);
                }

                _logger.LogInformation("✅ Real-time notifications sent to {Count} admins for order {OrderId}",
                    adminUsers.Count, orderId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to send admin notifications for order {OrderId}", orderId);
                // Không throw exception - notification failure không nên làm fail toàn bộ operation
            }
        }

        /// <summary>
        /// Kiểm tra có nên gửi notification cho admin không (dựa trên status)
        /// </summary>
        private bool ShouldNotifyAdminForStatus(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => true,      // Đơn hàng mới
                OrderStatus.Cancelled => true,    // Đơn bị hủy
                OrderStatus.Returned => true,     // Đơn trả hàng
                _ => false
            };
        }

        /// <summary>
        /// Tạo tiêu đề notification cho admin theo trạng thái
        /// </summary>
        private string GetAdminNotificationTitle(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "📦 Đơn hàng mới",
                OrderStatus.Cancelled => "❌ Đơn hàng bị hủy",
                OrderStatus.Returned => "↩️ Yêu cầu trả hàng",
                _ => "📋 Cập nhật đơn hàng"
            };
        }

        /// <summary>
        /// Tạo message notification cho customer theo trạng thái
        /// </summary>
        private string GetNotificationMessageForStatus(OrderStatus status, string? additionalNotes = null)
        {
            var baseMessage = status switch
            {
                OrderStatus.Pending => "📦 Đơn hàng của bạn đang chờ xác nhận",
                OrderStatus.Confirmed => "✅ Đơn hàng đã được xác nhận và sẽ sớm được xử lý",
                OrderStatus.Processing => "📦 Đơn hàng đang được chuẩn bị",
                OrderStatus.Packed => "✅ Đơn hàng đã được đóng gói và sẵn sàng giao",
                OrderStatus.Shipped => "🚚 Đơn hàng đã được giao cho đơn vị vận chuyển",
                OrderStatus.OutForDelivery => "🚗 Đơn hàng đang trên đường giao đến bạn",
                OrderStatus.Delivered => "✅ Đơn hàng đã được giao thành công. Vui lòng xác nhận đã nhận hàng!",
                OrderStatus.Cancelled => "❌ Đơn hàng đã bị hủy",
                OrderStatus.Returned => "↩️ Đơn hàng đã được trả lại",
                OrderStatus.Refunded => "💰 Đơn hàng đã được hoàn tiền",
                OrderStatus.Completed => "🎉 Đơn hàng đã hoàn thành. Cảm ơn bạn đã mua hàng!",
                _ => "📋 Trạng thái đơn hàng đã được cập nhật"
            };

            return string.IsNullOrEmpty(additionalNotes)
                ? baseMessage
                : $"{baseMessage}. {additionalNotes}";
        }

        /// <summary>
        /// ✅ THÊM: Gửi notification cho admin với scope riêng
        /// </summary>
        private async Task SendNotificationToAllAdminsWithScopeAsync(
            IServiceProvider scopedServiceProvider,
            string title,
            string message,
            NotificationType type,
            int orderId)
        {
            try
            {
                var scopedContext = scopedServiceProvider.GetRequiredService<ApplicationDbContext>();
                var notificationService = scopedServiceProvider.GetRequiredService<INotificationService>();

                var adminUsers = await scopedContext.Users
                    .Where(u => u.Role == UserRole.Admin || u.Role == UserRole.Staff || u.Role == UserRole.SuperAdmin)
                    .ToListAsync();

                if (!adminUsers.Any())
                {
                    _logger.LogWarning("No admin users found for order {OrderId}", orderId);
                    return;
                }

                foreach (var admin in adminUsers)
                {
                    var notification = new CreateNotificationRequestDto
                    {
                        UserId = admin.Id,
                        Title = title,
                        Message = message,
                        Type = type,
                        Priority = Priority.High,
                        ActionUrl = $"/admin/orders/{orderId}",
                        SendEmail = true, // ✅ Gửi cả email
                        Data = new Dictionary<string, object>
                {
                    { "orderId", orderId },
                    { "timestamp", DateTime.UtcNow }
                }
                    };

                    await notificationService.SendNotificationAsync(notification);
                }

                _logger.LogInformation("✅ Notifications sent to {Count} admins for order {OrderId}",
                    adminUsers.Count, orderId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send admin notifications for order {OrderId}", orderId);
            }
        }

        #endregion
    }
}