using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Itihas360.Models;
using Microsoft.EntityFrameworkCore;

namespace Itihas360.Services
{
    public class NewsFetchService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _httpClient;
        private Timer? _timer;

        // API key from https://currentsapi.services/ after registering!
        private const string ApiKey = "API_KEY";

        public NewsFetchService(IServiceProvider serviceProvider, HttpClient httpClient)
        {
            _serviceProvider = serviceProvider;
            _httpClient = httpClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            try
            {
                // Requesting trending English language news from India
                string url = $"https://api.currentsapi.services/v1/latest-news?language=en&country=IN&apiKey={ApiKey}";

                var response = await _httpClient.GetFromJsonAsync<CurrentsNewsResponse>(url);

                if (response?.News != null && response.News.Any())
                {
                    // ── 1. DEFINE CATEGORY BLACKLIST ──
                    var excludedCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "lifestyle", "home", "fashion", "food", "travel", "entertainment", "bollywood"
            };

                    // ── 2. DEFINE SENSATIONAL / CRIME KEYWORD BLACKLIST ──
                    var excludedKeywords = new[]
                    {
                        "death", "case", "probe", "arrest", "killed", "accused", "murder",
                        "suicide", "police", "scam", "court", "cbi", "incident", "firing",
                        "assault", "stabbed", "theft", "robbery", "encounter", "extortion", 
                        "bollywood", "actor", "actress"
            };

                    // ── 3. APPLY COMPREHENSIVE FILTER ENGINE ──
                    var filteredNews = response.News
                        .Where(item => item.Category == null || !item.Category.Any(cat => excludedCategories.Contains(cat)))
                        .Where(item => {
                            // Combine title and description to run a deep scan on content context
                            string textToScan = $"{(item.Title ?? "")} {(item.Description ?? "")}";

                            if (string.IsNullOrWhiteSpace(textToScan)) return true;

                            // If any blacklisted word is found, exclude the item (return false)
                            return !excludedKeywords.Any(word => textToScan.Contains(word, StringComparison.OrdinalIgnoreCase));
                        })
                        .ToList();

                    // ── 4. DB INJECTION ENGINE ──
                    if (filteredNews.Any())
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<Itihas360Context>();

                        // Wipe old cache rows to prevent table bloating
                        var oldEntries = await dbContext.NewsFeedCaches.ToListAsync();
                        dbContext.NewsFeedCaches.RemoveRange(oldEntries);

                        // Insert the top 5 clean trending items into your cache
                        foreach (var item in filteredNews.Take(5))
                        {
                            dbContext.NewsFeedCaches.Add(new NewsFeedCache
                            {
                                ExternalArticleId = item.Id,
                                Headline = !string.IsNullOrEmpty(item.Title) ? item.Title : "Breaking Dispatch",
                                Summary = item.Description,
                                SourceName = item.Author,
                                SourceUrl = item.Url,
                                ImageUrl = !string.IsNullOrEmpty(item.Image) && item.Image != "None"
                                    ? item.Image
                                    : "/images/default-news.jpg",
                                PublishedAt = DateTime.TryParse(item.Published, out var pubDate) ? pubDate : DateTime.Now,
                                FetchedAt = DateTime.Now,
                                IsVisible = true
                            });
                        }

                        await dbContext.SaveChangesAsync();
                        Console.WriteLine($"[News Work]: Cache updated successfully with clean records at {DateTime.Now}");
                    }
                    else
                    {
                        Console.WriteLine($"[News Work Warning]: All incoming dispatches were filtered out by the security parameters at {DateTime.Now}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[News Work Error]: Synchronization failed: {ex.Message}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }

    public class CurrentsNewsResponse
    {
        public List<CurrentsNewsItem> News { get; set; } = new();
    }

    public class CurrentsNewsItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Published { get; set; } = string.Empty;

        // 💡 Added: Currents API maps categories as an array of strings in their JSON schema
        public List<string> Category { get; set; } = new();
    }
}