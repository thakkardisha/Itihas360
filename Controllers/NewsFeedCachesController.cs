using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Itihas360.Models;
using Microsoft.AspNetCore.Authorization;

namespace Itihas360.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NewsFeedCachesController : ControllerBase
    {
        private readonly Itihas360Context _context;

        public NewsFeedCachesController(Itihas360Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NewsFeedCache>>> GetNewsFeedCaches()
        {
            return await _context.NewsFeedCaches.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NewsFeedCache>> GetNewsFeedCache(int id)
        {
            var newsFeedCache = await _context.NewsFeedCaches.FindAsync(id);
            if (newsFeedCache == null) return NotFound();
            return newsFeedCache;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutNewsFeedCache(int id, NewsFeedCache newsFeedCache)
        {
            if (id != newsFeedCache.NewsCacheId) return BadRequest();

            newsFeedCache.RelatedSector = null; // Avoid navigation issues
            _context.Entry(newsFeedCache).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NewsFeedCacheExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<NewsFeedCache>> PostNewsFeedCache(NewsFeedCache newsFeedCache)
        {
            newsFeedCache.RelatedSector = null;
            if (newsFeedCache.FetchedAt == null) newsFeedCache.FetchedAt = DateTime.Now;

            _context.NewsFeedCaches.Add(newsFeedCache);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetNewsFeedCache", new { id = newsFeedCache.NewsCacheId }, newsFeedCache);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNewsFeedCache(int id)
        {
            var newsFeedCache = await _context.NewsFeedCaches.FindAsync(id);
            if (newsFeedCache == null) return NotFound();
            _context.NewsFeedCaches.Remove(newsFeedCache);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool NewsFeedCacheExists(int id)
        {
            return _context.NewsFeedCaches.Any(e => e.NewsCacheId == id);
        }
    }
}