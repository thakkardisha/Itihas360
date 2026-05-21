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

            // Core Query: Only published, non-deleted articles
            var baseQuery = _context.Articles
                .Include(a => a.Sector)
                .Where(a => (a.IsPublished ?? false) == true && (a.IsDeleted ?? false) == false);

            // 1. Featured Article
            viewModel.FeaturedArticle = await baseQuery
                .OrderByDescending(a => a.ViewCount)
                .ThenByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            var featuredId = viewModel.FeaturedArticle?.ArticleId ?? 0;

            // 2. Secondary Articles
            viewModel.SecondaryArticles = await baseQuery
                .Where(a => a.ArticleId != featuredId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(2)
                .ToListAsync();

            // 3. Latest Articles Grid (Excluding the featured and secondary articles to prevent duplicates)
            viewModel.LatestArticles = await baseQuery
                .Where(a => a.ArticleId != featuredId && !viewModel.SecondaryArticles.Select(sa => sa.ArticleId).Contains(a.ArticleId))
                .OrderByDescending(a => a.CreatedAt)
                .Take(4)
                .ToListAsync();

            // 4. Global Stats
            viewModel.TotalArticles = await baseQuery.CountAsync();
            viewModel.HasTodayNotifications = await baseQuery.AnyAsync(a => a.CreatedAt >= DateTime.Today);

            // Note: Categories, RecentNotifications, UnreadNotifCount, and Organization are handled centrally by the LayoutDataFilter!
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}