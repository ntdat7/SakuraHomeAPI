using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.DTOs.Shipping.Requests;
using SakuraHomeAPI.DTOs.Shipping.Responses;
using SakuraHomeAPI.Models.Entities.Shipping;
using SakuraHomeAPI.Services.Common;
using SakuraHomeAPI.Services.Interfaces;

namespace SakuraHomeAPI.Services.Implementations
{
    public class ShippingService : IShippingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ShippingService> _logger;

        // Phí cơ bản cho hệ thống COD đơn giản
        private const decimal BASE_SHIPPING_FEE = 30000; // 30k VND
        private const decimal COD_FEE_RATE = 0.01m; // 1% của giá trị COD
        private const decimal MIN_COD_FEE = 5000; // 5k VND
        private const decimal MAX_COD_FEE = 50000; // 50k VND

        public ShippingService(
            ApplicationDbContext context,
            ILogger<ShippingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult<List<ShippingRateResponse>>> CalculateShippingRatesAsync(CalculateShippingRequest request)
        {
            try
            {
                var rates = new List<ShippingRateResponse>();

                // Standard shipping (COD)
                var standardRate = await GetStandardShippingRateAsync(request);
                if (standardRate.IsSuccess)
                {
                    rates.Add(standardRate.Data);
                }

                // Express shipping (COD)
                var expressRate = new ShippingRateResponse
                {
                    ServiceType = "EXPRESS",
                    ServiceName = "Giao hàng nhanh",
                    Fee = standardRate.Data.Fee * 1.5m, // 50% phụ phí
                    CODFee = standardRate.Data.CODFee,
                    TotalFee = (standardRate.Data.Fee * 1.5m) + standardRate.Data.CODFee,
                    EstimatedDays = 1,
                    EstimatedDelivery = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy"),
                    IsAvailable = true,
                    Note = "Giao trong 24h"
                };
                rates.Add(expressRate);

                return ServiceResult<List<ShippingRateResponse>>.Success(rates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating shipping rates");
                return ServiceResult<List<ShippingRateResponse>>.Failure("Có lỗi xảy ra khi tính phí vận chuyển");
            }
        }

        public async Task<ServiceResult<ShippingRateResponse>> GetStandardShippingRateAsync(CalculateShippingRequest request)
        {
            try
            {
                var shippingFee = await CalculateStandardShippingFeeAsync(request.Weight, request.FromProvinceCode, request.ToProvinceCode);
                var codFee = await CalculateCODFeeAsync(request.OrderValue);

                var rate = new ShippingRateResponse
                {
                    ServiceType = "STANDARD",
                    ServiceName = "Giao hàng tiêu chuẩn",
                    Fee = shippingFee,
                    CODFee = codFee,
                    TotalFee = shippingFee + codFee,
                    EstimatedDays = 3,
                    EstimatedDelivery = DateTime.Now.AddDays(3).ToString("dd/MM/yyyy"),
                    IsAvailable = true,
                    Note = "Giao trong 2-3 ngày làm việc"
                };

                return ServiceResult<ShippingRateResponse>.Success(rate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating standard shipping rate");
                return ServiceResult<ShippingRateResponse>.Failure("Có lỗi xảy ra khi tính phí vận chuyển");
            }
        }

        public async Task<ServiceResult<ShippingResponse>> CreateShippingOrderAsync(CreateShippingRequest request)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.ShippingAddress)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId);

                if (order == null)
                {
                    return ServiceResult<ShippingResponse>.Failure("Không tìm thấy đơn hàng");
                }

                // Check if shipping order already exists
                var existingShipping = await _context.ShippingOrders
                    .FirstOrDefaultAsync(s => s.OrderId == request.OrderId);

                if (existingShipping != null)
                {
                    return ServiceResult<ShippingResponse>.Failure("Đơn hàng đã có thông tin vận chuyển");
                }

                var trackingNumber = await GenerateTrackingNumberAsync();

                // Calculate shipping fees
                var shippingFee = await CalculateStandardShippingFeeAsync(
                    1, // Default 1kg weight (Order doesn't have TotalWeight property)
                    "HCM", // Default from Ho Chi Minh
                    "HCM" // Default to same province for now
                );

                var codFee = request.IsCOD ? await CalculateCODFeeAsync(request.CODAmount) : 0;

                var shippingOrder = new ShippingOrder
                {
                    OrderId = request.OrderId,
                    TrackingNumber = trackingNumber,
                    ServiceType = request.ServiceType,
                    ServiceName = request.ServiceType == "EXPRESS" ? "Giao hàng nhanh" : "Giao hàng tiêu chuẩn",
                    ShippingFee = shippingFee,
                    CODFee = codFee,
                    TotalFee = shippingFee + codFee,
                    Status = "PENDING",
                    StatusDescription = "Đang chuẩn bị hàng",
                    SenderName = "SakuraHome Store",
                    SenderPhone = "1900-1234",
                    SenderAddress = "123 Nguyễn Văn Cừ, Quận 5, TP.HCM",
                    ReceiverName = (order.User?.FirstName ?? "") + " " + (order.User?.LastName ?? ""),
                    ReceiverPhone = order.User?.PhoneNumber ?? "",
                    ReceiverAddress = "Địa chỉ giao hàng", // Simplified address
                    Weight = 1, // Default weight
                    Dimensions = "30x20x10", // Default dimensions
                    IsCOD = request.IsCOD,
                    CODAmount = request.CODAmount,
                    Notes = request.Notes ?? "",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ShippingOrders.Add(shippingOrder);

                // Add initial tracking
                var initialTracking = new ShippingTracking
                {
                    ShippingOrderId = shippingOrder.Id,
                    Status = "PENDING",
                    StatusDescription = "Đơn hàng đã được tạo và đang chuẩn bị",
                    UpdatedAt = DateTime.UtcNow,
                    Location = "SakuraHome Warehouse",
                    Notes = "Đơn hàng đã được tiếp nhận",
                    CreatedAt = DateTime.UtcNow
                };

                _context.ShippingTrackings.Add(initialTracking);
                await _context.SaveChangesAsync();

                // Load with relations
                var createdShipping = await _context.ShippingOrders
                    .Include(s => s.Order)
                    .Include(s => s.ShippingTrackings)
                    .FirstOrDefaultAsync(s => s.Id == shippingOrder.Id);

                return ServiceResult<ShippingResponse>.Success(MapToShippingResponse(createdShipping));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shipping order for order {OrderId}", request.OrderId);
                return ServiceResult<ShippingResponse>.Failure("Có lỗi xảy ra khi tạo đơn vận chuyển");
            }
        }

