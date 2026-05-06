using Da3m.Data.Repositories;
using Da3m.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Da3m.Controllers
{
    public class ProsthesesController : BaseController
    {
        private readonly IUnitOfWork _context;

        public ProsthesesController(IUnitOfWork context)
        {
            _context = context;
        }

        // ── GET: Index ──────────────────────
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "الأجهزة التعويضية";

            var userId = int.Parse(HttpContext.Session
                .GetString("UserId") ?? "0");

            if (IsDonor() || IsManufacturer())
            {
                var myDevices = await _context.Prostheses
                    .FindAsync(p => p.UserId == userId);

                var users = await _context.Users.GetAllAsync();
                ViewData["UsersDict"] = users
                    .ToDictionary(u => u.UserId, u => u.FullName);

                return View(myDevices);
            }

            if (!IsAdmin()) return AccessDenied();

            var allDevices = await _context.Prostheses.GetAllAsync();
            var allUsers = await _context.Users.GetAllAsync();
            ViewData["UsersDict"] = allUsers
                .ToDictionary(u => u.UserId, u => u.FullName);

            return View(allDevices);
        }

        // ── GET: Create ─────────────────────
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "إضافة جهاز تعويضي";

            if (IsDonor() || IsManufacturer())
            {
                var userId = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");
                ViewBag.OwnerId = userId;
                ViewBag.OwnerName = HttpContext.Session
                    .GetString("UserName");
                return View();
            }

            // ✅ Admin يختار المالك من كل المستخدمين
            var users = await _context.Users.GetAllAsync();
            ViewData["Users"] = users
                .Where(u => !u.IsDeleted)
                .ToList();
            return View();
        }

        // ── POST: Create ────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Prostheses device)
        {
            ModelState.Remove("User");
            ModelState.Remove("Matches");

            // ✅ Admin يحتاج UserId من الـ Form
            // Donor/Manufacturer يأخذون من Session
            if (IsDonor() || IsManufacturer())
            {
                device.UserId = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");
                // ✅ أزل validation الـ UserId لأنه من Session
                ModelState.Remove("UserId");
            }

            if (ModelState.IsValid)
            {
                device.AddedAt = DateTime.Now;
                await _context.Prostheses.AddAsync(device);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إضافة الجهاز بنجاح";
                return RedirectToAction(nameof(Index));
            }

            // ✅ أعد تحميل Users إذا Admin
            if (IsAdmin())
            {
                var users = await _context.Users.GetAllAsync();
                ViewData["Users"] = users
                    .Where(u => !u.IsDeleted)
                    .ToList();
            }
            else
            {
                ViewBag.OwnerId = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");
                ViewBag.OwnerName = HttpContext.Session
                    .GetString("UserName");
            }

            return View(device);
        }

        // ── GET: Edit ───────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "تعديل الجهاز";

            var device = await _context.Prostheses.GetByIdAsync(id);
            if (device == null) return NotFound();
            // ✅ Donor/Manufacturer يعدّلون أجهزتهم فقط
            if (IsDonor() || IsManufacturer())
            {
                var myUserId = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");
                if (device.UserId != myUserId)
                    return AccessDenied();
                return View(device);
            }
            if (IsAdmin())
            {

                var users = await _context.Users.GetAllAsync();
                ViewData["Users"] = users
                    .Where(u => !u.IsDeleted)
                    .ToList();
            }
            return View(device);
        }

        // ── POST: Edit ──────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, Prostheses device)
        {
            if (id != device.DeviceId) return NotFound();

            ModelState.Remove("User");
            ModelState.Remove("Matches");

            if (ModelState.IsValid)
            {
                _context.Prostheses.Update(device);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تعديل الجهاز بنجاح";
                return RedirectToAction(nameof(Index));
            }

            var users = await _context.Users.GetAllAsync();
            ViewData["Users"] = users
                .Where(u => !u.IsDeleted)
                .ToList();
            return View(device);
        }

        // ── Delete ──────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin() && !IsDonor() && !IsManufacturer())
                return AccessDenied();

            var device = await _context.Prostheses.GetByIdAsync(id);
            if (device == null) return NotFound();

            // ✅ Donor/Manufacturer يحذفون أجهزتهم فقط
            if (IsDonor() || IsManufacturer())
            {
                var myUserId = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");
                if (device.UserId != myUserId)
                    return AccessDenied();
            }

            _context.Prostheses.Delete(device);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف الجهاز بنجاح";
            return RedirectToAction(nameof(Index));
        }
    }
}