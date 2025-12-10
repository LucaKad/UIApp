using System.IO;

namespace NlpUiDemo.ValueObjects
{
    public class FilePaths
    {
        public string ArticlePath { get; }
        public string TokensPath { get; }
        public string MetadataPath { get; }

        public FilePaths(string basePath, string fileName)
        {
            ArticlePath = Path.Combine(basePath, $"{fileName}.txt");
            TokensPath = Path.Combine(basePath, $"{fileName}_tokens.json");
            MetadataPath = Path.Combine(basePath, $"{fileName}_metadata.json");
        }

        public bool AllExist()
        {
            return File.Exists(ArticlePath) && 
                   File.Exists(TokensPath) && 
                   File.Exists(MetadataPath);
        }
    }
}

