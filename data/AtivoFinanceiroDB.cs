using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtivoPlus.Logic;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<AtivoFinanceiro> AtivoFinanceiros { get; set; }



        public async Task<bool> CreateAtivoFinanceiro(int userId, string nome, int carteiraId, DateTime dataInicio, int duracaoMeses, float taxaImposto)
        {
            if (await UserLogic.CheckIfUserExistsById(this, userId) == false)
            {
                return false;
            }


            AtivoFinanceiro ativoFinanceiro = new AtivoFinanceiro
            {
                UserId = userId,
                CarteiraId = carteiraId,
                DataInicio = dataInicio,
                DuracaoMeses = duracaoMeses,
                TaxaImposto = taxaImposto,
                Nome = nome
            };

            await AtivoFinanceiros.AddAsync(ativoFinanceiro);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeAtivoFinanceiroCarteira(int ativoFinanceiroId, int novaCarteiraId)
        {
            var ativoFinanceiro = await AtivoFinanceiros.FirstOrDefaultAsync(c => c.Id == ativoFinanceiroId);
            if (ativoFinanceiro == null)
            {
                return false;
            }

            // Carteira existence check only
            var carteira = await Carteiras.FirstOrDefaultAsync(c => c.Id == novaCarteiraId);
            if (carteira == null)
            {
                return false;
            }

            // Ownership check will be handled at the logic layer

            ativoFinanceiro.CarteiraId = novaCarteiraId;
            await SaveChangesAsync();
            return true;
        }


        public async Task<int?> GetUserIdFromAtivoFinanceiro(int ativoFinanceiroId)
        {
            var ativoFinanceiro = await AtivoFinanceiros.FirstOrDefaultAsync(c => c.Id == ativoFinanceiroId);
            return ativoFinanceiro?.UserId;
        }

        public async Task<List<AtivoFinanceiro>> GetAtivoByUserId(int userId)
        {
            return await AtivoFinanceiros.Where(c => c.UserId == userId).ToListAsync();
        }

        public async Task RemoveAtivoFinanceiro(int ativoFinanceiroId)
        {
            var ativoFinanceiro = await AtivoFinanceiros.FirstOrDefaultAsync(c => c.Id == ativoFinanceiroId);
            if (ativoFinanceiro == null)
            {
                return;
            }

            AtivoFinanceiros.Remove(ativoFinanceiro);
            await SaveChangesAsync();
        }

        public async Task<AtivoFinanceiro?> GetAtivoFinanceiroById(int ativoFinanceiroId)
        {
            return await AtivoFinanceiros.FirstOrDefaultAsync(c => c.Id == ativoFinanceiroId);
        }

        public async Task<List<AtivoFinanceiro>> GetAtivosByCarteiraId(int carteiraId)
        {
            List<AtivoFinanceiro> ativos = await AtivoFinanceiros.Where(c => c.CarteiraId == carteiraId).ToListAsync();
            if (ativos == null)
            {
                return new List<AtivoFinanceiro>();
            }
            else
            {
                return ativos;
            }
        }
    }
}
