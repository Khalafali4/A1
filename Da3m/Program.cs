using Da3m.Data;
using Da3m.Data.Repositories;
using Da3m.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<Da3mDbContext>(ServiceLifetime.Scoped);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var uow = scope.ServiceProvider
        .GetRequiredService<IUnitOfWork>();

    await SeedAdminAsync(uow);
}

async Task SeedAdminAsync(IUnitOfWork uow)
{
    // ✅ أنشئ دور Admin إذا ما موجود
    var roles = await uow.Roles.GetAllAsync();
    var adminRole = roles.FirstOrDefault(r =>
        r.RoleName.ToLower() == "admin");

    if (adminRole == null)
    {
        adminRole = new Role
        {
            RoleName = "Admin",
            IsDeleted = false
        };
        await uow.Roles.AddAsync(adminRole);
        await uow.SaveChangesAsync();
    }

    // ✅ أنشئ مستخدم Admin إذا ما موجود
    var users = await uow.Users.GetAllAsync();
    var adminExists = users.Any(u =>
        u.RoleId == adminRole.RoleId);

    if (!adminExists)
    {
        var admin = new User
        {
            FullName = "مدير النظام",
            Email = "admin@da3m.com",
            Password = BCrypt.Net.BCrypt
                .HashPassword("Admin@123456"),
            Phone = "0900000000",
            RoleId = adminRole.RoleId,
            CreatedAt = DateTime.Now,
            IsDeleted = false,
            MustChangePassword = true
        };

        await uow.Users.AddAsync(admin);
        await uow.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
