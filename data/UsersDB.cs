using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }


        public async Task<List<User>> GetUsersByRawSqlAsync()
        {
            return await Users.FromSqlRaw("SELECT \"Id\", \"Username\" FROM \"Users\"").Select(u => new User
            {
                Id = u.Id,
                Username = u.Username,
                Hash = "",
                DataCriacao = DateTime.MinValue
            })
            .ToListAsync();
        }


        public async Task<List<User>> GetUserByUsername(string Username)
        {
            return await Users.FromSqlInterpolated($"SELECT * FROM \"Users\" WHERE \"Username\" = {Username}").ToListAsync();
        }
    }
}
