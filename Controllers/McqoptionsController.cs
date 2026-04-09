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

        // ROLE CHECK: Sirf Admin hi Options badal sakta hai
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMcqoption(int id, Mcqoption mcqoption)
        {
            if (id != mcqoption.OptionId) return BadRequest();

            mcqoption.Question = null; // FK navigation fix
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