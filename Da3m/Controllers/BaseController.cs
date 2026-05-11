using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Da3m.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var roleName = HttpContext.Session.GetString("RoleName");
            
            // Declare 'ctrl' and 'action' once
            var ctrl = context.RouteData.Values["controller"]?.ToString() ?? "";
            var action = context.RouteData.Values["action"]?.ToString() ?? "";

            // ✅ تحقق من MustChangePassword
            var mustChange = HttpContext.Session.GetString("MustChangePassword") ?? "false";

            bool isChangePassword =
                ctrl == "Account" &&
                action == "ChangePassword";

            if (mustChange == "true" && !isChangePassword)
            {
                context.Result = RedirectToAction(
                    "ChangePassword", "Account");
                return;
            }
            
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = RedirectToAction(
                    "Login", "Account");
                return;
            }

            // ✅ تحقق من إكمال البيانات
            var completed = HttpContext.Session
                .GetString("ProfileCompleted") ?? "true";

            // ✅ استثن صفحات الإكمال نفسها
            bool isCompleteAction = action == "Complete";
            bool isAccountAction = ctrl == "Account";

            if (completed == "false" &&
                !isCompleteAction && !isAccountAction)
            {
                var role = roleName?.ToLower() ?? "";
                var uid = userId;

                context.Result = role switch
                {
                    "doctor" => RedirectToAction("Complete", "Doctors", new { userId = uid }),
                    "patient" => RedirectToAction("Complete", "PatientDetails", new { userId = uid }),
                    "donor" => RedirectToAction("Complete", "DonorDetails", new { userId = uid }),
                    "manufacturer" => RedirectToAction("Complete", "ManufacturerDetails", new { userId = uid }),
                    _ => RedirectToAction("Index", "Home")
                };
                return;
            }

            ViewBag.CurrentUser = HttpContext.Session.GetString("UserName");
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