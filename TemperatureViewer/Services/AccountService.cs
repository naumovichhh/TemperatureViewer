using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
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

        public async Task<UserDTO> ValidateUserAsync(string name, string password)
        {
            string hashed = GetHashedPassword(password);

            if (name == "primary" && hashed == "lwCuMTZNTdlNkVaBS4Y0cCH6H018e/vzk3VMlEQRd6U=")
                return new UserDTO() { Name = "primary", Role = "a" };

            var user = (await _repository.GetAllAsync()).FirstOrDefault(u => u.Name == name && u.Password == hashed);
            if (user == null)
                return null;
            return new UserDTO() { Name = user.Name, Role = user.Role };
        }

        public bool CreateUser(User user, ref string message)
        {
            if (user.Name == "primary" || _repository.GetAllAsync().Result.Count(u => user.Name == u.Name) > 0)
            {
                message = "Имя пользователя занято.";
                return false;
            }

            try
            {
                string hashed = GetHashedPassword(user.Password);

                User userHashed = new User() { Name = user.Name, Password = hashed, Role = user.Role, Sensors = user.Sensors };
                _repository.CreateAsync(userHashed).Wait();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateUser(User user, ref string message)
        {
            if (user.Name == "primary" || _repository.GetAllAsync().Result.Count(u => user.Name == u.Name && user.Id != u.Id) > 0)
            {
                message = "Имя пользователя занято.";
                return false;
            }

            try
            {
                var fromContext = _repository.GetByIdAsync(user.Id, true).Result;
                fromContext.Name = user.Name;
                if (!string.IsNullOrEmpty(user.Password))
                {
                    string hashed = GetHashedPassword(user.Password);
                    fromContext.Password = hashed;
                }
                fromContext.Role = user.Role;
                fromContext.Sensors = user.Sensors;
                _repository.UpdateAsync(fromContext).Wait();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetHashedPassword(string password)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        password,
                        Salt,
                        KeyDerivationPrf.HMACSHA256,
                        10000,
                        32
                        ));
        }
    }
}
