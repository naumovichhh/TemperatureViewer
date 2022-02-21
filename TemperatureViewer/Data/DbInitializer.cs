﻿using System;
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

            Sensor[] termometers = new Sensor[]
            {
                //new Sensor() { Name = "ОАСУП", Uri = "http://10.195.6.59/temp1.txt" },
                //new Sensor() { Name = "Корпус 3", Uri = "http://localhost:7151/temp1.txt" },
                //new Sensor() { Name = "Корпус 14", Uri = "http://localhost:43210/temp2.txt" },
                //new Sensor() { Name = "Корпус 8", Uri = "http://localhost:43210/temp3.txt" },
                //new Sensor() { Name = "Корпус 9", Uri = "http://localhost:43210/temp4.txt" },
            };
            context.Sensors.AddRange(termometers);
            context.SaveChanges();

            Measurement[] measurements = new Measurement[]
            {
                new Measurement() { MeasurementTime = DateTime.Now - TimeSpan.FromMinutes(20), Temperature = 18.9m, SensorId = 1 },
                new Measurement() { MeasurementTime = DateTime.Now - TimeSpan.FromMinutes(15), Temperature = 19.2m, SensorId = 1 },
                new Measurement() { MeasurementTime = DateTime.Now - TimeSpan.FromMinutes(10), Temperature = 19.3m, SensorId = 1 },
                new Measurement() { MeasurementTime = DateTime.Now - TimeSpan.FromMinutes(5), Temperature = 19.3m, SensorId = 1 }
            };
            context.Measurements.AddRange(measurements);
            context.SaveChanges();
        }
    }
}
