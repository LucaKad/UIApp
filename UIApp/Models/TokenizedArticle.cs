using System;
using System.Collections.Generic;

namespace NlpUiDemo.Models
{
    public class TokenizedArticle
    {
        public Article Article { get; set; }
        public List<string> Tokens { get; set; } = new List<string>();
        public DateTime TokenizedDate { get; set; } = DateTime.Now;

        public int TokenCount => Tokens.Count;
        public bool IsValid => Article != null && Article.IsValid && Tokens.Count > 0;
    }
}

