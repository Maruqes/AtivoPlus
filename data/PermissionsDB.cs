using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }

        public async Task<List<UserPermission>> GetPermissionsByUserId(int userId)
        {
            return await UserPermissions
                .FromSqlInterpolated($"SELECT * FROM \"UserPermissions\" WHERE \"UserId\" = {userId}")
                .ToListAsync();
        }

        public async Task<List<Permission>> GetPermissionsByID(int permissionId)
        {
            return await Permissions
                .FromSqlInterpolated($"SELECT * FROM \"Permissions\" WHERE \"Id\" = {permissionId}")
                .ToListAsync();
        }

        public async Task<List<Permission>> GetPermissionsByName(string permissionname)
        {
            return await Permissions
                .FromSqlInterpolated($"SELECT * FROM \"Permissions\" WHERE \"Name\" = {permissionname}")
                .ToListAsync();
        }

        public async Task<bool> AddPermission(string name)
        {
            Permission permission = new Permission { Name = name };
            Permissions.Add(permission);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePermission(string name)
        {
            await Database.ExecuteSqlInterpolatedAsync($"DELETE FROM \"Permissions\" WHERE \"Name\" = {name}");
            return true;
        }

        public async Task<bool> DoesExistPermission(string name)
        {
            return await Permissions
                .FromSqlInterpolated($"SELECT * FROM \"Permissions\" WHERE \"Name\" = {name}")
                .AnyAsync();
        }

        public async Task<bool> AddUserPermission(int userId, int permissionId)
        {
            UserPermission userPermission = new UserPermission { UserId = userId, PermissionId = permissionId };
            UserPermissions.Add(userPermission);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserPermission(int userId, int permissionId)
        {
            await Database.ExecuteSqlInterpolatedAsync($"DELETE FROM \"UserPermissions\" WHERE \"UserId\" = {userId} AND \"PermissionId\" = {permissionId}");
            return true;
        }
    }
}
