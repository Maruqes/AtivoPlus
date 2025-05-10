using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtivoPlus.Logic;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<DepositoPrazo> DepositosPrazo { get; set; }

        public async Task<DepositoPrazo?> GetDepositoPrazo(int depositoPrazoId)
        {
            var depositoPrazo = await DepositosPrazo.FirstOrDefaultAsync(d => d.Id == depositoPrazoId);
            return depositoPrazo;
        }

        public async Task<DepositoPrazo?> GetDepositoPrazoByAtivoFinanceiroId(int ativoFinanceiroId)
        {
            var depositoPrazo = await DepositosPrazo.FirstOrDefaultAsync(d => d.AtivoFinaceiroId == ativoFinanceiroId);
            return depositoPrazo;
        }

        public async Task<bool> CreateDepositoPrazo(int ativoFinaceiroId, int tipoAtivoId, int bancoId, int titularId,
            int numeroConta, float taxaJuroAnual, decimal valorAtual, decimal valorInvestido, decimal valorAnualDespesasEstimadas)
        {
            DepositoPrazo depositoPrazo = new DepositoPrazo
            {
                AtivoFinaceiroId = ativoFinaceiroId,
                TipoAtivoId = tipoAtivoId,
                BancoId = bancoId,
                TitularId = titularId,
                NumeroConta = numeroConta,
                TaxaJuroAnual = taxaJuroAnual,
                ValorAtual = valorAtual,
                ValorInvestido = valorInvestido,
                ValorAnualDespesasEstimadas = valorAnualDespesasEstimadas
            };

            await DepositosPrazo.AddAsync(depositoPrazo);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateDepositoPrazo(int depositoPrazoId, int ativoFinaceiroId, int tipoAtivoId, int bancoId, int titularId,
            int numeroConta, float taxaJuroAnual, decimal valorAtual, decimal valorInvestido, decimal valorAnualDespesasEstimadas)
        {
            var depositoPrazo = await DepositosPrazo.FirstOrDefaultAsync(d => d.Id == depositoPrazoId);
            if (depositoPrazo == null)
            {
                return false;
            }

            depositoPrazo.AtivoFinaceiroId = ativoFinaceiroId;
            depositoPrazo.TipoAtivoId = tipoAtivoId;
            depositoPrazo.BancoId = bancoId;
            depositoPrazo.TitularId = titularId;
            depositoPrazo.NumeroConta = numeroConta;
            depositoPrazo.TaxaJuroAnual = taxaJuroAnual;
            depositoPrazo.ValorAtual = valorAtual;
            depositoPrazo.ValorInvestido = valorInvestido;
            depositoPrazo.ValorAnualDespesasEstimadas = valorAnualDespesasEstimadas;

            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDepositoPrazo(int depositoPrazoId)
        {
            var depositoPrazo = await DepositosPrazo.FirstOrDefaultAsync(d => d.Id == depositoPrazoId);
            if (depositoPrazo == null)
            {
                return false;
            }

            DepositosPrazo.Remove(depositoPrazo);
            await SaveChangesAsync();
            return true;
        }

        public async Task<List<DepositoPrazo>> GetDepositosPrazoByTitularId(int titularId)
        {
            return await DepositosPrazo.Where(d => d.TitularId == titularId).ToListAsync();
        }

        public async Task<List<DepositoPrazo>> GetDepositosPrazoByBancoId(int bancoId)
        {
            return await DepositosPrazo.Where(d => d.BancoId == bancoId).ToListAsync();
        }

        public async Task<DepositoPrazo?> GetDepositoPrazoById(int Id)
        {
            return await DepositosPrazo.Where(d => d.Id == Id).FirstOrDefaultAsync();
        }

        public async Task<DepositoPrazo?> GetDepositoPrazoByNumeroConta(int numeroConta, int bancoId)
        {
            return await DepositosPrazo.FirstOrDefaultAsync(d => d.NumeroConta == numeroConta && d.BancoId == bancoId);
        }
    }
}