using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.Models.Entities.Identity
{
    /// <summary>
    /// User address entity
    /// </summary>
    [Table("Addresses")]
    public class Address : BaseEntity, IAuditable, ISoftDelete
    {
        public Guid UserId { get; set; } // Changed to Guid to match User

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(20), Phone]
        public string Phone { get; set; }

        [Required, MaxLength(255)]
        public string AddressLine1 { get; set; }

        [MaxLength(255)]
        public string AddressLine2 { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; }

        [MaxLength(100)]
        public string State { get; set; }

        [MaxLength(20)]
        public string PostalCode { get; set; }

        [MaxLength(100)]
        public string Country { get; set; } = "Vietnam";

        public bool IsDefault { get; set; } = false;
        public AddressType Type { get; set; } = AddressType.Both;

        [MaxLength(500)]
        public string Notes { get; set; }

        // Audit properties (implementing IAuditable with int)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }

        // Soft delete properties (implementing ISoftDelete with int)
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

        // Navigation
        public virtual User User { get; set; }

        #region Computed Properties

        [NotMapped]
        public string FullAddress
        {
            get
            {
                var parts = new List<string>();
                if (!string.IsNullOrEmpty(AddressLine1)) parts.Add(AddressLine1);
                if (!string.IsNullOrEmpty(AddressLine2)) parts.Add(AddressLine2);
                if (!string.IsNullOrEmpty(City)) parts.Add(City);
                if (!string.IsNullOrEmpty(State)) parts.Add(State);
                if (!string.IsNullOrEmpty(PostalCode)) parts.Add(PostalCode);
                if (!string.IsNullOrEmpty(Country)) parts.Add(Country);
                return string.Join(", ", parts);
            }
        }

        #endregion
    }
}
