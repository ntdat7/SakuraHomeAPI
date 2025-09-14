using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.DTOs.Payments.Requests;
using SakuraHomeAPI.DTOs.Payments.Responses;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SakuraHomeAPI.Services.Implementations
{
    /// <summary>
    /// Payment service implementation
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _configuration;

        public PaymentService(
            ApplicationDbContext context,
            ILogger<PaymentService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<ApiResponse<List<PaymentMethodDto>>> GetPaymentMethodsAsync(GetPaymentMethodsRequestDto request)
        {
            try
            {
                var query = _context.PaymentMethods.AsQueryable();

                if (request.ActiveOnly)
                {
                    query = query.Where(pm => pm.IsActive);
                }

                // Filter by amount limits
                if (request.Amount > 0)
                {
                    query = query.Where(pm => 
                        (pm.MinAmount == 0 || request.Amount >= pm.MinAmount) &&
                        (pm.MaxAmount == 0 || request.Amount <= pm.MaxAmount));
                }

                var paymentMethods = await query
                    .OrderBy(pm => pm.DisplayOrder)
                    .ThenBy(pm => pm.Name)
                    .ToListAsync();

                var paymentMethodDtos = paymentMethods.Select(pm => new PaymentMethodDto
                {
                    Id = pm.Id,
                    Name = pm.Name,
                    Description = pm.Description,
                    Code = pm.Code,
                    LogoUrl = pm.LogoUrl,
                    IsActive = pm.IsActive,
                    DisplayOrder = pm.DisplayOrder,
                    FeePercentage = pm.FeePercentage,
                    FixedFee = pm.FixedFee,
                    MinAmount = pm.MinAmount,
                    MaxAmount = pm.MaxAmount,
                    IsAvailable = pm.IsActive &&
                                 (pm.MinAmount == 0 || request.Amount >= pm.MinAmount) &&
                                 (pm.MaxAmount == 0 || request.Amount <= pm.MaxAmount),
                    Instructions = GetPaymentInstructions(pm.Code),
                    RequiredFields = GetRequiredFields(pm.Code)
                }).ToList();

                return ApiResponse.SuccessResult(paymentMethodDtos, "Payment methods retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment methods");
                return ApiResponse.ErrorResult<List<PaymentMethodDto>>("Failed to retrieve payment methods");
            }
        }

        public async Task<ApiResponse<PaymentResponseDto>> CreatePaymentAsync(CreatePaymentRequestDto request, Guid userId)
        {
            try
            {
                _logger.LogInformation("Creating payment for order {OrderId} by user {UserId}", request.OrderId, userId);

                // Validate order
                var order = await _context.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.User.Id == userId);

                if (order == null)
                {
                    return ApiResponse.ErrorResult<PaymentResponseDto>("Order not found");
                }

                if (order.PaymentStatus == PaymentStatus.Paid)
                {
                    return ApiResponse.ErrorResult<PaymentResponseDto>("Order is already paid");
                }

                // For COD, we only support COD method
                if (request.PaymentMethod != PaymentMethod.COD)
                {
                    return ApiResponse.ErrorResult<PaymentResponseDto>("Currently only COD payment is supported");
                }

                // Check if there's already a pending payment
                var existingPayment = await _context.PaymentTransactions
                    .FirstOrDefaultAsync(pt => pt.OrderId == request.OrderId && 
                                             pt.Status == PaymentStatus.Pending);

                if (existingPayment != null)
                {
                    // Cancel existing pending payment
                    existingPayment.Status = PaymentStatus.Cancelled;
                    existingPayment.ResponseMessage = "Cancelled due to new payment request";
                }

                // Generate transaction ID
                var transactionId = GenerateTransactionId();

                // Create payment transaction
                var paymentTransaction = new PaymentTransaction
                {
                    TransactionId = transactionId,
                    OrderId = request.OrderId,
                    User = order.User,
                    PaymentMethod = GetPaymentMethodCode(request.PaymentMethod),
                    Amount = order.TotalAmount,
                    Currency = order.Currency ?? "VND",
                    Status = PaymentStatus.Pending,
                    Description = request.Description ?? $"Payment for order {order.OrderNumber}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.PaymentTransactions.Add(paymentTransaction);
                
                // For COD, immediately set order payment status to confirmed
                order.PaymentStatus = PaymentStatus.Confirmed;
                order.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment transaction {TransactionId} created for order {OrderId}", 
                    transactionId, request.OrderId);

                // Create response
                var response = new PaymentResponseDto
                {
                    Id = paymentTransaction.Id,
                    TransactionId = transactionId,
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    UserId = userId,
                    UserEmail = order.User.Email,
                    PaymentMethod = request.PaymentMethod,
                    PaymentMethodName = GetPaymentMethodName(request.PaymentMethod),
                    Status = PaymentStatus.Confirmed,
                    StatusText = GetStatusText(PaymentStatus.Confirmed),
                    Amount = order.TotalAmount,
                    Currency = order.Currency ?? "VND",
                    Description = paymentTransaction.Description,
                    CreatedAt = paymentTransaction.CreatedAt,
                    CanRefund = false,
                    CanCancel = false
                };

                return ApiResponse.SuccessResult(response, "Payment transaction created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment for order {OrderId}", request.OrderId);
                return ApiResponse.ErrorResult<PaymentResponseDto>("Failed to create payment");
            }
        }

        public async Task<ApiResponse<PaymentResponseDto>> GetPaymentAsync(string transactionId, Guid? userId = null)
        {
            try
            {
                var query = _context.PaymentTransactions
                    .Include(pt => pt.Order)
                    .Include(pt => pt.User)
                    .AsQueryable();

                if (userId.HasValue)
                {
                    query = query.Where(pt => pt.User.Id == userId.Value);
                }

                var payment = await query.FirstOrDefaultAsync(pt => pt.TransactionId == transactionId);

                if (payment == null)
                {
                    return ApiResponse.ErrorResult<PaymentResponseDto>("Payment not found");
                }

                var response = MapToPaymentResponseDto(payment);
                return ApiResponse.SuccessResult(response, "Payment retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment {TransactionId}", transactionId);
                return ApiResponse.ErrorResult<PaymentResponseDto>("Failed to retrieve payment");
            }
        }

        public async Task<ApiResponse<List<PaymentSummaryDto>>> GetUserPaymentsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var payments = await _context.PaymentTransactions
                    .Include(pt => pt.Order)
                    .Where(pt => pt.User.Id == userId)
                    .OrderByDescending(pt => pt.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var paymentSummaries = payments.Select(MapToPaymentSummaryDto).ToList();

                return ApiResponse.SuccessResult(paymentSummaries, "User payments retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for user {UserId}", userId);
                return ApiResponse.ErrorResult<List<PaymentSummaryDto>>("Failed to retrieve user payments");
            }
        }

        public async Task<ApiResponse<List<PaymentSummaryDto>>> GetOrderPaymentsAsync(int orderId, Guid? userId = null)
        {
            try
            {
                var query = _context.PaymentTransactions
                    .Include(pt => pt.Order)
                    .Where(pt => pt.OrderId == orderId);

                if (userId.HasValue)
                {
                    query = query.Where(pt => pt.User.Id == userId.Value);
                }

                var payments = await query
                    .OrderByDescending(pt => pt.CreatedAt)
                    .ToListAsync();

                var paymentSummaries = payments.Select(MapToPaymentSummaryDto).ToList();

                return ApiResponse.SuccessResult(paymentSummaries, "Order payments retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for order {OrderId}", orderId);
                return ApiResponse.ErrorResult<List<PaymentSummaryDto>>("Failed to retrieve order payments");
            }
        }

        // Placeholder implementations for gateway-specific methods
        public async Task<ApiResponse<VNPayResponseDto>> CreateVNPayPaymentAsync(VNPayRequestDto request, Guid userId)
        {
            // TODO: Implement VNPay integration
            return ApiResponse.ErrorResult<VNPayResponseDto>("VNPay integration not implemented yet");
        }

        public async Task<ApiResponse<PaymentCallbackResponseDto>> ProcessVNPayCallbackAsync(Dictionary<string, string> vnpayData)
        {
            // TODO: Implement VNPay callback processing
            return ApiResponse.ErrorResult<PaymentCallbackResponseDto>("VNPay callback processing not implemented yet");
        }

        public async Task<ApiResponse<MoMoResponseDto>> CreateMoMoPaymentAsync(MoMoRequestDto request, Guid userId)
        {
            // TODO: Implement MoMo integration
            return ApiResponse.ErrorResult<MoMoResponseDto>("MoMo integration not implemented yet");
        }

        public async Task<ApiResponse<PaymentCallbackResponseDto>> ProcessMoMoCallbackAsync(Dictionary<string, string> momoData)
        {
            // TODO: Implement MoMo callback processing
            return ApiResponse.ErrorResult<PaymentCallbackResponseDto>("MoMo callback processing not implemented yet");
        }

        public async Task<ApiResponse<BankTransferResponseDto>> CreateBankTransferAsync(BankTransferRequestDto request, Guid userId)
        {
            try
            {
                // For now, return basic bank transfer instructions
                var transactionId = GenerateTransactionId();
                
                var response = new BankTransferResponseDto
                {
                    TransactionId = transactionId,
                    BankName = "Vietcombank",
                    BankCode = "VCB",
                    AccountNumber = "1234567890",
                    AccountName = "SAKURA HOME COMPANY",
                    Amount = request.Amount,
                    TransferContent = $"SAKURA {request.OrderId} {transactionId}",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(3),
                    Instructions = new List<string>
                    {
                        "Chuy?n kho?n ?úng s? ti�n và n?i dung chuy?n kho?n",
                        "G?i ?nh ch?p biên lai chuy?n kho?n ?? xác nh?n",
                        "??n hàng s? ???c x? lý sau khi xác nh?n thanh toán"
                    }
                };

                return ApiResponse.SuccessResult(response, "Bank transfer instructions created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bank transfer for order {OrderId}", request.OrderId);
                return ApiResponse.ErrorResult<BankTransferResponseDto>("Failed to create bank transfer instructions");
            }
        }

        // More placeholder implementations
        public async Task<ApiResponse<PaymentResponseDto>> UpdatePaymentStatusAsync(string transactionId, UpdatePaymentStatusRequestDto request, Guid staffId)
        {
            return ApiResponse.ErrorResult<PaymentResponseDto>("Method not implemented yet");
        }

        public async Task<ApiResponse<RefundResponseDto>> ProcessRefundAsync(RefundPaymentRequestDto request, Guid staffId)
        {
            return ApiResponse.ErrorResult<RefundResponseDto>("Method not implemented yet");
        }

        public async Task<ApiResponse> CancelPaymentAsync(string transactionId, string reason, Guid userId)
        {
            return ApiResponse.ErrorResult("Method not implemented yet");
        }

        public async Task<ApiResponse<PaymentStatsDto>> GetPaymentStatsAsync(DateTime? fromDate = null, DateTime? toDate = null, PaymentMethod? method = null)
        {
            return ApiResponse.ErrorResult<PaymentStatsDto>("Method not implemented yet");
        }

        public async Task<ApiResponse<PaymentResponseDto>> ConfirmBankTransferAsync(string transactionId, BankTransferRequestDto request, Guid staffId)
        {
            return ApiResponse.ErrorResult<PaymentResponseDto>("Method not implemented yet");
        }

        public async Task<bool> VerifyPaymentSignatureAsync(PaymentMethod method, Dictionary<string, string> data)
        {
            return false; // TODO: Implement signature verification
        }

        public async Task<ApiResponse<decimal>> CalculatePaymentFeeAsync(PaymentMethod method, decimal amount)
        {
            try
            {
                if (method == PaymentMethod.COD)
                {
                    // COD usually has no fee
                    return ApiResponse.SuccessResult(0m, "COD payment has no fee");
                }

                var paymentMethodInfo = await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.Code == GetPaymentMethodCode(method));

                if (paymentMethodInfo == null)
                {
                    return ApiResponse.ErrorResult<decimal>("Payment method not found");
                }

                var fee = paymentMethodInfo.FixedFee + (amount * paymentMethodInfo.FeePercentage / 100);
                return ApiResponse.SuccessResult(fee, "Payment fee calculated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating payment fee for method {Method}", method);
                return ApiResponse.ErrorResult<decimal>("Failed to calculate payment fee");
            }
        }

        #region Private Helper Methods

        private string GenerateTransactionId()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var random = Random.Shared.Next(1000, 9999);
            return $"PAY{timestamp}{random}";
        }

        private PaymentResponseDto MapToPaymentResponseDto(PaymentTransaction payment)
        {
            return new PaymentResponseDto
            {
                Id = payment.Id,
                TransactionId = payment.TransactionId,
                ExternalTransactionId = payment.ExternalTransactionId,
                OrderId = payment.OrderId,
                OrderNumber = payment.Order.OrderNumber,
                UserId = payment.User.Id,
                UserEmail = payment.User.Email,
                PaymentMethod = ParsePaymentMethod(payment.PaymentMethod),
                PaymentMethodName = GetPaymentMethodName(ParsePaymentMethod(payment.PaymentMethod)),
                Status = payment.Status,
                StatusText = GetStatusText(payment.Status),
                Amount = payment.Amount,
                Currency = payment.Currency,
                FeeAmount = payment.FeeAmount,
                RefundedAmount = payment.RefundedAmount,
                Description = payment.Description,
                ResponseMessage = payment.ResponseMessage,
                CreatedAt = payment.CreatedAt,
                ProcessedAt = payment.ProcessedAt,
                CompletedAt = payment.CompletedAt,
                RefundedAt = payment.RefundedAt,
                CanRefund = payment.Status == PaymentStatus.Paid,
                CanCancel = payment.Status == PaymentStatus.Pending
            };
        }

        private PaymentSummaryDto MapToPaymentSummaryDto(PaymentTransaction payment)
        {
            return new PaymentSummaryDto
            {
                Id = payment.Id,
                TransactionId = payment.TransactionId,
                OrderId = payment.OrderId,
                OrderNumber = payment.Order.OrderNumber,
                PaymentMethod = ParsePaymentMethod(payment.PaymentMethod),
                PaymentMethodName = GetPaymentMethodName(ParsePaymentMethod(payment.PaymentMethod)),
                Status = payment.Status,
                StatusText = GetStatusText(payment.Status),
                Amount = payment.Amount,
                Currency = payment.Currency,
                CreatedAt = payment.CreatedAt,
                CompletedAt = payment.CompletedAt
            };
        }

        private string GetPaymentMethodCode(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.COD => "COD",
                PaymentMethod.BankTransfer => "BANK_TRANSFER",
                PaymentMethod.CreditCard => "CREDIT_CARD",
                PaymentMethod.DebitCard => "DEBIT_CARD",
                PaymentMethod.EWallet => "MOMO",
                PaymentMethod.QRCode => "QRCODE",
                PaymentMethod.Installment => "INSTALLMENT",
                _ => "COD"  // Default to COD
            };
        }

        private PaymentMethod ParsePaymentMethod(string code)
        {
            return code switch
            {
                "COD" => PaymentMethod.COD,
                "BANK_TRANSFER" => PaymentMethod.BankTransfer,
                "CREDIT_CARD" => PaymentMethod.CreditCard,
                "DEBIT_CARD" => PaymentMethod.DebitCard,
                "MOMO" or "ZALOPAY" => PaymentMethod.EWallet,
                "QRCODE" => PaymentMethod.QRCode,
                "INSTALLMENT" => PaymentMethod.Installment,
                _ => PaymentMethod.COD
            };
        }

        private string GetPaymentMethodName(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.COD => "Thanh toán khi nh?n hàng",
                PaymentMethod.BankTransfer => "Chuy?n kho?n ngân hàng",
                PaymentMethod.CreditCard => "Th? tín d?ng",
                PaymentMethod.DebitCard => "Th? ghi n?",
                PaymentMethod.EWallet => "Ví ?i?n t?",
                PaymentMethod.QRCode => "Quét mã QR",
                PaymentMethod.Installment => "Tr? góp",
                _ => "Thanh toán khi nh?n hàng"  // Default to COD
            };
        }

        private PaymentStatus ParsePaymentStatus(string status)
        {
            return status.ToUpper() switch
            {
                "SUCCESS" or "COMPLETED" or "PAID" => PaymentStatus.Paid,
                "FAILED" or "ERROR" => PaymentStatus.Failed,
                "CANCELLED" or "CANCELED" => PaymentStatus.Cancelled,
                "PENDING" => PaymentStatus.Pending,
                "PROCESSING" => PaymentStatus.Processing,
                "REFUNDED" => PaymentStatus.Refunded,
                "PARTIALLY_REFUNDED" => PaymentStatus.PartiallyRefunded,
                "EXPIRED" => PaymentStatus.Expired,
                "CONFIRMED" => PaymentStatus.Confirmed,
                _ => PaymentStatus.Pending
            };
        }

        private string GetStatusText(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => "Ch? thanh toán",
                PaymentStatus.Processing => "?ang x? lý",
                PaymentStatus.Paid => "?ã thanh toán",
                PaymentStatus.Failed => "Thanh toán th?t b?i",
                PaymentStatus.Cancelled => "?ã h?y",
                PaymentStatus.Refunded => "?ã hoàn ti?n",
                PaymentStatus.PartiallyRefunded => "Hoàn ti?n m?t ph?n",
                PaymentStatus.Expired => "H?t h?n",
                PaymentStatus.Confirmed => "?ã xác nh?n",
                _ => "Không xác ??nh"
            };
        }

        private string? GetPaymentInstructions(string code)
        {
            return code switch
            {
                "COD" => "Thanh toán b?ng ti?n m?t khi nh?n hàng. Shipper s? thu ti?n khi giao hàng.",
                _ => null
            };
        }

        private List<string>? GetRequiredFields(string code)
        {
            return code switch
            {
                "COD" => null, // COD doesn't require additional fields
                _ => null
            };
        }

        #endregion

        // Simplified implementations for methods not used in COD
        public async Task<ApiResponse<PaymentCallbackResponseDto>> ProcessPaymentCallbackAsync(PaymentCallbackRequestDto request)
        {
            return ApiResponse.ErrorResult<PaymentCallbackResponseDto>("Not supported for COD payments");
        }
    }
}