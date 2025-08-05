namespace SakuraHomeAPI.Models.Enums
{
    /// <summary>
    /// User roles in the system
    /// </summary>
    public enum UserRole
    {
        Customer = 1,
        Staff = 2,
        Admin = 3,
        SuperAdmin = 4
    }

    /// <summary>
    /// Gender options
    /// </summary>
    public enum Gender
    {
        Unknown = 0,        // Không xác định (mặc định)
        Male = 1,
        Female = 2,
        Other = 3,
        PreferNotToSay = 4
    }

    /// <summary>
    /// User loyalty tiers based on spending
    /// </summary>
    public enum UserTier
    {
        Bronze = 1,     // 0 - 1M VND
        Silver = 2,     // 1M - 5M VND
        Gold = 3,       // 5M - 10M VND
        Platinum = 4,   // 10M - 20M VND
        Diamond = 5     // 20M+ VND
    }

    /// <summary>
    /// Address types
    /// </summary>
    public enum AddressType
    {
        Billing = 1,
        Shipping = 2,
        Both = 3
    }

    /// <summary>
    /// Account status
    /// </summary>
    public enum AccountStatus
    {
        Pending = 1,        // Chờ xác thực email
        Active = 2,         // Hoạt động bình thường
        Suspended = 3,      // Bị tạm khóa
        Banned = 4,         // Bị cấm vĩnh viễn
        Inactive = 5        // Không hoạt động
    }

    /// <summary>
    /// Login providers
    /// </summary>
    public enum LoginProvider
    {
        Local = 1,          // Đăng ký trực tiếp
        Google = 2,         // Đăng nhập bằng Google
        Facebook = 3,       // Đăng nhập bằng Facebook
        Apple = 4           // Đăng nhập bằng Apple
    }
}