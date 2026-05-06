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

        // ── GET: Patients/Details/5 ─────────
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAdmin() && !IsDoctor()) return AccessDenied();
            ViewData["Title"] = "تفاصيل المريض";
            var patient = await _context.PatientDetails.GetByIdAsync(id);
            if (patient == null) return NotFound();
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
            if (ModelState.IsValid)
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

    }

}

