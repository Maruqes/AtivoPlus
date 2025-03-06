using Xunit;
using AtivoPlus.Data;
using AtivoPlus.Logic;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;

namespace AtivoPlus.Tests
{
    [CollectionDefinition("Database Tests", DisableParallelization = true)]
    public partial class UnitTests
    {
        //criar db mpts pa testes
        private static AppDbContext GetPostgresDbContext()
        {
            var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
            string connectionString = configuration.GetConnectionString("PostgreSqlConnectionUniTest") ?? throw new InvalidOperationException("Connection string 'PostgreSqlConnection' not found.");

            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

            var db = new AppDbContext(options);
            db.Database.EnsureCreated();

            // limpa as tabelas para os testes
            var entityTypes = db.Model.GetEntityTypes();
            foreach (var entityType in entityTypes)
            {
                var tableName = entityType.GetTableName();
                if (!string.IsNullOrEmpty(tableName))
                {
                    db.Database.ExecuteSqlRaw($"DELETE FROM \"{tableName}\";"); //nao mudar por algum motivo com ExecuteSql nao funfa
                }
            }

            db.SaveChanges();
            ExtraLogic.SetUpAdminPermission(db);
            return db;
        }



        [Fact]
        public async Task AddUser_ValidUser_ShouldBeAbleToLogIn()
        {
            var db = GetPostgresDbContext();


            bool addResult = await UserLogic.AddUser(db, "testUser2", "TestPassword123");
            Assert.True(addResult);

            //return token 
            string token = await UserLogic.LogarUser(db, "testUser2", "TestPassword123");
            Assert.False(string.IsNullOrEmpty(token));

            //return empty string
            string badToken = await UserLogic.LogarUser(db, "testUser2", "WrongPassword");
            Assert.True(string.IsNullOrEmpty(badToken));

            Assert.True(UserLogic.CheckUserLogged("testUser2", token));
            Assert.False(UserLogic.CheckUserLogged("testUser2", badToken));

            //logout and test old token
            UserLogic.LogoutUser("testUser2", token);
            Assert.False(UserLogic.CheckUserLogged("testUser2", token));
        }

    }
}