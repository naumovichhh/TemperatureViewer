using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using TemperatureViewer.Repositories;
using TemperatureViewer.Models.DTO;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Models.ViewModels;
using TemperatureViewer.Services;

namespace TemperatureViewer.BackgroundNAccessServices
{

    public class SensorsAccessService : ISingletonProcessingService, ISensorsAccessService
    {
        private const int fastTimeout = 700, slowTimeout = 4000;
        private IServiceProvider serviceProvider;
        private object lockObj = new object();
        private object lockNotifications = new object();
        private DateTime now;
        private int hourCounter = 0;
        private Dictionary<int, SensorState> stateDictionary = new Dictionary<int, SensorState>();
        private Dictionary<string, List<string>> notifications = new Dictionary<string, List<string>>();

        public SensorsAccessService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            DateTime nextMeasurementTime = DateTime.Now + TimeSpan.FromMinutes(5);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var sensorsRepository = scope.ServiceProvider.GetRequiredService<ISensorsRepository>();

                    Sensor[] sensorsArray;
                    sensorsArray = (await sensorsRepository.GetAllAsync(true)).Where(s => !s.WasDisabled).ToArray();

                    now = DateTime.Now;
                    Parallel.ForEach(sensorsArray, s => HandleMeasurement(s, scope));
                    SendNotifications();
                    hourCounter = (hourCounter + 1) % 12;
                    if (stoppingToken.IsCancellationRequested)
                        break;
                    TimeSpan timeSpan = nextMeasurementTime - DateTime.Now;
                    await Task.Delay(timeSpan > TimeSpan.Zero ? timeSpan : TimeSpan.FromSeconds(20), stoppingToken);
                    nextMeasurementTime = nextMeasurementTime + TimeSpan.FromMinutes(5);
                }
            }
        }

        public ValueDTO[] GetValues(bool slowly = false)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var sensorsRepository = scope.ServiceProvider.GetRequiredService<ISensorsRepository>();
                Sensor[] sensorsArray;
                sensorsArray = sensorsRepository.GetAllAsync(true).Result.Where(s => !s.WasDisabled).OrderBy(s => s.Name).ToArray();
                ValueDTO[] result = new ValueDTO[sensorsArray.Length];

                Parallel.For(0, sensorsArray.Length, (i) =>
                {
                    decimal? measured;
                    measured = GetValue(sensorsArray[i], slowly);

                    
                    result[i] = new ValueDTO() { Temperature = measured, Sensor = sensorsArray[i] };
                });

                return result;
            }
        }

        public ValueDTO[] GetValues(int locationId, bool slowly = false)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var sensorsRepository = scope.ServiceProvider.GetRequiredService<ISensorsRepository>();
                Sensor[] sensorsArray;
                sensorsArray = sensorsRepository.GetAllAsync(true).Result.Where(s => !s.WasDisabled && s.LocationId == locationId).OrderBy(s => s.Name).ToArray();
                ValueDTO[] result = new ValueDTO[sensorsArray.Length];

                Parallel.For(0, sensorsArray.Length, (i) =>
                {
                    decimal? measured;
                    measured = GetValue(sensorsArray[i], slowly);

                    result[i] = new ValueDTO() { Temperature = measured, Sensor = sensorsArray[i] };
                });

                return result;
            }
        }

        public ValueDTO[] GetValues(int[] sensorIds, bool slowly = false)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var sensorsRepository = scope.ServiceProvider.GetRequiredService<ISensorsRepository>();
                Sensor[] sensorsArray;
                sensorsArray = sensorsRepository.GetAllAsync(true).Result.Where(s => !s.WasDisabled && sensorIds.Contains(s.Id)).OrderBy(s => s.Name).ToArray();
                ValueDTO[] result = new ValueDTO[sensorsArray.Length];

                Parallel.For(0, sensorsArray.Length, (i) =>
                {
                    decimal? measured;
                    measured = GetValue(sensorsArray[i], slowly);

                    result[i] = new ValueDTO() { Temperature = measured, Sensor = sensorsArray[i] };
                });

                return result;
            }
        }

        private decimal? GetValue(Sensor sensor, bool slowly)
        {
            decimal? measured;
            if (!string.IsNullOrEmpty(sensor.XPath))
            {
                measured = GetTemperatureFromDoc(sensor, slowly);
            }
            else if (!string.IsNullOrEmpty(sensor.JSON))
            {
                measured = GetTemperatureFromJSON(sensor, slowly);
            }
            else
            {
                measured = GetTemperatureFromTxt(sensor, slowly);
            }

            return measured;
        }

        private decimal? GetTemperatureFromJSON(Sensor sensor, bool slowly)
        {
            int timeout = slowly ? slowTimeout : fastTimeout;
            
            HtmlDocument htmlDoc = new HtmlDocument();
            JObject jsonObject;
            try
            {
                using (var httpClient = CreateHttpClient(sensor))
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(timeout);
                    using (var streamReader = new StreamReader(httpClient.GetStreamAsync(sensor.Uri).Result))
                    {
                        jsonObject = JObject.Parse(streamReader.ReadToEnd());
                    }
                }

                try
                {
                    decimal number;
                    number = (decimal)jsonObject.SelectToken(sensor.JSON);
                    return number;
                }
                catch
                {
                    string str;
                    str = (string)jsonObject.SelectToken(sensor.JSON);
                    if (sensor.Regex != null)
                        str = ExtractByRegex(str, sensor.Regex);

                    return ParseNumberString(str);
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        private HttpClient CreateHttpClient(Sensor sensor)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            var httpClient = new HttpClient(clientHandler);
            if (sensor.HttpHeaders != null)
                SetHeaders(httpClient, sensor);

            return httpClient;
        }

        private void SetHeaders(HttpClient httpClient, Sensor sensor)
        {
            string[] arrayOfHeaders = sensor.HttpHeaders.Split(";");
            foreach (var header in arrayOfHeaders)
            {
                string[] nameAndValue = header.Split(":");
                httpClient.DefaultRequestHeaders.Add(nameAndValue[0], nameAndValue[1]);
            }
        }

        private decimal? GetTemperatureFromDoc(Sensor sensor, bool slowly)
        {
            int timeout = slowly ? slowTimeout : fastTimeout;
            HtmlDocument htmlDoc = new HtmlDocument();
            string str;
            try
            {
                using (var httpClient = CreateHttpClient(sensor))
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(timeout);
                    using (var stream = httpClient.GetStreamAsync(sensor.Uri).Result)
                    {
                        htmlDoc.Load(stream);
                    }
                }

                var root = htmlDoc.DocumentNode;
                var node = root.SelectSingleNode(sensor.XPath);
                str = node.InnerText;
            }
            catch (Exception ex)
            {
                return null;
            }

            if (sensor.Regex != null)
                str = ExtractByRegex(str, sensor.Regex);

            return ParseNumberString(str);
        }

        private decimal? GetTemperatureFromTxt(Sensor sensor, bool slowly)
        {
            int timeout = slowly ? slowTimeout : fastTimeout;
            string str;
            using (var httpClient = CreateHttpClient(sensor))
            {
                try
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(timeout);
                    str = httpClient.GetStringAsync(sensor.Uri).Result;
                }
                catch
                {
                    return null;
                }
            }

            if (sensor.Regex != null)
                str = ExtractByRegex(str, sensor.Regex);
            return ParseNumberString(str);
        }

        private decimal? ParseNumberString(string str)
        {
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

        private string ExtractByRegex(string str, string regex)
        {
            regex = Regex.Replace(regex, @"\[value\]", @"(-?(0|[1-9]\d*)([.,]\d+)?)");
            var match = Regex.Match(str, regex, RegexOptions.IgnoreCase);
            return match.Groups[1].Value;
        }

        private void HandleMeasurement(Sensor sensor, IServiceScope scope)
        {
            decimal? measured;
            measured = GetValue(sensor, true);

            if (measured != null)
            {
                if (hourCounter == 0)
                {
                    WriteValue(measured.Value, sensor, scope);
                }
                QueueNotifications(measured.Value, sensor, scope);
            }

        }

        private void WriteValue(decimal measured, Sensor sensor, IServiceScope scope)
        {
            var valuesRepository = scope.ServiceProvider.GetRequiredService<IValuesRepository>();
            Value value = new Value()
            {
                MeasurementTime = now,
                SensorId = sensor.Id,
                Temperature = measured
            };

            lock (lockObj)
            {
                valuesRepository.CreateAsync(value).Wait();
            }
        }

        private void SendNotifications()
        {
            SmtpSettings smtpSettings = SmtpService.GetSmtpSettings();
            SmtpClient client = new SmtpClient(smtpSettings.Server);
            client.EnableSsl = smtpSettings.SSL;
            client.Port = smtpSettings.Port;
            client.Credentials = new NetworkCredential(smtpSettings.Login, smtpSettings.Password);
            MailAddress from = new MailAddress(smtpSettings.Sender);
            foreach (var pair in notifications)
            {
                try
                {
                    var stringBuilder = new StringBuilder().AppendJoin("\r\n", pair.Value);
                    string resultMessage = stringBuilder.ToString();
                    MailAddress to = new MailAddress(pair.Key);
                    MailMessage message = new MailMessage(from, to);
                    message.IsBodyHtml = false;
                    message.Subject = "Температуры Радиоволна";
                    message.Body = resultMessage;
                    client.Send(message);
                }
                catch
                { }
            }

            notifications.Clear();
        }

        private void QueueNotifications(decimal measured, Sensor sensor, IServiceScope scope)
        {
            IList<Observer> observers;
            lock (lockObj)
            {
                var observersRepository = scope.ServiceProvider.GetRequiredService<IObserversRepository>();
                var all = observersRepository.GetAllAsync().Result;
                observers = all.Where(o => o.Sensors != null && o.Sensors.Any(s => s.Id == sensor.Id)).ToList();
            }

            if (observers == null || observers.Count() == 0)
                return;

            if (sensor.Threshold == null)
                sensor.Threshold = InformationService.GetDefaultThreshold();

            if (StateWasNormal(sensor.Id) && (measured >= sensor.Threshold.P4 || measured <= sensor.Threshold.P1))
            {
                if (measured >= sensor.Threshold.P4)
                    stateDictionary[sensor.Id] = SensorState.AboveMax;
                else
                    stateDictionary[sensor.Id] = SensorState.BelowMin;

                lock (lockNotifications)
                {
                    QueueNotNormal(sensor, measured, observers);
                }
            }
            else if (!StateWasNormal(sensor.Id) && measured < sensor.Threshold.P4 && measured > sensor.Threshold.P1)
            {
                stateDictionary[sensor.Id] = SensorState.Normal;
                lock (lockNotifications)
                {
                    QueueReturnedToNormal(sensor, observers);
                }
            }
        }

        private bool StateWasNormal(int id)
        {
            if (stateDictionary.ContainsKey(id) && stateDictionary[id] == SensorState.Normal)
                return true;
            if (!stateDictionary.ContainsKey(id))
                return true;

            return false;
        }

        private void QueueNotNormal(Sensor sensor, decimal measured, IList<Observer> observers)
        {
            string body;
            if (measured >= sensor.Threshold.P4)
            {
                body = $"Температура вышла за верхний критический предел: {sensor.Name} = {measured}°";
            }
            else
            {
                body = $"Температура вышла за нижний критический предел: {sensor.Name} = {measured}°";
            }

            try
            {
                foreach (var observer in observers)
                {
                    if (notifications.ContainsKey(observer.Email))
                    {
                        notifications[observer.Email].Add(body);
                    }
                    else
                    {
                        notifications.Add(observer.Email, new List<string>() { body });
                    }
                }
            }
            catch
            {
            }
        }

        private void QueueReturnedToNormal(Sensor sensor, IList<Observer> observers)
        {
            string body;
            body = $"Температура вернулась в нормальный диапазон: {sensor.Name}";

            try
            {
                foreach (var observer in observers)
                {
                    if (notifications.ContainsKey(observer.Email))
                    {
                        notifications[observer.Email].Add(body);
                    }
                    else
                    {
                        notifications.Add(observer.Email, new List<string>() { body });
                    }
                }
            }
            catch
            {
            }
        }

        public enum SensorState
        {
            Normal,
            AboveMax,
            BelowMin
        }
    }
}
