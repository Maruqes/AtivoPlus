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
[Fact]
public async Task AddFundoInvestimento()
{
var db = GetPostgresDbContext();
await UserLogic.AddUser(db, "admin", "admin");
await PermissionLogic.AddUserPermission(db, "admin", "admin");


        var adminId = (await UserLogic.GetUserID(db, "admin")).Value;
        var ativo = new AtivoFinanceiro { UserId = adminId };
        await db.AtivoFinanceiros.AddAsync(ativo);
        await db.SaveChangesAsync();

        var request = new FundoInvestimentoRequest
        {
            UserId = -1,
            AtivoFinaceiroId = ativo.Id,
            TipoAtivoId = 1,
            Nome = "FundoTeste",
            MontanteInvestido = 1000m,
            TaxaJuro = 0.05f,
            TaxaFixa = true,
            AtivoSigla = "FT",
            TaxaImposto = 0.15f
        };

        var result = await FundoInvestimentoLogic.AdicionarFundoInvestimento("admin", request, db);
        Assert.IsType<OkObjectResult>(result);

        var fundoEntity = await db.fundoInvestimentos.FirstOrDefaultAsync(fi => fi.AtivoFinaceiroId == ativo.Id);
        Assert.NotNull(fundoEntity);
        Assert.Equal("FundoTeste", fundoEntity!.Nome);
        Assert.Equal(1000m, fundoEntity.MontanteInvestido);
    }

    [Fact]
    public async Task AddFundoInvestimento_AccessControl()
    {
        var db = GetPostgresDbContext();
        await UserLogic.AddUser(db, "admin", "admin");
        await UserLogic.AddUser(db, "t1", "t1");
        await PermissionLogic.AddUserPermission(db, "admin", "admin");

        var adminId = (await UserLogic.GetUserID(db, "admin")).Value;
        var ativo = new AtivoFinanceiro { UserId = adminId };
        await db.AtivoFinanceiros.AddAsync(ativo);
        await db.SaveChangesAsync();

        var request = new FundoInvestimentoRequest
        {
            UserId = adminId,
            AtivoFinaceiroId = ativo.Id,
            TipoAtivoId = 1,
            Nome = "FundoTeste",
            MontanteInvestido = 500m,
            TaxaJuro = 0.03f,
            TaxaFixa = false,
            AtivoSigla = "FT",
            TaxaImposto = 0.10f
        };

        var result = await FundoInvestimentoLogic.AdicionarFundoInvestimento("t1", request, db);
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("User is not an admin", unauthorized.Value);
    }

    [Fact]
    public async Task UpdateFundoInvestimento_AccessControl()
    {
        var db = GetPostgresDbContext();
        await UserLogic.AddUser(db, "admin", "admin");
        await PermissionLogic.AddUserPermission(db, "admin", "admin");

        var adminId = (await UserLogic.GetUserID(db, "admin")).Value;
        var ativo = new AtivoFinanceiro { UserId = adminId };
        await db.AtivoFinanceiros.AddAsync(ativo);
        await db.SaveChangesAsync();

        var addReq = new FundoInvestimentoRequest
        {
            UserId = -1,
            AtivoFinaceiroId = ativo.Id,
            TipoAtivoId = 1,
            Nome = "FundoOriginal",
            MontanteInvestido = 800m,
            TaxaJuro = 0.04f,
            TaxaFixa = true,
            AtivoSigla = "FO",
            TaxaImposto = 0.12f
        };
        await FundoInvestimentoLogic.AdicionarFundoInvestimento("admin", addReq, db);

        var fundoEntity = await db.fundoInvestimentos.FirstOrDefaultAsync(fi => fi.AtivoFinaceiroId == ativo.Id);
        Assert.NotNull(fundoEntity);

        var editReq = new FundoInvestimentoRequestEdit
        {
            UserId = -1,
            FundoInvestimentoID = fundoEntity!.Id,
            TipoAtivoId = 2,
            Nome = "FundoEditado",
            MontanteInvestido = 1200m,
            TaxaJuro = 0.06f,
            TaxaFixa = false,
            AtivoSigla = "FE",
            TaxaImposto = 0.18f
        };

        var ok = await FundoInvestimentoLogic.EditarFundoInvestimento("admin", editReq, db);
        var okResult = Assert.IsType<OkObjectResult>(ok);
        var updatedEntity = Assert.IsType<FundoInvestimento>(okResult.Value);
        Assert.Equal("FundoEditado", updatedEntity.Nome);
        Assert.Equal(1200m, updatedEntity.MontanteInvestido);

        // Tentativa de edição por usuário não autorizado
        var unauthorized = await FundoInvestimentoLogic.EditarFundoInvestimento("t1", editReq, db);
        Assert.IsType<UnauthorizedObjectResult>(unauthorized);
    }

