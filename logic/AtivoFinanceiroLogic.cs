using System.Threading.Tasks;
using AtivoPlus.Data;
using AtivoPlus.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace AtivoPlus.Logic
{
    public class AtivoFinanceiroLogic
    {

        public static async Task<bool> CheckCarteiraOwner(AppDbContext db, int carteiraId, int userId)
        {
            int? carteiraOwnerId = await db.GetUserIdFromCarteira(carteiraId);
            return carteiraOwnerId == userId;
        }

        public static async Task<ActionResult> AlterarAtivoFinanceiroParaOutraCarteira(AppDbContext db, AtivoFinanceiroAlterarCarteiraRequest ativoFinanceiro, string username)
        {
            if (ativoFinanceiro.UserId == -1)
            {
                int? userId = await UserLogic.GetUserID(db, username);
                if (userId == null)
                {
                    return new UnauthorizedObjectResult("User not found");
                }
                if (!await CheckCarteiraOwner(db, ativoFinanceiro.CarteiraId, userId.Value))
                {
                    return new UnauthorizedObjectResult("User is not the owner of the wallet, trying to do something fishy?");
                }
                await db.ChangeAtivoFinanceiroCarteira(ativoFinanceiro.AtivoFinanceiroId, ativoFinanceiro.CarteiraId);

                return new OkResult();
            }
            else
            {
                if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
                {
                    return new UnauthorizedObjectResult("User is not an admin");
                }
                if (!CheckCarteiraOwner(db, ativoFinanceiro.CarteiraId, ativoFinanceiro.UserId).Result)
                {
                    return new UnauthorizedObjectResult("User is not the owner of the wallet, trying to do something fishy?");
                }
                await db.ChangeAtivoFinanceiroCarteira(ativoFinanceiro.AtivoFinanceiroId, ativoFinanceiro.CarteiraId);
            }
            return new OkResult();
        }



        public static async Task<ActionResult> AdicionarAtivoFinanceiro(AppDbContext db, AtivoFinanceiroRequest ativoFinanceiro, string username)
        {


            // -1 indicates use the owner’s userId
            if (ativoFinanceiro.UserId == -1)
            {
                int? userId = await UserLogic.GetUserID(db, username);
                if (userId == null)
                {
                    return new UnauthorizedObjectResult("User not found");
                }

                if (!CheckCarteiraOwner(db, ativoFinanceiro.CarteiraId, userId.Value).Result)
                {
                    return new UnauthorizedObjectResult("User is not the owner of the wallet, trying to do something fishy?");
                }

                await db.CreateAtivoFinanceiro(userId.Value, ativoFinanceiro.Nome, ativoFinanceiro.CarteiraId, ativoFinanceiro.DataInicio, ativoFinanceiro.DuracaoMeses, ativoFinanceiro.TaxaImposto);
            }
            else
            {
                if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
                {
                    return new UnauthorizedObjectResult("User is not an admin");
                }
                if (!CheckCarteiraOwner(db, ativoFinanceiro.CarteiraId, ativoFinanceiro.UserId).Result)
                {
                    return new UnauthorizedObjectResult("User is not the owner of the wallet, trying to do something fishy?");
                }
                await db.CreateAtivoFinanceiro(ativoFinanceiro.UserId, ativoFinanceiro.Nome, ativoFinanceiro.CarteiraId, ativoFinanceiro.DataInicio, ativoFinanceiro.DuracaoMeses, ativoFinanceiro.TaxaImposto);


            }
            return new OkResult();
        }

        public static async Task<ActionResult> RemoveAtivo(AppDbContext db, int ativoId, string username)
        {

            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }
            int? ativoUserId = await db.GetUserIdFromAtivoFinanceiro(ativoId);
            if (ativoUserId == null)
            {
                return new NotFoundObjectResult("Ativo not found");
            }

            bool adminUser = await PermissionLogic.CheckPermission(db, username, new[] { "admin" });

            if (ativoUserId != userId && !adminUser)
            {
                return new UnauthorizedObjectResult("User is not the owner of the asset, trying to do something fishy?");
            }
            await db.RemoveAtivoFinanceiro(ativoId);
            return new OkResult();
        }
    }
}