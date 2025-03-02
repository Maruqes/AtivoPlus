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

        [HttpPost("createPermission")]
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

            if (await db.DoesExistPermission(permissionRequest.Name) == true)
            {
                return BadRequest();
            }

            if (await db.AddPermission(permissionRequest.Name) == false)
            {
                return BadRequest();
            }
            return Ok();
        }

        [HttpPost("deletePermission")]
        public async Task<ActionResult<User>> DeletePermission([FromBody] PermsCreation permissionRequest)
        {
            string Username =  UserLogic.CheckUserLoggedRequest(Request);
            if (Username == string.Empty)
            {
                return Unauthorized();
            }

            if (await PermissionLogic.CheckPermission(db, Username, new[] { "admin" }) == false)
            {
                return Unauthorized();
            }

            if (await db.DeletePermission(permissionRequest.Name) == false)
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

        [HttpPost("addUserPermission")]
        public async Task<ActionResult<User>> AddUserPermission([FromBody] PermsUser userPermission)
        {
            string Username =  UserLogic.CheckUserLoggedRequest(Request);
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

            int userID = await UserLogic.GetUserID(db, userPermission.Username);
            if (userID == -1)
            {
                Console.WriteLine("User does not exist");
                return BadRequest();
            }

            int permID = await PermissionLogic.GetPermissionID(db, userPermission.PermissionName);
            if (permID == -2)
            {
                Console.WriteLine("Permission does not exist");
                return BadRequest();
            }

            if (await db.AddUserPermission(userID, permID) == false)
            {
                Console.WriteLine("Could not add permission");
                return BadRequest();
            }
            return Ok();
        }

        [HttpPost("deleteUserPermission")]
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

            int userID = await UserLogic.GetUserID(db, userPermission.Username);
            if (userID == -1)
            {
                return BadRequest();
            }

            int permID = await PermissionLogic.GetPermissionID(db, userPermission.PermissionName);
            if (permID == -1)
            {
                return BadRequest();
            }

            if (await db.DeleteUserPermission(userID, permID) == false)
            {
                return BadRequest();
            }
            return Ok();
        }


    }
}
