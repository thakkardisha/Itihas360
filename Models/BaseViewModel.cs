using System.Collections.Generic;

namespace Itihas360.Models.ViewModels
{
    public class BaseViewModel
    {
        public Organization Organization { get; set; }
        public List<CategoryWithCount> Categories { get; set; } = new List<CategoryWithCount>();
        public List<Article> LatestArticles { get; set; } = new List<Article>();
        public List<Article> RecentNotifications { get; set; } = new List<Article>();
        public int UnreadNotifCount { get; set; }
        public bool HasTodayNotifications { get; set; }
    }
}