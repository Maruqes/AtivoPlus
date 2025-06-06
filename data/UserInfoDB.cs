using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtivoPlus.Logic;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<UserInfo> UsersInfo { get; set; }

        public async Task<bool> SetUserInfo(int id, string nome, string email, string telefone, int morada_id, string nif, string iban)
        {
            if (await UserLogic.CheckIfUserExistsById(this, id) == false)
            {
                return false;
            }

            UserInfo? userInfo = await UsersInfo.FindAsync(id);
            if (userInfo == null)
            {
                userInfo = new UserInfo
                {
                    Id = id,
                    Nome = nome,
                    Email = email,
                    Telefone = telefone,
                    Morada_id = morada_id,
                    NIF = nif,
                    IBAN = iban,
                    DateTime = DateTime.UtcNow
                };

                await UsersInfo.AddAsync(userInfo);
            }
            else
            {
                userInfo.Nome = nome;
                userInfo.Email = email;
                userInfo.Telefone = telefone;
                userInfo.Morada_id = morada_id;
                userInfo.NIF = nif;
                userInfo.IBAN = iban;
                userInfo.DateTime = DateTime.UtcNow;
            }

            await SaveChangesAsync();
            return true;
        }

        public async Task<UserInfo?> GetUserInfo(int id)
        {
            if (await UserLogic.CheckIfUserExistsById(this, id) == false)
            {
                return null;
            }

            return await UsersInfo.FindAsync(id);
        }

        public async Task<string> GetEmailByUserId(int id)
        {
            if (await UserLogic.CheckIfUserExistsById(this, id) == false)
            {
                return string.Empty;
            }

            UserInfo? userInfo = await UsersInfo.FindAsync(id);
            if (userInfo == null)
            {
                return string.Empty;
            }

            return userInfo.Email;
        }

        public async Task<string> GetEmailByUsername(string username)
        {
            if (await UserLogic.CheckIfUserExists(this, username) == false)
            {
                return string.Empty;
            }

            //get user id by username
            User? user = await GetUserByUsername(username);
            if (user == null)
            {
                return string.Empty;
            }

            //get user info by user id
            UserInfo? userInfo = await UsersInfo.FindAsync(user.Id);
            if (userInfo == null)
            {
                return string.Empty;
            }
            
            //return email  
            if (string.IsNullOrEmpty(userInfo.Email))
            {
                return string.Empty;
            }

            return userInfo.Email;
        }
    }
}
