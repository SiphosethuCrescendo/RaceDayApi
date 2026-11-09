using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.MVC.Services
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string? _role;
        public SessionAuthorizeAttribute(string? role = null) { _role = role; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            if (string.IsNullOrEmpty(session.GetString("JwtToken")))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }
            if (_role != null && session.GetString("UserRole") != _role)
                context.Result = new RedirectToActionResult("Index", "Events", null);
        }
    }
}