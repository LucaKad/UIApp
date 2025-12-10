using System.Collections.Generic;
using System.Threading.Tasks;
using NlpUiDemo.Models;

namespace NlpUiDemo.Interfaces
{
    public interface IArticleRepository
    {
        Task<SavedArticle> SaveAsync(TokenizedArticle tokenizedArticle);
        Task<IEnumerable<SavedArticle>> GetAllAsync();
        Task<TokenizedArticle> LoadAsync(SavedArticle savedArticle);
        Task DeleteAsync(SavedArticle savedArticle);
        Task<bool> ExistsAsync(SavedArticle savedArticle);
    }
}

