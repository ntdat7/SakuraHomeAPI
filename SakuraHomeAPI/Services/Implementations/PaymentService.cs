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
using Microsoft.AspNetCore.SignalR; 
using SakuraHomeAPI.Hubs;

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
        private readonly IHubContext<NotificationHub> _hubContext;

        public PaymentService(
            ApplicationDbContext context,
            ILogger<PaymentService> logger,
            IConfiguration configuration,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _hubContext = hubContext;
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

        public async Task<ApiResponse<SePayResponseDto>> CreateSePayPaymentAsync(SePayRequestDto request, Guid userId)
        {
            try
            {
                _logger.LogInformation("Creating SePay payment for order {OrderId} by user {UserId}", request.OrderId, userId);

                // Validate order
                var order = await _context.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.User.Id == userId);

                if (order == null)
                {
                    return ApiResponse.ErrorResult<SePayResponseDto>("Order not found");
                }

                if (order.PaymentStatus == PaymentStatus.Paid)
                {
                    return ApiResponse.ErrorResult<SePayResponseDto>("Order is already paid");
                }

                // Get SePay configuration
                var sePayConfig = _configuration.GetSection("Payment:SePay");
                var accountNumber = sePayConfig["AccountNumber"] ?? "0123499999";
                var bankName = sePayConfig["BankName"] ?? "Vietcombank";
                var accountHolder = sePayConfig["AccountHolder"] ?? "CONG TY SAKURA HOME";
                var branch = sePayConfig["BankBranch"] ?? "Chi nhánh TP.HCM";
                var paymentPrefix = sePayConfig["PaymentPrefix"] ?? "SAKURA";

                // Generate transaction ID and payment code
                var transactionId = GenerateTransactionId();
                var paymentCode = $"{paymentPrefix}{request.OrderId:D6}";
                var transferContent = $"{paymentCode} {request.CustomerName?.Replace(" ", "")}";

                // Create payment transaction
                var paymentTransaction = new PaymentTransaction
                {
                    TransactionId = transactionId,
                    OrderId = request.OrderId,
                    User = order.User,
                    PaymentMethod = "SEPAY",
                    Amount = request.Amount,
                    Currency = "VND",
                    Status = PaymentStatus.Pending,
                    Description = request.OrderDescription,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PaymentTransactions.Add(paymentTransaction);

                // Update order payment status
                order.PaymentStatus = PaymentStatus.Pending;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("SePay payment transaction {TransactionId} created for order {OrderId}",
                    transactionId, request.OrderId);

                // Generate QR Code URL if requested
                var response = new SePayResponseDto
                {
                    TransactionId = transactionId,
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    Amount = request.Amount,
                    Currency = "VND",
                    PaymentCode = paymentCode,
                    TransferContent = transferContent,
                    BankAccount = new BankAccountInfo
                    {
                        BankName = bankName,
                        BankCode = "MB", // or get from config
                        AccountNumber = accountNumber,
                        AccountHolder = accountHolder,
                        Branch = branch
                    },
                    QRCodeUrl = null, // ← Remove QR generation
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(request.TimeoutMinutes),
                    Instructions = new List<string>
            {
                "1. Mở ứng dụng ngân hàng hoặc ví điện tử của bạn",
                "2. Quét mã QR hoặc chuyển khoản thủ công theo thông tin bên dưới",
                "3. Nhập ĐÚNG SỐ TIỀN và NỘI DUNG CHUYỂN KHOẢN",
                "4. Xác nhận giao dịch",
                "5. Hệ thống sẽ tự động xác nhận đơn hàng sau khi nhận được tiền (1-5 phút)"
            }
                };

                return ApiResponse.SuccessResult(response, "SePay payment created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SePay payment for order {OrderId}", request.OrderId);
                return ApiResponse.ErrorResult<SePayResponseDto>("Failed to create SePay payment");
            }
        }

        public async Task<ApiResponse<SePayWebhookResponseDto>> ProcessSePayWebhookAsync(SePayWebhookDto webhookData, string apiKey)
        {
            try
            {
                _logger.LogInformation("Processing SePay webhook: TransactionId={Id}, Amount={Amount}, Code={Code}",
                    webhookData.Id, webhookData.TransferAmount, webhookData.Code);

                // Verify API Key
                var expectedApiKey = _configuration["Payment:SePay:ApiKey"];
                if (!string.IsNullOrEmpty(expectedApiKey) && apiKey != expectedApiKey)
                {
                    _logger.LogWarning("Invalid API Key in SePay webhook");
                    return ApiResponse.ErrorResult<SePayWebhookResponseDto>("Invalid API Key");
                }

                // Verify transfer type is "in" (tiền vào)
                if (webhookData.TransferType?.ToLower() != "in")
                {
                    _logger.LogInformation("Ignoring SePay webhook with transfer type: {TransferType}", webhookData.TransferType);
                    return ApiResponse.SuccessResult(new SePayWebhookResponseDto
                    {
                        Success = true,
                        Message = "Transfer type ignored",
                        TransactionId = null,
                        OrderNumber = null,
                        Amount = null,
                        Status = "Ignored"
                    });
                }

                // Extract payment code from webhook
                var paymentCode = webhookData.Code;
                if (string.IsNullOrEmpty(paymentCode))
                {
                    _logger.LogWarning("No payment code found in SePay webhook");
                    return ApiResponse.ErrorResult<SePayWebhookResponseDto>("No payment code found");
                }

                // Extract order ID from payment code (e.g., "SAKURA001001" -> 1001)
                var prefix = _configuration["Payment:SePay:PaymentPrefix"] ?? "SAKURA";
                if (!paymentCode.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Invalid payment code format: {Code}", paymentCode);
                    return ApiResponse.ErrorResult<SePayWebhookResponseDto>("Invalid payment code format");
                }

                var orderIdString = paymentCode.Substring(prefix.Length);
                if (!int.TryParse(orderIdString, out int orderId))
                {
                    _logger.LogWarning("Could not parse order ID from payment code: {Code}", paymentCode);
                    return ApiResponse.ErrorResult<SePayWebhookResponseDto>("Invalid order ID in payment code");
                }

                // Find pending payment transaction for this order
                var paymentTransaction = await _context.PaymentTransactions
                    .Include(pt => pt.Order)
                    .ThenInclude(o => o.User)
                    .Where(pt => pt.OrderId == orderId &&
                                pt.PaymentMethod == "SEPAY" &&
                                pt.Status == PaymentStatus.Pending)
                    .OrderByDescending(pt => pt.CreatedAt)
                    .FirstOrDefaultAsync();

                if (paymentTransaction == null)
                {
                    _logger.LogWarning("No pending SePay payment found for order {OrderId}", orderId);
                    return ApiResponse.ErrorResult<SePayWebhookResponseDto>("No pending payment found for this order");
                }

                // Verify amount matches
                if (Math.Abs(webhookData.TransferAmount - paymentTransaction.Amount) > 0.01m)
                {
                    _logger.LogWarning("Payment amount mismatch. Expected: {Expected}, Received: {Received}",
                        paymentTransaction.Amount, webhookData.TransferAmount);
                    return ApiResponse.ErrorResult<SePayWebhookResponseDto>("Payment amount mismatch");
                }

                // Update payment status
                paymentTransaction.Status = PaymentStatus.Paid;
                paymentTransaction.ExternalTransactionId = webhookData.ReferenceCode;
                paymentTransaction.CompletedAt = DateTime.Parse(webhookData.TransactionDate);
                paymentTransaction.ResponseMessage = $"Paid via {webhookData.Gateway}";
                paymentTransaction.ResponseData = System.Text.Json.JsonSerializer.Serialize(webhookData);

                // Update order payment status
                var order = paymentTransaction.Order;
                order.PaymentStatus = PaymentStatus.Paid;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                try
                {
                    // Gửi thông báo đến User cụ thể sở hữu đơn hàng này
                    // Frontend cần lắng nghe sự kiện: "ReceivePaymentStatus"
                    await _hubContext.Clients.User(order.User.Id.ToString())
                        .SendAsync("ReceivePaymentStatus", new
                        {
                            Success = true,
                            OrderId = order.Id,
                            OrderNumber = order.OrderNumber,
                            Status = "Paid",
                            TransactionId = paymentTransaction.TransactionId,
                            Message = "Thanh toán thành công!"
                        });

                    _logger.LogInformation("Sent SignalR notification for Order {OrderId}", order.Id);
                }
                catch (Exception ex)
                {
                    // Log lỗi nhưng KHÔNG throw exception để tránh làm fail quy trình Webhook
                    _logger.LogError(ex, "Error sending SignalR notification for Order {OrderId}", order.Id);
                }
                // ===========================================================================

                _logger.LogInformation("SePay payment processed successfully for order {OrderId}", orderId);

                return ApiResponse.SuccessResult(new SePayWebhookResponseDto
                {
                    Success = true,
                    Message = "Webhook processed successfully",
                    TransactionId = paymentTransaction.TransactionId,
                    OrderNumber = order.OrderNumber,
                    Amount = paymentTransaction.Amount,
                    Status = "Paid"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SePay webhook");
                return ApiResponse.ErrorResult<SePayWebhookResponseDto>("Failed to process webhook");
            }
        }

        public async Task<ApiResponse<QRCodePaymentResponseDto>> CreateQRCodePaymentAsync(QRCodePaymentRequestDto request, Guid userId)
        {
            try
            {
                _logger.LogInformation("Creating QR Code payment for order {OrderId} by user {UserId}", request.OrderId, userId);

                // Validate order
                var order = await _context.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.User.Id == userId);

                if (order == null)
                {
                    return ApiResponse.ErrorResult<QRCodePaymentResponseDto>("Order not found");
                }

                if (order.PaymentStatus == PaymentStatus.Paid)
                {
                    return ApiResponse.ErrorResult<QRCodePaymentResponseDto>("Order is already paid");
                }

                // Get QR Code configuration
                var qrConfig = _configuration.GetSection("Payment:QRCode");
                var bankCode = request.BankCode ?? qrConfig["BankCode"] ?? "MB";
                var accountNumber = request.AccountNumber ?? qrConfig["AccountNumber"] ?? "0396966376";
                var accountName = request.AccountName ?? qrConfig["AccountName"] ?? "NGUYEN THANH DAT";

                // Generate transaction ID
                var transactionId = GenerateTransactionId();
                var transferContent = $"ORD{request.OrderId:D6}";

                // Create payment transaction
                var paymentTransaction = new PaymentTransaction
                {
                    TransactionId = transactionId,
                    OrderId = request.OrderId,
                    User = order.User,
                    PaymentMethod = "QRCODE",
                    Amount = request.Amount,
                    Currency = "VND",
                    Status = PaymentStatus.Pending,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PaymentTransactions.Add(paymentTransaction);

                // Update order payment status
                order.PaymentStatus = PaymentStatus.Pending;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("QR Code payment transaction {TransactionId} created for order {OrderId}",
                    transactionId, request.OrderId);

                // Generate QR Code URL
                var qrCodeUrl = GenerateVietQRUrl(
                    bankCode: bankCode,
                    accountNumber: accountNumber,
                    accountName: accountName,
                    amount: request.Amount,
                    description: transferContent,
                    template: request.Template
                );

                // Create response
                var response = new QRCodePaymentResponseDto
                {
                    TransactionId = transactionId,
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    Amount = request.Amount,
                    Description = request.Description,
                    QRCodeUrl = qrCodeUrl,
                    BankAccount = new BankAccountInfo
                    {
                        BankName = GetBankName(bankCode),
                        BankCode = bankCode,
                        AccountNumber = accountNumber,
                        AccountHolder = accountName
                    },
                    TransferContent = transferContent,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15)
                };

                return ApiResponse.SuccessResult(response, "QR Code payment created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating QR Code payment for order {OrderId}", request.OrderId);
                return ApiResponse.ErrorResult<QRCodePaymentResponseDto>("Failed to create QR Code payment");
            }
        }

        #region Private Helper Methods

        private string GenerateVietQRUrl(string bankCode, string accountNumber, string accountName, decimal amount, string description, string template = "compact")
        {
            try
            {
                // VietQR API format
                var baseUrl = "https://img.vietqr.io/image";
                var qrUrl = $"{baseUrl}/{bankCode}-{accountNumber}-{template}.png";

                var queryParams = new List<string>
        {
            $"amount={amount:F0}",
            $"addInfo={Uri.EscapeDataString(description)}",
            $"accountName={Uri.EscapeDataString(accountName)}"
        };

                qrUrl += "?" + string.Join("&", queryParams);

                return qrUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating VietQR URL");
                return string.Empty;
            }
        }

        private string GetBankName(string bankCode)
        {
            return bankCode.ToUpper() switch
            {
                "VCB" => "Vietcombank",
                "TCB" => "Techcombank",
                "MB" or "MBB" => "MB Bank",
                "VIB" => "VIB",
                "ICB" => "VietinBank",
                "ACB" => "ACB",
                "SHB" => "SHB",
                "VPB" => "VPBank",
                "TPB" => "TPBank",
                "STB" => "Sacombank",
                "HDB" => "HDBank",
                "BIDV" => "BIDV",
                "OCB" => "OCB",
                "MSB" => "MSB",
                _ => bankCode
            };
        }
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
                PaymentMethod.SePay => "SEPAY",         
                PaymentMethod.VNPay => "VNPAY",         
                PaymentMethod.MoMo => "MOMO",
                _ => "COD"  // Default to COD
            };
        }

        private PaymentMethod ParsePaymentMethod(string code)
        {
            return code.ToUpper() switch
            {
                "COD" => PaymentMethod.COD,
                "BANK_TRANSFER" => PaymentMethod.BankTransfer,
                "CREDIT_CARD" => PaymentMethod.CreditCard,
                "DEBIT_CARD" => PaymentMethod.DebitCard,
                "MOMO" => PaymentMethod.MoMo,         
                "ZALOPAY" => PaymentMethod.EWallet,
                "QRCODE" => PaymentMethod.QRCode,
                "INSTALLMENT" => PaymentMethod.Installment,
                "SEPAY" => PaymentMethod.SePay,          
                "VNPAY" => PaymentMethod.VNPay,          
                _ => PaymentMethod.COD
            };
        }

        private string GetPaymentMethodName(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.COD => "Thanh toán khi nhận hàng",
                PaymentMethod.BankTransfer => "Chuyển khoản ngân hàng",
                PaymentMethod.CreditCard => "Thẻ tín dụng",
                PaymentMethod.DebitCard => "Thẻ ghi nợ",
                PaymentMethod.EWallet => "Ví điện tử",
                PaymentMethod.QRCode => "Quét mã QR",
                PaymentMethod.Installment => "Trả góp",
                PaymentMethod.SePay => "Chuyển khoản SePay",    
                PaymentMethod.VNPay => "VNPay",                 
                PaymentMethod.MoMo => "Ví MoMo",                
                _ => "Thanh toán khi nhận hàng"
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
                PaymentStatus.Pending => "Chờ thanh toán",
                PaymentStatus.Processing => "Đang xử lý",
                PaymentStatus.Paid => "Đã thanh toán",
                PaymentStatus.Failed => "Thanh toán thất bại",
                PaymentStatus.Cancelled => "Đã hủy",
                PaymentStatus.Refunded => "Đã hoàn tiền",
                PaymentStatus.PartiallyRefunded => "Hoàn tiền một phần",
                PaymentStatus.Expired => "Hết hạn",
                PaymentStatus.Confirmed => "Đã xác nhận",
                _ => "Không xác ??nh"
            };
        }

        private string? GetPaymentInstructions(string code)
        {
            return code.ToUpper() switch
            {
                "COD" => "Thanh toán bằng tiền mặt khi nhận hàng. Shipper sẽ thu tiền khi giao hàng.",
                "SEPAY" => "Chuyển khoản ngân hàng với xác nhận tự động qua SePay.",         // ← Thêm
                "QRCODE" => "Quét mã QR Code bằng ứng dụng ngân hàng để thanh toán nhanh chóng.",  // ← Thêm
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