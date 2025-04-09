using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json.Linq;
using AtivoPlus.Models;
using AtivoPlus.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

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

        public static async Task<List<Candle>?> CheckDbCandles(AppDbContext db, string symbol, DateTime dateTimeFrom, DateTime dateTimeTo)
        {
            try
            {
                var candles = await db.GetCandleBySymbolAndDateTime(symbol, dateTimeFrom, dateTimeTo);
                if (candles == null || candles.Count == 0)
                {
                    Console.WriteLine($"Sem candles no banco de dados para o símbolo {symbol} entre {dateTimeFrom} e {dateTimeTo}");
                    return null;
                }
                return candles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao verificar candles no banco de dados: {ex.Message}");
                return null;
            }
        }

        public static DateTime CalculateDateTimeFrom(int outputSize, string interval)
        {
            DateTime dateTimeFrom = DateTime.UtcNow.Date.AddDays(1);
            switch (interval)
            {
                case "1day":
                    dateTimeFrom = dateTimeFrom.AddDays(-outputSize);
                    break;
                case "1week":
                    dateTimeFrom = dateTimeFrom.AddDays(-outputSize * 7);
                    break;
                case "1month":
                    dateTimeFrom = dateTimeFrom.AddMonths(-outputSize);
                    break;
                default:
                    Console.WriteLine("Intervalo inválido. Usando o padrão '1day'.");
                    dateTimeFrom = dateTimeFrom.AddDays(-outputSize);
                    break;
            }
            return dateTimeFrom;
        }

        public static async Task<List<Candle>?> GetCandles(string symbol, AppDbContext db, string interval = "1day", DateTime LastDay = default)
        {
            if (interval != "1day" || interval != "1week" || interval != "1month")
            {
                Console.WriteLine("Intervalo inválido. Usando o padrão '1day'.");
                interval = "1day";
            }
            if (string.IsNullOrWhiteSpace(symbol))
            {
                Console.WriteLine("Símbolo inválido.");
                return null;
            }

            List<Candle>? dbCandles = await CheckDbCandles(db, symbol, LastDay, DateTime.UtcNow);
            if (dbCandles != null && dbCandles.Count > 0)
            {
                var lastCandle = dbCandles.OrderBy(c => c.DateTime).Last();
                DateTime yesterday = DateTime.UtcNow.AddDays(-1).Date;

                var firstCandle = dbCandles.OrderBy(c => c.DateTime).First();

                Console.WriteLine($"lastCandle: {lastCandle.DateTime}");
                Console.WriteLine($"Yesterday: {yesterday}");
                Console.WriteLine($"FirstCandle: {firstCandle.DateTime}");
                Console.WriteLine($"LastDay: {LastDay}");

                if (Math.Abs((lastCandle.DateTime.Date - yesterday).Days) <= 2 &&
                    Math.Abs((firstCandle.DateTime.Date - LastDay.Date).Days) <= 2)
                {
                    Console.WriteLine($"Candles já existem no banco de dados para o símbolo {symbol} entre {firstCandle.DateTime} e {lastCandle.DateTime}");
                    return dbCandles;
                }
            }

            try
            {
                string url = $"https://api.twelvedata.com/time_series?symbol={symbol}&interval={interval}&start_date={LastDay.ToString("yyyy-MM-dd")}&apikey={_apiKey}";
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
                        string.IsNullOrEmpty(closeStr))
                    {
                        Console.WriteLine("Dados incompletos num candle, ignorando este item.");
                        continue;
                    }
                    if (string.IsNullOrEmpty(volumeStr))
                    {
                        volumeStr = "0"; // Se o volume não estiver disponível, define como 0
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
                            Volume = decimal.Parse(volumeStr, CultureInfo.InvariantCulture),
                            Symbol = symbol
                        };
                        candles.Add(candle);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao converter dados de candle: {ex.Message}");
                    }
                }
                db.AddMultipleCandles(candles);
                return candles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção em GetCandles: {ex.Message}");
                return null;
            }
        }

        public static async Task<List<Candle>?> GetStockCandles(string symbol, AppDbContext db, string interval = "1day", DateTime LastDay = default)
        {
            List<Candle>? candles = await GetCandles(symbol, db, interval, LastDay);
            return candles;
        }

        public static async Task<List<Candle>?> GetETFCandles(string symbol, AppDbContext db, string interval = "1day", DateTime LastDay = default)
        {
            List<Candle>? candles = await GetCandles(symbol, db, interval, LastDay);
            return candles;
        }

        public static async Task<List<Candle>?> GetCryptoCandles(string symbol, AppDbContext db, string interval = "1day", DateTime LastDay = default)
        {
            // Twelve Data usa símbolos como BTC/USD, ETH/USD, etc.
            string formattedSymbol = symbol.ToUpper().EndsWith("/USD") ? symbol : $"{symbol.ToUpper()}/USD";
            List<Candle>? candles = await GetCandles(formattedSymbol, db, interval, LastDay);
            return candles;
        }
    }
}
