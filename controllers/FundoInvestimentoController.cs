using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;

//ou app a usar api ou da par por razor em cima desta merda

namespace AtivoPlus.Controllers
{

    public class FundoInvestimentoRequest
    {
        /// <summary>
        /// ID do utilizador a quem o ativo será atribuído.
        /// Utiliza -1 para indicar o utilizador atualmente autenticado.  
        /// Qualquer outro ID só pode ser usado por administradores.
        /// </summary>
        public int UserId { get; set; }
        public int AtivoFinaceiroId { get; set; }
        public int TipoAtivoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal MontanteInvestido { get; set; }
        public float TaxaJuro { get; set; }
        public Boolean TaxaFixa { get; set; }
        public string AtivoSigla { get; set; } = string.Empty;
        public float TaxaImposto { get; set; }
    }

    public class FundoInvestimentoRequestEdit
    {
        public int UserId { get; set; }
        public int FundoInvestimentoID { get; set; }
        public int TipoAtivoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal MontanteInvestido { get; set; }
        public float TaxaJuro { get; set; }
        public Boolean TaxaFixa { get; set; }
        public string AtivoSigla { get; set; } = string.Empty;
        public float TaxaImposto { get; set; }
    }

    [Route("api/fundoinvestimento")]
    [ApiController] // Indica que este é um Controller de API
    public class FundoInvestimentoController : ControllerBase
    {
        private readonly AppDbContext db;

        public FundoInvestimentoController(AppDbContext context)
        {
            db = context;
        }

        [HttpPut("adicionar")]
        public async Task<ActionResult> AdicionarFundoInvestimento([FromBody] FundoInvestimentoRequest fundo)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await FundoInvestimentoLogic.AdicionarFundoInvestimento(username, fundo, db);
        }

        [HttpGet("editar")]
        public async Task<ActionResult> EditarFundoInvestimento([FromBody] FundoInvestimentoRequestEdit fundo)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await FundoInvestimentoLogic.EditarFundoInvestimento(username, fundo, db);
        }

        [HttpGet("get")]
        public async Task<ActionResult> GetFundoInvestimento(int ativoFinanceiroId)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            FundoInvestimento? fundo = await db.GetFundoInvestimento(ativoFinanceiroId);
            if (fundo == null)
            {
                return NotFound();
            }
            return Ok(fundo);
        }

        [HttpGet("getall")]
        public async Task<ActionResult> GetAllFundoInvestimento()
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            List<FundoInvestimento> fundos = await db.fundoInvestimentos.ToListAsync();
            if (fundos == null)
            {
                return NotFound();
            }
            return Ok(fundos);
        }

        [HttpDelete("delete")]
        public async Task<ActionResult> DeleteFundoInvestimento(int ativoFinanceiroId)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
          return await FundoInvestimentoLogic.DeleteFundoInvestimento(username, ativoFinanceiroId, db);
        }
    }
}