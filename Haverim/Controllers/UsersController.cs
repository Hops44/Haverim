using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Haverim.Models;
using Newtonsoft.Json;

namespace Haverim.Controllers
{
    [Produces("application/json")]
    [Route("api/Users")]
    public class UsersController : Controller
    {
        private readonly HaverimContext _context;

        public UsersController(HaverimContext context)
        {
            _context = context;
        }

        [HttpGet("[Action]/{Username}")]
        public bool IsUsernameTaken([FromRoute]string Username)
        {
            return _context.Users.Find(Username) != null;
        }

        [HttpPost("[Action]")]
        public string RegisterUser([FromBody]Helpers.ApiClasses.RegisterUser user)
        {
            if (_context.Users.Find(user.Username) != null)
            {
                return "error:2";
            }
            if (_context.Users.SingleOrDefault(_ => _.Email == user.Email) != null)
            {
                return "error:3";
            }

            _context.Users.Add(new Models.User
            {
                Username = user.Username,
                DisplayName = user.DisplayName,
                Password = user.Password,
                Email = user.Email,
                ActivityFeed = new List<string>(),
                BirthDate = (new DateTime(1970, 1, 1).AddSeconds(user.BirthDateUnix)),
                JoinDate = DateTime.Now,
                Country = user.Country,
                IsMale = user.IsMale,
                Followers = new List<string>(),
                PostFeed = new List<string>(),
                ProfilePic = user.ProfilePic
            });
            _context.SaveChanges();
            return "success" ;
        }

        //    // GET: api/Users
        //    [HttpGet]
        //    public IEnumerable<User> GetUsers()
        //    {
        //        return _context.Users;
        //    }

        //    // GET: api/Users/5
        //    [HttpGet("{id}")]
        //    public async Task<IActionResult> GetUser([FromRoute] string id)
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }

        //        var user = await _context.Users.SingleOrDefaultAsync(m => m.Username == id);

        //        if (user == null)
        //        {
        //            return NotFound();
        //        }

        //        return Ok(user);
        //    }

        //    // PUT: api/Users/5
        //    [HttpPut("{id}")]
        //    public async Task<IActionResult> PutUser([FromRoute] string id, [FromBody] User user)
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }

        //        if (id != user.Username)
        //        {
        //            return BadRequest();
        //        }

        //        _context.Entry(user).State = EntityState.Modified;

        //        try
        //        {
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!UserExists(id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }

        //        return NoContent();
        //    }

        //    // POST: api/Users
        //    [HttpPost]
        //    public async Task<IActionResult> PostUser([FromBody] User user)
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }

        //        _context.Users.Add(user);
        //        try
        //        {
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateException)
        //        {
        //            if (UserExists(user.Username))
        //            {
        //                return new StatusCodeResult(StatusCodes.Status409Conflict);
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }

        //        return CreatedAtAction("GetUser", new { id = user.Username }, user);
        //    }

        //    // DELETE: api/Users/5
        //    [HttpDelete("{id}")]
        //    public async Task<IActionResult> DeleteUser([FromRoute] string id)
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }

        //        var user = await _context.Users.SingleOrDefaultAsync(m => m.Username == id);
        //        if (user == null)
        //        {
        //            return NotFound();
        //        }

        //        _context.Users.Remove(user);
        //        await _context.SaveChangesAsync();

        //        return Ok(user);
        //    }

        //    private bool UserExists(string id)
        //    {
        //        return _context.Users.Any(e => e.Username == id);
        //    }
    }
}