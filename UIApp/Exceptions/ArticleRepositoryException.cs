namespace NlpUiDemo.Exceptions
{
    public class ArticleRepositoryException : DomainException
    {
        public ArticleRepositoryException(string message) : base(message) { }
        public ArticleRepositoryException(string message, System.Exception innerException) : base(message, innerException) { }
    }
}

