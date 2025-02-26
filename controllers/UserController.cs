using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;

//ou app a usar api ou da par por razor em cima desta merda

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

        [HttpPost("adicionar")]
        public async Task<ActionResult<User>> AdicionarUser([FromBody] UserRequest request)
        {
            UserLogic.AddUser(db, request.Username, request.Password);

            return Ok();
        }


        [HttpGet("getTodos")]
        public async Task<ActionResult<List<User>>> GetTodos()
        {
            return await db.GetUsersByRawSqlAsync();
        }
    }
}
