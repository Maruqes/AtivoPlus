using Xunit;
using Microsoft.AspNetCore.Mvc;
using AtivoPlus.Data;
using AtivoPlus.Logic;
using AtivoPlus.Models;
using AtivoPlus.Controllers;

namespace AtivoPlus.Tests
{
    public partial class UnitTests
    {
        // Método auxiliar para configurar o banco de dados com um admin
        private static async Task<AppDbContext> SetupDatabaseWithAdmin()
        {
            var db = GetPostgresDbContext();
            await UserLogic.AddUser(db, "admin", "admin");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");
            return db;
        }

        // Método auxiliar para configurar o banco de dados com múltiplos utilizadores
        private static async Task<(AppDbContext db, int userIdT1)> SetupDatabaseWithMultipleUsers()
        {
            var db = GetPostgresDbContext();
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "t1", "t1");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            var userIdT1Nullable = await UserLogic.GetUserID(db, "t1");
            Assert.NotNull(userIdT1Nullable);
            var userIdT1 = userIdT1Nullable.Value;
            return (db, userIdT1);
        }

        // ----------------------------------------------------------------
        // Testes unitários para EntidadeAtivoLogic
        // ----------------------------------------------------------------

        [Fact]
        public async Task AdicionarEntidadeAtivo_Success_AdminAddsForThemselves()
        {
            // Arrange: Configurar o contexto da base de dados e os dados necessários
            var db = await SetupDatabaseWithAdmin();

            var request = new EntidadeAtivoRequest
            {
                UserId = -1, // Utilizador autenticado (admin)
                Nome = "Banco XYZ"
            };

            // Act: Chamar a lógica para adicionar uma entidade ativa
            ActionResult result = await EntidadeAtivoLogic.AdicionarEntidadeAtivo(db, request, "admin");

            // Assert: Verificar o resultado e o estado da base de dados
            Assert.IsType<OkResult>(result);

            var userId = await UserLogic.GetUserID(db, "admin");
            Assert.NotNull(userId);
            var entidades = await db.GetEntidadeAtivoByUserId(userId.Value);
            Assert.NotNull(entidades);
            Assert.Single(entidades);
            Assert.Equal("Banco XYZ", entidades.First().Nome);
        }

        [Fact]
        public async Task AdicionarEntidadeAtivo_Success_AdminAddsForOtherUser()
        {
            // Arrange: Configurar o contexto da base de dados, utilizadores e permissões
            var (db, userIdT1) = await SetupDatabaseWithMultipleUsers();

            var request = new EntidadeAtivoRequest
            {
                UserId = userIdT1, // ID do utilizador "t1"
                Nome = "Banco T1"
            };

            // Act: Adicionar uma entidade ativa para outro utilizador como admin
            ActionResult result = await EntidadeAtivoLogic.AdicionarEntidadeAtivo(db, request, "admin");

            // Assert: Verificar o resultado e o estado da base de dados
            Assert.IsType<OkResult>(result);

            var entidades = await db.GetEntidadeAtivoByUserId(userIdT1);
            Assert.NotNull(entidades);
            Assert.Single(entidades);
            Assert.Equal("Banco T1", entidades.First().Nome);
        }

        [Fact]
        public async Task AdicionarEntidadeAtivo_Fails_NonAdminAddsForOtherUser()
        {
            // Arrange: Configurar o contexto da base de dados e utilizadores
            var (db, userIdAdmin) = await SetupDatabaseWithMultipleUsers();

            var request = new EntidadeAtivoRequest
            {
                UserId = userIdAdmin, // ID do utilizador "admin"
                Nome = "Banco Admin"
            };

            // Act: Tentar adicionar uma entidade ativa para outro utilizador como não-admin
            ActionResult result = await EntidadeAtivoLogic.AdicionarEntidadeAtivo(db, request, "t1");

            // Assert: Verificar que o resultado é UnauthorizedObjectResult com a mensagem correta
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User is not an admin", unauthorizedResult.Value);
        }

        [Fact]
        public async Task VerEntidadesAtivo_RetornaEntidadesDoUsuario()
        {
            var db = await SetupDatabaseWithAdmin();
            var userId = (await UserLogic.GetUserID(db, "admin")).Value;

            await db.AdicionarEntidadeAtivo("Banco A", userId);
            await db.AdicionarEntidadeAtivo("Banco B", userId);

            var entidades = await db.GetEntidadeAtivoByUserId(userId);

            Assert.NotNull(entidades);
            Assert.Equal(2, entidades.Count);
            Assert.Contains(entidades, e => e.Nome == "Banco A");
            Assert.Contains(entidades, e => e.Nome == "Banco B");
        }

        [Fact]
        public async Task ApagarEntidadeAtivo_Success_OwnerDeletesTheirOwnEntity()
        {
            // Arrange: Configurar o contexto da base de dados, utilizador e entidade ativa
            var db = await SetupDatabaseWithAdmin();
            var userId = (await UserLogic.GetUserID(db, "admin")).Value;
            var entidade = new EntidadeAtivo { Nome = "Banco XYZ", UserId = userId };
            await db.AdicionarEntidadeAtivo(entidade.Nome, entidade.UserId);

            var entidades = await db.GetEntidadeAtivoByUserId(userId);
            Assert.NotNull(entidades);
            Assert.Single(entidades);
            Assert.Equal("Banco XYZ", entidades.First().Nome);

            entidade.Id = entidades.First().Id; // Ensure the Id is set

            // Act: Chamar a lógica para apagar a entidade ativa
            ActionResult result = await EntidadeAtivoLogic.ApagarEntidadeAtivo(db, entidade.Id, "admin");

            // Assert: Verificar o resultado e o estado da base de dados
            Assert.IsType<OkResult>(result);

            var adminEntidades = await db.GetEntidadeAtivoByUserId(userId);
            Assert.Empty(adminEntidades);
        }

