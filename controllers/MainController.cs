using Microsoft.AspNetCore.Mvc;
using AtivoPlus.Data;

//ou app a usar api ou da par por razor em cima desta merda

namespace AtivoPlus.Controllers
{
    [Route("/")]
    [ApiController]
    public class MainController : ControllerBase
    {
        [HttpGet]
        public IActionResult ServeFile()
        {
            return Ok("Hello World!"); // Serve a simple text response
        }

    }
}
