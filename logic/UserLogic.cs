using AtivoPlus.Models;
using AtivoPlus.Data;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using System;
using Eatease.Service;

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
            User? users = await db.GetUserByUsername(Username);
            return users != null;
        }

        public static async Task<bool> CheckIfUserExistsById(AppDbContext db, int id)
        {
            User? users = await db.GetUserById(id);
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
            User? user = await db.GetUserByUsername(Username);
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

        public static async Task<int?> GetUserID(AppDbContext db, string Username)
        {
            User? user = await db.GetUserByUsername(Username);
            if (user == null)
            {
                return null;
            }
            return user.Id;
        }





        //create a map for user to token to check for valid token

        public class UserUpdate
        {
            public string Username { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
        }

        private static readonly ConcurrentDictionary<string, UserUpdate> userUpdateTokens = new();

        private static string GeneratePasswordResetCode()
        {
            string code;
            do
            {
                // Generate a random 6-digit code
                Random random = new Random();
                int codeNumber = random.Next(100000, 999999);
                code = codeNumber.ToString();
            }
            while (userUpdateTokens.ContainsKey(code));

            return code;
        }
        public static async Task<string> RequestPassUpdate(AppDbContext db, string username)
        {
            string email = await db.GetEmailByUsername(username);
            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("Email not found for user: " + username);
                return string.Empty;
            }

            string token = GeneratePasswordResetCode();

            userUpdateTokens[token] = new UserUpdate
            {
                Username = username,
                Token = token,
            };
            Console.WriteLine($"Generated token for {username}: {token}");

            string subject = "AtivoPlus - Password Update Request";
            string htmlContent = $@"
            <!DOCTYPE html>
            <html lang=""pt"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Password Reset</title>
                <style>
                    * {{
                        margin: 0;
                        padding: 0;
                        box-sizing: border-box;
                    }}
                    
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        background-color: #f5f7fa;
                        color: #333;
                        line-height: 1.6;
                        padding: 20px;
                    }}
                    
                    .email-container {{
                        max-width: 600px;
                        margin: 0 auto;
                        background-color: #ffffff;
                        border-radius: 12px;
                        box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
                        overflow: hidden;
                    }}
                    
                    .header {{
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                        padding: 40px 30px;
                        text-align: center;
                    }}
                    
                    .header h1 {{
                        color: #ffffff;
                        font-size: 32px;
                        font-weight: 700;
                        margin: 0;
                        text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
                    }}
                    
                    .content {{
                        padding: 40px 30px;
                    }}
                    
                    .greeting {{
                        font-size: 24px;
                        font-weight: 600;
                        color: #2d3748;
                        margin-bottom: 20px;
                    }}
                    
                    .message {{
                        font-size: 16px;
                        color: #4a5568;
                        margin-bottom: 16px;
                    }}
                    
                    .token-container {{
                        background: linear-gradient(135deg, #e3f2fd 0%, #f3e5f5 100%);
                        border: 2px solid #667eea;
                        border-radius: 10px;
                        padding: 30px;
                        margin: 30px 0;
                        text-align: center;
                        position: relative;
                    }}
                    
                    .token-container::before {{
                        content: '';
                        position: absolute;
                        top: -2px;
                        left: -2px;
                        right: -2px;
                        bottom: -2px;
                        background: linear-gradient(45deg, #667eea, #764ba2, #667eea);
                        border-radius: 10px;
                        z-index: -1;
                        animation: gradientShift 3s ease-in-out infinite;
                    }}
                    
                    @keyframes gradientShift {{
                        0%, 100% {{ opacity: 0.8; }}
                        50% {{ opacity: 1; }}
                    }}
                    
                    .token {{
                        font-family: 'Courier New', monospace;
                        font-size: 36px;
                        font-weight: bold;
                        color: #667eea;
                        letter-spacing: 8px;
                        text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.1);
                    }}
                    
                    .warning {{
                        background-color: #fff8e1;
                        border: 1px solid #ffcc02;
                        border-left: 4px solid #ff9800;
                        border-radius: 8px;
                        padding: 20px;
                        margin: 25px 0;
                    }}
                    
                    .warning-text {{
                        color: #e65100;
                        font-size: 14px;
                    }}
                    
                    .warning-icon {{
                        font-weight: bold;
                        margin-right: 8px;
                    }}
                    
                    .footer {{
                        border-top: 1px solid #e2e8f0;
                        padding-top: 25px;
                        margin-top: 30px;
                    }}
                    
                    .footer-text {{
                        color: #718096;
                        font-size: 14px;
                        line-height: 1.5;
                    }}
                    
                    .signature {{
                        margin-top: 15px;
                        font-weight: 600;
                        color: #4a5568;
                    }}
                    
                    .company-name {{
                        color: #667eea;
                        font-weight: bold;
                    }}
                    
                    @media (max-width: 600px) {{
                        body {{
                            padding: 10px;
                        }}
                        
                        .email-container {{
                            border-radius: 8px;
                        }}
                        
                        .header, .content {{
                            padding: 25px 20px;
                        }}
                        
                        .header h1 {{
                            font-size: 28px;
                        }}
                        
                        .greeting {{
                            font-size: 20px;
                        }}
                        
                        .token {{
                            font-size: 28px;
                            letter-spacing: 4px;
                        }}
                        
                        .token-container {{
                            padding: 20px;
                        }}
                    }}
                </style>
            </head>
            <body>
                <div class=""email-container"">
                    <div class=""header"">
                        <h1>AtivoPlus</h1>
                    </div>
                    <div class=""content"">
                        <h2 class=""greeting"">Olá {username}!</h2>
                        <p class=""message"">Recebemos um pedido para atualizar a sua password.</p>
                        <p class=""message"">Use o seguinte código para atualizar a sua password:</p>
                        
                        <div class=""token-container"">
                            <div class=""token"">{token}</div>
                        </div>
                        
                        <div class=""warning"">
                            <p class=""warning-text"">
                                <span class=""warning-icon"">⚠️ Importante:</span> 
                                Se não foi você que fez este pedido, por favor ignore este email.
                            </p>
                        </div>
                        
                        <div class=""footer"">
                            <p class=""footer-text"">Este código é válido por tempo limitado por questões de segurança.</p>
                            <p class=""signature"">Atenciosamente,<br><span class=""company-name"">Equipa AtivoPlus</span></p>
                        </div>
                    </div>
                </div>
            </body>
            </html>";

            try
            {
                await BrevoEmail.SendEmailAsync(email, subject, htmlContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email to {username}: {ex.Message}");
            }

            return "";
        }

        public static async Task<bool> UpdatePassword(AppDbContext db, string token, string newPassword)
        {
            if (!userUpdateTokens.TryGetValue(token, out UserUpdate? userUpdate))
            {
                Console.WriteLine("Invalid token.");
                return false;
            }

            string username = userUpdate.Username;
            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("Username not found for token.");
                return false;
            }

            User? user = await db.GetUserByUsername(username);
            if (user == null)
            {
                Console.WriteLine("User not found.");
                return false;
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.Hash = hashedPassword;
            db.Users.Update(user);

            // Save changes to the database
            await db.SaveChangesAsync();

            // Remove the used token
            userUpdateTokens.TryRemove(token, out _);

            Console.WriteLine($"Password updated successfully for user: {username}");
            return true;
        }
    }
}

