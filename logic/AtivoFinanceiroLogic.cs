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
            // First check if the ativo financeiro exists
            var ativoExistente = await db.GetAtivoFinanceiroById(ativoFinanceiro.AtivoFinanceiroId);
            if (ativoExistente == null)
            {
                return new NotFoundObjectResult("Ativo Financeiro not found");
            }

            if (ativoFinanceiro.UserId == -1)
            {
                int? userId = await UserLogic.GetUserID(db, username);
                if (userId == null)
                {
                    return new UnauthorizedObjectResult("User not found");
                }

                // Verify that the user owns the ativo financeiro
                if (ativoExistente.UserId != userId)
                {
                    return new UnauthorizedObjectResult("User is not the owner of the asset");
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
            // Validate the Nome field
            if (string.IsNullOrWhiteSpace(ativoFinanceiro.Nome))
            {
                return new BadRequestObjectResult("Asset name cannot be empty");
            }

            // -1 indicates use the owner's userId
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

            // Check if the ativo is linked to any ImovelArrendado or DepositoPrazo or FundoInvestimento
            var imovelArrendado = await db.GetImovelArrendadoByAtivoFinanceiroId(ativoId);
            var depositoPrazo = await db.GetDepositoPrazoByAtivoFinanceiroId(ativoId);
            var fundoInvestimento = await db.GetFundoInvestimentoByAtivoFinanceiroId(ativoId);

            if (imovelArrendado.Count > 0 || depositoPrazo.Count > 0 || fundoInvestimento.Count > 0)
            {
                return new BadRequestObjectResult("Cannot delete ativo financeiro linked to other entities");
            }

            await db.RemoveAtivoFinanceiro(ativoId);
            return new OkResult();
        }

        public static async Task<ActionResult> TransferirAtivos(AppDbContext db, AtivoFinanceiroTransferRequest transferRequest, string username)
        {
            // Verify the destination carteira exists
            var carteira = await db.GetCarteiraById(transferRequest.NovaCarteiraId);
            if (carteira == null)
            {
                return new NotFoundObjectResult("Destination wallet not found");
            }

            int? userId;
            if (transferRequest.UserId == -1)
            {
                userId = await UserLogic.GetUserID(db, username);
                if (userId == null)
                {
                    return new UnauthorizedObjectResult("User not found");
                }
            }
            else
            {
                // Only admins can specify a user ID
                if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
                {
                    return new UnauthorizedObjectResult("User is not an admin");
                }
                userId = transferRequest.UserId;
            }

            // Check if the user owns the destination carteira
            if (!await CheckCarteiraOwner(db, transferRequest.NovaCarteiraId, userId.Value))
            {
                return new UnauthorizedObjectResult("User is not the owner of the destination wallet");
            }

            var results = new List<object>();
            foreach (int ativoId in transferRequest.AtivoFinanceiroIds)
            {
                // Get the ativo
                var ativo = await db.GetAtivoFinanceiroById(ativoId);
                if (ativo == null)
                {
                    results.Add(new { ativoId, success = false, message = "Ativo not found" });
                    continue;
                }

                // Verify ownership of the ativo
                if (ativo.UserId != userId && !await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
                {
                    results.Add(new { ativoId, success = false, message = "User is not the owner of the asset" });
                    continue;
                }

                // Move the ativo to the new carteira
                ativo.CarteiraId = transferRequest.NovaCarteiraId;
                results.Add(new { ativoId, success = true, message = "Successfully transferred" });
            }

            await db.SaveChangesAsync();
            return new OkObjectResult(new { results });
        }
    }
}