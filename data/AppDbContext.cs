using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;


//ele conecta os Models à base de dados e define as tabelas.
namespace AtivoPlus.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Produto>().ToTable("Produtos"); // Ensures the correct table name
            modelBuilder.Entity<User>().ToTable("Users"); // Ensures the correct table name
        }
    }
}
