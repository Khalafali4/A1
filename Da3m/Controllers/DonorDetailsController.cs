using Da3m.Data.Repositories;
using Da3m.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Da3m.Controllers
{
    public class DonorDetailsController : BaseController
    {
        private readonly IUnitOfWork _context;

        public DonorDetailsController(IUnitOfWork context)
        {
            _context = context;
        }

        // ── GET: Index ──────────────────────
        // ── GET: Index ──────────────────────
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "المتبرعون";

            var all = await _context.DonorDetails.GetAllAsync();
            var donors = all
                .Where(d => !d.IsDeleted)
                .ToList();

            var users = await _context.Users.GetAllAsync();
            ViewData["UsersDict"] = users
                .ToDictionary(u => u.UserId, u => u.FullName);

            // ✅ الحل — قاموسان منفصلان بدل Anonymous Type
            var donations = await _context.Donations
                .FindAsync(d => !d.IsDeleted);

            ViewBag.DonationCounts = donations
                .GroupBy(d => d.UserId)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.DonationTotals = donations
                .GroupBy(d => d.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.Amount));

            ViewBag.GrandTotal = donations.Sum(d => d.Amount);
            ViewBag.TotalOps = donations.Count();

            return View(donors);
        }

        // ── GET: Details ────────────────────
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "تفاصيل المتبرع";

            var donor = await _context.DonorDetails
                .GetByIdAsync(id);
            if (donor == null || donor.IsDeleted)
                return NotFound();

            var user = await _context.Users.GetByIdAsync(id);
            ViewBag.UserName = user?.FullName ?? "—";
            ViewBag.UserEmail = user?.Email ?? "—";
            ViewBag.UserPhone = user?.Phone ?? "—";

            // ✅ جيب تبرعاته من جدول Donation
            var donations = await _context.Donations
                .FindAsync(d => d.UserId == id
                    && !d.IsDeleted);

            ViewBag.Donations = donations
                .OrderByDescending(d => d.DonationDate)
                .ToList();

            ViewBag.TotalAmount = donations.Sum(d => d.Amount);
            ViewBag.DonationsCount = donations.Count();

            // ✅ أجهزته
            var devices = await _context.Prostheses
                .FindAsync(p => p.UserId == id);
            ViewBag.DevicesCount = devices.Count();

            return View(donor);
        }

        // ── Soft Delete ─────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return AccessDenied();

            var donor = await _context.DonorDetails
                .GetByIdAsync(id);
            if (donor == null) return NotFound();

            donor.IsDeleted = true;
            _context.DonorDetails.Update(donor);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إيقاف المتبرع بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── Complete (تسجيل جديد) ───────────
        public IActionResult Complete(int userId = 0)
        {
            ViewData["Title"] = "إكمال بيانات المتبرع";
            if (userId == 0)
                userId = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");

            ViewBag.DonorUserId = userId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(
            DonorDetail donor)
        {
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                donor.IsDeleted = false;
                donor.TotalDonatedAmount = 0;
                donor.DonatedDevicesCount = 0;
                await _context.DonorDetails.AddAsync(donor);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString(
                    "ProfileCompleted", "true");

                TempData["Success"] =
                    "مرحباً! تم إكمال تسجيلك";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.DonorUserId = donor.UserId;
            return View(donor);
        }

        // ── Create (Admin) ──────────────────
        public IActionResult Create(int userId = 0)
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "إضافة متبرع";
            ViewBag.DonorUserId = userId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            DonorDetail donor)
        {
            if (!IsAdmin()) return AccessDenied();
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                donor.IsDeleted = false;
                donor.TotalDonatedAmount = 0;
                donor.DonatedDevicesCount = 0;
                await _context.DonorDetails.AddAsync(donor);
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "تم إضافة المتبرع بنجاح";
                return RedirectToAction("Index", "Users");
            }

            return View(donor);
        }
    }
}