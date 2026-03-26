using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FarmConBackened.DTOs.Paystack;

namespace FarmConBackened.Helpers
{
    public class PaystackHelper
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _config;

        private string Secret => _config["Paystack:SecretKey"]!;

        public PaystackHelper(IHttpClientFactory http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<PaystackInitData?> InitializeTransactionAsync(
            string email, long amount, string reference, object metadata)
        {
            var client = _http.CreateClient();

            var body = JsonSerializer.Serialize(new
            {
                email,
                amount, 
                reference,
                metadata
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.paystack.co/transaction/initialize")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Secret);

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<PaystackInitResponse>(json);
            return result?.Data;
        }

        public async Task<PaystackVerifyData?> VerifyTransactionAsync(string reference)
        {
            var client = _http.CreateClient();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.paystack.co/transaction/verify/{reference}");

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Secret);

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<PaystackVerifyResponse>(json);
            return result?.Data;
        }

        public async Task<bool> RefundAsync(string transactionId, long amount)
        {
            var client = _http.CreateClient();

            var body = JsonSerializer.Serialize(new
            {
                transaction = transactionId,
                amount
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.paystack.co/refund")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Secret);

            var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public bool VerifySignature(string body, string signature)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(Secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
            var computed = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return computed == signature;
        }
    }
}
