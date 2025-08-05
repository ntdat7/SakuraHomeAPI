using System.ComponentModel.DataAnnotations;

namespace SakuraHomeAPI.DTOs.Wishlist.Requests
{
    public class CreateWishlistRequestDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        public bool IsPublic { get; set; } = false;
    }

    public class UpdateWishlistRequestDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        public bool IsPublic { get; set; }
    }

    public class AddToWishlistRequestDto
    {
        [Required]
        public int ProductId { get; set; }
        
        public int? ProductVariantId { get; set; }
        
        public int? WishlistId { get; set; } // If null, add to default wishlist
        
        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class RemoveFromWishlistRequestDto
    {
        [Required]
        public int WishlistItemId { get; set; }
    }

    public class BulkAddToWishlistRequestDto
    {
        [Required]
        public List<AddToWishlistRequestDto> Items { get; set; } = new();
        
        public int? WishlistId { get; set; }
    }

    public class BulkRemoveFromWishlistRequestDto
    {
        [Required]
        public List<int> WishlistItemIds { get; set; } = new();
    }
}