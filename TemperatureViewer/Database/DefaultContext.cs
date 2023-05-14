using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Models.Entities;

namespace TemperatureViewer.Database
{
    public class DefaultContext : DbContext
    {
        public DefaultContext(DbContextOptions<DefaultContext> options) : base(options)
        {
        }

        public DbSet<Value> Values { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Threshold> Thresholds { get; set; }
        public DbSet<Observer> Observers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Sensor>().HasOne(s => s.Location).WithMany(l => l.Sensors).OnDelete(DeleteBehavior.SetNull);
            builder.Entity<Sensor>().HasOne(s => s.Threshold).WithMany(t => t.Sensors).OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Value>().HasIndex(v => v.MeasurementTime);
            base.OnModelCreating(builder);
        }
    }
}
