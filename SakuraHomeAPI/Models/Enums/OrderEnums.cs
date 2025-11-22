namespace SakuraHomeAPI.Models.Enums
{
    /// <summary>
    /// Order status workflow
    /// </summary>
    public enum OrderStatus
    {
        Pending = 1,        // Đang chờ xử lý
        Confirmed = 2,      // Đã xác nhận
        Processing = 3,     // Đang chuẩn bị hàng
        Packed = 4,         // Đã đóng gói
        Shipped = 5,        // Đã giao cho vận chuyển
        OutForDelivery = 6, // Đang giao hàng
        Delivered = 7,      // Đã giao thành công
        Cancelled = 8,      // Đã hủy
        Returned = 9,       // Đã trả hàng
        Refunded = 10,      // Đã hoàn tiền
        Completed = 11      // Hoàn tất    
    }

    /// <summary>
    /// Payment status
    /// </summary>
    public enum PaymentStatus
    {
        Pending = 1,            // Chờ thanh toán
        Processing = 2,         // Đang xử lý
        Paid = 3,              // Đã thanh toán
        Failed = 4,            // Thanh toán thất bại
        Cancelled = 5,         // Đã hủy
        Refunded = 6,          // Đã hoàn tiền
        PartiallyRefunded = 7, // Hoàn tiền một phần
        Expired = 8,           // Hết hạn thanh toán
        Confirmed = 9          // Đã xác nhận (for COD)
    }

    /// <summary>
    /// Return status
    /// </summary>
    public enum ReturnStatus
    {
        Requested = 1,      // Yêu cầu trả hàng
        Approved = 2,       // Chấp nhận trả hàng
        Rejected = 3,       // Từ chối trả hàng
        InTransit = 4,      // Đang vận chuyển về
        Received = 5,       // Đã nhận hàng trả
        Inspecting = 6,     // Đang kiểm tra
        Completed = 7,      // Hoàn tất
        RefundProcessed = 8 // Đã hoàn tiền
    }

    /// <summary>
    /// Delivery methods
    /// </summary>
    public enum DeliveryMethod
    {
        Standard = 1,   // Giao hàng tiêu chuẩn (3-5 ngày)
        Express = 2,    // Giao hàng nhanh (1-2 ngày)
        SuperFast = 3,  // Giao hàng siêu tốc (trong ngày)
        SelfPickup = 4  // Tự đến lấy
    }

    /// <summary>
    /// Payment methods
    /// </summary>
    public enum PaymentMethod
    {
        COD = 1,            // Thanh toán khi nhận hàng
        BankTransfer = 2,   // Chuyển khoản ngân hàng
        CreditCard = 3,     // Thẻ tín dụng
        DebitCard = 4,      // Thẻ ghi nợ
        EWallet = 5,        // Ví điện tử (MoMo, ZaloPay)
        QRCode = 6,         // Quét mã QR
        Installment = 7,    // Trả góp
        SePay = 8,          // SePay - Chuyển khoản ngân hàng qua SePay
        VNPay = 9,          // VNPay - Cổng thanh toán VNPay
        MoMo = 10           // MoMo - Ví điện tử MoMo
    }

    /// <summary>
    /// Discount types
    /// </summary>
    public enum DiscountType
    {
        Percentage = 1,     // Giảm theo phần trăm
        FixedAmount = 2     // Giảm số tiền cố định
    }

    /// <summary>
    /// Coupon types
    /// </summary>
    public enum CouponType
    {
        Percentage = 1,     // Giảm theo phần trăm
        FixedAmount = 2,    // Giảm số tiền cố định
        FreeShipping = 3,   // Miễn phí vận chuyển
        BuyXGetY = 4,       // Mua X tặng Y
        FirstOrder = 5,     // Đơn hàng đầu tiên
        BulkDiscount = 6    // Giảm giá khi mua số lượng lớn
    }

    /// <summary>
    /// Return/Refund reasons
    /// </summary>
    public enum ReturnReason
    {
        DefectiveProduct = 1,   // Sản phẩm bị lỗi
        WrongItem = 2,          // Giao sai sản phẩm
        NotAsDescribed = 3,     // Không đúng mô tả
        DamagedShipping = 4,    // Hư hỏng khi vận chuyển
        ChangedMind = 5,        // Đổi ý
        SizeIssue = 6,          // Vấn đề về kích thước
        QualityIssue = 7,       // Vấn đề về chất lượng
        Other = 8               // Lý do khác
    }

    /// <summary>
    /// Order cancellation reasons
    /// </summary>
    public enum CancellationReason
    {
        CustomerRequest = 1,    // Khách hàng yêu cầu
        OutOfStock = 2,         // Hết hàng
        PaymentFailed = 3,      // Thanh toán thất bại
        SystemError = 4,        // Lỗi hệ thống
        FraudSuspicion = 5,     // Nghi ngờ gian lận
        AddressIssue = 6,       // Vấn đề địa chỉ giao hàng
        Other = 7               // Lý do khác
    }
}