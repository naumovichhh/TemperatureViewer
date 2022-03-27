using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Models;

namespace TemperatureViewer.Helpers
{
    public class AccountHelper
    {
        private DbContext _context;

        public AccountHelper(DbContext context)
        {
            _context = context;
        }

        public byte[] Salt
        {
            get
            {
                byte[] salt = new byte[16];
                salt[0] = 116;
                salt[1] = 29;
                salt[2] = 44;
                salt[7] = 18;
                salt[8] = 249;
                salt[10] = 199;
                return salt;
            }
        }

        public User ValidateUser(string name, string password)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password,
                    Salt,
                    KeyDerivationPrf.HMACSHA256,
                    10000,
                    32
                    ));

            return _context.Set<User>().FirstOrDefault(u => u.Name == name && u.Password == hashed);
        }

        public User ValidateAdmin(string name, string password)
        {
            if (name == "adminad" && password == "rad2020")
            {
                return new User() { Name = name };
            }
            else
                return null;
        }

        public User CreateUser(string name, string password)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password,
                    Salt,
                    KeyDerivationPrf.HMACSHA256,
                    10000,
                    32
                    ));

            return new User() { Name = name, Password = hashed };
        }

        public User UpdateUser(User user)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    user.Password,
                    Salt,
                    KeyDerivationPrf.HMACSHA256,
                    10000,
                    32
                    ));
            user.Password = hashed;
            return user;
        }
    }
}
