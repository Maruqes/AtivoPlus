using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;

//ou app a usar api ou da par por razor em cima desta merda

namespace AtivoPlus.Controllers
{
    public class MoradaRequest
    {
        public string Rua { get; set; } = string.Empty;
        public string Piso { get; set; } = string.Empty;
        public string NumeroPorta { get; set; } = string.Empty;
        public string Concelho { get; set; } = string.Empty;
        public string Distrito { get; set; } = string.Empty;
        public string Localidade { get; set; } = string.Empty;
        public string CodPostal { get; set; } = string.Empty;
    }

    public class UserInfoWithMoradaRequest
    {
        public UserInfo? UserInfoRequest { get; set; }
        public MoradaRequest? MoradaRequest { get; set; }
    }


    [Route("api/userinfo")] // A API está definida em "api/user"
    [ApiController] // Indica que este é um Controller de API
    public class UserInfoController : ControllerBase
    {
        private readonly AppDbContext db;

        public UserInfoController(AppDbContext context)
        {
            db = context;
        }

        /*
        o q vamos fzr ->
            // ver info
                1. O proprio utilizador deve poder ver a sua info
                2. O ADMIN PRINCIPAL deve poder ver a info de qualquer utilizador

            // alterar info
                1. O proprio utilizador deve poder alterar a sua info
                2. O ADMIN PRINCIPAL deve poder alterar a info de qualquer utilizador
        */


        //se o id passado for -1, é o proprio utilizador

        //if Id = -1, é o proprio utilizador
        //if Id = "um id", é o admin
        /// <summary>
        /// Utiliza -1 para indicar o utilizador atualmente autenticado.  
        /// Qualquer outro ID só pode ser usado por administradores.
        /// morada_id leave empty, or 0
        /// </summary>
        [HttpPut("setInfo")]
        public async Task<ActionResult> SetInfo([FromBody] UserInfoWithMoradaRequest userInfoWithMoradaRequest)
        {
            // se for o próprio utilizador ou admin pode alterar
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            if (userInfoWithMoradaRequest.UserInfoRequest == null || userInfoWithMoradaRequest.MoradaRequest == null)
            {
                return BadRequest("Invalid request");
            }

            UserInfo userInfoRequest = userInfoWithMoradaRequest.UserInfoRequest!;
            MoradaRequest moradaRequest = userInfoWithMoradaRequest.MoradaRequest!;

            ActionResult result = await UserInfoLogic.SetUserInfo(db, username, userInfoRequest, moradaRequest);

            return result;
        }

        /// <summary>
        /// Utiliza -1 para indicar o utilizador atualmente autenticado.  
        /// Qualquer outro ID só pode ser usado por administradores.
        /// </summary>
        [HttpGet("getInfo")]
        public async Task<ActionResult<UserInfoWithMoradaRequest>> GetInfo([FromQuery] int id = -1)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            UserInfo? userInfo = await UserInfoLogic.GetUserInfo(db, username, id);
            if (userInfo == null)
            {
                return NotFound();
            }

            Morada? morada = await db.GetMoradasByUserId(userInfo.Id);
            if (morada == null)
            {
                return NotFound();
            }

            UserInfoWithMoradaRequest userInfoWithMorada = new UserInfoWithMoradaRequest
            {
                UserInfoRequest = userInfo,
                MoradaRequest = new MoradaRequest
                {
                    Rua = morada.Rua,
                    Piso = morada.Piso,
                    NumeroPorta = morada.NumeroPorta,
                    Concelho = morada.Concelho,
                    Distrito = morada.Distrito,
                    Localidade = morada.Localidade,
                    CodPostal = morada.CodPostal
                }
            };
            return Ok(userInfoWithMorada);
        }
    }
}