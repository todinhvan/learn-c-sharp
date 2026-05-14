namespace CoreBanking.API.Models
{
    /// <summary>
    /// Represents the outcome of a service operation.
    /// Encapsulates success/failure, the result value, error message, and error type
    /// so that API endpoints can map to appropriate HTTP responses.
    /// </summary>
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; private init; }
        public T? Value { get; private init; }
        public string? Error { get; private init; }
        public ErrorType ErrorType { get; private init; }

        public static ServiceResult<T> Success(T value) => new()
        {
            IsSuccess = true,
            Value = value
        };

        public static ServiceResult<T> NotFound(string error) => new()
        {
            IsSuccess = false,
            Error = error,
            ErrorType = ErrorType.NotFound
        };

        public static ServiceResult<T> BadRequest(string error) => new()
        {
            IsSuccess = false,
            Error = error,
            ErrorType = ErrorType.BadRequest
        };

        public static ServiceResult<T> Conflict(string error) => new()
        {
            IsSuccess = false,
            Error = error,
            ErrorType = ErrorType.Conflict
        };
    }

    public enum ErrorType
    {
        None,
        NotFound,
        BadRequest,
        Conflict
    }
}
