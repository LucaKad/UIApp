using System;

namespace NlpUiDemo.Models
{
    public class ArticleMetadata
    {
        public string Title { get; set; } = "";
        public string ArticlePath { get; set; } = "";
        public string TokensPath { get; set; } = "";
        public int TokenCount { get; set; }
        public DateTime SavedDate { get; set; }
        public int ContentLength { get; set; }
    }
}