        [Fact]
        public async Task ApagarEntidadeAtivo_AdminDeletesOtherUsersEntity()
        {
            var (db, userIdT1) = await SetupDatabaseWithMultipleUsers();
            var entidade = new EntidadeAtivo { Nome = "Banco T1", UserId = userIdT1 };
            await db.AdicionarEntidadeAtivo(entidade.Nome, entidade.UserId);
            var entidades = await db.GetEntidadeAtivoByUserId(userIdT1);
            Assert.NotNull(entidades);
            Assert.Single(entidades);
            Assert.Equal("Banco T1", entidades.First().Nome);
            entidade.Id = entidades.First().Id; // Ensure the Id is set
            // Act: Chamar a lógica para apagar a entidade ativa
            ActionResult result = await EntidadeAtivoLogic.ApagarEntidadeAtivo(db, entidade.Id, "admin");
            // Assert: Verificar o resultado e o estado da base de dados
            Assert.IsType<OkResult>(result);
            var t1Entidades = await db.GetEntidadeAtivoByUserId(userIdT1);
            Assert.Empty(t1Entidades);
        }
        
        [Fact]
        public async Task ApagarEntidadeAtivo_Fails_NonOwnerOrNonAdminAttemptsToDelete()
        {
            // Arrange: Configurar o contexto da base de dados, utilizadores e entidade ativa
            var (db, userIdT1) = await SetupDatabaseWithMultipleUsers();
            var entidade = new EntidadeAtivo { Nome = "Banco T1", UserId = userIdT1 };
            await db.AdicionarEntidadeAtivo(entidade.Nome, entidade.UserId);
            var entidades = await db.GetEntidadeAtivoByUserId(userIdT1);
            Assert.NotNull(entidades);
            Assert.Single(entidades);
            Assert.Equal("Banco T1", entidades.First().Nome);
            // Act: Tentar apagar a entidade ativa como um utilizador sem permissão
            ActionResult result = await EntidadeAtivoLogic.ApagarEntidadeAtivo(db, entidades.First().Id, "another_user");
            // Assert: Verificar que o resultado é UnauthorizedResult
            Assert.IsType<UnauthorizedObjectResult>(result);
            var t1Entidades = await db.GetEntidadeAtivoByUserId(userIdT1);
            Assert.Single(t1Entidades);
            Assert.Equal("Banco T1", t1Entidades.First().Nome);
        }
        
        [Fact]
        public async Task ApagarEntidadeAtivo_Fails_EntityNotFound()
        {
            // Arrange: Configurar o contexto da base de dados e utilizador
            var db = await SetupDatabaseWithAdmin();
            var userId = (await UserLogic.GetUserID(db, "admin")).Value;

            // Act: Tentar apagar uma entidade ativa que não existe
            ActionResult result = await EntidadeAtivoLogic.ApagarEntidadeAtivo(db, 999, "admin");

            // Assert: Verificar que o resultado é NotFoundObjectResult com a mensagem correta
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Entidade not found", notFoundResult.Value);
        }


        [Fact]
        public async Task ApagarEntidadeAtivo_Fails_InvalidUser()
        {
            // Arrange: Configurar o contexto da base de dados
            var db = GetPostgresDbContext();

            // Act: Tentar apagar uma entidade ativa com um utilizador inexistente
            ActionResult result = await EntidadeAtivoLogic.ApagarEntidadeAtivo(db, 999, "nonexistent_user");

            // Assert: Verificar que o resultado é UnauthorizedObjectResult com a mensagem correta
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User not found", unauthorizedResult.Value);
        }






        //Tem de se verificar na logica se o nome é vazio
        
        /*[Fact]
        public async Task AdicionarEntidadeAtivo_Fails_EmptyName()
        {
            var db = await SetupDatabaseWithAdmin();
            var userId = (await UserLogic.GetUserID(db, "admin")).Value;

            var request = new EntidadeAtivoRequest
            {
                UserId = userId,
                Nome = "" // Nome inválido
            };

            ActionResult result = await EntidadeAtivoLogic.AdicionarEntidadeAtivo(db, request, "admin");

            Assert.IsType<OkResult>(result);
            var entidades = await db.GetEntidadeAtivoByUserId(userId);
            Assert.NotNull(entidades);
            Assert.Empty(entidades); // Nenhuma entidade deve ser adicionada
        }*/

        //Tem de se verificar na logica se o nome é duplicado
        
        /*[Fact]
        public async Task AdicionarEntidadeAtivo_Fails_NomeDuplicado()
        {
            var db = await SetupDatabaseWithAdmin();
            var userId = (await UserLogic.GetUserID(db, "admin")).Value;

            await db.AdicionarEntidadeAtivo("Banco Duplicado", userId);

            var request = new EntidadeAtivoRequest
            {
                UserId = userId,
                Nome = "Banco Duplicado"
            };

            ActionResult result = await EntidadeAtivoLogic.AdicionarEntidadeAtivo(db, request, "admin");
            
            // Verifica se o resultado é um OkObjectResult
            // e se a mensagem de erro é a esperada
            Assert.IsType<OkResult>(result);
            // Verifica se a entidade não foi adicionada novamente
            var entidades = await db.GetEntidadeAtivoByUserId(userId);
            Assert.NotNull(entidades);
            Assert.Single(entidades);
            Assert.Equal("Banco Duplicado", entidades.First().Nome);
        }*/

    }
}