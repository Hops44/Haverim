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

        public UsersController(HaverimContext context)
        {
            _context = context;
        }

        [HttpGet("[Action]/{Username}")]
        public bool IsUsernameTaken([FromRoute]string Username)
        {
            return _context.Users.Find(Username) != null;
        }

        [HttpPost("[Action]")]
        public string RegisterUser([FromBody]ApiClasses.RegisterUser user)
        {
            if (_context.Users.Find(user.Username) != null)
            {
                return "error:2";
            }
            if (_context.Users.SingleOrDefault(_ => _.Email == user.Email) != null)
            {
                return "error:3";
            }

            _context.Users.Add(new Models.User
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
            _context.SaveChanges();
            return "success" ;
        }
    }
}