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
        public void IsUsernameTaken()
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
                UsersController controller = ControllerFactoy(db);
                string RegisterResult = controller.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "Test User",
                    DisplayName = "Omer Nahum",
                    Email = "example@mail.com",
                    Password = "123",
                    BirthDateUnix = 959558400,
                    Country = "Israel",
                    IsMale = true,
                    ProfilePic = "FCA8DCC2-1B1D-4CC3-82BE-B06B9444328D"
                });

                var User = db.Users.Find("Test User");

                Assert.AreEqual(RegisterResult, "success");
                Assert.IsNotNull(User);
                Assert.AreEqual(User.BirthDate, new DateTime(2000, 5, 29));

                Assert.AreEqual(User.PostFeed.Count, 0);
                Assert.AreEqual(User.ActivityFeed.Count, 0);
                Assert.AreEqual(User.Following.Count, 0);
                Assert.AreEqual(User.Followers.Count, 0);

                Assert.AreEqual((DateTime.Now - User.JoinDate).Days, 0);

                RegisterResult = controller.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "Test User",
                    DisplayName = "Omer Nahum",
                    Email = "example@mail.com",
                    Password = "123",
                    BirthDateUnix = 959558400,
                    Country = "Israel",
                    IsMale = true,
                    ProfilePic = "FCA8DCC2-1B1D-4CC3-82BE-B06B9444328D"
                });
                Assert.AreEqual(RegisterResult, "error:2"); // Username in use

                RegisterResult = controller.RegisterUser(new Controllers.Helpers.ApiClasses.RegisterUser
                {
                    Username = "Test User2",
                    DisplayName = "Omer Nahum",
                    Email = "example@mail.com",
                    Password = "123",
                    BirthDateUnix = 959558400,
                    Country = "Israel",
                    IsMale = true,
                    ProfilePic = "FCA8DCC2-1B1D-4CC3-82BE-B06B9444328D"
                });
                Assert.AreEqual(RegisterResult, "error:3"); // Email in use
            }
        }
    }
}
