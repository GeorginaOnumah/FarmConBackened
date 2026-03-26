using System.Text.Json.Serialization;


namespace FarmConnect.Models.Paystack
    {
        public class PaystackInitResponse
        {
            [JsonPropertyName("status")] public bool Status { get; set; }
            [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
            [JsonPropertyName("data")] public PaystackInitData? Data { get; set; }
        }

        public class PaystackInitData
        {
            [JsonPropertyName("authorization_url")] public string AuthorizationUrl { get; set; } = string.Empty;
            [JsonPropertyName("access_code")] public string AccessCode { get; set; } = string.Empty;
            [JsonPropertyName("reference")] public string Reference { get; set; } = string.Empty;
        }

        public class PaystackVerifyResponse
        {
            [JsonPropertyName("status")] public bool Status { get; set; }
            [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
            [JsonPropertyName("data")] public PaystackVerifyData? Data { get; set; }
        }

        public class PaystackVerifyData
        {
            [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
            [JsonPropertyName("reference")] public string Reference { get; set; } = string.Empty;
            [JsonPropertyName("amount")] public long Amount { get; set; }
            [JsonPropertyName("currency")] public string Currency { get; set; } = string.Empty;
            [JsonPropertyName("transaction_date")] public DateTime TransactionDate { get; set; }
            [JsonPropertyName("gateway_response")] public string GatewayResponse { get; set; } = string.Empty;
            [JsonPropertyName("channel")] public string Channel { get; set; } = string.Empty;
            [JsonPropertyName("id")] public long Id { get; set; }
        }

        public class PaystackWebhookPayload
        {
            [JsonPropertyName("event")] public string Event { get; set; } = string.Empty;
            [JsonPropertyName("data")] public PaystackVerifyData? Data { get; set; }
        }
    }
