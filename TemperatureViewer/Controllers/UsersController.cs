﻿using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Database;
using TemperatureViewer.Helpers;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Repositories;
using TemperatureViewer.Models.ViewModels;

namespace TemperatureViewer.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("Admin/{controller}/{action=Index}/{id?}")]
    public class UsersController : Controller
    {
        private readonly IUsersRepository _repository;
        private readonly AccountHelper _accountHelper;
        private readonly DefaultContext _context;

        public UsersController(IUsersRepository repository, AccountHelper accountHelper, DefaultContext context)
        {
            _repository = repository;
            _accountHelper = accountHelper;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _repository.GetAllAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _repository.GetByIdAsync(id.Value);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        public IActionResult Create()
        {
            SetViewbag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var accountHelper = new AccountHelper(_repository);
                if (viewModel.Sensors == null && viewModel.Role == "u")
                {
                    ModelState.AddModelError(string.Empty, "Пользователю должны быть видимы датчики");
                    SetViewbag();
                    return View(viewModel);
                }
                User user = GetUserFromViewModel(viewModel);
                bool successfull = await accountHelper.CreateUser(user);
                if (successfull)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Ошибка создания пользователя");
                }
            }
            SetViewbag();
            return View(viewModel);
        }

        private User GetUserFromViewModel(UserViewModel viewModel)
        {
            User user = new User()
            {
                Id = viewModel.Id,
                Name = viewModel.Name,
                Password = viewModel.Password,
                Role = viewModel.Role
            };
            if (viewModel.Role == "u")
            {
                user.Sensors = viewModel.Sensors?.Select(s => _context.Sensors.FirstOrDefault(e => e.Id == s.Value)).Where(e => e != null).ToList();
            }

            return user;
        }

        private UserViewModel GetViewModelFromUser(User user)
        {
            UserViewModel viewModel = new UserViewModel()
            {
                Id = user.Id,
                Name = user.Name,
                Role = user.Role,
            };
            if (user.Role == "u")
            {
                viewModel.Sensors = user.Sensors?.ToDictionary(s => s.Id, s => s.Id);
            }

            return viewModel;
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _repository.GetByIdAsync(id.Value, true);
            if (user == null)
            {
                return NotFound();
            }
            var viewModel = GetViewModelFromUser(user);
            SetViewbag();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            AllowEmptyPassword(viewModel);

            if (ModelState.IsValid)
            {
                if (viewModel.Sensors == null && viewModel.Role == "u")
                {
                    ModelState.AddModelError(string.Empty, "Пользователю должны быть видимы датчики");
                    SetViewbag();
                    return View(viewModel);
                }
                if (! await UserExistsAsync(viewModel.Id))
                {
                    return NotFound();
                }
                User user = GetUserFromViewModel(viewModel);
                bool successfull = await _accountHelper.UpdateUserAsync(user);
                if (successfull)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Ошибка изменения пользователя");
                }
            }
            SetViewbag();
            return View(viewModel);
        }

        private void AllowEmptyPassword(UserViewModel viewModel)
        {
            var value = ModelState[nameof(viewModel.Password)];
            if (value.ValidationState == ModelValidationState.Invalid && (string)value.RawValue == string.Empty)
            {
                value.ValidationState = ModelValidationState.Valid;
                value.Errors.Clear();
            }
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _repository.GetByIdAsync(id.Value);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private void SetViewbag()
        {
            dynamic roleUser = new ExpandoObject();
            roleUser.Title = "Пользователь";
            roleUser.Value = "u";
            ViewBag.UserRole = roleUser;
            dynamic roleOperator = new ExpandoObject();
            roleOperator.Title = "Оператор";
            roleOperator.Value = "o";
            dynamic roleAdministrator = new ExpandoObject();
            roleAdministrator.Title = "Администратор";
            roleAdministrator.Value = "a";
            ViewBag.Roles = new[] { roleOperator, roleAdministrator };
            ViewBag.Sensors = _context.Sensors.AsNoTracking().OrderBy(s => s.Name);
        }

        private async Task<bool> UserExistsAsync(int id)
        {
            return (await _repository.GetAllAsync()).Any(e => e.Id == id);
        }
    }
}
