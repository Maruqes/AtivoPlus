using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;
using Newtonsoft.Json.Linq;

//ou app a usar api ou da par por razor em cima desta merda

namespace AtivoPlus.Controllers
{


    public class DepositoRequest
    {
        public int UserId { get; set; }
        public int AtivoFinaceiroId { get; set; }
        public int TipoAtivoId { get; set; }
        public int BancoId { get; set; }
        public int NumeroConta { get; set; }
        public float TaxaJuroAnual { get; set; }
        public Decimal ValorAtual { get; set; }
        public Decimal ValorInvestido { get; set; }
        public Decimal ValorAnualDespesasEstimadas { get; set; }
    }

    public class DepositoRequestEdit
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AtivoFinaceiroId { get; set; }
        public int TipoAtivoId { get; set; }
        public int BancoId { get; set; }
        public int NumeroConta { get; set; }
        public float TaxaJuroAnual { get; set; }
        public Decimal ValorAtual { get; set; }
        public Decimal ValorInvestido { get; set; }
        public Decimal ValorAnualDespesasEstimadas { get; set; }
    }

    [Route("api/depositoprazo")] // Define a rota base para este controller

    [ApiController] // Indica que este é um Controller de API
    public class DepositoPrazoController : ControllerBase
    {
        private readonly AppDbContext db;

        public DepositoPrazoController(AppDbContext context)
        {
            db = context;
        }

        [HttpPut("adicionar")]
        public async Task<ActionResult> AdicionarDeposito([FromBody] DepositoRequest fundo)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await DepositoPrazoLogic.AdicionarDepositoPrazo(db, fundo, username);
        }

        [HttpGet("editar")]
        public async Task<ActionResult> EditarDeposito([FromBody] DepositoRequestEdit fundo)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await DepositoPrazoLogic.EditarDepositoPrazo(username, fundo, db);
        }


    }
}