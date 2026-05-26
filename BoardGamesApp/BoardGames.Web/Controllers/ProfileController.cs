namespace BoardGames.Web.Controllers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using BoardGames.Web.Helpers;
    using BoardGames.Web.Infrastructure;
    using BoardGames.Web.Models.Account;
    using BoardGames.Shared.DTO;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IAccountProxyService accountProxyService;
        private readonly IConfiguration configuration;

        public ProfileController(IAccountProxyService accountProxyService, IConfiguration configuration)
        {
            this.accountProxyService = accountProxyService ?? throw new ArgumentNullException(nameof(accountProxyService));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                Guid currentUserId = User.GetAccountId();
                AccountProfileDataTransferObject profileData = await accountProxyService.GetProfileAsync(currentUserId);

                string fullAvatarUrl = string.Empty;
                if (!string.IsNullOrEmpty(profileData.AvatarUrl))
                {
                    string apiBaseUrl = configuration["ApiBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5000";
                    string avatarPath = profileData.AvatarUrl.TrimStart('/');

                    if (!avatarPath.StartsWith("avatars/"))
                    {
                        avatarPath = $"avatars/{avatarPath}";
                    }

                    fullAvatarUrl = $"{apiBaseUrl}/{avatarPath}";
                }

                ProfileViewModel model = new ProfileViewModel
                {
                    Username = profileData.Username,
                    DisplayName = profileData.DisplayName ?? string.Empty,
                    Email = profileData.Email ?? string.Empty,
                    PhoneNumber = profileData.PhoneNumber,
                    Country = profileData.Country,
                    City = profileData.City,
                    StreetName = profileData.StreetName,
                    StreetNumber = profileData.StreetNumber,
                    AvatarUrl = fullAvatarUrl
                };

                return View(model);
            }
            catch (ProxyServiceException exception)
            {
                TempData["ErrorMessage"] = exception.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Guid currentUserId = User.GetAccountId();
                var profileData = await accountProxyService.GetProfileAsync(currentUserId);

                if (!string.IsNullOrEmpty(profileData.AvatarUrl))
                {
                    string apiBaseUrl = configuration["ApiBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5000";
                    string fileName = Path.GetFileName(profileData.AvatarUrl);
                    model.AvatarUrl = $"{apiBaseUrl}/avatars/{fileName}";
                }

                return View(model);
            }

            try
            {
                Guid currentUserId = User.GetAccountId();
                AccountProfileDataTransferObject updateData = new AccountProfileDataTransferObject
                {
                    DisplayName = model.DisplayName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Country = model.Country,
                    City = model.City,
                    StreetName = model.StreetName,
                    StreetNumber = model.StreetNumber
                };

                await accountProxyService.UpdateProfileAsync(currentUserId, updateData);
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction(nameof(this.Index));
            }
            catch (ProxyServiceException exception)
            {
                Guid currentUserId = User.GetAccountId();
                var profileData = await accountProxyService.GetProfileAsync(currentUserId);
                if (!string.IsNullOrEmpty(profileData.AvatarUrl))
                {
                    string apiBaseUrl = configuration["ApiBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5000";
                    model.AvatarUrl = $"{apiBaseUrl}/avatars/{Path.GetFileName(profileData.AvatarUrl)}";
                }

                ModelState.AddModelError(string.Empty, exception.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(ProfileViewModel model)
        {
            if (model.AvatarFile == null || model.AvatarFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a valid image file.";
                return RedirectToAction(nameof(this.Index));
            }

            try
            {
                Guid currentUserId = User.GetAccountId();

                string extension = Path.GetExtension(model.AvatarFile.FileName);
                string temporaryPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");

                using (FileStream stream = new FileStream(temporaryPath, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                await accountProxyService.UploadAvatarAsync(currentUserId, temporaryPath);

                if (System.IO.File.Exists(temporaryPath))
                {
                    System.IO.File.Delete(temporaryPath);
                }

                TempData["SuccessMessage"] = "Avatar uploaded successfully.";
            }
            catch (Exception exception)
            {
                TempData["ErrorMessage"] = exception.Message;
            }

            return RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAvatar()
        {
            try
            {
                Guid currentUserId = User.GetAccountId();
                await accountProxyService.RemoveAvatarAsync(currentUserId);
                TempData["SuccessMessage"] = "Avatar removed successfully.";
            }
            catch (ProxyServiceException exception)
            {
                TempData["ErrorMessage"] = exception.Message;
            }

            return RedirectToAction(nameof(this.Index));
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                Guid currentUserId = User.GetAccountId();
                await accountProxyService.ChangePasswordAsync(currentUserId, model.CurrentPassword, model.NewPassword);

                TempData["SuccessMessage"] = "Password changed successfully. Please login again.";
                return RedirectToAction("Login", "Auth");
            }
            catch (ProxyServiceException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                return View(model);
            }
        }
    }
}