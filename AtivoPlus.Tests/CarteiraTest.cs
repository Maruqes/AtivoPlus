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
            // var db = GetPostgresDbContext();
            // //criar user admin com permissao admin
            // await UserLogic.AddUser(db, "admin", "admin");
            // await UserLogic.AddUser(db, "t1", "t1");
            // await UserLogic.AddUser(db, "t2", "t2");
            // await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste", UserId = -1 }, "admin");
            // await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste", UserId = -1 }, "t1");   
            // await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste", UserId = -1 }, "t2");

            //ta meio de fzr    
        }
    }
}