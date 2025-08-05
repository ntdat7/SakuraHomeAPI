using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Payments.Responses
{
    /// <summary>
    /// Payment transaction response
    /// </summary>
    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public string? ExternalTransactionId { get; set; }
        
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        
        public Guid UserId { get; set; }
        public string UserEmail { get; set; }
        
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodName { get; set; }
        
        public PaymentStatus Status { get; set; }
        public string StatusText { get; set; }
        
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        
        public decimal? FeeAmount { get; set; }
        public decimal? RefundedAmount { get; set; }
        
        public string? Description { get; set; }
        public string? ResponseMessage { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        
        public bool CanRefund { get; set; }
        public bool CanCancel { get; set; }
        
        public List<PaymentLogDto>? PaymentLogs { get; set; }
    }

    /// <summary>
    /// Payment transaction summary
    /// </summary>
    public class PaymentSummaryDto
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodName { get; set; }
        public PaymentStatus Status { get; set; }
        public string StatusText { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// Payment log entry
    /// </summary>
    public class PaymentLogDto
    {
        public int Id { get; set; }
        public PaymentStatus FromStatus { get; set; }
        public PaymentStatus ToStatus { get; set; }
        public string? Notes { get; set; }
        public string? ResponseData { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Payment URL response (for redirect payments)
    /// </summary>
    public class PaymentUrlResponseDto
    {
        public string TransactionId { get; set; }
        public string PaymentUrl { get; set; }
        public string? QrCodeUrl { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? Instructions { get; set; }
        public Dictionary<string, string>? AdditionalData { get; set; }
    }

    /// <summary>
    /// Payment method information
    /// </summary>
    public class PaymentMethodDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string LogoUrl { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        
        // Fee information
        public decimal FeePercentage { get; set; }
        public decimal FixedFee { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        
        // Availability
        public bool IsAvailable { get; set; }
        public string? UnavailableReason { get; set; }
        
        // Instructions
        public string? Instructions { get; set; }
        public List<string>? RequiredFields { get; set; }
    }

    /// <summary>
    /// Payment statistics
    /// </summary>
    public class PaymentStatsDto
    {
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalFees { get; set; }
        public decimal TotalRefunds { get; set; }
        
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public int PendingTransactions { get; set; }
        
        public decimal SuccessRate { get; set; }
        public decimal AverageTransactionAmount { get; set; }
        
        public Dictionary<string, int> TransactionsByMethod { get; set; } = new();
        public Dictionary<string, decimal> AmountsByMethod { get; set; } = new();
        public Dictionary<string, int> TransactionsByStatus { get; set; } = new();
        
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    /// <summary>
    /// VNPay payment response
    /// </summary>
    public class VNPayResponseDto
    {
        public string TransactionId { get; set; }
        public string PaymentUrl { get; set; }
        public string? BankCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Instructions { get; set; }
    }

    /// <summary>
    /// MoMo payment response
    /// </summary>
    public class MoMoResponseDto
    {
        public string TransactionId { get; set; }
        public string PayUrl { get; set; }
        public string QrCodeUrl { get; set; }
        public string DeepLink { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Instructions { get; set; }
    }

    /// <summary>
    /// Bank transfer instructions
    /// </summary>
    public class BankTransferResponseDto
    {
        public string TransactionId { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
        public string TransferContent { get; set; }
        public string QrCodeUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public List<string> Instructions { get; set; } = new();
    }

    /// <summary>
    /// Payment callback result
    /// </summary>
    public class PaymentCallbackResponseDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public PaymentStatus Status { get; set; }
        public string? TransactionId { get; set; }
        public string? OrderNumber { get; set; }
        public decimal? Amount { get; set; }
        public DateTime ProcessedAt { get; set; }
    }

    /// <summary>
    /// Refund response
    /// </summary>
    public class RefundResponseDto
    {
        public string RefundId { get; set; }
        public string TransactionId { get; set; }
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; }
        public PaymentStatus Status { get; set; }
        public string StatusText { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessingMessage { get; set; }
        public int EstimatedDays { get; set; }
    }
}