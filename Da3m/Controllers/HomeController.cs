using Da3m.Data.Repositories;
using Da3m.Domain;
using Da3m.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace Da3m.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IUnitOfWork _context;

        public HomeController(IUnitOfWork context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "لوحة التحكم";
            var roleName = HttpContext.Session
                .GetString("RoleName") ?? "";
            var userId = int.Parse(HttpContext.Session
                .GetString("UserId") ?? "0");

            // ── مريض ────────────────────────────
            if (roleName.ToLower() == "patient" ||
                roleName == "مريض")
            {
                var myMeasurements = await _context.Measurements
                    .FindAsync(m => m.UserId == userId);
                var myMatches = await _context.Matches
                    .FindAsync(m => m.UserId == userId);

                ViewBag.MyMeasurementsCount = myMeasurements.Count();
                ViewBag.MyMatchesCount = myMatches.Count();
                ViewBag.BestMatch = myMatches
                    .OrderByDescending(m => m.MatchPercentage)
                    .FirstOrDefault();

                return View("PatientDashboard");
            }

            // ── متبرع ────────────────────────────
            if (roleName.ToLower() == "donor" ||
                roleName == "متبرع")
            {
                var myDonations = await _context.Donations
                    .FindAsync(d => d.UserId == userId);
                var myDevices = await _context.Prostheses
                    .FindAsync(p => p.UserId == userId);

                ViewBag.MyDonationsCount = myDonations.Count();
                ViewBag.MyTotalDonations = myDonations
                    .Sum(d => d.Amount);
                ViewBag.MyDevicesCount = myDevices.Count();
                ViewBag.LatestDonations = myDonations
                    .OrderByDescending(d => d.DonationDate)
                    .Take(5).ToList();

                return View("DonorDashboard");
            }

            // ── مصنّع ────────────────────────────
            if (roleName.ToLower() == "manufacturer" ||
                roleName == "مصنع")
            {
                var myDevices = await _context.Prostheses
                    .FindAsync(p => p.UserId == userId);

                ViewBag.MyDevicesCount = myDevices.Count();
                ViewBag.AvailableDevices = myDevices
                    .Count(d => d.IsAvailable);
                ViewBag.UnavailableDevices = myDevices
                    .Count(d => !d.IsAvailable);

                return View("ManufacturerDashboard");
            }

            // ── Admin + Doctor ───────────────────
            // ... الكود الموجود


            // ── إحصائيات أساسية ─────────────
            var users = (await _context.Users.GetAllAsync()).ToList();
            var doctors = (await _context.Doctors.GetAllAsync()).ToList();
            var patients = (await _context.PatientDetails.GetAllAsync()).ToList();
            var devices = (await _context.Prostheses.GetAllAsync()).ToList();
            var matches = (await _context.Matches.GetAllAsync()).ToList();
            var donations = (await _context.Donations.GetAllAsync()).ToList();
            var reports = (await _context.VisitReports.GetAllAsync()).ToList();
            var centers = (await _context.Centers.GetAllAsync()).ToList();
            var roles = (await _context.Roles.GetAllAsync()).ToList();

            // ── أرقام رئيسية ─────────────────
            ViewBag.UsersCount = users.Count();
            ViewBag.DoctorsCount = doctors.Count();
            ViewBag.PatientsCount = patients.Count();
            ViewBag.DevicesCount = devices.Count();
            ViewBag.MatchesCount = matches.Count();
            ViewBag.DonationsCount = donations.Count();
            ViewBag.ReportsCount = reports.Count();
            ViewBag.CentersCount = centers.Count();

            // ── إحصائيات متقدمة ──────────────
            ViewBag.AvailableDevices = devices.Count(d => d.IsAvailable);
            ViewBag.TotalDonations = donations.Sum(d => d.Amount);
            ViewBag.ExcellentMatches = matches.Count(m => m.MatchPercentage >= 80);
            ViewBag.ActiveCenters = centers.Count(c => c.IsActive);


            // ── Dictionaries ─────────────────
            Dictionary<int, string> usersDict = users
                .ToDictionary(u => u.UserId, u => u.FullName);
            ViewBag.UsersDict = usersDict;

            Dictionary<int, string> rolesDict = roles
                .ToDictionary(r => r.RoleId, r => r.RoleName);

            // ── آخر 5 مطابقات ────────────────
            List<Match> latestMatches = matches
                .OrderByDescending(m => m.MatchDate)
                .Take(5)
                .ToList();
            ViewBag.LatestMatches = latestMatches;

            // ── آخر 5 تبرعات ─────────────────
            List<Donation> latestDonations = donations
                .OrderByDescending(d => d.DonationDate)
                .Take(5)
                .ToList();
            ViewBag.LatestDonations = latestDonations;

            // ── توزيع الأدوار ─────────────────
            List<object> rolesCount = users
                .GroupBy(u => u.RoleId)
                .Select(g => (object)new
                {
                    Name = rolesDict.ContainsKey(g.Key)
                            ? rolesDict[g.Key] : "—",
                    Count = g.Count()
                })
                .ToList();
            ViewBag.RolesCount = rolesCount;

            // ── بيانات المطابقات للرسم ──────────
            var matchGroups = matches
                .GroupBy(m => m.Status)
                .Select(g => new {
                    Label = g.Key ?? "غير محدد",
                    Count = g.Count()
                }).ToList();

            ViewBag.MatchLabels = matchGroups
                .Select(g => g.Label).ToList();
            ViewBag.MatchCounts = matchGroups
                .Select(g => g.Count).ToList();

            // ── بيانات التبرعات الشهرية ─────────
            var donationGroups = donations
                .GroupBy(d => d.DonationDate.Month)
                .OrderBy(g => g.Key)
                .Select(g => new {
                    Month = g.Key,
                    Total = g.Sum(d => d.Amount)
                }).ToList();

            ViewBag.DonationMonths = donationGroups
                .Select(g => GetMonthName(g.Month)).ToList();
            ViewBag.DonationTotals = donationGroups
                .Select(g => g.Total).ToList();

            return View();
        }

        private string GetMonthName(int month)
        {
            string[] months = {
                "يناير","فبراير","مارس","أبريل",
                "مايو","يونيو","يوليو","أغسطس",
                "سبتمبر","أكتوبر","نوفمبر","ديسمبر"
            };
            if (month >= 1 && month <= 12)
                return months[month - 1];
            return "غير معروف";
        }
    }
}

