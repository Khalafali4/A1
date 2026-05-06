using Da3m.Data;
using Da3m.Data.Repositories;
using Da3m.Domain;
using Microsoft.AspNetCore.Mvc;
namespace Da3m.Controllers
{
    public class DonationsController : BaseController
        {
            private readonly IUnitOfWork _context;

            public DonationsController(IUnitOfWork context)
            {
                _context = context;
            }

        // ── GET: Donations ──────────────────
        public async Task<IActionResult> Index()
        {
            // ✅ المتبرع يرى تبرعاته فقط
            if (IsDonor())
            {
                var userId = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");
                var myDonations = await _context.Donations
                    .FindAsync(d => d.UserId == userId);

                var users = await _context.Users.GetAllAsync();
                ViewData["UsersDict"] = users
                    .ToDictionary(u => u.UserId, u => u.FullName);

                ViewData["Totals"] = myDonations
                    .GroupBy(d => d.UserId)
                    .ToDictionary(g => g.Key,
                        g => g.Sum(d => d.Amount));

                return View(myDonations);
            }

            // Admin يرى الكل
            if (!IsAdmin()) return AccessDenied();

            var donations = await _context.Donations.GetAllAsync();
            var allUsers = await _context.Users.GetAllAsync();
            ViewData["UsersDict"] = allUsers
                .ToDictionary(u => u.UserId, u => u.FullName);
            ViewData["Totals"] = donations
                .GroupBy(d => d.UserId)
                .ToDictionary(g => g.Key,
                    g => g.Sum(d => d.Amount));

            return View(donations);
        }

        // ✅ Create — المتبرع يتبرع باسمه تلقائياً
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "إضافة تبرع";

            // ✅ جيب فقط المستخدمين من نوع Donor
            if (IsDonor())
            {
                var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
                ViewBag.DonorUserId = userId;
                ViewBag.DonorName = HttpContext.Session.GetString("UserName");
                return View("DonorCreate");
            }
            if(IsAdmin())
            {
                var users = await _context.Users.GetAllAsync();
                ViewData["Users"] = users.Where(u => !u.IsDeleted).ToList();

                return View();
            }
            return AccessDenied();

        }

        // ── POST: Donations/Create ──────────
        [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(Donation donation)
            {
                ModelState.Remove("User");

                if (ModelState.IsValid)
                {
                    donation.DonationDate = DateTime.Now;
                    await _context.Donations.AddAsync(donation);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم إضافة التبرع بنجاح";
                    return RedirectToAction(nameof(Index));
                }

                var users = await _context.Users.GetAllAsync();
                ViewData["Users"] = users.ToList();
                return View(donation);
            }

            // ── GET: Donations/Edit/5 ───────────
            public async Task<IActionResult> Edit(int id)
            {
                ViewData["Title"] = "تعديل التبرع";
                var donation = await _context.Donations.GetByIdAsync(id);
                if (donation == null) return NotFound();

                var roles = await _context.Roles.GetAllAsync();
                var donorRole = roles.FirstOrDefault(r =>
                    r.RoleName.ToLower() == "donor" || r.RoleName == "متبرع");
               
            if(donorRole != null)
            {
                var donors = await _context.Users.FindAsync(u => u.RoleId == donorRole.RoleId && !u.IsDeleted);
                ViewData["Users"] = donors.GroupBy(u => u.UserId).Select(g => g.First()).ToList();
            }
            else
            {
                ViewData["Users"] = new List<User>();
            }

                return View(donation);
            }

            // ── POST: Donations/Edit/5 ──────────
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, Donation donation)
            {
                if (id != donation.DonationId) return NotFound();
                ModelState.Remove("User");

                if (ModelState.IsValid)
                {
                    _context.Donations.Update(donation);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تعديل التبرع بنجاح";
                    return RedirectToAction(nameof(Index));
                }

                var users = await _context.Users.GetAllAsync();
                ViewData["Users"] = users.ToList();
                return View(donation);
            }

            // ── POST: Donations/Delete/5 ────────
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var donation = await _context.Donations.GetByIdAsync(id);
                if (donation == null) return NotFound();
                _context.Donations.Delete(donation);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف التبرع بنجاح";
                return RedirectToAction(nameof(Index));
            }
        
    }
   
}


