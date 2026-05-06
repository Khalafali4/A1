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
    public class VisitReportsController : BaseController
    {
        private readonly IUnitOfWork _context;

        public VisitReportsController(IUnitOfWork context)
        {
            _context = context;
        }

        // ── GET: VisitReports ───────────────
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "تقارير الزيارات";
            var reports = await _context.VisitReports.GetAllAsync();

            var centers = await _context.Centers.GetAllAsync();
            ViewData["CentersDict"] = centers
                .ToDictionary(c => c.CenterId, c => c.CenterName);

            var matches = await _context.Matches.GetAllAsync();
            ViewData["MatchesDict"] = matches
                .ToDictionary(m => m.MatchId,
                    m => $"مطابقة #{m.MatchId}");

            return View(reports ?? new List<VisitReport>());
        }

        // ── GET: VisitReports/Create ────────
        public async Task<IActionResult> Create(int patientId = 0)
        {
            // Only doctors can add visit reports
            if (!IsDoctor() && !IsAdmin()) return AccessDenied();

            ViewData["Title"] = "إضافة تقرير زيارة";

            // ✅ المرضى فقط
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

            // ✅ مطابقات المريض المختار فقط
            if (patientId > 0)
            {
                var patientMatches = await _context.Matches
                    .FindAsync(m => m.UserId == patientId);
                ViewData["Matches"] = patientMatches
                    .OrderByDescending(m => m.MatchDate)
                    .ToList();
                ViewBag.SelectedPatientId = patientId;
            }
            else
            {
                ViewData["Matches"] = new List<Da3m.Domain.Match>();
            }

            var centers = await _context.Centers.GetAllAsync();
            ViewData["Centers"] = centers.ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            VisitReport report, int patientId = 0)
        {
            // Only doctors can add visit reports
            if (!IsDoctor() && !IsAdmin()) return AccessDenied();

            ModelState.Remove("Center");
            ModelState.Remove("Match");

            // ✅ إذا ضغط تغيير المريض — أعد تحميل
            if (!Request.Form.ContainsKey("action"))
            {
                return RedirectToAction("Create",
                    new { patientId = patientId });
            }

            if (ModelState.IsValid)
            {
                report.ReportDate = DateTime.Now;
                await _context.VisitReports.AddAsync(report);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إضافة التقرير بنجاح";
                return RedirectToAction(nameof(Index));
            }

            // إعادة تحميل البيانات عند الخطأ
            await LoadViewData();
            return View(report);
        }

        // ── GET: VisitReports/Edit/5 ────────
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "تعديل التقرير";
            var report = await _context.VisitReports
                                   .GetByIdAsync(id);
            if (report == null) return NotFound();
            await LoadViewData();
            return View(report);
        }

        // ── POST: VisitReports/Edit/5 ───────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, VisitReport report)
        {
            if (id != report.ReportId) return NotFound();
            ModelState.Remove("Center");
            ModelState.Remove("Match");

            if (ModelState.IsValid)
            {
                _context.VisitReports.Update(report);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تعديل التقرير بنجاح";
                return RedirectToAction(nameof(Index));
            }

            await LoadViewData();
            return View(report);
        }

        // ── POST: VisitReports/Delete/5 ─────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _context.VisitReports
                                   .GetByIdAsync(id);
            if (report == null) return NotFound();
            _context.VisitReports.Delete(report);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف التقرير بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── Helper ──────────────────────────
        private async Task LoadViewData()
        {
            var centers = await _context.Centers.GetAllAsync();
            ViewData["Centers"] = centers.ToList();

            var matches = await _context.Matches.GetAllAsync();
            ViewData["Matches"] = matches.ToList();
        }
    }
}
