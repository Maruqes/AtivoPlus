using System;
using System.Linq;
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
        // Helper para obter um contexto de BD isolado (pode ser um in-memory ou uma instância isolada de testes)
        private AppDbContext GetDbContext()
        {
            return GetPostgresDbContext();
        }

        [Fact]
        public async Task SetUserInfo_DeveAtualizarOsDadosDoUtilizadorEAtribuirAMorada_CuandoForAutoAtualizacao()
        {
            // ARRANGE
            var db = GetDbContext();
            // Criar os utilizadores: admin, t1 e t2
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "t1", "t1");
            await UserLogic.AddUser(db, "t2", "t2");
            // Atribuir permissão de admin
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // ACT
            // Cada utilizador define a sua própria informação e morada
            // O utilizador "admin"
            var resultAdmin = await UserInfoLogic.SetUserInfo(db, "admin", new UserInfo
            {
                Id = -1,
                Nome = "admin",
                Email = "email@email.com",
                Telefone = "123456789",
                Morada_id = 0,
                NIF = "123456789",
                IBAN = "PT50000201231234567890154"
            }, new MoradaRequest
            {
                Rua = "Rua do admin",
                Piso = "1",
                NumeroPorta = "1",
                Concelho = "Concelho do admin",
                Distrito = "Distrito do admin",
                Localidade = "Localidade do admin",
                CodPostal = "1234-567"
            });

            // O utilizador "t1"
            var resultT1 = await UserInfoLogic.SetUserInfo(db, "t1", new UserInfo
            {
                Id = -1,
                Nome = "t1",
                Email = "email@email.com",
                Telefone = "123456789",
                Morada_id = 0,
                NIF = "123456789",
                IBAN = "PT50000201231234567890154"
            }, new MoradaRequest
            {
                Rua = "Rua do t1",
                Piso = "1",
                NumeroPorta = "1",
                Concelho = "Concelho do t1",
                Distrito = "Distrito do t1",
                Localidade = "Localidade do t1",
                CodPostal = "1234-567"
            });

            // O utilizador "t2"
            var resultT2 = await UserInfoLogic.SetUserInfo(db, "t2", new UserInfo
            {
                Id = -1,
                Nome = "t2",
                Email = "email@email.com",
                Telefone = "123456789",
                Morada_id = 0,
                NIF = "123456789",
                IBAN = "PT50000201231234567890154"
            }, new MoradaRequest
            {
                Rua = "Rua do t2",
                Piso = "1",
                NumeroPorta = "1",
                Concelho = "Concelho do t2",
                Distrito = "Distrito do t2",
                Localidade = "Localidade do t2",
                CodPostal = "1234-567"
            });

            // ASSERT
            // Verifica a informação de "admin"
            UserInfo? adminInfo = await UserInfoLogic.GetUserInfo(db, "admin");
            Assert.NotNull(adminInfo);
            Assert.Equal("admin", adminInfo.Nome);
            Assert.Equal("123456789", adminInfo.Telefone);
            Assert.Equal("123456789", adminInfo.NIF);
            Assert.Equal("PT50000201231234567890154", adminInfo.IBAN);

            // Como a função para obter moradas devolve uma lista, verificamos que existe apenas um registo e depois os detalhes.
            Morada? adminMorada = await db.GetMoradasByUserId(adminInfo.Id);
            Assert.NotNull(adminMorada);
            Assert.Equal("Rua do admin", adminMorada.Rua);
            Assert.Equal("1", adminMorada.Piso);
            Assert.Equal("1", adminMorada.NumeroPorta);
            Assert.Equal("Concelho do admin", adminMorada.Concelho);
            Assert.Equal("Distrito do admin", adminMorada.Distrito);
            Assert.Equal("Localidade do admin", adminMorada.Localidade);
            Assert.Equal("1234-567", adminMorada.CodPostal);
            Assert.Equal(adminInfo.Id, adminMorada.User_id);

            // Verificar os nomes dos restantes utilizadores
            UserInfo? t1Info = await UserInfoLogic.GetUserInfo(db, "t1");
            Assert.NotNull(t1Info);
            Assert.Equal("t1", t1Info.Nome);

            UserInfo? t2Info = await UserInfoLogic.GetUserInfo(db, "t2");
            Assert.NotNull(t2Info);
            Assert.Equal("t2", t2Info.Nome);
        }

        [Fact]
        public async Task SetUserInfo_AdminNaoDevePermitirAtualizacaoPorOutrosUtilizadores()
        {
            // ARRANGE
            var db = GetDbContext();
            // Criar os utilizadores: admin, t1 e t2
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "t1", "t1");
            await UserLogic.AddUser(db, "t2", "t2");
            // Atribuir permissão de admin apenas ao "admin"
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Obter os IDs dos utilizadores para referência
            int? t1Id = await UserLogic.GetUserID(db, "t1");
            int? t2Id = await UserLogic.GetUserID(db, "t2");
            int? adminId = await UserLogic.GetUserID(db, "admin");
            Assert.NotNull(t1Id);
            Assert.NotNull(t2Id);
            Assert.NotNull(adminId);

            // ACT
            // O utilizador "admin" atualiza as informações de t1 (tendo permissão para tal)
            var resultt2Atualizat2 = await UserInfoLogic.SetUserInfo(db, "t2", new UserInfo
            {
                Id = -1, // Atualiza os dados de t2
                Nome = "t2",  
                Email = "t2@email.com",
                Telefone = "123456789",
                Morada_id = 0,
                NIF = "123456789",
                IBAN = "PT50000201231234567890154"
            }, new MoradaRequest
            {
                Rua = "Rua do t2",
                Piso = "2",
                NumeroPorta = "2",
                Concelho = "Concelho do admin",
                Distrito = "Distrito do admin",
                Localidade = "Localidade do admin",
                CodPostal = "1234-567"
            });

            // O utilizador "admin" cria a si proprio
            var resutlAdminAtualizaAdmin = await UserInfoLogic.SetUserInfo(db, "admin", new UserInfo
            {
                Id = -1, // Atualiza os dados de t2
                Nome = "admin",  
                Email = "admin@email.com",
                Telefone = "123456789",
                Morada_id = 0,
                NIF = "123456789",
                IBAN = "PT50000201231234567890154"
            }, new MoradaRequest
            {
                Rua = "Rua do admin",
                Piso = "admin",
                NumeroPorta = "admin",
                Concelho = "Concelho do admin",
                Distrito = "Distrito do admin",
                Localidade = "Localidade do admin",
                CodPostal = "1234-567"
            });


            // O utilizador "admin" atualiza as informações de t1 (tendo permissão para tal)
            var resultAdminAtualizaT1 = await UserInfoLogic.SetUserInfo(db, "admin", new UserInfo
            {
                Id = t1Id.Value, // Atualiza os dados de t1
                Nome = "admin",  // Neste cenário, admin define que o nome de t1 passe a ser "admin"
                Email = "admin@email.com",
                Telefone = "123456789",
                Morada_id = 0,
                NIF = "123456789",
                IBAN = "PT50000201231234567890154"
            }, new MoradaRequest
            {
                Rua = "Rua do admin",
                Piso = "1",
                NumeroPorta = "1",
                Concelho = "Concelho do admin",
                Distrito = "Distrito do admin",
                Localidade = "Localidade do admin",
                CodPostal = "1234-567"
            });

            // O utilizador "t1" tenta atualizar a informação de outro utilizador (neste caso, t2)
            var resultT1AtualizaT2 = await UserInfoLogic.SetUserInfo(db, "t1", new UserInfo
            {
                Id = t2Id.Value, // t1 tenta alterar os dados de t2
                Nome = "t1",
                Email = "t1@email.com",
                Telefone = "123456789",
                Morada_id = 0,
                NIF = "123456789",
                IBAN = "PT50000201231234567890154"
            }, new MoradaRequest
            {
                Rua = "Rua do t1",
                Piso = "1",
                NumeroPorta = "1",
                Concelho = "Concelho do t1",
                Distrito = "Distrito do t1",
                Localidade = "Localidade do t1",
                CodPostal = "1234-567"
            });

            // O utilizador "t2" tenta atualizar a informação de outro utilizador (admin)
            var resultT2AtualizaAdmin = await UserInfoLogic.SetUserInfo(db, "t2", new UserInfo
            {
                Id = adminId.Value, // t2 tenta alterar os dados de admin
                Nome = "t2",
                Email = "t2@email.com",
                Telefone = "123456789",
                Morada_id = 0,
                NIF = "123456789",
                IBAN = "PT50000201231234567890154"
            }, new MoradaRequest
            {
                Rua = "Rua do t2",
                Piso = "1",
                NumeroPorta = "1",
                Concelho = "Concelho do t2",
                Distrito = "Distrito do t2",
                Localidade = "Localidade do t2",
                CodPostal = "1234-567"
            });

            // ASSERT
            // Verificar que a intervenção de "admin" (que tem permissão) foi aplicada a t1
            UserInfo? t1Info = await UserInfoLogic.GetUserInfo(db, "t1");
            Assert.NotNull(t1Info);
            Assert.Equal("admin", t1Info.Nome);

            // Verificar que os dados de t2 não foram alterados, pois t1 não possui permissão para atualizar dados de terceiros
            UserInfo? t2Info = await UserInfoLogic.GetUserInfo(db, "t2");
            Assert.NotNull(t2Info);
            Assert.Equal("t2", t2Info.Nome);

            // Verificar que os dados de admin também se mantiveram inalterados
            UserInfo? adminInfo = await UserInfoLogic.GetUserInfo(db, "admin");
            Assert.NotNull(adminInfo);
            Assert.Equal("admin", adminInfo.Nome);
        }
    }
}
