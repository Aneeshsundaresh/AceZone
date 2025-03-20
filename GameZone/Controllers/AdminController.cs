using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GameZone.Data;
using GameZone.Models;

namespace GameZone.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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

        // GET: Admin/EditBooking/5
        public async Task<IActionResult> EditBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Game)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // You might need to create a view model for editing bookings
            // and populate it with data from the booking object here.
            // For now, I'm assuming you'll pass the booking object directly to the view.

            return View(booking);
        }

        // POST: Admin/EditBooking/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBooking(int id, Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBooking = await _context.Bookings.FindAsync(id);
                    if (existingBooking == null)
                    {
                        return NotFound();
                    }

                    existingBooking.BookingDate = booking.BookingDate;
                    existingBooking.BookingTime = booking.BookingTime;
                    existingBooking.Duration = booking.Duration;

                    _context.Update(existingBooking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }

        // GET: Admin/DeleteBooking/5
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Game)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Admin/DeleteBooking/5
        [HttpPost, ActionName("DeleteBooking")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBookingConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Bookings));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Games()
        {
            var games = await _context.Games.ToListAsync();
            return View(games);
        }

        // GET: Game/Edit/5
        public async Task<IActionResult> EditGame(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            return View(game);
        }

        // POST: Game/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGame(int id, [Bind("Id,Name,Description,Price")] Game game, IFormFile imageFile)
        {
            if (id != game.Id)
            {
                return NotFound();
            }
            var existingGame = await _context.Games.FindAsync(id);

            if (ModelState.IsValid)
            {
                try
                {
                    existingGame.Name = game.Name;
                    existingGame.Description = game.Description;
                    existingGame.Price = game.Price;

                   
                    // If no new image, keep the existing image data and content type

                    _context.Update(existingGame); // Update the existing entity
                    await _context.SaveChangesAsync();
                
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameExists(game.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Games", "Admin");
            }
            return View(game);
        }

        // GET: Game/Delete/5
        // GET: Game/Delete/5
        public async Task<IActionResult> DeleteGame(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // POST: Game/Delete/5
        [HttpPost, ActionName("DeleteGame")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _context.Games.FindAsync(id);
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
            return RedirectToAction("Games", "Admin"); // Or your games list action
        }
    


        private bool GameExists(int id)
        {
            return _context.Games.Any(e => e.Id == id);
        }

        public IActionResult CreateGame()
        {
            return View();
        }

        // POST: Game/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGame(Game game, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        game.ImageData = memoryStream.ToArray();
                        game.ImageContentType = imageFile.ContentType;
                    }
                }

                _context.Games.Add(game);
                await _context.SaveChangesAsync();
                return RedirectToAction("Games", "Admin"); // Or wherever you want to redirect
            }
            return View(game);
        }
    }

}

