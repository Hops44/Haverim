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

        public DateTime JoinDate { get; set; }
        public DateTime BirthDate { get; set; }

        public bool IsMale { get; set; }
        public string Country { get; set; }

        public List<Activity> ActivityFeed { get; set; }

        public List<Notification> Notifications { get; set; }

        [NotMapped]
        public List<string> PostFeed
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_postFeed))
                    return new List<string>();
                return JsonConvert.DeserializeObject<List<string>>(_postFeed);
            }
            set
            {
                _postFeed = JsonConvert.SerializeObject(value);
            }
        }
        public string _postFeed { get; set; }


        [NotMapped]
        public List<string> Followers
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_followers))
                    return new List<string>();
                return JsonConvert.DeserializeObject<List<string>>(_followers);
            }
            set
            {
                _followers = JsonConvert.SerializeObject(value);
            }
        }       
        public string _followers { get; set; }


        [NotMapped]
        public List<string> Following
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_following))
                    return new List<string>();
                return JsonConvert.DeserializeObject<List<string>>(_following);
            }
            set
            {
                _following = JsonConvert.SerializeObject(value);
            }
        }
        public string _following { get; set; }

    }
}
