using System.Threading.Tasks;
using AtivoPlus.Data;
using AtivoPlus.Controllers;
using Microsoft.AspNetCore.Mvc;

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
            }
            return new OkResult();
        }
    }
}