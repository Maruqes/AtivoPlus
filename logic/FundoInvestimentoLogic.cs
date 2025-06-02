using System.Threading.Tasks;
using AtivoPlus.Data;
using AtivoPlus.Controllers;
using Microsoft.AspNetCore.Mvc;
using AtivoPlus.Models;

namespace AtivoPlus.Logic
{
    public class FundoInvestimentoLogic
    {

        public static async Task<ActionResult> AdicionarFundoInvestimento(AppDbContext db, FundoInvestimentoRequest fundoInvestimento, string username)
        {

            AtivoFinanceiro? ativoFinanceiro = await db.GetAtivoFinanceiroById(fundoInvestimento.AtivoFinaceiroId);
            if (ativoFinanceiro == null)
            {
                return new BadRequestObjectResult("Ativo financeiro não encontrado");
            }

            if (TwelveDataLogic.DoesSymbolExists(fundoInvestimento.AtivoSigla) == false)
            {
                return new BadRequestObjectResult("Ativo financeiro não encontrado");
            }


            // -1 indicates use the owner’s userId
            if (fundoInvestimento.UserId == -1)
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

                await db.CreateFundoInvestimento(ativoFinanceiro.Id, fundoInvestimento.Nome, fundoInvestimento.MontanteInvestido, fundoInvestimento.AtivoSigla, fundoInvestimento.DataCriacao);
            }
            else
            {
                if (!await PermissionLogic.CheckPermission(db, username, new[] { "admin" }))
                {
                    return new UnauthorizedObjectResult("User is not an admin");
                }
                if (!await UserLogic.CheckIfUserExistsById(db, fundoInvestimento.UserId))
                {
                    return new BadRequestObjectResult("User not found");
                }

                if (ativoFinanceiro.UserId != fundoInvestimento.UserId)
                {
                    return new UnauthorizedObjectResult("User is not the owner of the asset, trying to do something fishy?");
                }

                await db.CreateFundoInvestimento(ativoFinanceiro.Id, fundoInvestimento.Nome, fundoInvestimento.MontanteInvestido, fundoInvestimento.AtivoSigla, fundoInvestimento.DataCriacao);
            }
            return new OkResult();
        }

        public static async Task<ActionResult> RemoverFundoInvestimento(AppDbContext db, int fundoInvestimentoID, string username)
        {
            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }
            int? fundoUserID = await db.GetUserIdFromFundoInvestimento(fundoInvestimentoID);
            if (fundoUserID == null)
            {
                return new NotFoundObjectResult("Ativo not found");
            }

            bool adminUser = await PermissionLogic.CheckPermission(db, username, new[] { "admin" });

            if (fundoUserID != userId && !adminUser)
            {
                return new UnauthorizedObjectResult("User is not the owner of the asset, trying to do something fishy?");
            }
            await db.DeleteFundoInvestimento(fundoInvestimentoID);
            return new OkResult();
        }

        public static async Task<ActionResult<List<FundoInvestimento>>> GetAllFundoInvestimentos(AppDbContext db, string username)
        {
            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }
            List<FundoInvestimento> fundos = await db.GetFundoInvestimentosByUserId(userId.Value);
            if (fundos == null)
            {
                return new NotFoundObjectResult("No funds found");
            }
            return new OkObjectResult(fundos);
        }

        public static async Task<ActionResult<LucroReturn>> GetLucroById(AppDbContext db, int fundoInvestimento, string username)
        {
            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }

            FundoInvestimento? fundo = await db.GetFundoInvestimentoById(fundoInvestimento);
            if (fundo == null)
            {
                return new NotFoundObjectResult("Fundo investimento not found");
            }

            AtivoFinanceiro? ativoFinanceiro = await db.GetAtivoFinanceiroById(fundo.AtivoFinaceiroId);
            if (ativoFinanceiro == null)
            {
                return new NotFoundObjectResult("Ativo financeiro not found");
            }
            if (ativoFinanceiro.UserId != userId)
            {
                return new UnauthorizedObjectResult("User is not the owner of the asset, trying to do something fishy?");
            }

            List<Candle>? today_candle = await TwelveDataLogic.GetCandles(fundo.AtivoSigla, db, "1day", DateTime.UtcNow.AddDays(-1));
            if (today_candle == null || today_candle.Count == 0)
            {
                return new NotFoundObjectResult("Candle not found for today");
            }

            List<Candle>? bought_candle = await TwelveDataLogic.GetCandles(fundo.AtivoSigla, db, "1day", fundo.DataCriacao);
            if (bought_candle == null || bought_candle.Count == 0)
            {
                return new NotFoundObjectResult("Candle not found for the date of purchase");
            }
            decimal valorCompra = bought_candle[0].Close;
            decimal valorVenda = today_candle[0].Close;

            // Validate to prevent division by zero
            if (valorCompra <= 0)
            {
                return new BadRequestObjectResult("Invalid purchase price (zero or negative)");
            }
            if (fundo.MontanteInvestido <= 0)
            {
                return new BadRequestObjectResult("Invalid investment amount for profit calculation");
            }

            decimal lucro = (valorVenda - valorCompra) * fundo.MontanteInvestido / valorCompra;

            LucroReturn lucroReturn = new LucroReturn
            {
                Base = fundo.MontanteInvestido,
                Lucro = lucro,
                Total = fundo.MontanteInvestido + lucro,
                PercentagemLucro = fundo.MontanteInvestido > 0 ? (lucro / fundo.MontanteInvestido) * 100 : 0,
                Despesas = 0
            };
            return new OkObjectResult(lucroReturn);
        }
    }
}