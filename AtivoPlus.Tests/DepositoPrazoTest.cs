using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using AtivoPlus.Data;
using AtivoPlus.Logic;
using AtivoPlus.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Controllers;

namespace AtivoPlus.Tests
{
    public partial class UnitTests
    {
        // Helper para criar ativo financeiro e banco
        private static async Task<(AppDbContext db, int userId, int ativoId, int bancoId)> SetupDepositoPrereqs()
        {
            var db = GetPostgresDbContext();
            // cria admin
            await UserLogic.AddUser(db, "admin", "admin");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");
            int userId = (await UserLogic.GetUserID(db, "admin")).Value;

            // cria ativo financeiro
            var ativo = new AtivoFinanceiro { UserId = userId };
            await db.AtivoFinanceiros.AddAsync(ativo);
            await db.SaveChangesAsync();

            // cria banco
            var banco = new Banco { Nome = "BancoTeste" };
            await db.Bancos.AddAsync(banco);
            await db.SaveChangesAsync();

            return (db, userId, ativo.Id, banco.Id);
        }

        [Fact]
        public async Task AdicionarDepositoPrazo_Success_OwnerAddsForSelf()
        {
            var (db, userId, ativoId, bancoId) = await SetupDepositoPrereqs();

            var req = new DepositoPrazoRequest {
                UserId = -1,
                AtivoFinaceiroId = ativoId,
                BancoId = bancoId,
                NumeroConta = 1234,
                TaxaJuroAnual = 0.05f,
                ValorAtual = 1000m,
                ValorInvestido = 1000m,
                ValorAnualDespesasEstimadas = 10m,
                DataCriacao = DateTime.UtcNow
            };

            // owner adiciona
            var result = await DepositoPrazoLogic.AdicionarDepositoPrazo(db, req, "admin");
            Assert.IsType<OkResult>(result);

            // verifica que foi criado
            var lista = await db.GetDepositoPrazosByTitularId(userId);
            Assert.Single(lista);
            Assert.Equal(1000m, lista[0].ValorInvestido);
        }

        [Fact]
        public async Task AdicionarDepositoPrazo_Fails_NonOwner()
        {
            var (db, userId, ativoId, bancoId) = await SetupDepositoPrereqs();
            // cria t1
            await UserLogic.AddUser(db, "t1", "t1");
            var req = new DepositoPrazoRequest {
                UserId = -1,
                AtivoFinaceiroId = ativoId,
                BancoId = bancoId,
                NumeroConta = 1234,
                TaxaJuroAnual = 0.05f,
                ValorAtual = 1000m,
                ValorInvestido = 1000m,
                ValorAnualDespesasEstimadas = 10m,
                DataCriacao = DateTime.UtcNow
            };

            // t1 não é owner
            var result = await DepositoPrazoLogic.AdicionarDepositoPrazo(db, req, "t1");
            var bad = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User is not the owner of the asset, trying to do something fishy?", bad.Value);
        }

        [Fact]
        public async Task AdicionarDepositoPrazo_Success_AdminAddsForOther()
        {
            var (db, adminId, ativoId, bancoId) = await SetupDepositoPrereqs();
            // cria t1
            await UserLogic.AddUser(db, "t1", "t1");
            int t1Id = (await UserLogic.GetUserID(db, "t1")).Value;

            var req = new DepositoPrazoRequest {
                UserId                        = t1Id,
                AtivoFinaceiroId              = ativoId,
                BancoId                       = bancoId,
                NumeroConta                   = 4321,
                TaxaJuroAnual                 = 0.04f,
                ValorAtual                    = 2000m,
                ValorInvestido                = 2000m,
                ValorAnualDespesasEstimadas   = 20m,
                DataCriacao                   = DateTime.UtcNow
            };

            // admin tenta adicionar para outro titular → Unauthorized
            var result = await DepositoPrazoLogic.AdicionarDepositoPrazo(db, req, "admin");
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User is not the owner of the asset, trying to do something fishy?", unauthorized.Value);
        }


        [Fact]
        public async Task RemoverDepositoPrazo_Success_OwnerDeletes()
        {
            var (db, userId, ativoId, bancoId) = await SetupDepositoPrereqs();

            // cria depósito direto
            await db.CreateDepositoPrazo(ativoId, bancoId, userId, 1111, 0.03f, 500m, 500m, 5m, DateTime.UtcNow);
            var dp = (await db.GetDepositoPrazosByTitularId(userId))[0];

            // owner remove
            var result = await DepositoPrazoLogic.RemoverDepositoPrazo(db, dp.Id, "admin");
            Assert.IsType<OkResult>(result);

            // agora lista vazia
            var lista = await db.GetDepositoPrazosByTitularId(userId);
            Assert.Empty(lista);
        }

