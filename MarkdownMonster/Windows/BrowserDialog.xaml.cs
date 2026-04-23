using System.Windows;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for BrowserDialog.xaml
    /// </summary>
    public partial class BrowserDialog : Window
    {
        public bool IsLoaded { get; set; }

        public BrowserDialog(string url = null)
        {
            InitializeComponent();

            this.Browser.LoadCompleted += Browser_LoadCompleted;

            if (!string.IsNullOrEmpty(url))
                Browser.Navigate(url);
        }

        public void Navigate(string url)
        {
            IsLoaded = false;
            Browser.Navigate(url);
        }

        private void Browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            IsLoaded = true;            
        }
    }
}
