using SakuraHomeAPI.Models.DTOs;
using SakuraHomeAPI.Models.Entities.Orders;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.Services.Interfaces
{
    /// <summary>
    /// Interface for email services
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Send email using template
        /// </summary>
        Task<ApiResponse> SendEmailAsync(string to, string templateName, object model, string? subject = null, string? language = "vi");

        /// <summary>
        /// Send welcome email to new user
        /// </summary>
        Task<ApiResponse> SendWelcomeEmailAsync(string email, string userName, string language = "vi");

        /// <summary>
        /// Send order confirmation email
        /// </summary>
        Task<ApiResponse> SendOrderConfirmationEmailAsync(Order order);

        /// <summary>
        /// Send order status update email
        /// </summary>
        Task<ApiResponse> SendOrderStatusUpdateEmailAsync(Order order, OrderStatus newStatus);

        /// <summary>
        /// Send password reset email
        /// </summary>
        Task<ApiResponse> SendPasswordResetEmailAsync(string email, string resetToken, string userName, string language = "vi");

        /// <summary>
        /// Send email verification
        /// </summary>
        Task<ApiResponse> SendEmailVerificationAsync(string email, string verificationToken, string userName, string language = "vi");

        /// <summary>
        /// Send order shipment notification
        /// </summary>
        Task<ApiResponse> SendShipmentNotificationEmailAsync(Order order, string? trackingNumber = null);

        /// <summary>
        /// Send order delivery confirmation
        /// </summary>
        Task<ApiResponse> SendDeliveryConfirmationEmailAsync(Order order);

        /// <summary>
        /// Send return/refund notification
        /// </summary>
        Task<ApiResponse> SendReturnNotificationEmailAsync(Order order, string reason);

        /// <summary>
        /// Send promotional email
        /// </summary>
        Task<ApiResponse> SendPromotionalEmailAsync(string email, string templateName, object model, string subject, string language = "vi");

        /// <summary>
        /// Queue email for batch sending
        /// </summary>
        Task<ApiResponse> QueueEmailAsync(string to, string subject, string body, EmailType type = EmailType.General, int priority = 1);

        /// <summary>
        /// Process queued emails
        /// </summary>
        Task<ApiResponse> ProcessQueuedEmailsAsync(int batchSize = 10);

        /// <summary>
        /// Get email template
        /// </summary>
        Task<Models.Entities.NotificationTemplate?> GetEmailTemplateAsync(string templateName, string language = "vi");

        /// <summary>
        /// Test email configuration
        /// </summary>
        Task<ApiResponse> TestEmailConfigurationAsync(string testEmail);
    }
}