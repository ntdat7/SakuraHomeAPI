using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.Models.Entities.Catalog
{
    /// <summary>
    /// Brand entity for Japanese product brands
    /// </summary>
    [Table("Brands")]
    public class Brand : ContentEntity, IFeaturable
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string LogoUrl { get; set; }

        [MaxLength(100)]
        public string Country { get; set; } = "Japan";

        [MaxLength(500)]
        public string Website { get; set; }

        [MaxLength(255)]
        public string ContactEmail { get; set; }

        [MaxLength(20)]
        public string ContactPhone { get; set; }

        // Brand specific fields
        public DateTime? FoundedYear { get; set; }

        [MaxLength(500)]
        public string Headquarters { get; set; }

        public bool IsFeatured { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public bool IsOfficial { get; set; } = false; // Official brand store

        // Statistics
        public int ProductCount { get; set; } = 0;
        public double AverageRating { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;
        public int FollowerCount { get; set; } = 0;

        // Social media
        [MaxLength(255)]
        public string FacebookUrl { get; set; }
        [MaxLength(255)]
        public string InstagramUrl { get; set; }
        [MaxLength(255)]
        public string TwitterUrl { get; set; }
        [MaxLength(255)]
        public string YoutubeUrl { get; set; }

        // Navigation
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        #region Computed Properties
        [NotMapped]
        public string DisplayName => !string.IsNullOrEmpty(Name) ? Name : "Unknown Brand";

        [NotMapped]
        public bool HasSocialMedia =>
            !string.IsNullOrEmpty(FacebookUrl) ||
            !string.IsNullOrEmpty(InstagramUrl) ||
            !string.IsNullOrEmpty(TwitterUrl) ||
            !string.IsNullOrEmpty(YoutubeUrl);
        #endregion

        #region Methods
        /// <summary>
        /// Update brand statistics
        /// </summary>
        public void UpdateStatistics()
        {
            if (Products?.Any() == true)
            {
                ProductCount = Products.Count(p => p.IsActive && !p.IsDeleted);
                var activeProducts = Products.Where(p => p.IsActive && !p.IsDeleted);
                if (activeProducts.Any())
                {
                    AverageRating = (double) activeProducts.Average(p => p.Rating);
                    ReviewCount = activeProducts.Sum(p => p.ReviewCount);
                }
            }
        }
        #endregion
    }
}
