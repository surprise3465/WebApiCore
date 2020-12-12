using Microsoft.EntityFrameworkCore;
using WebApiCore.Entities;

namespace WebApiCore.DbContexts
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
