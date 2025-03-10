using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtivoPlus.Logic;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<Carteira> Carteiras { get; set; }



        public async Task<bool> CreateCarteira(int userId, string nome)
        {
            if (await UserLogic.CheckIfUserExistsById(this, userId) == false)
            {
                return false;
            }


            Carteira carteira = new Carteira
            {
                UserId = userId,
                Nome = nome,
                DataCriacao = DateTime.UtcNow
            };

            await Carteiras.AddAsync(carteira);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCarteiraNome(int carteiraId, string novoNome)
        {
            var carteira = await Carteiras.FirstOrDefaultAsync(c => c.Id == carteiraId);
            if (carteira == null)
            {
                return false;
            }

            carteira.Nome = novoNome;
            await SaveChangesAsync();
            return true;
        }


        public async Task<int?> GetUserIdFromCarteira(int carteiraId)
        {
            var carteira = await Carteiras.FirstOrDefaultAsync(c => c.Id == carteiraId);
            return carteira?.UserId;
        }

        public async Task<bool> DeleteCarteira(int carteiraId)
        {
            var carteira = await Carteiras.FirstOrDefaultAsync(c => c.Id == carteiraId);
            if (carteira == null)
            {
                return false;
            }

            Carteiras.Remove(carteira);
            await SaveChangesAsync();
            return true;
        }

        public async Task<List<Carteira>> GetCarteirasByUserId(int userId)
        {
            return await Carteiras.Where(c => c.UserId == userId).ToListAsync();
        }

    }
}
