using Haverim.Controllers;
using Haverim.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public void CreatePost()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                var PController = new PostsController(db);
                var UController = new UsersController(db);

                var publisher = new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "Test User",
                    DisplayName = "Omer Nahum",
                    Email = "example@mail.com",
                    Password = "123",
                    BirthDateUnix = 959558400,
                    Country = "Israel",
                    IsMale = true,
                    ProfilePic = "Some Url"
                };

                var follower = new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "Test Follower",
                    DisplayName = "Omer Nahum",
                    Email = "example2@mail.com",
                    Password = "123",
                    BirthDateUnix = 959558400,
                    Country = "Israel",
                    IsMale = true,
                    ProfilePic = "Some Url"
                };

                UController.RegisterUser(publisher);
                UController.RegisterUser(follower);

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
                    PublisherUsername = "Test User",
                    Body = "12",
                    Tags = new List<string>() { "Test Follower" }
                };

                Assert.AreEqual(PController.CreatePost(new Controllers.Helpers.ApiClasses.CreatePost()), "error:5");
                Assert.AreEqual(PController.CreatePost(null), "error:5");
                Assert.AreEqual(PController.CreatePost(post), "error:4");
                post.Body = "Testing CreatePost Method!";

                string CreateResult = PController.CreatePost(post);
                string PostId = CreateResult.Split(';')[1];

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
                var PostFromPublisherActivityFeed = db.Posts.Find( PublisherUser.ActivityFeed[0].PostId);
                Assert.IsNotNull(PostFromPublisherActivityFeed);
                Assert.AreEqual(PostFromPublisherActivityFeed.PublisherId, "Test User");

            }
        }

    }
}