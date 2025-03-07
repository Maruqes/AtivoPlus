using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;

//ou app a usar api ou da par por razor em cima desta merda

namespace AtivoPlus.Controllers
{
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
        [HttpPut("setInfo")]
        public async Task<ActionResult> SetInfo([FromBody] UserInfo userInfoRequest)
        {
            // se for o próprio utilizador ou admin pode alterar
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            ActionResult result = await UserInfoLogic.SetUserInfo(db, username, userInfoRequest);

            return result;
        }

        [HttpGet("getInfo")]
        public async Task<ActionResult<UserInfo>> GetInfo([FromQuery] int id = -1)
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

            return Ok(userInfo);
        }
    }
}