using Haverim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haverim.Controllers.Helpers
{
    public static class ApiClasses
    {
        // Json body which is received from post request
        // Will be serialized to a RegisterUser object
        public class RegisterUser
        {
            public string Username;
            public string DisplayName;
            public string Email;
            public string Password;
            public int BirthDateUnix;
            public string Country;
            public bool IsMale;
            public string ProfilePicBase64;
        }

        public class CreatePost
        {
            public string Token;
            public string Body;
            public List<string> Tags;
        }

        public class LoginUser
        {
            public string Username;
            public string Password;
        }

        public class Payload
        {
            public string Username;

            public override bool Equals(object obj)
            {
                if (obj is Payload)
                {
                    if (((Payload)obj).Username == this.Username)
                    {
                        return true;
                    }
                }
                return false;
            }
            public override int GetHashCode() => base.GetHashCode();
        }

        public class FeedRequest
        {
            public string Token;
            public int index;
        }

        public class ActivityFeedRequest : FeedRequest
        {
            public string TargetUser;
        }

        public class CreateReply
        {
            public string Token;
            public string Body;
            public string PostId;
        }

        public class FollowRequest
        {
            public string Token;
            public string TargetUser;
        }

        public class UpvoteRequest
        {
            public string Token;
            public string PostId;
        }

        public class CommentUpvoteRequest : UpvoteRequest
        {
            public string CommentId;
        }

        public class PublicUserData
        {
            public PublicUserData() { }
            public PublicUserData(User user)
            {
                this.Username = user.Username;
                this.DisplayName = user.DisplayName;
                this.ProfilePic = user.ProfilePic;
                this.JoinDate = user.JoinDate;
                this.BirthDate = user.BirthDate;
                this.IsMale = user.IsMale;
                this.Country = user.Country;
                this.FollowersCount = user.Followers.Count;
                this.FollowingCount = user.Following.Count;
                this.ProfilePagePic = user.ProfilePagePicture;
            }
            public string Username;
            public string DisplayName;
            public string ProfilePic;
            public string ProfilePagePic;
            public DateTime JoinDate;
            public DateTime BirthDate;
            public bool IsMale;
            public string Country;
            public int FollowersCount;
            public int FollowingCount;
        }

        public class CurrentUserData
        {
            public CurrentUserData() { }

            public CurrentUserData(User u)
            {
                this.Username = u.Username;
                this.DisplayName = u.DisplayName;
                this.ProfilePic = u.ProfilePic;
                this.JoinDate = u.JoinDate;
                this.BirthDate = u.BirthDate;
                this.IsMale = u.IsMale;
                this.Country = u.Country;
                this.ActivityFeed = u.ActivityFeed;
                this.Notifications = u.Notifications;
                this.PostFeed = u.PostFeed;
                this.Followers = u.Followers;
                this.Following = u.Following;
                this.ProfilePagePicture = u.ProfilePagePicture;
            }
            public string Username { get; set; }
            public string DisplayName { get; set; }
            public string ProfilePic { get; set; }
            public string ProfilePagePicture { get; set; }

            public DateTime JoinDate { get; set; }
            public DateTime BirthDate { get; set; }

            public bool IsMale { get; set; }
            public string Country { get; set; }

            public List<Activity> ActivityFeed { get; set; }
            public List<Notification> Notifications { get; set; }

            public List<string> PostFeed;
            public List<string> Followers;
            public List<string> Following;
        }

        public class KeyClass
        {
            public string Key;
        }

        public class BasicUserData
        {
            public string DisplayName;
            public string Username;
            public string ProfilePic;
        }

        public class PostCommentsRequest
        {
            public string Token;
            public string PostId;
        }

        public class ActivityFeedItem
        {
            public Post Post;
            public Activity Activity;

            public ActivityFeedItem(Post post, Activity activity)
            {
                this.Post = post;
                this.Activity = activity;
            }

        }

        public class PictureUpadteRequest
        {
            public string Token;
            public string ImageBase64Data;
            public ImageType Type;
            public enum ImageType
            {
                ProfilePicture,
                ProfilePagePicture
            }
        }
    }
}
