using AtivoPlus.Models;
using AtivoPlus.Data;

namespace AtivoPlus.Logic
{
    class UserLogic
    {

        private static async Task<bool> CheckIfUserExists(AppDbContext db, string Username)
        {
            List<User> users = await db.GetUserByUsername(Username);
            if (users.Count != 0)
            {
                return false;
            }
            return true;
        }

        public static async Task<bool> AddUser(AppDbContext db, string Username, string Password)
        {
            if (await CheckIfUserExists(db, Username))
            {
                return false;
            }

            string hash = BCrypt.Net.BCrypt.HashPassword(Password);

            User novoUser = new User
            {
                Username = Username,
                Hash = hash,
                DataCriacao = DateTime.UtcNow
            };

            db.Users.Add(novoUser);
            db.SaveChanges();

            return true;
        }


        public static async Task<bool> CheckPassword(AppDbContext db, string Username, string Password)
        {
            //ir buscar a hash do user
            //compar a nossa pass

            List<User> users = await db.GetUserByUsername(Username);

            var user = users.FirstOrDefault(user => user.Username.Equals(Username));

            return user != null && BCrypt.Net.BCrypt.Verify(Password, user.Hash);
        }
    }
}