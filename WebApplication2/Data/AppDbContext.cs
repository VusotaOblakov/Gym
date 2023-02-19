
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;

namespace WebApplication2.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Region> Region { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Gym> Gym { get; set; }
    }
}
