using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<UserInfo>().ToTable("UsersInfo");
            modelBuilder.Entity<Permission>().ToTable("Permissions");
            modelBuilder.Entity<UserPermission>().ToTable("UserPermissions");
        }
    }
}