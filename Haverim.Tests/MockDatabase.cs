using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Haverim.Models;

namespace Haverim.Tests
{
    /// <summary>
    /// Using Mock          
    /// Save a variable of type<MockDatabase>
    /// Access the database with using...
    /// Update the mock variable with a copy of the database by calling the constuctor of MockDatabase with the
    /// DbContext as parameter assign the value to the variable
    /// Change the db as you like
    /// When you want to revert, close the using statement and call Mock.RevertToCopy()
    /// </summary>
    public static class MockDatabase
    {
        public class MockUsers
        {
            List<User> Copy;
            HaverimContext db;
            public MockUsers(HaverimContext db)
            {
                Copy = db.Users.ToList();
                this.db = db;
            }
            public void RevertToCopy()
            {
                using (db = new HaverimContext(Global.ContextOptions))
                {
                    db.Database.ExecuteSqlCommand("delete from Users");
                    db.SaveChanges();
                }

                using (db = new HaverimContext(Global.ContextOptions))
                {
                    db.Users.AddRange(Copy);
                    db.SaveChanges();
                }
            }
        }
        public class MockPosts
        {
            List<Post> Copy;
            HaverimContext db;
            public MockPosts(HaverimContext db)
            {
                Copy = db.Posts.ToList();
                this.db = db;
            }
            public void RevertToCopy()
            {
                using (db = new HaverimContext(Global.ContextOptions))
                {
                    db.Database.ExecuteSqlCommand("delete from Posts");
                    db.SaveChanges();
                }

                using (db = new HaverimContext(Global.ContextOptions))
                {
                    db.Posts.AddRange(Copy);
                    db.SaveChanges();
                }
            }
        }



    }
}
