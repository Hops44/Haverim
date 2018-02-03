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

        [NotMapped]
        public List<string> PostFeed
        {
            get
            {
                return JsonConvert.DeserializeObject<List<string>>(_postFeed);
            }
            set
            {
                _postFeed = JsonConvert.SerializeObject(value);
            }
        }

        public string _postFeed { get; set; }

        [NotMapped]
        public List<string> ActivityFeed
        {
            get
            {
                return JsonConvert.DeserializeObject<List<string>>(_activityFeed);
            }
            set
            {
                _activityFeed = JsonConvert.SerializeObject(value);
            }
        }

        public string _activityFeed { get; set; }

        [NotMapped]
        public List<string> Followers
        {
            get
            {
                return JsonConvert.DeserializeObject<List<string>>(_followers);
            }
            set
            {
                _followers = JsonConvert.SerializeObject(value);
            }
        }
        
        public string _followers { get; set; }

    }
}
