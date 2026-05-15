using System.Collections.Generic;
using Itihas360.Models;

namespace Itihas360.Models.ViewModels
{
    public class ArticleReadingViewModel
    {
        public Article Article { get; set; }
        public List<Mcqquestion> Questions { get; set; }
    }
}