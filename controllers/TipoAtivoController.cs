using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;

//ou app a usar api ou da par por razor em cima desta merda

namespace AtivoPlus.Controllers
{
    public class TipoAtivoRequest
    {
        public string Nome { get; set; } = string.Empty;
    }

    public class TipoAtivoRequestChangeName
    {
        public int TipoAtivoId { get; set; }
        public string Nome { get; set; } = string.Empty;
    }


    // [Route("api/tipoativo")] // A API está definida em "api/user"
    // [ApiController] // Indica que este é um Controller de API
    // public class TipoAtivoController : ControllerBase
    // {
    // private readonly AppDbContext db;

    // public TipoAtivoController(AppDbContext context)
    // {
    //     db = context;
    // }

    /// <summary>
    /// Adiciona um novo tipo de ativo.
    /// only admin can add
    /// </summary>
    // [HttpPut("adicionarTipoAtivo")]
    // public async Task<ActionResult> AdicionarTipoAtivo([FromBody] TipoAtivoRequest tipoAtivoRequest)
    // {
    //     string username = UserLogic.CheckUserLoggedRequest(Request);
    //     if (string.IsNullOrEmpty(username))
    //     {
    //         return Unauthorized();
    //     }

    //     var tipoAtivoModel = new AtivoPlus.Models.TipoAtivo
    //     {
    //         Nome = tipoAtivoRequest.Nome
    //     };


    //     return await TipoAtivoLogic.AdicionarTipoAtivo(db, tipoAtivoModel, username);
    // }

    // /// <summary>
    // /// Altera o nome de um tipo de ativo.
    // /// only admin can change
    // /// </summary>
    // [HttpPost("alterarTipoAtivo")]
    // public async Task<ActionResult> AlterarTipoAtivo([FromBody] TipoAtivoRequestChangeName tipoAtivo)
    // {
    //     string username = UserLogic.CheckUserLoggedRequest(Request);
    //     if (string.IsNullOrEmpty(username))
    //     {
    //         return Unauthorized();
    //     }
    //     return await TipoAtivoLogic.AlterarTipoAtivo(db, tipoAtivo, username);
    // }

    // /// <summary>
    // /// Apaga um tipo de ativo.
    // /// only admin can delete
    // /// </summary>
    // [HttpDelete("apagar")]
    // public async Task<ActionResult> ApagarTipoAtivo([FromBody] int tipoAtivoId)
    // {
    //     string username = UserLogic.CheckUserLoggedRequest(Request);
    //     if (string.IsNullOrEmpty(username))
    //     {
    //         return Unauthorized();
    //     }

    //     return await TipoAtivoLogic.ApagarTipoAtivo(db, tipoAtivoId, username);
    // }

    // /// <summary>
    // /// Ver todos os tipos de ativo.
    // /// </summary>
    // /// <returns>Lista de tipos de ativo</returns>
    // [HttpGet("ver")]
    // public async Task<ActionResult<List<TipoAtivo>>> VerTiposAtivo()
    // {
    //     string username = UserLogic.CheckUserLoggedRequest(Request);
    //     if (string.IsNullOrEmpty(username))
    //     {
    //         return Unauthorized();
    //     }
    //     return Ok(await TipoAtivoLogic.GetTiposAtivo(db));
    // }

    // }
}