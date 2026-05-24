namespace Itihas360.Models
{
    public class CategoryDistributionDto
    {
        public string CategoryName { get; set; } = null!;
        public int ArticleCount { get; set; }
    }

    public class MonthlyGrowthDto
    {
        public string MonthYear { get; set; } = null!;
        public int ContentCount { get; set; }
    }

    // Added: Transfers calculated quick analytics stats down to dashboard widgets
    public class PerformanceMetricsDto
    {
        public double AvgArticlesPerCategory { get; set; }
        public string MostProductiveMonth { get; set; } = "N/A";
        public int TotalSubscribers { get; set; }
    }

    // Maps operational dual-axis trends across chronological steps
    public class OperationsTrendDto
    {
        public string MonthYear { get; set; } = null!;
        public int ArticlesPublished { get; set; }
        public int SubscribersGained { get; set; }
    }

    // 🚀 Added: Lightweight blueprint for the interactive list query
    public class ArticleDrillDownDto
    {
        public string Title { get; set; } = null!;
        public string CreatedAtString { get; set; } = null!;
    }
}