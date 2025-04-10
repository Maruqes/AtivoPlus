using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;



namespace AtivoPlus.Controllers
{

    public class EntidadeAtivoRequest
    {
        /// <summary>
        /// Utiliza -1 para indicar o utilizador atualmente autenticado.  
        /// Qualquer outro ID só pode ser usado por administradores.
        /// </summary>
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


        /// <summary>
        /// Apagar uma entidade ativo.
        /// </summary>
        /// <param name="entidadeId">ID da entidade ativo a apagar.</param>
        /// <returns>Resultado da operação.</returns>
        /// <remarks>
        /// Esta operação só pode ser realizada pelo utilizador que criou a entidade ativo ou por um administrador.
        /// </remarks>
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

        /// <summary>
        /// Ver todos os ativos financeiros de um utilizador.
        /// </summary>
        /// <param name="userIdFromEntidade">Utiliza -1 para indicar o utilizador atualmente autenticado. Qualquer outro ID só pode ser usado por administradores.</param>
        /// <remarks>
        /// Esta operação só pode ser realizada pelo utilizador que criou a entidade ativo ou por um administrador.
        /// </remarks>
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
                if (await PermissionLogic.CheckPermission(db, username, new[] { "admin" }) == false)
                {
                    return Unauthorized();
                }
            }


            return Ok(await db.GetEntidadeAtivoByUserId(userIdFromEntidade.Value));
        }


    }
}
