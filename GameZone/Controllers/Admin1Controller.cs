
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GameZone.Data;
using GameZone.Models;

namespace GameZone.Controllers
{
    [Authorize(Roles = "Admin")]
    public class Admin1Controller : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public Admin1Controller(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalBookings = await _context.Bookings.CountAsync();
            ViewBag.TotalGames = await _context.Games.CountAsync();
            ViewBag.ActiveUsers = await _userManager.Users.CountAsync();
            ViewBag.Bookings = await _context.Bookings.Include(b => b.Game).Include(b => b.User).ToListAsync();

            return View();
        }

        // ... (existing actions)

        public async Task<IActionResult> Bookings(string searchString, string statusFilter)
        {
            var bookings = _context.Bookings.Include(b => b.Game).Include(b => b.User).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b => b.User.UserName.Contains(searchString) || b.Game.Name.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                bookings = bookings.Where(b => b.Status == statusFilter);
            }

            var bookingsList = await bookings.ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;

            return View(bookingsList);
        }
    }
}