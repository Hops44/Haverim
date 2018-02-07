using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haverim.Models
{
    public enum ActivityType
    {
        Reply,Post,Upvote
    }
    public class Activity
    {
        public int Id { get; set; }
        public Guid PostId { get; set; }
        public ActivityType Type { get; set; }
    }
}