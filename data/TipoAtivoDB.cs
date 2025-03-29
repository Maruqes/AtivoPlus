using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtivoPlus.Logic;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<TipoAtivo> TiposAtivo { get; set; }

        public async Task<bool> CreateTipoAtivo(string nome)
        {
            TipoAtivo tipoAtivo = new TipoAtivo
            {
                Nome = nome
            };

            await TiposAtivo.AddAsync(tipoAtivo);
            await SaveChangesAsync();
            return true;
        }


        public async Task<bool> UpdateTipoAtivo(int tipoAtivoId, string novoNome)
        {
            var tipoAtivo = await TiposAtivo.FirstOrDefaultAsync(c => c.Id == tipoAtivoId);
            if (tipoAtivo == null)
            {
                return false;
            }

            tipoAtivo.Nome = novoNome;
            await SaveChangesAsync();
            return true;
        }


        public async Task<bool> DeleteTipoAtivo(int tipoAtivoId)
        {
            var tipoAtivo = await TiposAtivo.FirstOrDefaultAsync(c => c.Id == tipoAtivoId);
            if (tipoAtivo == null)
            {
                return false;
            }

            TiposAtivo.Remove(tipoAtivo);
            await SaveChangesAsync();
            return true;
        }


        public async Task<List<TipoAtivo>> GetAllTiposAtivo()
        {
            return await TiposAtivo.ToListAsync();
        }


    }
}
