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
        /*
        where we store the user login data, a username can only have one token
        limits the number of tokens to 1 per user, so only one device can be logged in at a time

        a token is a random string that is generated when a user logs in and is stored in a cookie to identify the user
        only that user can use that token to authenticate himself in the future
        */
        private static ConcurrentDictionary<string, LoginToken> userData = new ConcurrentDictionary<string, LoginToken>();

        /// <summary>
        /// Adds a new user to the user data and returns the token generated for the user.
        /// </summary>
        /// <param name="username">The username of the user to add.</param>
        /// <returns>The token generated for the user.</returns>

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

        /// <summary>
        /// Removes the user with the given username from the user data.
        /// </summary>

        private static void RemoveUserData(string username)
        {
            userData.TryRemove(username, out _);
        }

        /// <summary>
        /// Removes all users that have not logged in for more than 7 days.
        /// </summary>
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

        /// <summary>
        /// Updates the last login date of the user with the given username.
        /// </summary>
        public static void UpdateLoginDate(string username)
        {
            if (userData.TryGetValue(username, out var lt))
            {
                lt.lastLogin = DateTime.UtcNow;
                userData[username] = lt;
            }
        }

        ///<summary>
        ///Checks if the user is logged in
        ///</summary>
        ///<param name="username">The username of the user to check.</param>
        ///<param name="token">The token of the user to check.</param>
        ///<returns>
        ///<c>true</c> if the user is logged in; otherwise, <c>false</c>.
        ///</returns>
        public static bool CheckUserLogged(string username, string token)
        {
            if (userData.TryGetValue(username, out var storedToken) && storedToken.token == token)
            {
                UpdateLoginDate(username);
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckIfUserExists(AppDbContext db, string Username)
        {
            User users = await db.GetUserByUsername(Username);
            return users != null;
        }

        public static async Task<bool> CheckIfUserExistsById(AppDbContext db, int id)
        {
            User users = await db.GetUserById(id);
            return users != null;
        }

        public static async Task<bool> AddUser(AppDbContext db, string Username, string Password)
        {
            //check if user already exists
            if (await CheckIfUserExists(db, Username))
            {
                return false;
            }

            //hash password for example -> 123 -> $2a$11$1qZQ7j....
            string hash = BCrypt.Net.BCrypt.HashPassword(Password);

            User novoUser = new()
            {
                Username = Username,
                Hash = hash,
                DataCriacao = DateTime.UtcNow
            };

            db.Users.Add(novoUser);
            await db.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Asynchronously checks if the provided password matches the stored password hash for the user with the given username.
        /// </summary>
        /// <param name="db">An instance of <c>AppDbContext</c> to access the user data.</param>
        /// <param name="Username">The username of the user to validate.</param>
        /// <param name="Password">The password provided for verification.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains <c>true</c>
        /// if the password matches the stored hash for the user; otherwise, <c>false</c>.
        /// </returns>
        public static async Task<bool> CheckPassword(AppDbContext db, string Username, string Password)
        {
            User user = await db.GetUserByUsername(Username);
            if (user == null)
            {
                return false;
            }

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

        public static void LogoutUser(string Username, string token)
        {
            if (!CheckUserLogged(Username, token))
            {
                return;
            }

            RemoveUserData(Username);
        }

        public static string GetUsernameWithRequest(HttpRequest request)
        {
            string cookieUsername = ExtraLogic.GetCookie(request, "username");
            return cookieUsername ?? string.Empty;
        }

        public static string GetTokenWithRequest(HttpRequest request)
        {
            string cookieToken = ExtraLogic.GetCookie(request, "token");
            return cookieToken ?? string.Empty;
        }


        //return username if logged in else return empty string
        public static string CheckUserLoggedRequest(HttpRequest Request)
        {
            string cookieUsername = GetUsernameWithRequest(Request);
            if (cookieUsername == null)
            {
                return string.Empty;
            }

            var cookieToken = GetTokenWithRequest(Request);
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
            User user = await db.GetUserByUsername(Username);
            if (user == null)
            {
                return -1;
            }
            return user.Id;
        }
    }
}

