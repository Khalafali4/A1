using Da3m.Data;
using Da3m.Data.Repositories;
using Da3m.Domain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Da3m.Controllers
{
    public class MatchesController : BaseController
    {
        private readonly IUnitOfWork _context;

        public MatchesController(IUnitOfWork context)
        {
            _context = context;
        }

        // ── GET: Matches ────────────────────
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "المطابقات";

            var userId = int.Parse(HttpContext.Session
                .GetString("UserId") ?? "0");

            // ✅ المريض يرى مطابقاته فقط
            if (IsPatient())
            {
                var myMatches = await _context.Matches
                    .FindAsync(m => m.UserId == userId);

                var users = await _context.Users.GetAllAsync();
                ViewData["UsersDict"] = users
                    .ToDictionary(u => u.UserId, u => u.FullName);

                var devices = await _context.Prostheses.GetAllAsync();
                ViewData["DevicesDict"] = devices
                    .ToDictionary(d => d.DeviceId, d => d.LimbType);

                return View(myMatches);
            }

            // Admin + Doctor يرون الكل
            if (!IsAdmin() && !IsDoctor())
                return AccessDenied();

            var allMatches = await _context.Matches.GetAllAsync();
            var allUsers = await _context.Users.GetAllAsync();
            ViewData["UsersDict"] = allUsers
                .ToDictionary(u => u.UserId, u => u.FullName);

            var allDevices = await _context.Prostheses.GetAllAsync();
            ViewData["DevicesDict"] = allDevices
                .ToDictionary(d => d.DeviceId, d => d.LimbType);

            return View(allMatches);
        }
        // ── GET: Matches/Create ─────────────
        public async Task<IActionResult> Create(int userId = 0, int deviceId = 0,int measurementId = 0)
        {
            ViewData["Title"] = "إضافة مطابقة";

            // ✅ إذا مريض — userId من Session
            if (IsPatient())
            {
                userId = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");
                ViewBag.IsPatient = true;
                ViewBag.PatientName = HttpContext.Session
                    .GetString("UserName");
            }

            await LoadViewData();

            // ✅ جيب مقاسات المريض المختار مرتبة بالأحدث
            if (userId > 0)
            {
                var measurements = await _context.Measurements
                    .FindAsync(m => m.UserId == userId);

                ViewBag.PatientMeasurements = measurements
                    .OrderByDescending(m => m.MeasuredAt)
                    .ToList();
            }

            var match = new Match
            {
                UserId = userId,
                DeviceId = deviceId
            };

            // ✅ احسب النسبة إذا تم اختيار مقاس وجهاز
            if (measurementId > 0 && deviceId > 0)
            {
                var measurement = await _context.Measurements
                    .GetByIdAsync(measurementId);
                var device = await _context.Prostheses
                    .GetByIdAsync(deviceId);

                if (measurement != null && device != null)
                    ViewBag.Percentage =
                        CalculateMatch(measurement, device);

                ViewBag.SelectedMeasurementId = measurementId;
            }

            return View(match);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Match match,
            int measurementId = 0)
        {
            ModelState.Remove("User");
            ModelState.Remove("Device");
            ModelState.Remove("VisitReports");
            ModelState.Remove("Status");

            if (!Request.Form.ContainsKey("action"))
            {
                return RedirectToAction("Create", new
                {
                    userId = match.UserId,
                    deviceId = match.DeviceId,
                    measurementId = measurementId
                });
            }

            if (ModelState.IsValid)
            {
                match.MatchDate = DateTime.Now;
                match.Status = match.MatchPercentage >= 80
                    ? "مطابقة ممتازة"
                    : match.MatchPercentage >= 60
                        ? "مطابقة جيدة"
                        : "مطابقة ضعيفة";

                await _context.Matches.AddAsync(match);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إضافة المطابقة بنجاح";
                return RedirectToAction(nameof(Index));
            }

            await LoadViewData();
            return View(match);
        }

        // ── GET: Matches/Edit/5 ─────────────
        // ✅ بدل Edit — إعادة حساب النسبة فقط
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
                match.Status = match.MatchPercentage >= 80
                    ? "مطابقة ممتازة"
                    : match.MatchPercentage >= 60
                        ? "مطابقة جيدة"
                        : "مطابقة ضعيفة";

                _context.Matches.Update(match);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إعادة حساب النسبة بنجاح";
            }

            return RedirectToAction(nameof(Index));
        }

        // ── POST: Matches/Delete/5 ──────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var match = await _context.Matches.GetByIdAsync(id);
            if (match == null) return NotFound();
            _context.Matches.Delete(match);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف المطابقة بنجاح";
            return RedirectToAction(nameof(Index));
        }
        // ── AutoMatch API ───────────────────
        [HttpGet]
        public async Task<IActionResult> AutoMatch(
       int patientId, int deviceId)
        {
            var measurements = await _context.Measurements
                .FindAsync(m => m.UserId == patientId);
            var lastMeasure = measurements
                .OrderByDescending(m => m.MeasuredAt)
                .FirstOrDefault();

            if (lastMeasure == null)
                return Json(new
                {
                    percentage = 0,
                    error = "لا توجد قياسات لهذا المريض"
                });

            var device = await _context.Prostheses
                .GetByIdAsync(deviceId);

            if (device == null)
                return Json(new
                {
                    percentage = 0,
                    error = "الجهاز غير موجود"
                });

            // ✅ تحقق من التوافق الأساسي
            var patType = lastMeasure.LimbType?.ToLower() ?? "";
            var devType = device.LimbType?.ToLower() ?? "";

            bool sameCategory =
                (patType.Contains("علوي") && devType.Contains("علوي")) ||
                (patType.Contains("سفلي") && devType.Contains("سفلي"));

            if (!sameCategory)
                return Json(new
                {
                    percentage = 0,
                    error = "لا يوجد تطابق — نوع الطرف مختلف كلياً"
                });

            var pct = CalculateMatch(lastMeasure, device);
            return Json(new { percentage = pct, error = "" });
        }

        // ── Helper: LoadViewData ────────────
        private async Task LoadViewData()
        {
            // ✅ إذا مريض — لا يحتاج اختيار مريض
            if (!IsPatient())
            {
                var roles = await _context.Roles.GetAllAsync();
                var patientRole = roles.FirstOrDefault(r =>
                    r.RoleName.ToLower() == "patient" ||
                    r.RoleName == "مريض");

                if (patientRole != null)
                {
                    var patients = await _context.Users.FindAsync(u =>
                        u.RoleId == patientRole.RoleId);
                    ViewData["Patients"] = patients.ToList();
                }
                else
                {
                    ViewData["Patients"] = (await _context.Users
                        .GetAllAsync()).ToList();
                }
            }

            var devices = await _context.Prostheses
                .FindAsync(d => d.IsAvailable);
            ViewData["Devices"] = devices.ToList();
        }

        // ── Helper: CalculateMatch ──────────
        private decimal CalculateMatch(Measurement patient, Prostheses device)
        {
            decimal score = 0;

            // ✅ نوع الطرف — 40 نقطة
            // نقارن بـ Contains لأن الأسماء قد تختلف قليلاً
            var patType = patient.LimbType?.ToLower() ?? "";
            var devType = device.LimbType?.ToLower() ?? "";

            if (patType == devType)
                score += 40;
            else if (patType.Contains("علوي") && devType.Contains("علوي"))
                score += 30; // نفس الفئة لكن ليس نفس الاتجاه
            else if (patType.Contains("سفلي") && devType.Contains("سفلي"))
                score += 30;
            else
                // ❌ نوع مختلف كلياً — لا تطابق
                return 0;

            // ✅ الطول — 30 نقطة
            decimal lengthDiff = Math.Abs(
                patient.LengthCm - device.LengthCm);
            if (lengthDiff == 0) score += 30;
            else if (lengthDiff <= 2) score += 20;
            else if (lengthDiff <= 5) score += 10;

            // ✅ العرض — 30 نقطة
            decimal widthDiff = Math.Abs(
                patient.WidthCm - device.WidthCm);
            if (widthDiff == 0) score += 30;
            else if (widthDiff <= 1) score += 20;
            else if (widthDiff <= 2) score += 10;

            return score;
        }
    }
}