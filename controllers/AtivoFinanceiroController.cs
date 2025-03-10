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
        public int EntidadeAtivoId {get; set;}
        public int CarteiraId { get; set; }
        public DateTime DataInicio { get; set; } 
        public int DuracaoMeses { get; set; }
        public float TaxaImposto { get; set; }
    }

    public class AtivoFinanceiroAlterarNomeRequest
    {
        public int AtivoFinanceiroId { get; set; }
        public string Nome { get; set; } = string.Empty;
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

    }
}