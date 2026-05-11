using Da3m.Data.Repositories;
using Da3m.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Da3m.Controllers
{
    public class CentersController : BaseController
    {
        private readonly IUnitOfWork _context;

        public CentersController(IUnitOfWork context)
        {
            _context = context;
        }

        // ── GET: Centers — النشطة فقط ────────
        public async Task<IActionResult> Index()
        {
            //if (!IsAdmin() && !IsDoctor())
                //return AccessDenied();

            ViewData["Title"] = "المراكز";
            var all = await _context.Centers.GetAllAsync();
            var active = all
                .Where(c => c.IsActive)
                .ToList();

            return View(active);
        }

        // ── GET: Centers/Inactive ─────────────
        [HttpGet]
        public async Task<IActionResult> Inactive()
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "المراكز غير النشطة";

            var all = await _context.Centers.GetAllAsync();
            var inactive = all
                .Where(c => !c.IsActive)
                .ToList();

            return View(inactive);
        }

        // ── GET: Centers/Create ───────────────
        public IActionResult Create()
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "إضافة مركز";
            return View();
        }

        // ── POST: Centers/Create ──────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Center center)
        {
            if (!IsAdmin()) return AccessDenied();
            ModelState.Remove("Doctors");
            ModelState.Remove("VisitReports");

            if (ModelState.IsValid)
            {
                center.IsActive = true;
                await _context.Centers.AddAsync(center);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إضافة المركز بنجاح";
                return RedirectToAction(nameof(Index));
            }
            return View(center);
        }

        // ── GET: Centers/Edit/5 ───────────────
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "تعديل المركز";

            var center = await _context.Centers.GetByIdAsync(id);
            if (center == null) return NotFound();

            return View(center);
        }

        // ── POST: Centers/Edit/5 ──────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, Center center)
        {
            if (!IsAdmin()) return AccessDenied();
            if (id != center.CenterId) return NotFound();

            ModelState.Remove("Doctors");
            ModelState.Remove("VisitReports");

            if (ModelState.IsValid)
            {
                _context.Centers.Update(center);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تعديل المركز بنجاح";
                return RedirectToAction(nameof(Index));
            }
            return View(center);
        }

        // ── Deactivate (Soft Delete) ──────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            if (!IsAdmin()) return AccessDenied();

            var center = await _context.Centers.GetByIdAsync(id);
            if (center == null) return NotFound();

            // ✅ نستخدم IsActive بدل حذف
            center.IsActive = false;
            _context.Centers.Update(center);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إيقاف المركز بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── Activate (Restore) ────────────────
        [HttpPost]
        public async Task<IActionResult> Activate(int id)
        {
            if (!IsAdmin()) return AccessDenied();
            var center = await _context.Centers.GetByIdAsync(id);
            if (center == null) return NotFound();

            center.IsActive = true;
            _context.Centers.Update(center);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تفعيل المركز بنجاح";
            return RedirectToAction(nameof(Index));
        }
    }
}