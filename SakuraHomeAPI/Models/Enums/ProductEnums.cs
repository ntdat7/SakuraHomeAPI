namespace SakuraHomeAPI.Models.Enums
{
    /// <summary>
    /// Product status lifecycle
    /// </summary>
    public enum ProductStatus
    {
        Draft = 0,          // Bản nháp
        Active = 1,         // Đang bán
        Inactive = 2,       // Tạm ngưng bán
        OutOfStock = 3,     // Hết hàng
        Discontinued = 4,   // Ngưng sản xuất
        ComingSoon = 5      // Sắp ra mắt
    }

    /// <summary>
    /// Product condition
    /// </summary>
    public enum ProductCondition
    {
        New = 1,            // Hàng mới
        LikeNew = 2,        // Như mới
        Used = 3,           // Đã sử dụng
        Refurbished = 4,    // Tân trang
        Damaged = 5         // Hư hỏng
    }

    /// <summary>
    /// Product attribute types for filtering and variants
    /// </summary>
    public enum AttributeType
    {
        Text = 1,           // Văn bản tự do
        Number = 2,         // Số
        Select = 3,         // Chọn từ danh sách
        MultiSelect = 4,    // Chọn nhiều
        Boolean = 5,        // True/False
        Color = 6,          // Màu sắc
        Date = 7,           // Ngày tháng
        File = 8,           // File đính kèm
        URL = 9             // Đường dẫn
    }

    /// <summary>
    /// Inventory tracking methods
    /// </summary>
    public enum InventoryAction
    {
        Purchase = 1,      // Nhập hàng từ nhà cung cấp
        Sale = 2,          // Bán hàng cho khách
        Return = 3,        // Trả hàng từ khách
        Adjustment = 4,    // Điều chỉnh tồn kho
        Damage = 5,        // Hàng bị hư hỏng
        Transfer = 6,      // Chuyển kho
        Lost = 7,          // Mất hàng
        Found = 8,         // Tìm thấy hàng
        Expired = 9,       // Hàng hết hạn
        Reserved = 10,     // Đặt trước
        Released = 11,     // Hủy đặt trước
        Promotion = 12,    // Khuyến mãi/tặng
        Sample = 13,       // Lấy mẫu
        QualityCheck = 14  // Kiểm tra chất lượng
    }

    /// <summary>
    /// Product weight units
    /// </summary>
    public enum WeightUnit
    {
        Gram = 1,
        Kilogram = 2,
        Pound = 3,
        Ounce = 4
    }

    /// <summary>
    /// Product dimension units
    /// </summary>
    public enum DimensionUnit
    {
        Centimeter = 1,
        Meter = 2,
        Inch = 3,
        Foot = 4
    }

    /// <summary>
    /// Japanese product authenticity levels
    /// </summary>
    public enum AuthenticityLevel
    {
        Unknown = 0,        // Chưa xác định
        NotVerified = 1,    // Chưa xác thực
        Basic = 2,          // Cơ bản
        Verified = 3,       // Đã xác thực
        Premium = 4,        // Cao cấp
        Certified = 5,      // Có chứng nhận
        DirectImport = 6,   // Nhập khẩu trực tiếp
        Authorized = 7      // Đại lý chính thức
    }

    /// <summary>
    /// Product age restrictions
    /// </summary>
    public enum AgeRestriction
    {
        None = 0,           // Không giới hạn
        Under3 = 1,         // Dưới 3 tuổi không phù hợp
        Teen = 13,          // Trên 13 tuổi
        Over13 = 13,        // Trên 13 tuổi (alias)
        Adult = 18,         // Trên 18 tuổi
        Over18 = 18,        // Trên 18 tuổi (alias)
        AdultPlus = 21      // Trên 21 tuổi
    }

    /// <summary>
    /// Product origins in Japan
    /// </summary>
    public enum JapaneseOrigin
    {
        Tokyo = 1,
        Osaka = 2,
        Kyoto = 3,
        Hokkaido = 4,
        Okinawa = 5,
        Hiroshima = 6,
        Fukuoka = 7,
        Nagoya = 8,
        Kobe = 9,
        Yokohama = 10,
        Kanto = 20,         
        Kansai = 21,        
        Chubu = 22,         
        Tohoku = 23,       
        Chugoku = 24,       
        Shikoku = 25,       
        Kyushu = 26,        
        Other = 99
    }

    /// <summary>
    /// Product visibility levels
    /// </summary>
    public enum ProductVisibility
    {
        Public = 1,         // Công khai
        Hidden = 2,         // Ẩn khỏi danh sách
        Private = 3,        // Chỉ admin xem được
        MembersOnly = 4,    // Chỉ thành viên
        VIPOnly = 5         // Chỉ VIP
    }
}