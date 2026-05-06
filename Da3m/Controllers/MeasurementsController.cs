using Da3m.Data;
using Da3m.Data.Repositories;
using Da3m.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Da3m.Controllers
{
       public class MeasurementsController : BaseController
        {
            private readonly IUnitOfWork _context;

            public MeasurementsController(IUnitOfWork context)
            {
                _context = context;
            }

        // ── GET: Measurements ───────────────
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "القياسات";

            var userId = int.Parse(HttpContext.Session
                .GetString("UserId") ?? "0");

            // ✅ المريض يرى قياساته فقط
            if (IsPatient())
            {
                var myMeasurements = await _context.Measurements
                    .FindAsync(m => m.UserId == userId);

                var users = await _context.Users.GetAllAsync();
                ViewData["UsersDict"] = users
                    .ToDictionary(u => u.UserId, u => u.FullName);

                return View(myMeasurements);
            }

            // Admin + Doctor يرون الكل
            if (!IsAdmin() && !IsDoctor())
                return AccessDenied();

            var allMeasurements = await _context.Measurements
                .GetAllAsync();
            var allUsers = await _context.Users.GetAllAsync();
            ViewData["UsersDict"] = allUsers
                .ToDictionary(u => u.UserId, u => u.FullName);

            return View(allMeasurements);
        }

        // ── GET: Measurements/Create ────────
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "إضافة قياس";

            // ✅ المريض — هو المريض تلقائياً
            if (IsPatient())
            {
                var userId = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");
                ViewBag.PatientId = userId;
                ViewBag.PatientName = HttpContext.Session
                    .GetString("UserName");
                return View("Create");
            }

            // Admin + Doctor — يختار المريض
            var roles = await _context.Roles.GetAllAsync();
            var patientRole = roles.FirstOrDefault(r =>
                r.RoleName.ToLower() == "patient" ||
                r.RoleName == "مريض");

            if (patientRole != null)
            {
                var patients = await _context.Users.FindAsync(u =>
                    u.RoleId == patientRole.RoleId);
                ViewData["Users"] = patients.ToList();
            }
            else
            {
                ViewData["Users"] = (await _context.Users
                    .GetAllAsync()).ToList();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Measurement measurement)
        {
            ModelState.Remove("User");

            // ✅ المريض — UserId من Session
            if (IsPatient())
            {
                measurement.UserId = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");
            }

            if (ModelState.IsValid)
            {
                measurement.MeasuredAt = DateTime.Now;
                await _context.Measurements.AddAsync(measurement);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إضافة القياس بنجاح";
                return RedirectToAction(nameof(Index));
            }

            if (!IsPatient())
            {
                var users = await _context.Users.GetAllAsync();
                ViewData["Users"] = users.ToList();
            }
            return View(measurement);
        }
        // ── GET: Measurements/Edit/5 ────────
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "تعديل القياس";
            var measurement = await _context.Measurements.GetByIdAsync(id);
            if (measurement == null) return NotFound();

            // ✅ نفس الفلترة
            var roles = await _context.Roles.GetAllAsync();
            var patientRole = roles.FirstOrDefault(r =>
                r.RoleName.ToLower() == "patient" ||
                r.RoleName == "مريض");

            if (patientRole != null)
            {
                var patients = await _context.Users.FindAsync(u =>
                    u.RoleId == patientRole.RoleId);
                ViewData["Users"] = patients.ToList();
            }
            else
            {
                var users = await _context.Users.GetAllAsync();
                ViewData["Users"] = users.ToList();
            }

            return View(measurement);
        }

        // ── POST: Measurements/Edit/5 ───────
        [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(
                int id, Measurement measurement)
            {
                if (id != measurement.MeasurementId) return NotFound();
                ModelState.Remove("User");

                if (ModelState.IsValid)
                {
                    _context.Measurements.Update(measurement);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تعديل القياس بنجاح";
                    return RedirectToAction(nameof(Index));
                }

                var users = await _context.Users.GetAllAsync();
                ViewData["Users"] = users.ToList();
                return View(measurement);
            }

            // ── POST: Measurements/Delete/5 ─────
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var measurement = await _context.Measurements.GetByIdAsync(id);
                if (measurement == null) return NotFound();
                _context.Measurements.Delete(measurement);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف القياس بنجاح";
                return RedirectToAction(nameof(Index));
            }
       
       }

   
} 

