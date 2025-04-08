using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        // In AppDbContextCandles.cs
        public DbSet<Candle> Candles { get; set; }

        public async Task<List<Candle>> GetCandlesBySymbol(string symbol)
        {
            return await Candles
                .FromSqlInterpolated($"SELECT * FROM \"Candle\" WHERE \"Symbol\" = {symbol}")
                .ToListAsync();
        }
        public async Task<Candle> GetCandleById(long id)
        {
            return await Candles
                .FromSqlInterpolated($"SELECT * FROM \"Candle\" WHERE \"Id\" = {id}")
                .FirstOrDefaultAsync();
        }

        public async Task<bool> AddCandle(string symbo, Candle candle)
        {
            Candles.Add(candle);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddMultipleCandles(List<Candle> candles)
        {
            await Candles.AddRangeAsync(candles);
            await SaveChangesAsync();
            return true;
        }

        public async Task<List<Candle>> GetCandleBySymbolAndDateTime(string symbol, DateTime dateTimeFrom, DateTime dateTimeTo)
        {
            return await Candles
                .FromSqlInterpolated($"SELECT * FROM \"Candle\" WHERE \"Symbol\" = {symbol} AND \"DateTime\" BETWEEN {dateTimeFrom} AND {dateTimeTo}")
                .ToListAsync();
        }

        public async Task<List<Candle>> GetCandleBySymbol(string symbol)
        {
            return await Candles
                .FromSqlInterpolated($"SELECT * FROM \"Candle\" WHERE \"Symbol\" = {symbol}")
                .ToListAsync();
        }

    }
}