using System.ComponentModel.DataAnnotations;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Payments.Requests
{
    /// <summary>
    /// Request to initiate a payment process
    /// </summary>
    public class CreatePaymentRequestDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [MaxLength(500)]
        public string? ReturnUrl { get; set; }

        [MaxLength(500)]
        public string? CancelUrl { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Additional payment data (JSON format)
        /// For e-wallet payments, bank transfers, etc.
        /// </summary>
        [MaxLength(2000)]
        public string? PaymentData { get; set; }
    }

    /// <summary>
    /// Request to process payment confirmation
    /// </summary>
    public class ProcessPaymentRequestDto
    {
        [Required]
        public string TransactionId { get; set; }

        [Required]
        public string ExternalTransactionId { get; set; }

        [Required]
        public PaymentStatus Status { get; set; }

        [MaxLength(1000)]
        public string? ResponseMessage { get; set; }

        /// <summary>
        /// Payment gateway response data (JSON format)
        /// </summary>
        [MaxLength(5000)]
        public string? ResponseData { get; set; }
    }

    /// <summary>
    /// Request to refund a payment
    /// </summary>
    public class RefundPaymentRequestDto
    {
        [Required]
        public string TransactionId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Refund amount must be greater than 0")]
        public decimal RefundAmount { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Request to update payment status
    /// </summary>
    public class UpdatePaymentStatusRequestDto
    {
        [Required]
        public PaymentStatus Status { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(2000)]
        public string? ResponseData { get; set; }
    }

    /// <summary>
    /// Payment callback request from payment gateways
    /// </summary>
    public class PaymentCallbackRequestDto
    {
        [Required]
        public string TransactionId { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public string? ExternalTransactionId { get; set; }
        
        public string? PaymentMethod { get; set; }
        
        public string? Message { get; set; }
        
        public DateTime? PaymentDate { get; set; }

        /// <summary>
        /// Raw callback data from payment gateway
        /// </summary>
        [MaxLength(10000)]
        public string? RawData { get; set; }

        /// <summary>
        /// Security signature from payment gateway
        /// </summary>
        [MaxLength(500)]
        public string? Signature { get; set; }
    }

    /// <summary>
    /// Request to get payment methods
    /// </summary>
    public class GetPaymentMethodsRequestDto
    {
        public decimal Amount { get; set; }
        public string? Currency { get; set; } = "VND";
        public bool ActiveOnly { get; set; } = true;
    }

    /// <summary>
    /// Request for VNPay payment
    /// </summary>
    public class VNPayRequestDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(500)]
        public string OrderDescription { get; set; }

        [Required]
        [MaxLength(500)]
        public string ReturnUrl { get; set; }

        [MaxLength(15)]
        public string? BankCode { get; set; } // For direct bank payment

        [MaxLength(15)]
        public string? Language { get; set; } = "vn"; // vn or en
    }

    /// <summary>
    /// Request for MoMo payment
    /// </summary>
    public class MoMoRequestDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(500)]
        public string OrderInfo { get; set; }

        [Required]
        [MaxLength(500)]
        public string ReturnUrl { get; set; }

        [MaxLength(500)]
        public string? NotifyUrl { get; set; }

        [MaxLength(2000)]
        public string? ExtraData { get; set; }
    }

    /// <summary>
    /// Request for Bank Transfer payment
    /// </summary>
    public class BankTransferRequestDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public string BankCode { get; set; }

        [Required]
        public string BankAccountNumber { get; set; }

        [Required]
        public string BankAccountName { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? TransferNote { get; set; }

        public DateTime? TransferDate { get; set; }

        [MaxLength(1000)]
        public string? ProofImageUrl { get; set; } // Upload transfer receipt
    }

    /// <summary>
    /// Request to calculate payment fee
    /// </summary>
    public class CalculatePaymentFeeRequestDto
    {
        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }
}