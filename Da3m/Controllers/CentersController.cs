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
    public class CentersController : BaseController
        {
            private readonly IUnitOfWork _context;

            public CentersController(IUnitOfWork context)
            {
                _context = context;
            }

            // ── GET: Centers ────────────────────
            public async Task<IActionResult> Index()
            {
                ViewData["Title"] = "المراكز";
                var centers = await _context.Centers.GetAllAsync();
                return View(centers ?? new List<Center>());
            }

            // ── GET: Centers/Create ─────────────
            public IActionResult Create()
            {
                ViewData["Title"] = "إضافة مركز";
                return View();
            }

            // ── POST: Centers/Create ────────────
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(Center center)
            {
                ModelState.Remove("Doctors");
                ModelState.Remove("VisitReports");

                if (ModelState.IsValid)
                {
                    await _context.Centers.AddAsync(center);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم إضافة المركز بنجاح";
                    return RedirectToAction(nameof(Index));
                }
                return View(center);
            }

            // ── GET: Centers/Edit/5 ─────────────
            public async Task<IActionResult> Edit(int id)
            {
                ViewData["Title"] = "تعديل المركز";
                var center = await _context.Centers.GetByIdAsync(id);
                if (center == null) return NotFound();
                return View(center);
            }

            // ── POST: Centers/Edit/5 ────────────
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, Center center)
            {
                if (id != center.CenterId) return NotFound();

                ModelState.Remove("Doctors");
                ModelState.Remove("VisitReports");

                if (ModelState.IsValid)
                {
                    _context.Centers.Update(center);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تعديل المركز بنجاح";
                    return RedirectToAction(nameof(Index));
                }
                return View(center);
            }

            // ── POST: Centers/Delete/5 ──────────
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var center = await _context.Centers.GetByIdAsync(id);
                if (center == null) return NotFound();
                _context.Centers.Delete(center);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف المركز بنجاح";
                return RedirectToAction(nameof(Index));
            }
      
   
    }
   
}


