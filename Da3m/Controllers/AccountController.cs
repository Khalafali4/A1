using Da3m.Data.Repositories;
using Da3m.Domain;
using Microsoft.AspNetCore.Mvc;
using Da3m.Helpers;

namespace Da3m.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _context;

        public AccountController(IUnitOfWork context)
        {
            _context = context;
        }

        // ── GET: Login ──────────────────────
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // ── POST: Login ─────────────────────


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email, string password)
        {
            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "يرجى إدخال البريد وكلمة المرور";
                return View();
            }

            // ✅ جيب المستخدم بالإيميل فقط
            var users = await _context.Users.FindAsync(u =>
                u.Email == email && !u.IsDeleted);

            var user = users.FirstOrDefault();

            if (user == null)
            {
                ViewBag.Error =
                    "البريد الإلكتروني أو كلمة المرور غير صحيحة";
                return View();
            }

            // ✅ تحقق من كلمة المرور
            bool isValid;

            if (PasswordHelper.IsHashed(user.Password))
            {
                // ✅ كلمة مرور مشفّرة — استخدم Verify
                isValid = PasswordHelper.Verify(
                    password, user.Password);
            }
            else
            {
                // ✅ كلمة مرور قديمة — قارن مباشرة
                // وشفّرها تلقائياً للمستقبل
                isValid = user.Password == password;
                if (isValid)
                {
                    user.Password = PasswordHelper.Hash(password);
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }
            }

            if (!isValid)
            {
                ViewBag.Error =
                    "البريد الإلكتروني أو كلمة المرور غير صحيحة";
                return View();
            }
            if(user.MustChangePassword)
            {
                HttpContext.Session.SetString("MustChangePassword", "true");
            }
            else
            {
                HttpContext.Session.SetString("MustChangePassword", "false");
            }

            // ✅ سجّل الجلسة
            var roles = await _context.Roles.GetAllAsync();
            var role = roles.FirstOrDefault(r =>
                r.RoleId == user.RoleId);
            // بعد تسجيل الدخول الناجح
            HttpContext.Session.SetString("UserId",
                user.UserId.ToString());
            HttpContext.Session.SetString("UserName",
                user.FullName);
            HttpContext.Session.SetString("UserEmail",
                user.Email);
            HttpContext.Session.SetString("RoleName",
                role?.RoleName ?? "—");

            // ✅ تحقق من اكتمال البيانات
            var roleLower = role?.RoleName?.ToLower() ?? "";
            bool isCompleted = true;

            if (roleLower == "patient")
            {
                var detail = await _context.PatientDetails
                    .GetByIdAsync(user.UserId);
                isCompleted = detail != null;
            }
            else if (roleLower == "doctor")
            {
                var detail = await _context.Doctors
                    .GetByIdAsync(user.UserId);
                isCompleted = detail != null;
            }
            else if (roleLower == "donor")
            {
                var detail = await _context.DonorDetails
                    .GetByIdAsync(user.UserId);
                isCompleted = detail != null;
            }
            else if (roleLower == "manufacturer")
            {
                var detail = await _context.ManufacturerDetails
                    .GetByIdAsync(user.UserId);
                isCompleted = detail != null;
            }

            HttpContext.Session.SetString("ProfileCompleted",
                isCompleted ? "true" : "false");

            if (!isCompleted)
            {
                TempData["Warning"] =
                    "يرجى إكمال بياناتك أولاً";
                return roleLower switch
                {
                    "doctor" => RedirectToAction(
                        "Complete", "Doctors",
                        new { userId = user.UserId }),
                    "patient" => RedirectToAction(
                        "Complete", "PatientDetails",
                        new { userId = user.UserId }),
                    "donor" => RedirectToAction(
                        "Complete", "DonorDetails",
                        new { userId = user.UserId }),
                    "manufacturer" => RedirectToAction(
                        "Complete", "ManufacturerDetails",
                        new { userId = user.UserId }),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            return RedirectToAction("Index", "Home");
        }
        // AccountController — Register GET
        public async Task<IActionResult> Register()
        {
            var roles = await _context.Roles.GetAllAsync();

            // ✅ إخفاء Admin من قائمة التسجيل
            var filteredRoles = roles.Where(r =>
                r.RoleName.ToLower() != "admin" &&
                !r.IsDeleted).ToList();

            ViewBag.Roles = filteredRoles;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string fullName, string email,
            string password, string confirmPassword,
            string phone, int roleId,
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
            // ✅ تفاصيل المتبرع
            string? PreferredDonationType,
            // ✅ تفاصيل المصنّع
            string? CompanyName,
            string? CommercialRegister,
            string? Website)
        {
            // ✅ تحقق أساسي
            if (password != confirmPassword)
            {
                ViewBag.Error = "كلمتا المرور غير متطابقتين";
                ViewBag.Roles = await GetFilteredRoles();
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.Error =
                    "كلمة المرور 6 أحرف على الأقل";
                ViewBag.Roles = await GetFilteredRoles();
                return View();
            }

            var existing = await _context.Users
                .FindAsync(u => u.Email == email);
            if (existing.Any())
            {
                ViewBag.Error = "البريد مسجل مسبقاً";
                ViewBag.Roles = await GetFilteredRoles();
                return View();
            }

            // ✅ احفظ المستخدم
            var user = new User
            {
                FullName = fullName,
                Email = email,
                Password = PasswordHelper.Hash(password),
                Phone = phone,
                RoleId = roleId,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // ✅ احفظ التفاصيل
            var roles = await _context.Roles.GetAllAsync();
            var role = roles.FirstOrDefault(r =>
                r.RoleId == roleId);
            var roleLower = role?.RoleName?.ToLower() ?? "";

            try
            {
                if (roleLower == "patient" ||
                    roleLower == "مريض")
                {
                    DateOnly bd = DateOnly.TryParse(
                        BirthDateStr, out var d)
                        ? d : DateOnly.FromDateTime(DateTime.Now);

                    await _context.PatientDetails.AddAsync(
                        new PatientDetail
                        {
                            UserId = user.UserId,
                            NationalId = NationalId ?? "—",
                            BirthDate = bd,
                            Gender = Gender,
                            Address = Address,
                            DisabilityType = DisabilityType,
                            IsDeleted = false
                        });
                }
                else if (roleLower == "doctor" ||
                         roleLower == "طبيب")
                {
                    await _context.Doctors.AddAsync(new Doctor
                    {
                        UserId = user.UserId,
                        Specialty = Specialty,
                        LicenseNumber = LicenseNumber,
                        HospitalName = HospitalName,
                        IsDeleted = false
                    });
                }
                else if (roleLower == "donor" ||
                         roleLower == "متبرع")
                {
                    await _context.DonorDetails.AddAsync(
                        new DonorDetail
                        {
                            UserId = user.UserId,
                            PreferredDonationType = PreferredDonationType,
                            IsDeleted = false,
                            TotalDonatedAmount = 0,
                            DonatedDevicesCount = 0
                        });
                }
                else if (roleLower == "manufacturer" ||
                         roleLower == "مصنع")
                {
                    await _context.ManufacturerDetails.AddAsync(
                        new ManufacturerDetail
                        {
                            UserId = user.UserId,
                            CompanyName = CompanyName ?? "—",
                            CommercialRegister = CommercialRegister,
                            Website = Website,
                            IsDeleted = false
                        });
                }

                await _context.SaveChangesAsync();
            }
            catch
            {
                // ✅ فشل → احذف المستخدم
                _context.Users.Delete(user);
                await _context.SaveChangesAsync();
                ViewBag.Error = "حدث خطأ — حاول مجدداً";
                ViewBag.Roles = await GetFilteredRoles();
                return View();
            }// ✅ تسجيل دخول تلقائي
            HttpContext.Session.SetString("UserId",
                user.UserId.ToString());
            HttpContext.Session.SetString("UserName",
                user.FullName);
            HttpContext.Session.SetString("UserEmail",
                user.Email);
            HttpContext.Session.SetString("RoleName",
                role?.RoleName ?? "—");
            HttpContext.Session.SetString(
                "ProfileCompleted", "true");

            TempData["Success"] = "مرحباً! تم إنشاء حسابك";
            return RedirectToAction("Index", "Home");
        }

        private async Task<IEnumerable<Da3m.Domain.Role>>
            GetFilteredRoles()
        {
            var roles = await _context.Roles.GetAllAsync();
            return roles.Where(r =>
                r.RoleName.ToLower() != "admin" &&
                !r.IsDeleted);
        }
        // ── Access Denied ───────────────────
        //GET: /Account/Denied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ── Logout ──────────────────────────
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ── GET: Profile ────────────────────
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(HttpContext.Session
                .GetString("UserId") ?? "0");

            var user = await _context.Users.GetByIdAsync(userId);
            if (user == null) return NotFound();

            var role = HttpContext.Session
                .GetString("RoleName") ?? "";

            // ✅ جيب البيانات التفصيلية حسب الدور
            if (role.ToLower() == "patient" ||
                role == "مريض")
            {
                var detail = await _context.PatientDetails
                    .GetByIdAsync(userId);
                ViewBag.PatientDetail = detail;

                var measurements = await _context.Measurements
                    .FindAsync(m => m.UserId == userId);
                ViewBag.Measurements = measurements
                    .OrderByDescending(m => m.MeasuredAt)
                    .ToList();

                var matches = await _context.Matches
                    .FindAsync(m => m.UserId == userId);
                ViewBag.Matches = matches
                    .OrderByDescending(m => m.MatchDate)
                    .ToList();

                var devices = await _context.Prostheses.GetAllAsync();
                ViewBag.DevicesDict = devices
                    .ToDictionary(d => d.DeviceId, d => d.LimbType);
            }
            else if (role.ToLower() == "doctor" ||
                     role == "طبيب")
            {
                var detail = await _context.Doctors
                    .GetByIdAsync(userId);
                ViewBag.DoctorDetail = detail;

                var centers = await _context.Centers.GetAllAsync();
                ViewBag.CentersDict = centers
                    .ToDictionary(c => c.CenterId, c => c.CenterName);
            }
            else if (role.ToLower() == "donor" ||
                     role == "متبرع")
            {
                var detail = await _context.DonorDetails
                    .GetByIdAsync(userId);
                ViewBag.DonorDetail = detail;

                var donations = await _context.Donations
                    .FindAsync(d => d.UserId == userId);
                ViewBag.MyDonations = donations
                    .OrderByDescending(d => d.DonationDate)
                    .ToList();
            }
            else if (role.ToLower() == "manufacturer" ||
                     role == "مصنع")
            {
                var detail = await _context.ManufacturerDetails
                    .GetByIdAsync(userId);
                ViewBag.ManufacturerDetail = detail;

                var devices = await _context.Prostheses
                    .FindAsync(p => p.UserId == userId);
                ViewBag.MyDevices = devices.ToList();
            }

            return View(user);
        }

        // ── GET: EditProfile ─────────────────
        public async Task<IActionResult> EditProfile()
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            var user = await _context.Users.GetByIdAsync(userId);
            if (user == null) return NotFound();

            return View(user);
        }

        // ── POST: EditProfile ────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(User user)
        {
            var userId = int.Parse(HttpContext.Session
                .GetString("UserId") ?? "0");

            if (user.UserId != userId) return NotFound();

            ModelState.Remove("Role");
            ModelState.Remove("Doctor");
            ModelState.Remove("PatientDetail");
            ModelState.Remove("DonorDetail");
            ModelState.Remove("ManufacturerDetail");
            ModelState.Remove("Donations");
            ModelState.Remove("Matches");
            ModelState.Remove("Measurements");
            ModelState.Remove("Prostheses");
            ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                // ✅ جيب المستخدم الحالي للحصول على كلمة المرور
                var currentUser = await _context.Users
                    .GetByIdAsync(userId);
                if (currentUser == null) return NotFound();

                // ✅ حدّث البيانات
                currentUser.FullName = user.FullName;
                currentUser.Email = user.Email;
                currentUser.Phone = user.Phone;

                // ✅ غيّر كلمة المرور فقط إذا أدخل واحدة جديدة
                if (!string.IsNullOrEmpty(user.Password))
                {
                    currentUser.Password =
                        PasswordHelper.Hash(user.Password);
                }

                _context.Users.Update(currentUser);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("UserName",
                    currentUser.FullName);

                TempData["Success"] = "تم تحديث بياناتك بنجاح";
                return RedirectToAction(nameof(Profile));
            }

            return View(user);
        }

        // ── GET: ChangePassword ─────────────────
        public IActionResult ChangePassword()
        {
            ViewData["Title"] = "تغيير كلمة المرور";
            return View();
        }

        // ── POST: ChangePassword ────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            string currentPassword,
            string newPassword,
            string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "كلمتا المرور غير متطابقتين";
                return View();
            }

            if (newPassword.Length < 8)
            {
                ViewBag.Error =
                    "كلمة المرور يجب أن تكون 8 أحرف على الأقل";
                return View();
            }

            var userId = int.Parse(HttpContext.Session
                .GetString("UserId") ?? "0");
            
            var user = await _context.Users.GetByIdAsync(userId);
            if (user == null) return NotFound();

            // ✅ تحقق من كلمة المرور الحالية
            if (!PasswordHelper.Verify(currentPassword,
                user.Password))
            {
                ViewBag.Error = "كلمة المرور الحالية غير صحيحة";
                return View();
            }

            // ✅ تحديث كلمة المرور
            user.Password = PasswordHelper.Hash(newPassword);
            user.MustChangePassword = false;
            
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString(
                "MustChangePassword", "false");

            TempData["Success"] =
                "تم تغيير كلمة المرور بنجاح";
            return RedirectToAction("Index", "Home");
        }
    }
}