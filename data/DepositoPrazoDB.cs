using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtivoPlus.Logic;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<DepositoPrazo> DepositoPrazos { get; set; }

        public async Task<bool> CreateDepositoPrazo(int ativoFinanceiroId, int bancoId, int titularId, int numeroConta, float taxaJuroAnual, decimal valorAtual, decimal valorInvestido, decimal valorAnualDespesasEstimadas, DateTime? dataCriacao = null)
        {
            if (dataCriacao == null)
            {
                dataCriacao = DateTime.UtcNow;
            }
            DepositoPrazo deposito = new DepositoPrazo
            {
                AtivoFinaceiroId = ativoFinanceiroId,
                BancoId = bancoId,
                TitularId = titularId,
                NumeroConta = numeroConta,
                TaxaJuroAnual = taxaJuroAnual,
                ValorAtual = valorAtual,
                ValorInvestido = valorInvestido,
                ValorAnualDespesasEstimadas = valorAnualDespesasEstimadas,
                DataCriacao = dataCriacao.Value
            };

            await DepositoPrazos.AddAsync(deposito);
            await SaveChangesAsync();
            return true;
        }

        public async Task<DepositoPrazo?> GetDepositoPrazoById(int depositoPrazoId)
        {
            return await DepositoPrazos.FirstOrDefaultAsync(d => d.Id == depositoPrazoId);
        }

        public async Task<List<DepositoPrazo>> GetDepositoPrazosByAtivoFinanceiroId(int ativoFinanceiroId)
        {
            return await DepositoPrazos.Where(d => d.AtivoFinaceiroId == ativoFinanceiroId).ToListAsync();
        }

        public async Task<List<DepositoPrazo>> GetDepositoPrazosByBancoId(int bancoId)
        {
            return await DepositoPrazos.Where(d => d.BancoId == bancoId).ToListAsync();
        }

        public async Task<List<DepositoPrazo>> GetDepositoPrazosByTitularId(int titularId)
        {
            return await DepositoPrazos.Where(d => d.TitularId == titularId).ToListAsync();
        }

        public async Task<bool> UpdateDepositoPrazo(int depositoPrazoId, float? taxaJuroAnual = null, decimal? valorAtual = null, decimal? valorInvestido = null, decimal? valorAnualDespesasEstimadas = null)
        {
            var deposito = await DepositoPrazos.FirstOrDefaultAsync(d => d.Id == depositoPrazoId);
            if (deposito == null)
            {
                return false;
            }
            if (taxaJuroAnual.HasValue)
            {
                deposito.TaxaJuroAnual = taxaJuroAnual.Value;
            }
            if (valorAtual.HasValue)
            {
                deposito.ValorAtual = valorAtual.Value;
            }
            if (valorInvestido.HasValue)
            {
                deposito.ValorInvestido = valorInvestido.Value;
            }
            if (valorAnualDespesasEstimadas.HasValue)
            {
                deposito.ValorAnualDespesasEstimadas = valorAnualDespesasEstimadas.Value;
            }
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDepositoPrazo(int depositoPrazoId)
        {
            var deposito = await DepositoPrazos.FirstOrDefaultAsync(d => d.Id == depositoPrazoId);
            if (deposito == null)
            {
                return false;
            }
            DepositoPrazos.Remove(deposito);
            await SaveChangesAsync();
            return true;
        }

        public async Task<List<DepositoPrazo>> GetAllDepositoPrazos()
        {
            return await DepositoPrazos.ToListAsync();
        }

        public async Task<int?> GetAtivoFinanceiroIdFromDepositoPrazo(int depositoPrazoId)
        {
            var deposito = await DepositoPrazos.FirstOrDefaultAsync(d => d.Id == depositoPrazoId);
            return deposito?.AtivoFinaceiroId;
        }

        public async Task<int?> GetUserIdFromDepositoPrazo(int depositoPrazoId)
        {
            var deposito = await DepositoPrazos.FirstOrDefaultAsync(d => d.Id == depositoPrazoId);
            return deposito?.TitularId;
        }

        public async Task<List<DepositoPrazo>> GetDepositoPrazosByUserId(int userId)
        {
            return await DepositoPrazos.Where(d => d.TitularId == userId).ToListAsync();
        }
    }
}
