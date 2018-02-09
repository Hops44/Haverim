using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Haverim.Models;
using Haverim.Controllers.Helpers;
using Newtonsoft.Json;

namespace Haverim.Controllers
{
    [Produces("application/json")]
    [Route("api/Posts")]
    public class PostsController : Controller
    {
        private readonly HaverimContext _context;

        public PostsController(HaverimContext context) => this._context = context;

        [HttpPost("[Action]")]
        public string CreatePost([FromBody]ApiClasses.CreatePost Post)
        {
            if (Post == null || String.IsNullOrWhiteSpace(Post.Token))
                return "error:5";

            string token = Post.Token;
            (Helpers.JWT.TokenStatus Status, ApiClasses.Payload Payload) VerifyResult = Helpers.JWT.VerifyToken(token);

            if (VerifyResult.Status != Helpers.JWT.TokenStatus.Valid)
                return "error:6";

            User Publisher = this._context.Users.Find(VerifyResult.Payload.Username);
            if (Publisher == null)
                return "error:0";
            if (Post.Body.Length < 3)
                return "error:4";

            var PostId = Guid.NewGuid();
            DateTime PublishDate = DateTime.Now;

            // TODO:[Future Feature] Add tagged topics

            // Iterate through tags check if tagged words are users if so, we will add a notification to tagged user
            if (Post.Tags != null)
            {
                foreach (string Tag in Post.Tags)
                {
                    User Tagged = this._context.Users.Find(Tag);
                    if (Tagged != null)
                    {

                        if (Tagged.Notifications == null)
                            Tagged.Notifications = new List<Notification>();

                        Tagged.Notifications.Add(new Notification
                        {
                            PostId = PostId,
                            PublishDate = PublishDate,
                            Type = NotificationType.Tag
                        });
                        this._context.Users.Attach(Tagged);
                    }
                }
            }
            this._context.Posts.Add(new Post
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
                this._context.Users.Attach(Publisher);
            }

            // Add the post activity to the publisher
            Publisher.ActivityFeed.Add(new Activity
            {
                PostId = PostId,
                Type = ActivityType.Post
            });

            // Add the post to the post feed of the publisher's followers
            foreach (string item in Publisher.Followers)
            {
                User Follower = this._context.Users.Find(item);
                var PostFeed = Follower.PostFeed;
                PostFeed.Add(PostId.ToString());
                Follower.PostFeed = PostFeed;
                this._context.Users.Attach(Follower);
            }


            this._context.SaveChanges();
            return "success:" + PostId;
        }
     
        [HttpPost("[Action]")]
        public string GetPostFeed([FromBody] ApiClasses.PostFeedRequest request)
        {
            if (String.IsNullOrWhiteSpace(request.Token))
                return "error:5";
            (Helpers.JWT.TokenStatus Status, ApiClasses.Payload Payload) VerifyResult = Helpers.JWT.VerifyToken(request.Token);

            if (VerifyResult.Status != Helpers.JWT.TokenStatus.Valid)
                return "error:6";
            var User = this._context.Users.Find(VerifyResult.Payload.Username);
            if (User == null)
                return "error:0";

            List<string> UserPostFeed = User.PostFeed;
            int FeedCount = UserPostFeed.Count();
            if (request.index >= FeedCount)
                return "error:7";

            Post[] RequestedPosts;

            if (request.index + 10 >= FeedCount)
            {
                // Loop through the postfeed from index to end
                RequestedPosts = new Post[UserPostFeed.Count - request.index];
                int PostsIndexer = 0;
                for (int i = request.index; i < UserPostFeed.Count; i++)
                {
                    Post CurrentPost = this._context.Posts.Find(Guid.Parse(UserPostFeed[i]));
                    RequestedPosts[PostsIndexer] = CurrentPost;
                    PostsIndexer++;
                }
            }
            else
            {
                // Loop through the postfeed from index to index + 10
                RequestedPosts = new Post[10];
                int PostsIndexer = 0;
                for (int i = request.index; i < request.index + 10; i++)
                {
                    Post CurrentPost = this._context.Posts.Find(Guid.Parse(UserPostFeed[i]));
                    RequestedPosts[PostsIndexer] = CurrentPost;
                    PostsIndexer++;
                }
            }
            return JsonConvert.SerializeObject(RequestedPosts);
        }


    }
}