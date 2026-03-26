namespace FarmConBackened.DTOs.Paystack
{
    public class PaystackWebhookPayload
    {
        public string Event { get; set; } = string.Empty;
        public PaystackVerifyData? Data { get; set; }
    }
}
