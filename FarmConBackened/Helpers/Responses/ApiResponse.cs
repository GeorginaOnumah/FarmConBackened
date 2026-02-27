namespace FarmConBackened.Helpers.Responses
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse Ok(string message = "Success") =>
            new() { Success = true, Message = message };

        public static ApiResponse Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
