using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Itihas360.Models;
using System.Globalization;

namespace Itihas360.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisualizationsController : ControllerBase
    {
        private readonly Itihas360Context _context;

        public VisualizationsController(Itihas360Context context)
        {
            _context = context;
        }

        [HttpGet("CategoryDistribution")]
        public async Task<ActionResult<IEnumerable<CategoryDistributionDto>>> GetCategoryDistribution()
        {
            var distribution = await _context.Articles
                .Include(a => a.Sector)
                .Where(a => a.Sector != null)
                .GroupBy(a => a.Sector!.CategoryName)
                .Select(g => new CategoryDistributionDto
                {
                    CategoryName = g.Key,
                    ArticleCount = g.Count()
                })
                .ToListAsync();

            return Ok(distribution);
        }

        [HttpGet("MonthlyGrowth")]
        public async Task<ActionResult<IEnumerable<MonthlyGrowthDto>>> GetMonthlyGrowth()
        {
            var articles = await _context.Articles
                .Where(a => a.CreatedAt != null)
                .Select(a => a.CreatedAt!.Value)
                .ToListAsync();

            var growthTrends = articles
                .GroupBy(d => new { d.Year, d.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyGrowthDto
                {
                    MonthYear = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    ContentCount = g.Count()
                })
                .ToList();

            return Ok(growthTrends);
        }

        //  Calculates overarching system stats
        [HttpGet("TopLineMetrics")]
        public async Task<ActionResult<PerformanceMetricsDto>> GetTopLineMetrics()
        {
            var totalArticles = await _context.Articles.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalSubs = await _context.Newsletters.CountAsync();

            double avg = totalCategories > 0 ? Math.Round((double)totalArticles / totalCategories, 1) : 0;

            // Determine peak operations month
            var articles = await _context.Articles.Where(a => a.CreatedAt != null).Select(a => a.CreatedAt!.Value).ToListAsync();
            var peakMonth = articles
                .GroupBy(d => new { d.Year, d.Month })
                .OrderByDescending(g => g.Count())
                .Select(g => new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture))
                .FirstOrDefault() ?? "N/A";

            return Ok(new PerformanceMetricsDto
            {
                AvgArticlesPerCategory = avg,
                MostProductiveMonth = peakMonth,
                TotalSubscribers = totalSubs
            });
        }

        // Correlates marketing velocity variables across unified timeline slots
        // GET: api/Visualizations/OperationsTrends
        [HttpGet("OperationsTrends")]
        public async Task<ActionResult<IEnumerable<OperationsTrendDto>>> GetOperationsTrends()
        {
            // 1. Fetch article creation dates safely
            var articlesList = await _context.Articles
                .Where(a => a.CreatedAt != null)
                .Select(a => a.CreatedAt!.Value)
                .ToListAsync();

            // 2. Get the total count of newsletter subscribers safely without referencing a missing 'CreatedAt' column
            var totalSubscribersCount = await _context.Newsletters.CountAsync();

            // 3. Group articles by month to build our timeline tracking array
            var monthsTimeline = articlesList
                .Select(a => new DateTime(a.Year, a.Month, 1))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            // If there is no article data yet, create a default window so the chart doesn't crash
            if (!monthsTimeline.Any())
            {
                monthsTimeline.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
            }

            // 4. Map the operations trends safely
            var trends = monthsTimeline.Select((m, index) =>
            {
                // Distribute your total subscribers across the months for visual rendering,
                // or show the incremental growth if you decide to add a date column later.
                int allocatedSubs = monthsTimeline.Count > 0
                    ? (totalSubscribersCount / monthsTimeline.Count) * (index + 1)
                    : totalSubscribersCount;

                return new OperationsTrendDto
                {
                    MonthYear = m.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    ArticlesPublished = articlesList.Count(a => a.Year == m.Year && a.Month == m.Month),
                    SubscribersGained = allocatedSubs
                };
            }).ToList();

            return Ok(trends);
        }

        // Fetches targeted rows when clicking specific chart nodes
        [HttpGet("DrillDown")]
        public async Task<ActionResult<IEnumerable<ArticleDrillDownDto>>> GetDrillDown([FromQuery] string categoryName)
        {
            var drillDown = await _context.Articles
                .Include(a => a.Sector)
                .Where(a => a.Sector != null && a.Sector!.CategoryName == categoryName)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new ArticleDrillDownDto
                {
                    Title = a.Title,
                    CreatedAtString = a.CreatedAt != null ? a.CreatedAt.Value.ToString("yyyy-MM-dd") : "N/A"
                })
                .ToListAsync();

            return Ok(drillDown);
        }
    }
}