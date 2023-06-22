using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using TemperatureViewer.Database;
using TemperatureViewer.Services;
using TemperatureViewer.Models.DTO;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Models.ViewModels;

namespace TemperatureViewer.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginModel, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                //User user;
                //bool admin = false;
                //if ((user = _accountHelper.ValidateAdmin(loginModel.Name, loginModel.Password)) != null)
                //{
                //    admin = true;
                //}
                //else
                //{
                //    user = await _accountHelper.ValidateUser(loginModel.Name, loginModel.Password);
                //}

                var user = await _accountService.ValidateUserAsync(loginModel.Name, loginModel.Password);

                if (user == null)
                {
                    ModelState.AddModelError("", "Неверное имя пользователя или пароль.");
                    return View(loginModel);
                }

                var claims = new List<Claim>()
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name)
                };

                SetRoleClaim(claims, user);

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                if (returnUrl != null)
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("Index", "Home");
            }

            return View(loginModel);
        }

        private void SetRoleClaim(List<Claim> claims, UserDTO user)
        {
            if (user.Role == "a")
            {
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, "admin"));
            }
            else if (user.Role == "o")
            {
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, "operator"));
            }
            else
            {
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, "user"));
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
