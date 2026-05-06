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
            public async Task<IActionResult> Create()
            {
                ViewData["Title"] = "إضافة تقرير زيارة";
                await LoadViewData();
                return View();
            }

            // ── POST: VisitReports/Create ───────
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(
                VisitReport report)
            {
                ModelState.Remove("Center");
                ModelState.Remove("Match");

                if (ModelState.IsValid)
                {
                    report.ReportDate = DateTime.Now;
                    await _context.VisitReports.AddAsync(report);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم إضافة التقرير بنجاح";
                    return RedirectToAction(nameof(Index));
                }

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
