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
            if (!IsAdmin() && !IsDoctor() && !IsPatient()) return AccessDenied();
            ViewData["Title"] = "الأطباء";

            var allDoctors = await _context.Doctors.GetAllAsync();
            var allUsers = await _context.Users.GetAllAsync();

            ViewData["UsersDict"] = allUsers.ToDictionary(u => u.UserId, u => u.FullName);

            var doctors = allDoctors.Where(d => !d.IsDeleted).ToList();

            var centersDict = await _context.Centers.GetAllAsync();
            ViewData["centersDict"] = centersDict.ToList().ToDictionary(c => c.CenterId, c => c.CenterName);

            //ViewBag.Center = centersDict.ToList().ToDictionary(c => c.CenterId, c => c.CenterName);
            return View(doctors);
        }

        // ── GET: Doctors/Details/5 ──────────
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAdmin() && !IsDoctor() && !IsPatient() ) return AccessDenied();


            var doctor = await _context.Doctors.GetByIdAsync(id);
            if (doctor == null || doctor.IsDeleted) return NotFound();
            var user = await _context.Users.GetAllAsync();
            ViewData["UsersDict"] = user.ToDictionary(u => u.UserId, u => u.FullName);

            var center = await _context.Centers.GetAllAsync();
            ViewData["CentersDict"] = center.ToDictionary(c => c.CenterId, c => c.CenterName);

            return View(doctor);
        }

        // ── GET: Doctors/Create ─────────────
        public async Task<IActionResult> Create(int userId = 0)
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "إضافة بيانات طبيب";
            ViewBag.Centers = await _context.Centers.GetAllAsync();
            ViewData["Users"] = await _context.Users.GetAllAsync();

            var doctor = new Doctor { UserId = userId };
            return View(doctor);
        }

        // ── POST: Doctors/Create ────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Doctor doctor)
        {
            if (!IsAdmin()) return AccessDenied();
            ModelState.Remove("User");
            ModelState.Remove("Center");
            if (ModelState.IsValid)
            {
                doctor.IsDeleted = false;
                await _context.Doctors.AddAsync(doctor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إضافة الطبيب بنجاح";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Centers = await _context.Centers.GetAllAsync();
            ViewBag.Users = await _context.Users.GetAllAsync();
            return View(doctor);
        }

        // ── GET: Doctors/Edit/5 ─────────────
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return AccessDenied();

            ViewData["Title"] = "تعديل الطبيب";
            var doctor = await _context.Doctors.GetByIdAsync(id);
            if (doctor == null || doctor.IsDeleted) return NotFound();

            ViewBag.Centers = await _context.Centers.GetAllAsync();
            return View(doctor);
        }

        // ── POST: Doctors/Edit/5 ────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Doctor doctor)
        {
            if (!IsAdmin()) return AccessDenied();
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
        //soft delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return AccessDenied();

            var doctor = await _context.Doctors.GetByIdAsync(id);
            if (doctor == null) return NotFound();

            doctor.IsDeleted = true;
            _context.Doctors.Update(doctor);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف الطبيب بنجاح";
            return RedirectToAction(nameof(Index));
        }
        //restore delete
        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            if (!IsAdmin()) return AccessDenied();

            var doctor = await _context.Doctors.GetByIdAsync(id);
            if (doctor == null) return NotFound();

            doctor.IsDeleted = false;
            _context.Doctors.Update(doctor);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم استعادة الطبيب بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── POST: Doctors/Delete/5 ──────────
        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "الاطباء الموقوفون";

            var allDoctors = await _context.Doctors.GetAllAsync();
            var deleteDoctors = allDoctors.Where(d => d.IsDeleted).ToList();

            var users = await _context.Users.GetAllAsync();
            ViewData["UsersDict"] = users.ToDictionary(u => u.UserId, u => u.FullName);
            return View(deleteDoctors);
        }
        public IActionResult Complete(int userId = 0)
        {
            ViewData["Title"] = "إكمال بياناتك المهنية";
            if (userId == 0)
                userId = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");

            ViewBag.DoctorUserId = userId;
            ViewBag.Centers = _context.Centers.GetAllAsync()
                .Result.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(Doctor doctor)
        {
            ModelState.Remove("User");
            ModelState.Remove("Center");

            if (ModelState.IsValid)
            {
                doctor.IsDeleted = false;
                await _context.Doctors.AddAsync(doctor);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString(
                    "ProfileCompleted", "true");

                TempData["Success"] =
                    "تم إكمال بياناتك بنجاح — مرحباً!";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.DoctorUserId = doctor.UserId;
            ViewBag.Centers = (await _context.Centers
                .GetAllAsync()).ToList();
            return View(doctor);
        }
    }
}

