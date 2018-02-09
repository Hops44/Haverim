using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Haverim.Models;
using Newtonsoft.Json;
using Haverim.Controllers.Helpers;

namespace Haverim.Controllers
{
    [Produces("application/json")]
    [Route("api/Users")]
    public class UsersController : Controller
    {
        private readonly HaverimContext _context;

        public UsersController(HaverimContext context) => this._context = context;

        [HttpGet("[Action]/{Username}")]
        public bool IsUsernameTaken([FromRoute]string Username) => this._context.Users.Find(Username) != null;

        [HttpPost("[Action]")]
        public string RegisterUser([FromBody]ApiClasses.RegisterUser user)
        {
            if (String.IsNullOrWhiteSpace(user.Username) || String.IsNullOrWhiteSpace(user.Password) || String.IsNullOrWhiteSpace(user.Email) ||
               (long)user.BirthDateUnix <= -2208988800 || String.IsNullOrWhiteSpace(user.Country) || String.IsNullOrWhiteSpace(user.DisplayName) ||
                user.Password.Length < 6)
            {
                return "error:5";
            }
            if (this._context.Users.Find(user.Username) != null)
                return "error:2";
            if (this._context.Users.SingleOrDefault(_ => _.Email == user.Email) != null)
                return "error:3";

            if (String.IsNullOrWhiteSpace(user.ProfilePic))
                user.ProfilePic = GlobalVariables.DefaultProfilePic;


            this._context.Users.Add(new Models.User
            {
                Username = user.Username,
                DisplayName = user.DisplayName,
                Password = user.Password,
                Email = user.Email,
                ActivityFeed = new List<Activity>(),
                BirthDate = (new DateTime(1970, 1, 1).AddSeconds(user.BirthDateUnix)),
                JoinDate = DateTime.Now,
                Country = user.Country,
                IsMale = user.IsMale,
                Followers = new List<string>(),
                PostFeed = new List<string>(),
                ProfilePic = user.ProfilePic
            });
            this._context.SaveChanges();

            var Payload = new ApiClasses.Payload
            {
                Username = user.Username
            };
            return "success:" + Helpers.JWT.GetToken(Payload);
        }

        [HttpPost("[Action]")]
        public string LoginUser([FromBody]ApiClasses.LoginUser user)
        {
            var LoginUser = this._context.Users.Find(user.Username);
            if (LoginUser == null)
                return "error:0";
            if (LoginUser.Password == user.Password)
            {
                var Payload = new ApiClasses.Payload
                {
                    Username = user.Username
                };
                return "success:" + Helpers.JWT.GetToken(Payload);
            }
            return "error:0";
        }
    }
}