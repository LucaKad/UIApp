using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NlpUiDemo.Interfaces;
using NlpUiDemo.Models;
using NlpUiDemo.Services;

namespace NlpUiDemo
{
    public partial class MainWindow : Window
    {
        private bool _isDarkTheme = false;
        private readonly IWikipediaService _wikipediaService;
        private readonly ITokenizationService _tokenizationService;
        private readonly IArticleRepository _articleRepository;
        private readonly ObservableCollection<SavedArticle> _savedArticles = new ObservableCollection<SavedArticle>();
        private TokenizedArticle? _currentTokenizedArticle;

        public MainWindow()
        {
            InitializeComponent();

            var container = ServiceContainer.Instance;
            _wikipediaService = container.WikipediaService;
            _tokenizationService = container.TokenizationService;
            _articleRepository = container.ArticleRepository;

            LoadSavedArticles();

            this.FontSize = FontSizeSlider.Value;
            
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
            
            this.Closed += (s, e) =>
            {
                if (_wikipediaService is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            };
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
                var results = await _wikipediaService.SearchArticlesAsync(searchQuery, 10);
                var resultsList = results.ToList();
                ResultsListBox.ItemsSource = resultsList;
                SearchStatusText.Text = resultsList.Count > 0 
                    ? $"Found {resultsList.Count} result(s)." 
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
                var article = await _wikipediaService.GetArticleAsync(selectedResult.PageId);
                
                if (!article.IsValid)
                {
                    ArticleContentTextBlock.Text = "Failed to retrieve article content.";
                    TokenizedTextBlock.Text = "No content to tokenize.";
                    return;
                }

                _currentTokenizedArticle = _tokenizationService.Tokenize(article);

                ArticleContentTextBlock.Text = $"Title: {_currentTokenizedArticle.Article.Title}\n\n{_currentTokenizedArticle.Article.Content}";
                TokenizedTextBlock.Text = $"Total tokens: {_currentTokenizedArticle.TokenCount}\n\n" +
                    $"Tokens (first 500):\n{string.Join(" | ", _currentTokenizedArticle.Tokens.Take(500))}";

                var savedArticle = await _articleRepository.SaveAsync(_currentTokenizedArticle);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _savedArticles.Insert(0, savedArticle);
                });
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

        private async void LoadSavedArticles()
        {
            try
            {
                var articles = await _articleRepository.GetAllAsync();
                
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

        private async void ViewSavedArticleButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavedArticlesGrid.SelectedItem is SavedArticle selectedArticle)
            {
                try
                {
                    var tokenizedArticle = await _articleRepository.LoadAsync(selectedArticle);
                    _currentTokenizedArticle = tokenizedArticle;

                    MainTabControl.SelectedIndex = 3;
                    ArticleContentTextBlock.Text = $"Title: {tokenizedArticle.Article.Title}\n\n{tokenizedArticle.Article.Content}";
                    TokenizedTextBlock.Text = $"Total tokens: {tokenizedArticle.TokenCount}\n\n" +
                        $"Tokens (first 500):\n{string.Join(" | ", tokenizedArticle.Tokens.Take(500))}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load article: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteSavedArticleButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavedArticlesGrid.SelectedItem is SavedArticle selectedArticle)
            {
                var result = MessageBox.Show($"Are you sure you want to delete '{selectedArticle.Title}'?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _articleRepository.DeleteAsync(selectedArticle);
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
}
