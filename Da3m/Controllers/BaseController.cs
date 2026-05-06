using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Da3m.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(
            ActionExecutingContext context)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = RedirectToAction(
                    "Login", "Account");
                return;
            }

            ViewBag.CurrentUser = HttpContext.Session
                .GetString("UserName");
            ViewBag.CurrentRole = roleName;
            ViewBag.CurrentUserId = userId;

            base.OnActionExecuting(context);
        }

        // ✅ Helper للتحقق من الدور
        protected bool IsAdmin()
            => HttpContext.Session
               .GetString("RoleName")?.ToLower() == "admin"
            || HttpContext.Session
               .GetString("RoleName") == "مدير";

        protected bool IsDoctor()
            => HttpContext.Session
               .GetString("RoleName")?.ToLower() == "doctor"
            || HttpContext.Session
               .GetString("RoleName") == "طبيب";

        protected bool IsPatient()
            => HttpContext.Session
               .GetString("RoleName")?.ToLower() == "patient"
            || HttpContext.Session
               .GetString("RoleName") == "مريض";

        protected bool IsDonor()
            => HttpContext.Session
               .GetString("RoleName")?.ToLower() == "donor"
            || HttpContext.Session
               .GetString("RoleName") == "متبرع";

        protected bool IsManufacturer()
            => HttpContext.Session
               .GetString("RoleName")?.ToLower() == "manufacturer"
            || HttpContext.Session
               .GetString("RoleName") == "مصنع";

        // ✅ منع الوصول
        protected IActionResult AccessDenied()
            => RedirectToAction("Denied", "Account");
    }
}