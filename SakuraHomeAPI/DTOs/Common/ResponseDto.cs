namespace SakuraHomeAPI.DTOs.Common
{
    /// <summary>
    /// Standard API response wrapper
    /// </summary>
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponseDto<T> SuccessResult(T data, string? message = null)
        {
            return new ApiResponseDto<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponseDto<T> ErrorResult(string message, IEnumerable<string>? errors = null)
        {
            return new ApiResponseDto<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }

    /// <summary>
    /// Standard API response without data
    /// </summary>
    public class ApiResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponseDto SuccessResult(string? message = null)
        {
            return new ApiResponseDto
            {
                Success = true,
                Message = message
            };
        }

        public static ApiResponseDto ErrorResult(string message, IEnumerable<string>? errors = null)
        {
            return new ApiResponseDto
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}