using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haverim.Models
{
    public enum NotificationType
    {
        Reply, Tag, UpvotePost, UpvoteComment
    }
    public class Notification
    {
        public int Id { get; set; }
        public string TargetUsername { get; set; }
        public NotificationType Type { get; set; }
        public DateTime PublishDate { get; set; }
        public Guid PostId { get; set; }
    }
}