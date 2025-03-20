using GameZone.Data;
using GameZone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace GameZone.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Games = new SelectList(await _context.Games.ToListAsync(), "Id", "Name");
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserName = user.FirstName;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                booking.UserId = user.Id;
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
               
                // **Eagerly load the related Game entity AFTER saving changes**
                booking = await _context.Bookings
                                         .Include(b => b.Game) // **EAGER LOAD Game NAVIGATION PROPERTY**
                                         .FirstOrDefaultAsync(b => b.Id == booking.Id); // Retrieve the saved booking again WITH Game

                if (booking != null && booking.Game != null) // Null check to be safe
                {
                    // Send confirmation email (now booking.Game should be loaded)
                    Task.Run(() => SendBookingConfirmationEmail(user.Email, booking));
                }
                else
                {
                    // Log an error or handle the case where Game is still not loaded (unexpected)
                    Console.WriteLine("Error: Game navigation property was not loaded for booking ID: " + booking?.Id);
                    // Optionally, you could still redirect even if email fails, or show an error message to the user.
                }

                return RedirectToAction("Index", "User");
            }

            ViewBag.Games = new SelectList(await _context.Games.ToListAsync(), "Id", "Name", booking.GameId);
            return View(booking);
        }

        private void SendBookingConfirmationEmail(string userEmail, Booking booking)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("GameZone", "aneeshsundaresh2002@gmail.com")); // Replace with your email
            message.To.Add(new MailboxAddress("User", userEmail));
            message.Subject = "GameZone Booking Confirmation";

            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $"<p>Dear User,</p><p>Your booking for {booking.Game.Name} on {booking.BookingDate.ToShortDateString()} at {booking.BookingTime} has been confirmed.</p><p>Duration: {booking.Duration} hours.</p><p>Thank you for booking with GameZone!</p>"
            };

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Timeout = 30000; // Set a timeout of 30 seconds (adjust as needed, in milliseconds)

                    client.Connect("smtp.gmail.com", 587, false); // Replace with your SMTP server details
                    client.Timeout = 30000; // Set timeout again before Authenticate and Send just to be safe
                    client.Authenticate("aneeshsundaresh2002@gmail.com", "Raheja504*"); // Replace with your email credentials
                    client.Timeout = 60000; // Maybe allow a bit longer for Send operation
                    client.Send(message);
                    client.Disconnect(true);
                }
                Console.WriteLine("Email sent successfully to: " + userEmail); // Success log
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email to: {userEmail}. Exception: {ex.Message}"); // Error log
                Console.WriteLine($"Stack Trace: {ex.StackTrace}"); // Log stack trace for debugging
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}"); // Log inner exception if any
                }                                                                                    // Consider logging the full exception details: _logger.LogError(ex, "Error sending email"); in a real app
            }
        }
        public IActionResult GetBookingCounts()
        {
            var pendingCount = _context.Bookings.Count(b => b.Status == "Pending");
            var cancelledCount = _context.Bookings.Count(b => b.Status == "Cancelled");
            var confirmedCount = _context.Bookings.Count(b => b.Status == "Confirmed");

            return Json(new { pending = pendingCount, cancelled = cancelledCount, confirmed = confirmedCount });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status == "Pending")
            {
                booking.Status = "Confirmed";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Bookings","Admin"); 
        }

        //        public async Task<IActionResult> SearchBookings(string searchString, DateTime? fromDate, DateTime? toDate, string status)
        //{
        //    ViewBag.SearchString = searchString;
        //    ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd"); // Format for the input type="date"
        //    ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        //    ViewBag.Status = status;

        //    var bookings = from b in _context.Bookings
        //                   .Include(b => b.Game)
        //                   .Include(b => b.User)
        //                   select b;

        //    if (!string.IsNullOrEmpty(searchString))
        //    {
        //        bookings = bookings.Where(b => b.User.UserName.Contains(searchString) || b.Game.Name.Contains(searchString));
        //    }
        //    if (fromDate.HasValue)
        //    {
        //        bookings = bookings.Where(b => b.BookingDate >= fromDate.Value);
        //    }
        //    if (toDate.HasValue)
        //    {
        //        bookings = bookings.Where(b => b.BookingDate <= toDate.Value);
        //    }
        //    if (!string.IsNullOrEmpty(status))
        //    {
        //        bookings = bookings.Where(b => b.Status == status);
        //    }

        //    return View(await bookings.ToListAsync());
        //}
        //    }
    }
}