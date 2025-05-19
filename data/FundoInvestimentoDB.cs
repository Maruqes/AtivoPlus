using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtivoPlus.Logic;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<FundoInvestimento> FundoInvestimentos { get; set; }

        public async Task<bool> CreateFundoInvestimento(int ativoFinanceiroId, string nome, decimal montanteInvestido, string ativoSigla)
        {
            FundoInvestimento fundoInvestimento = new FundoInvestimento
            {
                AtivoFinaceiroId = ativoFinanceiroId,
                Nome = nome,
                MontanteInvestido = montanteInvestido,
                AtivoSigla = ativoSigla,
                DataCriacao = DateTime.Now
            };

            await FundoInvestimentos.AddAsync(fundoInvestimento);
            await SaveChangesAsync();
            return true;
        }

        public async Task<FundoInvestimento?> GetFundoInvestimentoById(int fundoInvestimentoId)
        {
            return await FundoInvestimentos.FirstOrDefaultAsync(f => f.Id == fundoInvestimentoId);
        }

        public async Task<FundoInvestimento?> GetFundoInvestimentoByNome(string nome)
        {
            return await FundoInvestimentos.FirstOrDefaultAsync(f => f.Nome == nome);
        }

        public async Task<List<FundoInvestimento>> GetFundoInvestimentosByAtivoFinanceiroId(int ativoFinanceiroId)
        {
            return await FundoInvestimentos.Where(f => f.AtivoFinaceiroId == ativoFinanceiroId).ToListAsync();
        }

        public async Task<bool> UpdateFundoInvestimento(int fundoInvestimentoId, decimal? montanteInvestido = null)
        {
            var fundoInvestimento = await FundoInvestimentos.FirstOrDefaultAsync(f => f.Id == fundoInvestimentoId);
            if (fundoInvestimento == null)
            {
                return false;
            }

            if (montanteInvestido.HasValue)
            {
                fundoInvestimento.MontanteInvestido = montanteInvestido.Value;
            }


            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFundoInvestimento(int fundoInvestimentoId)
        {
            var fundoInvestimento = await FundoInvestimentos.FirstOrDefaultAsync(f => f.Id == fundoInvestimentoId);
            if (fundoInvestimento == null)
            {
                return false;
            }

            FundoInvestimentos.Remove(fundoInvestimento);
            await SaveChangesAsync();
            return true;
        }

        public async Task<List<FundoInvestimento>> GetAllFundoInvestimentos()
        {
            return await FundoInvestimentos.ToListAsync();
        }

        public async Task<int?> GetAtivoFinanceiroIdFromFundoInvestimento(int fundoInvestimentoId)
        {
            var fundoInvestimento = await FundoInvestimentos.FirstOrDefaultAsync(f => f.Id == fundoInvestimentoId);
            return fundoInvestimento?.AtivoFinaceiroId;
        }
    }
}
