using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using TemperatureViewer.Data;
using TemperatureViewer.Helpers;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Models.ViewModels;

namespace TemperatureViewer.Controllers
{
    public class AccountController : Controller
    {
        private readonly DefaultContext _context;

        public AccountController(DefaultContext context)
        {
            _context = context;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginModel, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var accountHelper = new AccountHelper(_context);
                User user;
                bool admin = false;
                if ((user = accountHelper.ValidateAdmin(loginModel.Name, loginModel.Password)) != null)
                {
                    admin = true;
                }
                else
                {
                    user = accountHelper.ValidateUser(loginModel.Name, loginModel.Password);
                }

                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(loginModel);
                }

                var claims = new List<Claim>()
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, loginModel.Name)
                    };
                if (admin)
                {
                    claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, "admin"));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                if (returnUrl != null)
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("Index", "Home");
            }

            return View(loginModel);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
