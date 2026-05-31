using BoardGames.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BoardGames.Web.Controllers
{
    public class BaseController : Controller
    {
        protected bool IsLoggedIn => SessionHelper.IsLoggedIn(HttpContext.Session);
        protected int? CurrentUserId => SessionHelper.GetUserId(HttpContext.Session);
        protected string? CurrentUsername => SessionHelper.GetUsername(HttpContext.Session);
        protected string? CurrentDisplayName => SessionHelper.GetDisplayName(HttpContext.Session);

        // Call this on any action that requires login
        protected IActionResult? RequireLogin()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            return null;
        }

        // every view automatically gets this
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.IsLoggedIn = IsLoggedIn;
            ViewBag.CurrentUsername = CurrentUsername;
            ViewBag.CurrentDisplayName = CurrentDisplayName;
            base.OnActionExecuting(context);
        }
    }
}
