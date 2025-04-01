using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using AtivoPlus.Data;
using AtivoPlus.Logic;
using AtivoPlus.Models;
using Microsoft.AspNetCore.Mvc;
using AtivoPlus.Controllers;

namespace AtivoPlus.Tests
{
    public partial class UnitTests
    {
        // ----------------------------------------------------------------
        // Testes para adicionar Banco (bancos) com controlo de acesso adequado.
        // ----------------------------------------------------------------

        [Fact]
        public async Task AddBanco()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizador "admin"
            await UserLogic.AddUser(db, "admin", "admin");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Apenas o admin pode adicionar um banco
            ActionResult result = await BancoLogic.AdicionarBanco(db, new BancoRequest { Nome = "BancoTeste" }, "admin");
            Assert.IsType<OkResult>(result);

            // Obter a lista de bancos e verificar se foi adicionado corretamente
            List<Banco>? bancos = await BancoLogic.GetBancos(db);
            Assert.NotNull(bancos);
            Assert.Single(bancos);
            Assert.Equal("BancoTeste", bancos[0].Nome);
        }

        [Fact]
        public async Task AddBanco_AccessControl()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizadores "admin" e "t1"
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "t1", "t1");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // O utilizador "t1" NÃO deve conseguir adicionar um banco
            ActionResult result = await BancoLogic.AdicionarBanco(db, new BancoRequest { Nome = "BancoTeste" }, "t1");
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task UpdateBanco_AccessControl()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizadores "admin" e "t1"
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "t1", "t1");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Adicionar um banco como admin
            await BancoLogic.AdicionarBanco(db, new BancoRequest { Nome = "BancoTeste" }, "admin");
            List<Banco>? bancos = await BancoLogic.GetBancos(db);
            Assert.NotNull(bancos);
            Assert.Single(bancos);

            // O admin pode atualizar o banco
            ActionResult result = await BancoLogic.AlterarBanco(db, new BancoRequestChangeName { bancoId = bancos[0].Id, Nome = "BancoNovo" }, "admin");
            Assert.IsType<OkResult>(result);

            // Verificar se o nome foi atualizado
            bancos = await BancoLogic.GetBancos(db);
            Assert.NotNull(bancos);
            Assert.Equal("BancoNovo", bancos![0].Nome);

            // O utilizador "t1" NÃO deve conseguir atualizar o banco
            result = await BancoLogic.AlterarBanco(db, new BancoRequestChangeName { bancoId = bancos[0].Id, Nome = "BancoT1" }, "t1");
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DeleteBanco_AccessControl()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizadores "admin" e "t1"
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "t1", "t1");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Adicionar um banco como admin
            await BancoLogic.AdicionarBanco(db, new BancoRequest { Nome = "BancoTeste" }, "admin");
            List<Banco>? bancos = await BancoLogic.GetBancos(db);
            Assert.NotNull(bancos);
            Assert.Single(bancos);

            // Apenas o admin pode apagar o banco
            ActionResult result = await BancoLogic.ApagarBanco(db, bancos[0].Id, "admin");
            Assert.IsType<OkResult>(result);

            // Verificar se o banco foi apagado
            bancos = await BancoLogic.GetBancos(db);
            Assert.NotNull(bancos);
            Assert.Empty(bancos);

            // O utilizador "t1" NÃO deve conseguir apagar um banco
            await BancoLogic.AdicionarBanco(db, new BancoRequest { Nome = "BancoTeste2" }, "admin");
            bancos = await BancoLogic.GetBancos(db);
            Assert.NotNull(bancos);
            Assert.Single(bancos);
            result = await BancoLogic.ApagarBanco(db, bancos[0].Id, "t1");
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DeleteBanco_NonExistent()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizador "admin"
            await UserLogic.AddUser(db, "admin", "admin");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Tentar apagar um banco inexistente
            ActionResult result = await BancoLogic.ApagarBanco(db, 999, "admin");
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateBanco_NonExistent()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizador "admin"
            await UserLogic.AddUser(db, "admin", "admin");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Tentar atualizar um banco inexistente
            ActionResult result = await BancoLogic.AlterarBanco(db, new BancoRequestChangeName { bancoId = 999, Nome = "BancoNovo" }, "admin");
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task AddBanco_DuplicateName()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizador "admin"
            await UserLogic.AddUser(db, "admin", "admin");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Adicionar um banco inicialmente
            await BancoLogic.AdicionarBanco(db, new BancoRequest { Nome = "BancoTeste" }, "admin");

            // Tentar adicionar um banco com o mesmo nome
            ActionResult result = await BancoLogic.AdicionarBanco(db, new BancoRequest { Nome = "BancoTeste" }, "admin");
            Assert.IsType<BadRequestObjectResult>(result);

            // Verificar se apenas um banco existe
            List<Banco>? bancos = await BancoLogic.GetBancos(db);
            Assert.NotNull(bancos);
            Assert.Single(bancos);
        }

        [Fact]
        public async Task UpdateBanco_DuplicateName()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizador "admin"
            await UserLogic.AddUser(db, "admin", "admin");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Adicionar dois bancos inicialmente
            await BancoLogic.AdicionarBanco(db, new BancoRequest { Nome = "BancoTeste1" }, "admin");
            await BancoLogic.AdicionarBanco(db, new BancoRequest { Nome = "BancoTeste2" }, "admin");

            // Obter a lista de bancos
            List<Banco>? bancos = await BancoLogic.GetBancos(db);
            Assert.NotNull(bancos);
            Assert.Equal(2, bancos.Count);

            // Tentar atualizar o nome do primeiro banco para o nome do segundo banco
            ActionResult result = await BancoLogic.AlterarBanco(db, new BancoRequestChangeName { bancoId = bancos[0].Id, Nome = "BancoTeste2" }, "admin");
            Assert.IsType<BadRequestObjectResult>(result);

            // Verificar se os nomes permanecem inalterados
            bancos = await BancoLogic.GetBancos(db);
            Assert.NotNull(bancos);
            Assert.Equal("BancoTeste1", bancos[0].Nome);
            Assert.Equal("BancoTeste2", bancos[1].Nome);
        }

        [Fact]
        public async Task GetBancos_Listing()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizador "admin"
            await UserLogic.AddUser(db, "admin", "admin");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Adicionar múltiplos bancos
            await BancoLogic.AdicionarBanco(db, new BancoRequest { Nome = "BancoA" }, "admin");
            await BancoLogic.AdicionarBanco(db, new BancoRequest { Nome = "BancoB" }, "admin");
            await BancoLogic.AdicionarBanco(db, new BancoRequest { Nome = "BancoC" }, "admin");

            // Obter a lista de bancos e verificar a contagem
            List<Banco>? bancos = await BancoLogic.GetBancos(db);
            Assert.NotNull(bancos);
            Assert.Equal(3, bancos.Count);

            // Verificar a ordem alfabética (se relevante)
            Assert.Equal("BancoA", bancos[0].Nome);
            Assert.Equal("BancoB", bancos[1].Nome);
            Assert.Equal("BancoC", bancos[2].Nome);
        }

        [Fact]
        public async Task AddMultipleBancos()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizador "admin"
            await UserLogic.AddUser(db, "admin", "admin");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Adicionar múltiplos bancos
            await BancoLogic.AdicionarBanco(db, new BancoRequest { Nome = "BancoX" }, "admin");
            await BancoLogic.AdicionarBanco(db, new BancoRequest { Nome = "BancoY" }, "admin");
            await BancoLogic.AdicionarBanco(db, new BancoRequest { Nome = "BancoZ" }, "admin");

            // Obter a lista de bancos e verificar a contagem
            List<Banco>? bancos = await BancoLogic.GetBancos(db);
            Assert.NotNull(bancos);
            Assert.Equal(3, bancos.Count);

            // Verificar a presença de todos os bancos
            Assert.Contains(bancos, b => b.Nome == "BancoX");
            Assert.Contains(bancos, b => b.Nome == "BancoY");
            Assert.Contains(bancos, b => b.Nome == "BancoZ");
        }
    }
}