        public async Task<ServiceResult<ShippingResponse>> UpdateTrackingAsync(int shippingId, UpdateTrackingRequest request)
        {
            try
            {
                var shipping = await _context.ShippingOrders
                    .Include(s => s.Order)
                    .Include(s => s.ShippingTrackings)
                    .FirstOrDefaultAsync(s => s.Id == shippingId);

                if (shipping == null)
                {
                    return ServiceResult<ShippingResponse>.Failure("Không tìm thấy đơn vận chuyển");
                }

                // Update shipping status
                shipping.Status = request.Status;
                shipping.StatusDescription = request.StatusDescription;
                shipping.UpdatedAt = DateTime.UtcNow;

                // Update picked up and delivered time
                if (request.Status == "PICKED_UP" && !shipping.PickedUpAt.HasValue)
                {
                    shipping.PickedUpAt = request.UpdatedAt;
                }
                else if (request.Status == "DELIVERED" && !shipping.DeliveredAt.HasValue)
                {
                    shipping.DeliveredAt = request.UpdatedAt;
                }

                // Add tracking entry
                var tracking = new ShippingTracking
                {
                    ShippingOrderId = shippingId,
                    Status = request.Status,
                    StatusDescription = request.StatusDescription,
                    UpdatedAt = request.UpdatedAt,
                    Location = request.Location,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ShippingTrackings.Add(tracking);
                await _context.SaveChangesAsync();

                return ServiceResult<ShippingResponse>.Success(MapToShippingResponse(shipping));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tracking for shipping {ShippingId}", shippingId);
                return ServiceResult<ShippingResponse>.Failure("Có lỗi xảy ra khi cập nhật tracking");
            }
        }

        public async Task<ServiceResult<ShippingListResponse>> GetShippingOrdersAsync(ShippingFilterRequest filter)
        {
            try
            {
                var query = _context.ShippingOrders
                    .Include(s => s.Order)
                    .Include(s => s.ShippingTrackings)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query = query.Where(s => s.Status == filter.Status);
                }

                if (!string.IsNullOrEmpty(filter.ServiceType))
                {
                    query = query.Where(s => s.ServiceType == filter.ServiceType);
                }

                if (filter.DateFrom.HasValue)
                {
                    query = query.Where(s => s.CreatedAt >= filter.DateFrom.Value);
                }

                if (filter.DateTo.HasValue)
                {
                    query = query.Where(s => s.CreatedAt <= filter.DateTo.Value);
                }

                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(s => s.TrackingNumber.Contains(filter.Search) ||
                                           s.Order.OrderNumber.Contains(filter.Search));
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "status" => filter.SortOrder == "desc"
                        ? query.OrderByDescending(s => s.Status)
                        : query.OrderBy(s => s.Status),
                    "trackingnumber" => filter.SortOrder == "desc"
                        ? query.OrderByDescending(s => s.TrackingNumber)
                        : query.OrderBy(s => s.TrackingNumber),
                    _ => filter.SortOrder == "desc"
                        ? query.OrderByDescending(s => s.CreatedAt)
                        : query.OrderBy(s => s.CreatedAt)
                };

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize);

