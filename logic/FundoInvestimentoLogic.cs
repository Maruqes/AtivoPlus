using System.Threading.Tasks;
using AtivoPlus.Data;
using AtivoPlus.Controllers;
using Microsoft.AspNetCore.Mvc;
using AtivoPlus.Models;

namespace AtivoPlus.Logic
{
    public class FundoInvestimentoLogic
    {

        public static async Task<ActionResult> AdicionarFundoInvestimento(AppDbContext db, FundoInvestimentoRequest fundoInvestimento, string username)
        {

            if (db.GetAtivoFinanceiroById(fundoInvestimento.AtivoFinaceiroId) == null)
            {
                return new BadRequestObjectResult("Ativo financeiro não encontrado");
            }

            if (TwelveDataLogic.DoesSymbolExists(fundoInvestimento.AtivoSigla) == false)
            {
                return new BadRequestObjectResult("Ativo financeiro não encontrado");
            }


            // -1 indicates use the owner’s userId
            if (fundoInvestimento.UserId == -1)
            {
                int? userId = await UserLogic.GetUserID(db, username);
                if (userId == null)
                {
                    return new UnauthorizedObjectResult("User not found");
                }
                await db.CreateFundoInvestimento(userId.Value, fundoInvestimento.Nome, fundoInvestimento.MontanteInvestido, fundoInvestimento.AtivoSigla);
            }
            else
            {
                if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
                {
                    return new UnauthorizedObjectResult("User is not an admin");
                }
                if (!await UserLogic.CheckIfUserExistsById(db, fundoInvestimento.UserId))
                {
                    return new BadRequestObjectResult("User not found");
                }
                await db.CreateFundoInvestimento(fundoInvestimento.UserId, fundoInvestimento.Nome, fundoInvestimento.MontanteInvestido, fundoInvestimento.AtivoSigla);
            }
            return new OkResult();
        }

    }
}