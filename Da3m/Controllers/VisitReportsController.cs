using Da3m.Data.Repositories;
using Da3m.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Da3m.Controllers
{
    public class VisitReportsController : BaseController
    {
        private readonly IUnitOfWork _context;

        public VisitReportsController(IUnitOfWork context)
        {
            _context = context;
        }

        // ── GET: Index ──────────────────────
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin() && !IsDoctor())
                return AccessDenied();

            ViewData["Title"] = "تقارير الزيارات";

            var reports = await _context.VisitReports.GetAllAsync();
            var list = reports
                .OrderByDescending(r => r.ReportDate)
                .ToList();

            // ✅ جيب أسماء المرضى عبر المطابقات
            var matches = await _context.Matches.GetAllAsync();
            var users = await _context.Users.GetAllAsync();
            var centers = await _context.Centers.GetAllAsync();
            var devices = await _context.Prostheses.GetAllAsync();

            var matchesDict = matches.ToDictionary(
                m => m.MatchId, m => m);
            var usersDict = users.ToDictionary(
                u => u.UserId, u => u.FullName);
            var centersDict = centers.ToDictionary(
                c => c.CenterId, c => c.CenterName);
            var devicesDict = devices.ToDictionary(
                d => d.DeviceId, d => d.LimbType);

            ViewData["MatchesDict"] = matchesDict;
            ViewData["UsersDict"] = usersDict;
            ViewData["CentersDict"] = centersDict;
            ViewData["DevicesDict"] = devicesDict;

            ViewBag.TotalReports = list.Count;

            return View(list);
        }

        // ── GET: Create ─────────────────────
        public async Task<IActionResult> Create(
            int patientId = 0)
        {
            if (!IsDoctor() && !IsAdmin())
                return AccessDenied();

            ViewData["Title"] = "إضافة تقرير زيارة";

            // ✅ المرضى فقط
            var roles = await _context.Roles.GetAllAsync();
            var patientRole = roles.FirstOrDefault(r =>
                r.RoleName.ToLower() == "patient" ||
                r.RoleName == "مريض");

            if (patientRole != null)
            {
                var patients = await _context.Users
                    .FindAsync(u =>
                        u.RoleId == patientRole.RoleId &&
                        !u.IsDeleted);
                ViewData["Patients"] = patients.ToList();
            }
            else
            {
                ViewData["Patients"] = new List<User>();
            }

            // ✅ مطابقات المريض المختار فقط
            if (patientId > 0)
            {
                var patientMatches = await _context.Matches
                    .FindAsync(m => m.UserId == patientId);

                var devices = await _context.Prostheses.GetAllAsync();
                var devDict = devices.ToDictionary(
                    d => d.DeviceId, d => d.LimbType);

                // ✅ أضف اسم الجهاز لكل مطابقة
                ViewBag.MatchesList = patientMatches
                    .OrderByDescending(m => m.MatchDate)
                    .Select(m => new {
                        m.MatchId,
                        m.MatchPercentage,
                        m.Status,
                        DeviceName = devDict.ContainsKey(m.DeviceId)
                            ? devDict[m.DeviceId] : "—",
                        Date = m.MatchDate.ToString("yyyy/MM/dd")
                    }).ToList();

                ViewBag.SelectedPatientId = patientId;
            }
            else
            {
                ViewBag.MatchesList = new List<object>();
            }

            var centers = await _context.Centers
                .FindAsync(c => c.IsActive);
            ViewData["Centers"] = centers.ToList();

            return View();
        }
        // ── POST: Create ────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            VisitReport report, int patientId = 0)
        {
            if (!IsDoctor() && !IsAdmin())
                return AccessDenied();

            ModelState.Remove("Center");
            ModelState.Remove("Match");

            if (!Request.Form.ContainsKey("action"))
            {
                return RedirectToAction("Create",
                    new { patientId });
            }

            if (ModelState.IsValid)
            {
                report.ReportDate = DateTime.Now;
                await _context.VisitReports.AddAsync(report);
                await _context.SaveChangesAsync(); // ✅
                TempData["Success"] = "تم إضافة التقرير بنجاح";
                return RedirectToAction(nameof(Index));
            }

            // إعادة تحميل عند الخطأ
            var roles = await _context.Roles.GetAllAsync();
            var patientRole = roles.FirstOrDefault(r =>
                r.RoleName.ToLower() == "patient" ||
                r.RoleName == "مريض");

            if (patientRole != null)
            {
                var patients = await _context.Users
                    .FindAsync(u =>
                        u.RoleId == patientRole.RoleId &&
                        !u.IsDeleted);
                ViewData["Patients"] = patients.ToList();
            }

            var centers = await _context.Centers
                .FindAsync(c => c.IsActive);
            ViewData["Centers"] = centers.ToList();
            ViewBag.SelectedPatientId = patientId;

            return View(report);
        }

        // ── GET: Edit ───────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin() && !IsDoctor())
                return AccessDenied();

            ViewData["Title"] = "تعديل التقرير";

            var report = await _context.VisitReports
                .GetByIdAsync(id);
            if (report == null) return NotFound();

            var centers = await _context.Centers
                .FindAsync(c => c.IsActive);
            ViewData["Centers"] = centers.ToList();

            return View(report);
        }

        // ── POST: Edit ──────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, VisitReport report)
        {
            if (!IsAdmin() && !IsDoctor())
                return AccessDenied();

            if (id != report.ReportId) return NotFound();

            ModelState.Remove("Center");
            ModelState.Remove("Match");

            if (ModelState.IsValid)
            {
                _context.VisitReports.Update(report);
                await _context.SaveChangesAsync(); // ✅
                TempData["Success"] = "تم تعديل التقرير بنجاح";
                return RedirectToAction(nameof(Index));
            }

            var centers = await _context.Centers
                .FindAsync(c => c.IsActive);
            ViewData["Centers"] = centers.ToList();
            return View(report);
        }

        // ── Delete ──────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return AccessDenied();

            var report = await _context.VisitReports
                .GetByIdAsync(id);
            if (report == null) return NotFound();

            _context.VisitReports.Delete(report);
            await _context.SaveChangesAsync(); // ✅
            TempData["Success"] = "تم حذف التقرير بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── Details ─────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAdmin() && !IsDoctor())
                return AccessDenied();

            ViewData["Title"] = "تفاصيل تقرير الزيارة";
            var report = await _context.VisitReports
                .GetByIdAsync(id);
            if (report == null) return NotFound();

            var match = await _context.Matches
                .GetByIdAsync(report.MatchId);
            ViewBag.Match = match;

            if (match != null)
            {
                var user = await _context.Users
                    .GetByIdAsync(match.UserId);
                ViewBag.PatientName = user?.FullName ?? "—";

                var device = await _context.Prostheses
                    .GetByIdAsync(match.DeviceId);
                ViewBag.DeviceName = device?.LimbType ?? "—";
                ViewBag.MatchPercentage = match.MatchPercentage;
                ViewBag.MatchStatus = match.Status;
            }

            var center = await _context.Centers
                .GetByIdAsync(report.CenterId);
            ViewBag.CenterName = center?.CenterName ?? "—";

            return View(report);
        }
    }
}