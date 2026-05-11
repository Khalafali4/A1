using Da3m.Data.Repositories;
using Da3m.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Da3m.Controllers
{
    public class ManufacturerDetailsController : BaseController
    {
        private readonly IUnitOfWork _context;

        public ManufacturerDetailsController(IUnitOfWork context)
        {
            _context = context;
        }

        // ── GET: Index ──────────────────────
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "المصنّعون";

            var all = await _context.ManufacturerDetails
                .GetAllAsync();
            var manufacturers = all
                .Where(m => !m.IsDeleted)
                .ToList();

            var users = await _context.Users.GetAllAsync();
            ViewData["UsersDict"] = users
                .ToDictionary(u => u.UserId, u => u.FullName);

            // ✅ إحصائيات الأجهزة لكل مصنّع
            var devices = await _context.Prostheses.GetAllAsync();
            ViewBag.DeviceCounts = devices
                .GroupBy(d => d.UserId)
                .ToDictionary(g => g.Key, g => g.Count());
            ViewBag.AvailableCounts = devices
                .Where(d => d.IsAvailable)
                .GroupBy(d => d.UserId)
                .ToDictionary(g => g.Key, g => g.Count());

            return View(manufacturers);
        }

        // ── GET: Details ────────────────────
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "تفاصيل المصنّع";

            var mfr = await _context.ManufacturerDetails
                .GetByIdAsync(id);
            if (mfr == null || mfr.IsDeleted)
                return NotFound();

            var user = await _context.Users.GetByIdAsync(id);
            ViewBag.UserName = user?.FullName ?? "—";
            ViewBag.UserEmail = user?.Email ?? "—";
            ViewBag.UserPhone = user?.Phone ?? "—";

            var devices = await _context.Prostheses
                .FindAsync(p => p.UserId == id);
            ViewBag.Devices = devices.ToList();
            ViewBag.DevicesCount = devices.Count();
            ViewBag.AvailableCount =
                devices.Count(d => d.IsAvailable);
            ViewBag.UnavailableCount =
                devices.Count(d => !d.IsAvailable);

            return View(mfr);
        }

        // ── GET: Create ─────────────────────
        public IActionResult Create(int userId = 0)
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "إضافة مصنّع";
            var mfr = new ManufacturerDetail
            {
                UserId = userId
            };
            return View(mfr);
        }

        // ── POST: Create ────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ManufacturerDetail mfr)
        {
            if (!IsAdmin()) return AccessDenied();
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                mfr.IsDeleted = false;
                await _context.ManufacturerDetails.AddAsync(mfr);
                await _context.SaveChangesAsync();
                TempData["Success"] =
                    "تم إضافة المصنّع بنجاح";
                return RedirectToAction("Index", "Users");
            }
            return View(mfr);
        }

        // ── GET: Edit ───────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "تعديل بيانات المصنّع";

            var mfr = await _context.ManufacturerDetails
                .GetByIdAsync(id);
            if (mfr == null) return NotFound();
            return View(mfr);
        }
        // ── POST: Edit ──────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, ManufacturerDetail mfr)
        {
            if (!IsAdmin()) return AccessDenied();
            if (id != mfr.UserId) return NotFound();
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                _context.ManufacturerDetails.Update(mfr);
                await _context.SaveChangesAsync();
                TempData["Success"] =
                    "تم تعديل بيانات المصنّع بنجاح";
                return RedirectToAction(nameof(Index));
            }
            return View(mfr);
        }

        // ── Soft Delete ─────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            if (!IsAdmin()) return AccessDenied();

            var mfr = await _context.ManufacturerDetails
                .GetByIdAsync(id);
            if (mfr == null) return NotFound();

            mfr.IsDeleted = true;
            _context.ManufacturerDetails.Update(mfr);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم إيقاف المصنّع بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── Complete ────────────────────────
        public IActionResult Complete(int userId = 0)
        {
            ViewData["Title"] = "إكمال بيانات شركتك";
            if (userId == 0)
                userId = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");
            ViewBag.ManufacturerUserId = userId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(
            ManufacturerDetail mfr)
        {
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                mfr.IsDeleted = false;
                await _context.ManufacturerDetails.AddAsync(mfr);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString(
                    "ProfileCompleted", "true");

                TempData["Success"] =
                    "مرحباً! تم إكمال تسجيلك";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ManufacturerUserId = mfr.UserId;
            return View(mfr);
        }
    }
}