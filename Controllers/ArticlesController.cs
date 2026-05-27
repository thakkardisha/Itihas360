using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Itihas360.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace Itihas360.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly Itihas360Context _context;
        private readonly IWebHostEnvironment _environment;

        public ArticlesController(Itihas360Context context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/Articles
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Article>>> GetArticles()
        {
            return await _context.Articles.ToListAsync();
        }

        // GET: api/Articles/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetArticle(int id)
        {
            var article = await _context.Articles
                .Include(a => a.Mcqquestions)
                    .ThenInclude(q => q.Mcqoptions)
                .FirstOrDefaultAsync(a => a.ArticleId == id);

            if (article == null) return NotFound();

            return article;
        }

        // POST: api/Articles
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Article>> PostArticle(Article article)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            article.CreatedBy = currentUserId ?? "System";
            article.CreatedAt = DateTime.Now;
            article.IsDeleted = false;
            article.ViewCount = 0;

            // Process Base64 assets asynchronously into physical file system paths
            article.ImageUrl = await ProcessBase64ImageAsync(article.ImageUrl, null);
            article.SecondaryImageUrl = await ProcessBase64ImageAsync(article.SecondaryImageUrl, null);

            article.Sector = null;
            article.CreatedByNavigation = null;

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetArticle", new { id = article.ArticleId }, article);
        }

        // PUT: api/Articles/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle(int id, Article article)
        {
            if (id != article.ArticleId) return BadRequest();

            // 1. Fetching the untracked snapshot of the existing record from the DB
            var existing = await _context.Articles.AsNoTracking().FirstOrDefaultAsync(x => x.ArticleId == id);
            if (existing == null) return NotFound();

            // 2. RETAINING HISTORIC VALUES (This prevents things missing from the form from blanking out)
            article.CreatedAt = existing.CreatedAt;
            article.CreatedBy = existing.CreatedBy;

            // RETAINS THE VIEW COUNT!
            article.ViewCount = existing.ViewCount;

            // PRESERVES DELETION STATE (So editing doesn't un-delete an item)
            article.IsDeleted = existing.IsDeleted;

            // 3. Keep updating auditing metrics
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            article.UpdatedBy = currentUserId;
            article.UpdatedAt = DateTime.Now;

            // Image processing
            article.ImageUrl = await ProcessBase64ImageAsync(article.ImageUrl, existing.ImageUrl);
            article.SecondaryImageUrl = await ProcessBase64ImageAsync(article.SecondaryImageUrl, existing.SecondaryImageUrl);

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

        // DELETE: api/Articles/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound();

            // Clear disk space on deletion
            DeletePhysicalFile(article.ImageUrl);
            DeletePhysicalFile(article.SecondaryImageUrl);

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- IMAGE HANDLING CORE HELPERS ---

        private async Task<string?> ProcessBase64ImageAsync(string? base64Data, string? existingPath)
        {
            if (string.IsNullOrEmpty(base64Data))
            {
                return existingPath;
            }

            if (!base64Data.StartsWith("data:image"))
            {
                return base64Data;
            }

            try
            {
                var match = System.Text.RegularExpressions.Regex.Match(base64Data, @"data:image/(?<type>.+?);base64,(?<data>.+)");
                if (!match.Success) return existingPath;

                string extension = match.Groups["type"].Value;
                string pureBase64 = match.Groups["data"].Value;

                byte[] imageBytes = Convert.FromBase64String(pureBase64);

                string folderPath = Path.Combine(_environment.WebRootPath, "uploads", "articles");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string uniqueFileName = $"{Guid.NewGuid()}.{extension}";
                string physicalPath = Path.Combine(folderPath, uniqueFileName);

                await System.IO.File.WriteAllBytesAsync(physicalPath, imageBytes);

                // Delete the old asset file if replaced
                DeletePhysicalFile(existingPath);

                return $"/uploads/articles/{uniqueFileName}";
            }
            catch
            {
                return existingPath;
            }
        }

        private void DeletePhysicalFile(string? path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("/uploads/articles/")) return;

            string fullPath = Path.Combine(_environment.WebRootPath, path.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.ArticleId == id);
        }
    }
}