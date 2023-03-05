
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
        public DbSet<GymAccessory> GymAccessory { get; set; }
        public DbSet<Accessory> Accessory { get; set; }
        public DbSet<Sport> Sport   { get; set; }
        public DbSet<GymSport> GymSport { get; set; }
        public DbSet<BookingOrder> BookingOrders { get; set; }
    }
}
