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
                    return new UnauthorizedObjectResult("User not found");
                }
                await db.CreateCarteira(userId.Value, carteira.Nome);
            }
            else
            {
                if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
                {
                    return new UnauthorizedObjectResult("User is not an admin");
                }
                if (!await UserLogic.CheckIfUserExistsById(db, carteira.UserId))
                {
                    return new BadRequestObjectResult("User not found");
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
                return new NotFoundObjectResult("Carteira not found");
            }

            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }

            if (userId.Value != userIdFromCarteira && !await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
            {
                return new UnauthorizedObjectResult("User is not an admin");
            }

            await db.UpdateCarteiraNome(carteira.CarteiraId, carteira.Nome);
            return new OkResult();
        }

        public static async Task<List<AtivoFinanceiro>> CanCarteiraBeDeleted(AppDbContext db, int carteiraId)
        {
            List<AtivoFinanceiro> ativos = await db.GetAtivosByCarteiraId(carteiraId);
            return ativos;
        }
        public static async Task<ActionResult> ApagarCarteira(AppDbContext db, int carteiraId, string username)
        {
            int? userIdFromCarteira = await db.GetUserIdFromCarteira(carteiraId);
            if (userIdFromCarteira == null)
            {
                return new NotFoundObjectResult("Carteira not found");
            }

            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }

            if (userId.Value != userIdFromCarteira && !await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
            {
                return new UnauthorizedObjectResult("User is not the owner of this wallet or an admin");
            }

            // Check if there are any ativos associated with this carteira
            List<AtivoFinanceiro> ativos = await CanCarteiraBeDeleted(db, carteiraId);
            if (ativos.Count > 0)
            {
                // Provide more detailed error message including assets that need to be moved
                return new BadRequestObjectResult(new
                {
                    message = "Carteira cannot be deleted because it contains financial assets that need to be moved or deleted first",
                    ativos = ativos.Select(a => new { id = a.Id, nome = a.Nome }).ToList()
                });
            }

            await db.DeleteCarteira(carteiraId);
            return new OkResult();
        }

        public static async Task<ActionResult> ApagarCarteiraComConfirmacao(AppDbContext db, DeleteCarteiraConfirmationRequest request, string username)
        {
            int carteiraId = request.CarteiraId;

            int? userIdFromCarteira = await db.GetUserIdFromCarteira(carteiraId);
            if (userIdFromCarteira == null)
            {
                return new NotFoundObjectResult("Carteira not found");
            }

            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }

            bool isAdmin = await PermissionLogic.CheckPermission(db, username, new[] { "admin" });
            if (userId.Value != userIdFromCarteira && !isAdmin)
            {
                return new UnauthorizedObjectResult("User is not the owner of this wallet or an admin");
            }

            // Check if there are any ativos associated with this carteira
            List<AtivoFinanceiro> ativos = await CanCarteiraBeDeleted(db, carteiraId);

            // If there are ativos and ForceDelete is false, return information about what assets will be deleted
            if (ativos.Count > 0 && !request.ForceDelete)
            {
                return new OkObjectResult(new
                {
                    confirmationRequired = true,
                    message = "This wallet contains assets that need to be handled before deletion",
                    ativos = ativos.Select(a => new { id = a.Id, nome = a.Nome }).ToList()
                });
            }

            // If ForceDelete and MoveAtivosToCarteiraId are specified, move the assets
            if (request.ForceDelete && request.MoveAtivosToCarteiraId.HasValue)
            {
                // Check if the destination carteira exists and belongs to the same user
                var destCarteira = await db.GetCarteiraById(request.MoveAtivosToCarteiraId.Value);
                if (destCarteira == null)
                {
                    return new NotFoundObjectResult("Destination wallet not found");
                }

                if (destCarteira.UserId != userId && !isAdmin)
                {
                    return new UnauthorizedObjectResult("User is not the owner of the destination wallet");
                }

                // Move all assets to the new carteira
                foreach (var ativo in ativos)
                {
                    ativo.CarteiraId = request.MoveAtivosToCarteiraId.Value;
                }

                await db.SaveChangesAsync();
            }
            else if (ativos.Count > 0)
            {
                // User wants to force delete without moving the assets
                return new BadRequestObjectResult(new
                {
                    message = "Cannot delete a wallet with assets without specifying where to move them"
                });
            }

            // Delete the carteira
            await db.DeleteCarteira(carteiraId);
            return new OkObjectResult(new
            {
                success = true,
                message = "Wallet successfully deleted"
            });
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