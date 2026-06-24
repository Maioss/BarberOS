namespace BarberOS.Api.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; private set; }
        public T? Data { get; private set; }
        public string? Message { get; private set; }
        public List<string>? Errors { get; private set; }

        private ApiResponse() { }

        public static ApiResponse<T> Ok(T data, string? message = null) =>
            new() { Success = true, Data = data, Message = message };

        public static ApiResponse<T> Fail(string message, List<string>? errors = null) =>
            new() { Success = false, Message = message, Errors = errors };
    }
}
