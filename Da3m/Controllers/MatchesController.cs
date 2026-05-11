using Da3m.Data.Repositories;
using Da3m.Domain;
using Microsoft.AspNetCore.Mvc;
using Match = Da3m.Domain.Match;

namespace Da3m.Controllers
{
    public class MatchesController : BaseController
    {
        private readonly IUnitOfWork _context;

        public MatchesController(IUnitOfWork context)
        {
            _context = context;
        }

        // ── GET: Index ──────────────────────
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "المطابقات";
            var userId = int.Parse(HttpContext.Session
                .GetString("UserId") ?? "0");

            if (IsPatient())
            {
                var myMatches = await _context.Matches
                    .FindAsync(m => m.UserId == userId);
                var users = await _context.Users.GetAllAsync();
                var devices = await _context.Prostheses.GetAllAsync();
                ViewData["UsersDict"] = users.ToDictionary(
                    u => u.UserId, u => u.FullName);
                ViewData["DevicesDict"] = devices.ToDictionary(
                    d => d.DeviceId, d => d.LimbType);
                return View(myMatches);
            }

            if (!IsAdmin() && !IsDoctor())
                return AccessDenied();

            var allMatches = await _context.Matches.GetAllAsync();
            var allUsers = await _context.Users.GetAllAsync();
            var allDevices = await _context.Prostheses.GetAllAsync();
            ViewData["UsersDict"] = allUsers.ToDictionary(
                u => u.UserId, u => u.FullName);
            ViewData["DevicesDict"] = allDevices.ToDictionary(
                d => d.DeviceId, d => d.LimbType);
            return View(allMatches);
        }

        // ── GET: Create ─────────────────────
        public async Task<IActionResult> Create(int userId = 0, int deviceId = 0,int measurementId = 0)
        {
            ViewData["Title"] = "إضافة مطابقة";

            if (IsPatient())
            {
                userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
                ViewBag.IsPatient = true;
                ViewBag.PatientName = HttpContext.Session.GetString("UserName");
            }

            await LoadViewData();

            if (userId > 0)
            {
                var measurements = await _context.Measurements.FindAsync(m => m.UserId == userId);
                ViewBag.PatientMeasurements = measurements.OrderByDescending(m => m.MeasuredAt).ToList();
            }

            if (measurementId > 0)
            {
                ViewBag.SelectedMeasurementId = measurementId;
            }

            // ✅ احسب النسبة
            if (measurementId > 0 && deviceId > 0)
            {
                var measurement = await _context.Measurements
                    .GetByIdAsync(measurementId);
                var device = await _context.Prostheses
                    .GetByIdAsync(deviceId);

                if (measurement != null && device != null)
                {
                    var patType = measurement.LimbType
                        ?.ToLower() ?? "";
                    var devType = device.LimbType
                        ?.ToLower() ?? "";

                    bool compatible =
                        (patType.Contains("علوي") &&
                         devType.Contains("علوي")) ||
                        (patType.Contains("سفلي") &&
                         devType.Contains("سفلي"));

                    if (compatible)
                        ViewBag.Percentage =
                            CalculateMatch(measurement, device);
                    else
                        ViewBag.NoMatch = true;
                }

                //ViewBag.SelectedMeasurementId = measurementId;
            }

            var match = new Match
            {
                UserId = userId,
                DeviceId = deviceId
            };

            return View(match);
        }
        // ── POST: Create ────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Match match, int measurementId = 0)
        {
            ModelState.Remove("User");
            ModelState.Remove("Device");
            ModelState.Remove("VisitReports");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                match.MatchDate = DateTime.Now;
                match.Status = GetStatus(match.MatchPercentage);
                await _context.Matches.AddAsync(match);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إضافة المطابقة بنجاح";
                return RedirectToAction(nameof(Index));
            }

            await LoadViewData();
            return View(match);
        }

        // ── Recalculate ─────────────────────
        [HttpPost]
        public async Task<IActionResult> Recalculate(int id)
        {
            var match = await _context.Matches.GetByIdAsync(id);
            if (match == null) return NotFound();

            var measurements = await _context.Measurements
                .FindAsync(m => m.UserId == match.UserId);
            var lastMeasure = measurements
                .OrderByDescending(m => m.MeasuredAt)
                .FirstOrDefault();

            var device = await _context.Prostheses
                .GetByIdAsync(match.DeviceId);

            if (lastMeasure != null && device != null)
            {
                match.MatchPercentage =
                    CalculateMatch(lastMeasure, device);
                match.Status = GetStatus(match.MatchPercentage);
                _context.Matches.Update(match);
                await _context.SaveChangesAsync();
                TempData["Success"] =
                    "تم إعادة حساب النسبة بنجاح";
            }

            return RedirectToAction(nameof(Index));
        }

        // ── Delete ──────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var match = await _context.Matches.GetByIdAsync(id);
            if (match == null) return NotFound();

            var reports = await _context.VisitReports
                .FindAsync(r => r.MatchId == id);
            foreach (var r in reports)
                _context.VisitReports.Delete(r);

            _context.Matches.Delete(match);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف المطابقة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ─────────────────────────
        private async Task LoadViewData()
        {
            if (!IsPatient())
            {
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
                    ViewData["Patients"] = (await _context.Users
                        .GetAllAsync()).ToList();
                }
            }

            // ✅ كل الأجهزة المتاحة
            var devices = await _context.Prostheses
                .FindAsync(d => d.IsAvailable);
            ViewData["Devices"] = devices.ToList();
        }

        private string GetStatus(decimal pct)
        {
            if (pct >= 99) return "تطابق ممتاز ⭐";
            if (pct >= 95) return "جيد جداً";
            if (pct >= 90) return "جيد";
            if (pct >= 85) return "مقبول";
            return "تطابق ضعيف";
        }
        private decimal CalculateMatch(
     Measurement patient, Prostheses device)
        {
            decimal score = 0;

            var patType = patient.LimbType?.ToLower().Trim() ?? "";
            var devType = device.LimbType?.ToLower().Trim() ?? "";

            // ✅ تحقق من نفس الفئة (علوي/سفلي)
            bool patUpper = patType.Contains("علوي");
            bool patLower = patType.Contains("سفلي");
            bool devUpper = devType.Contains("علوي");
            bool devLower = devType.Contains("سفلي");

            // ❌ فئات مختلفة كلياً — لا تطابق
            if (patUpper && devLower) return 0;
            if (patLower && devUpper) return 0;

            // ✅ نفس الفئة
            if (patType == devType)
            {
                // تطابق كامل في النوع
                score += 40;
            }
            else if ((patUpper && devUpper) ||
                     (patLower && devLower))
            {
                // نفس الفئة لكن مختلف التفاصيل
                score += 30;
            }
            else
            {
                return 0;
            }

            // ✅ الطول — 30 نقطة
            decimal ld = Math.Abs(
                patient.LengthCm - device.LengthCm);
            if (ld == 0) score += 30;
            else if (ld <= 2) score += 20;
            else if (ld <= 5) score += 10;

            // ✅ العرض — 30 نقطة
            decimal wd = Math.Abs(
                patient.WidthCm - device.WidthCm);
            if (wd == 0) score += 30;
            else if (wd <= 1) score += 20;
            else if (wd <= 2) score += 10;

            return score;
        }
    }
}