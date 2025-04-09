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
                .FromSqlInterpolated($"SELECT * FROM \"Candle\" WHERE \"symbol\" = {symbol}")
                .ToListAsync();
        }
        public async Task<Candle?> GetCandleById(long id)
        {
            return await Candles
                .FromSqlInterpolated($"SELECT * FROM \"Candle\" WHERE \"id\" = {id}")
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
            // Extrai os símbolos únicos para otimizar a consulta
            var symbols = candles.Select(c => c.Symbol).Distinct().ToList();

            // Busca os candles já existentes com esses símbolos, mas só os campos necessários
            var existingCandles = await Candles
                .Where(c => symbols.Contains(c.Symbol))
                .Select(c => new { c.Symbol, c.DateTime })
                .ToListAsync();

            // Filtra os candles que não estão presentes na base de dados
            var newCandles = candles
                .Where(c => !existingCandles.Any(ec => ec.Symbol == c.Symbol && ec.DateTime == c.DateTime))
                .ToList();

            // Se houver candles novos, adiciona-os à base de dados
            if (newCandles.Any())
            {
                await Candles.AddRangeAsync(newCandles);
                await SaveChangesAsync();
            }

            return true;
        }


        public async Task<List<Candle>> GetCandleBySymbolAndDateTime(string symbol, DateTime dateTimeFrom, DateTime dateTimeTo)
        {
            return await Candles
                .FromSqlInterpolated($"SELECT * FROM \"Candle\" WHERE \"symbol\" = {symbol} AND \"datetime\" BETWEEN {dateTimeFrom} AND {dateTimeTo}")
                .ToListAsync();
        }

        public async Task<List<Candle>> GetCandleBySymbol(string symbol)
        {
            return await Candles
                .FromSqlInterpolated($"SELECT * FROM \"Candle\" WHERE \"symbol\" = {symbol}")
                .ToListAsync();
        }

    }
}