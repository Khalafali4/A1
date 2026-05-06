using Da3m.Data.Repositories;
using Da3m.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Da3m.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _context;

        public AccountController(IUnitOfWork context)
        {
            _context = context;
        }

        // ── GET: Login ──────────────────────
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // ── POST: Login ─────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email, string password)
        {
            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "يرجى إدخال البريد وكلمة المرور";
                return View();
            }

            var users = await _context.Users.FindAsync(u =>
                u.Email == email &&
                u.Password == password);

            var user = users.FirstOrDefault();

            if (user == null)
            {
                ViewBag.Error =
                    "البريد الإلكتروني أو كلمة المرور غير صحيحة";
                return View();
            }

            var roles = await _context.Roles.GetAllAsync();
            var role = roles.FirstOrDefault(r =>
                r.RoleId == user.RoleId);

            HttpContext.Session.SetString("UserId",
                user.UserId.ToString());
            HttpContext.Session.SetString("UserName",
                user.FullName);
            HttpContext.Session.SetString("UserEmail",
                user.Email);
            HttpContext.Session.SetString("RoleName",
                role?.RoleName ?? "—");

            return RedirectToAction("Index", "Home");
        }

        // ── GET: Register ───────────────────
        public async Task<IActionResult> Register()
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Index", "Home");

            ViewBag.Roles = await _context.Roles.GetAllAsync();
            return View();
        }

        // ── POST: Register ──────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string fullName, string email,
            string password, string confirmPassword,
            string phone, int roleId)
        {
            // ✅ تحقق من تطابق كلمة المرور
            if (password != confirmPassword)
            {
                ViewBag.Error = "كلمتا المرور غير متطابقتين";
                ViewBag.Roles = await _context.Roles.GetAllAsync();
                return View();
            }

            // ✅ تحقق من عدم تكرار الإيميل
            var existing = await _context.Users.FindAsync(u =>
                u.Email == email);

            if (existing.Any())
            {
                ViewBag.Error =
                    "البريد الإلكتروني مسجل مسبقاً";
                ViewBag.Roles = await _context.Roles.GetAllAsync();
                return View();
            }

            // ✅ أنشئ المستخدم
            var user = new User
            {
                FullName = fullName,
                Email = email,
                Password = password,
                Phone = phone,
                RoleId = roleId,
                CreatedAt = DateTime.Now
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // ✅ سجّل دخوله تلقائياً
            var roles = await _context.Roles.GetAllAsync();
            var role = roles.FirstOrDefault(r =>
                r.RoleId == roleId);

            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserName",
                user.FullName);
            HttpContext.Session.SetString("UserEmail",
                user.Email);
            HttpContext.Session.SetString("RoleName",
                role?.RoleName ?? "—");

            TempData["Success"] =
                "تم إنشاء الحساب بنجاح — مرحباً بك!";

            return RedirectToAction("Index", "Home");
        }

        // ── Access Denied ───────────────────
        //GET: /Account/Denied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ── Logout ──────────────────────────
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}