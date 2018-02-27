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

            // TODO: [Future Feature] Add tagged topics

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

                        Tagged.Notifications.Insert(0, new Notification
                        {
                            PostId = PostId,
                            PublishDate = PublishDate,
                            Type = NotificationType.Tag,
                            TargetUsername = Publisher.Username
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
                Comments = new List<Comment>(),
                UpvotedUsers = new List<string>()
            });

            // If the its the publisher's first post
            // ActivityFeed will be null, so we initialize it
            var ActivityFeed = Publisher.ActivityFeed;
            if (ActivityFeed == null)
            {
                ActivityFeed = new List<Activity>();
            }

            // Add the post activity to the publisher
            ActivityFeed.Insert(0, new Activity
            {
                PostId = PostId,
                Type = ActivityType.Post
            });
            Publisher.ActivityFeed = ActivityFeed;
            // Add the post to the post feed of the publisher's followers
            foreach (string item in Publisher.Followers)
            {
                User Follower = this._context.Users.Find(item);
                List<string> PostFeed = Follower.PostFeed;
                if (PostFeed == null)
                {
                    PostFeed = new List<string>
                    {
                        PostId.ToString()
                    };
                }
                else
                {
                    PostFeed.Insert(0, PostId.ToString());
                }
                Follower.PostFeed = PostFeed;
            }


            this._context.SaveChanges();
            return "success:" + PostId;
        }

        [HttpPost("[Action]")]
        public string GetPostFeed([FromBody] ApiClasses.FeedRequest request)
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

        [HttpPost("[Action]")]
        public string ReplyToPost([FromBody] ApiClasses.CreateReply reply)
        {
            if (String.IsNullOrWhiteSpace(reply.Token))
                return "error:5";
            (Helpers.JWT.TokenStatus Status, ApiClasses.Payload Payload) = Helpers.JWT.VerifyToken(reply.Token);

            if (Status != Helpers.JWT.TokenStatus.Valid)
                return "error:6";
            var ReplyingUser = this._context.Users.Find(Payload.Username);
            if (ReplyingUser == null)
                return "error:0";

            if (String.IsNullOrEmpty(reply.Body))
                return "error:5";

            if (reply.Body.Length < 3)
                return "error:4";

            var Post = this._context.Posts.Find(Guid.Parse(reply.PostId));
            if (Post == null)
                return "error:1";

            DateTime PublishTime = DateTime.Now;
            var CommentId = Guid.NewGuid();
            Post.Comments.Add(new Comment
            {
                PublisherId = ReplyingUser.Username,
                Id = CommentId,
                Body = reply.Body,
                PublishDate = PublishTime,
                UpvotedUsers = new List<string>()
            });
            this._context.Posts.Attach(Post);
            User PostPublisher = this._context.Users.Find(Post.PublisherId);
            if (PostPublisher.Notifications == null)
                PostPublisher.Notifications = new List<Notification>();

            PostPublisher.Notifications.Insert(0, new Notification
            {
                PostId = Post.Id,
                PublishDate = PublishTime,
                Type = NotificationType.Reply,
                TargetUsername= Payload.Username
            });

            if (ReplyingUser.ActivityFeed == null)
                ReplyingUser.ActivityFeed = new List<Activity>();

            ReplyingUser.ActivityFeed.Insert(0, new Activity
            {
                PostId = Post.Id,
                Type = ActivityType.Reply
            });

            //this._context.Users.Attach(PostPublisher);
            this._context.SaveChanges();

            return $"success:{CommentId}";
        }

        [HttpPost("[Action]")]
        public string UpvotePost([FromBody] ApiClasses.UpvoteRequest request)
        {
            if (String.IsNullOrWhiteSpace(request.Token) || String.IsNullOrWhiteSpace(request.PostId))
                return "error:5";

            (Helpers.JWT.TokenStatus Status, ApiClasses.Payload Payload) = Helpers.JWT.VerifyToken(request.Token);
            if (Status != Helpers.JWT.TokenStatus.Valid)
                return "error:6";

            var PostUpvoter = this._context.Users.Find(Payload.Username);
            if (PostUpvoter == null)
                return "error:0";

            var Post = this._context.Posts.Find(Guid.Parse(request.PostId));
            if (Post == null)
                return "error:1";

            if (Post.UpvotedUsers.Contains(PostUpvoter.Username))
                return "success";

            List<string> UpvotedUsers = Post.UpvotedUsers;
            UpvotedUsers.Add(PostUpvoter.Username);
            Post.UpvotedUsers = UpvotedUsers;

            PostUpvoter.ActivityFeed.Insert(0, new Activity
            {
                PostId = Post.Id,
                Type = ActivityType.Upvote
            });

            var PostPublisher = this._context.Users.Find(Post.PublisherId);
            if (PostPublisher.Notifications == null)
            {
                PostPublisher.Notifications = new List<Notification>
                {
                    new Notification
                    {
                        PostId = Post.Id,
                        PublishDate = DateTime.Now,
                        Type = NotificationType.UpvotePost,
                        TargetUsername = PostUpvoter.Username
                    }
                };
            }
            else
            {
                PostPublisher.Notifications.Insert(0, new Notification
                {
                    PostId = Post.Id,
                    PublishDate = DateTime.Now,
                    Type = NotificationType.UpvotePost,
                    TargetUsername = PostUpvoter.Username
                });
            }
            this._context.SaveChanges();
            return "success";
        }

        [HttpPost("[Action]")]
        public string RemoveUpvoteFromPost([FromBody] ApiClasses.UpvoteRequest request)
        {
            if (String.IsNullOrWhiteSpace(request.Token) || String.IsNullOrWhiteSpace(request.PostId))
                return "error:5";

            (Helpers.JWT.TokenStatus Status, ApiClasses.Payload Payload) = Helpers.JWT.VerifyToken(request.Token);
            if (Status != Helpers.JWT.TokenStatus.Valid)
                return "error:6";

            var PostUpvoter = this._context.Users.Find(Payload.Username);
            if (PostUpvoter == null)
                return "error:0";

            var Post = this._context.Posts.Find(Guid.Parse(request.PostId));
            if (Post == null)
                return "error:1";

            if (!Post.UpvotedUsers.Contains(PostUpvoter.Username))
                return "success";

            List<string> UpvotedUser = Post.UpvotedUsers;
            UpvotedUser.Remove(Payload.Username);
            Post.UpvotedUsers = UpvotedUser;

            this._context.SaveChanges();

            return "success";

        }

        [HttpPost("[Action]")]
        public string UpvoteComment([FromBody] ApiClasses.CommentUpvoteRequest request)
        {
            if (String.IsNullOrWhiteSpace(request.Token) || String.IsNullOrWhiteSpace(request.PostId) || String.IsNullOrWhiteSpace(request.CommentId))
                return "error:5";

            (Helpers.JWT.TokenStatus Status, ApiClasses.Payload Payload) = Helpers.JWT.VerifyToken(request.Token);
            if (Status != Helpers.JWT.TokenStatus.Valid)
                return "error:6";

            var CommentUpvoter = this._context.Users.Find(Payload.Username);
            if (CommentUpvoter == null)
                return "error:0";

            var Post = this._context.Posts.Find(Guid.Parse(request.PostId));
            if (Post == null)
                return "error:1";

            if (Post.Comments == null)
                return "error:8";

            var TargetComment = Post.Comments.SingleOrDefault(x => x.Id == Guid.Parse(request.CommentId));
            if (TargetComment == null)
                return "error:8";

            List<string> UpvotedUsers = TargetComment.UpvotedUsers;
            if (UpvotedUsers == null)
                UpvotedUsers = new List<string> { Payload.Username };
            else if (UpvotedUsers.Contains(Payload.Username))
                return "success";
            else
                UpvotedUsers.Add(Payload.Username);

            TargetComment.UpvotedUsers = UpvotedUsers;

            var CommentPublisher = this._context.Users.Find(TargetComment.PublisherId);

            var NotificationToInsert = new Notification
            {
                PostId = Post.Id,
                PublishDate = DateTime.Now,
                Type = NotificationType.UpvoteComment,
                TargetUsername= Payload.Username
            };
            var Notifications = CommentPublisher.Notifications;
            if (Notifications == null)
                Notifications = new List<Notification> { NotificationToInsert };
            else
                Notifications.Insert(0,NotificationToInsert);
            CommentPublisher.Notifications = Notifications;

            this._context.SaveChanges();
            return "success";
        }

    }
}