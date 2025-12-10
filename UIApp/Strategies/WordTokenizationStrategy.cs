using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NlpUiDemo.Interfaces;

namespace NlpUiDemo.Strategies
{
    public class WordTokenizationStrategy : ITokenizationStrategy
    {
        public string Name => "Word";

        public IEnumerable<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Enumerable.Empty<string>();

            var cleanedText = CleanText(text);
            return ExtractWords(cleanedText);
        }

        private string CleanText(string text)
        {
            text = Regex.Replace(text, @"&[a-zA-Z]+;", " ");
            text = Regex.Replace(text, @"[^\w\s]", " ");
            text = Regex.Replace(text, @"\s+", " ");
            return text.Trim();
        }

        private IEnumerable<string> ExtractWords(string text)
        {
            return Regex.Split(text, @"\s+")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Where(w => w.Length > 1)
                .Select(w => w.ToLowerInvariant());
        }
    }
}

