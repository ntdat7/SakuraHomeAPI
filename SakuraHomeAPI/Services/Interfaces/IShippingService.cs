using SakuraHomeAPI.DTOs.Shipping.Requests;
using SakuraHomeAPI.DTOs.Shipping.Responses;
using SakuraHomeAPI.Services.Common;

namespace SakuraHomeAPI.Services.Interfaces
{
    public interface IShippingService
    {
        // Rate calculation
        Task<ServiceResult<List<ShippingRateResponse>>> CalculateShippingRatesAsync(CalculateShippingRequest request);
        Task<ServiceResult<ShippingRateResponse>> GetStandardShippingRateAsync(CalculateShippingRequest request);
        Task<ServiceResult<List<ShippingRateResponse>>> CalculateShippingFeeAsync(CalculateShippingRequest request);
        
        // Shipping management
        Task<ServiceResult<ShippingResponse>> CreateShippingOrderAsync(CreateShippingRequest request);
        Task<ServiceResult<ShippingResponse>> GetShippingOrderAsync(int shippingId);
        Task<ServiceResult<ShippingResponse>> GetShippingByOrderIdAsync(int orderId);
        Task<ServiceResult<ShippingResponse>> GetShippingByTrackingNumberAsync(string trackingNumber);
        Task<ServiceResult<ShippingResponse>> GetShippingOrderByIdAsync(int shippingId);
        
        // Tracking
        Task<ServiceResult<ShippingResponse>> UpdateTrackingAsync(int shippingId, UpdateTrackingRequest request);
        Task<ServiceResult<List<ShippingTrackingResponse>>> GetTrackingHistoryAsync(string trackingNumber);
        Task<ServiceResult<ShippingResponse>> TrackShippingAsync(string trackingNumber);
        
        // Admin operations
        Task<ServiceResult<ShippingListResponse>> GetShippingOrdersAsync(ShippingFilterRequest filter);
        Task<ServiceResult<ShippingStatsResponse>> GetShippingStatsAsync();
        Task<ServiceResult<ShippingStatsResponse>> GetShippingStatsAsync(DateTime? fromDate, DateTime? toDate);
        Task<ServiceResult<bool>> UpdateShippingStatusAsync(int shippingId, string status, string? description = null);
        Task<ServiceResult<bool>> CancelShippingOrderAsync(int shippingId, CancelShippingRequest request);
        Task<ServiceResult<bool>> AddTrackingEventAsync(int shippingId, AddTrackingEventRequest request);
        
        // Shipping Zones
        Task<ServiceResult<List<ShippingZoneResponse>>> GetShippingZonesAsync();
        Task<ServiceResult<ShippingZoneResponse>> GetShippingZoneByIdAsync(int zoneId);
        Task<ServiceResult<ShippingZoneResponse>> CreateShippingZoneAsync(CreateShippingZoneRequest request);
        Task<ServiceResult<ShippingZoneResponse>> UpdateShippingZoneAsync(int zoneId, UpdateShippingZoneRequest request);
        Task<ServiceResult<bool>> DeleteShippingZoneAsync(int zoneId);
        
        // Shipping Rates
        Task<ServiceResult<List<ShippingRateResponse>>> GetShippingRatesAsync();
        Task<ServiceResult<bool>> UpdateShippingRatesAsync(UpdateShippingRatesRequest request);
        
        // Reports
        Task<ServiceResult<byte[]>> ExportShippingReportAsync(ShippingReportRequest request);
        
        // Location services
        Task<ServiceResult<List<ShippingLocationResponse>>> GetProvincesAsync();
        Task<ServiceResult<List<ShippingLocationResponse>>> GetDistrictsAsync(string provinceCode);
        Task<ServiceResult<List<ShippingLocationResponse>>> GetWardsAsync(string districtCode);
        
        // Utility methods
        Task<string> GenerateTrackingNumberAsync();
        Task<decimal> CalculateStandardShippingFeeAsync(decimal weight, string fromProvince, string toProvince);
        Task<decimal> CalculateCODFeeAsync(decimal codAmount);
    }
}