using System.ComponentModel.DataAnnotations;

namespace SakuraHomeAPI.DTOs.Payments.Requests
{
    /// <summary>
    /// Request for SePay payment
    /// </summary>
    public class SePayRequestDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(500)]
        public string OrderDescription { get; set; }

        [MaxLength(100)]
        public string? CustomerName { get; set; }

        [MaxLength(100)]
        public string? CustomerEmail { get; set; }

        [MaxLength(20)]
        public string? CustomerPhone { get; set; }

        /// <summary>
        /// Generate QR code for payment
        /// </summary>
        public bool GenerateQRCode { get; set; } = true;

        /// <summary>
        /// Payment timeout in minutes
        /// </summary>
        [Range(5, 60)]
        public int TimeoutMinutes { get; set; } = 15;
    }

    /// <summary>
    /// SePay webhook callback data
    /// </summary>
    public class SePayWebhookDto
    {
        [Required]
        public int Id { get; set; } // ID giao dịch trên SePay

        [Required]
        public string Gateway { get; set; } // Brand name của ngân hàng

        [Required]
        public string TransactionDate { get; set; } // Thời gian xảy ra giao dịch

        [Required]
        public string AccountNumber { get; set; } // Số tài khoản ngân hàng

        public string? Code { get; set; } // Mã code thanh toán

        [Required]
        public string Content { get; set; } // Nội dung chuyển khoản

        [Required]
        public string TransferType { get; set; } // in: tiền vào, out: tiền ra

        [Required]
        public decimal TransferAmount { get; set; } // Số tiền giao dịch

        public decimal? Accumulated { get; set; } // Số dư tài khoản

        public string? SubAccount { get; set; } // Tài khoản phụ

        public string? ReferenceCode { get; set; } // Mã tham chiếu

        public string? Description { get; set; } // Toàn bộ nội dung tin nhắn SMS
    }

    /// <summary>
    /// Request for QR Code payment
    /// </summary>
    public class QRCodePaymentRequestDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// QR Code template (compact, compact2, qr_only, print)
        /// </summary>
        [MaxLength(20)]
        public string Template { get; set; } = "compact";

        /// <summary>
        /// Bank code for VietQR
        /// </summary>
        [MaxLength(10)]
        public string? BankCode { get; set; }

        /// <summary>
        /// Account number
        /// </summary>
        [MaxLength(50)]
        public string? AccountNumber { get; set; }

        /// <summary>
        /// Account name
        /// </summary>
        [MaxLength(200)]
        public string? AccountName { get; set; }
    }
}
