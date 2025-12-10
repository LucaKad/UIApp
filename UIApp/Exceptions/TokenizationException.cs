namespace NlpUiDemo.Exceptions
{
    public class TokenizationException : DomainException
    {
        public TokenizationException(string message) : base(message) { }
        public TokenizationException(string message, System.Exception innerException) : base(message, innerException) { }
    }
}

