using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Itihas360.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

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

            // Core Query: Only published, non-deleted articles with Categories included
            var baseQuery = _context.Articles
                .Include(a => a.Sector)
                .Where(a => (a.IsPublished ?? false) == true && (a.IsDeleted ?? false) == false);

            // 1. Featured Article (By ViewCount or Latest)
            viewModel.FeaturedArticle = await baseQuery
                .OrderByDescending(a => a.ViewCount)
                .ThenByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            var featuredId = viewModel.FeaturedArticle?.ArticleId ?? 0;

            // 2. Secondary Articles (Top 2 excluding Featured)
            viewModel.SecondaryArticles = await baseQuery
                .Where(a => a.ArticleId != featuredId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(2)
                .ToListAsync();

            // 3. Latest Articles Grid (6 items)
            viewModel.LatestArticles = await baseQuery
                .OrderByDescending(a => a.CreatedAt)
                .Take(6)
                .ToListAsync();

            // 4. Categories with Count logic
            viewModel.Categories = await _context.Categories
                .Where(c => (c.IsActive ?? false) == true)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new CategoryWithCount
                {
                    Category = c,
                    ArticleCount = _context.Articles.Count(a => a.SectorId == c.CategoryId && (a.IsPublished ?? false) == true)
                })
                .ToListAsync();

            // 5. Global Stats
            viewModel.TotalArticles = await baseQuery.CountAsync();
            viewModel.TotalCategories = viewModel.Categories.Count;

            // 6. Notifications (Last 7 days)
            var limitDate = DateTime.Now.AddDays(-7);
            viewModel.RecentNotifications = await baseQuery
                .Where(a => a.CreatedAt >= limitDate)
                .OrderByDescending(a => a.CreatedAt)
                .Take(8)
                .ToListAsync();

            viewModel.UnreadNotifCount = viewModel.RecentNotifications.Count;
            viewModel.HasTodayNotifications = await baseQuery.AnyAsync(a => a.CreatedAt >= DateTime.Today);

            //display organization's detail
            // Fetch the first organization record from the database
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