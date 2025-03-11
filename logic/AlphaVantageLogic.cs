using System.Threading.Tasks;
using AtivoPlus.Data;
using AtivoPlus.Controllers;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using DotNetEnv;

namespace AtivoPlus.Logic
{
    public class AlphaVantageLogic
    {
        private static string _apiKey;
        private static HttpClient _httpClient;

        public static void StartAlphaVantageLogic()
        {
            // carrega as variáveis do ficheiro .env
            Env.Load();
            _apiKey = Environment.GetEnvironmentVariable("ALPHAVANTAGE_API_KEY");
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new Exception("A chave da API não esta definida no ficheiro .env.");
            }
            _httpClient = new HttpClient();
        }

        public static async Task<decimal?> GetCrypto(string cryptoSymbol, string market = "USD")
        {
            var url = $"https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE&from_currency={cryptoSymbol}&to_currency={market}&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(json);
            var rateToken = jObject["Realtime Currency Exchange Rate"]?["5. Exchange Rate"];
            if (rateToken == null) return null;

            if (decimal.TryParse(rateToken.ToString(), out decimal rate))
            {
                return rate;
            }
            return null;
        }

        public static async Task<decimal?> GetStock(string symbol)
        {
            var url = $"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol={symbol}&interval=5min&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(json);
            var timeSeries = jObject["Time Series (5min)"];
            if (timeSeries == null) return null;

            // Seleciona o registo mais recente
            var latestEntry = timeSeries.Children<JProperty>().OrderByDescending(x => x.Name).FirstOrDefault();
            if (latestEntry == null) return null;

            var closeToken = latestEntry.Value["4. close"];
            if (closeToken == null) return null;

            if (decimal.TryParse(closeToken.ToString(), out decimal close))
            {
                return close;
            }
            return null;
        }

        public static async Task<decimal?> GetETF(string symbol)
        {
            // Neste exemplo, utiliza a mesma lógica que para ações
            return await GetStock(symbol);
        }
    }
}
