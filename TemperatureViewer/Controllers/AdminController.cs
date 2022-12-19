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
                if (!VerifySmtpSettings(smtpSettings))
                {
                    ModelState.AddModelError("", "Неправильная конфигурация SMTP");
                    return View(smtpSettings);
                }

                var list = new List<string>() { smtpSettings.Server, smtpSettings.Sender, smtpSettings.Login, smtpSettings.Password };
                System.IO.File.WriteAllLines("smtp", list.ToArray());
                return RedirectToAction("Index");
            }
            else
            {
                return View(smtpSettings);
            }
        }

        private bool VerifySmtpSettings(SmtpSettings settings)
        {
            SmtpClient client = new SmtpClient(settings.Server);
            client.Credentials = new NetworkCredential(settings.Login, settings.Password);
            string body = "Тест";

            try
            {
                MailAddress from = new MailAddress(settings.Sender);
                MailAddress to = new MailAddress("naumovichhh@gmail.com");
                MailMessage message = new MailMessage(from, to);
                message.Subject = "Термометры АСУП";
                message.Body = body;
                client.Send(message);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static SmtpSettings GetSmtpSettings()
        {
            if (System.IO.File.Exists("smtp"))
            {
                var strings = System.IO.File.ReadAllLines("smtp");
                try
                {
                    string server = strings[0], sender = strings[1], login = strings[2], password = strings[3];
                    return new SmtpSettings() { Server = server, Sender = sender, Login = login, Password = password };
                }
                catch
                {
                    return new SmtpSettings() { Server = "10.194.1.89", Sender = "service.asup@volna.grodno.by", Login = "service.asup@volna.grodno.by", Password = "serasu" };
                }
            }
            else
            {
                return new SmtpSettings() { Server = "10.194.1.89", Sender = "service.asup@volna.grodno.by", Login = "service.asup@volna.grodno.by", Password = "serasu" };
            }
        }
    }
}
