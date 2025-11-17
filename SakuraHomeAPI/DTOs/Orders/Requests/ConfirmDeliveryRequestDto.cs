using System.ComponentModel.DataAnnotations;

namespace SakuraHomeAPI.DTOs.Orders.Requests
{
    /// <summary>
    /// Confirm delivery request DTO
    /// </summary>
    public class ConfirmDeliveryRequestDto
    {
        /// <summary>
        /// Đã nhận được hàng hay chưa?
        /// true = Đã nhận, false = Chưa nhận (cần liên hệ)
        /// </summary>
        [Required]
        public bool IsReceived { get; set; }

        /// <summary>
        /// Ghi chú (bắt buộc nếu chưa nhận được hàng)
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}