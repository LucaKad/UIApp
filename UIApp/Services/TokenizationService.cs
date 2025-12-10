using System;
using System.Collections.Generic;
using NlpUiDemo.Interfaces;
using NlpUiDemo.Models;
using NlpUiDemo.Exceptions;

namespace NlpUiDemo.Services
{
    public class TokenizationService : ITokenizationService
    {
        private readonly ITokenizationStrategy _strategy;

        public TokenizationService(ITokenizationStrategy strategy)
        {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        public TokenizedArticle Tokenize(Article article)
        {
            if (article == null)
                throw new ArgumentNullException(nameof(article));

            if (!article.IsValid)
            {
                throw new TokenizationException("Cannot tokenize invalid article");
            }

            try
            {
                var tokens = _strategy.Tokenize(article.Content);
                
                return new TokenizedArticle
                {
                    Article = article,
                    Tokens = new List<string>(tokens),
                    TokenizedDate = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                throw new TokenizationException($"Tokenization failed: {ex.Message}", ex);
            }
        }

        public IEnumerable<string> TokenizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<string>();

            try
            {
                return _strategy.Tokenize(text);
            }
            catch (Exception ex)
            {
                throw new TokenizationException($"Text tokenization failed: {ex.Message}", ex);
            }
        }
    }
}

