using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;



namespace AtivoPlus.Controllers
{
    public class CarteiraRequest
    {
        public int UserId { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    public class CarteiraAlterarNomeRequest
    {
        public int CarteiraId { get; set; }
        public string Nome { get; set; } = string.Empty;
    }


    [Route("api/carteira")]
    [ApiController]
    public class CarteiraController : ControllerBase
    {
        private readonly AppDbContext db;

        public CarteiraController(AppDbContext context)
        {
            db = context;
        }


        /*
            1. criar carteira
            2. ver carteira
            3. alterar nome carteira
            4. apagar carteira

            1234.-> o proprio utilizador, admin 
        */



        [HttpPut("adicionar")]
        public async Task<ActionResult> AdicionarCarteira([FromBody] CarteiraRequest carteira)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            return await CarteiraLogic.AdicionarCarteira(db, carteira, username);
        }



        [HttpPost("alterarNome")]
        public async Task<ActionResult> AletrarNomecarteira([FromBody] CarteiraAlterarNomeRequest carteira)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            return await CarteiraLogic.AtualizarNomeCarteira(db, carteira, username);
        }



        [HttpDelete("apagar")]
        public async Task<ActionResult> ApagarCarteira(int carteiraId)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            return await CarteiraLogic.ApagarCarteira(db, carteiraId, username);
        }

        [HttpGet("ver")]
        public async Task<ActionResult<List<Carteira>>> VerCarteira(int? userIdFromCarteira)
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

            if (userIdFromCarteira == null || userIdFromCarteira == -1)
            {
                userIdFromCarteira = userId;

            }
            else
            {
                if (await PermissionLogic.CheckPermission(db, username, new[] { "admin" }) == false)
                {
                    return Unauthorized();
                }
            }


            return Ok(await db.GetCarteirasByUserId(userIdFromCarteira.Value));
        }

    }
}
