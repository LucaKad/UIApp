using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NlpUiDemo
{
    public partial class MainWindow : Window
    {
        private bool _isDarkTheme = false;
        private readonly HttpClient _httpClient;
        private string _currentArticleTitle = string.Empty;
        private string _currentArticleContent = string.Empty;
        private List<string> _currentTokens = new List<string>();
        private ObservableCollection<SavedArticle> _savedArticles = new ObservableCollection<SavedArticle>();
        private readonly string _articlesDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NLPExplorer", "Articles");

        public MainWindow()
        {
            InitializeComponent();

            // Initialize HttpClient for Wikipedia API calls
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "NLP-Explorer/1.0 (Educational App)");

            // Initialize articles directory
            if (!Directory.Exists(_articlesDirectory))
            {
                Directory.CreateDirectory(_articlesDirectory);
            }

            // Load saved articles
            LoadSavedArticles();

            // Initialize base font size for window
            this.FontSize = FontSizeSlider.Value;
            
            // Set DataGrid items source after window is loaded
            this.Loaded += (s, e) =>
            {
                SavedArticlesGrid.ItemsSource = _savedArticles;
            };
            
            // Close popups when clicking outside (but not on menu buttons)
            this.MouseDown += (s, e) =>
            {
                var source = e.OriginalSource as FrameworkElement;
                if (source != null && 
                    source != FileMenuButton && 
                    source != ViewMenuButton && 
                    source != HelpMenuButton &&
                    !IsDescendantOf(FileMenuButton, source) &&
                    !IsDescendantOf(ViewMenuButton, source) &&
                    !IsDescendantOf(HelpMenuButton, source) &&
                    !IsDescendantOf(FileMenuPopup.Child, source) &&
                    !IsDescendantOf(ViewMenuPopup.Child, source) &&
                    !IsDescendantOf(HelpMenuPopup.Child, source))
                {
                    CloseAllPopups();
                }
            };
            this.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Escape)
                    CloseAllPopups();
            };
            
            this.Closed += (s, e) => _httpClient?.Dispose();
        }

        private bool IsDescendantOf(DependencyObject ancestor, DependencyObject descendant)
        {
            if (ancestor == null || descendant == null) return false;
            var parent = VisualTreeHelper.GetParent(descendant);
            while (parent != null)
            {
                if (parent == ancestor) return true;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return false;
        }

        // -------- CUSTOM MENU BAR HANDLERS --------

        private void FileMenuButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllPopups();
            FileMenuPopup.IsOpen = !FileMenuPopup.IsOpen;
        }

        private void ViewMenuButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllPopups();
            ViewMenuPopup.IsOpen = !ViewMenuPopup.IsOpen;
        }

        private void HelpMenuButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllPopups();
            HelpMenuPopup.IsOpen = !HelpMenuPopup.IsOpen;
        }

        private void CloseAllPopups()
        {
            FileMenuPopup.IsOpen = false;
            ViewMenuPopup.IsOpen = false;
            HelpMenuPopup.IsOpen = false;
        }

        private void MenuButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button button)
            {
                if (_isDarkTheme)
                    button.Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
                else
                    button.Background = new SolidColorBrush(Color.FromArgb(30, 0, 0, 0));
            }
        }

        private void MenuButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = Brushes.Transparent;
            }
        }

        private void MenuItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button button)
            {
                if (_isDarkTheme)
                    button.Background = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255));
                else
                    button.Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0));
            }
        }

        private void MenuItem_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = Brushes.Transparent;
            }
        }

        // -------- MENU ITEM HANDLERS --------

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CloseAllPopups();
            Close();
        }

        private void ResetSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CloseAllPopups();
            FontSizeSlider.Value = 14;
            this.FontSize = 14;

            AccentColorComboBox.SelectedIndex = 0;
            SetAccentColor(Colors.RoyalBlue);

            _isDarkTheme = false;
            SetTheme(isDark: false);
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CloseAllPopups();
            var aboutDialog = new AboutDialog(this, this.FontSize, _isDarkTheme);
            aboutDialog.ShowDialog();
        }

        // -------- FONT SIZE / UI ADJUSTMENT --------

        private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                this.FontSize = e.NewValue;
            }
        }

        private void IncreaseFontButton_Click(object sender, RoutedEventArgs e)
        {
            if (FontSizeSlider.Value < FontSizeSlider.Maximum)
                FontSizeSlider.Value += 2;
        }

        private void DecreaseFontButton_Click(object sender, RoutedEventArgs e)
        {
            if (FontSizeSlider.Value > FontSizeSlider.Minimum)
                FontSizeSlider.Value -= 2;
        }

        // -------- ACCENT COLOR --------

        private void AccentColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            var selectedItem = (AccentColorComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (selectedItem == null) return;

            SetAccentColorAndMode(selectedItem);
        }

        private void SetAccentColorAndMode(string selectedItem)
        {
            switch (selectedItem)
            {
                case "Blue":
                    if (_isDarkTheme)
                        SetAccentColor(Colors.DarkBlue);
                    else
                        SetAccentColor(Colors.RoyalBlue);
                    break;
                case "Green":
                    if (_isDarkTheme)
                        SetAccentColor(Colors.DarkGreen);
                    else
                        SetAccentColor(Colors.SeaGreen);
                    break;
                case "Purple":
                    if (_isDarkTheme)
                        SetAccentColor(Colors.DarkOrchid);
                    else
                        SetAccentColor(Colors.MediumOrchid);
                    break;
                case "Orange":
                    if (_isDarkTheme)
                        SetAccentColor(Colors.SaddleBrown);
                    else
                        SetAccentColor(Colors.DarkOrange);
                    break;
            }
        }

        private void SetAccentColor(Color color)
        {
            if (Resources["AccentBrush"] is SolidColorBrush accentBrush)
            {
                accentBrush.Color = color;
            }
            if (Resources["HeaderBackgroundBrush"] is SolidColorBrush headerBrush)
            {
                headerBrush.Color = color;
            }
        }

        // -------- THEME TOGGLING --------

        private void ToggleThemeButton_Click(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;
            SetTheme(_isDarkTheme);

            var selectedItem = (AccentColorComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (selectedItem == null) return;

            SetAccentColorAndMode(selectedItem);
        }

        private void SetTheme(bool isDark)
        {
            if (isDark)
            {
                if (Resources["WindowBackgroundBrush"] is SolidColorBrush wBrush)
                    wBrush.Color = Colors.DimGray;

                if (Resources["FooterBackgroundBrush"] is SolidColorBrush fBrush)
                    fBrush.Color = Colors.DeepSkyBlue;

                if (Resources["WindowTextBrush"] is SolidColorBrush tBrush)
                    tBrush.Color = Colors.WhiteSmoke;

                Foreground = Brushes.WhiteSmoke;
            }
            else
            {
                if (Resources["WindowBackgroundBrush"] is SolidColorBrush wBrush)
                    wBrush.Color = Colors.LightGray;

                if (Resources["FooterBackgroundBrush"] is SolidColorBrush fBrush)
                    fBrush.Color = Colors.LightSkyBlue;

                if (Resources["WindowTextBrush"] is SolidColorBrush tBrush)
                    tBrush.Color = Colors.Black;

                Foreground = Brushes.Black;
            }

            Background = (Brush)Resources["WindowBackgroundBrush"];
        }

        // -------- NAVIGATION BUTTON --------

        private void GoToOverviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabControl != null)
            {
                MainTabControl.SelectedIndex = 0;
            }
        }

        // -------- WIKIPEDIA SEARCH FUNCTIONALITY --------

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await PerformSearch();
        }

        private async void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await PerformSearch();
            }
        }

        private async Task PerformSearch()
        {
            string searchQuery = SearchTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                SearchStatusText.Text = "Please enter a search term.";
                return;
            }

            SearchStatusText.Text = "Searching...";
            SearchButton.IsEnabled = false;
            ResultsListBox.ItemsSource = null;

            try
            {
                // Wikipedia API search endpoint
                string url = $"https://en.wikipedia.org/w/api.php?action=query&list=search&srsearch={Uri.EscapeDataString(searchQuery)}&format=json&srlimit=10";
                
                var response = await _httpClient.GetStringAsync(url);
                var jsonDoc = JsonDocument.Parse(response);

                var results = new List<WikipediaSearchResult>();
                if (jsonDoc.RootElement.TryGetProperty("query", out var query) &&
                    query.TryGetProperty("search", out var search))
                {
                    foreach (var item in search.EnumerateArray())
                    {
                        results.Add(new WikipediaSearchResult
                        {
                            Title = item.GetProperty("title").GetString() ?? "",
                            Snippet = item.GetProperty("snippet").GetString() ?? "",
                            PageId = item.GetProperty("pageid").GetInt32()
                        });
                    }
                }

                ResultsListBox.ItemsSource = results;
                SearchStatusText.Text = results.Count > 0 
                    ? $"Found {results.Count} result(s)." 
                    : "No results found.";
            }
            catch (Exception ex)
            {
                SearchStatusText.Text = $"Error: {ex.Message}";
                MessageBox.Show($"Failed to search Wikipedia: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SearchButton.IsEnabled = true;
            }
        }

        private void ResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DownloadButton.IsEnabled = ResultsListBox.SelectedItem != null;
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (ResultsListBox.SelectedItem is not WikipediaSearchResult selectedResult)
            {
                return;
            }

            DownloadButton.IsEnabled = false;
            DownloadButton.Content = "Downloading...";
            ArticleContentTextBlock.Text = "Downloading article...";
            TokenizedTextBlock.Text = "Waiting for article download...";

            try
            {
                // Get article content from Wikipedia API
                string url = $"https://en.wikipedia.org/w/api.php?action=query&prop=extracts&exintro=false&explaintext=true&pageids={selectedResult.PageId}&format=json";
                
                var response = await _httpClient.GetStringAsync(url);
                var jsonDoc = JsonDocument.Parse(response);

                string articleText = "";
                if (jsonDoc.RootElement.TryGetProperty("query", out var query) &&
                    query.TryGetProperty("pages", out var pages))
                {
                    foreach (var page in pages.EnumerateObject())
                    {
                        if (page.Value.TryGetProperty("extract", out var extract))
                        {
                            articleText = extract.GetString() ?? "";
                            _currentArticleTitle = selectedResult.Title;
                            break;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(articleText))
                {
                    ArticleContentTextBlock.Text = "Failed to retrieve article content.";
                    TokenizedTextBlock.Text = "No content to tokenize.";
                    return;
                }

                // Display article content
                _currentArticleContent = articleText;
                ArticleContentTextBlock.Text = $"Title: {_currentArticleTitle}\n\n{articleText}";

                // Tokenize the article
                _currentTokens = TokenizeText(articleText);
                TokenizedTextBlock.Text = $"Total tokens: {_currentTokens.Count}\n\n" +
                    $"Tokens (first 500):\n{string.Join(" | ", _currentTokens.Take(500))}";

                // Save article to file
                await SaveArticleToFile(_currentArticleTitle, articleText, _currentTokens);
            }
            catch (Exception ex)
            {
                ArticleContentTextBlock.Text = $"Error downloading article: {ex.Message}";
                TokenizedTextBlock.Text = "Tokenization failed.";
                MessageBox.Show($"Failed to download article: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DownloadButton.IsEnabled = true;
                DownloadButton.Content = "Download Selected Article";
            }
        }

        private List<string> TokenizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            // Remove HTML entities and clean text
            text = Regex.Replace(text, @"&[a-zA-Z]+;", " ");
            text = Regex.Replace(text, @"\s+", " ");
            
            // Basic tokenization: split by whitespace and punctuation
            // This is a simple tokenizer - in production, you'd use a proper NLP library
            var tokens = new List<string>();
            
            // Split by common delimiters while preserving some punctuation
            var words = Regex.Split(text, @"(\s+|[,;:!?\.\-\(\)\[\]""'`])")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Select(w => w.Trim())
                .Where(w => w.Length > 0);

            foreach (var word in words)
            {
                // Further split compound words if needed
                if (word.Length > 1 && char.IsLetterOrDigit(word[0]))
                {
                    tokens.Add(word.ToLowerInvariant());
                }
            }

            return tokens;
        }

        private async Task SaveArticleToFile(string title, string content, List<string> tokens)
        {
            try
            {
                string safeFileName = string.Join("_", title.Split(Path.GetInvalidFileNameChars()));
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{safeFileName}_{timestamp}";
                string articlePath = Path.Combine(_articlesDirectory, $"{fileName}.txt");
                string tokensPath = Path.Combine(_articlesDirectory, $"{fileName}_tokens.json");
                string metadataPath = Path.Combine(_articlesDirectory, $"{fileName}_metadata.json");

                // Save article content
                await File.WriteAllTextAsync(articlePath, content);

                // Save tokens as JSON
                var tokensJson = JsonSerializer.Serialize(tokens, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(tokensPath, tokensJson);

                // Save metadata
                var metadata = new ArticleMetadata
                {
                    Title = title,
                    ArticlePath = articlePath,
                    TokensPath = tokensPath,
                    TokenCount = tokens.Count,
                    SavedDate = DateTime.Now,
                    ContentLength = content.Length
                };
                var metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(metadataPath, metadataJson);

                // Add to collection
                var savedArticle = new SavedArticle
                {
                    Title = title,
                    FileName = fileName,
                    TokenCount = tokens.Count,
                    SavedDate = DateTime.Now,
                    ArticlePath = articlePath,
                    TokensPath = tokensPath,
                    MetadataPath = metadataPath
                };

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _savedArticles.Insert(0, savedArticle);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save article: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadSavedArticles()
        {
            try
            {
                if (!Directory.Exists(_articlesDirectory))
                    return;

                var metadataFiles = Directory.GetFiles(_articlesDirectory, "*_metadata.json");
                var articles = new List<SavedArticle>();

                foreach (var metadataFile in metadataFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(metadataFile);
                        var metadata = JsonSerializer.Deserialize<ArticleMetadata>(json);
                        
                        if (metadata != null && File.Exists(metadata.ArticlePath))
                        {
                            var fileName = Path.GetFileNameWithoutExtension(metadataFile).Replace("_metadata", "");
                            articles.Add(new SavedArticle
                            {
                                Title = metadata.Title,
                                FileName = fileName,
                                TokenCount = metadata.TokenCount,
                                SavedDate = metadata.SavedDate,
                                ArticlePath = metadata.ArticlePath,
                                TokensPath = metadata.TokensPath,
                                MetadataPath = metadataFile
                            });
                        }
                    }
                    catch
                    {
                        // Skip invalid metadata files
                    }
                }

                articles = articles.OrderByDescending(a => a.SavedDate).ToList();
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _savedArticles.Clear();
                    foreach (var article in articles)
                    {
                        _savedArticles.Add(article);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load saved articles: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SavedArticlesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SavedArticlesGrid.SelectedItem is SavedArticle selectedArticle)
            {
                ViewSavedArticleButton.IsEnabled = true;
                DeleteSavedArticleButton.IsEnabled = true;
            }
            else
            {
                ViewSavedArticleButton.IsEnabled = false;
                DeleteSavedArticleButton.IsEnabled = false;
            }
        }

        private void ViewSavedArticleButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavedArticlesGrid.SelectedItem is SavedArticle selectedArticle)
            {
                try
                {
                    if (File.Exists(selectedArticle.ArticlePath))
                    {
                        var content = File.ReadAllText(selectedArticle.ArticlePath);
                        var tokensJson = File.ReadAllText(selectedArticle.TokensPath);
                        var tokens = JsonSerializer.Deserialize<List<string>>(tokensJson);

                        MainTabControl.SelectedIndex = 3;
                        ArticleContentTextBlock.Text = $"Title: {selectedArticle.Title}\n\n{content}";
                        TokenizedTextBlock.Text = $"Total tokens: {tokens?.Count ?? 0}\n\n" +
                            $"Tokens (first 500):\n{string.Join(" | ", tokens?.Take(500) ?? new List<string>())}";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load article: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteSavedArticleButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavedArticlesGrid.SelectedItem is SavedArticle selectedArticle)
            {
                var result = MessageBox.Show($"Are you sure you want to delete '{selectedArticle.Title}'?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (File.Exists(selectedArticle.ArticlePath))
                            File.Delete(selectedArticle.ArticlePath);
                        if (File.Exists(selectedArticle.TokensPath))
                            File.Delete(selectedArticle.TokensPath);
                        if (File.Exists(selectedArticle.MetadataPath))
                            File.Delete(selectedArticle.MetadataPath);

                        _savedArticles.Remove(selectedArticle);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete article: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RefreshSavedArticlesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSavedArticles();
        }
    }

    // Helper class for Wikipedia search results
    public class WikipediaSearchResult
    {
        public string Title { get; set; } = "";
        public string Snippet { get; set; } = "";
        public int PageId { get; set; }
    }

    public class SavedArticle
    {
        public string Title { get; set; } = "";
        public string FileName { get; set; } = "";
        public int TokenCount { get; set; }
        public DateTime SavedDate { get; set; }
        public string ArticlePath { get; set; } = "";
        public string TokensPath { get; set; } = "";
        public string MetadataPath { get; set; } = "";
    }

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
