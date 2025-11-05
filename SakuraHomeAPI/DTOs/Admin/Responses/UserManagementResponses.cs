using SakuraHomeAPI.DTOs.Users.Responses;

namespace SakuraHomeAPI.DTOs.Admin.Responses
{
    /// <summary>
    /// Response khi check email availability
    /// </summary>
    public class EmailCheckResponse
    {
        public bool Exists { get; set; }
        public bool Available { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response khi check national ID card (CCCD) availability
    /// </summary>
    public class NationalIdCheckResponse
    {
        public bool Exists { get; set; }
        public bool Available { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserSummaryDto? ExistingUser { get; set; }
    }
}