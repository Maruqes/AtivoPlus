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

        public async Task<bool> CreateFundoInvestimento(int ativoFinanceiroId, string nome, decimal montanteInvestido, string ativoSigla, DateTime? dataCriacao = null)
        {
            if (dataCriacao == null)
            {
                dataCriacao = DateTime.UtcNow;
            }
            FundoInvestimento fundoInvestimento = new FundoInvestimento
            {
                AtivoFinaceiroId = ativoFinanceiroId,
                Nome = nome,
                MontanteInvestido = montanteInvestido,
                AtivoSigla = ativoSigla,
                DataCriacao = dataCriacao.Value
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
            var fundoInvestimento = await FundoInvestimentos
                .FirstOrDefaultAsync(f => f.Id == fundoInvestimentoId);
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
            var fundoInvestimento = await FundoInvestimentos
                .FirstOrDefaultAsync(f => f.Id == fundoInvestimentoId);
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
            var fundoInvestimento = await FundoInvestimentos
                .FirstOrDefaultAsync(f => f.Id == fundoInvestimentoId);
            return fundoInvestimento?.AtivoFinaceiroId;
        }

        public async Task<int?> GetUserIdFromFundoInvestimento(int fundoInvestimentoID)
        {
            // get the related AtivoFinanceiro and then its UserId
            var fundoInvestimento = await FundoInvestimentos
                .FirstOrDefaultAsync(f => f.Id == fundoInvestimentoID);
            if (fundoInvestimento == null) return null;

            var ativoFinanceiro = await AtivoFinanceiros
                .FirstOrDefaultAsync(a => a.Id == fundoInvestimento.AtivoFinaceiroId);
            return ativoFinanceiro?.UserId;
        }

        public async Task<List<FundoInvestimento>> GetFundoInvestimentosByUserId(int userId)
        {
            // first fetch all AtivoFinanceiro for this user
            var ativosFinanceiros = await AtivoFinanceiros
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var resultado = new List<FundoInvestimento>();
            foreach (var ativo in ativosFinanceiros)
            {
                var fundos = await FundoInvestimentos
                    .Where(f => f.AtivoFinaceiroId == ativo.Id)
                    .ToListAsync();
                resultado.AddRange(fundos);
            }

            return resultado;
        }
    }
}
