using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;

namespace AtivoPlus.Controllers
{
    [Route("api/user")] // A API está definida em "api/user"
    [ApiController] // Indica que este é um Controller de API
    public class UserController : ControllerBase
    {
        private readonly AppDbContext db;

        public UserController(AppDbContext context)
        {
            db = context;
        }


        public class UserRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPut("adicionar")]
        public async Task<ActionResult<User>> AdicionarUser([FromBody] UserRequest request)
        {
            if (await UserLogic.AddUser(db, request.Username, request.Password) == false)
            {
                return BadRequest();
            }

            return Ok();
        }


        // [HttpGet("getTodos")]
        // public async Task<ActionResult<List<User>>> GetTodos()
        // {
        //     return await db.GetUsersByRawSqlAsync();
        // }

        [HttpPost("logar")]
        public async Task<ActionResult<User>> LogarUser([FromBody] UserRequest request)
        {
            string userToken = await UserLogic.LogarUser(db, request.Username, request.Password);

            if (userToken == string.Empty)
            {
                return BadRequest();
            }

            ExtraLogic.SetCookie(HttpContext, "username", request.Username);
            ExtraLogic.SetCookie(HttpContext, "token", userToken);

            return Ok();
        }

        [HttpGet("logout")]
        public ActionResult<User> LogoutUser()
        {
            string Username = UserLogic.CheckUserLoggedRequest(Request);
            if (Username == string.Empty)
            {
                return Unauthorized();
            }

            UserLogic.LogoutUser(Username, UserLogic.GetTokenWithRequest(Request));

            ExtraLogic.SetCookie(HttpContext, "username", string.Empty);
            ExtraLogic.SetCookie(HttpContext, "token", string.Empty);

            return Ok();
        }

        [HttpPost("changepassword/{username}")]
        public async Task<ActionResult<string>> ChangePasswordAsync(string username)
        {
            string returnUrl = await UserLogic.RequestPassUpdate(db, username);
            return Ok(returnUrl);
        }


        public class UpdatePasswordRequest
        {
            public string Token { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
        }

        [HttpPost("resetpassword")]
        public async Task<ActionResult<bool>> ResetPassword([FromBody] UpdatePasswordRequest req)
        {

            return Ok(await UserLogic.UpdatePassword(db, req.Token, req.NewPassword));
        }


    }
}
