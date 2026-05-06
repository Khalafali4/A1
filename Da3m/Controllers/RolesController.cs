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
        public class RolesController : BaseController
        {
            private readonly IUnitOfWork _context;

            public RolesController(IUnitOfWork context)
            {
                _context = context;
            }

            // ── GET: Roles ──────────────────────
            public async Task<IActionResult> Index()
            {
                ViewData["Title"] = "الأدوار";
                var roles = await _context.Roles.GetAllAsync();
                return View(roles);
            }

            // ── GET: Roles/Create ───────────────
            public IActionResult Create()
            {
                ViewData["Title"] = "إضافة دور";
                return View();
            }

            // ── POST: Roles/Create ──────────────
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(Role role)
            {
                if (ModelState.IsValid)
                {
                    await _context.Roles.AddAsync(role);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم إضافة الدور بنجاح";
                    return RedirectToAction(nameof(Index));
                }
                return View(role);
            }

            // ── GET: Roles/Edit/5 ───────────────
            public async Task<IActionResult> Edit(int id)
            {
                ViewData["Title"] = "تعديل الدور";
                var role = await _context.Roles.GetByIdAsync(id);
                if (role == null) return NotFound();
                return View(role);
            }

            // ── POST: Roles/Edit/5 ──────────────
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, Role role)
            {
                if (id != role.RoleId) return NotFound();

                if (ModelState.IsValid)
                {
                    _context.Roles.Update(role);
                await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تعديل الدور بنجاح";
                    return RedirectToAction(nameof(Index));
                }
                return View(role);
            }

            // ── POST: Roles/Delete/5 ────────────
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var role = await _context.Roles.GetByIdAsync(id);
                if (role == null) return NotFound();
                _context.Roles.Delete(role);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف الدور بنجاح";
                return RedirectToAction(nameof(Index));
            }
        }
    }

