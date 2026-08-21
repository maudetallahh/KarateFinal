using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace KarateFinal.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public EmailService(IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
        }

        public async Task SendAsync(string toEmail, string toName, string subject, string body)
        {
            var apiKey = _config["EmailSettings__SenderPassword"]
                      ?? _config["EmailSettings:SenderPassword"] ?? "";
            var senderEmail = _config["EmailSettings__SenderEmail"]
                           ?? _config["EmailSettings:SenderEmail"] ?? "";
            var senderName = _config["EmailSettings__SenderName"]
                          ?? _config["EmailSettings:SenderName"] ?? "";

            var payload = new
            {
                sender = new { name = senderName, email = senderEmail },
                to = new[] { new { email = toEmail, name = toName } },
                subject = subject,
                htmlContent = body
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            request.Headers.Add("api-key", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Brevo error: " + error);
            }
        }
    }
}