using AtivoPlus.Models;
using AtivoPlus.Data;
using Microsoft.AspNetCore.Http;

namespace AtivoPlus.Logic
{
    class ExtraLogic
    {
        public static string GetCookie(HttpRequest Request, string key)
        {
            var ret = Request.Cookies[key];
            if (ret == null)
            {
                return string.Empty;
            }
            return ret;
        }

        public static void SetCookie(HttpResponse Response, string key, string value)
        {
            CookieOptions cookie = new CookieOptions();
            cookie.Expires = System.DateTime.Now.AddDays(7);
            cookie.HttpOnly = true;
            Response.Cookies.Append(key, value, cookie);
        }

        public static void SetCookie(HttpResponse Response, string key, string value, bool httpOnly, int expireDays)
        {
            CookieOptions cookie = new CookieOptions();
            cookie.Expires = System.DateTime.Now.AddDays(expireDays);
            cookie.HttpOnly = httpOnly;
            Response.Cookies.Append(key, value, cookie);
        }

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
