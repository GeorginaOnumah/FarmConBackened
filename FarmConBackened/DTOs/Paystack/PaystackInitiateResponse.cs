namespace FarmConBackened.DTOs.Paystack
{
    public class PaystackInitResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public PaystackInitData? Data { get; set; }
    }

    public class PaystackInitData
    {
        public string AuthorizationUrl { get; set; } = string.Empty;
        public string AccessCode { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
    }
}
