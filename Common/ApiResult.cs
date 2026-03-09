namespace HRManagement.DTOs.Common
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }

        public string? MessageCode { get; set; }

        public string? Message { get; set; }

        public T? Data { get; set; }

        public object? ExtraData { get; set; }

        public static ApiResult<T> Ok(T data, string? messageCode = null, string? message = null)
        {
            return new ApiResult<T>
            {
                Success = true,
                Data = data,
                MessageCode = messageCode,
                Message = message
            };
        }

        public static ApiResult<T> Fail(string messageCode, string message, object? extraData = null)
        {
            return new ApiResult<T>
            {
                Success = false,
                MessageCode = messageCode,
                Message = message,
                ExtraData = extraData
            };
        }
    }
}