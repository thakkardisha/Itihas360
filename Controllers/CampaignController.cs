using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Itihas360.Models;
using Itihas360.Services;

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

            ViewBag.Subscribers = await _context.Newsletters
                .OrderByDescending(n => n.SubscribedWhen)
                .ToListAsync();

            ViewBag.Articles = await _context.Articles
                .Where(a => a.IsPublished == true && (a.IsDeleted == null || a.IsDeleted == false))
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new ArticleDropdownDto { Title = a.Title, Slug = a.Slug })
                .ToListAsync();

            return PartialView("_CampaignWorkspace");
        }
    }
}