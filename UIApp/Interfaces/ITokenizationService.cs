using System.Collections.Generic;
using NlpUiDemo.Models;

namespace NlpUiDemo.Interfaces
{
    public interface ITokenizationService
    {
        TokenizedArticle Tokenize(Article article);
        IEnumerable<string> TokenizeText(string text);
    }
}

