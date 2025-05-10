using System.Threading.Tasks;
using AtivoPlus.Data;
using AtivoPlus.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AtivoPlus.Models;

namespace AtivoPlus.Logic
{
    public class DepositoPrazoLogic
    {



        public static async Task<ActionResult> AdicionarDepositoPrazo(AppDbContext db, DepositoRequest depositoPrazo, string username)
        {

            if (depositoPrazo == null)
            {
                return new BadRequestObjectResult("Dados do fundo de investimento inválidos");
            }
            int id_to_compare = -1;

            if (depositoPrazo.UserId == -1)
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
                id_to_compare = depositoPrazo.UserId;
            }



            //check for ativo id and owner
            AtivoFinanceiro? ativo = await db.GetAtivoFinanceiroById(depositoPrazo.AtivoFinaceiroId);
            if (ativo == null)
            {
                return new UnauthorizedObjectResult("Usuário não é o proprietário deste ativo financeiro");
            }
            if (ativo.UserId != id_to_compare)
            {
                return new UnauthorizedObjectResult("User is not the owner of the wallet, trying to do something fishy?");
            }

            //check for banco id
            Banco? banco = await db.GetBancoById(depositoPrazo.BancoId);
            if (banco == null)
            {
                return new UnauthorizedObjectResult("Banco não encontrado");
            }

            await db.CreateDepositoPrazo(
                ativoFinaceiroId: depositoPrazo.AtivoFinaceiroId,
                tipoAtivoId: depositoPrazo.TipoAtivoId,
                bancoId: depositoPrazo.BancoId,
                titularId: id_to_compare,
                numeroConta: depositoPrazo.NumeroConta,
                taxaJuroAnual: depositoPrazo.TaxaJuroAnual,
                valorAtual: depositoPrazo.ValorAtual,
                valorInvestido: depositoPrazo.ValorInvestido,
                valorAnualDespesasEstimadas: depositoPrazo.ValorAnualDespesasEstimadas
            );
            return new OkObjectResult(depositoPrazo);
        }

        public static async Task<ActionResult> EditarDepositoPrazo(string username, DepositoRequestEdit depositoPrazo, AppDbContext db)
        {
            if (depositoPrazo == null)
            {
                return new BadRequestObjectResult("Dados do fundo de investimento inválidos");
            }
            int id_to_compare = -1;

            if (depositoPrazo.UserId == -1)
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
                id_to_compare = depositoPrazo.UserId;
            }

            //check for ativo id and owner
            AtivoFinanceiro? ativo = await db.GetAtivoFinanceiroById(depositoPrazo.AtivoFinaceiroId);
            if (ativo == null)
            {
                return new UnauthorizedObjectResult("Usuário não é o proprietário deste ativo financeiro");
            }
            if (ativo.UserId != id_to_compare)
            {
                return new UnauthorizedObjectResult("User is not the owner of the wallet, trying to do something fishy?");
            }

            //check for banco id
            Banco? banco = await db.GetBancoById(depositoPrazo.BancoId);
            if (banco == null)
            {
                return new UnauthorizedObjectResult("Banco não encontrado");
            }

            bool result = await db.UpdateDepositoPrazo(
                depositoPrazoId: depositoPrazo.Id,
                ativoFinaceiroId: depositoPrazo.AtivoFinaceiroId,
                tipoAtivoId: depositoPrazo.TipoAtivoId,
                bancoId: depositoPrazo.BancoId,
                titularId: id_to_compare,
                numeroConta: depositoPrazo.NumeroConta,
                taxaJuroAnual: depositoPrazo.TaxaJuroAnual,
                valorAtual: depositoPrazo.ValorAtual,
                valorInvestido: depositoPrazo.ValorInvestido,
                valorAnualDespesasEstimadas: depositoPrazo.ValorAnualDespesasEstimadas
            );
            if (!result)
            {
                return new NotFoundObjectResult("Deposito não encontrado");
            }
            return new OkObjectResult(depositoPrazo);
        }
    }
}