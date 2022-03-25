using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public DbSet<Location> Location { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Measurement>().ToTable("Measurements");
            builder.Entity<Sensor>().ToTable("Sensors");
            builder.Entity<User>().ToTable("Users");
            base.OnModelCreating(builder);
        }
    }
}
