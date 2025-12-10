using System.Collections.Generic;
using System.Threading.Tasks;
using NlpUiDemo.Models;

namespace NlpUiDemo.Interfaces
{
    public interface IWikipediaService
    {
        Task<IEnumerable<WikipediaSearchResult>> SearchArticlesAsync(string query, int limit = 10);
        Task<Article> GetArticleAsync(int pageId);
    }
}

