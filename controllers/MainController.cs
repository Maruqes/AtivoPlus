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
        private readonly AppDbContext db;

        public MainController(AppDbContext context)
        {
            db = context;
        }



        [HttpGet]
        public IActionResult ServeFile()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "test/index.html");
            return PhysicalFile(filePath, "text/html");
        }

        [HttpGet("secret")]
        public async Task<IActionResult> ServeFile2()
        {
            string Username = UserLogic.CheckUserLoggedRequest(Request);
            if (Username == string.Empty)
            {
                return Unauthorized();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "test/index2.html");
            return PhysicalFile(filePath, "text/html");
        }

        [HttpGet("secret2")]
        public async Task<IActionResult> ServeFile3([FromQuery] string permission)
        {
            string Username = UserLogic.CheckUserLoggedRequest(Request);
            if (Username == string.Empty)
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(permission))
            {
                return BadRequest("Permission parameter is missing.");
            }

            if (await PermissionLogic.CheckPermission(db, Username, new[] { permission }) == false)
            {
                return Unauthorized();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "test/index2.html");
            return PhysicalFile(filePath, "text/html");
        }

    }
}
