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

            // 3. Latest Articles Grid
            viewModel.LatestArticles = await baseQuery
                .OrderByDescending(a => a.CreatedAt)
                .Take(6)
                .ToListAsync();

            // 4. Categories mapping to your project's explicit CategoryWithCount type
            viewModel.Categories = await _context.Categories
                .Where(c => (c.IsActive ?? false) == true)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new CategoryWithCount
                {
                    Category = c,
                    ArticleCount = _context.Articles.Count(a => a.SectorId == c.CategoryId && (a.IsPublished ?? false) == true && (a.IsDeleted ?? false) == false)
                })
                .ToListAsync();

            // 5. Global Stats
            viewModel.TotalArticles = await baseQuery.CountAsync();
            viewModel.TotalCategories = viewModel.Categories.Count;

            // 6. Notifications
            var limitDate = DateTime.Now.AddDays(-7);
            viewModel.RecentNotifications = await baseQuery
                .Where(a => a.CreatedAt >= limitDate)
                .OrderByDescending(a => a.CreatedAt)
                .Take(8)
                .ToListAsync();

            viewModel.UnreadNotifCount = viewModel.RecentNotifications.Count;
            viewModel.HasTodayNotifications = await baseQuery.AnyAsync(a => a.CreatedAt >= DateTime.Today);

            // 7. Organization Identity details mapping layout rules
            viewModel.Organization = await _context.Organizations.FirstOrDefaultAsync();

            return View(viewModel);
        }

        // 🎯 FIX: Changed to a single case-insensitive definition rule layout setup
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

            // Populate navigation elements to ensure the layout functions correctly
            viewModel.Categories = await _context.Categories
                .Where(c => (c.IsActive ?? false) == true)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new CategoryWithCount
                {
                    Category = c,
                    ArticleCount = _context.Articles.Count(a => a.SectorId == c.CategoryId && (a.IsPublished ?? false) == true && (a.IsDeleted ?? false) == false)
                })
                .ToListAsync();

            viewModel.TotalArticles = await _context.Articles.CountAsync(a => (a.IsPublished ?? false) == true && (a.IsDeleted ?? false) == false);
            viewModel.TotalCategories = viewModel.Categories.Count;

            // Map notification structures
            var limitDate = DateTime.Now.AddDays(-7);
            viewModel.RecentNotifications = await _context.Articles
                .Where(a => (a.IsPublished ?? false) == true && (a.IsDeleted ?? false) == false && a.CreatedAt >= limitDate)
                .OrderByDescending(a => a.CreatedAt)
                .Take(8)
                .ToListAsync();

            viewModel.UnreadNotifCount = viewModel.RecentNotifications.Count;
            viewModel.Organization = await _context.Organizations.FirstOrDefaultAsync();

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}