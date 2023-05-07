using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TemperatureViewer.Models.ViewModels;

namespace TemperatureViewer.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        private static readonly SmtpSettings defaultSettings = new SmtpSettings()
        {
            Server = "10.194.1.89",
            Sender = "service.asup@volna.grodno.by",
            SSL = false,
            Port = 25,
            Login = "service.asup@volna.grodno.by",
            Password = "serasu"
        };

        public IActionResult SmtpSettings()
        {
            var smtpSettings = GetSmtpSettings();
            return View(smtpSettings);
        }

        [HttpPost]
        public IActionResult SmtpSettings(SmtpSettings smtpSettings)
        {
            if (ModelState.IsValid)
            {
                //if (!VerifySmtpSettings(smtpSettings))
                //{
                //    ModelState.AddModelError("", "Неправильная конфигурация SMTP");
                //    return View(smtpSettings);
                //}

                var list = new List<string>() { smtpSettings.Server, smtpSettings.Sender, smtpSettings.SSL.ToString(), smtpSettings.Port.ToString(), smtpSettings.Login, smtpSettings.Password };
                System.IO.File.WriteAllLines("smtp", list.ToArray());
                return RedirectToAction("Index");
            }
            else
            {
                return View(smtpSettings);
            }
        }

        [HttpPost]
        public JsonResult Test(SmtpSettings smtpSettings, string testEmail)
        {
            SmtpSettings settings = smtpSettings;
            SmtpClient client = new SmtpClient(settings.Server);
            client.EnableSsl = smtpSettings.SSL;
            client.Port = smtpSettings.Port;
            client.Credentials = new NetworkCredential(settings.Login, settings.Password);
            string body = "Тест";
            bool result;

            try
            {
                MailAddress from = new MailAddress(settings.Sender);
                MailAddress to = new MailAddress(testEmail);
                MailMessage message = new MailMessage(from, to);
                message.Subject = "Температуры Радиоволна";
                message.Body = body;
                client.Send(message);
                result = true;
            }
            catch
            {
                result = false;
            }

            return Json(result);
        }

        public static SmtpSettings GetSmtpSettings()
        {
            if (System.IO.File.Exists("smtp"))
            {
                var strings = System.IO.File.ReadAllLines("smtp");
                try
                {
                    string server = strings[0], sender = strings[1], ssl = strings[2], port = strings[3], login = strings[4], password = strings[5];
                    return new SmtpSettings() { Server = server, Sender = sender, SSL = bool.Parse(ssl), Port = int.Parse(port), Login = login, Password = password };
                }
                catch
                {
                    return defaultSettings;
                }
            }
            else
            {
                return defaultSettings;
            }
        }
    }
}
