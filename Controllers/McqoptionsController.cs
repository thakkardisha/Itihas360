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
    public class McqoptionsController : ControllerBase
    {
        private readonly Itihas360Context _context;

        public McqoptionsController(Itihas360Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mcqoption>>> GetMcqoptions()
        {
            return await _context.Mcqoptions.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Mcqoption>> GetMcqoption(int id)
        {
            var mcqoption = await _context.Mcqoptions.FindAsync(id);
            if (mcqoption == null) return NotFound();
            return mcqoption;
        }

        // ROLE CHECK
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMcqoption(int id, Mcqoption mcqoption)
        {
            if (id != mcqoption.OptionId) return BadRequest();

            // 1. If this specific option is being marked as correct
            if (mcqoption.IsCorrect == true)
            {
                // 2. Find if there is ANOTHER option for this question that is currently correct
                // We exclude the current option ID so we don't find ourselves
                var existingCorrect = await _context.Mcqoptions
                    .FirstOrDefaultAsync(o => o.QuestionId == mcqoption.QuestionId
                                           && o.IsCorrect == true
                                           && o.OptionId != id);

                if (existingCorrect != null)
                {
                    existingCorrect.IsCorrect = false;
                }
            }

            mcqoption.Question = null;
            _context.Entry(mcqoption).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!McqoptionExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Mcqoption>> PostMcqoption(Mcqoption mcqoption)
        {
            // Ensure we handle potential nulls for IsCorrect safely
            if (mcqoption.IsCorrect == true)
            {
                // Change: Added '== true' to safely compare bool? with a bool value
                var existingCorrect = await _context.Mcqoptions
                    .FirstOrDefaultAsync(o => o.QuestionId == mcqoption.QuestionId && o.IsCorrect == true);

                if (existingCorrect != null)
                {
                    existingCorrect.IsCorrect = false;
                }
            }

            mcqoption.Question = null;
            _context.Mcqoptions.Add(mcqoption);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMcqoption", new { id = mcqoption.OptionId }, mcqoption);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMcqoption(int id)
        {
            var mcqoption = await _context.Mcqoptions.FindAsync(id);
            if (mcqoption == null) return NotFound();

            _context.Mcqoptions.Remove(mcqoption);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool McqoptionExists(int id)
        {
            return _context.Mcqoptions.Any(e => e.OptionId == id);
        }
    }
}