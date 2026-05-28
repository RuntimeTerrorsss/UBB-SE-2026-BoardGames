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
    public class AccountController : Controller
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
                var claims = new List<System.Security.Claims.Claim>
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new System.Security.Claims.Claim("PamUserId", user.PamUserId.ToString()),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Username),
                    new System.Security.Claims.Claim("DisplayName", user.DisplayName ?? string.Empty),
                };

                var identity = new System.Security.Claims.ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
                await this.HttpContext.SignInAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, new System.Security.Claims.ClaimsPrincipal(identity));
                return this.RedirectToAction("Index", "Home");
            }

            this.ModelState.AddModelError(string.Empty, "Username/email or password incorrect.");
            return this.View(loginViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await this.HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
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
                var claims = new List<System.Security.Claims.Claim>
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, loggedInUser.Id.ToString()),
                    new System.Security.Claims.Claim("PamUserId", loggedInUser.PamUserId.ToString()),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, loggedInUser.Username),
                    new System.Security.Claims.Claim("DisplayName", loggedInUser.DisplayName ?? string.Empty),
                };

                var identity = new System.Security.Claims.ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
                await this.HttpContext.SignInAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, new System.Security.Claims.ClaimsPrincipal(identity));
            }

            return this.RedirectToAction("Index", "Home");
        }
    }
}
