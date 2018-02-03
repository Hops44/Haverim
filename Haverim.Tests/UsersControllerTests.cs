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
        private void RemoveAllUsers(DbContext db)
        {
            db.Database.ExecuteSqlCommand("delete from Users");
        }
        private UsersController ControllerFactoy(HaverimContext db)
        {
           return new UsersController(db);
        }


        [TestMethod]
        public void TestMockUsers()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                int UsersCount = db.Users.CountAsync().Result;
                MockDatabase.MockUsers mock = new MockDatabase.MockUsers(db);
                RemoveAllUsers(db);
                Assert.AreEqual(db.Users.CountAsync().Result, 0);
                mock.RevertToCopy();
                Assert.AreEqual(db.Users.CountAsync().Result, UsersCount);
            }
        }

        [TestMethod]
        public void IsUsernameTaken()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                MockDatabase.MockUsers mock = new MockDatabase.MockUsers(db);
                RemoveAllUsers(db);
                db.Users.Add(new User
                {
                    Username = "Taken"
                });
                db.SaveChanges();

                UsersController Controller = ControllerFactoy(db);
                Assert.IsTrue(Controller.IsUsernameTaken("Taken"));         // Username is taken -> the method will return true
                Assert.IsFalse(Controller.IsUsernameTaken("NotTaken"));     // Username is not taken -> the method will return false


                mock.RevertToCopy();
            }
        }

        [TestMethod]
        public void RegisterUserTest()
        {
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                MockDatabase.MockUsers mock = new MockDatabase.MockUsers(db);
                RemoveAllUsers(db);
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
                Assert.AreEqual(User.ActivityFeed.Count, 0);
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




                mock.RevertToCopy();
            }
        }
    }
}
