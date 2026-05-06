            using Da3m.Data.Repositories;
            using Da3m.Domain;
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

                    // ── GET: Users/Create ───────────────
                    public async Task<IActionResult> Create()
                    {
                        if (!IsAdmin()) return AccessDenied();
                        ViewData["Title"] = "إضافة مستخدم";
                        ViewBag.Roles = await _context.Roles.GetAllAsync();
                        return View();
                    }

                    // ── POST: Users/Create ──────────────
                    [HttpPost]
                    [ValidateAntiForgeryToken]
                    public async Task<IActionResult> Create(User user)
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

                        if (ModelState.IsValid)
                        {
                            user.CreatedAt = DateTime.Now;
                            user.IsDeleted = false; // ✅ مهم
                            await _context.Users.AddAsync(user);
                            await _context.SaveChangesAsync();

                            var role = await _context.Roles
                                .GetByIdAsync(user.RoleId);
                            var roleName = role?.RoleName
                                ?.ToLower() ?? "";

                            TempData["Success"] =
                                "تم إضافة المستخدم — أكمل البيانات";

                            return roleName switch
                            {
                                "doctor" => RedirectToAction(
                                    "Create", "Doctors",
                                    new { userId = user.UserId }),
                                "patient" => RedirectToAction(
                                    "Create", "PatientDetails",
                                    new { userId = user.UserId }),
                                "donor" => RedirectToAction(
                                    "Create", "DonorDetails",
                                    new { userId = user.UserId }),
                                "manufacturer" => RedirectToAction(
                                    "Create", "ManufacturerDetails",
                                    new { userId = user.UserId }),
                                _ => RedirectToAction(nameof(Index))
                            };
                        }

                        ViewBag.Roles = await _context.Roles.GetAllAsync();
                        return View(user);
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

                        return View( "Delete",users);
                    }
                    //[HttpPost]
                    //[ActionName("Details")]
                    //public async Task<IActionResult> Details(int id)
                    //{
                    //    if (!IsAdmin()) return AccessDenied();

                    //    var users = await _context.Users.GetAllAsync();
                    //    var user = users.FirstOrDefault(u => u.UserId == id);
                    //    return View(user);

                    //}
                }
            }