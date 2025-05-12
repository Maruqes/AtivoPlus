using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;



namespace AtivoPlus.Controllers
{
    public class CarteiraRequest
    {
        /// <summary>
        /// Utiliza -1 para indicar o utilizador atualmente autenticado.  
        /// Qualquer outro ID só pode ser usado por administradores.
        /// </summary>
        public int UserId { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    public class CarteiraAlterarNomeRequest
    {
        /// <summary>
        /// Identificador da carteira a ser alterada
        /// Se for Admin pode alterar qualquer uma se não só pode alterar uma sua
        /// </summary>
        public int CarteiraId { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    public class DeleteCarteiraConfirmationRequest
    {
        /// <summary>
        /// ID da carteira a ser apagada
        /// </summary>
        public int CarteiraId { get; set; }

        /// <summary>
        /// Indica se o utilizador confirmou a eliminação mesmo com ativos presentes
        /// </summary>
        public bool ForceDelete { get; set; } = false;

        /// <summary>
        /// Se ForceDelete for true e esta propriedade for preenchida, os ativos serão transferidos para a carteira indicada
        /// </summary>
        public int? MoveAtivosToCarteiraId { get; set; }
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


        /// <summary>
        /// Apaga uma carteira.
        /// Id da carteira deve ser do utilizador autenticado ou um admin pode apagar qualquer carteira
        /// </summary>
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

        /// <summary>
        /// Obtém as carteiras associadas a um utilizador.
        /// Se userIdFromCarteira for -1, obtém as carteiras do utilizador autenticado.
        /// Se userIdFromCarteira for diferente de -1, verifica se o utilizador autenticado tem permissão de admin.
        /// Se não tiver permissão, retorna Unauthorized.
        /// Se tiver permissão, obtém as carteiras do utilizador especificado.
        /// </summary>
        /// <param name="userIdFromCarteira">ID do utilizador a partir do qual as carteiras serão obtidas. Se -1, obtém as carteiras do utilizador autenticado.</param>
        /// <returns>Lista de carteiras associadas ao utilizador.</returns>
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

        /// <summary>
        /// Apaga uma carteira com confirmação.
        /// Id da carteira deve ser do utilizador autenticado ou um admin pode apagar qualquer carteira.
        /// </summary>
        [HttpPost("apagarComConfirmacao")]
        public async Task<ActionResult> ApagarCarteiraComConfirmacao([FromBody] DeleteCarteiraConfirmationRequest request)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            return await CarteiraLogic.ApagarCarteiraComConfirmacao(db, request, username);
        }
    }
}
