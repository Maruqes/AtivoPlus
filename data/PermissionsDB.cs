using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermission { get; set; }

        public async Task<List<Permission>> DoesUserHavePermission(int userId, int[] permissions)
        {
            List<Permission> perms = new();

            foreach (int perm in permissions)
            {
                var permission = await Permissions.FindAsync(perm);
                if (permission != null)
                {
                    perms.Add(permission);
                }
            }

            return perms;
        }
    }
}
