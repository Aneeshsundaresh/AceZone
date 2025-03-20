using GameZone.Data;
using GameZone.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;

namespace GameZone.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
       
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Contact(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                _context.Feedbacks.Add(feedback);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Thank you for your feedback!"; //Display success message
                return RedirectToAction("Contact"); // Redirect to refresh the page
            }

            return View(feedback); // Return the view with validation errors
        }

        public async Task<IActionResult> Locations()
        {
            return View();
        }

        
    }
}