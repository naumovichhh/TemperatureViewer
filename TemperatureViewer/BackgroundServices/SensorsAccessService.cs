using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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

    public class SensorsAccessService : ISingletonProcessingService, ISensorsAccessService
    {
        private DefaultContext context;
        private object lockObject = new object();
        private DateTime nextMeasurementTime;
        private List<Sensor> sensors;
        private IServiceProvider serviceProvider;

        public SensorsAccessService(IServiceProvider serviceProvider)
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
            nextMeasurementTime = DateTime.Now + TimeSpan.FromSeconds(10);
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

                    //Parallel.ForEach(sensorsArray, WriteMeasurementsFromTxt);

                    await Task.Delay(nextMeasurementTime - DateTime.Now, stoppingToken);
                    nextMeasurementTime = nextMeasurementTime + TimeSpan.FromSeconds(2);
                }
            }
        }

        public Measurement[] GetMeasurements()
        {
            Sensor[] sensorsArray;
            lock (lockObject)
            {
                sensorsArray = sensors.ToArray();
            }
            Measurement[] result = new Measurement[sensorsArray.Length];

            Parallel.For(0, sensorsArray.Length, (i) =>
            {
                using (var httpClient = new HttpClient())
                {
                    var str = httpClient.GetStringAsync(sensors[i].Uri).Result;
                    decimal measured;
                    if (decimal.TryParse(str, out measured) || decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out measured))
                    {
                        result[i] = new Measurement() { Temperature = measured, Sensor = sensorsArray[i] };
                    }
                }
            });

            return result;
        }

        private void WriteMeasurementsFromTxt(Sensor sensor)
        {
            using (var httpClient = new HttpClient())
            {
                var str = httpClient.GetStringAsync(sensor.Uri).Result;
                decimal measured;
                if (decimal.TryParse(str, out measured) || decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out measured))
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
                }
            }
        }

        private void WriteMeasurementsFromXml(Sensor sensor)
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
