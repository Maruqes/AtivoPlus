using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;

//ou app a usar api ou da par por razor em cima desta merda

namespace AtivoPlus.Controllers
{

    /// <summary>
    /// Representa os dados necessários para adicionar um ativo financeiro à carteira de um utilizador.
    /// </summary>
    public class AtivoFinanceiroRequest
    {
        /// <summary>
        /// ID do utilizador a quem o ativo será atribuído.
        /// Utiliza -1 para indicar o utilizador atualmente autenticado.  
        /// Qualquer outro ID só pode ser usado por administradores.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// ID da carteira onde o ativo será adicionado.
        /// </summary>
        public int CarteiraId { get; set; }

        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Data de início do investimento.
        /// </summary>
        public DateTime DataInicio { get; set; }

        /// <summary>
        /// Duração do investimento em meses.
        /// </summary>
        public int DuracaoMeses { get; set; }

        /// <summary>
        /// Taxa de imposto associada ao ativo (em percentagem, ex: 0.28).
        /// </summary>
        public float TaxaImposto { get; set; }
    }


    public class AtivoFinanceiroAlterarCarteiraRequest
    {
        /// <summary>
        /// Utiliza -1 para indicar o utilizador atualmente autenticado.  
        /// Qualquer outro ID só pode ser usado por administradores.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// ID do ativo financeiro a ser alterado.
        /// </summary>
        public int AtivoFinanceiroId { get; set; }
        /// <summary>
        /// ID da nova carteira onde o ativo será movido.
        /// </summary>
        public int CarteiraId { get; set; }

        public string Nome { get; set; } = string.Empty;

    }


    [Route("api/ativofinanceiro")] // A API está definida em "api/user"
    [ApiController] // Indica que este é um Controller de API
    public class AtivoFinanceiroController : ControllerBase
    {
        private readonly AppDbContext db;

        public AtivoFinanceiroController(AppDbContext context)
        {
            db = context;
        }

        /// <summary>
        /// Adiciona um ativo financeiro à carteira do utilizador.
        /// </summary>
        /// <param name="ativoFinanceiro">Dados do ativo financeiro a adicionar.</param>
        /// <returns>Resultado da operação.</returns>
        [HttpPut("adicionar")]
        public async Task<ActionResult> AdicionarAtivoFinanceiro([FromBody] AtivoFinanceiroRequest ativoFinanceiro)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await AtivoFinanceiroLogic.AdicionarAtivoFinanceiro(db, ativoFinanceiro, username);
        }

        [HttpPost("alterarCarteira")]
        public async Task<ActionResult> AlterarAtivoFinanceiroParaOutraCarteira([FromBody] AtivoFinanceiroAlterarCarteiraRequest ativoFinanceiro)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await AtivoFinanceiroLogic.AlterarAtivoFinanceiroParaOutraCarteira(db, ativoFinanceiro, username);
        }

        /// <summary>
        /// Obtém os ativos financeiros associados a um utilizador.
        /// /// </summary>
        /// <param name="userIdFromAtivo">ID do utilizador a partir do qual os ativos serão obtidos. Se -1, obtém os ativos do utilizador autenticado.</param>
        /// <returns>Lista de ativos financeiros associados ao utilizador.</returns>
        /// <remarks>
        /// Se userIdFromAtivo for  -1, obtém os ativos do utilizador autenticado.
        /// Se userIdFromAtivo for diferente de -1, verifica se o utilizador autenticado tem permissão de admin.
        /// Se não tiver permissão, retorna Unauthorized.
        /// Se tiver permissão, obtém os ativos do utilizador especificado.
        /// </remarks>
        [HttpGet("ver")]
        public async Task<ActionResult<List<AtivoFinanceiro>>> VerAtivos(int? userIdFromAtivo)
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

            if (userIdFromAtivo == null || userIdFromAtivo == -1)
            {
                userIdFromAtivo = userId;

            }
            else
            {
                if (await PermissionLogic.CheckPermission(db, username, new[] { "admin" }) == false)
                {
                    return Unauthorized();
                }
            }
            return Ok(await db.GetAtivoByUserId(userIdFromAtivo.Value));
        }

        [HttpDelete("remover")]
        public async Task<ActionResult> RemoverAtivoFinanceiro(int ativoFinanceiroId)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await AtivoFinanceiroLogic.RemoveAtivo(db, ativoFinanceiroId, username);
        }

    }
}