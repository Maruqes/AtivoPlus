using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AtivoPlus.Data
{

    public partial class AppDbContext : DbContext
    {
        public static AppDbContext GetDb()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string connectionString = configuration.GetConnectionString("PostgreSqlConnection") ?? throw new InvalidOperationException("Connection string 'PostgreSqlConnection' not found.");

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            var db = new AppDbContext(options);
            db.Database.EnsureCreated();
            return db;
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<UserInfo>().ToTable("UsersInfo");
            modelBuilder.Entity<Permission>().ToTable("Permissions");
            modelBuilder.Entity<UserPermission>().ToTable("UserPermissions");
            modelBuilder.Entity<AtivoFinanceiro>().ToTable("AtivoFinanceiro");
            modelBuilder.Entity<Banco>().ToTable("Banco");
            modelBuilder.Entity<Carteira>().ToTable("Carteira");
            modelBuilder.Entity<DepositoPrazo>().ToTable("DepositoPrazo");
            modelBuilder.Entity<FundoInvestimento>().ToTable("FundoInvestimento");
            modelBuilder.Entity<ImovelArrendado>().ToTable("ImovelArrendado");
            modelBuilder.Entity<Morada>().ToTable("Morada");
            modelBuilder.Entity<TipoAtivo>().ToTable("TipoAtivo");
            modelBuilder.Entity<Candle>().ToTable("Candle");
            modelBuilder.Entity<Candle>()
                    .HasIndex(c => new { c.Symbol, c.DateTime })
                    .IsUnique();
        }
    }
}
