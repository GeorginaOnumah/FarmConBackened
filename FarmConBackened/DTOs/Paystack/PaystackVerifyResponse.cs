namespace FarmConBackened.DTOs.Paystack
{
    public class PaystackVerifyResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public PaystackVerifyData? Data { get; set; }
    }

    public class PaystackVerifyData
    {
        public string Status { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string GatewayResponse { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public long Id { get; set; }
    }
}
