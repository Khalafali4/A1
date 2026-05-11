using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Da3m.Data;
using Da3m.Domain;
using Da3m.Data.Repositories;

namespace Da3m.Controllers
{
    public class PatientDetailsController : BaseController
    {
        private readonly IUnitOfWork _context;

        public PatientDetailsController(IUnitOfWork context)
        {
            _context = context;
        }
        // ── GET: Patients ───────────────────
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin() && !IsDoctor()) return AccessDenied();
            ViewData["Title"] = "المرضى";
            var patientDetails = await _context.PatientDetails.GetAllAsync();

            var users = await _context.Users.GetAllAsync();
            var patientDict = users.ToDictionary(u => u.UserId, u => u.FullName);
            ViewData["PatientDict"] = patientDict;

            return View(patientDetails);
        }

        // ── GET: /Details/5 ─────────
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAdmin() && !IsDoctor() && !IsPatient())
                return AccessDenied();

            // ✅ المريض يرى بياناته فقط
            if (IsPatient())
            {
                var sessionId = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");
                if (id != sessionId) return AccessDenied();
            }

            var patient = await _context.PatientDetails
                .GetByIdAsync(id);
            if (patient == null || patient.IsDeleted)
                return NotFound();

            // ✅ جيب اسم المريض
            var user = await _context.Users.GetByIdAsync(id);
            ViewBag.UserName = user?.FullName ?? "—";
            ViewBag.UserEmail = user?.Email ?? "—";
            ViewBag.UserPhone = user?.Phone ?? "—";

            // ✅ قياساته
            var measurements = await _context.Measurements
                .FindAsync(m => m.UserId == id);
            ViewBag.Measurements = measurements
                .OrderByDescending(m => m.MeasuredAt)
                .ToList();

            // ✅ مطابقاته
            var matches = await _context.Matches
                .FindAsync(m => m.UserId == id);
            ViewBag.Matches = matches
                .OrderByDescending(m => m.MatchDate)
                .ToList();

            // ✅ أسماء الأجهزة
            var devices = await _context.Prostheses.GetAllAsync();
            ViewBag.DevicesDict = devices
                .ToDictionary(d => d.DeviceId, d => d.LimbType);

            return View(patient);
        }
        // ── GET: Patients/Create ────────────
        public IActionResult Create(int userId)
        {
            if (!IsAdmin()) return AccessDenied();

            ViewData["Title"] = "إضافة بيانات المريض";
            var patient = new PatientDetail { UserId = userId };
            return View(patient);
        }

        // ── POST: Patients/Create ───────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientDetail patient)
        {
            if (!IsAdmin()) return AccessDenied();
            ModelState.Remove("User");
            if (ModelState.IsValid )
            {

                foreach (var error in ModelState.Values.SelectMany(x => x.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                await _context.PatientDetails.AddAsync(patient);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إضافة بيانات المريض بنجاح";
                return RedirectToAction(nameof(Index));
            }
            //ViewBag.Users = await _context.Users.GetAllAsync();
            return View(patient);
        }

        // ── GET: Patients/Edit/5 ────────────
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "تعديل بيانات المريض";
            var patient = await _context.PatientDetails.GetByIdAsync(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        // ── POST: Patients/Edit/5 ───────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PatientDetail patient)
        {
            if (!IsAdmin()) return AccessDenied();
            if (id != patient.UserId) return NotFound();
            ModelState.Remove("User");
            ModelState.Remove("Matches");

            if (ModelState.IsValid)
            {
                _context.PatientDetails.Update(patient);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تعديل بيانات المريض بنجاح";
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // ── POST: Patients/Delete/5 ─────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return AccessDenied();
            var patient = await _context.PatientDetails.GetByIdAsync(id);
            if (patient == null) return NotFound();
            _context.PatientDetails.Delete(patient);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف بيانات المريض بنجاح";
            return RedirectToAction(nameof(Index));
        }
        // ── GET: Complete ───────────────────
        public IActionResult Complete(int userId = 0)
        {
            ViewData["Title"] = "إكمال بياناتك الطبية";
            if (userId == 0)
                userId = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");

            ViewBag.PatientUserId = userId;
            return View();
        }

        // ── POST: Complete ──────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(
            PatientDetail patient)
        {
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                patient.IsDeleted = false;
                await _context.PatientDetails.AddAsync(patient);
                await _context.SaveChangesAsync();

                // ✅ علّم البيانات مكتملة
                HttpContext.Session.SetString(
                    "ProfileCompleted", "true");

                TempData["Success"] =
                    "تم إكمال بياناتك بنجاح — مرحباً!";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.PatientUserId = patient.UserId;
            return View(patient);
        }

    }

}

