namespace SakuraHomeAPI.DTOs.Common
{
    /// <summary>
    /// Generic API response wrapper
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ApiResponse()
        {
        }

        public ApiResponse(T data, string message = "")
        {
            Success = true;
            Data = data;
            Message = message;
        }

        public ApiResponse(string message, List<string>? errors = null)
        {
            Success = false;
            Message = message;
            Errors = errors ?? new List<string>();
        }
    }

    /// <summary>
    /// Non-generic API response
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        public ApiResponse() : base()
        {
        }

        public ApiResponse(string message, List<string>? errors = null) : base(message, errors)
        {
        }

        public static ApiResponse SuccessResult(string message = "Thành công")
        {
            return new ApiResponse { Success = true, Message = message };
        }

        public static ApiResponse<T> SuccessResult<T>(T data, string message = "Thành công")
        {
            return new ApiResponse<T>(data, message);
        }

        public static ApiResponse ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse(message, errors);
        }

        public static ApiResponse<T> ErrorResult<T>(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>(message, errors);
        }
    }
}