
using Da3m.Data.Repositories;
using Da3m.Domain;
using Microsoft.AspNetCore.Mvc;

   namespace Da3m.Controllers
    {
        public class ProsthesesController : BaseController
        {
            private readonly IUnitOfWork _context;

            public ProsthesesController(IUnitOfWork context)
            {
                _context = context;
            }

        // ── GET: Prostheses ─────────────────
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "الأجهزة التعويضية";

            var userId = int.Parse(HttpContext.Session
                .GetString("UserId") ?? "0");

            // ✅ Donor + Manufacturer يرون أجهزتهم فقط
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

            var devices = await _context.Prostheses.GetAllAsync();
            var allUsers = await _context.Users.GetAllAsync();
            ViewData["UsersDict"] = allUsers
                .ToDictionary(u => u.UserId, u => u.FullName);

            return View(devices);
        }

        // ✅ Create — المالك تلقائياً
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "إضافة جهاز تعويضي";

            // ✅ Donor + Manufacturer — هم المالك تلقائياً
            if (IsDonor() || IsManufacturer())
            {
                var userId = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");
                ViewBag.OwnerId = userId;
                ViewBag.OwnerName = HttpContext.Session
                    .GetString("UserName");
                return View("Create");
            }

            // Admin يختار المالك
            var users = await _context.Users.GetAllAsync();
            ViewData["Users"] = users.ToList();
            return View("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Prostheses device)
        {
            ModelState.Remove("User");
            ModelState.Remove("Matches");

            if (ModelState.IsValid)
            {
                // ✅ إذا Donor أو Manufacturer — UserId من Session
                if (IsDonor() || IsManufacturer())
                {
                    device.UserId = int.Parse(HttpContext.Session
                        .GetString("UserId") ?? "0");
                }

                device.AddedAt = DateTime.Now;
                await _context.Prostheses.AddAsync(device);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إضافة الجهاز بنجاح";
                return RedirectToAction(nameof(Index));
            }

            if (!IsDonor() && !IsManufacturer())
            {
                var users = await _context.Users.GetAllAsync();
                ViewData["Users"] = users.ToList();
            }
            return View(device);
        }

            // ── GET: Prostheses/Edit/5 ──────────
            public async Task<IActionResult> Edit(int id)
            {
                ViewData["Title"] = "تعديل الجهاز";
                var device = await _context.Prostheses.GetByIdAsync(id);
                if (device == null) return NotFound();

                var users = await _context.Users.GetAllAsync();
                ViewData["Users"] = users.ToList();
                return View(device);
            }

            // ── POST: Prostheses/Edit/5 ─────────
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, Prostheses device)
            {
                if (id != device.DeviceId) return NotFound();
                ModelState.Remove("User");
                ModelState.Remove("Matches");

                if (ModelState.IsValid)
                {
                    _context.Prostheses.Update(device);
                    await _context.SaveChangesAsync ();
                    TempData["Success"] = "تم تعديل الجهاز بنجاح";
                    return RedirectToAction(nameof(Index));
                }

                var users = await _context.Users.GetAllAsync();
                ViewData["Users"] = users.ToList();
                return View(device);
            }

            // ── POST: Prostheses/Delete/5 ───────
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var device = await _context.Prostheses.GetByIdAsync(id);
                if (device == null) return NotFound();
                _context.Prostheses.Delete(device);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف الجهاز بنجاح";
                return RedirectToAction(nameof(Index));
            }
        }
    }