[Fact]
public async Task DeleteFundoInvestimento_AccessControl()
{
    var db = GetPostgresDbContext();
    await UserLogic.AddUser(db, "admin", "admin");
    await PermissionLogic.AddUserPermission(db, "admin", "admin");

    var adminId = (await UserLogic.GetUserID(db, "admin")).Value;
    var ativo = new AtivoFinanceiro { UserId = adminId };
    await db.AtivoFinanceiros.AddAsync(ativo);
    await db.SaveChangesAsync();

    // Adiciona o fundo
    var addReq = new FundoInvestimentoRequest
    {
        UserId = -1,
        AtivoFinaceiroId = ativo.Id,
        TipoAtivoId = 1,
        Nome = "FundoParaExcluir",
        MontanteInvestido = 600m,
        TaxaJuro = 0.05f,
        TaxaFixa = true,
        AtivoSigla = "FD",
        TaxaImposto = 0.11f
    };
    await FundoInvestimentoLogic.AdicionarFundoInvestimento("admin", addReq, db);

    var fundoEntity = await db.fundoInvestimentos.FirstOrDefaultAsync(fi => fi.AtivoFinaceiroId == ativo.Id);
    Assert.NotNull(fundoEntity);

    // Admin consegue apagar
    var ok = await FundoInvestimentoLogic.DeleteFundoInvestimento("admin", fundoEntity!.Id, db);
    Assert.IsType<OkObjectResult>(ok);

    // Tentativa de apagar de novo por não-admin deve dar NotFound
    var notFoundOnSecondDelete = await FundoInvestimentoLogic.DeleteFundoInvestimento("t1", fundoEntity.Id, db);
    var notFoundResult = Assert.IsType<NotFoundObjectResult>(notFoundOnSecondDelete);
    Assert.Equal("Fundo de investimento não encontrado", notFoundResult.Value);
}


    [Fact]
    public async Task DeleteFundoInvestimento_NonExistent()
    {
        var db = GetPostgresDbContext();
        await UserLogic.AddUser(db, "admin", "admin");
        await PermissionLogic.AddUserPermission(db, "admin", "admin");

        var result = await FundoInvestimentoLogic.DeleteFundoInvestimento("admin", 9999, db);
        Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Fundo de investimento não encontrado", ((NotFoundObjectResult)result).Value);
    }

    [Fact]
    public async Task UpdateFundoInvestimento_NonExistent()
    {
        var db = GetPostgresDbContext();
        await UserLogic.AddUser(db, "admin", "admin");
        await PermissionLogic.AddUserPermission(db, "admin", "admin");

        var editReq = new FundoInvestimentoRequestEdit
        {
            UserId = -1,
            FundoInvestimentoID = 9999,
            TipoAtivoId = 1,
            Nome = "X",
            MontanteInvestido = 100m,
            TaxaJuro = 0.01f,
            TaxaFixa = true,
            AtivoSigla = "X",
            TaxaImposto = 0.05f
        };

        var notFound = await FundoInvestimentoLogic.EditarFundoInvestimento("admin", editReq, db);
        Assert.IsType<NotFoundObjectResult>(notFound);
        Assert.Equal("Fundo de investimento não encontrado", ((NotFoundObjectResult)notFound).Value);
    }
}

}
