using System.Threading.Tasks;
using AtivoPlus.Data;
using AtivoPlus.Controllers;
using Microsoft.AspNetCore.Mvc;
using AtivoPlus.Models;

namespace AtivoPlus.Logic
{
    public class BancoLogic
    {


        public static async Task<ActionResult> AdicionarBanco(AppDbContext db, BancoRequest banco, string username)
        {
            if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
            {
                return new UnauthorizedObjectResult("User is not an admin");
            }

            if (await db.BancoNameExists(banco.Nome))
            {
                return new BadRequestObjectResult("Banco já existe");
            }

            await db.CreateBanco(banco.Nome);
            return new OkResult();
        }


        public static async Task<ActionResult> AlterarBanco(AppDbContext db, BancoRequestChangeName banco, string username)
        {
            if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
            {
                return new UnauthorizedObjectResult("User is not an admin");
            }

            var bancoDB = await db.GetBancoById(banco.bancoId);
            if (bancoDB == null)
            {
                return new NotFoundObjectResult("Banco não encontrado");
            }

            if (bancoDB.Nome != banco.Nome && await db.BancoNameExists(banco.Nome))
            {
                return new BadRequestObjectResult("Banco com esse nome já existe");
            }

            await db.UpdateBanco(banco.bancoId, banco.Nome);
            return new OkResult();
        }


        public static async Task<ActionResult> ApagarBanco(AppDbContext db, int bancoId, string username)
        {
            if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
            {
                return new UnauthorizedObjectResult("User is not an admin");
            }

            var banco = await db.GetBancoById(bancoId);
            if (banco == null)
            {
                return new NotFoundObjectResult("Banco não encontrado");
            }

            await db.DeleteBanco(bancoId);
            return new OkResult();
        }

        public static async Task<List<Banco>?> GetBancos(AppDbContext db)
        {
            return await db.GetAllBancos();
        }

    }
}