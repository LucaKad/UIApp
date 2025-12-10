namespace NlpUiDemo.Models
{
    public class WikipediaSearchResult
    {
        public string Title { get; set; } = "";
        public string Snippet { get; set; } = "";
        public int PageId { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Title) && PageId > 0;
    }
}

