using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;

//ou app a usar api ou da par por razor em cima desta merda

namespace AtivoPlus.Controllers
{

    public class AtivoFinanceiroRequest
    {
        public int UserId { get; set; }
        public int EntidadeAtivoId { get; set; }
        public int CarteiraId { get; set; }
        public DateTime DataInicio { get; set; }
        public int DuracaoMeses { get; set; }
        public float TaxaImposto { get; set; }
    }

    public class AtivoFinanceiroAlterarCarteiraRequest
    {
        public int UserId { get; set; }
        public int AtivoFinanceiroId { get; set; }
        public int CarteiraId { get; set; }
    }

    [Route("api/ativofinanceiro")] // A API está definida em "api/user"
    [ApiController] // Indica que este é um Controller de API
    public class AtivoFinanceiroController : ControllerBase
    {
        private readonly AppDbContext db;

        public AtivoFinanceiroController(AppDbContext context)
        {
            db = context;
        }

        [HttpPut("adicionar")]
        public async Task<ActionResult> AdicionarAtivoFinanceiro([FromBody] AtivoFinanceiroRequest ativoFinanceiro)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, ativoFinanceiro, username);
        }

        [HttpPost("alterarCarteira")]
        public async Task<ActionResult> AlterarAtivoFinanceiroParaOutraCarteira([FromBody] AtivoFinanceiroAlterarCarteiraRequest ativoFinanceiro)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await AtivoFinanceiroLogic.AlterarAtivoFinanceiroParaOutraCarteira(db, ativoFinanceiro, username);
        }

        [HttpGet("ver")]
        public async Task<ActionResult<List<AtivoFinanceiro>>> VerAtivos(int? userIdFromAtivo)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            int? userId = await UserLogic.GetUserID(db, username);
            if (userId == null)
            {
                return Unauthorized();
            }

            if (userIdFromAtivo == null || userIdFromAtivo == -1)
            {
                userIdFromAtivo = userId;

            }
            else
            {
                if (await PermissionLogic.CheckPermission(db, username, new[] { "admin" }) == false)
                {
                    return Unauthorized();
                }
            }
            return Ok(await db.GetAtivoByUserId(userIdFromAtivo.Value));
        }

        [HttpDelete("remover")]
        public async Task<ActionResult> RemoverAtivoFinanceiro(int ativoFinanceiroId)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await AtivoFinanceiroLogic.RemoveAtivo(db, ativoFinanceiroId, username);
        }

    }
}