        [Fact]
        public async Task RemoverDepositoPrazo_Fails_NonOwnerOrNonAdmin()
        {
            var (db, adminId, ativoId, bancoId) = await SetupDepositoPrereqs();
            // cria t1
            await UserLogic.AddUser(db, "t1", "t1");
            int t1Id = (await UserLogic.GetUserID(db, "t1")).Value;

            // cria depósito para admin
            await db.CreateDepositoPrazo(ativoId, bancoId, adminId, 2222, 0.03f, 500m, 500m, 5m, DateTime.UtcNow);
            var dp = (await db.GetDepositoPrazosByTitularId(adminId))[0];

            // t1 tenta remover
            var result = await DepositoPrazoLogic.RemoverDepositoPrazo(db, dp.Id, "t1");
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User is not the owner of the asset, trying to do something fishy?", unauthorized.Value);
        }

        [Fact]
        public async Task RemoverDepositoPrazo_Success_AdminDeletesOther()
        {
            var (db, adminId, ativoId, bancoId) = await SetupDepositoPrereqs();
            // cria t1
            await UserLogic.AddUser(db, "t1", "t1");
            int t1Id = (await UserLogic.GetUserID(db, "t1")).Value;

            // cria depósito para t1
            await db.CreateDepositoPrazo(ativoId, bancoId, t1Id, 3333, 0.02f, 300m, 300m, 3m, DateTime.UtcNow);
            var dp = (await db.GetDepositoPrazosByTitularId(t1Id))[0];

            // admin remove
            var result = await DepositoPrazoLogic.RemoverDepositoPrazo(db, dp.Id, "admin");
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task RemoverDepositoPrazo_Fails_NotFound()
        {
            var (db, _, _, _) = await SetupDepositoPrereqs();
            var result = await DepositoPrazoLogic.RemoverDepositoPrazo(db, 9999, "admin");
            var nf = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Deposito not found", nf.Value);
        }

        [Fact]
        public async Task GetAllDepositoPrazos_ReturnsOnlyUserRecords()
        {
            var (db, adminId, ativoId, bancoId) = await SetupDepositoPrereqs();
            await UserLogic.AddUser(db, "t1", "t1");
            int t1Id = (await UserLogic.GetUserID(db, "t1")).Value;

            // Dois depósitos para admin
            await db.DepositoPrazos.AddAsync(new DepositoPrazo {
                AtivoFinaceiroId            = ativoId,
                BancoId                     = bancoId,
                TitularId                   = adminId,
                NumeroConta                 = 4444,
                TaxaJuroAnual               = 0.01f,
                ValorAtual                  = 100m,
                ValorInvestido              = 100m,
                ValorAnualDespesasEstimadas = 1m,
                DataCriacao                 = DateTime.UtcNow
            });
            await db.DepositoPrazos.AddAsync(new DepositoPrazo {
                AtivoFinaceiroId            = ativoId,
                BancoId                     = bancoId,
                TitularId                   = adminId,
                NumeroConta                 = 5555,
                TaxaJuroAnual               = 0.01f,
                ValorAtual                  = 200m,
                ValorInvestido              = 200m,
                ValorAnualDespesasEstimadas = 2m,
                DataCriacao                 = DateTime.UtcNow
            });

            // Um para t1
            await db.DepositoPrazos.AddAsync(new DepositoPrazo {
                AtivoFinaceiroId            = ativoId,
                BancoId                     = bancoId,
                TitularId                   = t1Id,
                NumeroConta                 = 6666,
                TaxaJuroAnual               = 0.01f,
                ValorAtual                  = 300m,
                ValorInvestido              = 300m,
                ValorAnualDespesasEstimadas = 3m,
                DataCriacao                 = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var result = await DepositoPrazoLogic.GetAllDepositoPrazos(db, "admin");
            var action = Assert.IsType<ActionResult<List<DepositoPrazo>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(action.Result);
            var lista = okResult.Value as List<DepositoPrazo>;
            Assert.NotNull(lista);
            Assert.Equal(2, lista.Count);
        }


    }
}
