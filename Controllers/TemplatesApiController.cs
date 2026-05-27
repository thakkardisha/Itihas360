using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Itihas360.Models;

namespace Itihas360.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("api/[controller]")]
    [ApiController]
    public class TemplatesApiController : ControllerBase
    {
        private readonly Itihas360Context _context;

        public TemplatesApiController(Itihas360Context context)
        {
            _context = context;
        }

        // GET: api/TemplatesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmailTemplate>>> GetTemplates()
        {
            return await _context.EmailTemplates.ToListAsync();
        }

        // GET: api/TemplatesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmailTemplate>> GetTemplate(int id)
        {
            var template = await _context.EmailTemplates.FindAsync(id);
            if (template == null) return NotFound();
            return template;
        }

        // POST: api/TemplatesApi
        [HttpPost]
        public async Task<ActionResult<EmailTemplate>> PostTemplate(EmailTemplate template)
        {
            _context.EmailTemplates.Add(template);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
        }

        // PUT: api/TemplatesApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTemplate(int id, EmailTemplate template)
        {
            if (id != template.Id) return BadRequest();

            _context.Entry(template).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.EmailTemplates.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/TemplatesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var template = await _context.EmailTemplates.FindAsync(id);
            if (template == null) return NotFound();

            _context.EmailTemplates.Remove(template);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}