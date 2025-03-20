using GameZone.Data;
using GameZone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameZone.Controllers
{
    
    public class GameController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public GameController(ApplicationDbContext context)
        {
            _context = context;
            
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var games = from g in _context.Games // Assuming your DbSet is named "Games"
                        select g;

            // 2. Apply the search filter if a search string is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                games = games.Where(g => g.Name.Contains(searchString) || // Assuming your game model has a "Title" property
                                         g.Description.Contains(searchString)); // And a "Description" property (you can add more)
            }

            // 4. Convert the results to a list and pass them to the view
            
            return View(games.ToList());
        }

        public async Task<IActionResult> Details(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            return View(game);
        }
        public IActionResult GetImage(int id)
        {
            var game = _context.Games.FirstOrDefault(g => g.Id == id);
            if (game == null || game.ImageData == null)
            {
                return NotFound();
            }
            return File(game.ImageData, game.ImageContentType);
        }


        //[HttpGet, ActionName("Profile")]
        //public IActionResult Games(string searchString)
        //{
        //    // 1. Get all games or start with a base query
        //    var games = from g in _context.Games // Assuming your DbSet is named "Games"
        //                select g;

        //    // 2. Apply the search filter if a search string is provided
        //    if (!string.IsNullOrEmpty(searchString))
        //    {
        //        games = games.Where(g => g.Name.Contains(searchString) || // Assuming your game model has a "Title" property
        //                                 g.Description.Contains(searchString)); // And a "Description" property (you can add more)
        //    }

        //    // 4. Convert the results to a list and pass them to the view
        //    return View(games.ToList());
        //}
    }
}