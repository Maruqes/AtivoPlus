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

            ExtraLogic.SetCookie(Response, "username", request.Username);
            ExtraLogic.SetCookie(Response, "token", userToken);

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

            ExtraLogic.SetCookie(Response, "username", string.Empty);
            ExtraLogic.SetCookie(Response, "token", string.Empty);

            return Ok();
        }
    }
}
