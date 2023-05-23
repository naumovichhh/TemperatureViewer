using TemperatureViewer.Models.ViewModels;

namespace TemperatureViewer.Services
{
    public class SmtpService
    {

        private static readonly SmtpSettings defaultSettings = new SmtpSettings()
        {
            Server = "10.194.1.89",
            Sender = "service.asup@volna.grodno.by",
            SSL = false,
            Port = 25,
            Login = "service.asup@volna.grodno.by",
            Password = "serasu"
        };


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
