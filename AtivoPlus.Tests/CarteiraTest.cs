using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using AtivoPlus.Data;
using AtivoPlus.Logic;
using AtivoPlus.Models;
using AtivoPlus.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace AtivoPlus.Tests
{
    public partial class UnitTests
    {
        // ----------------------------------------------------------------
        // Tests for adding Carteira (wallets) with proper access control.
        // ----------------------------------------------------------------

        // This test verifies that when a caller uses -1 as the UserId,
        // the system correctly assigns the wallet to the caller.
        [Fact]
        public async Task AddCarteira()
        {
            var db = GetPostgresDbContext();
            // Setup: create admin and two regular users.
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "t1", "t1");
            await UserLogic.AddUser(db, "t2", "t2");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            // Each caller creates a wallet using -1 so that it is assigned to themselves.
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste", UserId = -1 }, "admin");
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste", UserId = -1 }, "t1");
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste", UserId = -1 }, "t2");

            List<Carteira>? carteirasAdmin = await CarteiraLogic.GetCarteiras(db, "admin");
            Assert.NotNull(carteirasAdmin);
            Assert.Single(carteirasAdmin);
            Assert.Equal("CarteiraTeste", carteirasAdmin[0].Nome);

            List<Carteira>? carteirasT1 = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteirasT1);
            Assert.Single(carteirasT1);
            Assert.Equal("CarteiraTeste", carteirasT1[0].Nome);

            List<Carteira>? carteirasT2 = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteirasT2);
            Assert.Single(carteirasT2);
            Assert.Equal("CarteiraTeste", carteirasT2[0].Nome);
        }

        // This test covers several scenarios:
        // • An admin can create a wallet for any user by specifying that user’s ID.
        // • A normal user must create a wallet for themselves using -1 (or their own ID) only.
        //   Attempts to create a wallet for another user should fail.
        // • Updating a wallet’s name is allowed only for the wallet’s owner or an admin.
        // • Deleting a wallet is allowed only for the wallet’s owner or an admin.
        [Fact]
        public async Task AddCarteira2()
        {
            var db = GetPostgresDbContext();
            // Setup: create admin, and users t1 and t2.
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "t1", "t1");
            await UserLogic.AddUser(db, "t2", "t2");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            int? t1Id = await UserLogic.GetUserID(db, "t1");
            int? t2Id = await UserLogic.GetUserID(db, "t2");
            int? adminId = await UserLogic.GetUserID(db, "admin");

            // Admin creates wallets for each user.
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste1", UserId = t1Id.Value }, "admin");
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste2", UserId = t2Id.Value }, "admin");
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste3", UserId = adminId.Value }, "admin");

            // Normal users attempting to create wallets:
            // t1 creating a wallet for themselves using their explicit ID should not succeed.
            ActionResult result = await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste11", UserId = t1Id.Value }, "t1");
            Assert.IsType<UnauthorizedObjectResult>(result);
            // t1 attempting to create a wallet for t2 should be rejected.
            result = await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste22", UserId = t2Id.Value }, "t1");
            Assert.IsType<UnauthorizedObjectResult>(result);
            // Similarly, t1 attempting to create a wallet for admin should be rejected.
            result = await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste33", UserId = adminId.Value }, "t1");
            Assert.IsType<UnauthorizedObjectResult>(result);

            // Also, t2 attempting to create a wallet for someone else should fail.
            result = await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraTeste111", UserId = t1Id.Value }, "t2");
            Assert.IsType<UnauthorizedObjectResult>(result);

            // Verify that the wallets created by admin are correctly assigned.
            List<Carteira>? carteirasAdmin = await CarteiraLogic.GetCarteiras(db, "admin");
            Assert.NotNull(carteirasAdmin);
            Assert.Single(carteirasAdmin);
            Assert.Equal("CarteiraTeste3", carteirasAdmin[0].Nome);

            List<Carteira>? carteirasT1 = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteirasT1);
            Assert.Single(carteirasT1);
            Assert.Equal("CarteiraTeste1", carteirasT1[0].Nome);

            List<Carteira>? carteirasT2 = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteirasT2);
            Assert.Single(carteirasT2);
            Assert.Equal("CarteiraTeste2", carteirasT2[0].Nome);

            // --------------------
            // Updating wallet names
            // --------------------
            // An admin or the wallet’s owner may update the wallet's name.
            // Admin updates its own wallet.
            carteirasAdmin = await CarteiraLogic.GetCarteiras(db, "admin");
            Assert.NotNull(carteirasAdmin);
            await CarteiraLogic.AtualizarNomeCarteira(db,
                 new CarteiraAlterarNomeRequest { CarteiraId = carteirasAdmin[0].Id, Nome = "CarteiraTeste3Novo" }, "admin");
            carteirasAdmin = await CarteiraLogic.GetCarteiras(db, "admin");
            Assert.NotNull(carteirasAdmin);
            Assert.Single(carteirasAdmin);
            Assert.Equal("CarteiraTeste3Novo", carteirasAdmin[0].Nome);

            // Owner (t1) updates their own wallet.
            carteirasT1 = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteirasT1);
            await CarteiraLogic.AtualizarNomeCarteira(db,
                 new CarteiraAlterarNomeRequest { CarteiraId = carteirasT1[0].Id, Nome = "CarteiraTeste1Novo" }, "t1");
            carteirasT1 = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteirasT1);
            Assert.Single(carteirasT1);
            Assert.Equal("CarteiraTeste1Novo", carteirasT1[0].Nome);

            // Owner (t2) updates their own wallet.
            carteirasT2 = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteirasT2);
            await CarteiraLogic.AtualizarNomeCarteira(db,
                 new CarteiraAlterarNomeRequest { CarteiraId = carteirasT2[0].Id, Nome = "CarteiraTeste2Novo" }, "t2");
            carteirasT2 = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteirasT2);
            Assert.Single(carteirasT2);
            Assert.Equal("CarteiraTeste2Novo", carteirasT2[0].Nome);

            // A user attempting to update another user's wallet should be denied.
            // t2 tries to update t1's wallet.
            carteirasT1 = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteirasT1);
            result = await CarteiraLogic.AtualizarNomeCarteira(db,
                 new CarteiraAlterarNomeRequest { CarteiraId = carteirasT1[0].Id, Nome = "CarteiraTeste1Alterado" }, "t2");
            Assert.IsType<UnauthorizedObjectResult>(result);
            // Similarly, t1 tries to update t2's wallet.
            carteirasT2 = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteirasT2);
            result = await CarteiraLogic.AtualizarNomeCarteira(db,
                 new CarteiraAlterarNomeRequest { CarteiraId = carteirasT2[0].Id, Nome = "CarteiraTeste2Alterado" }, "t1");
            Assert.IsType<UnauthorizedObjectResult>(result);

            // An admin should be able to update any wallet.
            result = await CarteiraLogic.AtualizarNomeCarteira(db,
                 new CarteiraAlterarNomeRequest { CarteiraId = carteirasT1[0].Id, Nome = "CarteiraTesteADMIN" }, "admin");
            Assert.IsType<OkResult>(result);
            carteirasT1 = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteirasT1);
            Assert.Equal("CarteiraTesteADMIN", carteirasT1[0].Nome);

            result = await CarteiraLogic.AtualizarNomeCarteira(db,
                 new CarteiraAlterarNomeRequest { CarteiraId = carteirasT2[0].Id, Nome = "CarteiraTesteADMIN" }, "admin");
            Assert.IsType<OkResult>(result);
            carteirasT2 = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteirasT2);
            Assert.Equal("CarteiraTesteADMIN", carteirasT2[0].Nome);

            // -----------------
            // Deleting wallets
            // -----------------
            // Only the wallet’s owner or an admin should be able to delete it.

            // Admin deletes its own wallet.
            carteirasAdmin = await CarteiraLogic.GetCarteiras(db, "admin");
            Assert.NotNull(carteirasAdmin);
            await CarteiraLogic.ApagarCarteira(db, carteirasAdmin[0].Id, "admin");
            carteirasAdmin = await CarteiraLogic.GetCarteiras(db, "admin");
            Assert.NotNull(carteirasAdmin);
            Assert.Empty(carteirasAdmin);

            // t1 deletes its own wallet.
            carteirasT1 = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteirasT1);
            await CarteiraLogic.ApagarCarteira(db, carteirasT1[0].Id, "t1");
            carteirasT1 = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteirasT1);
            Assert.Empty(carteirasT1);

            // t2 deletes its own wallet.
            carteirasT2 = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteirasT2);
            await CarteiraLogic.ApagarCarteira(db, carteirasT2[0].Id, "t2");
            carteirasT2 = await CarteiraLogic.GetCarteiras(db, "t2");
            Assert.NotNull(carteirasT2);
            Assert.Empty(carteirasT2);

            // Finally, test that a non-owner cannot delete someone else's wallet.
            // Recreate a wallet for t1 using admin.
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = "CarteiraNovo", UserId = t1Id.Value }, "admin");
            carteirasT1 = await CarteiraLogic.GetCarteiras(db, "t1");
            Assert.NotNull(carteirasT1);
            Assert.Single(carteirasT1);
            result = await CarteiraLogic.ApagarCarteira(db, carteirasT1[0].Id, "t2");
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
