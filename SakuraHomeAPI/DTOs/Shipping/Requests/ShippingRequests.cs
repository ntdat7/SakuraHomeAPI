namespace SakuraHomeAPI.DTOs.Shipping.Requests
{
    public class CalculateShippingRequest
    {
        public string FromProvinceCode { get; set; } = string.Empty;
        public string FromDistrictCode { get; set; } = string.Empty;
        public string ToProvinceCode { get; set; } = string.Empty;
        public string ToDistrictCode { get; set; } = string.Empty;
        public decimal Weight { get; set; } // kg
        public decimal Length { get; set; } // cm
        public decimal Width { get; set; } // cm
        public decimal Height { get; set; } // cm
        public decimal OrderValue { get; set; }
        public string ServiceType { get; set; } = "STANDARD"; // STANDARD, EXPRESS
    }

    public class CreateShippingRequest
    {
        public int OrderId { get; set; }
        public string ServiceType { get; set; } = "STANDARD";
        public string Notes { get; set; } = string.Empty;
        public bool IsCOD { get; set; } = true;
        public decimal CODAmount { get; set; }
    }

    public class CreateShippingOrderRequest
    {
        public int OrderId { get; set; }
        public string? ServiceType { get; set; } = "STANDARD";
        public string? Notes { get; set; } = string.Empty;
        public bool IsCOD { get; set; } = true;
        public decimal CODAmount { get; set; }
        public decimal Weight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string ReceiverAddress { get; set; } = string.Empty;
        public string ProvinceCode { get; set; } = string.Empty;
        public string DistrictCode { get; set; } = string.Empty;
        public string WardCode { get; set; } = string.Empty;
    }

    public class UpdateShippingStatusRequest
    {
        public string Status { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class CancelShippingOrderRequest
    {
        public string? CancelReason { get; set; } = string.Empty;
        public string? Notes { get; set; } = string.Empty;
    }

    public class ShippingOrderFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Status { get; set; } = string.Empty;
        public string? ServiceType { get; set; } = string.Empty;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Search { get; set; } = string.Empty; // Order number or tracking number
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc";
    }

    public class AddTrackingEventRequest
    {
        public string Status { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime? EventTime { get; set; }
    }

    public class CreateShippingZoneRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> ProvinceCodes { get; set; } = new List<string>();
        public bool IsActive { get; set; } = true;
    }

    public class UpdateShippingZoneRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> ProvinceCodes { get; set; } = new List<string>();
        public bool IsActive { get; set; }
    }

    public class UpdateShippingRatesRequest
    {
        public List<ShippingRateRequest> Rates { get; set; } = new List<ShippingRateRequest>();
    }

    public class ShippingRateRequest
    {
        public string ServiceType { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal FreeShippingThreshold { get; set; }
        public decimal MaxWeight { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ShippingStatsRequest
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string GroupBy { get; set; } = "day"; // day, week, month
    }

    public class ExportShippingReportRequest
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Status { get; set; } = string.Empty;
        public string? ServiceType { get; set; } = string.Empty;
        public string Format { get; set; } = "excel"; // excel, csv, pdf
    }

    public class UpdateTrackingRequest
    {
        public string TrackingNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class ShippingFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string Status { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string Search { get; set; } = string.Empty; // Order number or tracking number
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc";
    }

    public class CancelShippingRequest
    {
        public string CancelReason { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class ShippingReportRequest
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string Format { get; set; } = "excel"; // excel, csv, pdf
    }
}