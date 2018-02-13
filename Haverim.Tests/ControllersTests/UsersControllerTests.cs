using Haverim.Controllers;
using Haverim.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Haverim.Tests
{
    [TestClass]
    public class UsersControllerTests
    {
        private UsersController ControllerFactoy(HaverimContext db)
        {
            return new UsersController(db);
        }

        [TestMethod]
        public void IsUsernameTakenTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                db.Users.Add(new User
                {
                    Username = "Taken"
                });
                db.SaveChanges();

                UsersController Controller = ControllerFactoy(db);
                Assert.IsTrue(Controller.IsUsernameTaken("Taken"));         // Username is taken -> the method will return true
                Assert.IsFalse(Controller.IsUsernameTaken("NotTaken"));     // Username is not taken -> the method will return false
            }
        }

        [TestMethod]
        public void RegisterUserTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                UsersController UController = ControllerFactoy(db);
                string RegisterResult = UController.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "Test User",
                    DisplayName = "Some Name",
                    Email = "example@mail.com",
                    Password = "123456",
                    BirthDateUnix = 959558400,
                    Country = "Israel",
                    IsMale = true,
                    ProfilePic = "FCA8DCC2-1B1D-4CC3-82BE-B06B9444328D"
                });

                var User = db.Users.Find("Test User");

                Assert.AreEqual(RegisterResult.Split(':').Length, 2);
                Assert.AreEqual(RegisterResult.Split(':')[0], "success");
                Assert.IsNotNull(User);
                Assert.AreEqual(User.BirthDate, new DateTime(2000, 5, 29));

                Assert.AreEqual(User.PostFeed.Count, 0);
                Assert.AreEqual(User.ActivityFeed.Count, 0);
                Assert.AreEqual(User.Following.Count, 0);
                Assert.AreEqual(User.Followers.Count, 0);

                Assert.AreEqual((DateTime.Now - User.JoinDate).Days, 0);

                var RegisterUserObject = new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "Test User",
                    DisplayName = "Omer Nahum",
                    Email = "example@mail.com",
                    Password = "123456",
                    BirthDateUnix = 959558400,
                    Country = "Israel",
                    IsMale = true,
                    ProfilePic = "FCA8DCC2-1B1D-4CC3-82BE-B06B9444328D"
                };

                RegisterResult = UController.RegisterUser(RegisterUserObject);
                Assert.AreEqual(RegisterResult, "error:2"); // Username in use

                RegisterUserObject.Username = "Test User2";
                RegisterResult = UController.RegisterUser(RegisterUserObject);
                Assert.AreEqual(RegisterResult, "error:3"); // Email in use

                // Missing Parameters
                RegisterUserObject.Email = "NotTaken@mail.com";
                RegisterUserObject.Username = "Test User3";
                RegisterUserObject.DisplayName = null;

                RegisterResult = UController.RegisterUser(RegisterUserObject);
                Assert.AreEqual(RegisterResult, "error:5");

                // Short password ( shorter than 6 characters )
                RegisterUserObject.DisplayName = "Some Name";
                RegisterUserObject.Password = "12345";
                RegisterResult = UController.RegisterUser(RegisterUserObject);
                Assert.AreEqual(RegisterResult, "error:5");
            }
        }

        [TestMethod]
        public void LoginUserTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                var UController = ControllerFactoy(db);
                Global.ResetDatabase(db);
                db.Users.Add(new User
                {
                    Username = "Test User",
                    DisplayName = "Some User",
                    Email = "example@mail.com",
                    Password = "123456",
                    IsMale = true,
                });
                db.SaveChanges();
                var User = new Controllers.Helpers.ApiClasses.LoginUser
                {
                    Username = "Test User",
                    Password = "123456"
                };
                Assert.AreEqual(UController.LoginUser(User).Split(':').Length, 2);
                Assert.AreEqual(UController.LoginUser(User).Split(':')[0], "success");
                Assert.IsTrue(UController.LoginUser(User).Split(':')[1].Length > 1);


                User.Username = "False User";   // False Username
                Assert.IsFalse(UController.LoginUser(User).Split(':')[0] == "error:0");

                User.Username = "Test User";
                User.Password = "123455";   // False Password
                Assert.IsFalse(UController.LoginUser(User).Split(':')[0] == "error:0");


            }
        }

        [TestMethod]
        public void FollowUserTest()
        {
            //TODO:[Future Feature] when two people follow each other they become friends
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                var UController = new UsersController(db);

                string MainUserToken = UController.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "Test User",
                    DisplayName = "Some User",
                    Email = "example@mail.com",
                    Password = "123456",
                    IsMale = true,
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1985, 1, 1)).ToUnixTimeSeconds(),
                    Country = "United States",
                    ProfilePic = "SomeUrl"
                });
                string FollowerUserToken = UController.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "Follower User",
                    DisplayName = "Some User",
                    Email = "example2@mail.com",
                    Password = "123456",
                    IsMale = true,
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1985, 1, 1)).ToUnixTimeSeconds(),
                    Country = "United States",
                    ProfilePic = "SomeUrl"
                });

                Assert.AreEqual(MainUserToken.Split(':')[0], "success");
                Assert.AreEqual(FollowerUserToken.Split(':')[0], "success");

                MainUserToken = MainUserToken.Split(':')[1];
                FollowerUserToken = FollowerUserToken.Split(':')[1];

                var Request = new Controllers.Helpers.ApiClasses.FollowRequest
                {
                    Token = FollowerUserToken,
                    TargetUser = "Test User"
                };

                string FollowRequstResult = UController.FollowUser(Request);
                Assert.AreEqual(FollowRequstResult, "success");

                User MainUser = db.Users.Find("Test User");
                User Follower = db.Users.Find("Follower User");

                Assert.AreEqual(MainUser.Followers.Count, 1);
                Assert.AreEqual(Follower.Following.Count, 1);

                Assert.AreEqual(MainUser.Followers[0], "Follower User");
                Assert.AreEqual(Follower.Following[0], "Test User");

                // Repeat
                FollowRequstResult = UController.FollowUser(Request);
                Assert.AreEqual(FollowRequstResult, "success");

                Assert.AreEqual(MainUser.Followers.Count, 1);
                Assert.AreEqual(Follower.Following.Count, 1);

                Assert.AreEqual(MainUser.Followers[0], "Follower User");
                Assert.AreEqual(Follower.Following[0], "Test User");

                /// Errors tests
                Request.Token += ".";
                FollowRequstResult = UController.FollowUser(Request);
                Assert.AreEqual(FollowRequstResult, "error:6");

                Request.Token = null;
                FollowRequstResult = UController.FollowUser(Request);
                Assert.AreEqual(FollowRequstResult, "error:5");

                Request.Token = MainUserToken;
                Request.TargetUser = "None Existing";
                FollowRequstResult = UController.FollowUser(Request);
                Assert.AreEqual(FollowRequstResult, "error:0");
            }
        }

        [TestMethod]
        public void UnFollowUserTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                // Create 2 users -> 'Test User' and 'Follower User'
                // Follower User will follow Test User
                var UController = new UsersController(db);

                string MainUserToken = UController.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "Test User",
                    DisplayName = "Some User",
                    Email = "example@mail.com",
                    Password = "123456",
                    IsMale = true,
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1985, 1, 1)).ToUnixTimeSeconds(),
                    Country = "United States",
                    ProfilePic = "SomeUrl"
                });
                string FollowerUserToken = UController.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "Follower User",
                    DisplayName = "Some User",
                    Email = "example2@mail.com",
                    Password = "123456",
                    IsMale = true,
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1985, 1, 1)).ToUnixTimeSeconds(),
                    Country = "United States",
                    ProfilePic = "SomeUrl"
                });

                Assert.AreEqual(MainUserToken.Split(':')[0], "success");
                Assert.AreEqual(FollowerUserToken.Split(':')[0], "success");

                MainUserToken = MainUserToken.Split(':')[1];
                FollowerUserToken = FollowerUserToken.Split(':')[1];

                var Request = new Controllers.Helpers.ApiClasses.FollowRequest
                {
                    Token = FollowerUserToken,
                    TargetUser = "Test User"
                };

                string FollowRequstResult = UController.FollowUser(Request);

                User MainUser = db.Users.Find("Test User");
                User Follower = db.Users.Find("Follower User");

                // Make Follower User unfollow Test User
                Request = new Controllers.Helpers.ApiClasses.FollowRequest
                {
                    Token = FollowerUserToken,
                    TargetUser = "Test User"
                };

                string UnFollowRequstResult = UController.UnFollowUser(Request);

                Assert.AreEqual(UnFollowRequstResult, "success");

                Assert.AreEqual(MainUser.Followers.Count, 0);
                Assert.AreEqual(Follower.Following.Count, 0);

                // Repeat
                UnFollowRequstResult = UController.UnFollowUser(Request);

                Assert.AreEqual(UnFollowRequstResult, "success");

                Assert.AreEqual(MainUser.Followers.Count, 0);
                Assert.AreEqual(Follower.Following.Count, 0);

                // error tests
                Request.Token += ".";
                UnFollowRequstResult = UController.UnFollowUser(Request);

                Assert.AreEqual(UnFollowRequstResult, "error:6");

                Request.Token = FollowerUserToken;
                Request.TargetUser = "None Existing";
                UnFollowRequstResult = UController.UnFollowUser(Request);

                Assert.AreEqual(UnFollowRequstResult, "error:0");

                Request.TargetUser = null;
                UnFollowRequstResult = UController.UnFollowUser(Request);

                Assert.AreEqual(UnFollowRequstResult, "error:5");


            }
        }

        [TestMethod]
        public void GetNotificationsTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                var UController = new UsersController(db);
                var PController = new PostsController(db);

                var RegisterUser = new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "Test User",
                    DisplayName = "Some User",
                    Email = "example@mail.com",
                    Password = "123456",
                    IsMale = true,
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1985, 1, 1)).ToUnixTimeSeconds(),
                    Country = "United States",
                    ProfilePic = "SomeUrl"
                };

                string UserToken = UController.RegisterUser(RegisterUser).Split(':')[1] ;

                var NotificationsList = new List<Notification>();
                var User = db.Users.Find("Test User");

                for (int i = 0; i < 17; i++)
                {
                    NotificationsList.Add(new Notification
                    {
                        PostId = Guid.NewGuid(),
                        PublishDate = DateTime.Now,
                        Type = (NotificationType) new Random().Next(0, 4)
                    });
                }







            }


        }


    }
}