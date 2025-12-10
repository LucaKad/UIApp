using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NlpUiDemo.Interfaces;

namespace NlpUiDemo.Strategies
{
    public class BasicTokenizationStrategy : ITokenizationStrategy
    {
        public string Name => "Basic";

        public IEnumerable<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Enumerable.Empty<string>();

            var cleanedText = CleanText(text);
            return ExtractTokens(cleanedText);
        }

        private string CleanText(string text)
        {
            text = Regex.Replace(text, @"&[a-zA-Z]+;", " ");
            text = Regex.Replace(text, @"\s+", " ");
            return text;
        }

        private IEnumerable<string> ExtractTokens(string text)
        {
            var tokens = new List<string>();
            var words = Regex.Split(text, @"(\s+|[,;:!?\.\-\(\)\[\]""'`])")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Select(w => w.Trim())
                .Where(w => w.Length > 0);

            foreach (var word in words)
            {
                if (IsValidToken(word))
                {
                    tokens.Add(word.ToLowerInvariant());
                }
            }

            return tokens;
        }

        private bool IsValidToken(string word)
        {
            return word.Length > 1 && char.IsLetterOrDigit(word[0]);
        }
    }
}

