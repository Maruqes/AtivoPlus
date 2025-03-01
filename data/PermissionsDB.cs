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



    }
}
