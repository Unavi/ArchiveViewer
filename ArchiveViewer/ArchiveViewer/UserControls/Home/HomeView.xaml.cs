using System.Windows.Controls;

namespace ArchiveViewer.UserControls.Home
{
  /// <summary>
  ///     Interaction logic for HomeView.xaml
  /// </summary>
  public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }

        private void Languages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var homeViewModel = (HomeViewModel) DataContext;
            //Reset selection highlighting of languages
            if (e.AddedItems.Count > 0 && (string) e.AddedItems[0] != homeViewModel.SelectedLanguage)
            {
                Languages.SelectedIndex = homeViewModel.Languages.IndexOf(homeViewModel.SelectedLanguage);
            }
        }
    }
}