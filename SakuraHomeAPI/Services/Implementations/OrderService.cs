using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.DTOs.Orders.Requests;
using SakuraHomeAPI.DTOs.Orders.Responses;
using SakuraHomeAPI.Models.DTOs;
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

        public OrderService(
            ApplicationDbContext context,
            IMapper mapper,
            ICartService cartService,
            INotificationService notificationService,
            IEmailService emailService,
            ILogger<OrderService> logger)
        {
            _context = context;
            _mapper = mapper;
            _cartService = cartService;
            _notificationService = notificationService;
            _emailService = emailService;
            _logger = logger;
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
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _notificationService.SendOrderConfirmationNotificationAsync(order.Id);
                                await _emailService.SendOrderConfirmationEmailAsync(order);
                                _logger.LogInformation("Order notifications sent for order {OrderNumber}", order.OrderNumber);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to send order notifications for order {OrderNumber}", order.OrderNumber);
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
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null)
                    return ApiResponse.ErrorResult("Order not found");

                // Check if order can be cancelled
                if (!order.CanCancel)
                    return ApiResponse.ErrorResult("Order cannot be cancelled at this stage");

                // Restore product stock
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

                // Update order status
                order.Status = OrderStatus.Cancelled;
                order.CancelledDate = DateTime.UtcNow;
                order.CancelReason = reason;
                order.UpdatedAt = DateTime.UtcNow;

                // Add status history
                var statusHistory = new OrderStatusHistory
                {
                    OrderId = order.Id,
                    OldStatus = order.Status,
                    NewStatus = OrderStatus.Cancelled,
                    Notes = $"Order cancelled by customer. Reason: {reason}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.OrderStatusHistory.Add(statusHistory);

                // Update user statistics
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.TotalOrders = Math.Max(0, user.TotalOrders - 1);
                    user.TotalSpent = Math.Max(0, user.TotalSpent - order.TotalAmount);
                    user.UpdateTier();
                }

                await _context.SaveChangesAsync();

                // Send cancellation notifications
                try
                {
                    await _notificationService.SendOrderStatusNotificationAsync(orderId, OrderStatus.Cancelled, reason);
                    await _emailService.SendOrderStatusUpdateEmailAsync(order, OrderStatus.Cancelled);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send cancellation notifications for order {OrderNumber}", order.OrderNumber);
                }

                _logger.LogInformation("Order {OrderNumber} cancelled by user {UserId}", order.OrderNumber, userId);
                return ApiResponse.SuccessResult("Order cancelled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId} for user {UserId}", orderId, userId);
                return ApiResponse.ErrorResult("Failed to cancel order");
            }
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
                try
                {
                    await _notificationService.SendOrderStatusNotificationAsync(orderId, newStatus, notes);

                    // Send specific email notifications for important status changes
                    switch (newStatus)
                    {
                        case OrderStatus.Confirmed:
                        case OrderStatus.Shipped:
                        case OrderStatus.Delivered:
                        case OrderStatus.Cancelled:
                            await _emailService.SendOrderStatusUpdateEmailAsync(order, newStatus);
                            break;
                    }

                    _logger.LogInformation("Order status notifications sent for order {OrderNumber} - Status: {Status}",
                        order.OrderNumber, newStatus);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send status update notifications for order {OrderNumber}", order.OrderNumber);
                }

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
                        await _emailService.SendShipmentNotificationEmailAsync(order, request.TrackingNumber);
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
                var result = await UpdateOrderStatusAsync(orderId, OrderStatus.Delivered, "Order delivered successfully");

                if (result.Success)
                {
                    try
                    {
                        await _notificationService.SendOrderDeliveryNotificationAsync(orderId);

                        var order = await _context.Orders
                            .Include(o => o.User)
                            .FirstOrDefaultAsync(o => o.Id == orderId);

                        if (order != null)
                        {
                            await _emailService.SendDeliveryConfirmationEmailAsync(order);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send delivery notifications for order {OrderId}", orderId);
                    }
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
            return ApiResponse.ErrorResult<OrderStatsDto>("Feature not implemented yet");
        }

        public async Task<ApiResponse<List<OrderSummaryDto>>> GetRecentOrdersAsync(int count = 10)
        {
            return ApiResponse.ErrorResult<List<OrderSummaryDto>>("Feature not implemented yet");
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

        #region Helper Methods - FIXED

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

        #endregion
    }
}