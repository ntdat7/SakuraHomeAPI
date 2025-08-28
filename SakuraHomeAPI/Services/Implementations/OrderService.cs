using Microsoft.EntityFrameworkCore;
using AutoMapper;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.Services.Interfaces;
using SakuraHomeAPI.Models.DTOs;
using SakuraHomeAPI.DTOs.Orders.Requests;
using SakuraHomeAPI.DTOs.Orders.Responses;
using SakuraHomeAPI.Models.Entities.Orders;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.Models.Entities;

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
                _logger.LogInformation("Creating order for user {UserId}", userId);

                // Use execution strategy to handle retry logic with transactions
                var executionStrategy = _context.Database.CreateExecutionStrategy();

                return await executionStrategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        // 1. Validate user and get cart
                        var user = await _context.Users.FindAsync(userId);
                        if (user == null)
                            return ApiResponse.ErrorResult<OrderResponseDto>("User not found");

                        var cartResult = await _cartService.GetCartAsync(userId);
                        if (!cartResult.IsSuccess || cartResult.Data?.Items.Count == 0)
                            return ApiResponse.ErrorResult<OrderResponseDto>("Cart is empty");

                        // 2. Validate order
                        var validationResult = await ValidateOrderAsync(request, userId);
                        if (!validationResult.Success || !validationResult.Data.IsValid)
                            return ApiResponse.ErrorResult<OrderResponseDto>("Order validation failed", validationResult.Data.Errors);

                        // 3. Get shipping address
                        var shippingAddress = await _context.Addresses
                            .FirstOrDefaultAsync(a => a.Id == request.ShippingAddressId && a.UserId == userId);

                        if (shippingAddress == null)
                            return ApiResponse.ErrorResult<OrderResponseDto>("Shipping address not found");

                        // 4. Get billing address (use shipping if not specified)
                        Address billingAddress = shippingAddress;
                        if (request.BillingAddressId.HasValue && request.BillingAddressId != request.ShippingAddressId)
                        {
                            billingAddress = await _context.Addresses
                                .FirstOrDefaultAsync(a => a.Id == request.BillingAddressId && a.UserId == userId);

                            if (billingAddress == null)
                                return ApiResponse.ErrorResult<OrderResponseDto>("Billing address not found");
                        }

                        // 5. Calculate order totals
                        var cart = cartResult.Data;
                        var orderTotals = await CalculateOrderTotalAsync(
                            cart.Items.Select(i => new OrderItemRequestDto
                            {
                                ProductId = i.ProductId,
                                ProductVariantId = i.ProductVariantId,
                                Quantity = i.Quantity
                            }).ToList(),
                            request.ShippingAddressId,
                            request.CouponCode
                        );

                        // 6. Create order
                        var order = new Order
                        {
                            UserId = userId,
                            OrderNumber = await GenerateOrderNumberAsync(),
                            Status = OrderStatus.Pending,

                            // Use ReceiverName, ReceiverPhone, ShippingAddress instead of detailed fields
                            ReceiverName = shippingAddress.Name,
                            ReceiverPhone = shippingAddress.Phone,
                            ReceiverEmail = user.Email,
                            ShippingAddress = $"{shippingAddress.AddressLine1}, {shippingAddress.AddressLine2}, {shippingAddress.City}, {shippingAddress.State}, {shippingAddress.PostalCode}, {shippingAddress.Country}",
                            BillingAddress = $"{billingAddress.AddressLine1}, {billingAddress.AddressLine2}, {billingAddress.City}, {billingAddress.State}, {billingAddress.PostalCode}, {billingAddress.Country}",

                            // Financial Information
                            SubTotal = cart.SubTotal,
                            ShippingFee = await CalculateShippingCostAsync(request.ShippingAddressId, cart.Items),
                            TaxAmount = 0, // TODO: Implement tax calculation
                            DiscountAmount = await CalculateDiscountAsync(request.CouponCode, cart.SubTotal),
                            TotalAmount = orderTotals.Data,
                            Currency = "VND",

                            // Payment Information
                            PaymentStatus = PaymentStatus.Pending,

                            // Order Details
                            CustomerNotes = request.OrderNotes,
                            CouponCode = request.CouponCode,
                            IsGift = request.GiftWrap,
                            GiftMessage = request.GiftMessage,
                            GiftWrapRequested = request.GiftWrap,

                            // Delivery method based on express delivery flag
                            DeliveryMethod = request.ExpressDelivery ? DeliveryMethod.Express : DeliveryMethod.Standard,

                            OrderDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.Orders.Add(order);
                        await _context.SaveChangesAsync();

                        // 7. Create order items
                        foreach (var cartItem in cart.Items)
                        {
                            var product = await _context.Products
                                .Include(p => p.Brand)
                                .Include(p => p.Category)
                                .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);

                            if (product == null) continue;

                            var orderItem = new OrderItem
                            {
                                OrderId = order.Id,
                                ProductId = cartItem.ProductId,
                                ProductVariantId = cartItem.ProductVariantId,

                                // Product snapshot
                                ProductName = product.Name,
                                ProductSku = product.SKU,
                                ProductImage = cartItem.ProductImage,
                                VariantName = cartItem.VariantName ?? "",
                                VariantSku = cartItem.ProductSku ?? "", // Use ProductSku instead of VariantSku
                                ProductAttributes = cartItem.CustomOptions ?? "{}",

                                Quantity = cartItem.Quantity,
                                UnitPrice = cartItem.UnitPrice,
                                TotalPrice = cartItem.TotalPrice
                            };

                            _context.OrderItems.Add(orderItem);
                        }

                        // 8. Create initial status history
                        var statusHistory = new OrderStatusHistory
                        {
                            OrderId = order.Id,
                            OldStatus = OrderStatus.Pending,
                            NewStatus = OrderStatus.Pending,
                            Notes = "Order created successfully",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.OrderStatusHistory.Add(statusHistory);

                        // 9. Update product stock
                        foreach (var cartItem in cart.Items)
                        {
                            var product = await _context.Products.FindAsync(cartItem.ProductId);
                            if (product != null && product.Status == ProductStatus.Active)
                            {
                                // Assuming we have Stock property instead of StockQuantity
                                product.Stock = Math.Max(0, product.Stock - cartItem.Quantity);
                                if (product.Stock == 0)
                                {
                                    product.Status = ProductStatus.OutOfStock;
                                }
                            }
                        }

                        // 10. Clear cart
                        await _cartService.ClearCartAsync(userId);

                        // 11. Update user statistics
                        user.TotalOrders++;
                        user.TotalSpent += order.TotalAmount;
                        user.UpdateTier(); // Update user tier based on spending

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("Order {OrderNumber} created successfully for user {UserId}", order.OrderNumber, userId);

                        // 12. Send notifications and emails (outside of transaction)
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                // Send order confirmation notification
                                await _notificationService.SendOrderConfirmationNotificationAsync(order.Id);

                                // Send order confirmation email
                                await _emailService.SendOrderConfirmationEmailAsync(order);

                                _logger.LogInformation("Order notifications sent for order {OrderNumber}", order.OrderNumber);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to send order notifications for order {OrderNumber}", order.OrderNumber);
                                // Don't fail the order creation if notifications fail
                            }
                        });

                        // 13. Return order details
                        var orderDto = await MapOrderToDetailDto(order);
                        return ApiResponse.SuccessResult(orderDto, "Order created successfully");
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw; // Re-throw to be caught by execution strategy
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for user {UserId}", userId);
                return ApiResponse.ErrorResult<OrderResponseDto>("Failed to create order");
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
                        order.ShippingAddress = $"{address.AddressLine1}, {address.AddressLine2}, {address.City}, {address.State}, {address.PostalCode}, {address.Country}";
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

        public async Task<ApiResponse<decimal>> CalculateOrderTotalAsync(List<OrderItemRequestDto> items, int? shippingAddressId = null, string? couponCode = null)
        {
            try
            {
                decimal subTotal = 0;
                foreach (var item in items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        subTotal += product.Price * item.Quantity;
                    }
                }

                decimal shipping = shippingAddressId.HasValue 
                    ? await CalculateShippingCostAsync(shippingAddressId.Value, new List<DTOs.Cart.Responses.CartItemDto>())
                    : 0;

                decimal discount = await CalculateDiscountAsync(couponCode, subTotal);
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
                OrderStatus.Pending => "?ang ch? x? lý",
                OrderStatus.Confirmed => "?ã xác nh?n",
                OrderStatus.Processing => "?ang x? lý",
                OrderStatus.Packed => "?ã ?óng gói",
                OrderStatus.Shipped => "?ã giao cho v?n chuy?n",
                OrderStatus.OutForDelivery => "?ang giao hàng",
                OrderStatus.Delivered => "?ã giao hàng",
                OrderStatus.Cancelled => "?ã h?y",
                OrderStatus.Returned => "?ã tr? hàng",
                OrderStatus.Refunded => "?ã hoàn ti?n",
                _ => "Không xác ??nh"
            };
        }

        /// <summary>
        /// Generate unique order number
        /// </summary>
        private async Task<string> GenerateOrderNumberAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(100, 999);
            var orderNumber = $"{timestamp}{random}";
            
            // Ensure uniqueness
            while (await _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber))
            {
                random = new Random().Next(100, 999);
                orderNumber = $"{timestamp}{random}";
            }
            
            return orderNumber;
        }

        /// <summary>
        /// Calculate shipping cost based on address and items
        /// </summary>
        private async Task<decimal> CalculateShippingCostAsync(int shippingAddressId, List<DTOs.Cart.Responses.CartItemDto> items)
        {
            try
            {
                // Get shipping address
                var address = await _context.Addresses.FindAsync(shippingAddressId);
                if (address == null) return 0;

                // For now, return a fixed shipping cost based on location
                // In a real implementation, you would calculate based on weight, distance, etc.
                var baseCost = address.Country?.ToLower() == "vietnam" ? 30000m : 100000m;
                
                // Add weight-based calculation if needed
                // For now, assume average weight of 0.5kg per item if not specified
                var totalWeight = items?.Sum(i => 0.5m * i.Quantity) ?? 0;
                var weightCost = totalWeight > 2 ? (totalWeight - 2) * 10000 : 0;

                return baseCost + weightCost;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating shipping cost, using default");
                return 30000m; // Default shipping cost
            }
        }

        /// <summary>
        /// Calculate discount amount from coupon
        /// </summary>
        private async Task<decimal> CalculateDiscountAsync(string? couponCode, decimal subtotal)
        {
            if (string.IsNullOrEmpty(couponCode)) return 0;

            try
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code == couponCode && c.IsActive);

                if (coupon == null || !coupon.IsValidForOrder(subtotal))
                    return 0;

                return coupon.Type switch
                {
                    CouponType.Percentage => Math.Min(subtotal * coupon.Value / 100m, coupon.MaxDiscountAmount ?? decimal.MaxValue),
                    CouponType.FixedAmount => Math.Min(coupon.Value, subtotal),
                    _ => 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating discount for coupon {CouponCode}", couponCode);
                return 0;
            }
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
    }
}