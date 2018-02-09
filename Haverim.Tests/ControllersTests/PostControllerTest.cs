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
        public void GetPostFeed()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                var UController = new UsersController(db);
                var PController = new PostsController(db);

                string PublisherToken = UController.RegisterUser(new ApiClasses.RegisterUser
                {
                    Username = "Test User",
                    DisplayName = "Publisher",
                    Email = "example@mail.com",
                    Password = "123456",
                    BirthDateUnix = 959558400,
                    Country = "Canada",
                    IsMale = true,
                    ProfilePic = "Some Url"
                });
                string FollowerToken = UController.RegisterUser(new ApiClasses.RegisterUser
                {
                    Username = "Follower User",
                    DisplayName = "Follower",
                    Email = "example2@mail.com",
                    Password = "123456",
                    BirthDateUnix = 959558400,
                    Country = "Canada",
                    IsMale = true,
                    ProfilePic = "Some Url"
                });

                Assert.AreEqual(PublisherToken.Split(':')[0], "success");
                Assert.AreEqual(FollowerToken.Split(':')[0], "success");

                PublisherToken = PublisherToken.Split(':')[1];
                FollowerToken = FollowerToken.Split(':')[1];

                Assert.IsNotNull(db.Users.Find("Test User"));
                Assert.IsNotNull(db.Users.Find("Follower User"));

                // Make the Follower User follow Publisher User
                //TODO: use controller method to follow
                User PublisherUser = db.Users.Find("Test User");
                PublisherUser.Followers = new List<string> { "Follower User" };
                db.Users.Attach(PublisherUser);
                PublisherUser = db.Users.Find("Test User");
                Assert.IsNotNull(PublisherUser);
                Assert.AreEqual(PublisherUser.Followers.Count, 1);


                var PostedIdsStack = new Stack<string>();

                for (int i = 0; i < 17; i++)
                {
                    string Result = PController.CreatePost(new ApiClasses.CreatePost
                    {
                        Token = PublisherToken,
                        Body = "Sample Body",
                        Tags = null
                    });
                    // Result -> "success:PostId"
                    string PostId = Result.Split(':')[1];
                    Result = Result.Split(':')[0];

                    Assert.AreEqual(Result, "success");
                    Assert.AreEqual(PostId.Length, 36);

                    PostedIdsStack.Push(PostId);
                }
                Assert.AreEqual(db.Users.Find("Test User").ActivityFeed.Count, 17);
                Assert.AreEqual(db.Users.Find("Follower User").PostFeed.Count, 17);

                string PostFeedSerialized = PController.GetPostFeed(new ApiClasses.PostFeedRequest
                {
                    index = 0,
                    Token = FollowerToken
                });

                Assert.AreNotEqual(PostFeedSerialized.Split(':')[0], "error");

                var PostFeedStack = new Stack<Post>();
                
                Post[] PostFeedDeSerialized = JsonConvert.DeserializeObject<Post[]>(PostFeedSerialized);
                Assert.AreEqual(PostFeedDeSerialized.Length, 10);

                foreach (var post in PostFeedDeSerialized)
                {
                    PostFeedStack.Push(post);
                }

                PostFeedSerialized = PController.GetPostFeed(new ApiClasses.PostFeedRequest
                {
                    index = 10,
                    Token = FollowerToken
                });

                Assert.AreNotEqual(PostFeedSerialized.Split(':')[0], "error");

                PostFeedDeSerialized = JsonConvert.DeserializeObject<Post[]>(PostFeedSerialized);
                Assert.AreEqual(PostFeedDeSerialized.Length, 7);
                foreach (var post in PostFeedDeSerialized)
                {
                    PostFeedStack.Push(post);
                }

                Assert.AreEqual(PostFeedStack.Count, 17);
                Assert.IsFalse(PostFeedStack.Contains(null));

                var PostFeedClone = new Stack<Post>(new Stack<Post>(PostFeedStack));
                var PostedIdsClone = new Stack<string>(new Stack<string>(PostedIdsStack));


                for (int i = 0; i < PostFeedClone.Count; i++)
                {
                    Post CurrentPost = PostFeedClone.Pop();
                    string CurrentPostId = PostedIdsClone.Pop();

                    Assert.AreEqual(CurrentPost.Id.ToString(), CurrentPostId);
                }

                PostFeedSerialized = PController.GetPostFeed(new ApiClasses.PostFeedRequest
                {
                    index = 12,
                    Token = FollowerToken
                });
                Assert.AreNotEqual(PostFeedSerialized.Split(':')[0], "error");
                PostFeedDeSerialized = JsonConvert.DeserializeObject<Post[]>(PostFeedSerialized);
                Assert.AreEqual(PostFeedDeSerialized.Length, 5);

                PostFeedSerialized = PController.GetPostFeed(new ApiClasses.PostFeedRequest
                {
                    index = 17,
                    Token = FollowerToken
                });
                Assert.AreEqual(PostFeedSerialized, "error:7");

                PostFeedSerialized = PController.GetPostFeed(new ApiClasses.PostFeedRequest
                {
                    Token = FollowerToken+"."
                });
                Assert.AreEqual(PostFeedSerialized, "error:6");

                PostFeedSerialized = PController.GetPostFeed(new ApiClasses.PostFeedRequest
                {
                    Token = Controllers.Helpers.JWT.GetToken(new ApiClasses.Payload { Username="None Existing User"})
                });
                Assert.AreEqual(PostFeedSerialized, "error:0");

            }
        }

    }
}