// <copyright file="AuthController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Security.Claims;
using BoardGames.Shared.DTO;
using BoardGames.Web.Infrastructure;
using BoardGames.Web.Models.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthProxyService authProxyService;

        public AuthController(IAuthProxyService authProxyService)
        {
            this.authProxyService = authProxyService ?? throw new ArgumentNullException(nameof(authProxyService));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return this.View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            AccountProfileDTO profile;
            try
            {
                profile = await this.authProxyService.LoginAsync(new LoginDTO
                {
                    UsernameOrEmail = model.UsernameOrEmail,
                    Password = model.Password,
                    RememberMe = model.RememberMe,
                });
            }
            catch (ProxyServiceException ex)
            {
                this.ModelState.AddModelError(string.Empty, ex.Message);
                return this.View(model);
            }

            ClaimsIdentity identity = BuildIdentity(profile);
            AuthenticationProperties authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                AllowRefresh = true,
            };

            await this.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProperties);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && this.Url.IsLocalUrl(model.ReturnUrl))
            {
                return this.Redirect(model.ReturnUrl);
            }

            if (profile.Role?.Name?.Equals("admin", StringComparison.OrdinalIgnoreCase) == true)
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Games");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return this.View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            try
            {
                await this.authProxyService.RegisterAsync(new RegisterDTO
                {
                    DisplayName = model.DisplayName,
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password,
                    ConfirmPassword = model.ConfirmPassword,
                    PhoneNumber = model.PhoneNumber ?? string.Empty,
                    Country = model.Country ?? string.Empty,
                    City = model.City ?? string.Empty,
                    StreetName = model.StreetName ?? string.Empty,
                    StreetNumber = model.StreetNumber ?? string.Empty,
                });
            }
            catch (ProxyServiceException ex)
            {
                this.AddFieldErrors(ex.Message);
                return this.View(model);
            }

            AccountProfileDTO profile;
            try
            {
                profile = await this.authProxyService.LoginAsync(new LoginDTO
                {
                    UsernameOrEmail = model.Username,
                    Password = model.Password,
                });
            }
            catch (ProxyServiceException)
            {
                return this.RedirectToAction(nameof(this.Login));
            }

            ClaimsIdentity identity = BuildIdentity(profile);
            await this.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = false, AllowRefresh = true });

            return this.RedirectToAction("Index", "Games");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword()
        {
            string message;
            try
            {
                message = await this.authProxyService.ForgotPasswordAsync();
            }
            catch (ProxyServiceException)
            {
                message = "Please contact the administrator at admin@boardrent.com.";
            }

            this.ViewData["Message"] = message;
            return this.View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await this.authProxyService.LogoutAsync();
            }
            catch (ProxyServiceException)
            {
            }

            await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return this.RedirectToAction(nameof(this.Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return this.View();
        }

        private static ClaimsIdentity BuildIdentity(AccountProfileDTO profile)
        {
            ClaimsIdentity identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, profile.Id.ToString()));
            if (profile.PamUserId.HasValue)
            {
                identity.AddClaim(new Claim("PamUserId", profile.PamUserId.Value.ToString()));
            }
            identity.AddClaim(new Claim(ClaimTypes.Name, profile.Username ?? string.Empty));
            identity.AddClaim(new Claim("DisplayName", profile.DisplayName ?? string.Empty));

            string? roleName = profile.Role?.Name;
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
            }

            return identity;
        }

        private void AddFieldErrors(string errorMessage)
        {
            const int maximumSplitParts = 2;
            string[] errors = errorMessage.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (string error in errors)
            {
                string[] parts = error.Split('|', maximumSplitParts);
                if (parts.Length == maximumSplitParts)
                {
                    this.ModelState.AddModelError(parts[0].Trim(), parts[1].Trim());
                }
                else
                {
                    this.ModelState.AddModelError(string.Empty, error.Trim());
                }
            }
        }
    }
}
