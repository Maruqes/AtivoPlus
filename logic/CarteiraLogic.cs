using System.Threading.Tasks;
using AtivoPlus.Data;
using AtivoPlus.Controllers;
using Microsoft.AspNetCore.Mvc;
using AtivoPlus.Models;

namespace AtivoPlus.Logic
{
    public class CarteiraLogic
    {
        public static async Task<ActionResult> AdicionarCarteira(AppDbContext db, CarteiraRequest carteira, string username)
        {
            // -1 indicates use the owner’s userId
            if (carteira.UserId == -1)
            {
                int? userId = await UserLogic.GetUserID(db, username);
                if (userId == null)
                {
                    return new UnauthorizedResult();
                }
                await db.CreateCarteira(userId.Value, carteira.Nome);
            }
            else
            {
                if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
                {
                    return new UnauthorizedResult();
                }
                if (!await UserLogic.CheckIfUserExistsById(db, carteira.UserId))
                {
                    return new BadRequestResult();
                }
                await db.CreateCarteira(carteira.UserId, carteira.Nome);
            }
            return new OkResult();
        }

        public static async Task<ActionResult> AtualizarNomeCarteira(AppDbContext db, CarteiraAlterarNomeRequest carteira, string username)
        {
            int? userIdFromCarteira = await db.GetUserIdFromCarteira(carteira.CarteiraId);
            if (userIdFromCarteira == null)
            {
                return new NotFoundResult();
            }

            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedResult();
            }

            if (userId.Value != userIdFromCarteira && !await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
            {
                return new UnauthorizedResult();
            }

            await db.UpdateCarteiraNome(carteira.CarteiraId, carteira.Nome);
            return new OkResult();
        }

        public static async Task<ActionResult> ApagarCarteira(AppDbContext db, int carteiraId, string username)
        {
            int? userIdFromCarteira = await db.GetUserIdFromCarteira(carteiraId);
            if (userIdFromCarteira == null)
            {
                return new NotFoundResult();
            }

            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedResult();
            }

            if (userId.Value != userIdFromCarteira && !await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
            {
                return new UnauthorizedResult();
            }

            await db.DeleteCarteira(carteiraId);
            return new OkResult();
        }

        public static async Task<List<Carteira>?> GetCarteiras(AppDbContext db, string username)
        {
            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return null;
            }

            return await db.GetCarteirasByUserId(userId.Value);
        }
    }
}