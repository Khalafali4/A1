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
    public class DonorDetailsController : BaseController
    {
        
            private readonly IUnitOfWork _context;

            public DonorDetailsController(IUnitOfWork context)
            {
                _context = context;
            }

        // ── GET: Donors ─────────────────────
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "المتبرعون";

            // ✅ متبرع يرى بياناته فقط
            if (IsDonor())
            {
                var userId = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");
                var myDetail = await _context.DonorDetails
                    .GetByIdAsync(userId);

                var users = await _context.Users.GetAllAsync();
                ViewData["UsersDict"] = users
                    .ToDictionary(u => u.UserId, u => u.FullName);

                var list = myDetail != null
                    ? new List<DonorDetail> { myDetail }
                    : new List<DonorDetail>();

                return View(list);
            }

            // Admin يرى الكل
            if (!IsAdmin()) return AccessDenied();

            var donors = await _context.DonorDetails.GetAllAsync();
            var allUsers = await _context.Users.GetAllAsync();
            ViewData["UsersDict"] = allUsers
                .ToDictionary(u => u.UserId, u => u.FullName);

            return View(donors);
        }

        // ── GET: Donors/Create ──────────────
        public IActionResult Create(int userId = 0)
            {
                ViewData["Title"] = "إضافة متبرع";
                var donor = new DonorDetail { UserId = userId };
                return View(donor);
            }

        // ── POST: Donors/Create ─────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonorDetail donor)
        {
            ModelState.Remove("User");
            ModelState.Remove("TotalDonatedAmount");

            if (ModelState.IsValid)
            {
                await _context.DonorDetails.AddAsync(donor);
                await _context.SaveChangesAsync();

                // ✅ إضافة تلقائية للتبرعات
                if ((donor.PreferredDonationType == "مالي" ||
                     donor.PreferredDonationType == "كلاهما") &&
                     donor.TotalDonatedAmount.HasValue &&
                     donor.TotalDonatedAmount > 0)
                {
                    var donation = new Donation
                    {
                        UserId = donor.UserId,
                        Amount = donor.TotalDonatedAmount.Value,
                        DonationDate = DateTime.Now,
                        Note = "تبرع أولي عند التسجيل"
                    };
                    await _context.Donations.AddAsync(donation);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "تم تسجيل المتبرع بنجاح";
                return RedirectToAction(nameof(Index));
            }
            return View(donor);
        }

        // ── GET: Donors/Edit/5 ──────────────
        public async Task<IActionResult> Edit(int id)
            {
                ViewData["Title"] = "تعديل بيانات المتبرع";
                var donor = await _context.DonorDetails.GetByIdAsync(id);
                if (donor == null) return NotFound();
                return View(donor);
            }

            // ── POST: Donors/Edit/5 ─────────────
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, DonorDetail donor)
            {
                if (id != donor.UserId) return NotFound();
                ModelState.Remove("User");

                if (ModelState.IsValid)
                {
                    _context.DonorDetails.Update(donor);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تعديل بيانات المتبرع بنجاح";
                    return RedirectToAction(nameof(Index));
                }
                return View(donor);
            }

            // ── POST: Donors/Delete/5 ───────────
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var donor = await _context.DonorDetails.GetByIdAsync(id);
                if (donor == null) return NotFound();
                _context.DonorDetails.Delete(donor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف المتبرع بنجاح";
                return RedirectToAction(nameof(Index));
            }
      
    }

}
