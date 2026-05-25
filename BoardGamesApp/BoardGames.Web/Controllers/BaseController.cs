// <copyright file="BaseController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BoardGames.Web.Controllers
{
    public class BaseController : Controller
    {
        protected bool IsLoggedIn => SessionHelper.IsLoggedIn(this.HttpContext.Session);

        protected int? CurrentUserId => SessionHelper.GetUserId(this.HttpContext.Session);

        protected string? CurrentUsername => SessionHelper.GetUsername(this.HttpContext.Session);

        protected string? CurrentDisplayName => SessionHelper.GetDisplayName(this.HttpContext.Session);

        // Call this on any action that requires login
        protected IActionResult? RequireLogin()
        {
            if (!this.IsLoggedIn)
            {
                return this.RedirectToAction("Login", "Account");
            }

            return null;
        }

        // every view automatically gets this
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            this.ViewBag.IsLoggedIn = this.IsLoggedIn;
            this.ViewBag.CurrentUsername = this.CurrentUsername;
            this.ViewBag.CurrentDisplayName = this.CurrentDisplayName;
            base.OnActionExecuting(context);
        }
    }
}
