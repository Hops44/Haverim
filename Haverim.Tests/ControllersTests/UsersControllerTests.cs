﻿using Haverim.Controllers;
using Haverim.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using Haverim.Controllers.Helpers;

namespace Haverim.Tests
{
    [TestClass]
    public class UsersControllerTests
    {
        private UsersController ControllerFactoy(HaverimContext db) => new UsersController(db);

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
                string RegisterResult = UController.RegisterUser(new ApiClasses.RegisterUser
                {
                    Username = "test user",
                    DisplayName = "Some Name",
                    Email = "example@mail.com",
                    Password = "123456",
                    BirthDateUnix = (int)(new DateTime(2000, 5, 29).Subtract(new DateTime(1970, 1, 1))).TotalSeconds,
                    Country = "Israel",
                    IsMale = true,
                    ProfilePicBase64 = "FCA8DCC2-1B1D-4CC3-82BE-B06B9444328D"
                });

                var User = db.Users.Find("test user");

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
                    Username = "test user",
                    DisplayName = "Omer Nahum",
                    Email = "example@mail.com",
                    Password = "123456",
                    BirthDateUnix = 959558400,
                    Country = "Israel",
                    IsMale = true,
                    ProfilePicBase64 = "FCA8DCC2-1B1D-4CC3-82BE-B06B9444328D"
                };

                RegisterResult = UController.RegisterUser(RegisterUserObject);
                Assert.AreEqual(RegisterResult, "error:2"); // Username in use

                RegisterUserObject.Username = "test user2";
                RegisterResult = UController.RegisterUser(RegisterUserObject);
                Assert.AreEqual(RegisterResult, "error:3"); // Email in use

