using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;
using Newtonsoft.Json.Linq;

//ou app a usar api ou da par por razor em cima desta merda

namespace AtivoPlus.Controllers
{

    public class LucroReturn
    {
        public decimal Base { get; set; }
        public decimal Lucro { get; set; }
        public decimal Total { get; set; }
        public decimal PercentagemLucro { get; set; }

        public decimal Despesas { get; set; }
    }


    public class DepositoPrazoRequest
    {
        public int UserId { get; set; }
        public int AtivoFinaceiroId { get; set; }
        public int BancoId { get; set; }
        public int NumeroConta { get; set; }
        public float TaxaJuroAnual { get; set; }
        public decimal ValorAtual { get; set; }
        public decimal ValorInvestido { get; set; }
        public decimal ValorAnualDespesasEstimadas { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }

    [Route("api/depositoprazo")]
    [ApiController] // Indica que este é um Controller de API
    public class DepositoPrazoController : ControllerBase
    {
        private readonly AppDbContext db;

        public DepositoPrazoController(AppDbContext context)
        {
            db = context;
        }

        [HttpPost("adicionar")]
        public async Task<ActionResult> AdicionarDepositoPrazo([FromBody] DepositoPrazoRequest depositoRequest)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await DepositoPrazoLogic.AdicionarDepositoPrazo(db, depositoRequest, username);
        }

        [HttpDelete("remover")]
        public async Task<ActionResult> RemoverDepositoPrazo(int depositoPrazoId)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await DepositoPrazoLogic.RemoverDepositoPrazo(db, depositoPrazoId, username);
        }

        [HttpGet("getAllByUser")]
        public async Task<ActionResult<List<DepositoPrazo>>> GetAllDepositoPrazos()
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await DepositoPrazoLogic.GetAllDepositoPrazos(db, username);
        }

        [HttpGet("getLucroById")]
        public async Task<ActionResult<LucroReturn>> GetLucroById(int depositoPrazoId)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await DepositoPrazoLogic.GetLucroById(db, depositoPrazoId, username);
        }
    }
}