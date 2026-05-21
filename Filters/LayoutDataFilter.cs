using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Itihas360.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Itihas360.Filters
{
    public class LayoutDataFilter : IAsyncActionFilter
    {
        private readonly Itihas360Context _context;

        public LayoutDataFilter(Itihas360Context context)
        {
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Execute the controller action first to let it build its view models
            var resultContext = await next();

            // Only inject if we are returning an HTML View result to the browser
            if (resultContext.Result is ViewResult viewResult)
            {
                var controller = context.Controller as Controller;
                if (controller != null)
                {
                    // 1. Centralized Global Notification Engine Rules (Last 7 Days)
                    var limitDate = DateTime.Now.AddDays(-7);
                    var recentNotifications = await _context.Articles
                        .Where(a => (a.IsPublished ?? false) == true && (a.IsDeleted ?? false) == false && a.CreatedAt >= limitDate)
                        .OrderByDescending(a => a.CreatedAt)
                        .Take(8)
                        .ToListAsync();

                    // 2. Map Uniform Shared Properties
                    controller.ViewBag.RecentNotifications = recentNotifications;
                    controller.ViewBag.UnreadNotifCount = recentNotifications.Count;

                    // 3. Centralized Shared Sidebar/Navbar Categories Layout Count
                    controller.ViewBag.Categories = await _context.Categories
                        .Where(c => (c.IsActive ?? false) == true)
                        .OrderBy(c => c.DisplayOrder)
                        .Select(c => new CategoryWithCount
                        {
                            Category = c,
                            ArticleCount = _context.Articles.Count(a => a.SectorId == c.CategoryId && (a.IsPublished ?? false) == true && (a.IsDeleted ?? false) == false)
                        })
                        .ToListAsync();

                    // 4. Fallback safeguard for LatestArticles configuration used by older modules
                    controller.ViewBag.LatestArticles = recentNotifications;

                    // 5. Shared Organization Settings Data mapping
                    controller.ViewBag.Organization = await _context.Organizations.FirstOrDefaultAsync();
                }
            }
        }
    }
}