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
    [Authorize] // JWT Token zaroori hai
    [Route("api/[controller]")]
    [ApiController]
    public class McqquestionsController : ControllerBase
    {
        private readonly Itihas360Context _context;

        public McqquestionsController(Itihas360Context context)
        {
            _context = context;
        }

        // GET: api/Mcqquestions (Sabhi logged-in users ke liye)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mcqquestion>>> GetMcqquestions()
        {
            return await _context.Mcqquestions.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Mcqquestion>> GetMcqquestion(int id)
        {
            var mcqquestion = await _context.Mcqquestions.FindAsync(id);
            if (mcqquestion == null) return NotFound();
            return mcqquestion;
        }

        // ROLE CHECK: Sirf Admin hi sawal badal sakta hai
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMcqquestion(int id, Mcqquestion mcqquestion)
        {
            if (id != mcqquestion.QuestionId) return BadRequest();

            // Navigation properties ko null karna zaroori hai FK errors se bachne ke liye
            mcqquestion.Personality = null;
            mcqquestion.CreatedByNavigation = null;
            mcqquestion.Sector = null;

            _context.Entry(mcqquestion).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!McqquestionExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        // ROLE CHECK: Sirf Admin hi naya sawal add kar sakta hai
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Mcqquestion>> PostMcqquestion(Mcqquestion mcqquestion)
        {
            mcqquestion.Personality = null;
            mcqquestion.CreatedByNavigation = null;
            mcqquestion.Sector = null;
            mcqquestion.CreatedAt = DateTime.Now;
            mcqquestion.IsActive = true;

            _context.Mcqquestions.Add(mcqquestion);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetMcqquestion", new { id = mcqquestion.QuestionId }, mcqquestion);
        }

        // ROLE CHECK: Sirf Admin hi sawal delete kar sakta hai
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMcqquestion(int id)
        {
            var mcqquestion = await _context.Mcqquestions.FindAsync(id);
            if (mcqquestion == null) return NotFound();

            _context.Mcqquestions.Remove(mcqquestion);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool McqquestionExists(int id)
        {
            return _context.Mcqquestions.Any(e => e.QuestionId == id);
        }
    }
}