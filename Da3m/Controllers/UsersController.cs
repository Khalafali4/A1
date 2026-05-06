using Da3m.Data;
using Da3m.Data.Repositories;
using Da3m.Domain;
using Microsoft.AspNetCore.Mvc;
namespace Da3m.Controllers
{
   
        public class UsersController : BaseController
        {
        private readonly IUnitOfWork _context;

            public UsersController(IUnitOfWork context)
            {
                _context = context;
            }

        // GET: Users
        // ── GET: Users ──────────────────────
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
               return AccessDenied();
            //return RedirectToAction("AccessDenied", "Account");
            ViewData["Title"] = "المستخدمون";
            var users = await _context.Users.GetAllAsync();
            var roles = await _context.Roles.GetAllAsync();
            ViewBag.Roles = roles.ToDictionary(r =>r.RoleId,r=>r.RoleName);
            return View(users);
        }

        // ── GET: Users/Details/5 ────────────
        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "تفاصيل المستخدم";
            var user = await _context.Users.GetByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // ── GET: Users/Create ───────────────
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "إضافة مستخدم";
            ViewBag.Roles = await _context.Roles.GetAllAsync();
            return View();
        }

        // ── POST: Users/Create ──────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            //if (ModelState.IsValid)
            //{
                user.CreatedAt = DateTime.Now;
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                // ✅ بعد الحفظ نجيب اسم الدور
                var role = await _context.Roles.GetByIdAsync(user.RoleId);
                var roleName = role?.RoleName?.ToLower() ?? "";

                TempData["Success"] = "تم إضافة المستخدم — أكمل البيانات";

                // ✅ نوجّه حسب الدور
                return roleName switch
                {
                    "doctor" => RedirectToAction("Create", "Doctors",
                                        new { userId = user.UserId }),
                    "patient" => RedirectToAction("Create", "PatientDetails",
                                        new { userId = user.UserId }),
                    "donor" => RedirectToAction("Create", "DonorDetails",
                                        new { userId = user.UserId }),
                    "manufacturer" => RedirectToAction("Create", "ManufacturerDetails",
                                        new { userId = user.UserId }),
                    _ => RedirectToAction(nameof(Index))
                };
            //}

            ViewBag.Roles = await _context.Roles.GetAllAsync();
            return View(user);
        }
        // ── GET: Users/Edit/5 ───────────────
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "تعديل المستخدم";
            var user = await _context.Users.GetByIdAsync(id);
            if (user == null) return NotFound();
            ViewBag.Roles = await _context.Roles.GetAllAsync();
            return View(user);
        }

        // ── POST: Users/Edit/5 ──────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.UserId) return NotFound();

            ModelState.Remove("Role"); // نتجنب التحقق من الدور لأنه غير مملوء

            if (ModelState.IsValid)
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تعديل المستخدم بنجاح";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Roles = await _context.Roles.GetAllAsync();
            return View(user);
        }

        // ── POST: Users/Delete/5 ────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return AccessDenied();
            var user = await _context.Users.GetByIdAsync(id);
            if (user == null) return NotFound();
            _context.Users.Delete(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف المستخدم بنجاح";
            return RedirectToAction(nameof(Index));
        }
    }
}
