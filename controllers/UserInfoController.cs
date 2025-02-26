using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;

//ou app a usar api ou da par por razor em cima desta merda

namespace AtivoPlus.Controllers
{
    [Route("api/userinfo")] // A API está definida em "api/user"
    [ApiController] // Indica que este é um Controller de API
    public class UserInfoController : ControllerBase
    {
        private readonly AppDbContext db;

        public UserInfoController(AppDbContext context)
        {
            db = context;
        }



    }
}
