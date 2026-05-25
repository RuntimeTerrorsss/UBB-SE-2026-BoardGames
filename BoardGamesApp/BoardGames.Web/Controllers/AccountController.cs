using BoardGames.Web.Helpers;
using BoardGames.Web.Models.Account;
using BoardGames.Data.Repositories;
using BoardGames.Shared.Services;
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
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }

            var user = await userService.LoginAsync(loginViewModel.Identifier, loginViewModel.Password);

            if (user != null)
            {
                SessionHelper.SetUser(HttpContext.Session, user.Id, user.Username, user.DisplayName);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Username/email or password incorrect.");
            return View(loginViewModel);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            SessionHelper.Clear(HttpContext.Session);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registeringUserViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registeringUserViewModel);
            }

            var user = new User
            {
                Username = registeringUserViewModel.Username,
                DisplayName = registeringUserViewModel.DisplayName,
                Email = registeringUserViewModel.Email,
                PasswordHash = registeringUserViewModel.Password,
                City = registeringUserViewModel.City,
                Country = registeringUserViewModel.Country
            };
            var success = await userService.RegisterUserAsync(user);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Registration failed. The username or email may already be taken.");
                return View(registeringUserViewModel);
            }

            var loggedInUser = await userService.LoginAsync(user.Username, registeringUserViewModel.Password);
            if (loggedInUser != null)
            {
                SessionHelper.SetUser(HttpContext.Session, loggedInUser.Id, loggedInUser.Username, loggedInUser.DisplayName);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
