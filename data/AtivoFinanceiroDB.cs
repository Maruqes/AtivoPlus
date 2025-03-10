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



        public async Task<bool> CreateAtivoFinanceiro(int userId, int entidadeAtivoId, int carteiraId, DateTime dataInicio, int duracaoMeses, float taxaImposto)
        {
            if (await UserLogic.CheckIfUserExistsById(this, userId) == false)
            {
                return false;
            }


            AtivoFinanceiro ativoFinanceiro = new AtivoFinanceiro
            {
                UserId = userId,
                EntidadeAtivoId = entidadeAtivoId,
                CarteiraId = carteiraId,
                DataInicio = dataInicio,
                DuracaoMeses = duracaoMeses,
                TaxaImposto = taxaImposto
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

            ativoFinanceiro.CarteiraId = novaCarteiraId;
            await SaveChangesAsync();
            return true;
        }


        public async Task<int?> GetUserIdFromAtivoFinanceiro(int ativoFinanceiroId)
        {
            var ativoFinanceiro = await AtivoFinanceiros.FirstOrDefaultAsync(c => c.Id == ativoFinanceiroId);
            return ativoFinanceiro?.UserId;
        }

    }
}
