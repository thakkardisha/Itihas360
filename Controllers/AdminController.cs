using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Itihas360.Models;

namespace Itihas360.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly Itihas360Context _context;

        public AdminController(Itihas360Context context) => _context = context;

        // The shell page
        public IActionResult Index() => View();

        // Dashboard Partial
        public IActionResult Dashboard() => PartialView("_Dashboard");

        // Newsletters Partial
        public async Task<IActionResult> Newsletters()
        {
            var data = await _context.Newsletters.ToListAsync();
            return PartialView("_NewsletterList", data);
        }

        public async Task<IActionResult> Articles()
        {
            // 1. Fetch articles for the table
            var articles = await _context.Articles
                .Include(a => a.Sector)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            // 2. Fetch categories - DON'T use 'new { ... }' here.
            // Use the actual Category model or a DTO to ensure the View can read it.
            ViewBag.Sectors = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return PartialView("_ArticleList", articles);
        }

        // Analytics Partial
        public IActionResult Analytics() => PartialView("_Analytics");
    }
}