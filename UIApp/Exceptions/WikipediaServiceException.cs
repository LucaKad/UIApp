namespace NlpUiDemo.Exceptions
{
    public class WikipediaServiceException : DomainException
    {
        public WikipediaServiceException(string message) : base(message) { }
        public WikipediaServiceException(string message, System.Exception innerException) : base(message, innerException) { }
    }
}

