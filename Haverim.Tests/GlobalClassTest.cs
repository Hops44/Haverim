using Haverim.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Haverim.Tests
{
    [TestClass]
    public class GlobalClassTest
    {
        [TestMethod]
        public void ResetDatabaseTest()
        {
            int InsertUserAndAssert(HaverimContext db)
            {
                int OldUsersCount = db.Users.CountAsync().Result;
                db.Users.Add(new User
                {
                    Username = Guid.NewGuid().ToString(),
                    DisplayName = "SomeName",
                    Email = Guid.NewGuid().ToString(),
                    Password = "123",
                    Country = "United States",
                    IsMale = false,
                    ProfilePic = Guid.NewGuid().ToString()
                });
                db.SaveChanges();

                int NewUsersCount = db.Users.CountAsync().Result;
                Assert.AreEqual(OldUsersCount + 1, NewUsersCount);

                return NewUsersCount;
            }
            using (var db = new HaverimContext(Global.ContextOptions))
            {
                int NewUsersCount = InsertUserAndAssert(db);

                Global.ResetDatabase(db);

                int UsersCountAfterReset = db.Users.CountAsync().Result;
                Assert.AreEqual(UsersCountAfterReset, 0);
                Assert.AreNotEqual(UsersCountAfterReset, NewUsersCount);

                InsertUserAndAssert(db);



            }
        }
    }
}
