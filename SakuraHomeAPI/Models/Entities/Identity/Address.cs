using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.Models.Entities.Identity
{
    /// <summary>
    /// User address entity - Vietnam only, FE sends IDs
    /// </summary>
    [Table("Addresses")]
    public class Address : BaseEntity, IAuditable, ISoftDelete
    {
        public Guid UserId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(20), Phone]
        public string Phone { get; set; }

        [Required, MaxLength(255)]
        public string AddressLine1 { get; set; } // Số nhà, tên đường

        [MaxLength(255)]
        public string AddressLine2 { get; set; } // Tòa nhà, căn hộ

        // Vietnam Address Structure - IDs only
        [Required]
        public int ProvinceId { get; set; }  // ID from FE dropdown

        [Required]
        public int WardId { get; set; }      // ID from FE dropdown

        [MaxLength(20)]
        public string PostalCode { get; set; } // Optional

        [MaxLength(100)]
        public string Country { get; set; } = "Vietnam";

        public bool IsDefault { get; set; } = false;
        public AddressType Type { get; set; } = AddressType.Both;

        [MaxLength(500)]
        public string Notes { get; set; }

        // Audit properties
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }

        // Soft delete properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

        // Navigation Properties - ONLY User
        public virtual User User { get; set; }

        #region Computed Properties

        [NotMapped]
        public string ShortAddress => AddressLine1;

        // Note: FullAddress with province/ward names requires service layer lookup
        // Cannot be computed property since we don't have navigation to Province/Ward

        #endregion
    }
}