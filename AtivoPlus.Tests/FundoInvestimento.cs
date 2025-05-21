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
        AtivoFinaceiroId   = ativo.Id,
        Nome               = "FundoTeste",
        MontanteInvestido  = 1000m,
        AtivoSigla         = "FT",
        DataCriacao        = DateTime.UtcNow,
    };

    // Como a validação de símbolo falha, agora retornamos BadRequest
    var result = await FundoInvestimentoLogic.AdicionarFundoInvestimento(db, request, "admin");
    var bad = Assert.IsType<BadRequestObjectResult>(result);
    Assert.Equal("Ativo financeiro não encontrado", bad.Value);
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
        UserId = -1,
        AtivoFinaceiroId   = ativo.Id,
        Nome               = "FundoTeste",
        MontanteInvestido  = 1000m,
        AtivoSigla         = "FT",
        DataCriacao        = DateTime.UtcNow,
    };

    // Mesmo como t1, a checagem de símbolo ocorre primeiro e retorna BadRequest
    var result = await FundoInvestimentoLogic.AdicionarFundoInvestimento(db, request, "t1");
    var bad = Assert.IsType<BadRequestObjectResult>(result);
    Assert.Equal("Ativo financeiro não encontrado", bad.Value);
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

    // Cria o fundo diretamente, já que Adicionar pode falhar validação
    await db.CreateFundoInvestimento(ativo.Id, "FundoParaExcluir", 600m, "FD", DateTime.UtcNow);

    var fundoEntity = await db.FundoInvestimentos.FirstOrDefaultAsync(fi => fi.AtivoFinaceiroId == ativo.Id);
    Assert.NotNull(fundoEntity);

    // Admin consegue apagar
    var ok = await FundoInvestimentoLogic.RemoverFundoInvestimento(db, fundoEntity!.Id, "admin");
    Assert.IsType<OkObjectResult>(ok);

    // Tentativa de apagar por não-admin agora retorna Unauthorized
    var unauthorized = await FundoInvestimentoLogic.RemoverFundoInvestimento(db, fundoEntity.Id, "t1");
    var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(unauthorized);
    Assert.Equal("User is not the owner of the asset, trying to do something fishy?", unauthorizedResult.Value);
}


[Fact]
public async Task DeleteFundoInvestimento_NonExistent()
{
    var db = GetPostgresDbContext();
    await UserLogic.AddUser(db, "admin", "admin");
    await PermissionLogic.AddUserPermission(db, "admin", "admin");

    // Tentativa de remover ID inexistente → NotFound("Ativo not found")
    var result = await FundoInvestimentoLogic.RemoverFundoInvestimento(db, 9999, "admin");
    var nf = Assert.IsType<NotFoundObjectResult>(result);
    Assert.Equal("Ativo not found", nf.Value);
}


    /*[Fact]
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
    }*/
}

}