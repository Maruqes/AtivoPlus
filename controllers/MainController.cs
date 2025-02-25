using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;

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

    }
}
