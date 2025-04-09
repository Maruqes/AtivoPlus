using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtivoPlus.Logic;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<Banco> Bancos { get; set; }

        public async Task<bool> CreateBanco(string nome)
        {
            Banco banco = new Banco
            {
                Nome = nome
            };

            await Bancos.AddAsync(banco);
            await SaveChangesAsync();
            return true;
        }


        public async Task<bool> UpdateBanco(int bancoId, string novoNome)
        {
            var banco = await Bancos.FirstOrDefaultAsync(c => c.Id == bancoId);
            if (banco == null)
            {
                return false;
            }

            banco.Nome = novoNome;
            await SaveChangesAsync();
            return true;
        }


        public async Task<bool> DeleteBanco(int bancoId)
        {
            var banco = await Bancos.FirstOrDefaultAsync(c => c.Id == bancoId);
            if (banco == null)
            {
                return false;
            }

            Bancos.Remove(banco);
            await SaveChangesAsync();
            return true;
        }


        public async Task<List<Banco>> GetAllBancos()
        {
            return await Bancos.ToListAsync();
        }

        public async Task<bool> BancoNameExists(string nome)
        {
            return await Bancos.AnyAsync(b => b.Nome == nome);
        }

        public async Task<Banco?> GetBancoById(int bancoId)
        {
            return await Bancos.FirstOrDefaultAsync(b => b.Id == bancoId);
        }
    }
}
