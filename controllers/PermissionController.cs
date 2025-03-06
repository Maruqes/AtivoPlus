using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;



namespace AtivoPlus.Controllers
{
    [Route("api/permission")]
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

        private readonly AppDbContext db;

        public PermissionController(AppDbContext context)
        {
            db = context;
        }

        public class PermsCreation
        {
            public string Name { get; set; } = string.Empty;
        }

        [HttpPut("createPermission")]
        public async Task<ActionResult<User>> CreatePermission([FromBody] PermsCreation permissionRequest)
        {
            if (permissionRequest.Name == string.Empty)
            {
                return BadRequest();
            }
            string Username = UserLogic.CheckUserLoggedRequest(Request);
            if (Username == string.Empty)
            {
                return Unauthorized();
            }

            if (await PermissionLogic.CheckPermission(db, Username, new[] { "admin" }) == false)
            {
                return Unauthorized();
            }

            if (await PermissionLogic.DoesExistPermission(db, permissionRequest.Name) == true)
            {
                return BadRequest();
            }

            if (await PermissionLogic.AddPermission(db, permissionRequest.Name) == false)
            {
                return BadRequest();
            }
            return Ok();
        }

        [HttpDelete("deletePermission")]
        public async Task<ActionResult<User>> DeletePermission([FromBody] PermsCreation permissionRequest)
        {
            string Username = UserLogic.CheckUserLoggedRequest(Request);
            if (Username == string.Empty)
            {
                return Unauthorized();
            }

            if (await PermissionLogic.CheckPermission(db, Username, new[] { "admin" }) == false)
            {
                return Unauthorized();
            }

            if (await PermissionLogic.DeletePermission(db, permissionRequest.Name) == false)
            {
                return BadRequest();
            }
            return Ok();
        }

        public class PermsUser
        {
            public string Username { get; set; } = string.Empty;
            public string PermissionName { get; set; } = string.Empty;
        }

        [HttpPut("addUserPermission")]
        public async Task<ActionResult<User>> AddUserPermission([FromBody] PermsUser userPermission)
        {
            string Username = UserLogic.CheckUserLoggedRequest(Request);
            if (Username == string.Empty)
            {
                return Unauthorized();
            }
            if (await PermissionLogic.CheckPermission(db, Username, new[] { "admin" }) == false)
            {
                return Unauthorized();
            }


            if (await db.DoesExistPermission(userPermission.PermissionName) == false)
            {
                Console.WriteLine("Permission does not exist");
                return BadRequest();
            }

            if (await PermissionLogic.AddUserPermission(db, userPermission.Username, userPermission.PermissionName) == false)
            {
                Console.WriteLine("Could not add permission");
                return BadRequest();
            }
            return Ok();
        }

        [HttpDelete("deleteUserPermission")]
        public async Task<ActionResult<User>> DeleteUserPermission([FromBody] PermsUser userPermission)
        {
            string Username = UserLogic.CheckUserLoggedRequest(Request);
            if (Username == string.Empty)
            {
                return Unauthorized();
            }
            if (await PermissionLogic.CheckPermission(db, Username, new[] { "admin" }) == false)
            {
                return Unauthorized();
            }

            if (await PermissionLogic.DeleteUserPermission(db, userPermission.Username, userPermission.PermissionName) == false)
            {
                return BadRequest();
            }
            return Ok();
        }

        [HttpGet("getPermissions")]
        public async Task<ActionResult<List<Permission>>> GetPermissions()
        {
            string Username = UserLogic.CheckUserLoggedRequest(Request);
            if (Username == string.Empty)
            {
                return Unauthorized();
            }

            return await PermissionLogic.GetPermissionsByUsername(db, Username);

        }
    }
}
