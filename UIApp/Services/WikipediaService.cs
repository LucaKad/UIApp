using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using NlpUiDemo.Interfaces;
using NlpUiDemo.Models;
using NlpUiDemo.Exceptions;

namespace NlpUiDemo.Services
{
    public class WikipediaService : IWikipediaService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://en.wikipedia.org/w/api.php";
        private const string UserAgent = "NLP-Explorer/1.0 (Educational App)";

        public WikipediaService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        }

        public async Task<IEnumerable<WikipediaSearchResult>> SearchArticlesAsync(string query, int limit = 10)
        {
            ValidateQuery(query);

            try
            {
                var url = BuildSearchUrl(query, limit);
                var response = await _httpClient.GetStringAsync(url);
                return ParseSearchResults(response);
            }
            catch (Exception ex)
            {
                throw new WikipediaServiceException($"Failed to search Wikipedia: {ex.Message}", ex);
            }
        }

        public async Task<Article> GetArticleAsync(int pageId)
        {
            ValidatePageId(pageId);

            try
            {
                var url = BuildArticleUrl(pageId);
                var response = await _httpClient.GetStringAsync(url);
                return ParseArticle(response, pageId);
            }
            catch (Exception ex)
            {
                throw new WikipediaServiceException($"Failed to retrieve article: {ex.Message}", ex);
            }
        }

        private string BuildSearchUrl(string query, int limit)
        {
            var encodedQuery = Uri.EscapeDataString(query);
            return $"{BaseUrl}?action=query&list=search&srsearch={encodedQuery}&format=json&srlimit={limit}";
        }

        private string BuildArticleUrl(int pageId)
        {
            return $"{BaseUrl}?action=query&prop=extracts&exintro=false&explaintext=true&pageids={pageId}&format=json";
        }

        private IEnumerable<WikipediaSearchResult> ParseSearchResults(string jsonResponse)
        {
            var jsonDoc = JsonDocument.Parse(jsonResponse);
            var results = new List<WikipediaSearchResult>();

            if (jsonDoc.RootElement.TryGetProperty("query", out var queryElement) &&
                queryElement.TryGetProperty("search", out var search))
            {
                foreach (var item in search.EnumerateArray())
                {
                    var result = CreateSearchResult(item);
                    if (result.IsValid)
                    {
                        results.Add(result);
                    }
                }
            }

            return results;
        }

        private WikipediaSearchResult CreateSearchResult(JsonElement item)
        {
            return new WikipediaSearchResult
            {
                Title = item.GetProperty("title").GetString() ?? string.Empty,
                Snippet = item.GetProperty("snippet").GetString() ?? string.Empty,
                PageId = item.GetProperty("pageid").GetInt32()
            };
        }

        private Article ParseArticle(string jsonResponse, int pageId)
        {
            var jsonDoc = JsonDocument.Parse(jsonResponse);
            var (title, content) = ExtractArticleData(jsonDoc);

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new WikipediaServiceException("Article content is empty");
            }

            return new Article
            {
                Title = title,
                Content = content,
                PageId = pageId,
                RetrievedDate = DateTime.Now
            };
        }

        private (string title, string content) ExtractArticleData(JsonDocument jsonDoc)
        {
            string title = string.Empty;
            string content = string.Empty;

            if (jsonDoc.RootElement.TryGetProperty("query", out var query) &&
                query.TryGetProperty("pages", out var pages))
            {
                foreach (var page in pages.EnumerateObject())
                {
                    if (page.Value.TryGetProperty("title", out var titleElement))
                    {
                        title = titleElement.GetString() ?? string.Empty;
                    }
                    if (page.Value.TryGetProperty("extract", out var extract))
                    {
                        content = extract.GetString() ?? string.Empty;
                    }
                    break;
                }
            }

            return (title, content);
        }

        private void ValidateQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Search query cannot be null or empty", nameof(query));
            }
        }

        private void ValidatePageId(int pageId)
        {
            if (pageId <= 0)
            {
                throw new ArgumentException("Page ID must be greater than zero", nameof(pageId));
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}

