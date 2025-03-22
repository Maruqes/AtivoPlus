using System;
using System.Threading.Tasks;
using Xunit;
using AtivoPlus.Data;
using AtivoPlus.Logic;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using AtivoPlus.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace AtivoPlus.Tests
{
    public partial class UnitTests
    {
        // Use an in-memory database to isolate tests.


        [Fact]
        public async Task GetMoneyAtivoFinanceiroTeste()
        {
            FinnhubLogic.StartFinnhubLogic();

            Decimal? spy = await FinnhubLogic.GetETF("SPY");
            Decimal? aapl = await FinnhubLogic.GetStock("AAPL");
            Decimal? btc = await FinnhubLogic.GetCrypto("BTC");
            if (spy == null || aapl == null || btc == null)
            {
                Assert.True(false);
            }

            Assert.True(spy.Value > 0);
            Assert.True(aapl.Value > 0);
            Assert.True(btc.Value > 0);
        }

        private async static Task CreateCarteira(AppDbContext db, int? t1Id, int? t2Id, int? adminId)
        {
            if (!t1Id.HasValue || !t2Id.HasValue || !adminId.HasValue)
            {
                throw new ArgumentNullException("User IDs cannot be null");
            }

            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste1", UserId = t1Id.Value }, "admin");
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste11", UserId = t1Id.Value }, "admin");
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste2", UserId = t2Id.Value }, "admin");
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste3", UserId = adminId.Value }, "admin");

            return;
        }

        [Fact]
        public async Task AdicionarAtivoFinanceiroTeste()
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

            await CreateCarteira(db, t1Id, t2Id, adminId);

            List<Carteira>? carteiras = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteiras);

            ActionResult act1 = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = -1,
                CarteiraId = carteiras[0].Id,
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                EntidadeAtivoId = 1,
                TaxaImposto = 0.1f
            }, "t1");
            Assert.IsType<OkResult>(act1);
            List<AtivoFinanceiro> ativos = await db.GetAtivoByUserId(t1Id.Value);
            Assert.Single(ativos);

            ActionResult act2 = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = -1,
                CarteiraId = carteiras[0].Id,
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                EntidadeAtivoId = 1,
                TaxaImposto = 0.1f
            }, "t1");

            Assert.IsType<OkResult>(act2);
            ativos = await db.GetAtivoByUserId(t1Id.Value);
            Assert.Equal(2, ativos.Count);

            ActionResult act3 = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = -1,
                CarteiraId = carteiras[0].Id,
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                EntidadeAtivoId = 1,
                TaxaImposto = 0.1f
            }, "t1");

            Assert.IsType<OkResult>(act3);
            ativos = await db.GetAtivoByUserId(t1Id.Value);
            Assert.Equal(3, ativos.Count);


            await AtivoFinanceiroLogic.AlterarAtivoFinanceiroParaOutraCarteira(db, new AtivoFinanceiroAlterarCarteiraRequest
            {
                AtivoFinanceiroId = ativos[0].Id,
                CarteiraId = carteiras[1].Id,
                UserId = -1
            }, "t1");

            ativos = await db.GetAtivoByUserId(t1Id.Value);
            Assert.Equal(3, ativos.Count);
            if (ativos[0].CarteiraId != carteiras[1].Id)
            {
                Assert.True(true);
            }
            else
            {
                Assert.True(false);
            }
        }

        [Fact]
        public async Task AdicionarAtivoFinanceiroTeste2()
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

            await CreateCarteira(db, t1Id, t2Id, adminId);

            List<Carteira>? carteiras = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteiras);

            ActionResult act1 = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = -1,
                CarteiraId = carteiras[0].Id,
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                EntidadeAtivoId = 1,
                TaxaImposto = 0.1f
            }, "t2");
            Assert.IsType<OkResult>(act1);
            List<AtivoFinanceiro> ativos = await db.GetAtivoByUserId(t2Id.Value);
            Assert.Single(ativos);

            ActionResult act2 = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = -1,
                CarteiraId = carteiras[0].Id,
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                EntidadeAtivoId = 1,
                TaxaImposto = 0.1f
            }, "t2");

            Assert.IsType<OkResult>(act2);
            ativos = await db.GetAtivoByUserId(t2Id.Value);
            Assert.Equal(2, ativos.Count);


            ActionResult act3 = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = -1,
                CarteiraId = carteiras[0].Id,
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                EntidadeAtivoId = 1,
                TaxaImposto = 0.1f
            }, "t2");

            Assert.IsType<OkResult>(act3);
            ativos = await db.GetAtivoByUserId(t2Id.Value);
            Assert.Equal(3, ativos.Count);
        }

        [Fact]
        public async Task AdicionarAtivoFinanceiroTeste3()
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

            await CreateCarteira(db, t1Id, t2Id, adminId);

            List<Carteira>? carteiras = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteiras);

            ActionResult act1 = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = t1Id.Value,
                CarteiraId = carteiras[0].Id,
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                EntidadeAtivoId = 1,
                TaxaImposto = 0.1f
            }, "admin");
            Assert.IsType<OkResult>(act1);
            List<AtivoFinanceiro> ativos = await db.GetAtivoByUserId(t1Id.Value);
            Assert.Single(ativos);

            carteiras = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteiras);

            ActionResult act2 = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = t2Id.Value,
                CarteiraId = carteiras[0].Id,
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                EntidadeAtivoId = 1,
                TaxaImposto = 0.1f
            }, "admin");

            Assert.IsType<OkResult>(act2);
            ativos = await db.GetAtivoByUserId(t2Id.Value);
            Assert.Single(ativos);
        }
    }
}