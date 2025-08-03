using System.ComponentModel.DataAnnotations;

namespace SakuraHomeAPI.DTOs.Products.Requests
{
    /// <summary>
    /// Update product request
    /// </summary>
    public class UpdateProductRequestDto : CreateProductRequestDto
    {
        [MaxLength(500, ErrorMessage = "Update reason cannot exceed 500 characters")]
        public string? UpdateReason { get; set; }
    }
}