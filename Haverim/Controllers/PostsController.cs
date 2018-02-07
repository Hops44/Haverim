using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Haverim.Models;
using Haverim.Controllers.Helpers;

namespace Haverim.Controllers
{
    [Produces("application/json")]
    [Route("api/Posts")]
    public class PostsController : Controller
    {
        private readonly HaverimContext _context;

        public PostsController(HaverimContext context)
        {
            _context = context;
        }

        [HttpPost("[Action]")]
        public string CreatePost([FromBody]ApiClasses.CreatePost Post)
        {
            if (Post == null || Post.PublisherUsername ==null)
                return "error:5";
    
            User Publisher = _context.Users.Find(Post.PublisherUsername);
            if (Publisher == null)
                return "error:0";
            if (Post.Body.Length < 3)
                return "error:4";

            Guid PostId = Guid.NewGuid();
            DateTime PublishDate = DateTime.Now;

            // TODO:[Future Feature] Add tagged topics
          
            // Iterate through tags check if tagged words are users if so, we will add a notification to tagged user
            foreach (var Tag in Post.Tags)
            {
                User Tagged = _context.Users.Find(Tag);
                if (Tagged!=null)
                {

                    if (Tagged.Notifications == null)
                        Tagged.Notifications = new List<Notification>();

                    Tagged.Notifications.Add(new Notification
                    {
                        PostId = PostId,
                        PublishDate = PublishDate,
                        Type = NotificationType.Tag
                    });
                    _context.Users.Attach(Tagged);
                }
            }

            _context.Posts.Add(new Post
            {
                PublisherId = Publisher.Username,
                Id = PostId,
                PublishDate = PublishDate,
                Body = Post.Body,
                Comments=new List<Comment>(),
                UpvotedUsers=new List<string>()
            });

            // If the its the publisher's first post
            // ActivityFeed will be null, so we initialize it
            if (Publisher.ActivityFeed==null)
            {
                Publisher.ActivityFeed = new List<Activity>();
                _context.Users.Attach(Publisher);
            }

            // Add the post activity to the publisher
            Publisher.ActivityFeed.Add(new Activity
            {
                PostId = PostId,
                Type = ActivityType.Post
            });

            // Add the post to the post feed of the publisher's followers
            foreach (var item in Publisher.Followers)
            {
                User Follower = _context.Users.Find(item);
                var PostFeed = Follower.PostFeed;
                PostFeed.Add(PostId.ToString());
                Follower.PostFeed = PostFeed;
                _context.Users.Attach(Follower);
            }


            _context.SaveChanges();
            return "success;" + PostId;
        }
      //TODO: Write Tests for Post controller

    }
}