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

                RegisterResult = UController.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "Test User",
                    DisplayName = "Omer Nahum",
                    Email = "example@mail.com",
                    Password = "123456",
                    BirthDateUnix = 959558400,
                    Country = "Israel",
                    IsMale = true,
                    ProfilePic = "FCA8DCC2-1B1D-4CC3-82BE-B06B9444328D"
                });
                Assert.AreEqual(RegisterResult, "error:2"); // Username in use

                RegisterResult = UController.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "Test User2",
                    DisplayName = "Omer Nahum",
                    Email = "example@mail.com",
                    Password = "123456",
                    BirthDateUnix = 959558400,
                    Country = "Israel",
                    IsMale = true,
                    ProfilePic = "FCA8DCC2-1B1D-4CC3-82BE-B06B9444328D"
                });
                Assert.AreEqual(RegisterResult, "error:3"); // Email in use

                // Missing Parameters
                RegisterResult = UController.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = Guid.NewGuid().ToString(),
                    Password = "123456",
                    IsMale = false,
                    Country = "country",
                    BirthDateUnix = 20,
                    Email = "mail@mail.com",
                    ProfilePic = "url"
                });
                Assert.AreEqual(RegisterResult, "error:5");

                // Short password ( shorter than 6 characters )
                RegisterResult = UController.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = Guid.NewGuid().ToString(),
                    Password = "12345",
                    IsMale = false,
                    Country = "country",
                    BirthDateUnix = 20,
                    Email = "mail@mail.com",
                    ProfilePic = "url"
                });
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
    }
}