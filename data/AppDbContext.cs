using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;


//ele conecta os Models à base de dados e define as tabelas.
namespace AtivoPlus.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Produto> Produtos { get; set; }
    }
}
