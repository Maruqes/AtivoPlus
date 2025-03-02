using AtivoPlus.Models;
using AtivoPlus.Data;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace AtivoPlus.Logic
{
    class LoginToken
    {
        public string token = string.Empty;
        public DateTime lastLogin;
    }

    class UserLogic
    {
        private static ConcurrentDictionary<string, LoginToken> userData = new ConcurrentDictionary<string, LoginToken>();

        //if already exists substituir 
        private static string AddUserData(string username)
        {
            string token = Guid.NewGuid().ToString();

            userData[username] = new LoginToken
            {
                token = token,
                lastLogin = DateTime.UtcNow
            };
            return token;
        }

        private static void RemoveUserData(string username)
        {
            userData.TryRemove(username, out _);
        }

        public static void RemoveOutdatedLogins()
        {
            foreach (var usr in userData.Keys.ToList())
            {
                if (userData.TryGetValue(usr, out var lt) &&
                    DateTime.UtcNow - lt.lastLogin > TimeSpan.FromDays(7))
                {
                    RemoveUserData(usr);
                }
            }
        }

        public static void UpdateLoginDate(string username)
        {
            if (userData.TryGetValue(username, out var lt))
            {
                lt.lastLogin = DateTime.UtcNow;
                userData[username] = lt;
            }
        }

        public static bool CheckUserLogged(string username, string token)
        {
            if (userData.TryGetValue(username, out var storedToken) && storedToken.token == token)
            {
                UpdateLoginDate(username);
                return true;
            }
            return false;
        }

        private static async Task<bool> CheckIfUserExists(AppDbContext db, string Username)
        {
            List<User> users = await db.GetUserByUsername(Username);
            return users.Count != 0;
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
                DataCriacao = DateTime.UtcNow // Use DateTime.UtcNow instead of DateTime.Now
            };

            db.Users.Add(novoUser);
            await db.SaveChangesAsync();

            return true;
        }

        public static async Task<bool> CheckPassword(AppDbContext db, string Username, string Password)
        {
            List<User> users = await db.GetUserByUsername(Username);
            if (users == null)
            {
                return false;
            }

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

            return AddUserData(Username);
        }

        //return username if logged in else return empty string
        public static string CheckUserLoggedRequest(HttpRequest Request)
        {
            string cookieUsername = ExtraLogic.GetCookie(Request, "username");
            if (cookieUsername == null)
            {
                return string.Empty;
            }

            var cookieToken = ExtraLogic.GetCookie(Request, "token");
            if (cookieToken == null)
            {
                return string.Empty;
            }

            if (!CheckUserLogged(cookieUsername, cookieToken))
            {
                return string.Empty;
            }
            return cookieUsername;
        }

        public static async Task<int> GetUserID(AppDbContext db, string Username)
        {
            List<User> users = await db.GetUserByUsername(Username);
            if (users == null)
            {
                return -1;
            }

            var user = users.FirstOrDefault(user => user.Username.Equals(Username));
            if(user == null)
            {
                return -1;
            }
            return user.Id;
        }
    }
}
