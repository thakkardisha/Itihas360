using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Itihas360.Models;
using Itihas360.Services;
using System.Text;

namespace Itihas360.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignApiController : ControllerBase
    {
        private readonly Itihas360Context _context;
        private readonly IEmailService _emailService;

        public CampaignApiController(Itihas360Context context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // POST: api/CampaignApi/SendBlast
        [HttpPost("SendBlast")]
        public async Task<IActionResult> SendBlast([FromBody] SendCampaignModel model)
        {
            if (model == null) return BadRequest("Invalid payload.");

            var template = await _context.EmailTemplates.FindAsync(model.TemplateId);
            if (template == null) return NotFound("Template not found.");

            // SAFETY FALLBACK: If model.SendToAll is true OR if target list came back empty,
            // explicitly pull all active emails from your Newsletter context directly.
            List<string> targetEmails = new List<string>();

            if (model.SendToAll || model.TargetEmails == null || !model.TargetEmails.Any())
            {
                targetEmails = await _context.Newsletters
                    .Select(n => n.Email)
                    .Where(e => e != null && e != "")
                    .ToListAsync();
            }
            else
            {
                targetEmails = model.TargetEmails;
            }

            // Diagnostics check if you're debugging in Visual Studio
            System.Diagnostics.Debug.WriteLine($"Found {targetEmails.Count} subscribers in processing queue.");

            if (!targetEmails.Any())
            {
                return Ok(new { success = true, count = 0, message = "Database table returned no email string rows." });
            }

            string processedBody = template.HtmlBody;

            // Resolve dynamic articles
            if (processedBody.Contains("{{WeeklyArticles}}"))
            {
                var oneWeekAgo = DateTime.Now.AddDays(-7);
                var recentArticles = await _context.Articles
                    .Where(a => a.CreatedAt >= oneWeekAgo)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                var sb = new System.Text.StringBuilder("<ul>");
                foreach (var art in recentArticles)
                {
                    sb.Append($"<li>{art.Title}</li>");
                }
                sb.Append("</ul>");

                processedBody = processedBody.Replace("{{WeeklyArticles}}", recentArticles.Any() ? sb.ToString() : "No new articles.");
            }

            processedBody = processedBody.Replace("{{AdminNotes}}", model.CustomNotes ?? "");

            int successCount = 0;
            foreach (var email in targetEmails)
            {
                try
                {
                    await _emailService.SendEmailAsync(email, template.DefaultSubject, processedBody);
                    successCount++;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to send email to {email}: {ex.Message}");
                }
            }

            return Ok(new { success = true, count = successCount });
        }
    }
}