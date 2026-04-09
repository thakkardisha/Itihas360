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
    [Authorize(Roles = "Admin")] // Pura controller sirf Admin ke liye hai
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogsController : ControllerBase
    {
        private readonly Itihas360Context _context;

        public AuditLogsController(Itihas360Context context)
        {
            _context = context;
        }

        // GET: api/AuditLogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs()
        {
            return await _context.AuditLogs.ToListAsync();
        }

        // GET: api/AuditLogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AuditLog>> GetAuditLog(int id)
        {
            var auditLog = await _context.AuditLogs.FindAsync(id);
            if (auditLog == null) return NotFound();
            return auditLog;
        }

        // AuditLogs ko edit karna aam taur par zaroori nahi hota, lekin security ke liye yahan bhi Admin check hai
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAuditLog(int id, AuditLog auditLog)
        {
            if (id != auditLog.LogId) return BadRequest();

            // Navigation property ko null karna zaroori hai foreign key error se bachne ke liye
            auditLog.User = null;
            _context.Entry(auditLog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuditLogExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<AuditLog>> PostAuditLog(AuditLog auditLog)
        {
            auditLog.User = null;
            if (auditLog.PerformedAt == null) auditLog.PerformedAt = DateTime.Now;

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetAuditLog", new { id = auditLog.LogId }, auditLog);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuditLog(int id)
        {
            var auditLog = await _context.AuditLogs.FindAsync(id);
            if (auditLog == null) return NotFound();

            _context.AuditLogs.Remove(auditLog);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool AuditLogExists(int id)
        {
            return _context.AuditLogs.Any(e => e.LogId == id);
        }
    }
}