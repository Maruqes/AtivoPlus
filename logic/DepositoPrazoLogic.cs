using System.Threading.Tasks;
using AtivoPlus.Data;
using AtivoPlus.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using AtivoPlus.Models;

namespace AtivoPlus.Logic
{
    public class DepositoPrazoLogic
    {
        public static async Task<ActionResult> AdicionarDepositoPrazo(AppDbContext db, DepositoPrazoRequest depositoRequest, string username)
        {
            // Validate related asset and bank
            var ativoFinanceiro = await db.GetAtivoFinanceiroById(depositoRequest.AtivoFinaceiroId);
            if (ativoFinanceiro == null)
            {
                return new BadRequestObjectResult("Ativo financeiro não encontrado");
            }
            var banco = await db.GetBancoById(depositoRequest.BancoId);
            if (banco == null)
            {
                return new BadRequestObjectResult("Banco não encontrado");
            }

            // -1 indicates use the owner's userId
            if (depositoRequest.UserId == -1)
            {
                int? userId = await UserLogic.GetUserID(db, username);
                if (userId == null)
                {
                    return new UnauthorizedObjectResult("User not found");
                }
                if (ativoFinanceiro.UserId != userId.Value)
                {
                    return new UnauthorizedObjectResult("User is not the owner of the asset, trying to do something fishy?");
                }
                await db.CreateDepositoPrazo(
                    depositoRequest.AtivoFinaceiroId,
                    depositoRequest.BancoId,
                    userId.Value,
                    depositoRequest.NumeroConta,
                    depositoRequest.TaxaJuroAnual,
                    depositoRequest.ValorAtual,
                    depositoRequest.ValorInvestido,
                    depositoRequest.ValorAnualDespesasEstimadas,
                    depositoRequest.DataCriacao);
            }
            else
            {
                if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
                {
                    return new UnauthorizedObjectResult("User is not an admin");
                }
                if (!await UserLogic.CheckIfUserExistsById(db, depositoRequest.UserId))
                {
                    return new BadRequestObjectResult("User not found");
                }
                if (ativoFinanceiro.UserId != depositoRequest.UserId)
                {
                    return new UnauthorizedObjectResult("User is not the owner of the asset, trying to do something fishy?");
                }
                if (!await UserLogic.CheckIfUserExistsById(db, depositoRequest.UserId))
                {
                    return new BadRequestObjectResult("User not found");
                }
                await db.CreateDepositoPrazo(
                    depositoRequest.AtivoFinaceiroId,
                    depositoRequest.BancoId,
                    depositoRequest.UserId,
                    depositoRequest.NumeroConta,
                    depositoRequest.TaxaJuroAnual,
                    depositoRequest.ValorAtual,
                    depositoRequest.ValorInvestido,
                    depositoRequest.ValorAnualDespesasEstimadas,
                    depositoRequest.DataCriacao);
            }
            return new OkResult();
        }

        public static async Task<ActionResult> RemoverDepositoPrazo(AppDbContext db, int depositoPrazoId, string username)
        {
            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }
            int? depositoUserId = await db.GetUserIdFromDepositoPrazo(depositoPrazoId);
            if (depositoUserId == null)
            {
                return new NotFoundObjectResult("Deposito not found");
            }
            bool isAdmin = await PermissionLogic.CheckPermission(db, username, new[] { "admin" });
            if (depositoUserId != userId && !isAdmin)
            {
                return new UnauthorizedObjectResult("User is not the owner of the asset, trying to do something fishy?");
            }
            await db.DeleteDepositoPrazo(depositoPrazoId);
            return new OkResult();
        }

        public static async Task<ActionResult<List<DepositoPrazo>>> GetAllDepositoPrazos(AppDbContext db, string username)
        {
            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }
            List<DepositoPrazo> depositos = await db.GetDepositoPrazosByUserId(userId.Value);
            if (depositos == null)
            {
                return new NotFoundObjectResult("No deposits found");
            }
            return new OkObjectResult(depositos);
        }

        public static async Task<ActionResult<LucroReturn>> GetLucroById(AppDbContext db, int depositoPrazoId, string username)
        {
            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }
            DepositoPrazo? deposito = await db.GetDepositoPrazoById(depositoPrazoId);
            if (deposito == null)
            {
                return new NotFoundObjectResult("Deposito not found");
            }

            AtivoFinanceiro? ativoFinanceiro = await db.GetAtivoFinanceiroById(deposito.AtivoFinaceiroId);
            if (ativoFinanceiro == null)
            {
                return new NotFoundObjectResult("Ativo financeiro not found");
            }
            if (ativoFinanceiro.UserId != userId)
            {
                return new UnauthorizedObjectResult("User is not the owner of the asset, trying to do something fishy?");
            }

            DateTime dataCriacao = deposito.DataCriacao;
            DateTime dataAtual = DateTime.UtcNow;
            int monthsElapsed = ((dataAtual.Year - dataCriacao.Year) * 12) + dataAtual.Month - dataCriacao.Month;
            decimal taxaMensal = (decimal)deposito.TaxaJuroAnual / 100m / 12m;

            decimal fator = 1m;
            for (int i = 0; i < monthsElapsed; i++)
            {
                fator *= (1 + taxaMensal);
            }

            // Validate to prevent division by zero
            if (deposito.ValorInvestido <= 0)
            {
                return new BadRequestObjectResult("Invalid investment value for profit calculation");
            }

            decimal valorLucro = deposito.ValorInvestido * (fator - 1);

            LucroReturn lucroReturn = new LucroReturn
            {
                Base = deposito.ValorInvestido,
                Lucro = valorLucro,
                Total = deposito.ValorInvestido + valorLucro,
                PercentagemLucro = deposito.ValorInvestido > 0 ? (valorLucro / deposito.ValorInvestido) * 100 : 0,
                Despesas = (deposito.ValorAnualDespesasEstimadas / 12) * monthsElapsed
            };
            return new OkObjectResult(lucroReturn);
        }
    }
}