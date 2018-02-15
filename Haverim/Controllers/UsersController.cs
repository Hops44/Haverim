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
               (long)user.BirthDateUnix <= -2208988800 || user.BirthDateUnix > DateTimeOffset.Now.ToUnixTimeSeconds() || String.IsNullOrWhiteSpace(user.Country) || String.IsNullOrWhiteSpace(user.DisplayName) ||
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

        [HttpPost("[Action]")]
        public string FollowUser([FromBody]ApiClasses.FollowRequest request)
        {
            if (String.IsNullOrWhiteSpace(request.Token))
                return "error:5";
            (Helpers.JWT.TokenStatus Status, ApiClasses.Payload Payload) = Helpers.JWT.VerifyToken(request.Token);

            if (Status != Helpers.JWT.TokenStatus.Valid)
                return "error:6";

            var RequestUser = this._context.Users.Find(Payload.Username);
            var UserToBeFollowed = this._context.Users.Find(request.TargetUser);

            if (RequestUser == null)
                return "error:0";

            if (UserToBeFollowed == null)
                return "error:0";

            // User is already following target user
            List<string> RequestUserFollowingList = RequestUser.Following;
            if (RequestUserFollowingList.Contains(UserToBeFollowed.Username))
                return "success";

            List<string> UserToBeFollowedFollowersList = UserToBeFollowed.Followers;
            RequestUserFollowingList.Add(UserToBeFollowed.Username);
            UserToBeFollowedFollowersList.Add(RequestUser.Username);

            RequestUser.Following = RequestUserFollowingList;
            UserToBeFollowed.Followers = UserToBeFollowedFollowersList;

            this._context.SaveChanges();

            return "success";
        }

        [HttpPost("[Action]")]
        public string UnFollowUser([FromBody]ApiClasses.FollowRequest request)
        {
            if (String.IsNullOrWhiteSpace(request.Token) || String.IsNullOrWhiteSpace(request.TargetUser))
                return "error:5";
            (Helpers.JWT.TokenStatus Status, ApiClasses.Payload Payload) = Helpers.JWT.VerifyToken(request.Token);

            if (Status != Helpers.JWT.TokenStatus.Valid)
                return "error:6";


            var RequestUser = this._context.Users.Find(Payload.Username);
            var UserToBeFollowed = this._context.Users.Find(request.TargetUser);

            if (RequestUser == null)
                return "error:0";

            if (UserToBeFollowed == null)
                return "error:0";

            // User is already not following target user
            List<string> RequestUserFollowingList = RequestUser.Following;
            if (!RequestUserFollowingList.Contains(UserToBeFollowed.Username))
                return "success";

            List<string> UserToBeFollowedFollowersList = UserToBeFollowed.Followers;
            RequestUserFollowingList.Remove(UserToBeFollowed.Username);
            UserToBeFollowedFollowersList.Remove(RequestUser.Username);

            RequestUser.Following = RequestUserFollowingList;
            UserToBeFollowed.Followers = UserToBeFollowedFollowersList;

            this._context.SaveChanges();

            return "success";
        }

        [HttpPost("[Action]")]
        public string GetNotifications([FromBody]ApiClasses.FeedRequest request)
        {
            if (String.IsNullOrWhiteSpace(request.Token))
                return "error:5";
            (Helpers.JWT.TokenStatus Status, ApiClasses.Payload Payload) VerifyResult = Helpers.JWT.VerifyToken(request.Token);

            if (VerifyResult.Status != Helpers.JWT.TokenStatus.Valid)
                return "error:6";

            var User = this._context.Users.Find(VerifyResult.Payload.Username);
            if (User == null)
                return "error:0";

            List<Notification> UserNotifications = User.Notifications;
            int FeedCount = UserNotifications.Count();
            if (request.index >= FeedCount)
                return "error:7";

            Notification[] RequestedNotifications;

            if (request.index + 10 >= FeedCount)
            {
                // Loop through the postfeed from index to end
                RequestedNotifications = new Notification[FeedCount - request.index];
                int PostsIndexer = 0;
                for (int i = request.index; i < FeedCount; i++)
                {
                    RequestedNotifications[PostsIndexer] = UserNotifications[i];
                    PostsIndexer++;
                }
            }
            else
            {
                // Loop through the postfeed from index to index + 10
                RequestedNotifications = new Notification[10];
                int PostsIndexer = 0;
                for (int i = request.index; i < request.index + 10; i++)
                {
                    RequestedNotifications[PostsIndexer] = UserNotifications[i];
                    PostsIndexer++;
                }
            }
            return JsonConvert.SerializeObject(RequestedNotifications);
        }

        [HttpGet("[Action]/{username}")]
        public string GetUser([FromRoute]string username)
        {
            if (String.IsNullOrWhiteSpace(username))
                return "error:5";
            var RequestedUser = this._context.Users.Find(username);
            if (RequestedUser==null)
                return "error:0";
            return JsonConvert.SerializeObject(new ApiClasses.PublicUserData(RequestedUser));
            //TODO: [Future Feature] when token is passes return all information, when not return basic information
        }

        [HttpGet("[Action]/{username}/{followers}")]
        public string GetUserFollowers([FromRoute]string username, [FromRoute] bool followers)
        {
            if (String.IsNullOrWhiteSpace(username))
                return "error:5";
            var RequestedUser = this._context.Users.Find(username);
            if (RequestedUser == null)
                return "error:0";
            if (followers)
                return JsonConvert.SerializeObject(RequestedUser.Followers);
            return JsonConvert.SerializeObject(RequestedUser.Following);
        }
    }
}