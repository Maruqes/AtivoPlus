using AtivoPlus.Models;
using AtivoPlus.Data;
using Microsoft.AspNetCore.Mvc;
namespace AtivoPlus.Logic
{
    class UserInfoLogic
    {
        public async static Task<ActionResult> SetUserInfo(AppDbContext db, string Username, UserInfo userInfoRequest)
        {
            //se for -1, é o proprio utilizador, ou seja setamos o final_id para o id do utilizador
            //se for um id, DEVE ser um admin, confirmamos, se for, setamos o final_id para o id do utilizador
            int? final_id = null;

            if (userInfoRequest.Id == -1)
            {
                //é o proprio utilizador
                int userId = await UserLogic.GetUserID(db, Username);
                final_id = userId;
            }
            else
            {
                if (await PermissionLogic.CheckPermission(db, Username, new[] { "admin" }) == false)
                {
                    return new UnauthorizedResult();
                }

                final_id = userInfoRequest.Id;
            }

            if (final_id == null)
            {
                return new NotFoundResult();
            }

            bool res = await db.SetUserInfo(final_id.Value, userInfoRequest.Nome, userInfoRequest.Email, userInfoRequest.Telefone, userInfoRequest.Morada, userInfoRequest.NIF, userInfoRequest.IBAN);
            if (res)
            {
                return new OkResult(); // 200 OK
            }
            else
            {
                return new BadRequestResult(); // 400 Bad Request
            }
        }



        public async static Task<UserInfo?> GetUserInfo(AppDbContext db, string username, int id = -1)
        {
            if (id == -1)
            {
                id = await UserLogic.GetUserID(db, username);
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