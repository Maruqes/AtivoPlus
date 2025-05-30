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
using System.IO;
using System.Text.RegularExpressions;

namespace AtivoPlus.Logic
{
    public static class TwelveDataLogic
    {
        private static HttpClient _httpClient = null!;
        private static string _apiKey = ""; // A tua API key do Twelve Data
        private static readonly string[] _priority =
        { "symbol", "name", "currency", "category", "country" };

        private static readonly Dictionary<string, JObject> _cachedJsonFiles = new();
        // Lista achada e plana de todos os objetos
        private static List<JObject> _allEntries = new();

        private static Regex _searchRegex;

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

                // Carregar e cache de ficheiros JSON
                string jsonDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TwelveJson");
                if (Directory.Exists(jsonDir))
                {
                    var jsonFiles = Directory.GetFiles(jsonDir, "*.json");
                    foreach (var file in jsonFiles)
                    {
                        try
                        {
                            var fileName = Path.GetFileName(file);
                            var jsonContent = File.ReadAllText(file);
                            var jsonObject = JObject.Parse(jsonContent);
                            _cachedJsonFiles[fileName] = jsonObject;
                            Console.WriteLine($"Loaded JSON file: {fileName}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error loading {file}: {ex.Message}");
                        }
                    }
                    Console.WriteLine($"Cached {_cachedJsonFiles.Count} JSON files");

                    // Flattening apenas uma vez ao iniciar
                    _allEntries = _cachedJsonFiles.Values
                        .SelectMany(token => Flatten(token))
                        .ToList();
                    Console.WriteLine($"Flattened total entries: {_allEntries.Count}");
                }
                else
                {
                    Console.WriteLine($"JSON directory not found: {jsonDir}");
                }
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
            if (interval != "1day" && interval != "1week" && interval != "1month")
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

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await using var temp_db = AppDbContext.GetDb();
                        await temp_db.AddMultipleCandles(candles);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao inserir candles em background: {ex.Message}");
                    }
                });


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

        public static JObject? GetCachedJsonFile(string fileName)
        {
            if (_cachedJsonFiles.TryGetValue(fileName, out JObject? jsonObject))
            {
                return jsonObject;
            }

            try
            {
                string jsonDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TwelveJson");
                string filePath = Path.Combine(jsonDir, fileName);

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"JSON file not found: {filePath}");
                    return null;
                }

                string jsonContent = File.ReadAllText(filePath);
                jsonObject = JObject.Parse(jsonContent);
                _cachedJsonFiles[fileName] = jsonObject;

                return jsonObject;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading JSON file {fileName}: {ex.Message}");
                return null;
            }
        }

        public static bool DoesSymbolExists(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol) || _allEntries.Count == 0)
                return false;

            // Procura direta em lista plana
            return _allEntries.Any(j =>
                string.Equals(j["symbol"]?.ToString(), symbol, StringComparison.OrdinalIgnoreCase));
        }

        public static List<JObject> SearchJsonFiles(string searchTerm, int numberOfResults = 50)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || numberOfResults <= 0 || _allEntries.Count == 0)
                return new();

            // Construir regex uma vez por termo
            _searchRegex = new Regex($"(?<![\\p{{L}}\\p{{N}}]){Regex.Escape(searchTerm)}(?![\\p{{L}}\\p{{N}}])",
                                     RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var results = _allEntries
                // Filtrar apenas objetos que contenham termo em campos prioritários ou em qualquer campo
                .Select(j => new
                {
                    Obj = j,
                    Score = GetScore(j)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Obj["symbol"]?.ToString())
                .Take(numberOfResults)
                .Select(x => x.Obj)
                .DistinctBy(o => (string?)o["symbol"] + "|" + (string?)o["exchange"])
                .ToList();

            return results;
        }

        private static IEnumerable<JObject> Flatten(JToken token)
        {
            if (token is JObject obj)
                yield return obj;

            foreach (var child in token.Children())
            {
                foreach (var j in Flatten(child))
                    yield return j;
            }
        }

        private static int GetScore(JObject j)
        {
            for (int i = 0; i < _priority.Length; i++)
            {
                var field = _priority[i];
                var value = j[field]?.ToString();
                if (!string.IsNullOrEmpty(value) && _searchRegex.IsMatch(value))
                    return _priority.Length - i;
            }
            return 0;
        }
    }
}
