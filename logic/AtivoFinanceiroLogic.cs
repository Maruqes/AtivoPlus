using System.Threading.Tasks;
using AtivoPlus.Data;
using AtivoPlus.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace AtivoPlus.Logic
{
    public class AtivoFinanceiroLogic
    {

        public static async Task<bool> CheckCarteiraOwner(AppDbContext db, int carteiraId, int userId)
        {
            int? carteiraOwnerId = await db.GetUserIdFromCarteira(carteiraId);
            return carteiraOwnerId == userId;
        }

        public static async Task<ActionResult> AdicionarAtivoFinanceiro(AppDbContext db, AtivoFinanceiroRequest ativoFinanceiro, string username)
        {
        

            // -1 indicates use the owner’s userId
            if (ativoFinanceiro.UserId == -1)
            {
                int? userId = await UserLogic.GetUserID(db, username);
                if (userId == null)
                {
                    return new UnauthorizedResult();
                }

                if(!CheckCarteiraOwner(db, ativoFinanceiro.CarteiraId, userId.Value).Result)
                {
                    return new UnauthorizedResult();
                }

                await db.CreateAtivoFinanceiro(userId.Value, ativoFinanceiro.EntidadeAtivoId, ativoFinanceiro.CarteiraId, ativoFinanceiro.DataInicio, ativoFinanceiro.DuracaoMeses, ativoFinanceiro.TaxaImposto);
            }
            else
            {
                if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
                {
                    return new UnauthorizedResult();
                }
                if(!CheckCarteiraOwner(db, ativoFinanceiro.CarteiraId, ativoFinanceiro.UserId).Result)
                {
                    return new UnauthorizedResult();
                }
                await db.CreateAtivoFinanceiro(ativoFinanceiro.UserId, ativoFinanceiro.EntidadeAtivoId, ativoFinanceiro.CarteiraId, ativoFinanceiro.DataInicio, ativoFinanceiro.DuracaoMeses, ativoFinanceiro.TaxaImposto);


            }
            return new OkResult();
        }
    }
}