using System.Threading.Tasks;
using AtivoPlus.Data;
using AtivoPlus.Controllers;
using Microsoft.AspNetCore.Mvc;
using AtivoPlus.Models;

namespace AtivoPlus.Logic
{
    public class TipoAtivoLogic
    {
        public static async Task<ActionResult> AdicionarTipoAtivo(AppDbContext db, TipoAtivo tipoAtivo, string username)
        {
            if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
            {
                return new UnauthorizedObjectResult("User is not an admin");
            }

            await db.CreateTipoAtivo(tipoAtivo.Nome);
            return new OkResult();
        }


        public static async Task<ActionResult> AlterarTipoAtivo(AppDbContext db, TipoAtivoRequestChangeName tipoAtivo, string username)
        {
              if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
            {
                return new UnauthorizedObjectResult("User is not an admin");
            }

            await db.UpdateTipoAtivo(tipoAtivo.tipoAtivoId, tipoAtivo.Nome);
            return new OkResult();
        }


         public static async Task<ActionResult> ApagarTipoAtivo(AppDbContext db, int tipoAtivoId, string username)
        {
            if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
            {
                return new UnauthorizedObjectResult("User is not an admin");
            }

            await db.DeleteTipoAtivo(tipoAtivoId);
            return new OkResult();
        }

        public static async Task<List<TipoAtivo>?> GetTiposAtivo(AppDbContext db)
        {
            return await db.GetAllTiposAtivo();
        }

    }
}