using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haverim.Models
{
    public class Comment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public string PublisherId { get; set; }
        public DateTime PublishDate { get; set; }
        public string Body { get; set; }

        public List<User> UpvotedUsers { get; set; } = new List<User>();
       

    }
}