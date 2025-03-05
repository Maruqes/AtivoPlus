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


        public bool TestLoggedIn(string username, string token)
        {
            //somewhat the logic inside  UserLogic.CheckUserLoggedRequest(Request)
            if (!UserLogic.CheckUserLogged(username, token))
            {
                return false;
            }
            return true;
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

            Assert.True(TestLoggedIn("testUser2", token));
            Assert.False(TestLoggedIn("testUser2", badToken));

        }

    }
}