using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Itihas360.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

namespace Itihas360.Controllers
{
    public class HomeController : Controller
    {
        private readonly Itihas360Context _context;

        public HomeController(Itihas360Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            // 1. Fetch ALL valid articles sequentially in a single database round-trip call
            var allArticles = await _context.Articles
                .Include(a => a.Sector)
                .Where(a => (a.IsPublished ?? false) == true && (a.IsDeleted ?? false) == false)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            // 2. Set Global Stats tracking counters safely
            viewModel.TotalArticles = allArticles.Count;
            viewModel.HasTodayNotifications = allArticles.Any(a => a.CreatedAt >= DateTime.Today);

            viewModel.TotalCategories = await _context.Categories
                .CountAsync(c => (c.IsActive ?? false) == true);

            if (allArticles.Any())
            {
                // 3. Featured Article selection: Pick the highest viewed item
                viewModel.FeaturedArticle = allArticles
                    .OrderByDescending(a => a.ViewCount)
                    .ThenByDescending(a => a.CreatedAt)
                    .FirstOrDefault();

                // 4. Secondary Articles selection: Just take the next two latest articles
                viewModel.SecondaryArticles = allArticles
                    .Where(a => a.ArticleId != (viewModel.FeaturedArticle?.ArticleId ?? 0))
                    .Take(2)
                    .ToList();

                // 5. FIX: Pull the absolute latest articles completely unfiltered for your grid and ticker
                // This makes sure it functions exactly like it was earlier, irrespective of featured status.
                viewModel.LatestArticles = allArticles
                    .Take(6)
                    .ToList();
            }
            else
            {
                viewModel.SecondaryArticles = new List<Article>();
                viewModel.LatestArticles = new List<Article>();
            }

            // 6. Bind data explicitly to prevent any null reference issues with shared layout layout partials
            viewModel.Organization = await _context.Organizations.FirstOrDefaultAsync();

            viewModel.Categories = await _context.Categories
                .Where(c => (c.IsActive ?? false) == true)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new CategoryWithCount
                {
                    Category = c,
                    ArticleCount = _context.Articles.Count(a => a.SectorId == c.CategoryId && (a.IsPublished ?? false) == true && (a.IsDeleted ?? false) == false)
                })
                .ToListAsync();

            return View(viewModel);
        }

        // GET: /articles
        [HttpGet("articles")]
        public async Task<IActionResult> Articles(string? search, string? category)
        {
            var viewModel = new HomeViewModel();

            // Base query mirroring your precise publishing requirements criteria logic rules
            var articlesQuery = _context.Articles
                .Include(a => a.Sector)
                .Where(a => (a.IsPublished ?? false) == true && (a.IsDeleted ?? false) == false);

            // Apply filter if a category filter parameter is passed down
            if (!string.IsNullOrEmpty(category))
            {
                articlesQuery = articlesQuery.Where(a => a.Sector != null && a.Sector.CategorySlug == category);
            }

            // Apply filter if search text matches title or body summaries
            if (!string.IsNullOrEmpty(search))
            {
                articlesQuery = articlesQuery.Where(a => a.Title.Contains(search) || a.ShortBio.Contains(search));
            }

            // Bind evaluated list models mapping properties
            viewModel.LatestArticles = await articlesQuery
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            viewModel.TotalArticles = await _context.Articles.CountAsync(a => (a.IsPublished ?? false) == true && (a.IsDeleted ?? false) == false);

            // Note: Categories, RecentNotifications, UnreadNotifCount, and Organization are handled centrally by the LayoutDataFilter!
            return View(viewModel);
        }

        // GET: /quiz
        [Route("quiz")]
        public async Task<IActionResult> Quiz()
        {
            ViewData["Title"] = "Test Your Knowledge — Itihas 360";
            ViewData["ActivePage"] = "Quiz";

            // 1. Fetch active global questions and their choices
            var questions = await _context.Mcqquestions
                .Where(q => q.IsActive == true)
                .Include(q => q.Mcqoptions)
                .OrderBy(q => q.QuestionId)
                .ToListAsync();

            // Note: Categories, RecentNotifications, UnreadNotifCount, and Organization are handled centrally by the LayoutDataFilter!
            return View("Quiz", questions);
        }

        //Timeline
        [Route("timeline")]
        [Route("Home/Timeline")]
        public IActionResult Timeline()
        {
            // If your layout requires Categories/Organization info globally, map them here:
            ViewBag.Organization = _context.Organizations.FirstOrDefault();
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.LatestArticles = _context.Articles.OrderByDescending(a => a.CreatedAt).Take(5).ToList();
            ViewBag.UnreadNotifCount = 3; // Or your custom query engine count logic

            return View();
        }

        // GET: /about
        [Route("about")]
        public IActionResult About()
        {
            ViewData["Title"] = "Our Mission — Itihas 360";
            ViewData["ActivePage"] = "About"; // Optional context tracker variable
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}