namespace SakuraHomeAPI.Models.Enums
{
    /// <summary>
    /// Notification delivery channels
    /// </summary>
    public enum NotificationChannel
    {
        InApp = 1,              // Trong ứng dụng
        Email = 2,              // Email
        SMS = 3,                // Tin nhắn
        Push = 4,               // Push notification
        All = 5                 // Tất cả kênh
    }

    /// <summary>
    /// Email status for queue processing
    /// </summary>
    public enum EmailStatus
    {
        Pending = 1,            // Chờ gửi
        Sending = 2,            // Đang gửi
        Sent = 3,               // Đã gửi
        Failed = 4,             // Gửi thất bại
        Cancelled = 5,          // Đã hủy
        Scheduled = 6           // Đã lên lịch
    }

    /// <summary>
    /// System setting data types
    /// </summary>
    public enum SettingType
    {
        String = 1,
        Number = 2,
        Boolean = 3,
        Json = 4,
        Date = 5,
        File = 6
    }

    /// <summary>
    /// Contact message categories
    /// </summary>
    public enum ContactCategory
    {
        General = 1,            // Tổng quát
        Order = 2,              // Đơn hàng
        Product = 3,            // Sản phẩm
        Payment = 4,            // Thanh toán
        Shipping = 5,           // Vận chuyển
        Return = 6,             // Trả hàng
        Technical = 7,          // Kỹ thuật
        Complaint = 8,          // Khiếu nại
        Suggestion = 9          // Góp ý
    }

    /// <summary>
    /// Contact message status
    /// </summary>
    public enum ContactStatus
    {
        New = 1,                // Mới
        InProgress = 2,         // Đang xử lý
        Waiting = 3,            // Chờ phản hồi
        Resolved = 4,           // Đã giải quyết
        Closed = 5,             // Đã đóng
        Escalated = 6           // Chuyển lên cấp cao
    }

