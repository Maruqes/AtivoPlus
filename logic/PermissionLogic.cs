using AtivoPlus.Models;
using AtivoPlus.Data;
using Microsoft.AspNetCore.Http;

namespace AtivoPlus.Logic
{
    class PermissionLogic
    {

        // -1 is the admin
        public static async Task<int> GetPermissionID(AppDbContext db, string name)
        {
            List<Permission> permissions = await db.GetPermissionsByName(name);
            if (permissions.Count == 0)
            {
                return -2;
            }
            return permissions[0].Id;
        }
        public static async Task<int[]> ConvertPermissionStringToID(AppDbContext db, string[] permsString)
        {
            int[] perms = new int[permsString.Length];
            for (int i = 0; i < permsString.Length; i++)
            {
                perms[i] = await GetPermissionID(db, permsString[i]);
            }
            return perms;
        }
        public static async Task<bool> CheckPermission(AppDbContext db, string username, string[] permsSring)
        {

            int userID = await UserLogic.GetUserID(db, username);
            if (userID == -1)
            {
                return false;
            }

            int[] perms = await ConvertPermissionStringToID(db, permsSring);

            List<UserPermission> userPerms = await db.GetPermissionsByUserId(userID);
            int[] userPermsID = userPerms.Select(x => x.PermissionId).ToArray();

            foreach (int perm in perms)
            {
                if (!userPermsID.Contains(perm))
                {
                    return false;
                }
            }
            return true;
        }

        public static async Task<bool> DoesExistPermission(AppDbContext db, string name)
        {
            return await db.DoesExistPermission(name);
        }

        public static async Task<bool> AddPermission(AppDbContext db, string name)
        {
            if (await db.DoesExistPermission(name) == true)
            {
                return false;
            }
            Permission permission = new Permission();
            permission.Name = name;
            db.Permissions.Add(permission);
            await db.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> DeletePermission(AppDbContext db, string name)
        {
            if (await db.DoesExistPermission(name) == false)
            {
                return false;
            }
            await db.DeletePermission(name);
            return true;
        }


        public static async Task<bool> AddUserPermission(AppDbContext db, string username, string permissionName)
        {
            int userID = await UserLogic.GetUserID(db, username);
            if (userID == -1)
            {
                Console.WriteLine("User does not exist");
                return false;
            }

            int permID = await PermissionLogic.GetPermissionID(db, permissionName);
            if (permID == -2)
            {
                Console.WriteLine("Permission does not exist");
                return false;
            }

            if (await db.AddUserPermission(userID, permID) == false)
            {
                Console.WriteLine("Could not add permission");
                return false;
            }
            return true;
        }

        public static async Task<bool> DeleteUserPermission(AppDbContext db, string username, string permissionName)
        {
            int userID = await UserLogic.GetUserID(db, username);
            if (userID == -1)
            {
                return false;
            }

            int permID = await PermissionLogic.GetPermissionID(db, permissionName);
            if (permID == -1)
            {
                return false;
            }


            if (await db.DeleteUserPermission(userID, permID) == false)
            {
                return false;
            }
            return true;
        }

        public static async Task<List<Permission>> GetPermissionsByUsername(AppDbContext db, string username)
        {
            int userID = await UserLogic.GetUserID(db, username);
            if (userID == -1)
            {
                return new List<Permission>();
            }

            List<UserPermission> userPerms = await db.GetPermissionsByUserId(userID);
            List<Permission> permissions = new List<Permission>();
            foreach (UserPermission userPerm in userPerms)
            {
                permissions.Add(await db.GetPermissionById(userPerm.PermissionId));
            }
            return permissions;
        }
    }
}