                var shippings = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var response = new ShippingListResponse
                {
                    Items = shippings.Select(MapToShippingResponse).ToList(),
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = filter.Page,
                    PageSize = filter.PageSize
                };

                return ServiceResult<ShippingListResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shipping orders");
                return ServiceResult<ShippingListResponse>.Failure("Có lỗi xảy ra khi lấy danh sách đơn vận chuyển");
            }
        }

        public async Task<ServiceResult<ShippingStatsResponse>> GetShippingStatsAsync()
        {
            try
            {
                var shippings = await _context.ShippingOrders.ToListAsync();

                var stats = new ShippingStatsResponse
                {
                    TotalShipments = shippings.Count,
                    PendingShipments = shippings.Count(s => s.Status == "PENDING"),
                    InTransitShipments = shippings.Count(s => s.Status == "IN_TRANSIT" || s.Status == "PICKED_UP"),
                    DeliveredShipments = shippings.Count(s => s.Status == "DELIVERED"),
                    FailedShipments = shippings.Count(s => s.Status == "FAILED" || s.Status == "RETURNED"),
                    TotalShippingFee = shippings.Sum(s => s.ShippingFee),
                    TotalCODAmount = shippings.Where(s => s.IsCOD).Sum(s => s.CODAmount),
                    StatusDistribution = shippings.GroupBy(s => s.Status)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                // Calculate average delivery days
                var deliveredOrders = shippings.Where(s => s.DeliveredAt.HasValue && s.PickedUpAt.HasValue).ToList();
                if (deliveredOrders.Any())
                {
                    stats.AverageDeliveryDays = deliveredOrders
                        .Average(s => (s.DeliveredAt.Value - s.PickedUpAt.Value).TotalDays);
                }

                // Calculate delivery success rate
                var completedShipments = shippings.Count(s => s.Status == "DELIVERED" || s.Status == "FAILED");
                if (completedShipments > 0)
                {
                    stats.DeliverySuccessRate = (double)stats.DeliveredShipments / completedShipments * 100;
                }

                return ServiceResult<ShippingStatsResponse>.Success(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shipping stats");
                return ServiceResult<ShippingStatsResponse>.Failure("Có lỗi xảy ra khi lấy thống kê vận chuyển");
            }
        }

        // Helper methods
        public async Task<string> GenerateTrackingNumberAsync()
        {
            var prefix = "SKH";
            var timestamp = DateTime.Now.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            
            string trackingNumber;
            bool isUnique;
            
            do
            {
                trackingNumber = $"{prefix}{timestamp}{random}";
                isUnique = !await _context.ShippingOrders
                    .AnyAsync(s => s.TrackingNumber == trackingNumber);
                
                if (!isUnique)
                {
                    random = new Random().Next(1000, 9999);
                }
            } while (!isUnique);

            return trackingNumber;
        }

        public async Task<decimal> CalculateStandardShippingFeeAsync(decimal weight, string fromProvince, string toProvince)
        {
            // Simplified shipping fee calculation
            var baseFee = BASE_SHIPPING_FEE;
            
            // Weight surcharge (per kg over 1kg)
            if (weight > 1)
            {
                baseFee += (weight - 1) * 10000; // 10k per additional kg
            }

            // Distance surcharge (simplified)
            if (fromProvince != toProvince)
            {
                baseFee += 15000; // 15k for inter-province
            }

            return await Task.FromResult(baseFee);
        }

        public async Task<decimal> CalculateCODFeeAsync(decimal codAmount)
        {
            var codFee = codAmount * COD_FEE_RATE;
            
            // Apply min/max limits
            if (codFee < MIN_COD_FEE) codFee = MIN_COD_FEE;
            if (codFee > MAX_COD_FEE) codFee = MAX_COD_FEE;

            return await Task.FromResult(codFee);
        }

        private ShippingResponse MapToShippingResponse(ShippingOrder shipping)
        {
            return new ShippingResponse
            {
                Id = shipping.Id,
                OrderId = shipping.OrderId,
                OrderNumber = shipping.Order?.OrderNumber ?? "",
                TrackingNumber = shipping.TrackingNumber,
                ServiceType = shipping.ServiceType,
                ServiceName = shipping.ServiceName,
                ShippingFee = shipping.ShippingFee,
                CODFee = shipping.CODFee,
                TotalFee = shipping.TotalFee,
                Status = shipping.Status,
                StatusDescription = shipping.StatusDescription,
                CreatedAt = shipping.CreatedAt,
                PickedUpAt = shipping.PickedUpAt,
                DeliveredAt = shipping.DeliveredAt,
                SenderName = shipping.SenderName,
                SenderPhone = shipping.SenderPhone,
                SenderAddress = shipping.SenderAddress,
                ReceiverName = shipping.ReceiverName,
                ReceiverPhone = shipping.ReceiverPhone,
                ReceiverAddress = shipping.ReceiverAddress,
                Weight = shipping.Weight,
                Dimensions = shipping.Dimensions,
                IsCOD = shipping.IsCOD,
                CODAmount = shipping.CODAmount,
                Notes = shipping.Notes,
                Trackings = shipping.ShippingTrackings?.Select(t => new ShippingTrackingResponse
                {
                    Id = t.Id,
                    Status = t.Status,
                    StatusDescription = t.StatusDescription,
                    UpdatedAt = t.UpdatedAt,
                    Location = t.Location,
                    Notes = t.Notes
                }).OrderByDescending(t => t.UpdatedAt).ToList() ?? new List<ShippingTrackingResponse>()
            };
        }

        // Implement remaining interface methods
        public async Task<ServiceResult<ShippingResponse>> GetShippingOrderAsync(int shippingId)
        {
            try
            {
                var shipping = await _context.ShippingOrders
                    .Include(s => s.Order)
                    .Include(s => s.ShippingTrackings)
                    .FirstOrDefaultAsync(s => s.Id == shippingId);

                if (shipping == null)
                {
                    return ServiceResult<ShippingResponse>.Failure("Không tìm thấy đơn vận chuyển");
                }

                return ServiceResult<ShippingResponse>.Success(MapToShippingResponse(shipping));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shipping order {ShippingId}", shippingId);
                return ServiceResult<ShippingResponse>.Failure("Có lỗi xảy ra khi lấy thông tin vận chuyển");
            }
        }

        public async Task<ServiceResult<ShippingResponse>> GetShippingByOrderIdAsync(int orderId)
        {
            try
            {
                var shipping = await _context.ShippingOrders
                    .Include(s => s.Order)
                    .Include(s => s.ShippingTrackings)
                    .FirstOrDefaultAsync(s => s.OrderId == orderId);

                if (shipping == null)
                {
                    return ServiceResult<ShippingResponse>.Failure("Không tìm thấy thông tin vận chuyển cho đơn hàng");
                }

                return ServiceResult<ShippingResponse>.Success(MapToShippingResponse(shipping));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shipping by order {OrderId}", orderId);
                return ServiceResult<ShippingResponse>.Failure("Có lỗi xảy ra khi lấy thông tin vận chuyển");
            }
        }

        public async Task<ServiceResult<ShippingResponse>> GetShippingByTrackingNumberAsync(string trackingNumber)
        {
            try
            {
                var shipping = await _context.ShippingOrders
                    .Include(s => s.Order)
                    .Include(s => s.ShippingTrackings)
                    .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);

                if (shipping == null)
                {
                    return ServiceResult<ShippingResponse>.Failure("Không tìm thấy đơn vận chuyển");
                }

                return ServiceResult<ShippingResponse>.Success(MapToShippingResponse(shipping));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shipping by tracking {TrackingNumber}", trackingNumber);
                return ServiceResult<ShippingResponse>.Failure("Có lỗi xảy ra khi tra cứu vận chuyển");
            }
        }

        public async Task<ServiceResult<List<ShippingTrackingResponse>>> GetTrackingHistoryAsync(string trackingNumber)
        {
            try
            {
                var shipping = await _context.ShippingOrders
                    .Include(s => s.ShippingTrackings)
                    .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);

                if (shipping == null)
                {
                    return ServiceResult<List<ShippingTrackingResponse>>.Failure("Không tìm thấy đơn vận chuyển");
                }

                var trackings = shipping.ShippingTrackings
                    .OrderByDescending(t => t.UpdatedAt)
                    .Select(t => new ShippingTrackingResponse
                    {
                        Id = t.Id,
                        Status = t.Status,
                        StatusDescription = t.StatusDescription,
                        UpdatedAt = t.UpdatedAt,
                        Location = t.Location,
                        Notes = t.Notes
                    }).ToList();

                return ServiceResult<List<ShippingTrackingResponse>>.Success(trackings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tracking history {TrackingNumber}", trackingNumber);
                return ServiceResult<List<ShippingTrackingResponse>>.Failure("Có lỗi xảy ra khi lấy lịch sử vận chuyển");
            }
        }

        public async Task<ServiceResult<bool>> UpdateShippingStatusAsync(int shippingId, string status, string description = null)
        {
            try
            {
                var shipping = await _context.ShippingOrders.FindAsync(shippingId);
                if (shipping == null)
                {
                    return ServiceResult<bool>.Failure("Không tìm thấy đơn vận chuyển");
                }

                shipping.Status = status;
                shipping.StatusDescription = description ?? GetStatusDescription(status);
                shipping.UpdatedAt = DateTime.UtcNow;

                if (status == "PICKED_UP" && !shipping.PickedUpAt.HasValue)
                {
                    shipping.PickedUpAt = DateTime.UtcNow;
                }
                else if (status == "DELIVERED" && !shipping.DeliveredAt.HasValue)
                {
                    shipping.DeliveredAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shipping status {ShippingId}", shippingId);
                return ServiceResult<bool>.Failure("Có lỗi xảy ra khi cập nhật trạng thái vận chuyển");
            }
        }

        // Simplified location services (Vietnam provinces/districts)
        public async Task<ServiceResult<List<ShippingLocationResponse>>> GetProvincesAsync()
        {
            var provinces = new List<ShippingLocationResponse>
            {
                new() { Code = "HCM", Name = "TP. Hồ Chí Minh", Type = "Province" },
                new() { Code = "HN", Name = "Hà Nội", Type = "Province" },
                new() { Code = "DN", Name = "Đà Nẵng", Type = "Province" },
                new() { Code = "CT", Name = "Cần Thơ", Type = "Province" },
                new() { Code = "BD", Name = "Bình Dương", Type = "Province" },
                new() { Code = "DL", Name = "Đồng Nai", Type = "Province" }
            };

            return await Task.FromResult(ServiceResult<List<ShippingLocationResponse>>.Success(provinces));
        }

        public async Task<ServiceResult<List<ShippingLocationResponse>>> GetDistrictsAsync(string provinceCode)
        {
            var districts = provinceCode switch
            {
                "HCM" => new List<ShippingLocationResponse>
                {
                    new() { Code = "Q1", Name = "Quận 1", Type = "District", ParentCode = "HCM" },
                    new() { Code = "Q3", Name = "Quận 3", Type = "District", ParentCode = "HCM" },
                    new() { Code = "Q5", Name = "Quận 5", Type = "District", ParentCode = "HCM" },
                    new() { Code = "Q7", Name = "Quận 7", Type = "District", ParentCode = "HCM" },
                    new() { Code = "TB", Name = "Thủ Đức", Type = "District", ParentCode = "HCM" }
                },
                "HN" => new List<ShippingLocationResponse>
                {
                    new() { Code = "HK", Name = "Hoàn Kiếm", Type = "District", ParentCode = "HN" },
                    new() { Code = "BD", Name = "Ba Đình", Type = "District", ParentCode = "HN" },
                    new() { Code = "CG", Name = "Cầu Giấy", Type = "District", ParentCode = "HN" }
                },
                _ => new List<ShippingLocationResponse>()
            };

            return await Task.FromResult(ServiceResult<List<ShippingLocationResponse>>.Success(districts));
        }

        public async Task<ServiceResult<List<ShippingLocationResponse>>> GetWardsAsync(string districtCode)
        {
            // Simplified - return sample wards
            var wards = new List<ShippingLocationResponse>
            {
                new() { Code = "P1", Name = "Phường 1", Type = "Ward", ParentCode = districtCode },
                new() { Code = "P2", Name = "Phường 2", Type = "Ward", ParentCode = districtCode },
                new() { Code = "P3", Name = "Phường 3", Type = "Ward", ParentCode = districtCode }
            };

            return await Task.FromResult(ServiceResult<List<ShippingLocationResponse>>.Success(wards));
        }

        private string GetStatusDescription(string status)
        {
            return status switch
            {
                "PENDING" => "Đang chuẩn bị hàng",
                "PICKED_UP" => "Đã lấy hàng",
                "IN_TRANSIT" => "Đang vận chuyển",
                "OUT_FOR_DELIVERY" => "Đang giao hàng",
                "DELIVERED" => "Đã giao hàng",
                "FAILED" => "Giao hàng thất bại",
                "RETURNED" => "Đã trả về",
                _ => "Không xác định"
            };
        }

        // Missing method implementations
        public async Task<ServiceResult<List<ShippingRateResponse>>> CalculateShippingFeeAsync(CalculateShippingRequest request)
        {
            return await CalculateShippingRatesAsync(request);
        }

        public async Task<ServiceResult<ShippingResponse>> GetShippingOrderByIdAsync(int shippingId)
        {
            return await GetShippingOrderAsync(shippingId);
        }

        public async Task<ServiceResult<ShippingResponse>> TrackShippingAsync(string trackingNumber)
        {
            return await GetShippingByTrackingNumberAsync(trackingNumber);
        }

        public async Task<ServiceResult<ShippingStatsResponse>> GetShippingStatsAsync(DateTime? fromDate, DateTime? toDate)
        {
            // Filtered stats implementation
            try
            {
                var query = _context.ShippingOrders.AsQueryable();

                if (fromDate.HasValue)
                    query = query.Where(s => s.CreatedAt >= fromDate.Value);
                
                if (toDate.HasValue)
                    query = query.Where(s => s.CreatedAt <= toDate.Value);

                var shippings = await query.ToListAsync();

                var stats = new ShippingStatsResponse
                {
                    TotalShipments = shippings.Count,
                    PendingShipments = shippings.Count(s => s.Status == "PENDING"),
                    InTransitShipments = shippings.Count(s => s.Status == "IN_TRANSIT" || s.Status == "PICKED_UP"),
                    DeliveredShipments = shippings.Count(s => s.Status == "DELIVERED"),
                    FailedShipments = shippings.Count(s => s.Status == "FAILED" || s.Status == "RETURNED"),
                    TotalShippingFee = shippings.Sum(s => s.ShippingFee),
                    TotalCODAmount = shippings.Where(s => s.IsCOD).Sum(s => s.CODAmount),
                    StatusDistribution = shippings.GroupBy(s => s.Status)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                return ServiceResult<ShippingStatsResponse>.Success(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filtered shipping stats");
                return ServiceResult<ShippingStatsResponse>.Failure("Có lỗi xảy ra khi lấy thống kê vận chuyển");
            }
        }

        public async Task<ServiceResult<bool>> CancelShippingOrderAsync(int shippingId, CancelShippingRequest request)
        {
            try
            {
                var shipping = await _context.ShippingOrders.FindAsync(shippingId);
                if (shipping == null)
                {
                    return ServiceResult<bool>.Failure("Không tìm thấy đơn vận chuyển");
                }

                if (shipping.Status == "DELIVERED" || shipping.Status == "CANCELLED")
                {
                    return ServiceResult<bool>.Failure("Không thể hủy đơn vận chuyển này");
                }

                shipping.Status = "CANCELLED";
                shipping.StatusDescription = "Đã hủy: " + request.CancelReason;
                shipping.UpdatedAt = DateTime.UtcNow;

                // Add tracking entry
                var tracking = new ShippingTracking
                {
                    ShippingOrderId = shippingId,
                    Status = "CANCELLED",
                    StatusDescription = "Đơn hàng đã bị hủy: " + request.CancelReason,
                    UpdatedAt = DateTime.UtcNow,
                    Location = "System",
                    Notes = request.Notes ?? "",
                    CreatedAt = DateTime.UtcNow
                };

                _context.ShippingTrackings.Add(tracking);
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling shipping order {ShippingId}", shippingId);
                return ServiceResult<bool>.Failure("Có lỗi xảy ra khi hủy đơn vận chuyển");
            }
        }

        public async Task<ServiceResult<bool>> AddTrackingEventAsync(int shippingId, AddTrackingEventRequest request)
        {
            try
            {
                var shipping = await _context.ShippingOrders.FindAsync(shippingId);
                if (shipping == null)
                {
                    return ServiceResult<bool>.Failure("Không tìm thấy đơn vận chuyển");
                }

                var tracking = new ShippingTracking
                {
                    ShippingOrderId = shippingId,
                    Status = request.Status,
                    StatusDescription = request.StatusDescription,
                    UpdatedAt = DateTime.UtcNow,
                    Location = request.Location,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ShippingTrackings.Add(tracking);

                // Update shipping status if needed
                shipping.Status = request.Status;
                shipping.StatusDescription = request.StatusDescription;
                shipping.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tracking event for shipping {ShippingId}", shippingId);
                return ServiceResult<bool>.Failure("Có lỗi xảy ra khi thêm sự kiện tracking");
            }
        }

        // Shipping Zones (Stub implementations)
        public async Task<ServiceResult<List<ShippingZoneResponse>>> GetShippingZonesAsync()
        {
            // Stub implementation
            var zones = new List<ShippingZoneResponse>();
            return await Task.FromResult(ServiceResult<List<ShippingZoneResponse>>.Success(zones));
        }

        public async Task<ServiceResult<ShippingZoneResponse>> GetShippingZoneByIdAsync(int zoneId)
        {
            // Stub implementation
            return await Task.FromResult(ServiceResult<ShippingZoneResponse>.Failure("Method not implemented"));
        }

        public async Task<ServiceResult<ShippingZoneResponse>> CreateShippingZoneAsync(CreateShippingZoneRequest request)
        {
            // Stub implementation
            return await Task.FromResult(ServiceResult<ShippingZoneResponse>.Failure("Method not implemented"));
        }

        public async Task<ServiceResult<ShippingZoneResponse>> UpdateShippingZoneAsync(int zoneId, UpdateShippingZoneRequest request)
        {
            // Stub implementation
            return await Task.FromResult(ServiceResult<ShippingZoneResponse>.Failure("Method not implemented"));
        }

        public async Task<ServiceResult<bool>> DeleteShippingZoneAsync(int zoneId)
        {
            // Stub implementation
            return await Task.FromResult(ServiceResult<bool>.Failure("Method not implemented"));
        }

        public async Task<ServiceResult<List<ShippingRateResponse>>> GetShippingRatesAsync()
        {
            // Stub implementation
            var rates = new List<ShippingRateResponse>();
            return await Task.FromResult(ServiceResult<List<ShippingRateResponse>>.Success(rates));
        }

        public async Task<ServiceResult<bool>> UpdateShippingRatesAsync(UpdateShippingRatesRequest request)
        {
            // Stub implementation
            return await Task.FromResult(ServiceResult<bool>.Failure("Method not implemented"));
        }

        public async Task<ServiceResult<byte[]>> ExportShippingReportAsync(ShippingReportRequest request)
        {
            // Stub implementation
            return await Task.FromResult(ServiceResult<byte[]>.Failure("Method not implemented"));
        }
    }
}