using System;
using System.Threading.Tasks;
using Xunit;
using AtivoPlus.Data;
using AtivoPlus.Logic;
using Microsoft.EntityFrameworkCore;

namespace AtivoPlus.Tests
{
    public partial class UnitTest1Test
    {
        // Use an in-memory database to isolate tests.


        // [Fact]
        // public async Task Admin_CanCreatePermission()
        // {
        //     var db = GetPostgresDbContext();

        //     //loga o user admin 
        //     string token = await UserLogic.LogarUser(db, "admin", "admin");
        //     Assert.False(string.IsNullOrEmpty(token));

        //     //checka se o user admin tem a permissão de admin
        //     bool hasAdminRole = await PermissionLogic.CheckPermission(db, "admin", new[] { "admin" });
        //     Assert.True(hasAdminRole);

        //     //checka se a permissão existe "testPermission" nao deve existir
        //     bool exists = await PermissionLogic.DoesExistPermission(db, "testPermission");
        //     Assert.False(exists);

        //     //adiciona a permissão "testPermission"
        //     bool addResult = await PermissionLogic.AddPermission(db, "testPermission");
        //     Assert.True(addResult);

        //     //checka se a permissão existe "testPermission" deve existir
        //     exists = await PermissionLogic.DoesExistPermission(db, "testPermission");
        //     Assert.True(exists);

        //     //adiciona a permissão "testPermission" novamente, deve retornar false para nao repetir
        //     bool duplicateAdd = await PermissionLogic.AddPermission(db, "testPermission");
        //     Assert.False(duplicateAdd);
        // }

        // [Fact]
        // public async Task NonAdmin_CannotCreatePermission()
        // {
        //     var db = GetPostgresDbContext();

        //     await UserLogic.AddUser(db, "user1", "password1");
        //     string token = await UserLogic.LogarUser(db, "user1", "password1");
        //     Assert.False(string.IsNullOrEmpty(token));

        //     // Check that the user does not have admin permission.
        //     bool hasAdminRole = await PermissionLogic.CheckPermission(db, "user1", new[] { "testPermission" });
        //     Assert.False(hasAdminRole);


        //     if (await PermissionLogic.AddUserPermission(db, "user1", "testPermission") == false)
        //     {
        //         Console.WriteLine("Could not add permission");
        //         Assert.False(true);
        //     }

        //     // Now the user has the permission.
        //     hasAdminRole = await PermissionLogic.CheckPermission(db, "user1", new[] { "testPermission" });
        //     Assert.True(hasAdminRole);

        // }

    }
}