using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<FundoInvestimento> fundoInvestimentos { get; set; }

        public async Task<FundoInvestimento?> GetFundoInvestimento(int ativoFinanceiroId)
        {
            var fundoInvestimento = await fundoInvestimentos.FirstOrDefaultAsync(c => c.Id == ativoFinanceiroId);
            return fundoInvestimento;
        }

        public async Task<bool> CreateFundoInvestimento(int ativoFinanceiroId, int tipoAtivoId, string nome, decimal montanteInvestido, float taxaJuro, bool taxaFixa, string ativoSigla)
        {
            FundoInvestimento fundoInvestimento = new FundoInvestimento
            {
                AtivoFinaceiroId = ativoFinanceiroId,
                TipoAtivoId = tipoAtivoId,
                Nome = nome,
                MontanteInvestido = montanteInvestido,
                TaxaJuro = taxaJuro,
                TaxaFixa = taxaFixa,
                AtivoSigla = ativoSigla
            };

            await fundoInvestimentos.AddAsync(fundoInvestimento);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateFundoInvestimento(int fundoInvestimentoId, int ativoFinanceiroId, int tipoAtivoId, string nome, decimal montanteInvestido, float taxaJuro, bool taxaFixa, string ativoSigla)
        {
            var fundoInvestimento = await fundoInvestimentos.FirstOrDefaultAsync(c => c.Id == fundoInvestimentoId);
            if (fundoInvestimento == null)
            {
                return false;
            }

            fundoInvestimento.AtivoFinaceiroId = ativoFinanceiroId;
            fundoInvestimento.TipoAtivoId = tipoAtivoId;
            fundoInvestimento.Nome = nome;
            fundoInvestimento.MontanteInvestido = montanteInvestido;
            fundoInvestimento.TaxaJuro = taxaJuro;
            fundoInvestimento.TaxaFixa = taxaFixa;
            fundoInvestimento.AtivoSigla = ativoSigla;

            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFundoInvestimento(int fundoInvestimentoId)
        {
            var fundoInvestimento = await fundoInvestimentos.FirstOrDefaultAsync(c => c.Id == fundoInvestimentoId);
            if (fundoInvestimento == null)
            {
                return false;
            }

            fundoInvestimentos.Remove(fundoInvestimento);
            await SaveChangesAsync();
            return true;
        }

    }
}