using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using DotNetEnv;

namespace AtivoPlus.Logic
{
    public class FinnhubLogic
    {
        private static string _apiKey;
        private static HttpClient _httpClient;

        public static void StartFinnhubLogic()
        {
            try
            {
                // Carrega as variáveis do ficheiro .env
                Env.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".env"));
                _apiKey = Environment.GetEnvironmentVariable("FINNHUB_API_KEY");
                if (string.IsNullOrEmpty(_apiKey))
                {
                    throw new Exception("A chave da API não está definida no ficheiro .env.");
                }
                _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "AtivoPlusApp/1.0");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao iniciar FinnhubLogic: {ex.Message}");
            }
        }


        public static async Task<decimal?> GetStock(string symbol)
        {
            try
            {
                var url = $"https://finnhub.io/api/v1/quote?symbol={symbol}&token={_apiKey}";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Erro na requisição GetStock: {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(content);
                var currentToken = jObject["c"];  // "c" representa o preço atual

                if (currentToken == null)
                {
                    Console.WriteLine("Token de preço não encontrado em GetStock.");
                    return null;
                }

                if (decimal.TryParse(currentToken.ToString(), out decimal current))
                {
                    return current;
                }
                else
                {
                    Console.WriteLine("Falha ao converter o token de preço em GetStock.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção em GetStock: {ex.Message}");
                return null;
            }
        }

        public static async Task<decimal?> GetETF(string symbol)
        {
            // Neste exemplo, utiliza a mesma lógica que para ações
            return await GetStock(symbol);
        }


        public static async Task<decimal?> GetCrypto(string cryptoSymbol)
        {
            try
            {
                string symbol = $"BINANCE:{cryptoSymbol}USDT";
                string url = $"https://finnhub.io/api/v1/quote?symbol={symbol}&token={_apiKey}";

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();


                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Erro na requisição: {response.StatusCode}");
                    return null;
                }

                var jObject = JObject.Parse(content);
                var priceToken = jObject["c"];  // "c" representa o preço atual

                if (priceToken == null)
                {
                    Console.WriteLine("Token de preço não encontrado.");
                    return null;
                }

                return decimal.TryParse(priceToken.ToString(), out decimal price) ? price : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter preço do Bitcoin: {ex.Message}");
                return null;
            }
        }
    }
}
