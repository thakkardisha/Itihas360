using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Itihas360.Models;
using Itihas360.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itihas360.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class CampaignController : Controller
    {
        private readonly Itihas360Context _context;
        private readonly IEmailService _emailService;

        public CampaignController(Itihas360Context context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: Campaign/Index -> Injected into your Admin Workspace Sidebar Router
        public async Task<IActionResult> Index()
        {
            ViewBag.Templates = await _context.EmailTemplates.ToListAsync();

            // Fixed to handle your unique Newsletter schema sorted chronologically
            ViewBag.Subscribers = await _context.Newsletters
                .OrderByDescending(n => n.SubscribedWhen)
                .ToListAsync();

            return PartialView("_CampaignWorkspace");
        }

        // POST: Campaign/SendBlast
        [HttpPost]
        public async Task<IActionResult> SendBlast([FromBody] SendCampaignModel model)
        {
            if (model == null) return BadRequest("Invalid operational payload request data.");

            var template = await _context.EmailTemplates.FindAsync(model.TemplateId);
            if (template == null) return NotFound("The requested template archetype structure does not exist.");

            // 1. Resolve Target Audience Segment Matrix
            List<string> targetEmails;
            if (model.SendToAll)
            {
                targetEmails = await _context.Newsletters.Select(n => n.Email).ToListAsync();
            }
            else
            {
                targetEmails = model.TargetEmails ?? new List<string>();
            }

            if (!targetEmails.Any()) return BadRequest("No active recipient emails selected to receive blast.");

            // 2. Parse Dynamic Variable Replacements
            string processedBody = template.HtmlBody;

            // Handle the dynamic {{WeeklyArticles}} placeholder block if chosen
            if (processedBody.Contains("{{WeeklyArticles}}"))
            {
                var oneWeekAgo = DateTime.Now.AddDays(-7);
                var recentArticles = await _context.Articles
                    .Where(a => a.CreatedAt >= oneWeekAgo)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                var sb = new StringBuilder("<ul style='font-family:sans-serif; line-height:1.6;'>");
                foreach (var art in recentArticles)
                {
                    sb.Append($"<li style='margin-bottom:8px;'><strong>{art.Title}</strong></li>");
                }
                sb.Append("</ul>");

                if (!recentArticles.Any())
                {
                    processedBody = processedBody.Replace("{{WeeklyArticles}}", "<p style='color:#888;'>No new archival publications logged this week.</p>");
                }
                else
                {
                    processedBody = processedBody.Replace("{{WeeklyArticles}}", sb.ToString());
                }
            }

            // Bind the admin's extra textarea message into the core template body placeholder
            processedBody = processedBody.Replace("{{AdminNotes}}", model.CustomNotes ?? "");

            // 3. Dispatch Emails via System Infrastructure Worker Pipeline
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
                    // Log mail server exceptions here if needed, continues loop for remaining targets
                    System.Diagnostics.Debug.WriteLine($"Failed execution path for address row {email}: {ex.Message}");
                }
            }

            return Ok(new { success = true, count = successCount });
        }
    }
}