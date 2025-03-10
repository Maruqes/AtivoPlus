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


    }
}
