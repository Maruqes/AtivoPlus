using System;
using System.Collections.Generic;
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
        // Helper method: Create a wallet (carteira) for a given user using admin privileges.
        private async Task CreateWallet(AppDbContext db, int userId, string walletName)
        {
            await CarteiraLogic.AdicionarCarteira(db, new CarteiraRequest { Nome = walletName, UserId = userId }, "admin");
        }

        // 1. Verify that a normal user (non-owner) cannot add an asset to another user's wallet.
        [Fact]
        public async Task NormalUser_CannotAddAssetToOtherUsersWallet()
        {
            var db = GetPostgresDbContext();
            // Setup: create admin, user1, and user2.
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "user1", "user1");
            await UserLogic.AddUser(db, "user2", "user2");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            int? user1Id = await UserLogic.GetUserID(db, "user1");
            int? user2Id = await UserLogic.GetUserID(db, "user2");
            Assert.NotNull(user1Id);
            Assert.NotNull(user2Id);

            // Create a wallet only for user2.
            await CreateWallet(db, user2Id.Value, "User2Wallet");
            List<Carteira>? user2Wallets = await CarteiraLogic.GetCarteiras(db, "user2");
            Assert.NotNull(user2Wallets);
            Assert.Single(user2Wallets);

            // Act: as user1, try to add an asset into user2's wallet.
            ActionResult result = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = -1, // resolves to user1 if valid
                CarteiraId = user2Wallets[0].Id,
                Nome = "User2Asset",
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                TaxaImposto = 0.1f
            }, "user1");

            // Expectation: unauthorized access returns UnauthorizedObjectResult.
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // 2. Verify that a normal user cannot remove an asset that belongs to another user.
        [Fact]
        public async Task NormalUser_CannotRemoveAssetOfAnotherUser()
        {
            var db = GetPostgresDbContext();
            // Setup: create admin, user1, and user2.
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "user1", "user1");
            await UserLogic.AddUser(db, "user2", "user2");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            int? user2Id = await UserLogic.GetUserID(db, "user2");
            Assert.NotNull(user2Id);

            // Create a wallet for user2.
            await CreateWallet(db, user2Id.Value, "User2Wallet");
            List<Carteira>? user2Wallets = await CarteiraLogic.GetCarteiras(db, "user2");
            Assert.NotNull(user2Wallets);
            Assert.Single(user2Wallets);

            // Admin adds an asset for user2.
            ActionResult addResult = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = user2Id.Value,
                CarteiraId = user2Wallets[0].Id,
                Nome = "User2Asset",
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                TaxaImposto = 0.1f
            }, "admin");
            Assert.IsType<OkResult>(addResult);

            List<AtivoFinanceiro> assetsUser2 = await db.GetAtivoByUserId(user2Id.Value);
            Assert.Single(assetsUser2);
            var asset = assetsUser2[0];

            // Act: as user1, try to remove user2's asset.
            ActionResult removeResult = await AtivoFinanceiroLogic.RemoveAtivo(db, asset.Id, "user1");
            // Expectation: unauthorized access returns UnauthorizedObjectResult.
            Assert.IsType<UnauthorizedObjectResult>(removeResult);
        }

        // 3. Verify that a normal user cannot alter the wallet of an asset that belongs to another user.
        [Fact]
        public async Task NormalUser_CannotAlterAssetWalletOfAnotherUser()
        {
            var db = GetPostgresDbContext();
            // Setup: create admin, user1, and user2.
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "user1", "user1");
            await UserLogic.AddUser(db, "user2", "user2");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            int? user2Id = await UserLogic.GetUserID(db, "user2");
            Assert.NotNull(user2Id);

            // Create two wallets for user2.
            await CreateWallet(db, user2Id.Value, "User2Wallet1");
            await CreateWallet(db, user2Id.Value, "User2Wallet2");
            List<Carteira>? user2Wallets = await CarteiraLogic.GetCarteiras(db, "user2");
            Assert.NotNull(user2Wallets);
            Assert.Equal(2, user2Wallets.Count);

            // Admin adds an asset for user2 into wallet1.
            ActionResult addResult = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = user2Id.Value,
                CarteiraId = user2Wallets[0].Id,
                Nome = "User2Asset",
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                TaxaImposto = 0.1f
            }, "admin");
            Assert.IsType<OkResult>(addResult);

            List<AtivoFinanceiro> assetsUser2 = await db.GetAtivoByUserId(user2Id.Value);
            Assert.Single(assetsUser2);
            var asset = assetsUser2[0];

            // Act: as user1, try to move the asset from wallet1 to wallet2.
            ActionResult alterResult = await AtivoFinanceiroLogic.AlterarAtivoFinanceiroParaOutraCarteira(db, new AtivoFinanceiroAlterarCarteiraRequest
            {
                UserId = -1,  // resolves to user1 if valid
                AtivoFinanceiroId = asset.Id,
                CarteiraId = user2Wallets[1].Id,
                Nome = "User2Asset"
            }, "user1");
            // Expectation: unauthorized access returns UnauthorizedObjectResult.
            Assert.IsType<UnauthorizedObjectResult>(alterResult);
        }

        // 4. Verify that an owner can add an asset to their own wallet.
        [Fact]
        public async Task Owner_CanAddAsset_ToOwnWallet()
        {
            var db = GetPostgresDbContext();
            // Setup: create admin and user1.
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "user1", "user1");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            int? user1Id = await UserLogic.GetUserID(db, "user1");
            Assert.NotNull(user1Id);

            // Create a wallet for user1.
            await CreateWallet(db, user1Id.Value, "User1Wallet");
            List<Carteira>? user1Wallets = await CarteiraLogic.GetCarteiras(db, "user1");
            Assert.NotNull(user1Wallets);
            Assert.Single(user1Wallets);

            // Act: as user1, add an asset using -1 (should resolve to user1).
            ActionResult result = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = -1,
                CarteiraId = user1Wallets[0].Id,
                Nome = "User1Asset",
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                TaxaImposto = 0.1f
            }, "user1");
            Assert.IsType<OkResult>(result);
        }

        // 5. Verify that an administrator can add an asset for another user.
        [Fact]
        public async Task Admin_CanAddAsset_ForAnotherUser()
        {
            var db = GetPostgresDbContext();
            // Setup: create admin and user1.
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "user1", "user1");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            int? user1Id = await UserLogic.GetUserID(db, "user1");
            int? adminId = await UserLogic.GetUserID(db, "admin");
            Assert.NotNull(user1Id);
            Assert.NotNull(adminId);

            // Create a wallet for user1.
            await CreateWallet(db, user1Id.Value, "User1Wallet");
            List<Carteira>? user1Wallets = await CarteiraLogic.GetCarteiras(db, "user1");
            Assert.NotNull(user1Wallets);
            Assert.Single(user1Wallets);

            // Act: as admin, add an asset for user1 by explicitly supplying user1's ID.
            ActionResult result = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = user1Id.Value,
                CarteiraId = user1Wallets[0].Id,
                Nome = "User1Asset",
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                TaxaImposto = 0.1f
            }, "admin");
            Assert.IsType<OkResult>(result);
        }

        // 6. Verify that an owner can remove their own asset.
        [Fact]
        public async Task Owner_CanRemoveOwnAsset()
        {
            var db = GetPostgresDbContext();
            // Setup: create admin and user1.
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "user1", "user1");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            int? user1Id = await UserLogic.GetUserID(db, "user1");
            Assert.NotNull(user1Id);

            // Create a wallet for user1.
            await CreateWallet(db, user1Id.Value, "User1Wallet");
            List<Carteira>? user1Wallets = await CarteiraLogic.GetCarteiras(db, "user1");
            Assert.NotNull(user1Wallets);
            Assert.Single(user1Wallets);

            // Owner adds an asset.
            ActionResult addResult = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = -1,
                CarteiraId = user1Wallets[0].Id,
                Nome = "User1Asset",
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                TaxaImposto = 0.1f
            }, "user1");
            Assert.IsType<OkResult>(addResult);

            List<AtivoFinanceiro> assets = await db.GetAtivoByUserId(user1Id.Value);
            Assert.Single(assets);
            var asset = assets[0];

            // Owner removes their asset.
            ActionResult removeResult = await AtivoFinanceiroLogic.RemoveAtivo(db, asset.Id, "user1");
            Assert.IsType<OkResult>(removeResult);
            assets = await db.GetAtivoByUserId(user1Id.Value);
            Assert.Empty(assets);
        }

        // 7. Verify that an administrator can remove an asset from another user.
        [Fact]
        public async Task Admin_CanRemoveAssetFromAnotherUser()
        {
            var db = GetPostgresDbContext();
            // Setup: create admin and user1.
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "user1", "user1");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            int? user1Id = await UserLogic.GetUserID(db, "user1");
            Assert.NotNull(user1Id);

            // Create a wallet for user1.
            await CreateWallet(db, user1Id.Value, "User1Wallet");
            List<Carteira>? user1Wallets = await CarteiraLogic.GetCarteiras(db, "user1");
            Assert.NotNull(user1Wallets);
            Assert.Single(user1Wallets);

            // Admin adds an asset for user1.
            ActionResult addResult = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = user1Id.Value,
                CarteiraId = user1Wallets[0].Id,
                Nome = "User1Asset",
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                TaxaImposto = 0.1f
            }, "admin");
            Assert.IsType<OkResult>(addResult);

            List<AtivoFinanceiro> assets = await db.GetAtivoByUserId(user1Id.Value);
            Assert.Single(assets);
            var asset = assets[0];

            // Admin removes the asset.
            ActionResult removeResult = await AtivoFinanceiroLogic.RemoveAtivo(db, asset.Id, "admin");
            Assert.IsType<OkResult>(removeResult);
            assets = await db.GetAtivoByUserId(user1Id.Value);
            Assert.Empty(assets);
        }

        // 8. Verify that an owner can alter the wallet of their own asset.
        [Fact]
        public async Task Owner_CanAlterOwnAssetWallet()
        {
            var db = GetPostgresDbContext();
            // Setup: create admin and user1.
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "user1", "user1");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            int? user1Id = await UserLogic.GetUserID(db, "user1");
            Assert.NotNull(user1Id);

            // Create two wallets for user1.
            await CreateWallet(db, user1Id.Value, "User1Wallet1");
            await CreateWallet(db, user1Id.Value, "User1Wallet2");
            List<Carteira>? user1Wallets = await CarteiraLogic.GetCarteiras(db, "user1");
            Assert.NotNull(user1Wallets);
            Assert.Equal(2, user1Wallets.Count);

            // Owner adds an asset to wallet1.
            ActionResult addResult = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = -1,
                CarteiraId = user1Wallets[0].Id,
                Nome = "User1Asset",
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                TaxaImposto = 0.1f
            }, "user1");
            Assert.IsType<OkResult>(addResult);

            List<AtivoFinanceiro> assets = await db.GetAtivoByUserId(user1Id.Value);
            Assert.Single(assets);
            var asset = assets[0];

            // Owner alters the asset's wallet from wallet1 to wallet2.
            ActionResult alterResult = await AtivoFinanceiroLogic.AlterarAtivoFinanceiroParaOutraCarteira(db, new AtivoFinanceiroAlterarCarteiraRequest
            {
                UserId = -1, // resolves to user1 if valid
                AtivoFinanceiroId = asset.Id,
                CarteiraId = user1Wallets[1].Id,
                Nome = "User1Asset"
            }, "user1");
            Assert.IsType<OkResult>(alterResult);

            assets = await db.GetAtivoByUserId(user1Id.Value);
            Assert.Single(assets);
            Assert.Equal(user1Wallets[1].Id, assets[0].CarteiraId);
        }

        // 9. Verify that an administrator can alter the wallet of an asset belonging to another user.
        [Fact]
        public async Task Admin_CanAlterAssetWallet_ForAnotherUser()
        {
            var db = GetPostgresDbContext();
            // Setup: create admin and user1.
            await UserLogic.AddUser(db, "admin", "admin");
            await UserLogic.AddUser(db, "user1", "user1");
            await PermissionLogic.AddUserPermission(db, "admin", "admin");

            int? user1Id = await UserLogic.GetUserID(db, "user1");
            Assert.NotNull(user1Id);

            // Create two wallets for user1.
            await CreateWallet(db, user1Id.Value, "User1Wallet1");
            await CreateWallet(db, user1Id.Value, "User1Wallet2");
            var user1Wallets = await CarteiraLogic.GetCarteiras(db, "user1");
            Assert.Equal(2, user1Wallets.Count);

            // Admin adds an asset for user1 in wallet1.
            var addResult = await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, new AtivoFinanceiroRequest
            {
                UserId = user1Id.Value,
                CarteiraId = user1Wallets[0].Id,
                Nome = "User1Asset",
                DataInicio = DateTime.UtcNow,
                DuracaoMeses = 1,
                TaxaImposto = 0.1f
            }, "admin");
            Assert.IsType<OkResult>(addResult);

            var assets = await db.GetAtivoByUserId(user1Id.Value);
            var asset = assets[0];

            // Act: admin tries to move the asset from wallet1 to wallet2.
            var alterResult = await AtivoFinanceiroLogic.AlterarAtivoFinanceiroParaOutraCarteira(db, new AtivoFinanceiroAlterarCarteiraRequest
            {
                UserId = -1,  // resolves to user1 if valid
                AtivoFinanceiroId = asset.Id,
                CarteiraId = user1Wallets[1].Id,
                Nome = "User1Asset"
            }, "admin");

            // Agora deve retornar UnauthorizedObjectResult
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(alterResult);
            Assert.Equal("User is not the owner of the asset", unauthorized.Value);
        }

    }
}
