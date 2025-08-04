using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities.Identity
{
    /// <summary>
    /// Refresh token entity for JWT token management
    /// </summary>
    [Table("RefreshTokens")]
    public class RefreshToken : BaseEntity
    {
        [Required]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        [MaxLength(500)]
        public string? ReplacedByToken { get; set; }

        [MaxLength(500)]
        public string? ReasonRevoked { get; set; }

        [MaxLength(45)]
        public string? CreatedByIp { get; set; }

        [MaxLength(45)]
        public string? RevokedByIp { get; set; }

        // Navigation Properties
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Computed Properties
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        [NotMapped]
        public bool IsRevoked => RevokedAt != null;

        [NotMapped]
        public bool IsActive => !IsRevoked && !IsExpired;

        // Methods
        public void Revoke(string? reason = null, string? revokedByIp = null, string? replacedByToken = null)
        {
            RevokedAt = DateTime.UtcNow;
            ReasonRevoked = reason;
            RevokedByIp = revokedByIp;
            ReplacedByToken = replacedByToken;
        }
    }
}