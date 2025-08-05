using SakuraHomeAPI.DTOs.Payments.Requests;
using SakuraHomeAPI.DTOs.Payments.Responses;
using SakuraHomeAPI.Models.DTOs;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.Services.Interfaces
{
    /// <summary>
    /// Interface for payment processing services
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Get available payment methods for an order
        /// </summary>
        Task<ApiResponse<List<PaymentMethodDto>>> GetPaymentMethodsAsync(GetPaymentMethodsRequestDto request);

        /// <summary>
        /// Create a new payment transaction
        /// </summary>
        Task<ApiResponse<PaymentResponseDto>> CreatePaymentAsync(CreatePaymentRequestDto request, Guid userId);

        /// <summary>
        /// Get payment by transaction ID
        /// </summary>
        Task<ApiResponse<PaymentResponseDto>> GetPaymentAsync(string transactionId, Guid? userId = null);

        /// <summary>
        /// Get payments for a user
        /// </summary>
        Task<ApiResponse<List<PaymentSummaryDto>>> GetUserPaymentsAsync(Guid userId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get payments for an order
        /// </summary>
        Task<ApiResponse<List<PaymentSummaryDto>>> GetOrderPaymentsAsync(int orderId, Guid? userId = null);

        /// <summary>
        /// Process payment confirmation/callback
        /// </summary>
        Task<ApiResponse<PaymentCallbackResponseDto>> ProcessPaymentCallbackAsync(PaymentCallbackRequestDto request);

        /// <summary>
        /// Update payment status (staff only)
        /// </summary>
        Task<ApiResponse<PaymentResponseDto>> UpdatePaymentStatusAsync(string transactionId, UpdatePaymentStatusRequestDto request, Guid staffId);

        /// <summary>
        /// Process refund request
        /// </summary>
        Task<ApiResponse<RefundResponseDto>> ProcessRefundAsync(RefundPaymentRequestDto request, Guid staffId);

        /// <summary>
        /// Cancel a payment
        /// </summary>
        Task<ApiResponse> CancelPaymentAsync(string transactionId, string reason, Guid userId);

        /// <summary>
        /// Get payment statistics (staff only)
        /// </summary>
        Task<ApiResponse<PaymentStatsDto>> GetPaymentStatsAsync(DateTime? fromDate = null, DateTime? toDate = null, PaymentMethod? method = null);

        // Gateway-specific methods

        /// <summary>
        /// Create VNPay payment URL
        /// </summary>
        Task<ApiResponse<VNPayResponseDto>> CreateVNPayPaymentAsync(VNPayRequestDto request, Guid userId);

        /// <summary>
        /// Process VNPay callback
        /// </summary>
        Task<ApiResponse<PaymentCallbackResponseDto>> ProcessVNPayCallbackAsync(Dictionary<string, string> vnpayData);

        /// <summary>
        /// Create MoMo payment
        /// </summary>
        Task<ApiResponse<MoMoResponseDto>> CreateMoMoPaymentAsync(MoMoRequestDto request, Guid userId);

        /// <summary>
        /// Process MoMo callback
        /// </summary>
        Task<ApiResponse<PaymentCallbackResponseDto>> ProcessMoMoCallbackAsync(Dictionary<string, string> momoData);

        /// <summary>
        /// Create bank transfer instructions
        /// </summary>
        Task<ApiResponse<BankTransferResponseDto>> CreateBankTransferAsync(BankTransferRequestDto request, Guid userId);

        /// <summary>
        /// Confirm bank transfer payment
        /// </summary>
        Task<ApiResponse<PaymentResponseDto>> ConfirmBankTransferAsync(string transactionId, BankTransferRequestDto request, Guid staffId);

        /// <summary>
        /// Verify payment gateway signature
        /// </summary>
        Task<bool> VerifyPaymentSignatureAsync(PaymentMethod method, Dictionary<string, string> data);

        /// <summary>
        /// Calculate payment fee
        /// </summary>
        Task<ApiResponse<decimal>> CalculatePaymentFeeAsync(PaymentMethod method, decimal amount);
    }
}