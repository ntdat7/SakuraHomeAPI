using System.ComponentModel.DataAnnotations;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Products.Requests
{
    /// <summary>
    /// Update stock request
    /// </summary>
    public class UpdateStockRequestDto
    {
        [Required(ErrorMessage = "New stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "New stock must be greater than or equal to 0")]
        public int NewStock { get; set; }

        [Required(ErrorMessage = "Action is required")]
        public InventoryAction Action { get; set; }

        [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? Reason { get; set; }

        [MaxLength(50, ErrorMessage = "Batch number cannot exceed 50 characters")]
        public string? BatchNumber { get; set; }

        [MaxLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
        public string? Location { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit cost must be greater than or equal to 0")]
        public decimal? UnitCost { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit price must be greater than or equal to 0")]
        public decimal? UnitPrice { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int? ProductVariantId { get; set; }

        [MaxLength(200, ErrorMessage = "Notes cannot exceed 200 characters")]
        public string? Notes { get; set; }
    }
}