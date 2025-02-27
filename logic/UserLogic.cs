using AtivoPlus.Models;
using AtivoPlus.Data;

namespace AtivoPlus.Logic
{
    class UserLogic
    {
        private static Dictionary < string, string > userData = new Dictionary < string, string > ();

        private static string addUserData(string Username){
            string token = Guid.NewGuid().ToString();
            userData.Add(Username, token);
            return token;
        }

        private static void removeUserData(string Username){
            userData.Remove(Username);
        }

        public static bool CheckUserLogged(string Username, string token){
            string tokenGet = userData[Username];
            
            return token == tokenGet;
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

        public static async void AddUser(AppDbContext db, string Username, string Password)
        {
            if (await CheckIfUserExists(db, Username))
            {
                return;
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
        }


        public static async Task<bool> CheckPassword(AppDbContext db, string Username, string Password)
        {
            //ir buscar a hash do user
            //compar a nossa pass

            List<User> users = await db.GetUserByUsername(Username);

            var user = users.FirstOrDefault(user => user.Username.Equals(Username));

            return user != null && BCrypt.Net.BCrypt.Verify(Password, user.Hash);
        }

        //Returns user Token else returns empty string
        public static async Task<string> LogarUser(AppDbContext db, string Username, string Password){
            // confirmar se user psw existe,
            // se existir adicionar um mapa a combinacao do user e token gerada
            // map string string  usernsme - token
            
            if(await CheckPassword(db, Username, Password)){
                return addUserData(Username);
            }

            return string.Empty;
        }


    }
}
