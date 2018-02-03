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

        public ICollection<IdClass> UpvotedUsers { get; set; } 
    }
    public class IdClass
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
    }
}