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
                // If accessed by ID: /ArticlesView/Read/3
                article = await _context.Articles
                    .FirstOrDefaultAsync(a => a.ArticleId == id);
            }
            else if (!string.IsNullOrEmpty(slug))
            {
                // If accessed by Slug: /article/Disha
                article = await _context.Articles
                    .FirstOrDefaultAsync(a => a.Slug == slug);
            }

            if (article == null) return NotFound();

            var questions = await _context.Mcqquestions
                .Where(q => q.PersonalityId == article.ArticleId && q.IsActive == true)
                .Include(q => q.Mcqoptions)
                .ToListAsync();

            var viewModel = new ArticleReadingViewModel
            {
                Article = article,
                Questions = questions
            };

            // Increment view count logic
            article.ViewCount++;
            _context.Update(article);
            await _context.SaveChangesAsync();

            return View(viewModel);
        }
    }
}