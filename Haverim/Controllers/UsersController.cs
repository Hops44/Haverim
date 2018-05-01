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
using System.Threading;
using System.IO;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Net;
using RestSharp;

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

            if (String.IsNullOrWhiteSpace(user.ProfilePicBase64))
                user.ProfilePicBase64 = GlobalVariables.DefaultProfilePic;

            string ImageUrl;
            if (String.IsNullOrWhiteSpace(user.ProfilePicBase64))
            {
                var RestClient = new RestClient(GlobalVariables.ImageApiBaseUrl);
                var RestRequest = new RestRequest("/api/Image", Method.POST);
                RestRequest.AddJsonBody(user.ProfilePicBase64);

                IRestResponse response = RestClient.Execute(RestRequest);
                ImageUrl = response.Content;
                try
                {
                    ImageUrl = ImageUrl.Substring(1, ImageUrl.Length - 2);
                }
                catch { }
            }
            else
            {
                ImageUrl = "default";
            }



            user.Username = user.Username.ToLower();
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
                ProfilePic = ImageUrl,
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
            if (UserToBeFollowed.Username == Payload.Username)
                return "error:9";

            // User is already following target user
            List<string> RequestUserFollowingList = RequestUser.Following;
            if (RequestUserFollowingList.Contains(UserToBeFollowed.Username))
                return "success";

            List<string> UserToBeFollowedFollowersList = UserToBeFollowed.Followers;
            RequestUserFollowingList.Add(UserToBeFollowed.Username);
            UserToBeFollowedFollowersList.Add(RequestUser.Username);

            RequestUser.Following = RequestUserFollowingList;
            UserToBeFollowed.Followers = UserToBeFollowedFollowersList;

            // Notify user of the new follower
            var UserToBeFollowedNotifications = UserToBeFollowed.Notifications;
            if (UserToBeFollowedNotifications == null)
                UserToBeFollowedNotifications = new List<Notification>();
            UserToBeFollowedNotifications.Insert(0, new Notification
            {
                PublishDate = DateTime.Now,
                TargetUsername = Payload.Username,
                Type = NotificationType.NewFollower
            });
            UserToBeFollowed.Notifications = UserToBeFollowedNotifications;


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

            if (UserToBeFollowed.Username == Payload.Username)
                return "error:9";

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
        public JsonResult GetNotifications([FromBody]ApiClasses.FeedRequest request)
        {
            if (String.IsNullOrWhiteSpace(request.Token))
                return Json("error:5");
            (Helpers.JWT.TokenStatus Status, ApiClasses.Payload Payload) = Helpers.JWT.VerifyToken(request.Token);

            if (Status != Helpers.JWT.TokenStatus.Valid)
                return Json("error:6");

            var User = this._context.Users.Find(Payload.Username);
            if (User == null)
                return Json("error:0");

            List<Notification> UserNotifications = User.Notifications;
            if (UserNotifications == null)
                return Json("[]");

            int FeedCount = UserNotifications.Count();
            if (request.index >= FeedCount)
                return Json("error:7");

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
            return Json(RequestedNotifications);
        }

        [HttpGet("[Action]/{username}")]
        public JsonResult GetUser([FromRoute]string username)
        {
            if (String.IsNullOrWhiteSpace(username))
                return Json("error:5");
            var RequestedUser = this._context.Users.Find(username);
            if (RequestedUser == null)
                return Json("error:0");
            if (!String.IsNullOrWhiteSpace(RequestedUser.ProfilePic ))
            {
                RequestedUser.ProfilePic = GlobalVariables.ImageApiGetUrl + RequestedUser.ProfilePic;
            }
            else
            {
                RequestedUser.ProfilePic = GlobalVariables.ImageApiGetUrl + "default";
            }
            if (RequestedUser.ProfilePagePicture != null)
            {
                RequestedUser.ProfilePagePicture = GlobalVariables.ImageApiGetUrl + RequestedUser.ProfilePagePicture;
            }

            return Json(RequestedUser);
            //TODO: [Future Feature] when token is passes return all information, when not return basic information
        }

        [HttpGet("[Action]/{username}/{followers}/{getAll}/{index}")]
        public JsonResult GetUserFollowers([FromRoute]string username, [FromRoute] bool followers, [FromRoute] bool getAll = true, [FromRoute] int index = 0)
        {
            if (String.IsNullOrWhiteSpace(username))
                return Json("error:5");
            var RequestedUser = this._context.Users.Find(username);
            if (RequestedUser == null)
                return Json("error:0");

            var FollowUsers = followers ? RequestedUser.Followers : RequestedUser.Following;
            if (getAll)
            {
                if (followers)
                    return Json(RequestedUser.Followers);
                return Json(RequestedUser.Following);
            }

            int Length = FollowUsers.Count;
            List<ApiClasses.BasicUserData> RequestedUsers = new List<ApiClasses.BasicUserData>();


            if (index >= Length)
            {
                return Json(new string[] { });
            }

            int RequestLength = 10;
            if (index + 10 >= Length)
                RequestLength = Length - index;

            for (int i = 0; i < RequestLength; i++)
            {
                User CurrentUser = this._context.Users.Find(FollowUsers[index + i]);
                RequestedUsers.Add(new ApiClasses.BasicUserData
                {
                    DisplayName = CurrentUser.DisplayName,
                    Username = CurrentUser.Username,
                    ProfilePic = CurrentUser.ProfilePic,
                });
            }

            return Json(RequestedUsers);
        }

        [HttpPost("[Action]")]
        public JsonResult GetUserByToken([FromBody] ApiClasses.KeyClass JWTKey)
        {
            (Helpers.JWT.TokenStatus status, Helpers.ApiClasses.Payload payload) = Helpers.JWT.VerifyToken(JWTKey.Key);
            if (status != Helpers.JWT.TokenStatus.Valid)
            {
                return Json("error:6");
            }
            else
            {
                var User = this._context.Users.Find(payload.Username);
                if (User == null)
                {
                    return Json("error:0");
                }
                if (string.IsNullOrWhiteSpace(User.ProfilePic))
                {
                    User.ProfilePic = "default";
                }
                User.ProfilePic = GlobalVariables.ImageApiGetUrl + User.ProfilePic;
                User.ProfilePagePicture = GlobalVariables.ImageApiGetUrl + User.ProfilePagePicture;
                return Json(new ApiClasses.CurrentUserData(User));
            }

        }

        [HttpGet("[Action]/{q}")]
        public JsonResult SearchUser([FromRoute]string q)
        {
            q = q.ToUpperInvariant();
            List<ApiClasses.BasicUserData> MatchedUsers = new List<ApiClasses.BasicUserData>();
            foreach (var user in _context.Users)
            {
                if (user.DisplayName.ToUpperInvariant().Contains(q) || user.Username.ToUpperInvariant().Contains(q))
                {
                    MatchedUsers.Add(new ApiClasses.BasicUserData { Username = user.Username, DisplayName = user.DisplayName, ProfilePic = GlobalVariables.ImageApiGetUrl + user.ProfilePic });
                }
                if (MatchedUsers.Count == 10)
                {
                    break;
                }
            }
            return Json(MatchedUsers);
        }

        [HttpPost("[Action]")]
        public string UploadImage([FromBody] string Base64Data)
        {
            if (string.IsNullOrWhiteSpace(Base64Data))
            {
                return "error:5";
            }
            var RestClient = new RestClient(GlobalVariables.ImageApiBaseUrl);
            var RestRequest = new RestRequest("/api/Image", Method.POST);
            RestRequest.AddJsonBody(Base64Data);

            IRestResponse response = RestClient.Execute(RestRequest);
            string ImageUrl = response.Content;
            try
            {
                ImageUrl = ImageUrl.Substring(1, ImageUrl.Length - 2);
            }
            catch { }
            return ImageUrl;
        }

        [HttpPost("[Action]")]
        public string ChangeUserPictrue([FromBody] ApiClasses.PictureUpadteRequest request)
        {
            if (String.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.ImageBase64Data))
                return "error:5";
            (Helpers.JWT.TokenStatus Status, ApiClasses.Payload Payload) = Helpers.JWT.VerifyToken(request.Token);

            if (Status != Helpers.JWT.TokenStatus.Valid)
                return "error:6";

            var User = this._context.Users.Find(Payload.Username);
            if (User == null)
                return "error:0";

            string ImageId = UploadImage(request.ImageBase64Data);
            switch (request.Type)
            {
                case ApiClasses.PictureUpadteRequest.ImageType.ProfilePicture:
                    User.ProfilePic = ImageId;
                    break;
                case ApiClasses.PictureUpadteRequest.ImageType.ProfilePagePicture:
                    User.ProfilePagePicture = ImageId;
                    break;
            }
            this._context.SaveChanges();
            return "success";
        }
    }
}