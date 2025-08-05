using System.ComponentModel.DataAnnotations;

namespace SakuraHomeAPI.DTOs.Cart.Requests
{
    public class AddToCartRequestDto
    {
        [Required]
        public int ProductId { get; set; }
        
        public int? ProductVariantId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
        
        public string? CustomOptions { get; set; }
        public string? GiftMessage { get; set; }
        public bool IsGift { get; set; } = false;
    }

    public class UpdateCartItemRequestDto
    {
        [Required]
        public int ProductId { get; set; }
        
        public int? ProductVariantId { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or more")]
        public int Quantity { get; set; }
        
        public string? CustomOptions { get; set; }
        public string? GiftMessage { get; set; }
        public bool IsGift { get; set; }
    }

    public class RemoveFromCartRequestDto
    {
        [Required]
        public int ProductId { get; set; }
        
        public int? ProductVariantId { get; set; }
    }

    public class BulkUpdateCartRequestDto
    {
        [Required]
        public List<UpdateCartItemRequestDto> Items { get; set; } = new();
    }
}