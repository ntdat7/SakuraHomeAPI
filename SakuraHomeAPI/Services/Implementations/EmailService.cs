using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Models.Entities.Orders;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.Services.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace SakuraHomeAPI.Services.Implementations
{
    /// <summary>
    /// Email service implementation using SMTP
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly SmtpClient _smtpClient;

        public EmailService(
            ApplicationDbContext context,
            ILogger<EmailService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _smtpClient = CreateSmtpClient();
        }

        public async Task<ApiResponse> SendEmailAsync(string to, string templateName, object model, string? subject = null, string? language = "vi")
        {
            try
            {
                var template = await GetEmailTemplateAsync(templateName, language);
                if (template == null)
                {
                    return ApiResponse.ErrorResult($"Email template '{templateName}' not found for language '{language}'");
                }

                var emailSubject = subject ?? template.Subject;
                var emailBody = await ProcessTemplate(template.BodyTemplate, model);

                return await SendRawEmailAsync(to, emailSubject, emailBody, EmailType.General);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email} with template {Template}", to, templateName);
                return ApiResponse.ErrorResult("Failed to send email");
            }
        }

        public async Task<ApiResponse> SendWelcomeEmailAsync(string email, string userName, string language = "vi")
        {
            var model = new
            {
                UserName = userName,
                Email = email,
                WelcomeMessage = "Chào m?ng b?n ??n v?i Sakura Home!",
                LoginUrl = $"{_configuration["Frontend:BaseUrl"]}/login",
                SupportEmail = _configuration["Email:SupportEmail"]
            };

            return await SendEmailAsync(email, "Welcome Email", model, null, language);
        }

        public async Task<ApiResponse> SendOrderConfirmationEmailAsync(Order order)
        {
            try
            {
                var model = new
                {
                    UserName = order.User.FullName,
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    TotalAmount = order.TotalAmount.ToString("N0"),
                    Currency = order.Currency,
                    Items = order.OrderItems.Select(item => new
                    {
                        ProductName = item.ProductName,
                        SKU = item.ProductSku,
                        Quantity = item.Quantity,
                        Price = item.UnitPrice.ToString("N0"),
                        Total = (item.Quantity * item.UnitPrice).ToString("N0")
                    }).ToList(),
                    ShippingAddress = order.ShippingAddress,
                    PaymentMethod = GetPaymentMethodName(order.PaymentMethod),
                    DeliveryMethod = GetDeliveryMethodName(order.DeliveryMethod),
                    EstimatedDelivery = order.EstimatedDeliveryDate?.ToString("dd/MM/yyyy"),
                    TrackingUrl = $"{_configuration["Frontend:BaseUrl"]}/orders/{order.Id}/tracking"
                };

                return await SendEmailAsync(order.User.Email, "Order Confirmation", model, $"Xác nh?n ??n hàng #{order.OrderNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order confirmation email for order {OrderId}", order.Id);
                return ApiResponse.ErrorResult("Failed to send order confirmation email");
            }
        }

        public async Task<ApiResponse> SendOrderStatusUpdateEmailAsync(Order order, OrderStatus newStatus)
        {
            try
            {
                var model = new
                {
                    UserName = order.User.FullName,
                    OrderNumber = order.OrderNumber,
                    NewStatus = GetOrderStatusText(newStatus),
                    UpdateTime = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm"),
                    TrackingUrl = $"{_configuration["Frontend:BaseUrl"]}/orders/{order.Id}/tracking",
                    SupportEmail = _configuration["Email:SupportEmail"]
                };

                var templateName = newStatus switch
                {
                    OrderStatus.Confirmed => "Order Confirmed",
                    OrderStatus.Processing => "Order Processing",
                    OrderStatus.Shipped => "Order Shipped",
                    OrderStatus.Delivered => "Order Delivered",
                    OrderStatus.Cancelled => "Order Cancelled",
                    OrderStatus.Returned => "Order Returned",
                    _ => "Order Status Update"
                };

                var subject = $"C?p nh?t ??n hàng #{order.OrderNumber} - {GetOrderStatusText(newStatus)}";

                return await SendEmailAsync(order.User.Email, templateName, model, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order status update email for order {OrderId}", order.Id);
                return ApiResponse.ErrorResult("Failed to send order status update email");
            }
        }

        public async Task<ApiResponse> SendPasswordResetEmailAsync(string email, string resetToken, string userName, string language = "vi")
        {
            var model = new
            {
                UserName = userName,
                ResetUrl = $"{_configuration["Frontend:BaseUrl"]}/reset-password?token={resetToken}",
                ExpiryHours = _configuration["JWT:PasswordResetTokenExpiryHours"] ?? "24",
                SupportEmail = _configuration["Email:SupportEmail"]
            };

            return await SendEmailAsync(email, "Password Reset", model, "??t l?i m?t kh?u tài kho?n", language);
        }

        public async Task<ApiResponse> SendEmailVerificationAsync(string email, string verificationToken, string userName, string language = "vi")
        {
            var model = new
            {
                UserName = userName,
                VerificationUrl = $"{_configuration["Frontend:BaseUrl"]}/verify-email?token={verificationToken}",
                SupportEmail = _configuration["Email:SupportEmail"]
            };

            return await SendEmailAsync(email, "Email Verification", model, "Xác th?c ??a ch? email", language);
        }

        public async Task<ApiResponse> SendShipmentNotificationEmailAsync(Order order, string? trackingNumber = null)
        {
            try
            {
                var model = new
                {
                    UserName = order.User.FullName,
                    OrderNumber = order.OrderNumber,
                    TrackingNumber = trackingNumber ?? "S? ???c c?p nh?t s?m",
                    EstimatedDelivery = order.EstimatedDeliveryDate?.ToString("dd/MM/yyyy"),
                    CarrierName = GetCarrierName(order.ShippingCarrier),
                    TrackingUrl = !string.IsNullOrEmpty(trackingNumber) ? 
                        $"{_configuration["Frontend:BaseUrl"]}/tracking/{trackingNumber}" : null,
                    OrderTrackingUrl = $"{_configuration["Frontend:BaseUrl"]}/orders/{order.Id}/tracking"
                };

                return await SendEmailAsync(order.User.Email, "Order Shipped", model, $"??n hàng #{order.OrderNumber} ?ã ???c giao cho v?n chuy?n");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending shipment notification email for order {OrderId}", order.Id);
                return ApiResponse.ErrorResult("Failed to send shipment notification email");
            }
        }

        public async Task<ApiResponse> SendDeliveryConfirmationEmailAsync(Order order)
        {
            try
            {
                var model = new
                {
                    UserName = order.User.FullName,
                    OrderNumber = order.OrderNumber,
                    DeliveryDate = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm"),
                    ReviewUrl = $"{_configuration["Frontend:BaseUrl"]}/orders/{order.Id}/review",
                    SupportEmail = _configuration["Email:SupportEmail"]
                };

                return await SendEmailAsync(order.User.Email, "Order Delivered", model, $"??n hàng #{order.OrderNumber} ?ã ???c giao thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending delivery confirmation email for order {OrderId}", order.Id);
                return ApiResponse.ErrorResult("Failed to send delivery confirmation email");
            }
        }

        public async Task<ApiResponse> SendReturnNotificationEmailAsync(Order order, string reason)
        {
            try
            {
                var model = new
                {
                    UserName = order.User.FullName,
                    OrderNumber = order.OrderNumber,
                    ReturnReason = reason,
                    ReturnDate = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm"),
                    RefundAmount = order.TotalAmount.ToString("N0"),
                    ProcessingDays = "3-5 ngày làm vi?c",
                    SupportEmail = _configuration["Email:SupportEmail"]
                };

                return await SendEmailAsync(order.User.Email, "Return Processed", model, $"X? lý tr? hàng ??n #{order.OrderNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending return notification email for order {OrderId}", order.Id);
                return ApiResponse.ErrorResult("Failed to send return notification email");
            }
        }

        public async Task<ApiResponse> SendPromotionalEmailAsync(string email, string templateName, object model, string subject, string language = "vi")
        {
            return await SendEmailAsync(email, templateName, model, subject, language);
        }

        public async Task<ApiResponse> QueueEmailAsync(string to, string subject, string body, EmailType type = EmailType.General, int priority = 1)
        {
            try
            {
                var emailQueue = new EmailQueue
                {
                    To = to,
                    Subject = subject,
                    Body = body,
                    Type = type,
                    Priority = priority,
                    Status = EmailDeliveryStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    Attempts = 0
                };

                _context.EmailQueues.Add(emailQueue);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Email queued for {Email} with subject: {Subject}", to, subject);
                return ApiResponse.SuccessResult("Email queued successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error queuing email for {Email}", to);
                return ApiResponse.ErrorResult("Failed to queue email");
            }
        }

        public async Task<ApiResponse> ProcessQueuedEmailsAsync(int batchSize = 10)
        {
            try
            {
                var pendingEmails = await _context.EmailQueues
                    .Where(eq => eq.Status == EmailDeliveryStatus.Pending || eq.Status == EmailDeliveryStatus.Retry)
                    .Where(eq => eq.NextRetryAt == null || eq.NextRetryAt <= DateTime.UtcNow)
                    .OrderBy(eq => eq.Priority)
                    .ThenBy(eq => eq.CreatedAt)
                    .Take(batchSize)
                    .ToListAsync();

                int processed = 0;
                foreach (var email in pendingEmails)
                {
                    try
                    {
                        email.Status = EmailDeliveryStatus.Sending;
                        email.Attempts++;
                        await _context.SaveChangesAsync();

                        var result = await SendRawEmailAsync(email.To, email.Subject, email.Body, email.Type);
                        
                        if (result.Success)
                        {
                            email.Status = EmailDeliveryStatus.Sent;
                            email.SentAt = DateTime.UtcNow;
                            processed++;
                        }
                        else
                        {
                            email.Status = email.Attempts >= 3 ? EmailDeliveryStatus.Failed : EmailDeliveryStatus.Retry;
                            email.ErrorMessage = result.Message;
                            if (email.Status == EmailDeliveryStatus.Retry)
                            {
                                email.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, email.Attempts) * 5); // Exponential backoff
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing queued email {EmailId}", email.Id);
                        email.Status = email.Attempts >= 3 ? EmailDeliveryStatus.Failed : EmailDeliveryStatus.Retry;
                        email.ErrorMessage = ex.Message;
                        if (email.Status == EmailDeliveryStatus.Retry)
                        {
                            email.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, email.Attempts) * 5);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Processed {ProcessedCount}/{TotalCount} queued emails", processed, pendingEmails.Count);
                return ApiResponse.SuccessResult($"Processed {processed}/{pendingEmails.Count} emails");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing queued emails");
                return ApiResponse.ErrorResult("Failed to process queued emails");
            }
        }

        public async Task<NotificationTemplate?> GetEmailTemplateAsync(string templateName, string language = "vi")
        {
            return await _context.NotificationTemplates
                .FirstOrDefaultAsync(nt => nt.Name == templateName && 
                                         nt.Language == language && 
                                         nt.IsActive);
        }

        public async Task<ApiResponse> TestEmailConfigurationAsync(string testEmail)
        {
            try
            {
                var testSubject = "Test Email Configuration - Sakura Home";
                var testBody = @"
                    <h2>Email Configuration Test</h2>
                    <p>This is a test email to verify your email configuration is working correctly.</p>
                    <p>If you receive this email, your SMTP settings are configured properly.</p>
                    <p>Test sent at: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC") + @"</p>
                    <br>
                    <p>Best regards,<br>Sakura Home Team</p>
                ";

                return await SendRawEmailAsync(testEmail, testSubject, testBody, EmailType.General);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing email configuration");
                return ApiResponse.ErrorResult($"Email configuration test failed: {ex.Message}");
            }
        }

        #region Private Methods

        private SmtpClient CreateSmtpClient()
        {
            var smtpConfig = _configuration.GetSection("Email:Smtp");
            
            var client = new SmtpClient(smtpConfig["Host"])
            {
                Port = int.Parse(smtpConfig["Port"] ?? "587"),
                EnableSsl = bool.Parse(smtpConfig["EnableSsl"] ?? "true"),
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpConfig["Username"], smtpConfig["Password"])
            };

            return client;
        }

        private async Task<ApiResponse> SendRawEmailAsync(string to, string subject, string body, EmailType type)
        {
            try
            {
                var fromEmail = _configuration["Email:FromEmail"];
                var fromName = _configuration["Email:FromName"];

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await _smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent successfully to {Email} with subject: {Subject}", to, subject);
                return ApiResponse.SuccessResult("Email sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", to);
                return ApiResponse.ErrorResult($"Failed to send email: {ex.Message}");
            }
        }

        private async Task<string> ProcessTemplate(string template, object model)
        {
            try
            {
                var json = JsonSerializer.Serialize(model);
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                var processedTemplate = template;
                
                if (data != null)
                {
                    foreach (var kvp in data)
                    {
                        var placeholder = "{" + kvp.Key + "}";
                        var value = kvp.Value?.ToString() ?? "";
                        processedTemplate = processedTemplate.Replace(placeholder, value);
                    }
                }

                return processedTemplate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email template");
                return template; // Return original template if processing fails
            }
        }

        private string GetPaymentMethodName(PaymentMethod? method)
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
                _ => "Không xác ??nh"
            };
        }

        private string GetDeliveryMethodName(DeliveryMethod? method)
        {
            return method switch
            {
                DeliveryMethod.Standard => "Giao hàng tiêu chu?n",
                DeliveryMethod.Express => "Giao hàng nhanh",
                DeliveryMethod.SuperFast => "Giao hàng siêu t?c",
                DeliveryMethod.SelfPickup => "T? ??n l?y",
                _ => "Không xác ??nh"
            };
        }

        private string GetOrderStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Ch? x? lý",
                OrderStatus.Confirmed => "?ã xác nh?n",
                OrderStatus.Processing => "?ang x? lý",
                OrderStatus.Packed => "?ã ?óng gói",
                OrderStatus.Shipped => "?ang v?n chuy?n",
                OrderStatus.Delivered => "?ã giao hàng",
                OrderStatus.Cancelled => "?ã h?y",
                OrderStatus.Returned => "?ã tr? hàng",
                OrderStatus.Refunded => "?ã hoàn ti?n",
                _ => "Không xác ??nh"
            };
        }

        private string GetCarrierName(string? carrier)
        {
            return carrier switch
            {
                "GHN" => "Giao Hàng Nhanh",
                "GHTK" => "Giao Hàng Ti?t Ki?m",
                "VTP" => "Viettel Post",
                "VNPOST" => "Vietnam Post",
                _ => carrier ?? "??n v? v?n chuy?n"
            };
        }

        #endregion

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
}