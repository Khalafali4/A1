using Da3m.Data.Repositories;
using Da3m.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Da3m.Controllers
{
    public class ManufacturerDetailsController : BaseController
    {
        
            private readonly IUnitOfWork _context;

            public ManufacturerDetailsController(IUnitOfWork context)
            {
                _context = context;
            }

            // ── GET: Manufacturers ──────────────
            public async Task<IActionResult> Index()
            {
                ViewData["Title"] = "المصنّعون";
                var manufacturers = await _context.ManufacturerDetails.GetAllAsync();

                var users = await _context.Users.GetAllAsync();
                ViewData["UsersDict"] = users
                    .ToDictionary(u => u.UserId, u => u.FullName);

                return View(manufacturers
                    ?? new List<ManufacturerDetail>());
            }

            // ── GET: Manufacturers/Create ───────
            public IActionResult Create(int userId = 0)
            {
                //ModelState.Remove("User");
                ViewData["Title"] = "إضافة مصنّع";
                var manufacturer = new ManufacturerDetail
                {
                    UserId = userId
                };
                return View(manufacturer);
            }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ManufacturerDetail manufacturer, int userId = 0)
        {
            if (userId != 0) manufacturer.UserId = userId;

            if (manufacturer.UserId == 0)
                ModelState.AddModelError("UserId", "UserId is required.");

            if (!ModelState.IsValid)
            {
                ViewBag.Users = await _context.Users.GetAllAsync();
                return View(manufacturer);
            }

            await _context.ManufacturerDetails.AddAsync(manufacturer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // ── GET: Manufacturers/Edit/5 ───────
        public async Task<IActionResult> Edit(int id)
            {
                ViewData["Title"] = "تعديل بيانات المصنّع";
                var manufacturer = await _context.ManufacturerDetails.GetByIdAsync(id);
                if (manufacturer == null) return NotFound();
                return View(manufacturer);
            }

            // ── POST: Manufacturers/Edit/5 ──────
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(
                int id, ManufacturerDetail manufacturer)
            {
                if (id != manufacturer.UserId) return NotFound();
                ModelState.Remove("User");

                if (ModelState.IsValid)
                {
                    _context.ManufacturerDetails.Update(manufacturer);
                    await _context.SaveChangesAsync ();
                    TempData["Success"] = "تم تعديل بيانات المصنّع بنجاح";
                    return RedirectToAction(nameof(Index));
                }
                return View(manufacturer);
            }

            // ── POST: Manufacturers/Delete/5 ────
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var manufacturer = await _context.ManufacturerDetails
                                             .GetByIdAsync(id);
                if (manufacturer == null) return NotFound();
                _context.ManufacturerDetails.Delete(manufacturer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف المصنّع بنجاح";
                return RedirectToAction(nameof(Index));
            }
      
    }
 
}


