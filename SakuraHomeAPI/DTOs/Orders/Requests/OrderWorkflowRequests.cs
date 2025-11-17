using System.ComponentModel.DataAnnotations;

namespace SakuraHomeAPI.DTOs.Orders.Requests
{
    /// <summary>
    /// Pack order request DTO
    /// </summary>
    public class PackOrderRequestDto
    {
        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Out for delivery request DTO
    /// </summary>
    public class OutForDeliveryRequestDto
    {
        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime? EstimatedArrival { get; set; }
    }

    /// <summary>
    /// Delivery failed request DTO
    /// </summary>
    public class DeliveryFailedRequestDto
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public DateTime? RetryDate { get; set; }
    }
}