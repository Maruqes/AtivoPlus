using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;

//ou app a usar api ou da par por razor em cima desta merda

namespace AtivoPlus.Controllers
{
    [Route("/")] // A API está definida em "api/produto"
    [ApiController] // Indica que este é um Controller de API
    public class MainController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MainController(AppDbContext context)
        {
            _context = context;
        }



        [HttpGet]
        public IActionResult ServeFile()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "test/index.html");
            return PhysicalFile(filePath, "text/html");
        }

        [HttpGet("secret")]
        public IActionResult ServeFile2()
        {
            string cookieUsername = ExtraLogic.GetCookie(Request, "username");
            if (cookieUsername == null)
            {
                return BadRequest();
            }

            var cookieToken = ExtraLogic.GetCookie(Request, "token");
            if (cookieToken == null)
            {
                return BadRequest();
            }

            if (!UserLogic.CheckUserLogged(cookieUsername, cookieToken))
            {
                return BadRequest();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "test/index2.html");
            return PhysicalFile(filePath, "text/html");
        }

    }
}
