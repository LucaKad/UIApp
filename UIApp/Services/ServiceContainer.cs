using System;
using NlpUiDemo.Interfaces;
using NlpUiDemo.Repositories;
using NlpUiDemo.Services;
using NlpUiDemo.Strategies;

namespace NlpUiDemo.Services
{
    public class ServiceContainer
    {
        private static ServiceContainer? _instance;
        private static readonly object _lock = new object();

        public IWikipediaService WikipediaService { get; private set; }
        public ITokenizationService TokenizationService { get; private set; }
        public IArticleRepository ArticleRepository { get; private set; }

        private ServiceContainer()
        {
            InitializeServices();
        }

        public static ServiceContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ServiceContainer();
                        }
                    }
                }
                return _instance;
            }
        }

        private void InitializeServices()
        {
            WikipediaService = new WikipediaService();
            
            var tokenizationStrategy = new BasicTokenizationStrategy();
            TokenizationService = new TokenizationService(tokenizationStrategy);

            var articlesDirectory = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "NLPExplorer",
                "Articles");
            
            ArticleRepository = new FileArticleRepository(articlesDirectory);
        }

        public void Dispose()
        {
            if (WikipediaService is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}

