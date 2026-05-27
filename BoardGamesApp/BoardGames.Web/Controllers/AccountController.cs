// <copyright file="AccountController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Models;
using BoardGames.Shared.ProxyServices;
using BoardGames.Web.Helpers;
using BoardGames.Web.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : BaseController
    {
        private readonly IUserService userService;

        public AccountController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return this.View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(loginViewModel);
            }

            var user = await this.userService.LoginAsync(loginViewModel.Identifier, loginViewModel.Password);

            if (user != null)
            {
                SessionHelper.SetUser(this.HttpContext.Session, user.Id, user.Username, user.DisplayName);
                return this.RedirectToAction("Index", "Home");
            }

            this.ModelState.AddModelError(string.Empty, "Username/email or password incorrect.");
            return this.View(loginViewModel);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            SessionHelper.Clear(this.HttpContext.Session);
            return this.RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register() => this.View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registeringUserViewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(registeringUserViewModel);
            }

            var user = new User
            {
                Username = registeringUserViewModel.Username,
                DisplayName = registeringUserViewModel.DisplayName,
                Email = registeringUserViewModel.Email,
                PasswordHash = registeringUserViewModel.Password,
                City = registeringUserViewModel.City,
                Country = registeringUserViewModel.Country,
            };
            var success = await this.userService.RegisterUserAsync(user);

            if (!success)
            {
                this.ModelState.AddModelError(string.Empty, "Registration failed. The username or email may already be taken.");
                return this.View(registeringUserViewModel);
            }

            var loggedInUser = await this.userService.LoginAsync(user.Username, registeringUserViewModel.Password);
            if (loggedInUser != null)
            {
                SessionHelper.SetUser(this.HttpContext.Session, loggedInUser.Id, loggedInUser.Username, loggedInUser.DisplayName);
            }

            return this.RedirectToAction("Index", "Home");
        }
    }
}
