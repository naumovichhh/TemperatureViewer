using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TemperatureViewer.Models;

namespace TemperatureViewer.Data
{
    public class DbInitializer
    {
        public static void Initialize(DefaultContext context)
        {
            context.Database.EnsureCreated();

            if (context.Measurements.Any())
            {
                return;
            }

            Termometer[] termometers = new Termometer[]
            {
                new Termometer() { Name = "Termometer1" },
                new Termometer() { Name = "Termometer2" }
            };
            context.Termometers.AddRange(termometers);
            context.SaveChanges();

            Measurement[] measurements = new Measurement[]
            {
                new Measurement() { MeasurementTime = new DateTime(2021, 8, 19, 19, 21, 18), Temperature = 18.9m, TermometerId = 1 },
                new Measurement() { MeasurementTime = new DateTime(2021, 8, 19, 19, 31, 18), Temperature = 19.2m, TermometerId = 1 },
                new Measurement() { MeasurementTime = new DateTime(2021, 8, 19, 19, 41, 19), Temperature = 19.3m, TermometerId = 1 },
                new Measurement() { MeasurementTime = new DateTime(2021, 8, 19, 19, 51, 19), Temperature = 19.3m, TermometerId = 1 }
            };
            context.Measurements.AddRange(measurements);
            context.SaveChanges();
        }
    }
}
