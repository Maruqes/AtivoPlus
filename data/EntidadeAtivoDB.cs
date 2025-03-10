using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtivoPlus.Logic;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<EntidadeAtivo> entidades { get; set; }

        public async Task<EntidadeAtivo> AdicionarEntidadeAtivo(string name, int userId)
        {
            var entidade = new EntidadeAtivo
            {
                Nome = name,
                UserId = userId
            };
            await entidades.AddAsync(entidade);
            await SaveChangesAsync();
            return entidade;
        }

        public async Task<EntidadeAtivo?> GetEntidadeAtivo(int id)
        {
            return await entidades.FindAsync(id);
        }

        public async Task ApagarEntidadeAtivo(int id)
        {
            var entidade = await entidades.FindAsync(id);
            if (entidade != null)
            {
                entidades.Remove(entidade);
                await SaveChangesAsync();
            }
        }

        public async Task<List<EntidadeAtivo>> GetEntidadeAtivoByUserId(int userId)
        {
            return await entidades.Where(e => e.UserId == userId).ToListAsync();
        }

        public async Task<bool> DoesEntidadeExist(int id)
        {
            return await entidades.AnyAsync(e => e.Id == id);
        }


    }
}
