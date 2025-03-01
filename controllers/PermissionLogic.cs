using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;



namespace AtivoPlus.Controllers
{
    [Route("/permission")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        /*
        create perms form -1 only
        create delete

        set perms
        endpoints-> sets from -1
        gets from -1 or itself
        */

        public class PermsCreation
        {
            string Name = string.Empty;
        }

        [HttpPost("/createPermission")]
        public async Task<ActionResult<User>> CreatePermission([FromBody] PermsCreation request)
        {
            return Ok();
        }

    }
}
