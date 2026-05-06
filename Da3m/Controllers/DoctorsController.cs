 using global::Da3m.Data.Repositories;
 using global::Da3m.Domain;
 using Microsoft.AspNetCore.Mvc;

    namespace Da3m.Controllers
    {
        public class DoctorsController : BaseController
        {
            private readonly IUnitOfWork _context;

            public DoctorsController(IUnitOfWork context)
            {
                _context = context;
            }

            // ── GET: Doctors ────────────────────
            public async Task<IActionResult> Index()
            {
                if (!IsAdmin() && !IsDoctor()) return AccessDenied();
                ViewData["Title"] = "الأطباء";
                var doctors = await _context.Doctors.GetAllAsync();
                var centers = await _context.Centers.GetAllAsync();
                ViewBag.Center = centers.ToList().ToDictionary(c => c.CenterId, c => c.CenterName);

            return View(doctors ?? new List<Doctor>());
            }

            // ── GET: Doctors/Details/5 ──────────
            public async Task<IActionResult> Details(int id)
            {
                ViewData["Title"] = "تفاصيل الطبيب";
                var doctor = await _context.Doctors.GetByIdAsync(id);
                if (doctor == null) return NotFound();
                return View(doctor);
            }

            // ── GET: Doctors/Create ─────────────
            public async Task<IActionResult> Create(int userId =0)
            {
                ViewData["Title"] = "إضافة بيانات طبيب";
                ViewBag.Centers = await _context.Centers.GetAllAsync();
                var doctor = new Doctor { UserId = userId };
                return View(doctor);
            }

            // ── POST: Doctors/Create ────────────
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(Doctor doctor)
            {
                //if (ModelState.IsValid)
                //{
                    await _context.Doctors.AddAsync(doctor);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم إضافة الطبيب بنجاح";
                    return RedirectToAction(nameof(Index));
                //}
                ViewBag.Centers = await _context.Centers.GetAllAsync();
                ViewBag.Users = await _context.Users.GetAllAsync();
                return View(doctor);
            }

            // ── GET: Doctors/Edit/5 ─────────────
            public async Task<IActionResult> Edit(int id)
            {
                ViewData["Title"] = "تعديل الطبيب";
                var doctor = await _context.Doctors.GetByIdAsync(id);
                if (doctor == null) return NotFound();
                ViewBag.Centers = await _context.Centers.GetAllAsync();
                return View(doctor);
            }

            // ── POST: Doctors/Edit/5 ────────────
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, Doctor doctor)
            {
                if (id != doctor.UserId) return NotFound();
                  ModelState.Remove("User");
                  ModelState.Remove("Center");
                if (ModelState.IsValid)
                {
                    _context.Doctors.Update(doctor);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تعديل بيانات الطبيب بنجاح";
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.Centers = await _context.Centers.GetAllAsync();
                return View(doctor);
            }

            // ── POST: Doctors/Delete/5 ──────────
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var doctor = await _context.Doctors.GetByIdAsync(id);
                if (doctor == null) return NotFound();
                _context.Doctors.Delete(doctor);
                await _context.SaveChangesAsync();  
                TempData["Success"] = "تم حذف الطبيب بنجاح";
                return RedirectToAction(nameof(Index));
            }
        }
    }

