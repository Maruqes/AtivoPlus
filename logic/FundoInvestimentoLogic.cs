using AtivoPlus.Models;
using AtivoPlus.Data;
using Microsoft.AspNetCore.Http;
using AtivoPlus.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace AtivoPlus.Logic
{
    class FundoInvestimentoLogic
    {
        public static async Task<ActionResult> AdicionarFundoInvestimento(string username, FundoInvestimentoRequest fundo, AppDbContext db)
        {
            if (fundo == null)
            {
                return new BadRequestObjectResult("Dados do fundo de investimento inválidos");
            }
            int id_to_compare = -1;

            if (fundo.UserId == -1)
            {
                int? userId = await UserLogic.GetUserID(db, username);
                if (userId == null)
                {
                    return new UnauthorizedObjectResult("User not found");
                }
                id_to_compare = userId.Value;
            }
            else
            {
                if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
                {
                    return new UnauthorizedObjectResult("User is not an admin");
                }
                id_to_compare = fundo.UserId;
            }


            AtivoFinanceiro? ativo = await db.GetAtivoFinanceiroById(fundo.AtivoFinaceiroId);
            if (ativo == null)
            {
                return new UnauthorizedObjectResult("Usuário não é o proprietário deste ativo financeiro");
            }
            if (ativo.UserId != id_to_compare)
            {
                return new UnauthorizedObjectResult("User is not the owner of the wallet, trying to do something fishy?");
            }

            await db.CreateFundoInvestimento(fundo.AtivoFinaceiroId, fundo.TipoAtivoId, fundo.Nome, fundo.MontanteInvestido, fundo.TaxaJuro, fundo.TaxaFixa, fundo.AtivoSigla);
            return new OkObjectResult(fundo);
        }

        public static async Task<ActionResult> EditarFundoInvestimento(string username, FundoInvestimentoRequestEdit fundo, AppDbContext db)
        {
            if (fundo == null)
            {
                return new BadRequestObjectResult("Dados do fundo de investimento inválidos");
            }
            int id_to_compare = -1;

            if (fundo.UserId == -1)
            {
                int? userId = await UserLogic.GetUserID(db, username);
                if (userId == null)
                {
                    return new UnauthorizedObjectResult("User not found");
                }
                id_to_compare = userId.Value;
            }
            else
            {
                if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
                {
                    return new UnauthorizedObjectResult("User is not an admin");
                }
                id_to_compare = fundo.UserId;
            }

            FundoInvestimento? fundoInvestimento = await db.GetFundoInvestimento(fundo.FundoInvestimentoID);
            if (fundoInvestimento == null)
            {
                return new NotFoundObjectResult("Fundo de investimento não encontrado");
            }

            await db.UpdateFundoInvestimento(fundo.FundoInvestimentoID, fundoInvestimento.AtivoFinaceiroId, fundo.TipoAtivoId, fundo.Nome, fundo.MontanteInvestido, fundo.TaxaJuro, fundo.TaxaFixa, fundo.AtivoSigla);

            return new OkObjectResult(fundoInvestimento);
        }


        public static async Task<ActionResult> DeleteFundoInvestimento(string username, int ativoFinanceiroId, AppDbContext db)
        {
           
            FundoInvestimento? fundoInvestimento = await db.GetFundoInvestimento(ativoFinanceiroId);
            if (fundoInvestimento == null)
            {
                return new NotFoundObjectResult("Fundo de investimento não encontrado");
            }

            AtivoFinanceiro? ativo = await db.GetAtivoFinanceiroById(fundoInvestimento.AtivoFinaceiroId);
            if (ativo == null)
            {
                return new UnauthorizedObjectResult("Usuário não é o proprietário deste ativo financeiro");
            }

            User? user = await db.GetUserById(ativo.UserId);
            if (user == null)
            {
                return new NotFoundObjectResult("User not found");
            }

            if (user.Username != username)
            {
                if(!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
                {
                    return new UnauthorizedObjectResult("User is not the owner of the wallet, trying to do something fishy?");
                }
            }

            await db.DeleteFundoInvestimento(fundoInvestimento.Id);

            return new OkObjectResult(fundoInvestimento);
        }
    }
}