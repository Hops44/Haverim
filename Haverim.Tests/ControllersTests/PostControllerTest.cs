﻿using Haverim.Controllers;
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
                    Username = "Test User",
                    DisplayName = "Publisher",
                    Email = "example@mail.com",
                    Password = "123456",
                    BirthDateUnix = 959558400,
                    Country = "Canada",
                    IsMale = true,
                    ProfilePic = "Some Url"
                };
                var follower = new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "Test Follower",
                    DisplayName = "Follower",
                    Email = "example2@mail.com",
                    Password = "123456",
                    BirthDateUnix = 959558400,
                    Country = "Canada",
                    IsMale = true,
                    ProfilePic = "Some Url"
                };

                string PublisherToken = UController.RegisterUser(publisher);
                string FollowerToken = UController.RegisterUser(follower);

                Assert.AreNotEqual(PublisherToken.Split(':')[0], "error");
                Assert.AreNotEqual(FollowerToken.Split(':')[0], "error");

                Assert.AreEqual(PublisherToken.Split(':')[0], "success");
                Assert.AreEqual(FollowerToken.Split(':')[0], "success");

                PublisherToken = PublisherToken.Split(':')[1];
                FollowerToken = FollowerToken.Split(':')[1];

                var PublisherUser = db.Users.Find("Test User");
                Assert.IsNotNull(PublisherUser);
                PublisherUser.Followers = new List<string> { "Test Follower" };
                db.SaveChanges();

                var FollowerAccualUser = db.Users.Find("Test Follower");
                Assert.IsNotNull(FollowerAccualUser);
                FollowerAccualUser.Following = new List<string>() { "Test User" };
                db.SaveChanges();

                FollowerAccualUser = db.Users.Find("Test Follower");
                Assert.IsNotNull(FollowerAccualUser);
                Assert.AreEqual(FollowerAccualUser.Following.Count, 1);
                Assert.AreEqual(PublisherUser.Followers.Count, 1);

                var post = new Controllers.Helpers.ApiClasses.CreatePost
                {
                    Token = PublisherToken,
                    Body = "12",
                    Tags = new List<string>() { "Test Follower" }
                };

                Assert.AreEqual(PController.CreatePost(new Controllers.Helpers.ApiClasses.CreatePost()), "error:5");
                Assert.AreEqual(PController.CreatePost(null), "error:5");
                Assert.AreEqual(PController.CreatePost(post), "error:4");
                post.Body = "Testing CreatePost Method!";

                string CreateResult = PController.CreatePost(post);
                string PostId = CreateResult.Split(':')[1];

                Assert.IsTrue(CreateResult.Contains("success"));
                Assert.AreEqual(PostId.Length, 36);

                FollowerAccualUser = db.Users.Find("Test Follower");
                Assert.AreEqual(FollowerAccualUser.Notifications.Count, 1);

                Assert.AreEqual(FollowerAccualUser.PostFeed.Count, 1);

                var PostFromFollowerPostFeed = db.Posts.Find(new Guid(FollowerAccualUser.PostFeed[0]));
                Assert.IsNotNull(PostFromFollowerPostFeed);
                Assert.AreEqual(PostFromFollowerPostFeed.Body, "Testing CreatePost Method!");

                var notification = FollowerAccualUser.Notifications[0];
                Assert.AreEqual(notification.Type, NotificationType.Tag);
                Assert.AreEqual(notification.PostId, new Guid(PostId));

                PublisherUser = db.Users.Find("Test User");
                Assert.AreEqual(PublisherUser.ActivityFeed.Count, 1);
                var PostFromPublisherActivityFeed = db.Posts.Find(PublisherUser.ActivityFeed[0].PostId);
                Assert.IsNotNull(PostFromPublisherActivityFeed);
                Assert.AreEqual(PostFromPublisherActivityFeed.PublisherId, "Test User");

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
                    Username = "Test User",
                    DisplayName = "Publisher",
                    Email = "example@mail.com",
                    Password = "123456",
                    BirthDateUnix = 959558400,
                    Country = "Canada",
                    IsMale = true,
                    ProfilePic = "Some Url"
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
                    TargetUser = "Test User"
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
                var Publisher = db.Users.Find("Test User");
                var Follower = db.Users.Find("Follower User");

                string PostFeedResult = PController.GetPostFeed(new ApiClasses.FeedRequest
                {
                    Token = FollowerToken
                });
                Assert.AreNotEqual(PostFeedResult.Split(':')[0], "error");

                List<Post> PostFeed = JsonConvert.DeserializeObject<Post[]>(PostFeedResult).ToList();

                Assert.AreEqual(PostFeed.Count, 10);

                // Get next 7 posts
                PostFeedResult = PController.GetPostFeed(new ApiClasses.FeedRequest
                {
                    Token = FollowerToken,
                    index = 10
                });
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

                PostFeedResult = PController.GetPostFeed(FeedRequest);
                Assert.AreEqual("error:0", PostFeedResult);

                FeedRequest.Token = null;
                FeedRequest.index = 0;
                PostFeedResult = PController.GetPostFeed(FeedRequest);
                Assert.AreEqual("error:5", PostFeedResult);

                FeedRequest.Token = FollowerToken + ".";
                PostFeedResult = PController.GetPostFeed(FeedRequest);
                Assert.AreEqual("error:6", PostFeedResult);

                FeedRequest.index = 17;
                FeedRequest.Token = FollowerToken;
                PostFeedResult = PController.GetPostFeed(FeedRequest);
                Assert.AreEqual("error:7", PostFeedResult);
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

                string PublisherToken = UController.RegisterUser(new ApiClasses.RegisterUser
                {
                    Username = "PostPublisher",
                    DisplayName = "SomeDisplayName",
                    Email = "example@mail.com",
                    Country = "United States",
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1990, 1, 2)).ToUnixTimeSeconds(),
                    IsMale = false,
                    Password = "123456",
                    ProfilePic = "Url"
                });
                string UserToReplyToken = UController.RegisterUser(new ApiClasses.RegisterUser
                {
                    Username = "UserToReply",
                    DisplayName = "SomeDisplayName",
                    Email = "example2@mail.com",
                    Country = "United States",
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1990, 1, 2)).ToUnixTimeSeconds(),
                    IsMale = false,
                    Password = "123456",
                    ProfilePic = "Url"
                });

                Assert.AreEqual(PublisherToken.Split(':')[0], "success");
                Assert.AreEqual(UserToReplyToken.Split(':')[0], "success");

                PublisherToken = PublisherToken.Split(':')[1];
                UserToReplyToken = UserToReplyToken.Split(':')[1];

                User PostPublisher = db.Users.Find("PostPublisher");
                User ReplyingUser = db.Users.Find("UserToReply");

                Assert.IsNotNull(PostPublisher);
                Assert.IsNotNull(ReplyingUser);

                string PostId = PController.CreatePost(new ApiClasses.CreatePost
                {
                    Token = PublisherToken,
                    Body = "Writing some post body for testing!",
                }).Split(':')[1];

                var Post = db.Posts.Find(Guid.Parse(PostId));

                Assert.AreEqual(PostPublisher.ActivityFeed.Count, 1);

                string ReplyResult = PController.ReplyToPost(new ApiClasses.CreateReply
                {
                    Token = UserToReplyToken,
                    PostId = PostId,
                    Body = "Writing some reply body for testing!"
                });

                Assert.AreEqual(ReplyResult, "success");
                Assert.AreEqual(Post.Comments.Count, 1);
                Assert.AreEqual(Post.Comments[0].PublisherId, ReplyingUser.Username);
                Assert.AreEqual(PostPublisher.Notifications.Count, 1);
                Assert.AreEqual(ReplyingUser.ActivityFeed.Count, 1);
                Assert.AreEqual(ReplyingUser.ActivityFeed[0].Type, ActivityType.Reply);
                Assert.AreEqual(ReplyingUser.ActivityFeed[0].PostId, Guid.Parse(PostId));

                /// Tests that will return error
                // Invalid Token
                var Reply = new ApiClasses.CreateReply
                {
                    Token = UserToReplyToken + ".",
                    Body = "Some Body",
                    PostId = PostId
                };
                ReplyResult = PController.ReplyToPost(Reply);
                Assert.AreEqual(ReplyResult, "error:6");

                // Short body
                Reply.Token = UserToReplyToken;
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
    }
}