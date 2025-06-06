using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Eatease.Service
{
    public class BrevoEmail
    {
        private static readonly string API_KEY;
        private static readonly HttpClient CLIENT = new HttpClient();

        static BrevoEmail()
        {
            var appsettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (!File.Exists(appsettingsPath))
            {
                throw new Exception("Ficheiro appsettings.json não encontrado em: " + appsettingsPath);
            }

            try
            {
                var jsonContent = File.ReadAllText(appsettingsPath);
                var json = JObject.Parse(jsonContent);

                API_KEY = json["Brevo"]?["ApiKey"]?.ToString() ?? string.Empty;

                if (string.IsNullOrEmpty(API_KEY))
                {
                    throw new Exception("Brevo:ApiKey não definido em appsettings.json");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao ler appsettings.json: {ex.Message}");
            }
        }

        public static async Task SendEmailAsync(
            string fromEmail, string fromName,
            string toEmail, string toName,
            string subject, string htmlContent)
        {
            // Monta o payload usando Newtonsoft.Json
            var payload = new JObject();
            var sender = new JObject
            {
                ["name"] = fromName,
                ["email"] = fromEmail
            };
            payload["sender"] = sender;

            var toArray = new JArray();
            var recipient = new JObject
            {
                ["email"] = toEmail,
                ["name"] = toName
            };
            toArray.Add(recipient);
            payload["to"] = toArray;

            payload["subject"] = subject;
            payload["htmlContent"] = htmlContent;

            var requestBody = payload.ToString();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            request.Headers.Accept.ParseAdd("application/json");
            request.Headers.Add("api-key", API_KEY);
            
            // Debug: Print the API key being used (first 10 characters for security)
            Console.WriteLine($"🔑 Using API Key: {API_KEY.Substring(0, Math.Min(10, API_KEY.Length))}...");

            HttpResponseMessage response;
            try
            {
                response = await CLIENT.SendAsync(request);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("❌ Erro ao enviar o pedido HTTP: " + ex.Message);
                return;
            }

            var body = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ Email enviado com sucesso! Resposta: " + body);
            }
            else
            {
                Console.Error.WriteLine($"❌ Falha ao enviar email. Código: {(int)response.StatusCode} — {body}");
            }
        }

        public static async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            // Use default sender information
            await SendEmailAsync("noreply@ativoplus.com", "AtivoPlus", toEmail, toEmail, subject, htmlContent);
        }
    }
}
