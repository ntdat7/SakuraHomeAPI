namespace SakuraHomeAPI.Services.Common
{
    /// <summary>
    /// Generic service result wrapper for consistent API responses
    /// Provides standardized success/error handling across all services
    /// </summary>
    /// <typeparam name="T">Type of data returned on success</typeparam>
    public class ServiceResult<T>
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// The data returned when operation is successful
        /// </summary>
        public T? Data { get; private set; }

        /// <summary>
        /// Error message when operation fails
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// List of validation errors or detailed error information
        /// </summary>
        public IEnumerable<string> Errors { get; private set; } = new List<string>();

        /// <summary>
        /// HTTP status code for API responses
        /// </summary>
        public int StatusCode { get; private set; } = 200;

        /// <summary>
        /// Additional metadata for the result
        /// </summary>
        public Dictionary<string, object>? Metadata { get; private set; }

        /// <summary>
        /// Private constructor to enforce factory methods
        /// </summary>
        private ServiceResult() { }

        #region Success Factory Methods

        /// <summary>
        /// Create successful result with data
        /// </summary>
        public static ServiceResult<T> Success(T data, string? message = null, Dictionary<string, object>? metadata = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data,
                ErrorMessage = message,
                StatusCode = 200,
                Metadata = metadata
            };
        }

        /// <summary>
        /// Create successful result for creation operations
        /// </summary>
        public static ServiceResult<T> Created(T data, string? message = null, Dictionary<string, object>? metadata = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data,
                ErrorMessage = message,
                StatusCode = 201,
                Metadata = metadata
            };
        }

        /// <summary>
        /// Create successful result with no content
        /// </summary>
        public static ServiceResult<T> NoContent(string? message = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = default,
                ErrorMessage = message,
                StatusCode = 204
            };
        }

        #endregion

        #region Error Factory Methods

        /// <summary>
        /// Create failure result with error message
        /// </summary>
        public static ServiceResult<T> Failure(string errorMessage, int statusCode = 400)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Data = default,
                ErrorMessage = errorMessage,
                StatusCode = statusCode,
                Errors = new[] { errorMessage }
            };
        }

        /// <summary>
        /// Create failure result with multiple errors
        /// </summary>
        public static ServiceResult<T> Failure(IEnumerable<string> errors, string? mainErrorMessage = null, int statusCode = 400)
        {
            var errorList = errors.ToList();
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Data = default,
                ErrorMessage = mainErrorMessage ?? errorList.FirstOrDefault() ?? "Operation failed",
                StatusCode = statusCode,
                Errors = errorList
            };
        }

        /// <summary>
        /// Create not found result
        /// </summary>
        public static ServiceResult<T> NotFound(string? message = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Data = default,
                ErrorMessage = message ?? "Resource not found",
                StatusCode = 404,
                Errors = new[] { message ?? "Resource not found" }
            };
        }

        /// <summary>
        /// Create unauthorized result
        /// </summary>
        public static ServiceResult<T> Unauthorized(string? message = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Data = default,
                ErrorMessage = message ?? "Unauthorized access",
                StatusCode = 401,
                Errors = new[] { message ?? "Unauthorized access" }
            };
        }

        /// <summary>
        /// Create forbidden result
        /// </summary>
        public static ServiceResult<T> Forbidden(string? message = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Data = default,
                ErrorMessage = message ?? "Access forbidden",
                StatusCode = 403,
                Errors = new[] { message ?? "Access forbidden" }
            };
        }

        /// <summary>
        /// Create conflict result
        /// </summary>
        public static ServiceResult<T> Conflict(string? message = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Data = default,
                ErrorMessage = message ?? "Resource conflict",
                StatusCode = 409,
                Errors = new[] { message ?? "Resource conflict" }
            };
        }

        /// <summary>
        /// Create validation error result
        /// </summary>
        public static ServiceResult<T> ValidationError(IEnumerable<string> validationErrors)
        {
            var errorList = validationErrors.ToList();
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Data = default,
                ErrorMessage = "Validation failed",
                StatusCode = 422,
                Errors = errorList
            };
        }

        /// <summary>
        /// Create internal server error result
        /// </summary>
        public static ServiceResult<T> InternalError(string? message = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Data = default,
                ErrorMessage = message ?? "Internal server error",
                StatusCode = 500,
                Errors = new[] { message ?? "Internal server error" }
            };
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Add metadata to the result
        /// </summary>
        public ServiceResult<T> WithMetadata(string key, object value)
        {
            Metadata ??= new Dictionary<string, object>();
            Metadata[key] = value;
            return this;
        }

        /// <summary>
        /// Add multiple metadata entries
        /// </summary>
        public ServiceResult<T> WithMetadata(Dictionary<string, object> metadata)
        {
            if (metadata == null) return this;
            
            Metadata ??= new Dictionary<string, object>();
            foreach (var kvp in metadata)
            {
                Metadata[kvp.Key] = kvp.Value;
            }
            return this;
        }

        /// <summary>
        /// Transform the data type while preserving error state
        /// </summary>
        public ServiceResult<TNew> Map<TNew>(Func<T, TNew> mapper)
        {
            if (!IsSuccess)
            {
                return ServiceResult<TNew>.Failure(Errors, ErrorMessage, StatusCode);
            }

            try
            {
                var newData = Data != null ? mapper(Data) : default;
                return ServiceResult<TNew>.Success(newData!, ErrorMessage, Metadata);
            }
            catch (Exception ex)
            {
                return ServiceResult<TNew>.Failure($"Mapping failed: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Convert to non-generic ServiceResult
        /// </summary>
        public ServiceResult ToNonGeneric()
        {
            if (IsSuccess)
            {
                return ServiceResult.Success(ErrorMessage, Metadata);
            }
            else
            {
                return ServiceResult.Failure(Errors, ErrorMessage, StatusCode);
            }
        }

        #endregion

        #region Implicit Operators

        /// <summary>
        /// Implicit conversion from T to ServiceResult&lt;T&gt;
        /// </summary>
        public static implicit operator ServiceResult<T>(T data)
        {
            return Success(data);
        }

        #endregion
    }

    /// <summary>
    /// Non-generic service result for operations that don't return data
    /// </summary>
    public class ServiceResult
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Error message when operation fails
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// List of validation errors or detailed error information
        /// </summary>
        public IEnumerable<string> Errors { get; private set; } = new List<string>();

        /// <summary>
        /// HTTP status code for API responses
        /// </summary>
        public int StatusCode { get; private set; } = 200;

        /// <summary>
        /// Additional metadata for the result
        /// </summary>
        public Dictionary<string, object>? Metadata { get; private set; }

        /// <summary>
        /// Private constructor to enforce factory methods
        /// </summary>
        private ServiceResult() { }

        #region Success Factory Methods

        /// <summary>
        /// Create successful result
        /// </summary>
        public static ServiceResult Success(string? message = null, Dictionary<string, object>? metadata = null)
        {
            return new ServiceResult
            {
                IsSuccess = true,
                ErrorMessage = message,
                StatusCode = 200,
                Metadata = metadata
            };
        }

        /// <summary>
        /// Create successful result for creation operations
        /// </summary>
        public static ServiceResult Created(string? message = null, Dictionary<string, object>? metadata = null)
        {
            return new ServiceResult
            {
                IsSuccess = true,
                ErrorMessage = message,
                StatusCode = 201,
                Metadata = metadata
            };
        }

        /// <summary>
        /// Create successful result with no content
        /// </summary>
        public static ServiceResult NoContent(string? message = null)
        {
            return new ServiceResult
            {
                IsSuccess = true,
                ErrorMessage = message,
                StatusCode = 204
            };
        }

        #endregion

        #region Error Factory Methods

        /// <summary>
        /// Create failure result with error message
        /// </summary>
        public static ServiceResult Failure(string errorMessage, int statusCode = 400)
        {
            return new ServiceResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                StatusCode = statusCode,
                Errors = new[] { errorMessage }
            };
        }

        /// <summary>
        /// Create failure result with multiple errors
        /// </summary>
        public static ServiceResult Failure(IEnumerable<string> errors, string? mainErrorMessage = null, int statusCode = 400)
        {
            var errorList = errors.ToList();
            return new ServiceResult
            {
                IsSuccess = false,
                ErrorMessage = mainErrorMessage ?? errorList.FirstOrDefault() ?? "Operation failed",
                StatusCode = statusCode,
                Errors = errorList
            };
        }

        /// <summary>
        /// Create not found result
        /// </summary>
        public static ServiceResult NotFound(string? message = null)
        {
            return Failure(message ?? "Resource not found", 404);
        }

        /// <summary>
        /// Create unauthorized result
        /// </summary>
        public static ServiceResult Unauthorized(string? message = null)
        {
            return Failure(message ?? "Unauthorized access", 401);
        }

        /// <summary>
        /// Create forbidden result
        /// </summary>
        public static ServiceResult Forbidden(string? message = null)
        {
            return Failure(message ?? "Access forbidden", 403);
        }

        /// <summary>
        /// Create conflict result
        /// </summary>
        public static ServiceResult Conflict(string? message = null)
        {
            return Failure(message ?? "Resource conflict", 409);
        }

        /// <summary>
        /// Create validation error result
        /// </summary>
        public static ServiceResult ValidationError(IEnumerable<string> validationErrors)
        {
            var errorList = validationErrors.ToList();
            return new ServiceResult
            {
                IsSuccess = false,
                ErrorMessage = "Validation failed",
                StatusCode = 422,
                Errors = errorList
            };
        }

        /// <summary>
        /// Create internal server error result
        /// </summary>
        public static ServiceResult InternalError(string? message = null)
        {
            return Failure(message ?? "Internal server error", 500);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Add metadata to the result
        /// </summary>
        public ServiceResult WithMetadata(string key, object value)
        {
            Metadata ??= new Dictionary<string, object>();
            Metadata[key] = value;
            return this;
        }

        /// <summary>
        /// Add multiple metadata entries
        /// </summary>
        public ServiceResult WithMetadata(Dictionary<string, object> metadata)
        {
            if (metadata == null) return this;
            
            Metadata ??= new Dictionary<string, object>();
            foreach (var kvp in metadata)
            {
                Metadata[kvp.Key] = kvp.Value;
            }
            return this;
        }

        #endregion
    }
}