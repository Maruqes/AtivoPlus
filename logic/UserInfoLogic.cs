using AtivoPlus.Models;
using AtivoPlus.Controllers;
using AtivoPlus.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
namespace AtivoPlus.Logic
{
    class UserInfoLogic
    {
        public static async Task<int> CreateMorada(AppDbContext db, MoradaRequest moradaRequest, int userId)
        {
            return await db.CreateMorada(userId, moradaRequest.Rua, moradaRequest.Piso, moradaRequest.NumeroPorta, moradaRequest.Concelho, moradaRequest.Distrito, moradaRequest.Localidade, moradaRequest.CodPostal);
        }

        public async static Task<ActionResult> SetUserInfo(AppDbContext db, string Username, UserInfo userInfoRequest, MoradaRequest moradaRequest)
        {
            //se for -1, é o proprio utilizador, ou seja setamos o final_id para o id do utilizador
            //se for um id, DEVE ser um admin, confirmamos, se for, setamos o final_id para o id do utilizador
            int? final_id = null;
            int? userId = await UserLogic.GetUserID(db, Username);
            if (userId == null)
            {
                return new UnauthorizedObjectResult("User not found");
            }

            if (userInfoRequest.Id == -1)
            {
                final_id = userId;
            }
            else
            {
                if (await PermissionLogic.CheckPermission(db, Username, new[] { "admin" }) == false)
                {
                    return new UnauthorizedObjectResult("User is not an admin");
                }

                final_id = userInfoRequest.Id;
            }

            if (final_id == null)
            {
                return new NotFoundObjectResult("User not found");
            }

            int? morada_id = await CreateMorada(db, moradaRequest, final_id.Value);
            if (morada_id == -1 || morada_id == null)
            {
                return new NotFoundObjectResult("User not found");
            }

            bool res = await db.SetUserInfo(final_id.Value, userInfoRequest.Nome, userInfoRequest.Email, userInfoRequest.Telefone, morada_id.Value, userInfoRequest.NIF, userInfoRequest.IBAN);
            if (res)
            {
                return new OkResult(); // 200 OK
            }
            else
            {
                return new ObjectResult("estourou primasso") { StatusCode = StatusCodes.Status500InternalServerError }; // 500 Internal Server Error
            }
        }



        public async static Task<UserInfo?> GetUserInfo(AppDbContext db, string username, int id = -1)
        {
            if (id == -1)
            {
                int? newid = await UserLogic.GetUserID(db, username);
                if (newid == null)
                {
                    return null;
                }
                id = newid.Value;
            }
            else
            {
                if (await PermissionLogic.CheckPermission(db, username, new[] { "admin" }) == false)
                {
                    return null;
                }
            }

            UserInfo? userInfo = await db.GetUserInfo(id);

            return userInfo;
        }
    }
}