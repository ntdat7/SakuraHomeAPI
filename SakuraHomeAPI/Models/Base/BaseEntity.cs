using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Models.Entities.Catalog;

namespace SakuraHomeAPI.Models.Base
{
    #region Base Entity Classes

    /// <summary>
    /// Base entity with integer primary key only
    /// </summary>
    public abstract class BaseEntity : IEntity
    {
        [Key]
        public int Id { get; set; }
    }

    /// <summary>
    /// Base entity with Guid primary key only (for user-related entities)
    /// </summary>
    public abstract class BaseGuidEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    /// <summary>
    /// Entity with audit tracking capabilities
    /// </summary>
    public abstract class AuditableEntity : BaseEntity, IAuditable
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }

        // Navigation properties for audit tracking
        public virtual User CreatedByUser { get; set; }
        public virtual User UpdatedByUser { get; set; }
    }

    /// <summary>
    /// Guid-based entity with audit tracking
    /// </summary>
    public abstract class AuditableGuidEntity : BaseGuidEntity, IGuidAuditable
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Navigation properties for audit tracking
        public virtual User CreatedByUser { get; set; }
        public virtual User UpdatedByUser { get; set; }
    }

    /// <summary>
    /// Entity with soft delete functionality
    /// </summary>
    public abstract class SoftDeleteEntity : AuditableEntity, ISoftDelete
    {
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

        public virtual User DeletedByUser { get; set; }
    }

    /// <summary>
    /// Guid-based entity with soft delete functionality
    /// </summary>
    public abstract class SoftDeleteGuidEntity : AuditableGuidEntity, IGuidSoftDelete
    {
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }

        public virtual User DeletedByUser { get; set; }
    }

    /// <summary>
    /// Full-featured entity with all common properties
    /// Most entities should inherit from this
    /// </summary>
    public abstract class FullEntity : SoftDeleteEntity, IActivatable
    {
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Full-featured Guid entity
    /// </summary>
    public abstract class FullGuidEntity : SoftDeleteGuidEntity, IActivatable
    {
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Entity that supports multi-language translations
    /// </summary>
    public abstract class TranslatableEntity : FullEntity, ITranslatable
    {
        public virtual ICollection<Translation> Translations { get; set; } = new List<Translation>();
    }

    /// <summary>
    /// Content entity with SEO and slug support
    /// Perfect for public-facing content like products, categories, brands
    /// </summary>
    public abstract class ContentEntity : TranslatableEntity, ISlugifiable, ISeoFriendly, IOrderable
    {
        [MaxLength(255)]
        public string? Slug { get; set; } // ✅ Changed to nullable - Slug có thể chưa tạo

        [MaxLength(160)]
        public string? MetaTitle { get; set; } // ✅ Changed to nullable - SEO có thể chưa set

        [MaxLength(320)]
        public string? MetaDescription { get; set; } // ✅ Changed to nullable - CHÍNH LỖI HIỆN TẠI

        [MaxLength(500)]
        public string? MetaKeywords { get; set; } // ✅ Changed to nullable - Keywords có thể chưa có

        public int DisplayOrder { get; set; } = 0;
    }

    /// <summary>
    /// Simple entity for lookup tables
    /// </summary>
    public abstract class LookupEntity : FullEntity, IOrderable
    {
        [Required, MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string? Code { get; set; } // ✅ Changed to nullable - Code có thể không có

        public string? Description { get; set; } // ✅ Changed to nullable - Description có thể rỗng

        public int DisplayOrder { get; set; } = 0;
    }

    /// <summary>
    /// Entity for system logs and tracking
    /// </summary>
    public abstract class LogEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(45)]
        public string? IpAddress { get; set; } // ✅ Changed to nullable - IP có thể không có

        [MaxLength(500)]
        public string? UserAgent { get; set; } // ✅ Changed to nullable - UserAgent có thể rỗng

        public Guid? UserId { get; set; }
        public virtual User User { get; set; }
    }

    #endregion
}