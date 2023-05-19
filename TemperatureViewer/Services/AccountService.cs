using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Models.DTO;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Repositories;

namespace TemperatureViewer.Services
{
    public class AccountService
    {
        private IUsersRepository _repository;

        public AccountService(IUsersRepository repository)
        {
            _repository = repository;
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

        public async Task<UserDTO> ValidateUser(string name, string password)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password,
                    Salt,
                    KeyDerivationPrf.HMACSHA256,
                    10000,
                    32
                    ));

            if (name == "adminad" && hashed == "VBBnikBLRFREizRFF3jiA0V7RoGo5S9C8bjOcusgocs=")
                return new UserDTO() { Name = "adminad", Role = "a" };

            var user = (await _repository.GetAllAsync()).FirstOrDefault(u => u.Name == name && u.Password == hashed);
            return new UserDTO() { Name = user.Name, Role = user.Role };
        }

        public async Task<bool> CreateUser(User user)
        {
            try
            {
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        user.Password,
                        Salt,
                        KeyDerivationPrf.HMACSHA256,
                        10000,
                        32
                        ));

                User userHashed = new User() { Name = user.Name, Password = hashed, Role = user.Role, Sensors = user.Sensors };
                await _repository.CreateAsync(userHashed);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                var fromContext = await _repository.GetByIdAsync(user.Id, true);
                fromContext.Name = user.Name;
                if (!string.IsNullOrEmpty(user.Password))
                {
                    string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        user.Password,
                        Salt,
                        KeyDerivationPrf.HMACSHA256,
                        10000,
                        32
                        ));
                    fromContext.Password = hashed;
                }
                fromContext.Role = user.Role;
                fromContext.Sensors = user.Sensors;
                await _repository.UpdateAsync(fromContext);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
