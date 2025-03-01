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

        public static async Task<bool> AddPermission(AppDbContext db, string name)
        {
            Permission permission = new Permission();
            permission.Name = name;
            db.Permissions.Add(permission);
            await db.SaveChangesAsync();
            return true;
        }


    }
}
