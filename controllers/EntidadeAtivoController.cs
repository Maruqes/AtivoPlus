using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;



namespace AtivoPlus.Controllers
{

    public class EntidadeAtivoRequest
    {
        public int UserId { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    [Route("api/entidadeativo")]
    [ApiController]
    public class EntidadeAtivoController : ControllerBase
    {
        private readonly AppDbContext db;

        public EntidadeAtivoController(AppDbContext context)
        {
            db = context;
        }

        [HttpPut("adicionar")]
        public async Task<ActionResult> AdicionarEntidadeAtivo([FromBody] EntidadeAtivoRequest carteira)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            return await EntidadeAtivoLogic.AdicionarEntidadeAtivo(db, carteira, username);
        }

        [HttpDelete("apagar")]
        public async Task<ActionResult> ApagarEntidadeAtivo(int entidadeId)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            return await EntidadeAtivoLogic.ApagarEntidadeAtivo(db, entidadeId, username);
        }

        [HttpGet("ver")]
        public async Task<ActionResult<List<Carteira>>> VerCarteira(int? userIdFromEntidade)
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

            if (userIdFromEntidade == null || userIdFromEntidade == -1)
            {
                userIdFromEntidade = userId;

            }
            else
            {
                if (userId != userIdFromEntidade && await PermissionLogic.CheckPermission(db, username, new[] { "admin" }) == false)
                {
                    return Unauthorized();
                }
            }


            return Ok(await db.GetEntidadeAtivoByUserId(userIdFromEntidade.Value));
        }


    }
}
