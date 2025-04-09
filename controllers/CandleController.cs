using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;
using System.Globalization;

//ou app a usar api ou da par por razor em cima desta merda

namespace AtivoPlus.Controllers
{
    [Route("api/candle")] // A API está definida em "api/user"
    [ApiController] // Indica que este é um Controller de API
    public class CandleController : ControllerBase
    {
        private readonly AppDbContext db;

        public CandleController(AppDbContext context)
        {
            db = context;
        }

        /// <summary>
        /// Obtém os candles para um símbolo, data e intervalo.
        /// </summary>
        /// <param name="symbol">Símbolo do ativo (ex: BTC/USD)</param>
        /// <param name="type">Tipo do ativo: STOCK, ETF ou CRYPTO</param>
        /// <param name="date">Data inicial no formato dd/MM/yyyy</param>
        /// <param name="interval">Intervalo: 1day, 1week ou 1month</param>
        /// <returns>Lista de candles</returns>
        [HttpGet("time/{date}/{symbol}/{type}/{interval}")]
        public async Task<ActionResult<string>> GetCandles(string symbol, string type, string date, string interval)
        {
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                         DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsedDate))
            {
                return BadRequest("Invalid date format. Expected yyyy-MM-dd.");
            }

            if (interval != "1day" && interval != "1week" && interval != "1month")
            {
                return BadRequest("Invalid interval. Expected \"1day\", \"1week\", or \"1month\".");
            }
            if (type != "ETF" && type != "STOCK" && type != "CRYPTO")
            {
                return BadRequest("Invalid type. Expected \"ETF\", \"STOCK\", or \"CRYPTO\".");
            }

            if (type == "ETF")
            {
                List<Candle>? candles = await TwelveDataLogic.GetETFCandles(symbol, db, interval, parsedDate);
                return Ok(candles);
            }
            else if (type == "STOCK")
            {
                List<Candle>? candles = await TwelveDataLogic.GetStockCandles(symbol, db, interval, parsedDate);
                return Ok(candles);
            }
            else if (type == "CRYPTO")
            {
                List<Candle>? candles = await TwelveDataLogic.GetCryptoCandles(symbol, db, interval, parsedDate);
                return Ok(candles);
            }

            return Ok($"Parsed date: {parsedDate:F}");
        }
    }
}