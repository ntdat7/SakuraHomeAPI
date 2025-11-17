using SakuraHomeAPI.Hubs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SakuraHomeAPI.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time notifications
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Called when client connects
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their personal group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

                // Add admin/staff to admin group
                var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
                if (role == "Admin" || role == "Staff" || role == "SuperAdmin")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
                    _logger.LogInformation("Admin/Staff {UserName} connected to notifications hub. ConnectionId: {ConnectionId}",
                        userName, Context.ConnectionId);
                }
                else
                {
                    _logger.LogInformation("User {UserName} connected to notifications hub. ConnectionId: {ConnectionId}",
                        userName, Context.ConnectionId);
                }
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when client disconnects
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

            _logger.LogInformation("User {UserName} disconnected from notifications hub. ConnectionId: {ConnectionId}",
                userName ?? "Unknown", Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        public async Task MarkAsRead(int notificationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("User {UserId} marked notification {NotificationId} as read",
                userId, notificationId);

            // Can add logic to update database here
            await Clients.Caller.SendAsync("NotificationRead", notificationId);
        }

        /// <summary>
        /// Get connection info (for debugging)
        /// </summary>
        public async Task GetConnectionInfo()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Clients.Caller.SendAsync("ConnectionInfo", new
            {
                ConnectionId = Context.ConnectionId,
                UserId = userId,
                ConnectedAt = DateTime.UtcNow
            });
        }
    }
}