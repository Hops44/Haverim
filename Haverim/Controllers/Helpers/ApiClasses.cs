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

        public class PostFeedRequest
        {
            public string Token;
            public int index;
        }
    }
}
