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

        //Organization
        public async Task<IActionResult> Organization()
        {
            var org = await _context.Organizations.FirstOrDefaultAsync();
            // Passing null if no record exists — the view will handle it
            return PartialView("_OrganizationDetail", org);
        }

        // Newsletters Partial
        public async Task<IActionResult> Newsletters()
        {
            var data = await _context.Newsletters.ToListAsync();
            return PartialView("_NewsletterList", data);
        }

        public async Task<IActionResult> Articles()
        {
            var articles = await _context.Articles
                .Include(a => a.Sector)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.Sectors = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return PartialView("_ArticleList", articles);
        }

        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return PartialView("_CategoryList", categories);
        }

        // Analytics Partial
        public IActionResult Analytics() => PartialView("_Analytics");
    }
}