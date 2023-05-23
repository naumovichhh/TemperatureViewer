using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using TemperatureViewer.Models.ViewModels;

namespace TemperatureViewer.Services
{
    public static class SmtpService
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
        private static readonly string key = "asdffaf9soKKpom4";
        private static readonly string iv = ";aspdias";

        public static void SetSmtpSettings(SmtpSettings newSettings)
        {
            var list = new List<string>() { newSettings.Server, newSettings.Sender, newSettings.SSL.ToString(), newSettings.Port.ToString(), newSettings.Login, Encrypt(newSettings.Password) };
            System.IO.File.WriteAllLines("smtp", list.ToArray());
        }

        public static SmtpSettings GetSmtpSettings()
        {
            if (System.IO.File.Exists("smtp"))
            {
                var strings = System.IO.File.ReadAllLines("smtp");
                try
                {
                    string server = strings[0], sender = strings[1], ssl = strings[2], port = strings[3], login = strings[4], password = strings[5];
                    return new SmtpSettings() { Server = server, Sender = sender, SSL = bool.Parse(ssl), Port = int.Parse(port), Login = login, Password = Decrypt(password) };
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

        private static string Encrypt(string password)
        {
            using (SymmetricAlgorithm algorithm = Aes.Create())
            {
                algorithm.Padding = PaddingMode.ISO10126;
                algorithm.Key = Encoding.Unicode.GetBytes(key);
                algorithm.IV = Encoding.Unicode.GetBytes(iv);

                ICryptoTransform encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV);

                byte[] encryptedData;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] data = Encoding.Unicode.GetBytes(password);
                        cs.Write(data, 0, data.Length);
                    }
                    encryptedData = ms.ToArray();
                }

                return Convert.ToBase64String(encryptedData);
            }
        }

        private static string Decrypt(string crypted)
        {
            byte[] byteArray = Convert.FromBase64String(crypted);

            using (SymmetricAlgorithm algorithm = Aes.Create())
            {
                algorithm.Padding = PaddingMode.ISO10126;
                algorithm.Key = Encoding.Unicode.GetBytes(key);
                algorithm.IV = Encoding.Unicode.GetBytes(iv);

                ICryptoTransform decryptor = algorithm.CreateDecryptor(algorithm.Key, algorithm.IV);

                using (MemoryStream ms = new MemoryStream(byteArray))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cs))
                        {
                            string decryptedData = reader.ReadToEnd();
                            return decryptedData;
                        }
                    }
                }
            }
        }
    }
}
