using System;
using System.Threading.Tasks;
using Xunit;
using AtivoPlus.Data;
using AtivoPlus.Logic;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using AtivoPlus.Controllers;

namespace AtivoPlus.Tests
{
    public partial class UnitTests
    {
        // Use an in-memory database to isolate tests.


        [Fact]
        public async Task AddCarteira()
        {
            var db = GetPostgresDbContext();
            //criar user admin com permissao admin
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "t1", "t1");
            await UserLogic.AddUser(db, "t2", "t2");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste", UserId = -1 }, "admin");
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste", UserId = -1 }, "t1");
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste", UserId = -1 }, "t2");

            List<Carteira>? carteiras = await CarteiraLogic.GetCarteiras(db, "admin");
            Assert.NotNull(carteiras);
            Assert.Single(carteiras);
            Assert.Equal("CarteiraTeste", carteiras[0].Nome);

            carteiras = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteiras);
            Assert.Single(carteiras);
            Assert.Equal("CarteiraTeste", carteiras[0].Nome);

            carteiras = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteiras);
            Assert.Single(carteiras);
            Assert.Equal("CarteiraTeste", carteiras[0].Nome);
        }

        [Fact]
        public async Task AddCarteira2()
        {
            var db = GetPostgresDbContext();
            //criar user admin com permissao admin
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "t1", "t1");
            await UserLogic.AddUser(db, "t2", "t2");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            int? t1Id = await UserLogic.GetUserID(db, "t1");
            int? t2Id = await UserLogic.GetUserID(db, "t2");
            int? adminId = await UserLogic.GetUserID(db, "admin");

            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste1", UserId = t1Id.Value }, "admin");
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste2", UserId = t2Id.Value }, "admin");
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste3", UserId = adminId.Value }, "admin");

            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste11", UserId = t1Id.Value }, "t1");
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste22", UserId = t2Id.Value }, "t1");
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste33", UserId = adminId.Value }, "t1");

            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste111", UserId = t1Id.Value }, "t2");
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste222", UserId = t2Id.Value }, "t2");
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste333", UserId = adminId.Value }, "t2");

            List<Carteira>? carteiras = await CarteiraLogic.GetCarteiras(db, "admin");
            Assert.NotNull(carteiras);
            Assert.Single(carteiras);
            Assert.Equal("CarteiraTeste3", carteiras[0].Nome);

            carteiras = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteiras);
            Assert.Single(carteiras);
            Assert.Equal("CarteiraTeste1", carteiras[0].Nome);

            carteiras = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteiras);
            Assert.Single(carteiras);
            Assert.Equal("CarteiraTeste2", carteiras[0].Nome);



            //atualizar nome carteira
            //deve conseguir carteiras com o seu id ou admins
            carteiras = await CarteiraLogic.GetCarteiras(db, "admin");
            Assert.NotNull(carteiras);
            await CarteiraLogic.AtualizarNomeCarteira(db,
                 new CarteiraAlterarNomeRequest { CarteiraId = carteiras[0].Id, Nome = "CarteiraTeste3Novo" }, "admin");
            carteiras = await CarteiraLogic.GetCarteiras(db, "admin");
            Assert.NotNull(carteiras);
            Assert.Single(carteiras);
            Assert.Equal("CarteiraTeste3Novo", carteiras[0].Nome);

            carteiras = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteiras);
            await CarteiraLogic.AtualizarNomeCarteira(db,
                 new CarteiraAlterarNomeRequest { CarteiraId = carteiras[0].Id, Nome = "CarteiraTeste1Novo" }, "t1");
            carteiras = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteiras);
            Assert.Single(carteiras);
            Assert.Equal("CarteiraTeste1Novo", carteiras[0].Nome);

            carteiras = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteiras);
            await CarteiraLogic.AtualizarNomeCarteira(db,
                 new CarteiraAlterarNomeRequest { CarteiraId = carteiras[0].Id, Nome = "CarteiraTeste2Novo" }, "t2");
            carteiras = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteiras);
            Assert.Single(carteiras);
            Assert.Equal("CarteiraTeste2Novo", carteiras[0].Nome);


            //test change other users carteira
            carteiras = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteiras);
            await CarteiraLogic.AtualizarNomeCarteira(db,
                 new CarteiraAlterarNomeRequest { CarteiraId = carteiras[0].Id, Nome = "CarteiraTeste1Novo" }, "t2");
            carteiras = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteiras);
            Assert.Single(carteiras);
            Assert.Equal("CarteiraTeste1Novo", carteiras[0].Nome);

            carteiras = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteiras);
            await CarteiraLogic.AtualizarNomeCarteira(db,
                 new CarteiraAlterarNomeRequest { CarteiraId = carteiras[0].Id, Nome = "CarteiraTeste2Novo" }, "t1");
            carteiras = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteiras);
            Assert.Single(carteiras);
            Assert.Equal("CarteiraTeste2Novo", carteiras[0].Nome);

            carteiras = await CarteiraLogic.GetCarteiras(db, "admin");
            Assert.NotNull(carteiras);
            await CarteiraLogic.AtualizarNomeCarteira(db,
                 new CarteiraAlterarNomeRequest { CarteiraId = carteiras[0].Id, Nome = "CarteiraTeste3Novo" }, "t1");
            carteiras = await CarteiraLogic.GetCarteiras(db, "admin");
            Assert.NotNull(carteiras);
            Assert.Single(carteiras);
            Assert.Equal("CarteiraTeste3Novo", carteiras[0].Nome);


            //admin should be able to change carteiras name
            carteiras = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteiras);
            await CarteiraLogic.AtualizarNomeCarteira(db,
                 new CarteiraAlterarNomeRequest { CarteiraId = carteiras[0].Id, Nome = "CarteiraTesteADMIN" }, "admin");
            carteiras = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteiras);
            Assert.Single(carteiras);
            Assert.Equal("CarteiraTesteADMIN", carteiras[0].Nome);

            carteiras = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteiras);
            await CarteiraLogic.AtualizarNomeCarteira(db,
                 new CarteiraAlterarNomeRequest { CarteiraId = carteiras[0].Id, Nome = "CarteiraTesteADMIN" }, "admin");
            carteiras = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteiras);
            Assert.Single(carteiras);

            //delete carteira
            carteiras = await CarteiraLogic.GetCarteiras(db, "admin");
            Assert.NotNull(carteiras);
            await CarteiraLogic.ApagarCarteira(db, carteiras[0].Id, "admin");
            carteiras = await CarteiraLogic.GetCarteiras(db, "admin");
            Assert.NotNull(carteiras);
            Assert.Empty(carteiras);

            carteiras = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteiras);
            await CarteiraLogic.ApagarCarteira(db, carteiras[0].Id, "t1");
            carteiras = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteiras);
            Assert.Empty(carteiras);

            carteiras = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteiras);
            await CarteiraLogic.ApagarCarteira(db, carteiras[0].Id, "t2");
            carteiras = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteiras);
            Assert.Empty(carteiras);


        }

    }
}