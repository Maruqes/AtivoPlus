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


        /*
            [HttpGet]  [HttpGet("{id}")]
            [HttpPost]
            [HttpPut]
            [HttpDelete]

            /api/user/adionar/user
        */

        [HttpPost("adicionar")]
        public async Task<ActionResult<User>> AdicionarUser([FromBody] User userE)
        {
            var user = new User
            {
                Nome = userE.Nome,
                Idade = userE.Idade
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return Ok(user);
        }


        [HttpGet("getTodos")]
        public async Task<ActionResult<List<User>>> GetTodos()
        {
            UserLogic.teste();
            return await db.Users.ToListAsync();
        }
    }
}
