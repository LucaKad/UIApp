using System.Windows;

namespace NlpUiDemo
{
    public partial class AboutDialog : Window
    {
        public AboutDialog(Window owner, double fontSize, bool isDarkTheme)
        {
            InitializeComponent();
            Owner = owner;
            FontSize = fontSize;
            
            // Apply theme
            if (isDarkTheme)
            {
                if (Resources["WindowBackgroundBrush"] is System.Windows.Media.SolidColorBrush wBrush)
                    wBrush.Color = System.Windows.Media.Colors.DimGray;
                if (Resources["WindowTextBrush"] is System.Windows.Media.SolidColorBrush tBrush)
                    tBrush.Color = System.Windows.Media.Colors.WhiteSmoke;
                Foreground = System.Windows.Media.Brushes.WhiteSmoke;
            }
            else
            {
                if (Resources["WindowBackgroundBrush"] is System.Windows.Media.SolidColorBrush wBrush)
                    wBrush.Color = System.Windows.Media.Colors.LightGray;
                if (Resources["WindowTextBrush"] is System.Windows.Media.SolidColorBrush tBrush)
                    tBrush.Color = System.Windows.Media.Colors.Black;
                Foreground = System.Windows.Media.Brushes.Black;
            }
            
            Background = (System.Windows.Media.Brush)Resources["WindowBackgroundBrush"];
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

