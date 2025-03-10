using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;



namespace AtivoPlus.Controllers
{

    public class EntidadeAtivoRequest
    {
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


    }
}
