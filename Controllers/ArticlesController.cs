using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Itihas360.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace Itihas360.Controllers
{
    [Authorize] // Sabhi methods ke liye login zaroori hai
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly Itihas360Context _context;

        public ArticlesController(Itihas360Context context)
        {
            _context = context;
        }

        // GET: api/Articles (Sab dekh sakte hain)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Article>>> GetArticles()
        {
            return await _context.Articles.ToListAsync();
        }

        // GET: api/Articles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Article>> GetArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound();
            return article;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Article>> PostArticle(Article article)
        {
            // Use NameIdentifier to get the AspNetUsers String ID
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            article.CreatedBy = currentUserId ?? "System"; // Fallback if ID is null
            article.CreatedAt = DateTime.Now;
            article.IsDeleted = false;
            article.ViewCount = 0;

            // IMPORTANT: Clear navigation properties so EF doesn't try to 
            // insert a new Category/User record alongside the article
            article.Sector = null;
            article.CreatedByNavigation = null;

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetArticle", new { id = article.ArticleId }, article);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle(int id, Article article)
        {
            if (id != article.ArticleId) return BadRequest();

            var existing = await _context.Articles.AsNoTracking().FirstOrDefaultAsync(x => x.ArticleId == id);
            if (existing == null) return NotFound();

            // 1. Preserve the original creation data
            article.CreatedAt = existing.CreatedAt;
            article.CreatedBy = existing.CreatedBy;

            // 2. Set update metadata using the SAME String ID logic as Post
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            article.UpdatedBy = currentUserId;
            article.UpdatedAt = DateTime.Now;

            // 3. Prevent EF from getting confused by related objects
            article.Sector = null;
            article.UpdatedByNavigation = null;

            _context.Entry(article).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        // ROLE CHECK: Sirf Admin hi Article Delete kar sakta hai
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound();

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.ArticleId == id);
        }
    }
}