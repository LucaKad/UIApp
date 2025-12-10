using System.Collections.Generic;

namespace NlpUiDemo.Interfaces
{
    public interface ITokenizationStrategy
    {
        string Name { get; }
        IEnumerable<string> Tokenize(string text);
    }
}

