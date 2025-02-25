using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;

//ou app a usar api ou da par por razor em cima desta merda

namespace AtivoPlus.Controllers
{
    [Route("api/user")] // A API está definida em "api/user"
    [ApiController] // Indica que este é um Controller de API
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
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

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

    }
}
