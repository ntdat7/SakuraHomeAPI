using Microsoft.AspNetCore.Identity;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Orders;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Models.Entities.Reviews;
using SakuraHomeAPI.Models.Entities.UserCart;
using SakuraHomeAPI.Models.Entities.UserWishlist;
using SakuraHomeAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities.Identity
{
    /// <summary>
    /// User entity representing customers and admin users
    /// Kế thừa từ IdentityUser<Guid> để tương thích với ASP.NET Core Identity
    /// </summary>
    [Table("Users")]
    public class User : IdentityUser<Guid>, IGuidAuditable, IGuidSoftDelete, IActivatable
    {
        #region Identity Properties Override
        // Email đã có sẵn từ IdentityUser, chỉ cần override validation
        [Required, MaxLength(255), EmailAddress]
        public override string Email { get; set; } = string.Empty;

        // PhoneNumber đã có sẵn từ IdentityUser
        [MaxLength(20), Phone]
        public override string? PhoneNumber { get; set; }
        #endregion

        #region Basic Information

        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }

        [MaxLength(500)]
        public string? Avatar { get; set; }

        #endregion

        #region Preferences & Settings

        [MaxLength(5)]
        public string PreferredLanguage { get; set; } = "vi";

        [MaxLength(3)]
        public string PreferredCurrency { get; set; } = "VND";

        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public bool PushNotifications { get; set; } = true;

        #endregion

        #region Account Status & Security

        public AccountStatus Status { get; set; } = AccountStatus.Pending;
        public UserRole Role { get; set; } = UserRole.Customer;
        public LoginProvider Provider { get; set; } = LoginProvider.Local;

        [MaxLength(255)]
        public string? ExternalId { get; set; }

        // EmailConfirmed đã có sẵn từ IdentityUser
        public bool EmailVerified
        {
            get => EmailConfirmed;
            set => EmailConfirmed = value;
        }

        public DateTime? EmailVerifiedAt { get; set; }

        [MaxLength(100)]
        public string? EmailVerificationToken { get; set; }

        // PhoneNumberConfirmed đã có sẵn từ IdentityUser
        public bool PhoneVerified
        {
            get => PhoneNumberConfirmed;
            set => PhoneNumberConfirmed = value;
        }

        public DateTime? PhoneVerifiedAt { get; set; }

        [MaxLength(10)]
        public string? PhoneVerificationCode { get; set; }

        [MaxLength(100)]
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpires { get; set; }

        // AccessFailedCount đã có sẵn từ IdentityUser
        public int FailedLoginAttempts
        {
            get => AccessFailedCount;
            set => AccessFailedCount = value;
        }

        // LockoutEnd đã có sẵn từ IdentityUser
        public DateTime? LastLoginAt { get; set; }

        [MaxLength(45)]
        public string? LastLoginIp { get; set; }

        #endregion

        #region Loyalty & Points System

        public UserTier Tier { get; set; } = UserTier.Bronze;
        public int Points { get; set; } = 0;
        public decimal TotalSpent { get; set; } = 0;
        public int TotalOrders { get; set; } = 0;
        public DateTime? LastOrderDate { get; set; }

        #endregion

        #region Audit Properties (từ IAuditable) - FIXED: Changed to Guid to match User key type

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        #endregion

        #region Soft Delete Properties (từ ISoftDelete) - FIXED: Changed to Guid

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }

        #endregion

        #region Activatable Properties (từ IActivatable)

        public bool IsActive { get; set; } = true;

        #endregion

        #region Computed Properties

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [NotMapped]
        public int Age
        {
            get
            {
                if (!DateOfBirth.HasValue) return 0;
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Value.Year;
                if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        [NotMapped]
        public bool IsLocked => LockoutEnd.HasValue && LockoutEnd > DateTime.UtcNow;

        [NotMapped]
        public bool CanResetPassword =>
            !string.IsNullOrEmpty(PasswordResetToken) &&
            PasswordResetExpires.HasValue &&
            PasswordResetExpires > DateTime.UtcNow;

        #endregion

        #region Navigation Properties

        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual Wishlist? Wishlist { get; set; }
        public virtual Cart? Cart { get; set; }
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<ContactMessage> ContactMessages { get; set; } = new List<ContactMessage>();
        public virtual ICollection<UserActivity> Activities { get; set; } = new List<UserActivity>();
        public virtual ICollection<ProductView> ProductViews { get; set; } = new List<ProductView>();
        public virtual ICollection<SearchLog> SearchLogs { get; set; } = new List<SearchLog>();

        // Navigation properties cho audit - FIXED: Changed to Guid
        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }

        [ForeignKey("UpdatedBy")]
        public virtual User? UpdatedByUser { get; set; }

        [ForeignKey("DeletedBy")]
        public virtual User? DeletedByUser { get; set; }

        #endregion

        #region Methods

        public bool HasRole(UserRole role) => Role >= role;
        public bool IsAdmin() => Role >= UserRole.Admin;

        public void UpdateTier()
        {
            Tier = TotalSpent switch
            {
                >= 20000000 => UserTier.Diamond,
                >= 10000000 => UserTier.Platinum,
                >= 5000000 => UserTier.Gold,
                >= 1000000 => UserTier.Silver,
                _ => UserTier.Bronze
            };
        }

        public void GenerateEmailVerificationToken()
            => EmailVerificationToken = Guid.NewGuid().ToString("N");

        public void GeneratePasswordResetToken(int expirationHours = 1)
        {
            PasswordResetToken = Guid.NewGuid().ToString("N");
            PasswordResetExpires = DateTime.UtcNow.AddHours(expirationHours);
        }

        public void LockAccount(int minutes = 30)
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(minutes);
        }

        public void UnlockAccount()
        {
            LockoutEnd = null;
            AccessFailedCount = 0; // Sử dụng AccessFailedCount thay vì FailedLoginAttempts
        }

        #endregion

        #region Constructor

        public User()
        {
            // Tự động tạo Id khi khởi tạo
            Id = Guid.NewGuid();
        }

        #endregion
    }
}