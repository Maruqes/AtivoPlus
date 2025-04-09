using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;

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

        [HttpGet("time/{date}")]
        public async Task<ActionResult<string>> GetTime(string symbol, string type, string date)
        {
            if (!DateTime.TryParseExact(date, "dd/ww/yyyy", System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var parsedDate))
            {
                return BadRequest("Invalid date format. Expected dd/ww/yyyy.");
            }

            if (type == "ETF")
            {
                List<Candle>? candles = await TwelveDataLogic.GetETFCandles(symbol, db, "1day", parsedDate);
                return Ok(candles);
            }else if(type == "STOCK")
            {
                List<Candle>? candles = await TwelveDataLogic.GetStockCandles(symbol, db, "1day", parsedDate);
                return Ok(candles);
            }
            else if (type == "CRYPTO")
            {
                List<Candle>? candles = await TwelveDataLogic.GetCryptoCandles(symbol, db, "1day", parsedDate);
                return Ok(candles);
            }

            return Ok($"Parsed date: {parsedDate:F}");
        }
    }
}