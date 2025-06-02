using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Controllers;

namespace AtivoPlus.Logic
{
    public class ImovelArrendadoLogic
    {

        // -1 indicates use the owner's userId

        public static async Task<ActionResult> AdicionarImovelArrendado(AppDbContext db, ImovelArrendadoRequest request, string username)
        {
            // Validate related entities
            var ativo = await db.GetAtivoFinanceiroById(request.AtivoFinaceiroId);
            if (ativo == null)
            {
                return new BadRequestObjectResult("Ativo financeiro não encontrado");
            }

            // Determine user ID context
            if (request.UserId == -1)
            {
                int? userId = await UserLogic.GetUserID(db, username);
                if (userId == null)
                {
                    return new UnauthorizedObjectResult("User not found");
                }
                if (ativo.UserId != userId.Value)
                {
                    return new UnauthorizedObjectResult("User is not the owner of the asset");
                }

                await db.CreateImovelArrendado(
                    request.AtivoFinaceiroId,
                    request.Morada,
                    request.Designacao,
                    request.Localizacao,
                    request.ValorImovel,
                    request.ValorRenda,
                    request.ValorMensalCondominio,
                    request.ValorAnualDespesasEstimadas,
                    request.DataCriacao);
            }
            else
            {
                if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
                {
                    return new UnauthorizedObjectResult("User is not an admin");
                }
                if (!await UserLogic.CheckIfUserExistsById(db, request.UserId))
                {
                    return new BadRequestObjectResult("User not found");
                }
                if (ativo.UserId != request.UserId)
                {
                    return new UnauthorizedObjectResult("User is not the owner of the asset");
                }

                await db.CreateImovelArrendado(
                    request.AtivoFinaceiroId,
                    request.Morada,
                    request.Designacao,
                    request.Localizacao,
                    request.ValorImovel,
                    request.ValorRenda,
                    request.ValorMensalCondominio,
                    request.ValorAnualDespesasEstimadas,
                    request.DataCriacao);
            }
            return new OkResult();
        }

        public static async Task<ActionResult> RemoverImovelArrendado(AppDbContext db, int imovelArrendadoId, string username)
        {
            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }
            int? imovelUserId = await db.GetUserIdFromImovelArrendado(imovelArrendadoId);
            if (imovelUserId == null)
            {
                return new NotFoundObjectResult("Imóvel arrendado não encontrado");
            }
            bool isAdmin = await PermissionLogic.CheckPermission(db, username, new[] { "admin" });
            if (imovelUserId.Value != userId.Value && !isAdmin)
            {
                return new UnauthorizedObjectResult("User is not the owner of the asset");
            }
            await db.DeleteImovelArrendado(imovelArrendadoId);
            return new OkResult();
        }

        public static async Task<ActionResult<List<ImovelArrendado>>> GetAllImovelArrendados(AppDbContext db, string username)
        {
            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }
            var imoveis = await db.GetImovelArrendadosByUserId(userId.Value);
            if (imoveis == null || imoveis.Count == 0)
            {
                return new NotFoundObjectResult("No real estate entries found");
            }
            return new OkObjectResult(imoveis);
        }

        /// <summary>
        /// Atualiza um imóvel arrendado.
        /// </summary>
        public static async Task<ActionResult> AtualizarImovelArrendado(AppDbContext db, ImovelArrendadoUpdateRequest request, string username)
        {
            // Verify user
            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }
            // Retrieve imovel
            var imovel = await db.GetImovelArrendadoById(request.ImovelArrendadoId);
            if (imovel == null)
            {
                return new NotFoundObjectResult("Imóvel arrendado não encontrado");
            }
            // Permission check
            bool isAdmin = await PermissionLogic.CheckPermission(db, username, new[] { "admin" });
            int? ownerId = await db.GetUserIdFromImovelArrendado(request.ImovelArrendadoId);
            if (ownerId != userId.Value && !isAdmin)
            {
                return new UnauthorizedObjectResult("User is not the owner of the asset");
            }
            // Perform update
            bool updated = await db.UpdateImovelArrendado(
                request.ImovelArrendadoId,
                request.Morada,
                request.Designacao,
                request.Localizacao,
                request.ValorImovel,
                request.ValorRenda,
                request.ValorMensalCondominio,
                request.ValorAnualDespesasEstimadas);
            if (!updated)
            {
                return new BadRequestObjectResult("Failed to update imóvel arrendado");
            }
            return new OkResult();
        }

        public static async Task<ActionResult<LucroReturn>> GetLucroById(AppDbContext db, int imovelArrendadoId, string username)
        {
            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }

            ImovelArrendado? imovel = await db.GetImovelArrendadoById(imovelArrendadoId);
            if (imovel == null)
            {
                return new NotFoundObjectResult("Imóvel arrendado not found");
            }

            AtivoFinanceiro? ativoFinanceiro = await db.GetAtivoFinanceiroById(imovel.AtivoFinaceiroId);
            if (ativoFinanceiro == null)
            {
                return new NotFoundObjectResult("Ativo financeiro not found");
            }
            if (ativoFinanceiro.UserId != userId)
            {
                return new UnauthorizedObjectResult("User is not the owner of the asset, trying to do something fishy?");
            }

            // Calculate lucro
            DateTime dataAtual = DateTime.UtcNow;
            DateTime dataCriacao = imovel.DataCriacao;
            decimal baseLucro = imovel.ValorRenda * ((decimal)(dataAtual - dataCriacao).TotalDays / 30);

            // Validate to prevent division by zero
            if (imovel.ValorImovel <= 0)
            {
                return new BadRequestObjectResult("Invalid property value for profit calculation");
            }

            LucroReturn lucroReturn = new LucroReturn
            {
                Base = imovel.ValorImovel,
                Lucro = baseLucro,
                Total = imovel.ValorImovel + baseLucro,
                PercentagemLucro = imovel.ValorImovel > 0 ? (baseLucro / imovel.ValorImovel) * 100 : 0,
                Despesas = (imovel.ValorAnualDespesasEstimadas / 12) * ((decimal)(dataAtual - dataCriacao).TotalDays / 30)
            };
            return new OkObjectResult(lucroReturn);
        }


    }
}
