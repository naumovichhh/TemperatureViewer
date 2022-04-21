using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TemperatureViewer.Controllers;
using TemperatureViewer.Data;
using TemperatureViewer.Models;

namespace TemperatureViewer.BackgroundServices
{

    public class SensorsAccessService : ISingletonProcessingService, ISensorsAccessService
    {
        private object lockObject = new object();
        private DateTime nextMeasurementTime;
        private List<Sensor> sensors;
        private IServiceProvider serviceProvider;
        private DateTime now;

        public SensorsAccessService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            nextMeasurementTime = DateTime.Now + TimeSpan.FromSeconds(10);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    using var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();

                    Sensor[] sensorsArray;
                    lock (lockObject)
                    {
                        sensors = context.Sensors.Where(s => !s.WasDeleted).Include(s => s.Threshold).AsNoTracking().ToList();
                        sensorsArray = sensors.ToArray();
                    }

                    now = DateTime.Now;
                    Parallel.ForEach(sensorsArray, s => HandleMeasurement(s, context));

                    await Task.Delay(nextMeasurementTime - DateTime.Now, stoppingToken);
                    nextMeasurementTime = nextMeasurementTime + TimeSpan.FromSeconds(120);
                }
            }
        }

        public Measurement[] GetMeasurements()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                using var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
                Sensor[] sensorsArray;
                lock (lockObject)
                {
                    sensorsArray = context.Sensors.Where(s => !s.WasDeleted).Include(s => s.Threshold).ToArray();
                }
                List<Measurement> list = new List<Measurement>();

                Parallel.For(0, sensorsArray.Length, (i) =>
                {
                    decimal? measured;
                    if (string.IsNullOrEmpty(sensorsArray[i].XPath))
                    {
                        measured = GetTemperatureFromTxt(sensorsArray[i].Uri);
                    }
                    else
                    {
                        measured = GetTemperatureFromXml(sensorsArray[i].Uri, sensorsArray[i].XPath);
                    }

                    if (measured != null)
                    {
                        list.Add(new Measurement() { Temperature = measured.Value, Sensor = sensorsArray[i] });
                    }
                });

                return list.ToArray();
            }
        }

        public Measurement[] GetMeasurements(int locationId)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                using var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
                Sensor[] sensorsArray;
                lock (lockObject)
                {
                    sensorsArray = context.Sensors.Where(s => !s.WasDeleted && s.LocationId == locationId).Include(s => s.Threshold).ToArray();
                }
                List<Measurement> list = new List<Measurement>();

                Parallel.For(0, sensorsArray.Length, (i) =>
                {
                    decimal? measured;
                    if (string.IsNullOrEmpty(sensorsArray[i].XPath))
                    {
                        measured = GetTemperatureFromTxt(sensorsArray[i].Uri);
                    }
                    else
                    {
                        measured = GetTemperatureFromXml(sensorsArray[i].Uri, sensorsArray[i].XPath);
                    }

                    if (measured != null)
                    {
                        list.Add(new Measurement() { Temperature = measured.Value, Sensor = sensorsArray[i] });
                    }
                });

                return list.ToArray();
            }
        }

        private decimal? GetTemperatureFromXml(string uri, string xPath)
        {
            var xmlDocument = new XmlDocument();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var stream = httpClient.GetStreamAsync(uri).Result)
                    {
                        xmlDocument.Load(stream);
                    }
                }
            }
            catch
            {
                return null;
            }

            var root = xmlDocument.DocumentElement;
            var node = root.SelectSingleNode(xPath);
            string str = node.InnerText;
            decimal result;
            if (decimal.TryParse(str, out result) || decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
            {
                result /= 1.000000000000000000000000000000000m;
                return result;
            }
            else
            {
                return null;
            }
        }

        private decimal? GetTemperatureFromTxt(string uri)
        {
            string str;
            decimal result;
            using (var httpClient = new HttpClient())
            {
                try
                {
                    str = httpClient.GetStringAsync(uri).Result;
                }
                catch
                {
                    return null;
                }
            }

            if (decimal.TryParse(str, out result) || decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
            {
                result /= 1.000000000000000000000000000000000m;
                return result;
            }
            else
            {
                return null;
            }
        }

        private void HandleMeasurement(Sensor sensor, DefaultContext context)
        {
            decimal? measured;
            if (string.IsNullOrEmpty(sensor.XPath))
            {
                measured = GetTemperatureFromTxt(sensor.Uri);
            }
            else
            {
                measured = GetTemperatureFromXml(sensor.Uri, sensor.XPath);
            }

            if (measured != null)
            {
                WriteMeasurement(measured.Value, sensor, context);
                SendNotifications(measured.Value, sensor, context);
            }
        }

        private void WriteMeasurement(decimal measured, Sensor sensor, DefaultContext context)
        {
            Measurement measurement = new Measurement()
            {
                MeasurementTime = now,
                SensorId = sensor.Id,
                Temperature = measured
            };

            lock (lockObject)
            {
                context.Measurements.Add(measurement);
                context.SaveChanges();
            }
        }

        private void SendNotifications(decimal measured, Sensor sensor, DefaultContext context)
        {
            IList<Observer> observers;
            lock (lockObject)
            {
                observers = context.Observers.AsNoTracking().ToList();
            }

            if (observers == null || observers.Count() == 0)
                return;

            if (sensor.Threshold == null)
                sensor.Threshold = HomeController.GetDefaultThreshold();

            if (measured >= sensor.Threshold.P4 || measured <= sensor.Threshold.P1)
            {
                SmtpSettings smtpSettings = AdminController.GetSmtpSettings();
                SmtpClient client = new SmtpClient(smtpSettings.Server);
                client.Credentials = new NetworkCredential(smtpSettings.Login, smtpSettings.Password);
                string body;
                if (measured >= sensor.Threshold.P4)
                {
                    body = $"Температура вышла за верхний предел: {sensor.Name} = {measured}°";
                }
                else
                {
                    body = $"Температура вышла за нижний предел: {sensor.Name} = {measured}°";
                }

                try
                {
                    foreach (var observer in observers)
                    {
                        MailAddress from = new MailAddress(smtpSettings.Sender);
                        MailAddress to = new MailAddress(observer.Email);
                        MailMessage message = new MailMessage(from, to);
                        message.Subject = "Термометры АСУП";
                        message.Body = body;
                        client.Send(message);
                    }
                }
                catch
                {
                }
            }
        }
    }
}
