using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Itihas360.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Itihas360.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class NewslettersController : ControllerBase
    {
        private readonly Itihas360Context _context;

        public NewslettersController(Itihas360Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Newsletter>>> GetNewsletters()
        {
            return await _context.Newsletters.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Newsletter>> GetNewsletter(int id)
        {
            var newsletter = await _context.Newsletters.FindAsync(id);
            return newsletter == null ? NotFound() : newsletter;
        }

        // ROLE CHECK: Sirf Admin hi Update kar sakta hai
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNewsletter(int id, Newsletter newsletter)
        {
            if (id != newsletter.SubscriberId) return BadRequest();
            _context.Entry(newsletter).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Newsletter>> PostNewsletter(Newsletter newsletter)
        {
            newsletter.SubscribedWhen ??= DateTime.Now;
            _context.Newsletters.Add(newsletter);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetNewsletter", new { id = newsletter.SubscriberId }, newsletter);
        }

        // ROLE CHECK: Sirf Admin hi Delete kar sakta hai
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNewsletter(int id)
        {
            var newsletter = await _context.Newsletters.FindAsync(id);
            if (newsletter == null) return NotFound();
            _context.Newsletters.Remove(newsletter);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}