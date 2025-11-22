namespace SakuraHomeAPI.DTOs.Payments.Responses
{
    /// <summary>
    /// Response for SePay payment creation
    /// </summary>
    public class SePayResponseDto
    {
        public string TransactionId { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string PaymentCode { get; set; } // Mã thanh toán để khách hàng điền vào nội dung chuyển khoản
        public string TransferContent { get; set; } // Nội dung chuyển khoản đầy đủ
        public BankAccountInfo BankAccount { get; set; }
        public string? QRCodeUrl { get; set; } // URL của QR code
        public string? QRCodeBase64 { get; set; } // QR code dưới dạng base64
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public List<string> Instructions { get; set; } = new();
    }

    /// <summary>
    /// Bank account information for transfer
    /// </summary>
    public class BankAccountInfo
    {
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolder { get; set; }
        public string? Branch { get; set; }
    }

    /// <summary>
    /// Response for QR Code payment
    /// </summary>
    public class QRCodePaymentResponseDto
    {
        public string TransactionId { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string QRCodeUrl { get; set; }
        public string? QRCodeBase64 { get; set; }
        public BankAccountInfo BankAccount { get; set; }
        public string TransferContent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// SePay webhook response
    /// </summary>
    public class SePayWebhookResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? TransactionId { get; set; }
        public string? OrderNumber { get; set; }
        public decimal? Amount { get; set; }
        public string? Status { get; set; }
    }
}
