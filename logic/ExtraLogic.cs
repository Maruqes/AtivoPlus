using AtivoPlus.Models;
using AtivoPlus.Data;
using Microsoft.AspNetCore.Http;

namespace AtivoPlus.Logic
{
    class ExtraLogic
    {
        /// <summary>
        /// Verifica se a permissão 'admin' já existe, e cria-a caso não exista
        /// </summary>
        public static void SetUpAdminPermission(AppDbContext db)
        {
            // Verifica se a permissão 'admin' já existe
            if (!db.Permissions.Any(p => p.Id == -1))
            {
                var adminPermission = new Permission
                {
                    Id = -1,
                    Name = "admin"
                };

                db.Permissions.Add(adminPermission);
                db.SaveChanges();
            }
        }
        public static string GetCookie(HttpRequest request, string key)
        {
            return request.Cookies[key] ?? string.Empty;
        }

        public static void SetCookie(HttpContext context, string key, string value)
        {
            var isDev = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();

            var cookie = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true // mesmo em dev, se estiveres a fazer chamadas cross-origin
            };

            context.Response.Cookies.Append(key, value, cookie);
        }


        public static void SetCookie(HttpContext context, string key, string value, bool httpOnly = true, int expireDays = 7)
        {
            // ⚠️ SEMPRE usar SameSite=None e Secure=true para funcionar em cross-site (com credentials: 'include')
            var cookie = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(expireDays),
                HttpOnly = httpOnly,
                SameSite = SameSiteMode.None,
                Secure = true // mesmo em dev, se estiveres a fazer chamadas cross-origin
            };

            context.Response.Cookies.Append(key, value, cookie);
        }



        /// <summary>
        /// uma rotina que corre todos os dias para remover logins antigos
        /// </summary>
        public async static void CheckUsersTokens()
        {
            while (true)
            {
                try
                {
                    UserLogic.RemoveOutdatedLogins();
                    Console.WriteLine("Removing Logins");
                    await Task.Delay(TimeSpan.FromDays(1));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro: {ex.Message}");
                    // Opcionalmente, podes adicionar uma espera curta para evitar loop infinito em caso de erro
                    await Task.Delay(TimeSpan.FromMinutes(30));
                }
            }
        }
    }
}
