using System.Collections.Generic;

namespace Itihas360.Models
{
    public class HomeViewModel
    {
        public List<Article> LatestArticles { get; set; } = new List<Article>();
        public List<Article> SecondaryArticles { get; set; } = new List<Article>();
        public Article FeaturedArticle { get; set; }
        public List<CategoryWithCount> Categories { get; set; } = new List<CategoryWithCount>();
        public List<Article> RecentNotifications { get; set; } = new List<Article>();

        public int TotalArticles { get; set; }
        public int TotalCategories { get; set; }
        public int UnreadNotifCount { get; set; }
        public bool HasTodayNotifications { get; set; }
        public Organization Organization { get; set; }
    }

    public class CategoryWithCount
{
    public Category Category { get; set; }
    public int ArticleCount { get; set; }

    // Helper properties
    public string CategoryName => Category?.CategoryName ?? "Uncategorized";
    public string CategorySlug => Category?.CategorySlug ?? "";
    public int? DisplayOrder => Category?.DisplayOrder;
    public List<Article> Articles => Category?.Articles.ToList() ?? new List<Article>();
}
}