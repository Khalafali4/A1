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

        // ── GET: Index ──────────────────────
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "التبرعات";

            var userId = int.Parse(HttpContext.Session
                .GetString("UserId") ?? "0");

            IEnumerable<Donation> donations;
            Dictionary<int, string> usersDict;

            if (IsDonor())
            {
                // ✅ متبرع يرى تبرعاته فقط
                donations = await _context.Donations
                    .FindAsync(d => d.UserId == userId
                        && !d.IsDeleted);
                var users = await _context.Users.GetAllAsync();
                usersDict = users.ToDictionary(
                    u => u.UserId, u => u.FullName);
            }
            else
            {
                if (!IsAdmin()) return AccessDenied();
                donations = await _context.Donations
                    .FindAsync(d => !d.IsDeleted);
                var users = await _context.Users.GetAllAsync();
                usersDict = users.ToDictionary(
                    u => u.UserId, u => u.FullName);
            }

            var list = donations
                .OrderByDescending(d => d.DonationDate)
                .ToList();

            ViewData["UsersDict"] = usersDict;
            ViewBag.TotalAmount = list.Sum(d => d.Amount);
            ViewBag.DonationsCount = list.Count;

            return View(list);
        }

        // ── GET: Create ─────────────────────
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "إضافة تبرع";

            if (IsDonor())
            {
                var uid = int.Parse(HttpContext.Session
                    .GetString("UserId") ?? "0");
                ViewBag.DonorId = uid;
                ViewBag.DonorName = HttpContext.Session
                    .GetString("UserName");
                return View();
            }

            if (!IsAdmin()) return AccessDenied();

            // ✅ جيب المتبرعين فقط
            var roles = await _context.Roles.GetAllAsync();
            var donorRole = roles.FirstOrDefault(r =>
                r.RoleName.ToLower() == "donor" ||
                r.RoleName == "متبرع");

            if (donorRole != null)
            {
                var donors = await _context.Users
                    .FindAsync(u =>
                        u.RoleId == donorRole.RoleId &&
                        !u.IsDeleted);
                ViewData["Users"] = donors.ToList();
            }
            else
            {
                ViewData["Users"] = new List<User>();
            }

            return View();
        }

        // ── POST: Create ────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Donation donation)
        {
            ModelState.Remove("User");

            if (IsDonor())
            {
                donation.UserId = int.Parse(
                    HttpContext.Session
                        .GetString("UserId") ?? "0");
                ModelState.Remove("UserId");
            }

            if (ModelState.IsValid)
            {
                donation.DonationDate = DateTime.Now;
                donation.IsDeleted = false;
                await _context.Donations.AddAsync(donation);
                await _context.SaveChangesAsync();

                // ✅ حدّث TotalDonatedAmount في DonorDetail
                await UpdateDonorStats(donation.UserId);

                TempData["Success"] =
                    "تم إضافة التبرع بنجاح — شكراً!";
                return RedirectToAction(nameof(Index));
            }
            if (IsAdmin())
            {
                var roles = await _context.Roles.GetAllAsync();
                var donorRole = roles.FirstOrDefault(r =>
                    r.RoleName.ToLower() == "donor" ||
                    r.RoleName == "متبرع");
                if (donorRole != null)
                {
                    var donors = await _context.Users
                        .FindAsync(u =>
                            u.RoleId == donorRole.RoleId &&
                            !u.IsDeleted);
                    ViewData["Users"] = donors.ToList();
                }
            }
            else
            {
                ViewBag.DonorId = donation.UserId;
                ViewBag.DonorName = HttpContext.Session
                    .GetString("UserName");
            }

            return View(donation);
        }

        // ── Soft Delete ─────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            if (!IsAdmin()) return AccessDenied();

            var donation = await _context.Donations
                .GetByIdAsync(id);
            if (donation == null) return NotFound();

            donation.IsDeleted = true;
            _context.Donations.Update(donation);
            await _context.SaveChangesAsync();

            // ✅ حدّث الإحصائيات بعد الحذف
            await UpdateDonorStats(donation.UserId);

            TempData["Success"] = "تم حذف التبرع";
            return RedirectToAction(nameof(Index));
        }

        // ✅ تحديث إحصائيات المتبرع تلقائياً
        private async Task UpdateDonorStats(int userId)
        {
            var donor = await _context.DonorDetails
                .GetByIdAsync(userId);
            if (donor == null) return;

            var donations = await _context.Donations
                .FindAsync(d => d.UserId == userId
                    && !d.IsDeleted);

            var devices = await _context.Prostheses
                .FindAsync(p => p.UserId == userId);

            donor.TotalDonatedAmount =
                donations.Sum(d => d.Amount);
            donor.DonatedDevicesCount = devices.Count();

            _context.DonorDetails.Update(donor);
            await _context.SaveChangesAsync();
        }
    }
}