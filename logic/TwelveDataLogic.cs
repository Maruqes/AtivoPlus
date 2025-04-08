using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json.Linq;
using AtivoPlus.Models;

namespace AtivoPlus.Logic
{
    public static class TwelveDataLogic
    {
        private static HttpClient _httpClient = null!;
        private static string _apiKey = ""; // A tua API key do Twelve Data

        public static void StartTwelveDataLogic(string apiKey)
        {
            try
            {
                _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0");
                _apiKey = apiKey;

                if (string.IsNullOrWhiteSpace(_apiKey))
                    throw new Exception("API key do Twelve Data não definida.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao iniciar TwelveDataLogic: {ex.Message}");
            }
        }

        public static async Task<List<Candle>?> GetCandles(string symbol, string interval = "1day", int outputSize = 30)
        {
            try
            {
                string url = $"https://api.twelvedata.com/time_series?symbol={symbol}&interval={interval}&outputsize={outputSize}&apikey={_apiKey}";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Erro na requisição GetCandles: {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(content);

                // Verifica se a API retornou um erro
                if (jObject["status"] != null && jObject["status"]!.ToString().Equals("error", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Erro da API: {jObject["message"]?.ToString()}");
                    return null;
                }

                var values = jObject["values"] as JArray;
                if (values == null || !values.Any())
                {
                    Console.WriteLine("Sem dados de candles. Resposta completa: " + content);
                    return null;
                }


                var candles = new List<Candle>();

                foreach (var v in values)
                {
                    var datetimeStr = v["datetime"]?.ToString();
                    var openStr = v["open"]?.ToString();
                    var highStr = v["high"]?.ToString();
                    var lowStr = v["low"]?.ToString();
                    var closeStr = v["close"]?.ToString();
                    var volumeStr = v["volume"]?.ToString();

                    if (string.IsNullOrEmpty(datetimeStr) ||
                        string.IsNullOrEmpty(openStr) ||
                        string.IsNullOrEmpty(highStr) ||
                        string.IsNullOrEmpty(lowStr) ||
                        string.IsNullOrEmpty(closeStr) ||
                        string.IsNullOrEmpty(volumeStr))
                    {
                        Console.WriteLine("Dados incompletos num candle, ignorando este item.");
                        continue;
                    }

                    try
                    {
                        var candle = new Candle
                        {
                            DateTime = DateTime.Parse(datetimeStr, CultureInfo.InvariantCulture),
                            Open = decimal.Parse(openStr, CultureInfo.InvariantCulture),
                            High = decimal.Parse(highStr, CultureInfo.InvariantCulture),
                            Low = decimal.Parse(lowStr, CultureInfo.InvariantCulture),
                            Close = decimal.Parse(closeStr, CultureInfo.InvariantCulture),
                            Volume = decimal.Parse(volumeStr, CultureInfo.InvariantCulture)
                        };
                        candles.Add(candle);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao converter dados de candle: {ex.Message}");
                    }
                }


                return candles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção em GetCandles: {ex.Message}");
                return null;
            }
        }

        public static async Task<List<Candle>?> GetStockCandles(string symbol, string interval = "1day", int outputSize = 30)
            => await GetCandles(symbol, interval, outputSize);

        public static async Task<List<Candle>?> GetETFCandles(string symbol, string interval = "1day", int outputSize = 30)
            => await GetCandles(symbol, interval, outputSize);

        public static async Task<List<Candle>?> GetCryptoCandles(string symbol, string interval = "1day", int outputSize = 30)
        {
            // Twelve Data usa símbolos como BTC/USD, ETH/USD, etc.
            string formattedSymbol = symbol.ToUpper().EndsWith("/USD") ? symbol : $"{symbol.ToUpper()}/USD";
            return await GetCandles(formattedSymbol, interval, outputSize);
        }
    }
}
