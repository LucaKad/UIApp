using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NlpUiDemo
{
    public partial class MainWindow : Window
    {
        private bool _isDarkTheme = false;

        public MainWindow()
        {
            InitializeComponent();

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
    }
}
