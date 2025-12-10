using System;
using System.IO;

namespace NlpUiDemo.Models
{
    public class SavedArticle
    {
        public string Title { get; set; } = "";
        public string FileName { get; set; } = "";
        public int TokenCount { get; set; }
        public DateTime SavedDate { get; set; }
        public string ArticlePath { get; set; } = "";
        public string TokensPath { get; set; } = "";
        public string MetadataPath { get; set; } = "";

        public bool ExistsOnDisk => File.Exists(ArticlePath) && File.Exists(TokensPath) && File.Exists(MetadataPath);
    }
}

