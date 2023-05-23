using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TemperatureViewer.Models.ViewModels;
using TemperatureViewer.Services;

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
            var smtpSettings = SmtpService.GetSmtpSettings();
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
    }
}
