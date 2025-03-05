using System;
using System.Threading.Tasks;
using Xunit;
using AtivoPlus.Data;
using AtivoPlus.Logic;
using Microsoft.EntityFrameworkCore;

namespace AtivoPlus.Tests
{
    public partial class UnitTests
    {
        // Use an in-memory database to isolate tests.


        [Fact]
        public async Task Admin_CanCreatePermission()
        {
            var db = GetPostgresDbContext();

            //criar user admin com permissao admin
            await UserLogic.AddUser(db, "admin", "admin");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");


            //loga o user admin     
            string token = await UserLogic.LogarUser(db, "admin", "admin");
            Assert.False(string.IsNullOrEmpty(token));

            //checka se o user admin tem a permissão de admin
            bool hasAdminRole = await PermissionLogic.CheckPermission(db, "admin", new[] { "admin" });
            Assert.True(hasAdminRole);

            //checka se a permissão existe "testPermission" nao deve existir
            bool exists = await PermissionLogic.DoesExistPermission(db, "testPermission");
            Assert.False(exists);

            //adiciona a permissão "testPermission"
            bool addResult = await PermissionLogic.AddPermission(db, "testPermission");
            Assert.True(addResult);

            //checka se a permissão existe "testPermission" deve existir
            exists = await PermissionLogic.DoesExistPermission(db, "testPermission");
            Assert.True(exists);

            //adiciona a permissão "testPermission" novamente, deve retornar false para nao repetir
            bool duplicateAdd = await PermissionLogic.AddPermission(db, "testPermission");
            Assert.False(duplicateAdd);
        }
    }
}