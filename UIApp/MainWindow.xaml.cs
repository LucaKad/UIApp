using System;
using System.Collections.Generic;
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

        public MainWindow()
        {
            InitializeComponent();

            // Initialize HttpClient for Wikipedia API calls
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "NLP-Explorer/1.0 (Educational App)");

            // Initialize base font size for window
            this.FontSize = FontSizeSlider.Value;
            
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
                ArticleContentTextBlock.Text = $"Title: {_currentArticleTitle}\n\n{articleText}";

                // Tokenize the article
                var tokens = TokenizeText(articleText);
                TokenizedTextBlock.Text = $"Total tokens: {tokens.Count}\n\n" +
                    $"Tokens (first 500):\n{string.Join(" | ", tokens.Take(500))}";
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
    }

    // Helper class for Wikipedia search results
    public class WikipediaSearchResult
    {
        public string Title { get; set; } = "";
        public string Snippet { get; set; } = "";
        public int PageId { get; set; }
    }
}