    /// <summary>
    /// Priority levels
    /// </summary>
    public enum Priority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Urgent = 4,
        Critical = 5
    }

    /// <summary>
    /// Notification priority levels
    /// </summary>
    public enum NotificationPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4,
        Critical = 5
    }

    /// <summary>
    /// Banner positions
    /// </summary>
    public enum BannerPosition
    {
        MainSlider = 1,         // Slider chính
        SideBar = 2,            // Thanh bên
        CategoryPage = 3,       // Trang danh mục
        ProductPage = 4,        // Trang sản phẩm
        Header = 5,             // Header
        Footer = 6,             // Footer
        Popup = 7,              // Popup
        FloatingButton = 8      // Nút nổi
    }

    /// <summary>
    /// File types for uploads
    /// </summary>
    public enum FileType
    {
        Image = 1,              // Hình ảnh
        Document = 2,           // Tài liệu
        Video = 3,              // Video
        Audio = 4,              // Âm thanh
        Archive = 5,            // File nén
        Other = 6               // Khác
    }

    /// <summary>
    /// Log levels
    /// </summary>
    public enum LogLevel
    {
        Trace = 1,
        Debug = 2,
        Information = 3,
        Warning = 4,
        Error = 5,
        Critical = 6
    }

    /// <summary>
    /// Activity types for user tracking
    /// </summary>
    public enum ActivityType
    {
        Login = 1,              // Đăng nhập
        Logout = 2,             // Đăng xuất
        ViewProduct = 3,        // Xem sản phẩm
        AddToCart = 4,          // Thêm vào giỏ hàng
        RemoveFromCart = 5,     // Xóa khỏi giỏ hàng
        AddToWishlist = 6,      // Thêm vào yêu thích
        PlaceOrder = 7,         // Đặt hàng
        CancelOrder = 8,        // Hủy đơn hàng
        WriteReview = 9,        // Viết đánh giá
        UpdateProfile = 10,     // Cập nhật profile
        ChangePassword = 11,    // Đổi mật khẩu
        Search = 12,            // Tìm kiếm
        Contact = 13,           // Liên hệ
        Register = 14,          // Đăng ký
        ResetPassword = 15      // Đặt lại mật khẩu
    }

    /// <summary>
    /// Supported languages
    /// </summary>
    public enum SupportedLanguage
    {
        Vietnamese = 1,         // vi
        English = 2,            // en
        Japanese = 3            // ja
    }

    /// <summary>
    /// Currency codes
    /// </summary>
    public enum Currency
    {
        VND = 1,                // Vietnamese Dong
        USD = 2,                // US Dollar
        JPY = 3,                // Japanese Yen
        EUR = 4                 // Euro
    }

    /// <summary>
    /// Discount scope for coupons
    /// </summary>
    public enum DiscountScope
    {
        AllProducts = 1,        // Tất cả sản phẩm
        Category = 2,           // Theo danh mục
        Brand = 3,              // Theo thương hiệu
        Product = 4,            // Sản phẩm cụ thể
        FirstOrder = 5,         // Đơn hàng đầu tiên
        MinimumAmount = 6       // Đơn hàng tối thiểu
    }

    /// <summary>
    /// Notification types
    /// </summary>
    public enum NotificationType
    {
        General = 1,            // Thông báo chung
        Order = 2,              // Đơn hàng
        Payment = 3,            // Thanh toán
        Shipping = 4,           // Vận chuyển
        Product = 5,            // Sản phẩm
        Promotion = 6,          // Khuyến mãi
        System = 7,             // Hệ thống
        Security = 8,           // Bảo mật
        Marketing = 9,          // Marketing
        Reminder = 10,          // Nhắc nhở
        Alert = 11,             // Cảnh báo
        Newsletter = 12,        // Bản tin
        Info = 13,              // Thông tin
        Warning = 14,           // Cảnh báo
        Success = 15,           // Thành công
        Error = 16              // Lỗi
    }

    /// <summary>
    /// Email types
    /// </summary>
    public enum EmailType
    {
        General = 1,            // Email chung
        Welcome = 2,            // Chào mừng
        OrderConfirmation = 3,  // Xác nhận đơn hàng
        OrderUpdate = 4,        // Cập nhật đơn hàng
        PasswordReset = 5,      // Đặt lại mật khẩu
        EmailVerification = 6,  // Xác thực email
        Promotional = 7,        // Khuyến mãi
        Newsletter = 8,         // Bản tin
        SystemAlert = 9,        // Cảnh báo hệ thống
        Receipt = 10,           // Hóa đơn
        Shipping = 11,          // Vận chuyển
        Return = 12,            // Trả hàng
        Survey = 13,            // Khảo sát
        Announcement = 14       // Thông báo
    }

    /// <summary>
    /// Email delivery status
    /// </summary>
    public enum EmailDeliveryStatus
    {
        Pending = 1,            // Chờ gửi
        Sending = 2,            // Đang gửi
        Sent = 3,               // Đã gửi
        Delivered = 4,          // Đã nhận
        Failed = 5,             // Thất bại
        Bounced = 6,            // Bị trả về
        Spam = 7,               // Spam
        Unsubscribed = 8,       // Hủy đăng ký
        Retry = 9               // Thử lại
    }

    /// <summary>
    /// SMS status
    /// </summary>
    public enum SmsStatus
    {
        Pending = 1,            // Chờ gửi
        Sending = 2,            // Đang gửi
        Sent = 3,               // Đã gửi
        Delivered = 4,          // Đã nhận
        Failed = 5,             // Thất bại
        Expired = 6,            // Hết hạn
        Rejected = 7,           // Bị từ chối
        Retry = 8               // Thử lại
    }

    /// <summary>
    /// Push notification status
    /// </summary>
    public enum PushNotificationStatus
    {
        Pending = 1,            // Chờ gửi
        Sending = 2,            // Đang gửi
        Sent = 3,               // Đã gửi
        Delivered = 4,          // Đã nhận
        Failed = 5,             // Thất bại
        Clicked = 6,            // Đã click
        Dismissed = 7,          // Đã bỏ qua
        Expired = 8             // Hết hạn
    }
}