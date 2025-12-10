using System;

namespace NlpUiDemo.Models
{
    public class Article
    {
        private string _title = string.Empty;
        private string _content = string.Empty;

        public string Title
        {
            get => _title;
            set => _title = value ?? throw new ArgumentNullException(nameof(Title));
        }

        public string Content
        {
            get => _content;
            set => _content = value ?? throw new ArgumentNullException(nameof(Content));
        }

        public int PageId { get; set; }
        public DateTime RetrievedDate { get; set; } = DateTime.Now;

        public bool IsValid => !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(Content);
        
        public int WordCount => string.IsNullOrWhiteSpace(Content) 
            ? 0 
            : Content.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;

        public int CharacterCount => Content?.Length ?? 0;
    }
}

