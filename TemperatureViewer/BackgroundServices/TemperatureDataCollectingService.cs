using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using TemperatureViewer.Data;
using TemperatureViewer.Models;

namespace TemperatureViewer.BackgroundServices
{
    public interface ISingletonProcessingService
    {
        Task DoWork(CancellationToken token);
    }

    public class TemperatureDataCollectingService : ISingletonProcessingService
    {
        private DefaultContext context;
        private object lockObject = new object();
        private DateTime nextMeasurementTime;
        private List<Sensor> sensors;
        private IServiceProvider serviceProvider;

        public TemperatureDataCollectingService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void AddSensor(Sensor sensor)
        {
            lock (lockObject)
            {
                sensors?.Add(sensor);
            }
        }

        public void RemoveSensor(Sensor sensor)
        {
            lock (lockObject)
            {
                sensors?.Remove(sensor);
            }
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            nextMeasurementTime = DateTime.Now + TimeSpan.FromMinutes(1);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var scope = serviceProvider.CreateScope())
            {
                context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
                lock (lockObject)
                {
                    sensors = context.Sensors.ToList();
                }
                
                while (!stoppingToken.IsCancellationRequested)
                {
                    Sensor[] sensorsArray;
                    lock (lockObject)
                    {
                        sensorsArray = sensors.ToArray();
                    }

                    Parallel.ForEach(sensorsArray, WriteMeasurements);

                    await Task.Delay(nextMeasurementTime - DateTime.Now, stoppingToken);
                    nextMeasurementTime = nextMeasurementTime + TimeSpan.FromSeconds(20);
                }
            }
        }

        private void WriteMeasurements(Sensor sensor)
        {
            using (var reader = XmlReader.Create(sensor.Uri))
            {
                while (reader.Read())
                {
                    if (reader.Name == "term0" && reader.IsStartElement() && reader.Read())
                    {
                        decimal measured;
                        if (decimal.TryParse(reader.Value, out measured) || decimal.TryParse(reader.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out measured))
                        {
                            Measurement measurement = new Measurement()
                            {
                                MeasurementTime = DateTime.Now,
                                Sensor = sensor,
                                SensorId = sensor.Id,
                                Temperature = measured
                            };
                            context.Measurements.Add(measurement);
                            context.SaveChanges();
                            break;
                        }
                    }
                }
            }
        }
    }
}
