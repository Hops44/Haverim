using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haverim.Models
{
    public class Post
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public string PublisherId { get; set; }
        public DateTime PublishDate { get; set; }
        public string Body { get; set; }
        public List<Comment> Comments { get; set; }

        [NotMapped]
        public List<string> UpvotedUsers
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_upvoteUsers))
                    return new List<string>();
                return JsonConvert.DeserializeObject<List<string>>(_upvoteUsers);
            }
            set
            {
                _upvoteUsers = JsonConvert.SerializeObject(value);
            }
        }

        public string _upvoteUsers { get; set; }
    }

}
