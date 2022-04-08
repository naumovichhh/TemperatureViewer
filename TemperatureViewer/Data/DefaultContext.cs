using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Models;

namespace TemperatureViewer.Data
{
    public class DefaultContext : DbContext
    {
        public DefaultContext(DbContextOptions<DefaultContext> options) : base(options)
        {
        }

        public DbSet<Measurement> Measurements { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Location> Locations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Measurement>().ToTable("Measurements");
            builder.Entity<Sensor>().ToTable("Sensors");
            builder.Entity<User>().ToTable("Users");
            builder.Entity<Location>().ToTable("Locations");
            builder.Entity<Sensor>().HasOne(s => s.Location).WithMany(l => l.Sensors).OnDelete(DeleteBehavior.SetNull);
            base.OnModelCreating(builder);
        }
    }
}
