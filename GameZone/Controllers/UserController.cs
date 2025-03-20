using GameZone.Data;
using GameZone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using System.Security.Claims;

namespace GameZone.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            List<Booking> bookings = await _context.Bookings
                .Where(b => b.UserId == user.Id)
                .Include(b => b.Game)
                .ToListAsync();
            ViewBag.UserName = user.FirstName;
            ViewBag.ProfilePic = TempData["ProfilePic"];
            return View(bookings);
        }

        // GET: /Booking/CancelBooking/5
        [HttpPost("Cancel/{id}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            // Authorization: Ensure the booking belongs to the logged-in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (booking.UserId != userId) // Assuming UserId is a string
            {
                return Forbid();
            }

            booking.Status = "Cancelled"; // Or another appropriate status
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "User");
        }


        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileModel
            {
                Username = user.UserName,
                Email = user.Email
                //Note: We are NOT populating the CurrentPassword field.
                //It's a security best practice to NOT pre-fill password fields.
            };
            ViewBag.UserName = user.FirstName;
            return View(model);
        }

        

        [HttpPost, ActionName("Profile")]
        public async Task<IActionResult> Profile(ProfileModel model)
        {
            
            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = model.Username;
            user.Email = model.Email;

            // Change Password Logic
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError("NewPassword", error.Description);
                    }
                    return View("Profile", model); // Return with password change errors
                }
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                ViewBag.Message = "Profile updated successfully.";
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View("Profile", model);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                TempData["ProfilePic"] = "~/uploads/" + fileName; // Store in TempData
            }
            return RedirectToAction("Profile");
        }
    }

    //private void SendBookingCancellationEmail(string userEmail, Booking booking)
    //{
    //    var message = new MimeMessage();
    //    message.From.Add(new MailboxAddress("GameZone", "aneeshsundaresh2002@gmail.com")); // Replace with your email
    //    message.To.Add(new MailboxAddress("User", userEmail));
    //    message.Subject = "GameZone Booking Cancellation";

    //    message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
    //    {
    //        Text = $"<p>Dear User,</p><p>Your booking for {booking.Game.Name} on {booking.BookingDate.ToShortDateString()} at {booking.BookingTime} has been cancelled.</p><p>Thank you for using GameZone.</p>"
    //    };

    //    using (var client = new SmtpClient())
    //    {
    //        client.Connect("smtp.example.com", 587, false); // Replace with your SMTP server details
    //        client.Authenticate("your_email@example.com", "your_password"); // Replace with your email credentials
    //        client.Send(message);
    //        client.Disconnect(true);
    //    }
    //}
}
