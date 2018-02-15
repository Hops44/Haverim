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
            public string ProfilePic;
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
            }
            public string Username;
            public string DisplayName;
            public string ProfilePic;
            public DateTime JoinDate;
            public DateTime BirthDate;
            public bool IsMale;
            public string Country;
            public int FollowersCount;
            public int FollowingCount;
        }
    }
}
