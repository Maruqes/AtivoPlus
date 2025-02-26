using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AtivoPlus.Data
{
    public partial class AppDbContext
    {
        public DbSet<UserInfo> UsersInfo { get; set; }
    }
}