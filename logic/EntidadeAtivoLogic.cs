using System.Threading.Tasks;
using AtivoPlus.Data;
using AtivoPlus.Controllers;
using Microsoft.AspNetCore.Mvc;
using AtivoPlus.Models;

namespace AtivoPlus.Logic
{
    public class EntidadeAtivoLogic
    {
        public static async Task<ActionResult> AdicionarEntidadeAtivo(AppDbContext db, EntidadeAtivoRequest entidade, string username)
        {
            // -1 indicates use the owner’s userId
            if (entidade.UserId == -1)
            {
                int? userId = await UserLogic.GetUserID(db, username);
                if (userId == null)
                {
                    return new UnauthorizedResult();
                }
                await db.AdicionarEntidadeAtivo(entidade.Nome, userId.Value);
            }
            else
            {
                if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
                {
                    return new UnauthorizedResult();
                }
                if (!await UserLogic.CheckIfUserExistsById(db, entidade.UserId))
                {
                    return new BadRequestResult();
                }
                await db.AdicionarEntidadeAtivo(entidade.Nome, entidade.UserId);
            }
            return new OkResult();
        }

        public static async Task<bool> IsEntidadeOwner(AppDbContext db, int entidadeId, int userId)
        {
            EntidadeAtivo? entidade = await db.GetEntidadeAtivo(entidadeId);
            if (entidade == null)
            {
                return false;
            }
            return entidade.UserId == userId;
        }

        public static async Task<ActionResult> ApagarEntidadeAtivo(AppDbContext db, int entidadeId, string username)
        {
            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedResult();
            }

            if (!await IsEntidadeOwner(db, entidadeId, userId.Value) && !await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
            {
                return new UnauthorizedResult();
            }

            if (!await db.DoesEntidadeExist(entidadeId))
            {
                return new NotFoundResult();
            }
            await db.ApagarEntidadeAtivo(entidadeId);
            return new OkResult();
        }
    }
}