using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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


    // Modelo de requisição para Imóvel Arrendado
    public class ImovelArrendadoRequest
    {
        // -1 indica uso do usuário autenticado, outro valor requer permissão de admin
        public int UserId { get; set; }
        public int AtivoFinaceiroId { get; set; }
        public string Morada { get; set; } = string.Empty;
        public string Designacao { get; set; } = string.Empty;
        public string Localizacao { get; set; } = string.Empty;
        public decimal ValorImovel { get; set; }
        public decimal ValorRenda { get; set; }
        public decimal ValorMensalCondominio { get; set; }
        public decimal ValorAnualDespesasEstimadas { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
    // Modelo de requisição para atualizar Imóvel Arrendado
    public class ImovelArrendadoUpdateRequest
    {
        public int ImovelArrendadoId { get; set; }
        public string? Morada { get; set; } = string.Empty;
        public string? Designacao { get; set; }
        public string? Localizacao { get; set; }
        public decimal? ValorImovel { get; set; }
        public decimal? ValorRenda { get; set; }
        public decimal? ValorMensalCondominio { get; set; }
        public decimal? ValorAnualDespesasEstimadas { get; set; }
    }
    [Route("api/imovelarrendado")]
    [ApiController]
    public class ImovelArrendadoController : ControllerBase
    {
        private readonly AppDbContext db;

        public ImovelArrendadoController(AppDbContext context)
        {
            db = context;
        }
        /// <summary>
        /// Adiciona um imóvel arrendado.
        /// </summary>
        [HttpPost("adicionar")]
        public async Task<ActionResult> AdicionarImovelArrendado([FromBody] ImovelArrendadoRequest request)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await ImovelArrendadoLogic.AdicionarImovelArrendado(db, request, username);
        }

        /// <summary>
        /// Remove um imóvel arrendado.
        /// </summary>
        [HttpDelete("remover")]
        public async Task<ActionResult> RemoverImovelArrendado(int imovelArrendadoId)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await ImovelArrendadoLogic.RemoverImovelArrendado(db, imovelArrendadoId, username);
        }

        /// <summary>
        /// Obtém todos os imóveis arrendados do usuário.
        /// </summary>
        [HttpGet("getAllByUser")]
        public async Task<ActionResult<List<ImovelArrendado>>> GetAllImovelArrendados()
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await ImovelArrendadoLogic.GetAllImovelArrendados(db, username);
        }

        /// <summary>
        /// Atualiza um imóvel arrendado.
        /// </summary>
        [HttpPut("atualizar")]
        public async Task<ActionResult> AtualizarImovelArrendado([FromBody] ImovelArrendadoUpdateRequest request)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await ImovelArrendadoLogic.AtualizarImovelArrendado(db, request, username);
        }

        [HttpGet("getLucroById")]
        public async Task<ActionResult<LucroReturn>> GetLucroById(int imovelArrendadoId)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await ImovelArrendadoLogic.GetLucroById(db, imovelArrendadoId, username);
        }
    }
}