using AtivoPlus.Models;
using AtivoPlus.Data;

namespace AtivoPlus.Logic
{

    class LoginToken
    {
        public string token;
        public DateTime lastLogin;
    }

    class UserLogic
    {
        private static Dictionary<string, string> userData = new Dictionary<string, string>();

        //if aready exists substituir 
        private static string addUserData(string username)
        {
            string token = Guid.NewGuid().ToString();
            userData[username] = token;
            return token;
        }

        private static void removeUserData(string username)
        {
            userData.Remove(username);
        }

        public static bool CheckUserLogged(string username, string token)
        {
            return userData.TryGetValue(username, out var storedToken) && storedToken == token;
        }


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
            List<User> users = await db.GetUserByUsername(Username);

            var user = users.FirstOrDefault(user => user.Username.Equals(Username));

            return user != null && BCrypt.Net.BCrypt.Verify(Password, user.Hash);
        }

        //Returns user Token else returns empty string
        public static async Task<string> LogarUser(AppDbContext db, string Username, string Password)
        {
            if (await CheckPassword(db, Username, Password) == false)
            {
                return string.Empty;
            }

            return addUserData(Username);
        }


    }
}
