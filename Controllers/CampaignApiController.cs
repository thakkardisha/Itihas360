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
        private readonly IConfiguration _config;

        public CampaignApiController(Itihas360Context context, IEmailService emailService, IConfiguration config)
        {
            _context = context;
            _emailService = emailService;
            _config = config;
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

            System.Diagnostics.Debug.WriteLine($"Found {targetEmails.Count} subscribers in processing queue.");

            if (!targetEmails.Any())
            {
                return Ok(new { success = true, count = 0, message = "Database table returned no email string rows." });
            }

            // Build the base URL (e.g. "https://www.itihas360.com") from config or request
            string baseUrl = _config["AppSettings:BaseUrl"]?.TrimEnd('/')
                             ?? $"{Request.Scheme}://{Request.Host}";

            string processedBody = template.HtmlBody;

            // ── "New Article Alert" template ──────────────────────────────────────────
            // Replace {{ArticleLink}} with a clickable link to the selected article
            if (processedBody.Contains("{{ArticleLink}}"))
            {
                if (!string.IsNullOrWhiteSpace(model.SelectedArticleSlug))
                {
                    var article = await _context.Articles
                        .Where(a => a.Slug == model.SelectedArticleSlug && a.IsPublished == true && a.IsDeleted != true)
                        .Select(a => new { a.Title, a.Slug })
                        .FirstOrDefaultAsync();

                    if (article != null)
                    {
                        string articleUrl = $"{baseUrl}/article/{article.Slug}";
                        string linkHtml = $"<a href=\"{articleUrl}\" style=\"color:#a07820; font-weight:bold;\">{article.Title}</a>";
                        processedBody = processedBody.Replace("{{ArticleLink}}", linkHtml);
                    }
                    else
                    {
                        processedBody = processedBody.Replace("{{ArticleLink}}", "the latest article on our website");
                    }
                }
                else
                {
                    processedBody = processedBody.Replace("{{ArticleLink}}", "the latest article on our website");
                }
            }

            // ── "Weekly Articles Alert" template ──────────────────────────────────────
            // Replace {{WeeklyArticles}} with a clickable list of this week's articles
            if (processedBody.Contains("{{WeeklyArticles}}"))
            {
                var oneWeekAgo = DateTime.Now.AddDays(-7);
                var recentArticles = await _context.Articles
                    .Where(a => a.CreatedAt >= oneWeekAgo && a.IsPublished == true && a.IsDeleted != true)
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => new { a.Title, a.Slug })
                    .ToListAsync();

                string weeklyHtml;
                if (recentArticles.Any())
                {
                    var sb = new StringBuilder("<ul style=\"padding-left:20px;\">");
                    foreach (var art in recentArticles)
                    {
                        string articleUrl = $"{baseUrl}/article/{art.Slug}";
                        sb.Append($"<li style=\"margin-bottom:8px;\">" +
                                  $"<a href=\"{articleUrl}\" style=\"color:#a07820; font-weight:bold; text-decoration:none;\">{art.Title}</a>" +
                                  $"</li>");
                    }
                    sb.Append("</ul>");
                    weeklyHtml = sb.ToString();
                }
                else
                {
                    weeklyHtml = "<p>No new articles were published this week.</p>";
                }

                processedBody = processedBody.Replace("{{WeeklyArticles}}", weeklyHtml);
            }

            // ── Common placeholder ────────────────────────────────────────────────────
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