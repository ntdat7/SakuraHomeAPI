using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;

namespace SakuraHomeAPI.Models.Entities.Catalog
{
    /// <summary>
    /// Translation entity for multi-language support
    /// </summary>
    [Table("Translations")]
    public class Translation : AuditableEntity
    {
        [Required, MaxLength(50)]
        public string EntityType { get; set; } // "Product", "Category", "Brand", etc.

        [Required]
        public int EntityId { get; set; }

        [Required, MaxLength(50)]
        public string FieldName { get; set; } // "Name", "Description", etc.

        [Required, MaxLength(5)]
        public string Language { get; set; } // "vi", "en", "ja"

        [Required]
        public string Value { get; set; }

        public bool IsApproved { get; set; } = false;
        public bool IsAutoTranslated { get; set; } = false;
        [MaxLength(50)]
        public string TranslationSource { get; set; } // "manual", "google", "deepl"
        public double? ConfidenceScore { get; set; } // 0-1 for auto translations

        #region Computed Properties

        [NotMapped]
        public string EntityKey => $"{EntityType}:{EntityId}:{FieldName}";
        [NotMapped]
        public bool NeedsReview => IsAutoTranslated && !IsApproved;

        #endregion

        #region Methods

        public static string CreateKey(string entityType, int entityId, string fieldName, string language)
            => $"{entityType}:{entityId}:{fieldName}:{language}";

        public bool IsComplete() => !string.IsNullOrWhiteSpace(Value);
        public void Approve(int? approvedBy = null)
        {
            IsApproved = true;
            UpdatedBy = approvedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
