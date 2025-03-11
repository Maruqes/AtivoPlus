using System;
using System.Threading.Tasks;
using Xunit;
using AtivoPlus.Data;
using AtivoPlus.Logic;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;

namespace AtivoPlus.Tests
{
    public partial class UnitTests
    {
        // Use an in-memory database to isolate tests.


        [Fact]
        public async Task SetUserInfo()
        {
            var db = GetPostgresDbContext();

            //criar user admin com permissao admin
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "t1", "t1");
            await UserLogic.AddUser(db, "t2", "t2");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            //testes a si mesmo
            await UserInfoLogic.SetUserInfo(db, "admin", new Models.UserInfo
            {
                Id = -1,
                Nome = "admin",
                Email = "email@email.com",
                Telefone = "123456789",
                Morada = "Rua do admin",
                NIF = "123456789",
                IBAN = "PT50000201231234567890154"
            });

            await UserInfoLogic.SetUserInfo(db, "t1", new Models.UserInfo
            {
                Id = -1,
                Nome = "t1",
                Email = "email@email.com",
                Telefone = "123456789",
                Morada = "Rua do t1",
                NIF = "123456789",
                IBAN = "PT50000201231234567890154"
            });

            await UserInfoLogic.SetUserInfo(db, "t2", new Models.UserInfo
            {
                Id = -1,
                Nome = "t2",
                Email = "email@email.com",
                Telefone = "123456789",
                Morada = "Rua do t2",
                NIF = "123456789",
                IBAN = "PT50000201231234567890154"
            });

            UserInfo? userInfo = await UserInfoLogic.GetUserInfo(db, "admin");
            Assert.NotNull(userInfo);
            Assert.Equal("admin", userInfo?.Nome);
            Assert.Equal("Rua do admin", userInfo?.Morada);
            Assert.Equal("123456789", userInfo?.Telefone);
            Assert.Equal("123456789", userInfo?.NIF);
            Assert.Equal("PT50000201231234567890154", userInfo?.IBAN);

            userInfo = await UserInfoLogic.GetUserInfo(db, "t1");
            Assert.NotNull(userInfo);
            Assert.Equal("t1", userInfo?.Nome);
            Assert.Equal("Rua do t1", userInfo?.Morada);
            Assert.Equal("123456789", userInfo?.Telefone);
            Assert.Equal("123456789", userInfo?.NIF);
            Assert.Equal("PT50000201231234567890154", userInfo?.IBAN);

            userInfo = await UserInfoLogic.GetUserInfo(db, "t2");
            Assert.NotNull(userInfo);
            Assert.Equal("t2", userInfo?.Nome);
            Assert.Equal("Rua do t2", userInfo?.Morada);
            Assert.Equal("123456789", userInfo?.Telefone);
            Assert.Equal("123456789", userInfo?.NIF);
            Assert.Equal("PT50000201231234567890154", userInfo?.IBAN);
        }

        [Fact]
        public async Task SetUserInfo2()
        {
            var db = GetPostgresDbContext();

            //criar user admin com permissao admin
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "t1", "t1");
            await UserLogic.AddUser(db, "t2", "t2");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            int? usert1Id = await UserLogic.GetUserID(db, "t1");
            int? usert12d = await UserLogic.GetUserID(db, "t2");
            int? useradmin = await UserLogic.GetUserID(db, "admin");
            if (usert1Id == null || usert12d == null || useradmin == null)
            {
                Assert.True(false);
            }
            await UserInfoLogic.SetUserInfo(db, "admin", new Models.UserInfo
            {
                Id = usert1Id.Value,
                Nome = "admin",
                Email = "email@email.com",
                Telefone = "123456789",
                Morada = "Rua do admin",
                NIF = "123456789",
                IBAN = "PT50000201231234567890154"
            });


            await UserInfoLogic.SetUserInfo(db, "t1", new Models.UserInfo
            {
                Id = usert12d.Value,
                Nome = "t1",
                Email = "email@email.com",
                Telefone = "123456789",
                Morada = "Rua do t1",
                NIF = "123456789",
                IBAN = "PT50000201231234567890154"
            });

            await UserInfoLogic.SetUserInfo(db, "t2", new Models.UserInfo
            {
                Id = useradmin.Value,
                Nome = "t2",
                Email = "email@email.com",
                Telefone = "123456789",
                Morada = "Rua do t2",
                NIF = "123456789",
                IBAN = "PT50000201231234567890154"
            });

            UserInfo? userInfo = await UserInfoLogic.GetUserInfo(db, "t1");
            Assert.NotNull(userInfo);
            Assert.Equal("admin", userInfo?.Nome);
            Assert.Equal("Rua do admin", userInfo?.Morada);
            Assert.Equal("123456789", userInfo?.Telefone);
            Assert.Equal("123456789", userInfo?.NIF);
            Assert.Equal("PT50000201231234567890154", userInfo?.IBAN);
        }
    }
}