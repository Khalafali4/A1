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
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "الأدوار";
            var allRoles = await _context.Roles.GetAllAsync();

            var roles = allRoles.Where(r => !r.IsDeleted).ToList();

            var users = await _context.Users.GetAllAsync();
            ViewData["UsersCount"] = users.Where(u => !u.IsDeleted).GroupBy(u => u.RoleId).ToDictionary(g => g.Key, g => g.Count());
            return View(roles);
        }

        // ── GET: Roles/Create ───────────────
        public IActionResult Create()
        {
            if (!IsAdmin()) return AccessDenied();

            ViewData["Title"] = "إضافة دور";
            return View();
        }

        // ── POST: Roles/Create ──────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role role)
        {
            if (!IsAdmin()) return AccessDenied();
            var exiting = await _context.Roles.FindAsync(r => r.RoleName.ToLower().Trim() == role.RoleName.ToLower().Trim() && !r.IsDeleted);
            if(exiting.Any())
            {
                ModelState.AddModelError("RoleName", "هذا الدور موجود مسبقاً");
                return View(role);
            }

            if (ModelState.IsValid)
            {
                role.IsDeleted = false;
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
            if (!IsAdmin()) return AccessDenied();
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
            if (!IsAdmin()) return AccessDenied();
            var exiting = await _context.Roles.FindAsync(r => r.RoleName.ToLower().Trim() == role.RoleName.ToLower().Trim() && !r.IsDeleted);
            if (exiting.Any())
            {
                ModelState.AddModelError("RoleName", "هذا الدور موجود مسبقاً");
                return View(role);
            }


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

        // ── POST: soft Delete/5 ─────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return AccessDenied();
            var role = await _context.Roles.GetByIdAsync(id);
            if (role == null) return NotFound();

            var usersWithRole = await _context.Users.FindAsync(u => u.RoleId == role.RoleId && !u.IsDeleted);

            if (usersWithRole.Any())
            {
                TempData["Error"] = $"لا يمكن حذف هذا الدور لأنه مرتبط بـ {usersWithRole.Count()} مستخدمين يرجى إعادة تعيين أدوار هؤلاء المستخدمين قبل الحذف.";
                return RedirectToAction(nameof(Index));
            }

            role.IsDeleted = true;
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إيقاف الدور بنجاح";
            return RedirectToAction(nameof(Index));
        }
        // ── Restore ─────────────────────────
        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            if (!IsAdmin()) return AccessDenied();

            var role = await _context.Roles.GetByIdAsync(id);
            if (role == null) return NotFound();

            role.IsDeleted = false;
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم استعادة الدور بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── Deleted List ────────────────────
        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "الأدوار الموقوفة";

            var allRoles = await _context.Roles.GetAllAsync();
            var roles = allRoles
                .Where(r => r.IsDeleted)
                .ToList();

            return View("Delete",roles);
        }
    }
}

