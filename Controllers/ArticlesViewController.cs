using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Itihas360.Models;
using Itihas360.Models.ViewModels;

namespace Itihas360.Controllers
{
    public class ArticlesViewController : Controller
    {
        private readonly Itihas360Context _context;

        public ArticlesViewController(Itihas360Context context)
        {
            _context = context;
        }

        [Route("ArticlesView/Read/{id?}")]
        [Route("article/{slug}")]
        public async Task<IActionResult> Read(string slug, int? id)
        {
            Article article = null;

            if (id.HasValue)
            {
                article = await _context.Articles
                    .FirstOrDefaultAsync(a => a.ArticleId == id);
            }
            else if (!string.IsNullOrEmpty(slug))
            {
                article = await _context.Articles
                    .FirstOrDefaultAsync(a => a.Slug == slug);
            }

            if (article == null) return NotFound();

            // 1. Fetch MCQs for the current article
            var questions = await _context.Mcqquestions
                .Where(q => q.PersonalityId == article.ArticleId && q.IsActive == true)
                .Include(q => q.Mcqoptions)
                .ToListAsync();

            // 2. Setup the exact same base query filters used by your HomeController
            var baseQuery = _context.Articles
                .Include(a => a.Sector)
                .Where(a => (a.IsPublished ?? false) == true && (a.IsDeleted ?? false) == false);

            // 3. Fetch layout details directly into ViewBag packets for _Layout.cshtml
            ViewBag.Organization = await _context.Organizations.FirstOrDefaultAsync();

            // Using full explicit namespace to avoid CS0246 error without creating new files
            ViewBag.Categories = await _context.Categories
                .Where(c => (c.IsActive ?? false) == true)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new Itihas360.Models.CategoryWithCount
                {
                    Category = c,
                    ArticleCount = _context.Articles.Count(a => a.SectorId == c.CategoryId && (a.IsPublished ?? false) == true && (a.IsDeleted ?? false) == false)
                })
                .ToListAsync();

            ViewBag.LatestArticles = await baseQuery
                .OrderByDescending(a => a.CreatedAt)
                .Take(6)
                .ToListAsync();

            // Match notifications 7-day limit rule lookback filter
            var limitDate = DateTime.Now.AddDays(-7);
            var recentNotifications = await baseQuery
                .Where(a => a.CreatedAt >= limitDate)
                .OrderByDescending(a => a.CreatedAt)
                .Take(8)
                .ToListAsync();

            ViewBag.RecentNotifications = recentNotifications;
            ViewBag.UnreadNotifCount = recentNotifications.Count;

            // 4. Construct your original, simple reading view model
            var viewModel = new ArticleReadingViewModel
            {
                Article = article,
                Questions = questions
            };

            // 5. Increment view count analytics safely
            article.ViewCount++;
            _context.Update(article);
            await _context.SaveChangesAsync();

            return View(viewModel);
        }
    }
}