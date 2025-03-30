using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;

//ou app a usar api ou da par por razor em cima desta merda

namespace AtivoPlus.Controllers
{  
    public class BancoRequest
    {   
        public string Nome { get ; set; } = string.Empty;
    }

    public class BancoRequestChangeName
    {   
        public int bancoId { get; set; }
        public string Nome { get ; set; } = string.Empty;
    }

   
    [Route("api/banco")] // A API está definida em "api/user"
    [ApiController] // Indica que este é um Controller de API
    public class BancoController : ControllerBase
    {
        private readonly AppDbContext db;

        public BancoController(AppDbContext context)
        {
            db = context;
        }

        [HttpPut("adicionar")]
        public async Task<ActionResult> AdicionarBanco([FromBody] BancoRequest banco)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            
            return await BancoLogic.AdicionarBanco(db, banco, username);
        }


         [HttpPost("alterarBanco")]
        public async Task<ActionResult> AlterarBanco([FromBody] BancoRequestChangeName banco)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await BancoLogic.AlterarBanco(db, banco, username);
        }

        [HttpDelete("apagar")]
        public async Task<ActionResult> ApagarBanco([FromBody] int bancoId)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            return await BancoLogic.ApagarBanco(db, bancoId, username);
        }


        [HttpGet("ver")]
        public async Task<ActionResult<List<Banco>>> VerBancos()
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return Ok(await BancoLogic.GetBancos(db));
        }
       
    }
}