namespace SakuraHomeAPI.DTOs.Shipping.Responses
{
    public class ShippingRateResponse
    {
        public string ServiceType { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public decimal CODFee { get; set; }
        public decimal TotalFee { get; set; }
        public int EstimatedDays { get; set; }
        public string EstimatedDelivery { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    public class ShippingResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public decimal ShippingFee { get; set; }
        public decimal CODFee { get; set; }
        public decimal TotalFee { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderPhone { get; set; } = string.Empty;
        public string SenderAddress { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string ReceiverAddress { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public string Dimensions { get; set; } = string.Empty;
        public bool IsCOD { get; set; }
        public decimal CODAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<ShippingTrackingResponse> Trackings { get; set; } = new List<ShippingTrackingResponse>();
    }

    public class ShippingTrackingResponse
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class ShippingListResponse
    {
        public List<ShippingResponse> Items { get; set; } = new List<ShippingResponse>();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    public class ShippingStatsResponse
    {
        public int TotalShipments { get; set; }
        public int PendingShipments { get; set; }
        public int InTransitShipments { get; set; }
        public int DeliveredShipments { get; set; }
        public int FailedShipments { get; set; }
        public decimal TotalShippingFee { get; set; }
        public decimal TotalCODAmount { get; set; }
        public double AverageDeliveryDays { get; set; }
        public double DeliverySuccessRate { get; set; }
        public Dictionary<string, int> StatusDistribution { get; set; } = new Dictionary<string, int>();
    }

    public class ShippingLocationResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Province, District, Ward
        public string ParentCode { get; set; } = string.Empty;
        public List<ShippingLocationResponse> Children { get; set; } = new List<ShippingLocationResponse>();
    }

    public class ShippingZoneResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> ProvinceCodes { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ShippingRateResponse> Rates { get; set; } = new List<ShippingRateResponse>();
    }
}