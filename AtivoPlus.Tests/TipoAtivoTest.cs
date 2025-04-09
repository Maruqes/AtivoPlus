using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        // ----------------------------------------------------------------
        // Testes para TipoAtivoLogic
        // ----------------------------------------------------------------

        [Fact]
        public async Task AdicionarTipoAtivo()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizador "admin"
            await UserLogic.AddUser(db, "admin", "admin");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Apenas o admin pode adicionar um TipoAtivo
            ActionResult result = await TipoAtivoLogic.AdicionarTipoAtivo(db, new TipoAtivo { Nome = "Ações" }, "admin");
            // Verificar se o resultado é do tipo OkResult
            Assert.IsType<OkResult>(result);

            // Obter a lista de TiposAtivo
            List<TipoAtivo>? tiposAtivo = await TipoAtivoLogic.GetTiposAtivo(db);
            // Verificar se a lista não é nula
            Assert.NotNull(tiposAtivo);
            // Verificar se existe apenas um TipoAtivo
            Assert.Single(tiposAtivo);
            // Verificar se o nome do TipoAtivo é "Ações"
            Assert.Equal("Ações", tiposAtivo[0].Nome);
        }

        [Fact]
        public async Task AdicionarTipoAtivo_AccessControl()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizadores "admin" e "t1"
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "t1", "t1");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // O utilizador "t1" NÃO deve conseguir adicionar um TipoAtivo
            ActionResult result = await TipoAtivoLogic.AdicionarTipoAtivo(db, new TipoAtivo { Nome = "Ações" }, "t1");
            // Verificar se o resultado é do tipo UnauthorizedObjectResult
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task AlterarTipoAtivo()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizador "admin"
            await UserLogic.AddUser(db, "admin", "admin");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Adicionar um TipoAtivo como admin
            await TipoAtivoLogic.AdicionarTipoAtivo(db, new TipoAtivo { Nome = "Ações" }, "admin");
            // Obter a lista de TiposAtivo
            List<TipoAtivo>? tiposAtivo = await TipoAtivoLogic.GetTiposAtivo(db);
            // Verificar se a lista não é nula
            Assert.NotNull(tiposAtivo);
            // Verificar se existe apenas um TipoAtivo
            Assert.Single(tiposAtivo);

            // O admin pode atualizar o TipoAtivo
            ActionResult result = await TipoAtivoLogic.AlterarTipoAtivo(db, new TipoAtivoRequestChangeName { TipoAtivoId = tiposAtivo[0].Id, Nome = "Fundos" }, "admin");
            // Verificar se o resultado é do tipo OkResult
            Assert.IsType<OkResult>(result);

            // Obter a lista de TiposAtivo novamente
            tiposAtivo = await TipoAtivoLogic.GetTiposAtivo(db);
            // Verificar se a lista não é nula
            Assert.NotNull(tiposAtivo);
            // Verificar se o nome do TipoAtivo foi alterado para "Fundos"
            Assert.Equal("Fundos", tiposAtivo![0].Nome);
        }

        [Fact]
        public async Task AlterarTipoAtivo_AccessControl()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizadores "admin" e "t1"
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "t1", "t1");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Adicionar um TipoAtivo como admin
            await TipoAtivoLogic.AdicionarTipoAtivo(db, new TipoAtivo { Nome = "Ações" }, "admin");
            // Obter a lista de TiposAtivo
            List<TipoAtivo>? tiposAtivo = await TipoAtivoLogic.GetTiposAtivo(db);
            // Verificar se a lista não é nula
            Assert.NotNull(tiposAtivo);
            // Verificar se existe apenas um TipoAtivo
            Assert.Single(tiposAtivo);

            // O utilizador "t1" NÃO deve conseguir atualizar o TipoAtivo
            ActionResult result = await TipoAtivoLogic.AlterarTipoAtivo(db, new TipoAtivoRequestChangeName { TipoAtivoId = tiposAtivo[0].Id, Nome = "Fundos" }, "t1");
            // Verificar se o resultado é do tipo UnauthorizedObjectResult
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task ApagarTipoAtivo()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizador "admin"
            await UserLogic.AddUser(db, "admin", "admin");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Adicionar um TipoAtivo como admin
            await TipoAtivoLogic.AdicionarTipoAtivo(db, new TipoAtivo { Nome = "Ações" }, "admin");
            // Obter a lista de TiposAtivo
            List<TipoAtivo>? tiposAtivo = await TipoAtivoLogic.GetTiposAtivo(db);
            // Verificar se a lista não é nula
            Assert.NotNull(tiposAtivo);
            // Verificar se existe apenas um TipoAtivo
            Assert.Single(tiposAtivo);

            // O admin pode apagar o TipoAtivo
            ActionResult result = await TipoAtivoLogic.ApagarTipoAtivo(db, tiposAtivo[0].Id, "admin");
            // Verificar se o resultado é do tipo OkResult
            Assert.IsType<OkResult>(result);

            // Obter a lista de TiposAtivo novamente
            tiposAtivo = await TipoAtivoLogic.GetTiposAtivo(db);
            // Verificar se a lista não é nula
            Assert.NotNull(tiposAtivo);
            // Verificar se a lista está vazia
            Assert.Empty(tiposAtivo);
        }

        [Fact]
        public async Task ApagarTipoAtivo_AccessControl()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizadores "admin" e "t1"
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "t1", "t1");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Adicionar um TipoAtivo como admin
            await TipoAtivoLogic.AdicionarTipoAtivo(db, new TipoAtivo { Nome = "Ações" }, "admin");
            // Obter a lista de TiposAtivo
            List<TipoAtivo>? tiposAtivo = await TipoAtivoLogic.GetTiposAtivo(db);
            // Verificar se a lista não é nula
            Assert.NotNull(tiposAtivo);
            // Verificar se existe apenas um TipoAtivo
            Assert.Single(tiposAtivo);

            // O utilizador "t1" NÃO deve conseguir apagar o TipoAtivo
            ActionResult result = await TipoAtivoLogic.ApagarTipoAtivo(db, tiposAtivo[0].Id, "t1");
            // Verificar se o resultado é do tipo UnauthorizedObjectResult
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task VerTiposAtivo()
        {
            // Obter o contexto da base de dados PostgreSQL
            var db = GetPostgresDbContext();
            // Adicionar utilizador "admin"
            await UserLogic.AddUser(db, "admin", "admin");
            // Adicionar permissões ao utilizador "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Adicionar múltiplos TiposAtivo como admin
            await TipoAtivoLogic.AdicionarTipoAtivo(db, new TipoAtivo { Nome = "Ações" }, "admin");
            await TipoAtivoLogic.AdicionarTipoAtivo(db, new TipoAtivo { Nome = "Fundos" }, "admin");
            await TipoAtivoLogic.AdicionarTipoAtivo(db, new TipoAtivo { Nome = "Criptomoedas" }, "admin");

            // Obter a lista de TiposAtivo
            List<TipoAtivo>? tiposAtivo = await TipoAtivoLogic.GetTiposAtivo(db);
            // Verificar se a lista não é nula
            Assert.NotNull(tiposAtivo);
            // Verificar se existem 3 TiposAtivo
            Assert.Equal(3, tiposAtivo.Count);
            // Verificar se a lista contém os TiposAtivo adicionados
            Assert.Contains(tiposAtivo, t => t.Nome == "Ações");
            Assert.Contains(tiposAtivo, t => t.Nome == "Fundos");
            Assert.Contains(tiposAtivo, t => t.Nome == "Criptomoedas");
        }
    }
}