                // Missing Parameters
                RegisterUserObject.Email = "NotTaken@mail.com";
                RegisterUserObject.Username = "test user3";
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
                    Username = "test user",
                    DisplayName = "Some User",
                    Email = "example@mail.com",
                    Password = "123456",
                    IsMale = true,
                });
                db.SaveChanges();
                var User = new Controllers.Helpers.ApiClasses.LoginUser
                {
                    Username = "test user",
                    Password = "123456"
                };
                Assert.AreEqual(UController.LoginUser(User).Split(':').Length, 2);
                Assert.AreEqual(UController.LoginUser(User).Split(':')[0], "success");
                Assert.IsTrue(UController.LoginUser(User).Split(':')[1].Length > 1);


                User.Username = "False User";   // False Username
                Assert.IsFalse(UController.LoginUser(User).Split(':')[0] == "error:0");

                User.Username = "test user";
                User.Password = "123455";   // False Password
                Assert.IsFalse(UController.LoginUser(User).Split(':')[0] == "error:0");


            }
        }

        [TestMethod]
        public void FollowUserTest()
        {
            //TODO: [Future Feature] when two people follow each other they become friends
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                var UController = new UsersController(db);

                string MainUserToken = UController.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "test user",
                    DisplayName = "Some User",
                    Email = "example@mail.com",
                    Password = "123456",
                    IsMale = true,
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1985, 1, 1)).ToUnixTimeSeconds(),
                    Country = "United States",
                    ProfilePicBase64 = "SomeUrl"
                });
                string FollowerUserToken = UController.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "follower user",
                    DisplayName = "Some User",
                    Email = "example2@mail.com",
                    Password = "123456",
                    IsMale = true,
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1985, 1, 1)).ToUnixTimeSeconds(),
                    Country = "United States",
                    ProfilePicBase64 = "SomeUrl"
                });

                Assert.AreEqual(MainUserToken.Split(':')[0], "success");
                Assert.AreEqual(FollowerUserToken.Split(':')[0], "success");

                MainUserToken = MainUserToken.Split(':')[1];
                FollowerUserToken = FollowerUserToken.Split(':')[1];

                var Request = new Controllers.Helpers.ApiClasses.FollowRequest
                {
                    Token = FollowerUserToken,
                    TargetUser = "test user"
                };

                string FollowRequstResult = UController.FollowUser(Request);
                Assert.AreEqual(FollowRequstResult, "success");

                User MainUser = db.Users.Find("test user");
                User Follower = db.Users.Find("follower user");

                Assert.AreEqual(MainUser.Followers.Count, 1);
                Assert.AreEqual(Follower.Following.Count, 1);

                Assert.AreEqual(MainUser.Followers[0], "follower user");
                Assert.AreEqual(Follower.Following[0], "test user");

                Assert.AreEqual(1, MainUser.Notifications.Count);
                Assert.AreEqual(NotificationType.NewFollower, MainUser.Notifications[0].Type);
                Assert.AreEqual("follower user", MainUser.Notifications[0].TargetUsername);

                // Repeat
                FollowRequstResult = UController.FollowUser(Request);
                Assert.AreEqual(FollowRequstResult, "success");

                Assert.AreEqual(MainUser.Followers.Count, 1);
                Assert.AreEqual(Follower.Following.Count, 1);

                Assert.AreEqual(MainUser.Followers[0], "follower user");
                Assert.AreEqual(Follower.Following[0], "test user");

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

                // Create 2 users -> 'test user' and 'follower user'
                // follower user will follow test user
                var UController = new UsersController(db);

                string MainUserToken = UController.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "test user",
                    DisplayName = "Some User",
                    Email = "example@mail.com",
                    Password = "123456",
                    IsMale = true,
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1985, 1, 1)).ToUnixTimeSeconds(),
                    Country = "United States",
                    ProfilePicBase64 = "SomeUrl"
                });
                string FollowerUserToken = UController.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "follower user",
                    DisplayName = "Some User",
                    Email = "example2@mail.com",
                    Password = "123456",
                    IsMale = true,
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1985, 1, 1)).ToUnixTimeSeconds(),
                    Country = "United States",
                    ProfilePicBase64 = "SomeUrl"
                });

                Assert.AreEqual(MainUserToken.Split(':')[0], "success");
                Assert.AreEqual(FollowerUserToken.Split(':')[0], "success");

                MainUserToken = MainUserToken.Split(':')[1];
                FollowerUserToken = FollowerUserToken.Split(':')[1];

                var Request = new Controllers.Helpers.ApiClasses.FollowRequest
                {
                    Token = FollowerUserToken,
                    TargetUser = "test user"
                };

                string FollowRequstResult = UController.FollowUser(Request);

                User MainUser = db.Users.Find("test user");
                User Follower = db.Users.Find("follower user");

                // Make follower user unfollow test user
                Request = new Controllers.Helpers.ApiClasses.FollowRequest
                {
                    Token = FollowerUserToken,
                    TargetUser = "test user"
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

                var RegisterUser = new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "test user",
                    DisplayName = "Some User",
                    Email = "example@mail.com",
                    Password = "123456",
                    IsMale = true,
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1985, 1, 1)).ToUnixTimeSeconds(),
                    Country = "United States",
                    ProfilePicBase64 = "SomeUrl"
                };

                string UserToken = UController.RegisterUser(RegisterUser).Split(':')[1];

                var FakeNotificationList = new List<Notification>();
                var User = db.Users.Find("test user");

                // Add fake notifications
                for (int i = 0; i < 17; i++)
                {
                    FakeNotificationList.Add(new Notification
                    {
                        PostId = Guid.NewGuid(),
                        PublishDate = DateTime.Now,
                        // Type will be i mod 4, this will make the types in the list apear ordered -> 0,1,2,3,0,1,2....
                        Type = (NotificationType)(i % 4)
                    });
                }

                User.Notifications = FakeNotificationList;
                db.SaveChanges();

                string GetNotificationResult = JsonConvert.SerializeObject(UController.GetNotifications(new Controllers.Helpers.ApiClasses.FeedRequest
                {
                    Token = UserToken,
                    index = 0
                }).Value);

                Assert.AreNotEqual("error", GetNotificationResult.Split(':')[0]);

                List<Notification> NotificationList = JsonConvert.DeserializeObject<Notification[]>(GetNotificationResult).ToList();
                Assert.AreEqual(10, NotificationList.Count);

                GetNotificationResult = JsonConvert.SerializeObject(UController.GetNotifications(new Controllers.Helpers.ApiClasses.FeedRequest
                {
                    Token = UserToken,
                    index = 10
                }).Value);

                Assert.AreNotEqual("error", GetNotificationResult.Split(':')[0]);

                List<Notification> TempNotificationList = JsonConvert.DeserializeObject<Notification[]>(GetNotificationResult).ToList();
                Assert.AreEqual(7, TempNotificationList.Count);

                NotificationList.AddRange(TempNotificationList);

                // This will check if notifications are return in the correct order
                // it checks if the TypeCounter mod 4 equals the notification type at index i 

                int TypeCounter = 0;
                for (int i = 0; i < 17; i++)
                {
                    int ModResult = TypeCounter % 4;
                    Notification CurrentNotification = NotificationList[i];
                    Assert.AreEqual(ModResult, (int)CurrentNotification.Type);
                    TypeCounter++;
                }

                // Error tests

                string NonExistingToken = Controllers.Helpers.JWT.GetToken(new Controllers.Helpers.ApiClasses.Payload { Username = "None" });

                var Request = new Controllers.Helpers.ApiClasses.FeedRequest
                {
                    Token = NonExistingToken
                };

                GetNotificationResult = JsonConvert.SerializeObject(UController.GetNotifications(Request).Value);
                Assert.AreEqual("\"error:0\"", GetNotificationResult);

                Request.Token = null;
                GetNotificationResult = JsonConvert.SerializeObject(UController.GetNotifications(Request).Value);
                Assert.AreEqual("\"error:5\"", GetNotificationResult);

                Request.Token = UserToken + ".";
                GetNotificationResult = JsonConvert.SerializeObject(UController.GetNotifications(Request).Value);
                Assert.AreEqual("\"error:6\"", GetNotificationResult);

                Request.Token = UserToken;
                Request.index = 17;
                GetNotificationResult = JsonConvert.SerializeObject(UController.GetNotifications(Request).Value);
                Assert.AreEqual("\"error:7\"", GetNotificationResult);
            }
        }

        [TestMethod]
        public void GetUserTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                var UController = new UsersController(db);
                UController.RegisterUser(new ApiClasses.RegisterUser
                {
                    Username = "test user",
                    DisplayName = "Some User",
                    Email = "example@mail.com",
                    Password = "123456",
                    IsMale = true,
                    BirthDateUnix = (int)(new DateTime(1985, 1, 1).Subtract(new DateTime(1970, 1, 1))).TotalSeconds,
                    Country = "United States",
                    ProfilePicBase64 = "SomeUrl"
                });
                Console.WriteLine(UController);
                UController.GetUser("test user");

                string GetUserResult = JsonConvert.SerializeObject(UController.GetUser("test user").Value);
                Assert.AreNotEqual("error", GetUserResult.Split(':'));
                var User = JsonConvert.DeserializeObject<ApiClasses.PublicUserData>(GetUserResult);

                Assert.AreEqual("test user", User.Username);
                Assert.AreEqual(new DateTime(1985, 1, 1), User.BirthDate);
                Assert.AreEqual(0, User.FollowersCount);

                // Error tests
                GetUserResult = JsonConvert.SerializeObject(UController.GetUser("None Existing").Value);
                Assert.AreEqual("\"error:0\"", GetUserResult);

                GetUserResult = JsonConvert.SerializeObject(UController.GetUser(null).Value);
                Assert.AreEqual("\"error:5\"", GetUserResult);
            }
        }

        [TestMethod]
        public void GetUserFollowersTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                var UController = new UsersController(db);
                UController.RegisterUser(new ApiClasses.RegisterUser
                {
                    Username = "test user",
                    DisplayName = "Some User",
                    Email = "example@mail.com",
                    Password = "123456",
                    IsMale = true,
                    BirthDateUnix = (int)new DateTimeOffset(new DateTime(1985, 1, 1)).ToUnixTimeSeconds(),
                    Country = "United States",
                    ProfilePicBase64 = "SomeUrl"
                });

                var User = db.Users.Find("test user");
                // Fake Followers
                User.Followers = new List<string> { "1", "2", "3" };
                // Fake Following
                User.Following = new List<string> { "1", "2", "3", "4", "5" };
                db.SaveChanges();

                // Get Following
                string GetResult = JsonConvert.SerializeObject(UController.GetUserFollowers("test user", false).Value);
                Assert.AreNotEqual("error", GetResult.Split(':')[0]);

                List<string> Following = JsonConvert.DeserializeObject<List<string>>(GetResult);
                for (int i = 0; i < 5; i++)
                {
                    Assert.AreEqual((i + 1).ToString(), Following[i]);
                }

                GetResult = JsonConvert.SerializeObject(UController.GetUserFollowers("test user", true).Value);
                Assert.AreNotEqual("error", GetResult.Split(':')[0]);

                List<string> Followers = JsonConvert.DeserializeObject<List<string>>(GetResult);
                for (int i = 0; i < 3; i++)
                {
                    Assert.AreEqual((i + 1).ToString(), Following[i]);
                }
                // Error tests
                GetResult = JsonConvert.SerializeObject(UController.GetUserFollowers(null, false).Value);
                Assert.AreEqual("\"error:5\"", GetResult);
                GetResult = JsonConvert.SerializeObject(UController.GetUserFollowers("None Existing", false).Value);
                Assert.AreEqual("\"error:0\"", GetResult);
            }
        }

        [TestMethod]
        public void BirthdayTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                Global.ResetDatabase(db);

                var UController = new UsersController(db);
                string MainUserToken = UController.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "test user",
                    DisplayName = "Some User",
                    Email = "example@mail.com",
                    Password = "123456",
                    IsMale = true,
                    BirthDateUnix = 959558400,
                    Country = "United States",
                    ProfilePicBase64 = "SomeUrl"
                }).Split(':')[1];

                Assert.AreEqual(5, db.Users.Find("test user").BirthDate.Month);
                Assert.AreEqual(2000, db.Users.Find("test user").BirthDate.Year);
                Assert.AreEqual(29, db.Users.Find("test user").BirthDate.Day);

            }
        }
    }
}