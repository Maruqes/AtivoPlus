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
        // Helper: cria contexto, admin+permissão e um AtivoFinanceiro
        private static async Task<(AppDbContext db, int userId, int ativoId)> SetupImovelArrendadoPrereqs()
        {
            var db = GetPostgresDbContext();
            await UserLogic.AddUser(db, "admin", "admin");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");
            int uid = (await UserLogic.GetUserID(db, "admin")).Value;
            var ativo = new AtivoFinanceiro { UserId = uid };
            await db.AtivoFinanceiros.AddAsync(ativo);
            await db.SaveChangesAsync();
            return (db, uid, ativo.Id);
        }

        [Fact]
        public async Task AddImovelArrendado_Success_OwnerAddsForSelf()
        {
            var (db, userId, ativoId) = await SetupImovelArrendadoPrereqs();

            var req = new ImovelArrendadoRequest
            {
                UserId = -1,
                AtivoFinaceiroId = ativoId,
                Morada = "Rua A, 123",
                Designacao = "Apartamento",
                Localizacao = "Lisboa",
                ValorImovel = 200000m,
                ValorRenda = 1000m,
                ValorMensalCondominio = 50m,
                ValorAnualDespesasEstimadas = 500m,
                DataCriacao = DateTime.UtcNow
            };

            var result = await ImovelArrendadoLogic.AdicionarImovelArrendado(db, req, "admin");
            Assert.IsType<OkResult>(result);

            var lista = await db.GetImovelArrendadosByUserId(userId);
            Assert.Single(lista);
            Assert.Equal("Apartamento", lista[0].Designacao);
        }

        [Fact]
        public async Task AddImovelArrendado_Fails_NonOwner()
        {
            var (db, userId, ativoId) = await SetupImovelArrendadoPrereqs();
            await UserLogic.AddUser(db, "t1", "t1");

            var req = new ImovelArrendadoRequest
            {
                UserId = -1,
                AtivoFinaceiroId = ativoId,
                Morada = "Rua A, 123",
                Designacao = "Apartamento",
                Localizacao = "Lisboa",
                ValorImovel = 200000m,
                ValorRenda = 1000m,
                ValorMensalCondominio = 50m,
                ValorAnualDespesasEstimadas = 500m,
                DataCriacao = DateTime.UtcNow
            };

            var result = await ImovelArrendadoLogic.AdicionarImovelArrendado(db, req, "t1");
            var unauth = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User is not the owner of the asset", unauth.Value);
        }

        [Fact]
        public async Task RemoveImovelArrendado_Success_OwnerDeletes()
        {
            var (db, userId, ativoId) = await SetupImovelArrendadoPrereqs();

            // adiciona via lógica
            var addReq = new ImovelArrendadoRequest
            {
                UserId = -1,
                AtivoFinaceiroId = ativoId,
                Morada = "Rua B, 456",
                Designacao = "Moradia",
                Localizacao = "Porto",
                ValorImovel = 300000m,
                ValorRenda = 1500m,
                ValorMensalCondominio = 0m,
                ValorAnualDespesasEstimadas = 800m,
                DataCriacao = DateTime.UtcNow
            };
            await ImovelArrendadoLogic.AdicionarImovelArrendado(db, addReq, "admin");
            var imovel = (await db.GetImovelArrendadosByUserId(userId))[0];

            var result = await ImovelArrendadoLogic.RemoverImovelArrendado(db, imovel.Id, "admin");
            Assert.IsType<OkResult>(result);

            var lista = await db.GetImovelArrendadosByUserId(userId);
            Assert.Empty(lista);
        }

        [Fact]
        public async Task RemoveImovelArrendado_Fails_NonOwner()
        {
            var (db, userId, ativoId) = await SetupImovelArrendadoPrereqs();
            await UserLogic.AddUser(db, "t1", "t1");

            // Owner adiciona
            var addReq = new ImovelArrendadoRequest
            {
                UserId = -1,
                AtivoFinaceiroId = ativoId,
                Morada = "Rua C, 789",
                Designacao = "T1",
                Localizacao = "Coimbra",
                ValorImovel = 150000m,
                ValorRenda = 800m,
                ValorMensalCondominio = 30m,
                ValorAnualDespesasEstimadas = 400m,
                DataCriacao = DateTime.UtcNow
            };
            await ImovelArrendadoLogic.AdicionarImovelArrendado(db, addReq, "admin");
            var imovel = (await db.GetImovelArrendadosByUserId(userId))[0];

            var result = await ImovelArrendadoLogic.RemoverImovelArrendado(db, imovel.Id, "t1");
            var unauth = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User is not the owner of the asset", unauth.Value);
        }

        [Fact]
        public async Task RemoveImovelArrendado_Success_AdminDeletesOther()
        {
            var (db, userId, ativoId) = await SetupImovelArrendadoPrereqs();
            await UserLogic.AddUser(db, "t1", "t1");

            // Owner adiciona
            var addReq = new ImovelArrendadoRequest
            {
                UserId = -1,
                AtivoFinaceiroId = ativoId,
                Morada = "Rua D, 101",
                Designacao = "Studio",
                Localizacao = "Braga",
                ValorImovel = 120000m,
                ValorRenda = 600m,
                ValorMensalCondominio = 25m,
                ValorAnualDespesasEstimadas = 300m,
                DataCriacao = DateTime.UtcNow
            };
            await ImovelArrendadoLogic.AdicionarImovelArrendado(db, addReq, "admin");
            var imovel = (await db.GetImovelArrendadosByUserId(userId))[0];

            // Admin consegue remover (é owner via ativo.UserId)
            var result = await ImovelArrendadoLogic.RemoverImovelArrendado(db, imovel.Id, "admin");
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetAllImovelArrendados_Success_ReturnsList()
        {
            var (db, userId, ativoId) = await SetupImovelArrendadoPrereqs();

            // adiciona dois imóveis
            var req1 = new ImovelArrendadoRequest
            {
                UserId = -1,
                AtivoFinaceiroId = ativoId,
                Morada = "Rua E, 111",
                Designacao = "Flat",
                Localizacao = "Faro",
                ValorImovel = 130000m,
                ValorRenda = 650m,
                ValorMensalCondominio = 40m,
                ValorAnualDespesasEstimadas = 350m,
                DataCriacao = DateTime.UtcNow
            };
            var req2 = new ImovelArrendadoRequest
            {
                UserId = -1,
                AtivoFinaceiroId = ativoId,
                Morada = "Rua F, 222",
                Designacao = "Loja",
                Localizacao = "Évora",
                ValorImovel = 180000m,
                ValorRenda = 900m,
                ValorMensalCondominio = 45m,
                ValorAnualDespesasEstimadas = 370m,
                DataCriacao = DateTime.UtcNow
            };
            await ImovelArrendadoLogic.AdicionarImovelArrendado(db, req1, "admin");
            await ImovelArrendadoLogic.AdicionarImovelArrendado(db, req2, "admin");

            var action = await ImovelArrendadoLogic.GetAllImovelArrendados(db, "admin");
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var lista = Assert.IsType<List<ImovelArrendado>>(ok.Value);
            Assert.Equal(2, lista.Count);
        }


        [Fact]
        public async Task UpdateImovelArrendado_Success_OwnerUpdates()
        {
            var (db, userId, ativoId) = await SetupImovelArrendadoPrereqs();
            // adiciona
            var addReq = new ImovelArrendadoRequest
            {
                UserId = -1,
                AtivoFinaceiroId = ativoId,
                Morada = "Rua G, 333",
                Designacao = "Bangalô",
                Localizacao = "Sintra",
                ValorImovel = 220000m,
                ValorRenda = 1100m,
                ValorMensalCondominio = 50m,
                ValorAnualDespesasEstimadas = 400m,
                DataCriacao = DateTime.UtcNow
            };
            await ImovelArrendadoLogic.AdicionarImovelArrendado(db, addReq, "admin");
            var imovel = (await db.GetImovelArrendadosByUserId(userId))[0];

            var updReq = new ImovelArrendadoUpdateRequest
            {
                ImovelArrendadoId = imovel.Id,
                Morada = "Rua G, 999",
                ValorRenda = 1200m
            };
            var result = await ImovelArrendadoLogic.AtualizarImovelArrendado(db, updReq, "admin");
            Assert.IsType<OkResult>(result);

            var updated = await db.GetImovelArrendadoById(imovel.Id);
            Assert.Equal("Rua G, 999", updated!.MoradaId);
            Assert.Equal(1200m, updated.ValorRenda);
        }

        [Fact]
        public async Task UpdateImovelArrendado_Fails_NotOwner()
        {
            var (db, userId, ativoId) = await SetupImovelArrendadoPrereqs();
            await UserLogic.AddUser(db, "t1", "t1");

            // adiciona
            var addReq = new ImovelArrendadoRequest
            {
                UserId = -1,
                AtivoFinaceiroId = ativoId,
                Morada = "Rua H, 444",
                Designacao = "Quarto",
                Localizacao = "Bragança",
                ValorImovel = 90000m,
                ValorRenda = 450m,
                ValorMensalCondominio = 20m,
                ValorAnualDespesasEstimadas = 200m,
                DataCriacao = DateTime.UtcNow
            };
            await ImovelArrendadoLogic.AdicionarImovelArrendado(db, addReq, "admin");
            var imovel = (await db.GetImovelArrendadosByUserId(userId))[0];

            var updReq = new ImovelArrendadoUpdateRequest
            {
                ImovelArrendadoId = imovel.Id,
                Localizacao = "Viana do Castelo"
            };
            var result = await ImovelArrendadoLogic.AtualizarImovelArrendado(db, updReq, "t1");
            var unauth = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User is not the owner of the asset", unauth.Value);
        }

        [Fact]
        public async Task UpdateImovelArrendado_Fails_NotFound()
        {
            var (db, _, _) = await SetupImovelArrendadoPrereqs();
            var updReq = new ImovelArrendadoUpdateRequest
            {
                ImovelArrendadoId = 9999,
                Morada = "Nada"
            };
            var result = await ImovelArrendadoLogic.AtualizarImovelArrendado(db, updReq, "admin");
            var nf = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Imóvel arrendado não encontrado", nf.Value);
        }
    }
}
