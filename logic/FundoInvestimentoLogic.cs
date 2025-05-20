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

            AtivoFinanceiro? ativoFinanceiro = await db.GetAtivoFinanceiroById(fundoInvestimento.AtivoFinaceiroId);
            if (ativoFinanceiro == null)
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

                if (ativoFinanceiro.UserId != userId.Value)
                {
                    return new UnauthorizedObjectResult("User is not the owner of the asset, trying to do something fishy?");
                }

                await db.CreateFundoInvestimento(ativoFinanceiro.Id, fundoInvestimento.Nome, fundoInvestimento.MontanteInvestido, fundoInvestimento.AtivoSigla, fundoInvestimento.DataCriacao);
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

                if (ativoFinanceiro.UserId != fundoInvestimento.UserId)
                {
                    return new UnauthorizedObjectResult("User is not the owner of the asset, trying to do something fishy?");
                }

                await db.CreateFundoInvestimento(ativoFinanceiro.Id, fundoInvestimento.Nome, fundoInvestimento.MontanteInvestido, fundoInvestimento.AtivoSigla, fundoInvestimento.DataCriacao);
            }
            return new OkResult();
        }

        public static async Task<ActionResult> RemoverFundoInvestimento(AppDbContext db, int fundoInvestimentoID, string username)
        {
            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }
            int? fundoUserID = await db.GetUserIdFromFundoInvestimento(fundoInvestimentoID);
            if (fundoUserID == null)
            {
                return new NotFoundObjectResult("Ativo not found");
            }

            bool adminUser = await PermissionLogic.CheckPermission(db, username, new[] { "admin" });

            if (fundoUserID != userId && !adminUser)
            {
                return new UnauthorizedObjectResult("User is not the owner of the asset, trying to do something fishy?");
            }
            await db.DeleteFundoInvestimento(fundoInvestimentoID);
            return new OkResult();
        }

        public static async Task<ActionResult<List<FundoInvestimento>>> GetAllFundoInvestimentos(AppDbContext db, string username)
        {
            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }
            List<FundoInvestimento> fundos = await db.GetFundoInvestimentosByUserId(userId.Value);
            if (fundos == null)
            {
                return new NotFoundObjectResult("No funds found");
            }
            return new OkObjectResult(fundos);
        }
    }
}