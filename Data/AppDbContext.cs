using UserServiceAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace UserServiceAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
