using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NlpUiDemo.Interfaces;
using NlpUiDemo.Models;
using NlpUiDemo.Exceptions;
using NlpUiDemo.ValueObjects;

namespace NlpUiDemo.Repositories
{
    public class FileArticleRepository : IArticleRepository
    {
        private readonly string _articlesDirectory;
        private readonly JsonSerializerOptions _jsonOptions;

        public FileArticleRepository(string articlesDirectory)
        {
            _articlesDirectory = articlesDirectory ?? throw new ArgumentNullException(nameof(articlesDirectory));
            
            if (!Directory.Exists(_articlesDirectory))
            {
                Directory.CreateDirectory(_articlesDirectory);
            }

            _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        }

        public async Task<SavedArticle> SaveAsync(TokenizedArticle tokenizedArticle)
        {
            ValidateTokenizedArticle(tokenizedArticle);

            try
            {
                var fileName = GenerateFileName(tokenizedArticle.Article.Title);
                var filePaths = new FilePaths(_articlesDirectory, fileName);

                await SaveArticleContentAsync(filePaths.ArticlePath, tokenizedArticle.Article.Content);
                await SaveTokensAsync(filePaths.TokensPath, tokenizedArticle.Tokens);
                await SaveMetadataAsync(filePaths.MetadataPath, tokenizedArticle, filePaths);

                return CreateSavedArticle(tokenizedArticle, fileName, filePaths);
            }
            catch (Exception ex)
            {
                throw new ArticleRepositoryException($"Failed to save article: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<SavedArticle>> GetAllAsync()
        {
            try
            {
                if (!Directory.Exists(_articlesDirectory))
                    return Enumerable.Empty<SavedArticle>();

                var metadataFiles = Directory.GetFiles(_articlesDirectory, "*_metadata.json");
                var articles = new List<SavedArticle>();

                foreach (var metadataFile in metadataFiles)
                {
                    var savedArticle = TryLoadMetadata(metadataFile);
                    if (savedArticle != null)
                    {
                        articles.Add(savedArticle);
                    }
                }

                return articles.OrderByDescending(a => a.SavedDate);
            }
            catch (Exception ex)
            {
                throw new ArticleRepositoryException($"Failed to load saved articles: {ex.Message}", ex);
            }
        }

        public async Task<TokenizedArticle> LoadAsync(SavedArticle savedArticle)
        {
            ValidateSavedArticle(savedArticle);

            try
            {
                var filePaths = new FilePaths(_articlesDirectory, savedArticle.FileName);
                
                if (!filePaths.AllExist())
                {
                    throw new FileNotFoundException("Article files not found");
                }

                var content = await File.ReadAllTextAsync(filePaths.ArticlePath);
                var tokensJson = await File.ReadAllTextAsync(filePaths.TokensPath);
                var tokens = JsonSerializer.Deserialize<List<string>>(tokensJson) ?? new List<string>();

                var article = new Article
                {
                    Title = savedArticle.Title,
                    Content = content,
                    RetrievedDate = savedArticle.SavedDate
                };

                return new TokenizedArticle
                {
                    Article = article,
                    Tokens = tokens,
                    TokenizedDate = savedArticle.SavedDate
                };
            }
            catch (Exception ex)
            {
                throw new ArticleRepositoryException($"Failed to load article: {ex.Message}", ex);
            }
        }

        public async Task DeleteAsync(SavedArticle savedArticle)
        {
            if (savedArticle == null)
                return;

            try
            {
                var filePaths = new FilePaths(_articlesDirectory, savedArticle.FileName);
                
                await DeleteFileIfExistsAsync(filePaths.ArticlePath);
                await DeleteFileIfExistsAsync(filePaths.TokensPath);
                await DeleteFileIfExistsAsync(filePaths.MetadataPath);
            }
            catch (Exception ex)
            {
                throw new ArticleRepositoryException($"Failed to delete article: {ex.Message}", ex);
            }
        }

        public Task<bool> ExistsAsync(SavedArticle savedArticle)
        {
            if (savedArticle == null)
                return Task.FromResult(false);

            var filePaths = new FilePaths(_articlesDirectory, savedArticle.FileName);
            return Task.FromResult(filePaths.AllExist());
        }

        private string GenerateFileName(string title)
        {
            var safeFileName = string.Join("_", title.Split(Path.GetInvalidFileNameChars()));
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"{safeFileName}_{timestamp}";
        }

        private async Task SaveArticleContentAsync(string path, string content)
        {
            await File.WriteAllTextAsync(path, content);
        }

        private async Task SaveTokensAsync(string path, IEnumerable<string> tokens)
        {
            var tokensJson = JsonSerializer.Serialize(tokens, _jsonOptions);
            await File.WriteAllTextAsync(path, tokensJson);
        }

        private async Task SaveMetadataAsync(string path, TokenizedArticle tokenizedArticle, FilePaths filePaths)
        {
            var metadata = new ArticleMetadata
            {
                Title = tokenizedArticle.Article.Title,
                ArticlePath = filePaths.ArticlePath,
                TokensPath = filePaths.TokensPath,
                TokenCount = tokenizedArticle.TokenCount,
                SavedDate = DateTime.Now,
                ContentLength = tokenizedArticle.Article.Content.Length
            };

            var metadataJson = JsonSerializer.Serialize(metadata, _jsonOptions);
            await File.WriteAllTextAsync(path, metadataJson);
        }

        private SavedArticle CreateSavedArticle(TokenizedArticle tokenizedArticle, string fileName, FilePaths filePaths)
        {
            return new SavedArticle
            {
                Title = tokenizedArticle.Article.Title,
                FileName = fileName,
                TokenCount = tokenizedArticle.TokenCount,
                SavedDate = DateTime.Now,
                ArticlePath = filePaths.ArticlePath,
                TokensPath = filePaths.TokensPath,
                MetadataPath = filePaths.MetadataPath
            };
        }

        private SavedArticle? TryLoadMetadata(string metadataFile)
        {
            try
            {
                var json = File.ReadAllText(metadataFile);
                var metadata = JsonSerializer.Deserialize<ArticleMetadata>(json);
                
                if (metadata == null || !File.Exists(metadata.ArticlePath))
                    return null;

                var fileName = Path.GetFileNameWithoutExtension(metadataFile).Replace("_metadata", "");
                return new SavedArticle
                {
                    Title = metadata.Title,
                    FileName = fileName,
                    TokenCount = metadata.TokenCount,
                    SavedDate = metadata.SavedDate,
                    ArticlePath = metadata.ArticlePath,
                    TokensPath = metadata.TokensPath,
                    MetadataPath = metadataFile
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task DeleteFileIfExistsAsync(string path)
        {
            if (File.Exists(path))
            {
                await Task.Run(() => File.Delete(path));
            }
        }

        private void ValidateTokenizedArticle(TokenizedArticle tokenizedArticle)
        {
            if (tokenizedArticle == null)
                throw new ArgumentNullException(nameof(tokenizedArticle));

            if (!tokenizedArticle.IsValid)
                throw new ArgumentException("Invalid tokenized article", nameof(tokenizedArticle));
        }

        private void ValidateSavedArticle(SavedArticle savedArticle)
        {
            if (savedArticle == null)
                throw new ArgumentNullException(nameof(savedArticle));
        }
    }
}

