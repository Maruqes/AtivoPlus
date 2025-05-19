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
        public int UserId { get; set; }
        public int AtivoFinaceiroId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal MontanteInvestido { get; set; }
        public string AtivoSigla { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }


    [Route("api/fundoinvestimento")] // A API está definida em "api/user"
    [ApiController] // Indica que este é um Controller de API
    public class FundoInvestimentoController : ControllerBase
    {
        private readonly AppDbContext db;

        public FundoInvestimentoController(AppDbContext context)
        {
            db = context;
        }


        [HttpPost("adicionar")]
        public async Task<ActionResult> AdicionarFundoInvestimento([FromBody] FundoInvestimentoRequest fundoInvestimento)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await FundoInvestimentoLogic.AdicionarFundoInvestimento(db, fundoInvestimento, username);
        }
    }
}