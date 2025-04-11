using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtivoPlus.Logic;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<Morada> Moradas { get; set; }

        public async Task<int> CreateMorada(int userId, string rua, string piso, string numeroPorta, string concelho, string distrito, string localidade, string codPostal)
        {
            // Verificar se o utilizador existe
            if (await UserLogic.CheckIfUserExistsById(this, userId) == false)
            {
                return -1;
            }

            Morada morada = null;

            var old_morada = await Moradas.FirstOrDefaultAsync(m => m.User_id == userId);
            if (old_morada == null)
            {
                morada = new Morada
                {
                    User_id = userId,
                    Rua = rua,
                    Piso = piso,
                    NumeroPorta = numeroPorta,
                    Concelho = concelho,
                    Distrito = distrito,
                    Localidade = localidade,
                    CodPostal = codPostal
                };
                await Moradas.AddAsync(morada);
            }
            else
            {
                morada = old_morada;
                morada.Rua = rua;
                morada.Piso = piso;
                morada.NumeroPorta = numeroPorta;
                morada.Concelho = concelho;
                morada.Distrito = distrito;
                morada.Localidade = localidade;
                morada.CodPostal = codPostal;
            }
            await SaveChangesAsync();
            return morada.Id;
        }

        public async Task<bool> UpdateMorada(int moradaId, string rua, string piso, string numeroPorta, string concelho, string distrito, string localidade, string codPostal)
        {
            var morada = await Moradas.FirstOrDefaultAsync(m => m.Id == moradaId);
            if (morada == null)
            {
                return false;
            }

            morada.Rua = rua;
            morada.Piso = piso;
            morada.NumeroPorta = numeroPorta;
            morada.Concelho = concelho;
            morada.Distrito = distrito;
            morada.Localidade = localidade;
            morada.CodPostal = codPostal;

            await SaveChangesAsync();
            return true;
        }

        public async Task<Morada?> GetMoradaById(int moradaId)
        {
            return await Moradas.FirstOrDefaultAsync(m => m.Id == moradaId);
        }

        public async Task<Morada?> GetMoradasByUserId(int userId)
        {
            return await Moradas.Where(m => m.User_id == userId).FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteMorada(int moradaId)
        {
            var morada = await Moradas.FirstOrDefaultAsync(m => m.Id == moradaId);
            if (morada == null)
            {
                return false;
            }

            Moradas.Remove(morada);
            await SaveChangesAsync();
            return true;
        }
    }
}
