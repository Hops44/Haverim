using Microsoft.EntityFrameworkCore;


namespace Haverim.Models
{
    public class HaverimContext:DbContext
    {
        public HaverimContext(DbContextOptions<HaverimContext> options)
            : base(options)
        { }
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts{ get; set; }
    }
}
