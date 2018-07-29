using System.Windows;
using ArchiveViewer.UserControls.Home;
using ArchiveViewer.UserControls.Option;
using Caliburn.Micro;

namespace ArchiveViewer.UserControls.Shell
{
    public class ShellViewModel : Conductor<Screen>.Collection.OneActive
    {
        private readonly HomeViewModel _homeViewModel;
        private readonly OptionViewModel _optionViewModel;

        public ShellViewModel()
        {
            
        }

        public ShellViewModel(
            HomeViewModel homeViewModel,
            OptionViewModel optionViewModel)
        {
            _homeViewModel = homeViewModel;
            _optionViewModel = optionViewModel;
            ShowHome();
        }

        public void ShowHome()
        {
            ActivateItem(_homeViewModel);
        }

        public void ShowOption()
        {
            ActivateItem(_optionViewModel);
        }
    }
}