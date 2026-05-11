using Da3m.Data.Repositories;
using Da3m.Domain;
using Da3m.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Da3m.Controllers
{
    public class UsersController : BaseController
    {
        private readonly IUnitOfWork _context;

        public UsersController(IUnitOfWork context)
        {
            _context = context;
        }

        // ── GET: Users ──────────────────────
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "المستخدمون";

            var allUsers = await _context.Users.GetAllAsync();
            var users = allUsers
                .Where(u => !u.IsDeleted)
                .ToList();

            // load roles and expose a dictionary to the view
            var roles = await _context.Roles.GetAllAsync();
            ViewData["RolesDict"] = roles
                .ToDictionary(r => r.RoleId, r => r.RoleName);

            return View(users);
        }
        // ── GET: Create ─────────────────────
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "إضافة مستخدم";

            var roles = await _context.Roles.GetAllAsync();
            // ✅ بدون Admin
            ViewBag.Roles = roles
                .Where(r => r.RoleName.ToLower() != "admin"
                         && !r.IsDeleted)
                .ToList();

            var centers = await _context.Centers
                .FindAsync(c => c.IsActive);
            ViewBag.Centers = centers.ToList();

            return View();
        }

        // ── POST: Create ────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            User user,
            // ✅ تفاصيل المريض
            string? NationalId,
            string? BirthDateStr,
            string? Gender,
            string? Address,
            string? DisabilityType,
            // ✅ تفاصيل الطبيب
            string? Specialty,
            string? LicenseNumber,
            string? HospitalName,
            int? CenterId,
            // ✅ تفاصيل المتبرع
            string? PreferredDonationType,
            // ✅ تفاصيل المصنّع
            string? CompanyName,
            string? CommercialRegister,
            string? Website)
        {
            if (!IsAdmin()) return AccessDenied();

            ModelState.Remove("Role");
            ModelState.Remove("Doctor");
            ModelState.Remove("PatientDetail");
            ModelState.Remove("DonorDetail");
            ModelState.Remove("ManufacturerDetail");
            ModelState.Remove("Donations");
            ModelState.Remove("Matches");
            ModelState.Remove("Measurements");
            ModelState.Remove("Prostheses");

            // ✅ تحقق من الإيميل
            var existing = await _context.Users.FindAsync(u =>
                u.Email == user.Email && !u.IsDeleted);
            if (existing.Any())
            {
                ModelState.AddModelError("Email",
                    "البريد الإلكتروني مسجل مسبقاً");
            }

            // ✅ تحقق من التفاصيل حسب الدور
            var roles = await _context.Roles.GetAllAsync();
            var role = roles.FirstOrDefault(r =>
                r.RoleId == user.RoleId);
            var roleLower = role?.RoleName?.ToLower() ?? "";

            if (roleLower == "patient" || roleLower == "مريض")
            {
                if (string.IsNullOrEmpty(NationalId))
                    ModelState.AddModelError("NationalId",
                        "الرقم الوطني مطلوب");
                if (string.IsNullOrEmpty(DisabilityType))
                    ModelState.AddModelError("DisabilityType",
                        "نوع الإعاقة مطلوب");
            }
            else if (roleLower == "doctor" || roleLower == "طبيب")
            {
                if (string.IsNullOrEmpty(Specialty))
                    ModelState.AddModelError("Specialty",
                        "التخصص مطلوب");
            }
            else if (roleLower == "manufacturer" ||
                     roleLower == "مصنع")
            {
                if (string.IsNullOrEmpty(CompanyName))
                    ModelState.AddModelError("CompanyName",
                        "اسم الشركة مطلوب");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = roles
                    .Where(r => r.RoleName.ToLower() != "admin"
                             && !r.IsDeleted).ToList();
                var centers = await _context.Centers
                    .FindAsync(c => c.IsActive);
                ViewBag.Centers = centers.ToList();
                return View(user);
            }

            // ✅ احفظ User
            user.CreatedAt = DateTime.Now;
            user.IsDeleted = false;
            user.Password = BCrypt.Net.BCrypt
                .HashPassword(user.Password);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // ✅ احفظ التفاصيل حسب الدور في نفس العملية
            try
            {
                if (roleLower == "patient" ||
                    roleLower == "مريض")
                {
                    DateOnly birthDate = DateOnly.TryParse(
                        BirthDateStr, out var bd) ? bd :
                        DateOnly.FromDateTime(DateTime.Now);

                    var patient = new PatientDetail
                    {
                        UserId = user.UserId,
                        NationalId = NationalId!,
                        BirthDate = birthDate,
                        Gender = Gender,
                        Address = Address,
                        DisabilityType = DisabilityType,
                        IsDeleted = false
                    };
                    await _context.PatientDetails.AddAsync(patient);
                }
                else if (roleLower == "doctor" ||
                         roleLower == "طبيب")
                {
                    var doctor = new Doctor
                    {
                        UserId = user.UserId,
                        Specialty = Specialty,
                        LicenseNumber = LicenseNumber,
                        HospitalName = HospitalName,
                        CenterId = CenterId,
                        IsDeleted = false
                    };
                    await _context.Doctors.AddAsync(doctor);
                }
                else if (roleLower == "donor" ||
                         roleLower == "متبرع")
                {
                    var donor = new DonorDetail
                    {
                        UserId = user.UserId,
                        PreferredDonationType = PreferredDonationType,
                        IsDeleted = false,
                        TotalDonatedAmount = 0,
                        DonatedDevicesCount = 0
                    };
                    await _context.DonorDetails.AddAsync(donor);
                }
                else if (roleLower == "manufacturer" ||
                         roleLower == "مصنع")
                {
                    var mfr = new ManufacturerDetail
                    {
                        UserId = user.UserId,
                        CompanyName = CompanyName!,
                        CommercialRegister = CommercialRegister,
                        Website = Website,
                        IsDeleted = false
                    };
                    await _context.ManufacturerDetails.AddAsync(mfr);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] =
                    "تم إضافة المستخدم بنجاح";
            }
            catch
            {
                // ✅ إذا فشل حفظ التفاصيل — احذف المستخدم
                _context.Users.Delete(user);
                await _context.SaveChangesAsync();

                TempData["Error"] =
                    "حدث خطأ أثناء الحفظ — حاول مجدداً";
                ViewBag.Roles = roles
                    .Where(r => r.RoleName.ToLower() != "admin"
                             && !r.IsDeleted).ToList();
                var centers = await _context.Centers
                    .FindAsync(c => c.IsActive);
                ViewBag.Centers = centers.ToList();
                return View(user);
            }

            return RedirectToAction(nameof(Index));
        }

        // ── GET: Users/Edit/5 ───────────────
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "تعديل المستخدم";

            var user = await _context.Users.GetByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.Roles = await _context.Roles.GetAllAsync();
            return View(user);
        }

        // ── POST: Users/Edit/5 ──────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (!IsAdmin()) return AccessDenied();
            if (id != user.UserId) return NotFound(); ModelState.Remove("Role");
            ModelState.Remove("Doctor");
            ModelState.Remove("PatientDetail");
            ModelState.Remove("DonorDetail");
            ModelState.Remove("ManufacturerDetail");
            ModelState.Remove("Donations");
            ModelState.Remove("Matches");
            ModelState.Remove("Measurements");
            ModelState.Remove("Prostheses");

            if (ModelState.IsValid)
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                TempData["Success"] =
                    "تم تعديل المستخدم بنجاح";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = await _context.Roles.GetAllAsync();
            return View(user);
        }

        // ── Soft Delete ─────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return AccessDenied();

            var user = await _context.Users.GetByIdAsync(id);
            if (user == null) return NotFound();

            user.IsDeleted = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إيقاف المستخدم بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── Restore ─────────────────────────
        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            if (!IsAdmin()) return AccessDenied();

            var user = await _context.Users.GetByIdAsync(id);
            if (user == null) return NotFound();

            user.IsDeleted = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم استعادة المستخدم بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── Deleted List ────────────────────
        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            if (!IsAdmin()) return AccessDenied(); // ✅ !IsAdmin

            ViewData["Title"] = "المستخدمون الموقوفون";

            var allUsers = await _context.Users.GetAllAsync();
            var users = allUsers
                .Where(u => u.IsDeleted)
                .ToList();

            var roles = await _context.Roles.GetAllAsync();
            ViewData["RolesDict"] = roles
                .ToDictionary(r => r.RoleId, r => r.RoleName);

            return View("Delete", users);
        }
     
        [ActionName("Details")]
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAdmin()) return AccessDenied();
            ViewData["Title"] = "تفاصيل المستخدم";

            // 1. جلب بيانات المستخدم الأساسية
            var user = await _context.Users.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
                return NotFound();

            // 2. جلب الدور
            var roles = await _context.Roles.GetAllAsync();
            var role = roles.FirstOrDefault(r => r.RoleId == user.RoleId);
            ViewBag.RoleName = role?.RoleName ?? "—";

            var roleName = role?.RoleName?.ToLower() ?? "";

            // وضع مؤشر افتراضي بأن الملف مكتمل، ونغيره لو لم نجد السجل الفرعي
            ViewBag.IsProfileIncomplete = false;

            // ✅ جلب التفاصيل الذكية حسب الدور مع تلافي الـ Null
            if (roleName == "doctor")
            {
                // استخدام دالة البحث بالشرط لضمان المطابقة مع الـ UserId في جدول الأطباء
                var doctors = await _context.Doctors.FindAsync(d => d.UserId == id);
                var doctor = doctors.FirstOrDefault();

                if (doctor == null || doctor.IsDeleted)
                {
                    ViewBag.IsProfileIncomplete = true;
                    ViewBag.IncompleteMessage = "هذا الحساب مسجل كطبيب، ولكن لم يتم استكمال ملف البيانات المهنية له بعد.";
                }
                else
                {
                    ViewBag.DoctorDetail = doctor;
                    if (doctor.CenterId != null)
                    {
                        var center = await _context.Centers.GetByIdAsync(doctor.CenterId.Value);
                        ViewBag.CenterName = center?.CenterName ?? "—";
                    }
                }
            }
            else if (roleName == "patient")
            {
                var patients = await _context.PatientDetails.FindAsync(p => p.UserId == id);
                var patient = patients.FirstOrDefault();

                if (patient == null)
                {
                    ViewBag.IsProfileIncomplete = true;
                    ViewBag.IncompleteMessage = "هذا الحساب مسجل كمريض، ولكن لم يتم إنشاء ملف طبي تفصيلي له بعد.";
                    ViewBag.MeasurementsCount = 0;
                    ViewBag.MatchesCount = 0;
                }
                else
                {
                    ViewBag.PatientDetail = patient;
                    var measurements = await _context.Measurements.FindAsync(m => m.UserId == id);
                    ViewBag.MeasurementsCount = measurements.Count();

                    var matches = await _context.Matches.FindAsync(m => m.UserId == id);
                    ViewBag.MatchesCount = matches.Count();
                }
            }
            else if (roleName == "donor")
            {
                var donors = await _context.DonorDetails.FindAsync(d => d.UserId == id);
                var donor = donors.FirstOrDefault();

                if (donor == null)
                {
                    ViewBag.IsProfileIncomplete = true;
                    ViewBag.IncompleteMessage = "لم يتم إدخال البيانات الشخصية وجدول التبرعات الخاص بهذا المتبرع بعد.";
                    ViewBag.DonationsCount = 0;
                    ViewBag.TotalDonations = 0;
                }
                else
                {
                    ViewBag.DonorDetail = donor;
                    var donations = await _context.Donations.FindAsync(d => d.UserId == id);
                    ViewBag.DonationsCount = donations.Count();
                    ViewBag.TotalDonations = donations.Sum(d => d.Amount);
                }
            }
            else if (roleName == "manufacturer")
            {
                var manufacturers = await _context.ManufacturerDetails.FindAsync(m => m.UserId == id);
                var manufacturer = manufacturers.FirstOrDefault();

                if (manufacturer == null)
                {
                    ViewBag.IsProfileIncomplete = true;
                    ViewBag.IncompleteMessage = "لم يتم استكمال بيانات جهة التصنيع أو المعمل المرتبط بهذا الحساب.";
                    ViewBag.DevicesCount = 0;
                }
                else
                {
                    ViewBag.ManufacturerDetail = manufacturer;
                    var devices = await _context.Prostheses.FindAsync(p => p.UserId == id);
                    ViewBag.DevicesCount = devices.Count();
                }
            }

            return View(user);
        }
    }
}