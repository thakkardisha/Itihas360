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

            // Fetch the article along with its Sector/Category relationship
            if (id.HasValue)
            {
                article = await _context.Articles
                    .Include(a => a.Sector)
                    .FirstOrDefaultAsync(a => a.ArticleId == id);
            }
            else if (!string.IsNullOrEmpty(slug))
            {
                article = await _context.Articles
                    .Include(a => a.Sector)
                    .FirstOrDefaultAsync(a => a.Slug == slug);
            }

            if (article == null) return NotFound();

            // 1. Fetch MCQs for the current article
            var questions = await _context.Mcqquestions
                .Where(q => q.PersonalityId == article.ArticleId && q.IsActive == true)
                .Include(q => q.Mcqoptions)
                .ToListAsync();

            // 2. Construct your clean reading view model
            var viewModel = new ArticleReadingViewModel
            {
                Article = article,
                Questions = questions
            };

            // 3. FIX: Safely increment view count handling nullable int? states
            article.ViewCount = (article.ViewCount ?? 0) + 1;

            _context.Update(article);
            await _context.SaveChangesAsync();

            // Note: Categories, RecentNotifications, UnreadNotifCount, and Organization 
            // are now handled centrally by the LayoutDataFilter!
            return View(viewModel);
        }
    }
}