using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Haverim.Models
{
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public string ProfilePic { get; set; }
        public string ProfilePagePicture { get; set; }

        public DateTime JoinDate { get; set; }
        public DateTime BirthDate { get; set; }

        public bool IsMale { get; set; }
        public string Country { get; set; }

        [NotMapped]
        public List<Activity> ActivityFeed
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_activityFeed))
                    return new List<Activity>();
                else
                    return JsonConvert.DeserializeObject<List<Activity>>(_activityFeed);
            }
            set
            {
                _activityFeed = JsonConvert.SerializeObject(value);
            }
        }
        public string _activityFeed { get; set; }

        [NotMapped]
        public List<Notification> Notifications
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_notifications))
                    return new List<Notification>();
                else
                    return JsonConvert.DeserializeObject<List<Notification>>(_notifications);
            }
            set
            {
                _notifications = JsonConvert.SerializeObject(value);
            }
        }
        public string _notifications { get; set; }

        [NotMapped]
        public List<string> PostFeed
        {
            get
            {
                if (String.IsNullOrWhiteSpace(this._postFeed))
                    return new List<string>();
                return JsonConvert.DeserializeObject<List<string>>(this._postFeed);
            }
            set
            {
                this._postFeed = JsonConvert.SerializeObject(value);
            }
        }
        public string _postFeed { get; set; }


        [NotMapped]
        public List<string> Followers
        {
            get
            {
                if (String.IsNullOrWhiteSpace(this._followers))
                    return new List<string>();
                return JsonConvert.DeserializeObject<List<string>>(this._followers);
            }
            set
            {
                this._followers = JsonConvert.SerializeObject(value);
            }
        }
        public string _followers { get; set; }


        [NotMapped]
        public List<string> Following
        {
            get
            {
                if (String.IsNullOrWhiteSpace(this._following))
                    return new List<string>();
                return JsonConvert.DeserializeObject<List<string>>(this._following);
            }
            set
            {
                this._following = JsonConvert.SerializeObject(value);
            }
        }
        public string _following { get; set; }

    }
}
