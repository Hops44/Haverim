using Haverim.Controllers;
using Haverim.Controllers.Helpers;
using Haverim.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Haverim.Tests.ControllersTests
{
    [TestClass]
    public class PostControllerTest
    {
        /// <summary>
        // * The function will receive a CreatePost object named post
        // * The function will check the following:
        //     - none of the post field is null except for tags
        //     - post's publisher id is an existing user's id
        //     - post's body is atleast 3 chars
        // * the function will iterate through the tags list, if a tag is a user, it will add a notification to his notification list
        // * the function will iterate through the publisher's followers list and add the post to their post feed
        // * the function will add the post to the publisher's activity feed
        /// </summary>
        /// 
        [TestMethod]
        public void CreatePostTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                var PController = new PostsController(db);
                var UController = new UsersController(db);

                var publisher = new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "test user",
                    DisplayName = "Publisher",
                    Email = "example@mail.com",
                    Password = "123456",
                    BirthDateUnix = 959558400,
                    Country = "Canada",
                    IsMale = true,
                    ProfilePicBase64 = "Some Url"
                };
                var follower = new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "test follower",
                    DisplayName = "Follower",
                    Email = "example2@mail.com",
                    Password = "123456",
                    BirthDateUnix = 959558400,
                    Country = "Canada",
                    IsMale = true,
                    ProfilePicBase64 = "Some Url"
                };

                string PublisherToken = UController.RegisterUser(publisher);
                string FollowerToken = UController.RegisterUser(follower);

                Assert.AreNotEqual(PublisherToken.Split(':')[0], "error");
                Assert.AreNotEqual(FollowerToken.Split(':')[0], "error");

                Assert.AreEqual(PublisherToken.Split(':')[0], "success");
                Assert.AreEqual(FollowerToken.Split(':')[0], "success");

                PublisherToken = PublisherToken.Split(':')[1];
                FollowerToken = FollowerToken.Split(':')[1];

                var PublisherUser = db.Users.Find("test user");
                Assert.IsNotNull(PublisherUser);
                PublisherUser.Followers = new List<string> { "test follower" };
                db.SaveChanges();

                var FollowerAccualUser = db.Users.Find("test follower");
                Assert.IsNotNull(FollowerAccualUser);
                FollowerAccualUser.Following = new List<string>() { "test user" };
                db.SaveChanges();

                FollowerAccualUser = db.Users.Find("test follower");
                Assert.IsNotNull(FollowerAccualUser);
                Assert.AreEqual(FollowerAccualUser.Following.Count, 1);
                Assert.AreEqual(PublisherUser.Followers.Count, 1);

                var post = new ApiClasses.CreatePost
                {
                    Token = PublisherToken,
                    Body = "12",
                    Tags = new List<string>() { "test follower" }
                };

                Assert.AreEqual(PController.CreatePost(new Controllers.Helpers.ApiClasses.CreatePost()), "error:5");
                Assert.AreEqual(PController.CreatePost(null), "error:5");
                Assert.AreEqual(PController.CreatePost(post), "error:4");
                post.Body = "Testing CreatePost Method!";

                string CreateResult = PController.CreatePost(post);
                string PostId = CreateResult.Split(':')[1];

                Assert.IsTrue(CreateResult.Contains("success"));
                Assert.AreEqual(PostId.Length, 36);

                FollowerAccualUser = db.Users.Find("test follower");
                Assert.AreEqual(FollowerAccualUser.Notifications.Count, 1);

                Assert.AreEqual(FollowerAccualUser.PostFeed.Count, 1);

                var PostFromFollowerPostFeed = db.Posts.Find(new Guid(FollowerAccualUser.PostFeed[0]));
                Assert.IsNotNull(PostFromFollowerPostFeed);
                Assert.AreEqual(PostFromFollowerPostFeed.Body, "Testing CreatePost Method!");

                var notification = FollowerAccualUser.Notifications[0];
                Assert.AreEqual(notification.Type, NotificationType.Tag);
                Assert.AreEqual(notification.PostId, new Guid(PostId));

                PublisherUser = db.Users.Find("test user");
                Assert.AreEqual(PublisherUser.ActivityFeed.Count, 1);
                var PostFromPublisherActivityFeed = db.Posts.Find(PublisherUser.ActivityFeed[0].PostId);
                Assert.IsNotNull(PostFromPublisherActivityFeed);
                Assert.AreEqual(PostFromPublisherActivityFeed.PublisherId, "test user");

            }
        }

        [TestMethod]
        public void GetPostFeedTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                var UController = new UsersController(db);
                var PController = new PostsController(db);

                var RegisterUserInstance = new ApiClasses.RegisterUser
                {
                    Username = "test user",
                    DisplayName = "Publisher",
                    Email = "example@mail.com",
                    Password = "123456",
                    BirthDateUnix = 959558400,
                    Country = "Canada",
                    IsMale = true,
                    ProfilePicBase64 = "Some Url"
                };
                // Publisher
                string PublisherToken = UController.RegisterUser(RegisterUserInstance).Split(':')[1];
                // Follower
                RegisterUserInstance.Username = "Follower User";
                RegisterUserInstance.Email += "m";
                string FollowerToken = UController.RegisterUser(RegisterUserInstance).Split(':')[1];

                // Follow the publisher
                UController.FollowUser(new ApiClasses.FollowRequest
                {
                    Token = FollowerToken,
                    TargetUser = "test user"
                });

                // Post 17 Posts with a body of its posted index (i)
                for (int i = 1; i <= 17; i++)
                {
                    string PostResult = PController.CreatePost(new ApiClasses.CreatePost
                    {
                        Token = PublisherToken,
                        Body = $"PostIndex:{i}",
                        Tags = null
                    });
                }
                var Publisher = db.Users.Find("test user");
                var Follower = db.Users.Find("Follower User");

                string PostFeedResult = JsonConvert.SerializeObject(PController.GetPostFeed(new ApiClasses.FeedRequest
                {
                    Token = FollowerToken
                }).Value);
                Assert.AreNotEqual(PostFeedResult.Split(':')[0], "error");

                List<Post> PostFeed = JsonConvert.DeserializeObject<Post[]>(PostFeedResult).ToList();

                Assert.AreEqual(PostFeed.Count, 10);

                // Get next 7 posts
                PostFeedResult = JsonConvert.SerializeObject(PController.GetPostFeed(new ApiClasses.FeedRequest
                {
                    Token = FollowerToken,
                    index = 10
                }).Value);
                List<Post> TempPostFeed = JsonConvert.DeserializeObject<Post[]>(PostFeedResult).ToList();
                Assert.AreEqual(TempPostFeed.Count, 7);

                PostFeed.AddRange(TempPostFeed);

                // Check for correct feed 
                int BodyNumber = 17;
                for (int i = 0; i < 17; i++)
                {
                    string CurrentPostBody = PostFeed[i].Body;
                    int PostBodyNumber = int.Parse(CurrentPostBody.Split(':')[1]);
                    Assert.AreEqual(BodyNumber, PostBodyNumber);
                    BodyNumber--;
                }

                // Error tests
                string NonExistingUserToken = Controllers.Helpers.JWT.GetToken(new ApiClasses.Payload
                {
                    Username = "None Existing"
                });
                var FeedRequest = new ApiClasses.FeedRequest { Token = NonExistingUserToken };

                PostFeedResult = JsonConvert.SerializeObject(PController.GetPostFeed(FeedRequest).Value);
                Assert.AreEqual("\"error:0\"", PostFeedResult);

                FeedRequest.Token = null;
                FeedRequest.index = 0;
                PostFeedResult = JsonConvert.SerializeObject(PController.GetPostFeed(FeedRequest).Value);
                Assert.AreEqual("\"error:5\"", PostFeedResult);

                FeedRequest.Token = FollowerToken + ".";
                PostFeedResult = JsonConvert.SerializeObject(PController.GetPostFeed(FeedRequest).Value);
                Assert.AreEqual("\"error:6\"", PostFeedResult);

                FeedRequest.index = 17;
                FeedRequest.Token = FollowerToken;
                PostFeedResult = JsonConvert.SerializeObject(PController.GetPostFeed(FeedRequest).Value);
                Assert.AreEqual("\"error:7\"", PostFeedResult);
            }
        }

        [TestMethod]
        public void ReplyToPostTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                var UController = new UsersController(db);
                var PController = new PostsController(db);

                var RegisterUser = new ApiClasses.RegisterUser
                {
                    Username = "post publisher",
                    DisplayName = "SomeDisplayName",
                    Email = "example@mail.com",
                    Country = "United States",
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1990, 1, 2)).ToUnixTimeSeconds(),
                    IsMale = false,
                    Password = "123456",
                    ProfilePicBase64 = "Url"
                };
                string PublisherToken = UController.RegisterUser(RegisterUser);
                RegisterUser.Username = "usertoreply"; RegisterUser.Email += "m";
                string usertoreplyToken = UController.RegisterUser(RegisterUser);

                PublisherToken = PublisherToken.Split(':')[1];
                usertoreplyToken = usertoreplyToken.Split(':')[1];

                User PostPublisher = db.Users.Find("post publisher");
                User ReplyingUser = db.Users.Find("usertoreply");

                string PostId = PController.CreatePost(new ApiClasses.CreatePost
                {
                    Token = PublisherToken,
                    Body = "Writing some post body for testing!",
                }).Split(':')[1];

                var Post = db.Posts.Find(Guid.Parse(PostId));

                string ReplyResult = PController.ReplyToPost(new ApiClasses.CreateReply
                {
                    Token = usertoreplyToken,
                    PostId = PostId,
                    Body = "Writing some reply body for testing!"
                });

                Assert.AreEqual(ReplyResult.Split(':')[0], "success");
                Assert.AreEqual(Post.Comments.Count, 1);
                Assert.AreEqual(Post.Comments[0].PublisherId, ReplyingUser.Username);
                Assert.AreEqual(PostPublisher.Notifications.Count, 1);
                Assert.AreEqual(ReplyingUser.ActivityFeed.Count, 1);
                Assert.AreEqual(ReplyingUser.ActivityFeed[0].Type, ActivityType.Reply);
                Assert.AreEqual(ReplyingUser.ActivityFeed[0].PostId, Guid.Parse(PostId));
                Assert.AreEqual(1, PostPublisher.Notifications.Count);
                Assert.AreEqual("usertoreply", PostPublisher.Notifications[0].TargetUsername);

                // Reply From Another User
                RegisterUser.Username = "Second User"; RegisterUser.Email += "m";
                string SecondUserToken = UController.RegisterUser(RegisterUser).Split(':')[1];

                ReplyResult = PController.ReplyToPost(new ApiClasses.CreateReply
                {
                    Body = "Another Reply",
                    PostId = PostId,
                    Token = SecondUserToken
                });
                Assert.AreEqual("success", ReplyResult.Split(':')[0]);
                Assert.AreEqual(2, PostPublisher.Notifications.Count);
                Assert.AreEqual("second user", PostPublisher.Notifications[0].TargetUsername);

                /// Tests that will return error
                // Invalid Token
                var Reply = new ApiClasses.CreateReply
                {
                    Token = usertoreplyToken + ".",
                    Body = "Some Body",
                    PostId = PostId
                };
                ReplyResult = PController.ReplyToPost(Reply);
                Assert.AreEqual(ReplyResult, "error:6");

                // Short body
                Reply.Token = usertoreplyToken;
                Reply.Body = "12";
                ReplyResult = PController.ReplyToPost(Reply);
                Assert.AreEqual(ReplyResult, "error:4");

                // None existing postid
                Reply.Body = "Some Body";
                Reply.PostId = Guid.NewGuid().ToString();
                ReplyResult = PController.ReplyToPost(Reply);
                Assert.AreEqual(ReplyResult, "error:1");

                // None existing username
                Reply.PostId = Post.Id.ToString();
                Reply.Token = Controllers.Helpers.JWT.GetToken(new ApiClasses.Payload
                {
                    Username = "None Existing"
                });
                ReplyResult = PController.ReplyToPost(Reply);
                Assert.AreEqual(ReplyResult, "error:0");

                // Blank body
                Reply.Body = null;
                Reply.Token = PublisherToken;
                ReplyResult = PController.ReplyToPost(Reply);
                Assert.AreEqual(ReplyResult, "error:5");

            }

        }

        [TestMethod]
        public void UpvotePostTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                var UController = new UsersController(db);
                var PController = new PostsController(db);

                // Regiser post publisher and another user which will upvote the publisher's post
                var RegisterRequest = new ApiClasses.RegisterUser
                {
                    Username = "post publisher",
                    DisplayName = "SomeDisplayName",
                    Email = "example@mail.com",
                    Country = "United States",
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1990, 1, 2)).ToUnixTimeSeconds(),
                    IsMale = false,
                    Password = "123456",
                    ProfilePicBase64 = "Url"
                };
                string PublisherToken = UController.RegisterUser(RegisterRequest);
                RegisterRequest.Username = "upvote user";
                RegisterRequest.Email += "m";
                string UpvoteUserToken = UController.RegisterUser(RegisterRequest);

                PublisherToken = PublisherToken.Split(':')[1];
                UpvoteUserToken = UpvoteUserToken.Split(':')[1];

                // Create post
                string PostId = PController.CreatePost(new ApiClasses.CreatePost
                {
                    Token = PublisherToken,
                    Body = "This post will get we some upvotes",
                    Tags = null
                });
                PostId = PostId.Split(':')[1];

                string UpvoteResult = PController.UpvotePost(new ApiClasses.UpvoteRequest
                {
                    Token = UpvoteUserToken,
                    PostId = PostId
                });
                // Repeat
                UpvoteResult = PController.UpvotePost(new ApiClasses.UpvoteRequest
                {
                    Token = UpvoteUserToken,
                    PostId = PostId
                });
                Assert.AreEqual("success", UpvoteResult);

                var Post = db.Posts.Find(Guid.Parse(PostId));
                var Publisher = db.Users.Find("post publisher");
                var UpvoteUser = db.Users.Find("upvote user");

                Assert.AreEqual(1, Post.UpvotedUsers.Count);
                Assert.AreEqual("upvote user", Post.UpvotedUsers[0]);

                Assert.AreEqual(1, Publisher.Notifications.Count);
                Assert.AreEqual(NotificationType.UpvotePost, Publisher.Notifications[0].Type);

                Assert.AreEqual(1, UpvoteUser.ActivityFeed.Count);
                Assert.AreEqual(ActivityType.Upvote, UpvoteUser.ActivityFeed[0].Type);

                // Error tests

                string NonExistingUserToken = Controllers.Helpers.JWT.GetToken(new ApiClasses.Payload { Username = "None" });

                var UpvoteRequest = new ApiClasses.UpvoteRequest
                {
                    Token = NonExistingUserToken,
                    PostId = PostId
                };
                UpvoteResult = PController.UpvotePost(UpvoteRequest);
                Assert.AreEqual("error:0", UpvoteResult);

                UpvoteRequest.Token = UpvoteUserToken;
                UpvoteRequest.PostId = Guid.NewGuid().ToString();
                UpvoteResult = PController.UpvotePost(UpvoteRequest);
                Assert.AreEqual("error:1", UpvoteResult);

                UpvoteRequest.Token = " ";
                UpvoteRequest.PostId = null;
                UpvoteResult = PController.UpvotePost(UpvoteRequest);
                Assert.AreEqual("error:5", UpvoteResult);

                UpvoteRequest.Token = UpvoteUserToken + ".";
                UpvoteRequest.PostId = PostId;
                UpvoteResult = PController.UpvotePost(UpvoteRequest);
                Assert.AreEqual("error:6", UpvoteResult);
            }
        }

        [TestMethod]
        public void RemoveUpvoteFromPostTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                var UController = new UsersController(db);
                var PController = new PostsController(db);

                // Regiser post publisher and another user which will upvote the publisher's post
                var RegisterRequest = new ApiClasses.RegisterUser
                {
                    Username = "post publisher",
                    DisplayName = "SomeDisplayName",
                    Email = "example@mail.com",
                    Country = "United States",
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1990, 1, 2)).ToUnixTimeSeconds(),
                    IsMale = false,
                    Password = "123456",
                    ProfilePicBase64 = "Url"
                };
                string PublisherToken = UController.RegisterUser(RegisterRequest);
                RegisterRequest.Username = "upvote user";
                RegisterRequest.Email += "m";
                string UpvoteUserToken = UController.RegisterUser(RegisterRequest);

                PublisherToken = PublisherToken.Split(':')[1];
                UpvoteUserToken = UpvoteUserToken.Split(':')[1];

                // Create post
                string PostId = PController.CreatePost(new ApiClasses.CreatePost
                {
                    Token = PublisherToken,
                    Body = "This post will get we some upvotes",
                    Tags = null
                });
                PostId = PostId.Split(':')[1];

                string UpvoteResult = PController.UpvotePost(new ApiClasses.UpvoteRequest
                {
                    Token = UpvoteUserToken,
                    PostId = PostId
                });

                var Post = db.Posts.Find(Guid.Parse(PostId));
                var Publisher = db.Users.Find("post publisher");
                var UpvoteUser = db.Users.Find("upvote user");

                Assert.AreEqual(1, Post.UpvotedUsers.Count);

                string RemoveUpvoteResult = PController.RemoveUpvoteFromPost(new ApiClasses.UpvoteRequest
                {
                    PostId = PostId,
                    Token = UpvoteUserToken
                });
                Assert.AreEqual("success", RemoveUpvoteResult);
                Assert.AreEqual(0, Post.UpvotedUsers.Count);
                // Repeat
                RemoveUpvoteResult = PController.RemoveUpvoteFromPost(new ApiClasses.UpvoteRequest
                {
                    PostId = PostId,
                    Token = UpvoteUserToken
                });
                Assert.AreEqual("success", RemoveUpvoteResult);
                Assert.AreEqual(0, Post.UpvotedUsers.Count);


                // Error tests

                string NonExistingUserToken = Controllers.Helpers.JWT.GetToken(new ApiClasses.Payload { Username = "None" });

                var UpvoteRequest = new ApiClasses.UpvoteRequest
                {
                    Token = NonExistingUserToken,
                    PostId = PostId
                };
                UpvoteResult = PController.RemoveUpvoteFromPost(UpvoteRequest);
                Assert.AreEqual("error:0", UpvoteResult);

                UpvoteRequest.Token = UpvoteUserToken;
                UpvoteRequest.PostId = Guid.NewGuid().ToString();
                UpvoteResult = PController.RemoveUpvoteFromPost(UpvoteRequest);
                Assert.AreEqual("error:1", UpvoteResult);

                UpvoteRequest.Token = " ";
                UpvoteRequest.PostId = null;
                UpvoteResult = PController.RemoveUpvoteFromPost(UpvoteRequest);
                Assert.AreEqual("error:5", UpvoteResult);

                UpvoteRequest.Token = UpvoteUserToken + ".";
                UpvoteRequest.PostId = PostId;
                UpvoteResult = PController.RemoveUpvoteFromPost(UpvoteRequest);
                Assert.AreEqual("error:6", UpvoteResult);
            }
        }

        [TestMethod]
        public void UpvoteCommentTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                var UController = new UsersController(db);
                var PController = new PostsController(db);

                var RegisterUser = new ApiClasses.RegisterUser
                {
                    Username = "post publisher",
                    DisplayName = "SomeDisplayName",
                    Email = "example@mail.com",
                    Country = "United States",
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1990, 1, 2)).ToUnixTimeSeconds(),
                    IsMale = false,
                    Password = "123456",
                    ProfilePicBase64 = "Url"
                };
                string PostPublisherToken = UController.RegisterUser(RegisterUser).Split(':')[1];
                RegisterUser.Username = "reply user"; RegisterUser.Email += "m";
                string ReplyUserToken = UController.RegisterUser(RegisterUser).Split(':')[1];

                string PostId = PController.CreatePost(new ApiClasses.CreatePost
                {
                    Body = "This post will get me some upvote for sure",
                    Token = PostPublisherToken,
                    Tags = null
                }).Split(':')[1];

                var Post = db.Posts.Find(Guid.Parse(PostId));
                var PostPublisher = db.Users.Find("post publisher");
                var ReplyUser = db.Users.Find("reply user");

                string CommentId = PController.ReplyToPost(new ApiClasses.CreateReply
                {
                    Token = ReplyUserToken,
                    Body = "Some Comment Body",
                    PostId = PostId
                }).Split(':')[1];

                string UpvoteResult;
                // Upvote Twice
                for (int i = 0; i < 2; i++)
                {
                    UpvoteResult = PController.UpvoteComment(new ApiClasses.CommentUpvoteRequest
                    {
                        Token = PostPublisherToken,
                        PostId = PostId,
                        CommentId = CommentId
                    });
                    Assert.AreEqual("success", UpvoteResult);
                    Assert.AreEqual(1, Post.Comments[0].UpvotedUsers.Count);
                    Assert.AreEqual(1, ReplyUser.Notifications.Count);
                    Assert.AreEqual("post publisher", ReplyUser.Notifications[0].TargetUsername);
                }

                // Upvote from another user
                RegisterUser.Username = "second user"; RegisterUser.Email += "m";
                string SecondUserToken = UController.RegisterUser(RegisterUser).Split(':')[1];
                UpvoteResult = PController.UpvoteComment(new ApiClasses.CommentUpvoteRequest
                {
                    Token = SecondUserToken,
                    PostId = PostId,
                    CommentId = CommentId
                });
                Assert.AreEqual("success", UpvoteResult);
                Assert.AreEqual(2, Post.Comments[0].UpvotedUsers.Count);
                Assert.AreEqual(2, ReplyUser.Notifications.Count);
                Assert.AreEqual("second user", ReplyUser.Notifications[0].TargetUsername);
                Assert.AreEqual("post publisher", ReplyUser.Notifications[1].TargetUsername);

                /// Error tests

                // Reset Database and re-register users
                Global.ResetDatabase(db);
            }
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                var RegisterUser = new ApiClasses.RegisterUser
                {
                    Username = "post publisher",
                    DisplayName = "SomeDisplayName",
                    Email = "example@mail.com",
                    Country = "United States",
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1990, 1, 2)).ToUnixTimeSeconds(),
                    IsMale = false,
                    Password = "123456",
                    ProfilePicBase64 = "Url"
                };

                var UController = new UsersController(db);
                var PController = new PostsController(db);

                string PostPublisherToken = UController.RegisterUser(RegisterUser).Split(':')[1];
                RegisterUser.Username = "reply user2"; RegisterUser.Email += "mm";
                string ReplyUserToken = UController.RegisterUser(RegisterUser).Split(':')[1];

                string PostId = PController.CreatePost(new ApiClasses.CreatePost
                {
                    Token = PostPublisherToken,
                    Body = "Another Post"
                });
                PostId = PostId.Split(':')[1];

                string CommentId = PController.ReplyToPost(new ApiClasses.CreateReply
                {
                    Token = ReplyUserToken,
                    Body = "Some Reply",
                    PostId = PostId
                }).Split(':')[1];

                var Request = new ApiClasses.CommentUpvoteRequest
                {
                    Token = PostPublisherToken,
                    CommentId = CommentId,
                    PostId = PostId
                };

                // False Token
                Request.Token += ".";
                string UpvoteResult = PController.UpvoteComment(Request);
                Assert.AreEqual("error:6", UpvoteResult);

                // Non-existing comment 
                Request.CommentId = Guid.NewGuid().ToString();
                Request.Token = PostPublisherToken;
                UpvoteResult = PController.UpvoteComment(Request);
                Assert.AreEqual("error:8", UpvoteResult);

                // Non-existing post
                Request.PostId = Guid.NewGuid().ToString();
                Request.CommentId = CommentId;
                UpvoteResult = PController.UpvoteComment(Request);
                Assert.AreEqual("error:1", UpvoteResult);

                // Missing Arguments
                UpvoteResult = PController.UpvoteComment(new ApiClasses.CommentUpvoteRequest
                {
                    CommentId = null,
                    PostId = null,
                    Token = null
                });
                Assert.AreEqual("error:5", UpvoteResult);
            }
        }

        [TestMethod]
        public void RemoveUpvoteFromCommentTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                var UController = new UsersController(db);
                var PController = new PostsController(db);

                var RegisterUser = new ApiClasses.RegisterUser
                {
                    Username = "post publisher",
                    DisplayName = "SomeDisplayName",
                    Email = "example@mail.com",
                    Country = "United States",
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1990, 1, 2)).ToUnixTimeSeconds(),
                    IsMale = false,
                    Password = "123456",
                    ProfilePicBase64 = "Url"
                };
                string PostPublisherToken = UController.RegisterUser(RegisterUser).Split(':')[1];
                RegisterUser.Username = "reply user"; RegisterUser.Email += "m";
                string ReplyUserToken = UController.RegisterUser(RegisterUser).Split(':')[1];

                string PostId = PController.CreatePost(new ApiClasses.CreatePost
                {
                    Body = "This post will get me some upvote for sure",
                    Token = PostPublisherToken,
                    Tags = null
                }).Split(':')[1];

                var Post = db.Posts.Find(Guid.Parse(PostId));
                var PostPublisher = db.Users.Find("post publisher");
                var ReplyUser = db.Users.Find("reply user");

                string CommentId = PController.ReplyToPost(new ApiClasses.CreateReply
                {
                    Token = ReplyUserToken,
                    Body = "Some Comment Body",
                    PostId = PostId
                }).Split(':')[1];

                // Upvote
                string UpvoteResult = PController.UpvoteComment(new ApiClasses.CommentUpvoteRequest
                {
                    Token = PostPublisherToken,
                    PostId = PostId,
                    CommentId = CommentId
                });

                // Remove upvote twice
                for (int i = 0; i < 2; i++)
                {
                    string RemoveResult = PController.RemoveUpvoteFromComment(new ApiClasses.CommentUpvoteRequest
                    {
                        Token = PostPublisherToken,
                        PostId = PostId,
                        CommentId = CommentId
                    });

                    Assert.AreEqual("success", RemoveResult);
                    Assert.AreEqual(0, Post.Comments[0].UpvotedUsers.Count);
                }
            }
        }

        [TestMethod]
        public void GetUserActivityFeedTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                var UController = new UsersController(db);
                var PController = new PostsController(db);

                var register_user = new ApiClasses.RegisterUser
                {
                    BirthDateUnix = 630720000,
                    Country = "United States",
                    DisplayName = "Test User",
                    Email = "mail@email.com",
                    IsMale = true,
                    Password = "123456",
                    Username = "TestUser"
                };
                var TestUserToken = UController.RegisterUser(register_user).Split(':')[1];

                register_user.Username = "SecondUser";
                register_user.Email = "mail2@mail.com";

                var SecondUserToken = UController.RegisterUser(register_user).Split(':')[1];

                // Create 12 posts
                for (int i = 0; i < 12; i++)
                {
                    PController.CreatePost(new ApiClasses.CreatePost
                    {
                        Body = "Some Text",
                        Token = TestUserToken
                    });
                }

                PController.CreatePost(new ApiClasses.CreatePost
                {
                    Body = "Some Text",
                    Token = SecondUserToken
                });

                // Get first 10
                string ActivityFeedSerialized = JsonConvert.SerializeObject(PController.GetUserActivityFeed(new ApiClasses.ActivityFeedRequest
                {
                    TargetUser="testuser",
                    index = 0,
                    Token = TestUserToken
                }).Value);

                var ActivityFeed = JsonConvert.DeserializeObject<List<ApiClasses.ActivityFeedItem>>(ActivityFeedSerialized);

                Assert.AreEqual(10, ActivityFeed.Count);

                // Get last 2
                ActivityFeedSerialized = JsonConvert.SerializeObject(PController.GetUserActivityFeed(new ApiClasses.ActivityFeedRequest
                {
                    TargetUser="testuser",
                    index = 10,
                    Token = TestUserToken
                }).Value);

                ActivityFeed = JsonConvert.DeserializeObject<List<ApiClasses.ActivityFeedItem>>(ActivityFeedSerialized);

                Assert.AreEqual(2, ActivityFeed.Count);
            }
        }
    }
}