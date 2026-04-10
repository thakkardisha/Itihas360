using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO; // Added for File operations
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Itihas360.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting; // Added for WebRootPath

namespace Itihas360.Controllers
{
    [Authorize(Policy = "AdminOnly")] // Applying the Admin restriction we discussed
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private readonly Itihas360Context _context;
        private readonly IWebHostEnvironment _environment;

        public OrganizationsController(Itihas360Context context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizations()
        {
            return await _context.Organizations.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Organization>> GetOrganization(int id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null) return NotFound();
            return organization;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrganization(int id, OrganizationDto dto)
        {
            if (id != dto.OrganizationId) return BadRequest();

            var existingOrg = await _context.Organizations.AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrganizationId == id);

            if (existingOrg == null) return NotFound();

            // Map DTO → Model
            var organization = new Organization
            {
                OrganizationId = dto.OrganizationId,
                OrganizationName = dto.OrganizationName,
                Mobile = dto.Mobile,
                AlterMobile = dto.AlterMobile,
                Email = dto.Email,
                CompanyAddress = dto.CompanyAddress,
                City = dto.City,
                State = dto.State,
                Instagram = dto.Instagram,
                Facebook = dto.Facebook,
                LinkedIn = dto.LinkedIn,
                X = dto.X,
                CreatedAt = existingOrg.CreatedAt, // ✅ Preserve original date
            };

            // ✅ Process Base64 → file path (now validation won't block it)
            organization.OrganizationPhoto = ProcessBase64Image(dto.OrganizationPhoto, existingOrg.OrganizationPhoto);

            _context.Entry(organization).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrganizationExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Organization>> PostOrganization(OrganizationDto dto)
        {
            var organization = new Organization
            {
                OrganizationName = dto.OrganizationName,
                Mobile = dto.Mobile,
                AlterMobile = dto.AlterMobile,
                Email = dto.Email,
                CompanyAddress = dto.CompanyAddress,
                City = dto.City,
                State = dto.State,
                Instagram = dto.Instagram,
                Facebook = dto.Facebook,
                LinkedIn = dto.LinkedIn,
                X = dto.X,
                CreatedAt = DateTime.Now,
            };

            // Process Base64 → file path
            organization.OrganizationPhoto = ProcessBase64Image(dto.OrganizationPhoto, null);

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrganization", new { id = organization.OrganizationId }, organization);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganization(int id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null) return NotFound();

            //Delete physical file from folder when record is deleted
            DeletePhysicalFile(organization.OrganizationPhoto);

            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- HELPER METHODS ---

        private string ProcessBase64Image(string photoData, string existingPath)
        {
            // If it's not a Base64 string, the user didn't upload a new file, return current path
            if (string.IsNullOrEmpty(photoData) || !photoData.StartsWith("data:image"))
            {
                return existingPath;
            }

            try
            {
                // 1. Prepare folder
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // 2. Extract Base64 data
                // Format: "data:image/png;base64,iVBOR..."
                var parts = photoData.Split(',');
                string base64Part = parts[1];
                byte[] imageBytes = Convert.FromBase64String(base64Part);

                // 3. Generate filename
                string extension = parts[0].Contains("png") ? ".png" : ".jpg";
                string fileName = $"org_logo_{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                // 4. Save to disk
                System.IO.File.WriteAllBytes(filePath, imageBytes);

                // 5. Delete old file if it exists to save space
                DeletePhysicalFile(existingPath);

                return "/uploads/" + fileName;
            }
            catch
            {
                return existingPath; // Fallback to existing if something fails
            }
        }

        private void DeletePhysicalFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("/uploads/")) return;

            string fullPath = Path.Combine(_environment.WebRootPath, path.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        private bool OrganizationExists(int id)
        {
            return _context.Organizations.Any(e => e.OrganizationId == id);
        }
    }
}