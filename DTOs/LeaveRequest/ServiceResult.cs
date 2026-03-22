namespace HRManagement.DTOs.LeaveRequest
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string MessageCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ServiceResult<T> Ok(string code, string message, T? data)
        {
            return new ServiceResult<T>
            {
                Success = true,
                MessageCode = code,
                Message = message,
                Data = data
            };
        }

        public static ServiceResult<T> Fail(string code, string message, T? data = default)
        {
            return new ServiceResult<T>
            {
                Success = false,
                MessageCode = code,
                Message = message,
                Data = data
            };
        }
    }
}